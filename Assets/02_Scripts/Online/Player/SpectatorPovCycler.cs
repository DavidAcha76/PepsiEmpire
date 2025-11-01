using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Al terminar, este cliente pierde control y puede ciclar por las cámaras
/// de compañeros que aún NO han terminado. Q = anterior, E = siguiente.
/// Nunca activamos AudioListener en la cámara de espectador.
/// </summary>
[DisallowMultipleComponent]
public class SpectatorPovCycler : NetworkBehaviour
{
    [Header("Referencias locales a desactivar al terminar")]
    public MonoBehaviour movementScript;   // ej: PlayerNetworkController
    public MonoBehaviour inputScript;      // tu manejador de entrada
    public Camera localPlayerCamera;  // tu cámara local
    public AudioListener localAudio;       // opcional

    [Header("Cámara del espectador (se crea si es null)")]
    public Camera spectatorCameraPrefab;
    private Camera _spectatorCamera;

    [Header("Teclas para ciclar")]
    public KeyCode prevKey = KeyCode.Q;
    public KeyCode nextKey = KeyCode.E;

    // Estado
    private bool _spectating;
    private List<PlayerRef> _candidates = new();
    private int _index = -1;
    private Camera _currentTargetCam;

    private NetworkRunner _runner;
    private PlayerStatus _status;

    private void Awake()
    {
        _runner = FindAnyObjectByType<NetworkRunner>();
        _status = GetComponent<PlayerStatus>();

        if (localPlayerCamera == null)
            localPlayerCamera = GetComponentInChildren<Camera>(true);
        if (localAudio == null && localPlayerCamera != null)
            localAudio = localPlayerCamera.GetComponent<AudioListener>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object || !Object.HasInputAuthority) return;

        if (!_spectating && _status != null && _status.IsFinished)
            EnterSpectator();

        if (_spectating)
        {
            if (Input.GetKeyDown(prevKey)) Cycle(-1);
            if (Input.GetKeyDown(nextKey)) Cycle(+1);

            if (!IsCurrentTargetValid())
                Cycle(+1);
        }
    }

    private void LateUpdate()
    {
        if (!_spectating || _spectatorCamera == null || _currentTargetCam == null) return;

        _spectatorCamera.transform.SetPositionAndRotation(
            _currentTargetCam.transform.position,
            _currentTargetCam.transform.rotation
        );
        _spectatorCamera.fieldOfView = _currentTargetCam.fieldOfView;
        _spectatorCamera.nearClipPlane = _currentTargetCam.nearClipPlane;
        _spectatorCamera.farClipPlane = _currentTargetCam.farClipPlane;
    }

    // ---------- Espectador ----------

    private void EnterSpectator()
    {
        _spectating = true;

        if (movementScript) movementScript.enabled = false;
        if (inputScript) inputScript.enabled = false;

        if (localPlayerCamera) localPlayerCamera.enabled = false;
        if (localAudio) localAudio.enabled = false;

        EnsureSpectatorCamera();
        _spectatorCamera.enabled = true;

        BuildCandidates();
        ChooseFirstValidTarget();
    }

    private void EnsureSpectatorCamera()
    {
        if (_spectatorCamera != null) return;

        if (spectatorCameraPrefab)
        {
            _spectatorCamera = Instantiate(spectatorCameraPrefab, transform);
        }
        else
        {
            var go = new GameObject("SpectatorCamera");
            go.transform.SetParent(transform, false);
            _spectatorCamera = go.AddComponent<Camera>();
            _spectatorCamera.clearFlags = CameraClearFlags.Skybox;
            _spectatorCamera.depth = 10f;
        }

        var al = _spectatorCamera.GetComponent<AudioListener>();
        if (al) al.enabled = false;
    }

    private void BuildCandidates()
    {
        _candidates.Clear();
        if (_runner == null) return;

        var me = Object.InputAuthority;

        foreach (var p in _runner.ActivePlayers)
        {
            if (p == me) continue;
            if (!_runner.TryGetPlayerObject(p, out var no) || no == null) continue;

            var st = no.GetComponent<PlayerStatus>();
            if (st != null && !st.IsFinished)
                _candidates.Add(p);
        }

        _candidates = _candidates.OrderBy(p => p.RawEncoded).ToList();
    }

    private void ChooseFirstValidTarget()
    {
        _index = -1;
        _currentTargetCam = null;

        if (_candidates.Count == 0) return;

        _index = 0;
        ResolveTargetCamera(_candidates[_index]);
        if (_currentTargetCam == null)
            Cycle(+1);
    }

    private void Cycle(int dir)
    {
        if (_candidates.Count == 0)
        {
            BuildCandidates();
            if (_candidates.Count == 0) return;
        }

        for (int i = 0; i < _candidates.Count; i++)
        {
            _index = (_index + (dir >= 0 ? 1 : -1) + _candidates.Count) % _candidates.Count;
            ResolveTargetCamera(_candidates[_index]);
            if (_currentTargetCam != null) return;
        }

        BuildCandidates();
        if (_candidates.Count == 0)
        {
            _currentTargetCam = null;
            return;
        }
        _index = 0;
        ResolveTargetCamera(_candidates[_index]);
    }

    private bool IsCurrentTargetValid()
    {
        if (_runner == null || _index < 0 || _index >= _candidates.Count) return false;

        var p = _candidates[_index];
        if (!_runner.TryGetPlayerObject(p, out var no) || no == null) return false;

        var st = no.GetComponent<PlayerStatus>();
        if (st == null || st.IsFinished) return false;

        return _currentTargetCam != null;
    }

    private void ResolveTargetCamera(PlayerRef targetRef)
    {
        _currentTargetCam = null;

        if (_runner == null || targetRef == PlayerRef.None) return;
        if (!_runner.TryGetPlayerObject(targetRef, out var no) || no == null) return;

        var anchor = no.GetComponentInChildren<PlayerCameraAnchor>(true);
        if (anchor && anchor.PlayerCamera)
            _currentTargetCam = anchor.PlayerCamera;
    }

    // ---------- RPC para que el Host fuerce entrar a espectador en el dueño ----------
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_EnterSpectatorOnOwner()
    {
        if (!_spectating)
            EnterSpectator();
    }
}
