using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkObject), typeof(Collider))]
public class NetworkSceneButton : NetworkBehaviour
{
    [Header("Escena destino (en Build Settings)")]
    [SerializeField] private SceneRef targetScene; 

    [Header("Entrada")]
    [SerializeField] private KeyCode useKey = KeyCode.E;

    [Header("Opcional")]
    [SerializeField] private bool onlyHostCanTrigger = false;   
    [SerializeField] private string playerRootTag = "Player";   

    private Collider _col;

    private bool _localPlayerInside;     
    private bool _requestPending;        

    private void Reset()
    {
        _col = GetComponent<Collider>();
        if (_col) _col.isTrigger = true;
    }

    private void Awake()
    {
        _col = GetComponent<Collider>();
        if (_col && !_col.isTrigger) _col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var nob = other.GetComponentInParent<NetworkObject>();
        if (nob == null) return;

        if (!string.IsNullOrEmpty(playerRootTag) && !nob.transform.root.CompareTag(playerRootTag))
            return;

        if (nob.HasInputAuthority)
            _localPlayerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        var nob = other.GetComponentInParent<NetworkObject>();
        if (nob == null) return;

        if (!string.IsNullOrEmpty(playerRootTag) && !nob.transform.root.CompareTag(playerRootTag))
            return;

        if (nob.HasInputAuthority)
        {
            _localPlayerInside = false;
            _requestPending = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_localPlayerInside) return;

        if (onlyHostCanTrigger && !Runner.IsServer) return;

        if (Input.GetKeyDown(useKey))
        {
            _requestPending = true; 
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!_requestPending) return;

        if (onlyHostCanTrigger && Runner.IsServer)
        {
            if (Object.HasStateAuthority)
            {
                TryLoadForAll();
                _requestPending = false;
            }
            return;
        }

        if (Runner.IsForward) 
        {
            _requestPending = false;
            RPC_RequestSceneLoad(targetScene);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestSceneLoad(SceneRef scene, RpcInfo info = default)
    {
        if (!Object.HasStateAuthority) return;
        if (!scene.IsValid)
        {
            Debug.LogError("[NetworkSceneButton] SceneRef inválido. Asigna 'Sqare cell mazes' en el inspector.");
            return;
        }
        TryLoadForAll();
    }

    private void TryLoadForAll()
    {
        if (!targetScene.IsValid)
        {
            Debug.LogError("[NetworkSceneButton] SceneRef inválido (targetScene).");
            return;
        }
        Runner.LoadScene(targetScene, LoadSceneMode.Single);
    }
}
