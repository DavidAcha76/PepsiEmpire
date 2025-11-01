using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkObject))]
public class PlayerNetworkController : NetworkBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 4f;
    public float runMultiplier = 1.7f;

    [Header("Salto & Gravedad")]
    public float jumpVelocity = 5f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.15f;
    public float maxFallSpeed = -20f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.15f;
    public LayerMask groundMask = ~0;

    [Header("Rotación (Look)")]
    public float sensitivityYaw = 2f;
    public float sensitivityPitch = 2f;
    public float mouseSmoothTime = 0.03f;
    public float maxPitch = 80f;
    public bool invertY = false;

    [Header("Agachado (cámara)")]
    public float standCamY = 0.6f;
    public float crouchCamY = 0.3f;
    public float crouchLerp = 10f;

    [Header("Interacción")]
    public float interactDistance = 2f;
    public LayerMask interactMask = ~0;

    [Header("Presentación local")]
    public Camera playerCamera;
    public GameObject localUIRoot;

    [Header("Debug")]
    public bool showDebugLogs = false;

    // ====== Espectador ======
    [Networked] public NetworkBool IsSpectator { get; set; }
    private Vector3[] _specCorners = new Vector3[4];
    private const KeyCode SpecKey1 = KeyCode.Alpha1;
    private const KeyCode SpecKey2 = KeyCode.Alpha2;
    private const KeyCode SpecKey3 = KeyCode.Alpha3;
    private const KeyCode SpecKey4 = KeyCode.Alpha4;

    // Internos
    NetworkRigidbody3D _nrb;
    Rigidbody _rb;
    Transform _tf;
    CapsuleCollider _col;

    float _lastGroundTime = -999f;
    float _lastJumpPressTime = -999f;
    bool _isLocalPlayer = false;

    float _yaw;
    float _pitch;
    Vector2 _lookSmoothed;
    Vector2 _lookVel;

    public override void Spawned()
    {
        _tf = transform;
        _nrb = GetComponentInChildren<NetworkRigidbody3D>();
        _rb = _nrb ? _nrb.Rigidbody : GetComponent<Rigidbody>();
        _col = GetComponent<CapsuleCollider>();

        if (showDebugLogs)
        {
            Debug.Log($"[Spawned] name={name} SA={Object.HasStateAuthority} IA={Object.HasInputAuthority} " +
                      $"InputAuth={Object.InputAuthority} LocalPlayer={Runner.LocalPlayer}");
        }

        if (Runner != null && Runner.IsRunning && !Runner.ProvideInput)
            Runner.ProvideInput = true;

        // Server-authoritative sin predicción
        if (Object.HasStateAuthority)
        {
            _rb.isKinematic = false;
        }
        else if (Object.HasInputAuthority)
        {
            Runner.SetIsSimulated(Object, false);
            _rb.isKinematic = true;
        }
        else
        {
            _rb.isKinematic = true;
        }

        _rb.useGravity = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.None;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        _rb.angularDamping = 999f;
        _rb.maxAngularVelocity = 0f;

        _yaw = _tf.eulerAngles.y;
        if (!playerCamera) playerCamera = GetComponentInChildren<Camera>(true);
        if (playerCamera)
        {
            var e = playerCamera.transform.localEulerAngles;
            _pitch = e.x > 180f ? e.x - 360f : e.x;
        }

        SetPresentation(Object.HasInputAuthority);

        // IMPORTANTÍSIMO: si este avatar es del jugador local, registrar su Transform como yawSource para el input
        if (Object.HasInputAuthority)
        {
            var cb = FindObjectOfType<GameInputCallbacks>();
            if (cb != null) cb.SetYawSource(this.transform);
        }

        var sim3D = Runner ? Runner.GetComponent<RunnerSimulatePhysics3D>() : null;
        if (showDebugLogs && sim3D == null)
            Debug.LogWarning("[PlayerNetworkController] RunnerSimulatePhysics3D será auto-creado por Fusion (defaults).");
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        SetPresentation(false);
    }

    void SetPresentation(bool isLocal)
    {
        _isLocalPlayer = isLocal;
        if (!playerCamera) playerCamera = GetComponentInChildren<Camera>(true);
        if (playerCamera)
        {
            playerCamera.enabled = isLocal;
            playerCamera.gameObject.SetActive(isLocal);
            playerCamera.tag = isLocal ? "MainCamera" : "Untagged";
            var listener = playerCamera.GetComponent<AudioListener>();
            if (listener) listener.enabled = isLocal;
        }
        if (localUIRoot) localUIRoot.SetActive(isLocal);
    }

    // Solo UX local (cámara y sensación de giro inmediata)
    // Nota: aquí SÍ rotamos el root local para que el jugador vea el giro al instante.
    // El server aplicará esa misma rotación con el yaw absoluto recibido por input.
    public override void Render()
    {
        if (!_isLocalPlayer || !playerCamera) return;

        float rawX = Input.GetAxisRaw("Mouse X") * sensitivityYaw;
        float rawY = Input.GetAxisRaw("Mouse Y") * sensitivityPitch;
        if (invertY) rawY = -rawY;

        Vector2 raw = new Vector2(rawX, rawY);
        _lookSmoothed = (mouseSmoothTime > 0f)
          ? Vector2.SmoothDamp(_lookSmoothed, raw, ref _lookVel, mouseSmoothTime)
          : raw;

        _yaw += _lookSmoothed.x;
        _pitch = Mathf.Clamp(_pitch - _lookSmoothed.y, -maxPitch, maxPitch);

        // Giro visual inmediato local (también alimenta yawSource.eulerAngles.y)
        _tf.rotation = Quaternion.Euler(0f, _yaw, 0f);

        var cam = playerCamera.transform;
        cam.localEulerAngles = new Vector3(_pitch, 0f, 0f);

        bool crouchHeld = Input.GetKey(KeyCode.LeftControl);
        float targetCamY = crouchHeld ? crouchCamY : standCamY;
        Vector3 camPos = cam.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * crouchLerp);
        cam.localPosition = camPos;

        // Espectador: teleports
        if (IsSpectator)
        {
            if (Input.GetKeyDown(SpecKey1)) RPC_RequestTeleport(0);
            if (Input.GetKeyDown(SpecKey2)) RPC_RequestTeleport(1);
            if (Input.GetKeyDown(SpecKey3)) RPC_RequestTeleport(2);
            if (Input.GetKeyDown(SpecKey4)) RPC_RequestTeleport(3);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Solo simula el host
        if (!Object.HasStateAuthority) return;

        if (!GetInput<NetworkInputData>(out var input))
        {
            if (showDebugLogs)
                Debug.LogWarning("[PlayerNetworkController] Host: no input recibido este tick.");
            return;
        }

        // ===== ORIENTACIÓN EN SERVER: usar CameraYaw si viene válido =====
        bool hasValidYaw = !float.IsNaN(input.CameraYaw) && !float.IsInfinity(input.CameraYaw);
        if (hasValidYaw)
        {
            _yaw = input.CameraYaw;
        }
        else
        {
            // Fallback muy raro: integra con LookDelta
            _yaw += input.LookDelta.x * sensitivityYaw;
        }

        // Aplicar siempre la rotación del cuerpo en el server (para proxies y consistencia)
        _tf.rotation = Quaternion.Euler(0f, _yaw, 0f);

        // ===== ESPECTADOR =====
        if (IsSpectator)
        {
            Vector3 velS = _rb.linearVelocity;
            velS.x = 0f; velS.z = 0f;
            velS.y += Physics.gravity.y * Runner.DeltaTime;
            velS.y = Mathf.Max(velS.y, maxFallSpeed);
            _rb.linearVelocity = velS;
            _rb.angularVelocity = Vector3.zero;
            return;
        }

        // ===== Movimiento normal =====
        _rb.angularVelocity = Vector3.zero;

        // Ground check (coyote)
        Vector3 center = _tf.position + (_col ? _col.center : Vector3.zero);
        float radius = _col ? Mathf.Max(0.05f, _col.radius * 0.95f) : 0.4f;
        Vector3 groundOrigin = center + Vector3.down * ((_col ? _col.height * 0.5f : 0.9f) - radius + groundCheckDistance);
        bool grounded = Physics.CheckSphere(groundOrigin, radius, groundMask, QueryTriggerInteraction.Ignore);
        if (grounded) _lastGroundTime = (float)Runner.SimulationTime;

        Vector3 vel = _rb.linearVelocity;

        if (input.IsJumpPressed) _lastJumpPressTime = (float)Runner.SimulationTime;
        bool canCoyote = ((float)Runner.SimulationTime - _lastGroundTime) <= coyoteTime;
        bool hasBuffer = ((float)Runner.SimulationTime - _lastJumpPressTime) <= jumpBufferTime;
        if (hasBuffer && canCoyote && vel.y <= 0.1f)
        {
            vel.y = jumpVelocity;
            _lastJumpPressTime = -999f;
            _lastGroundTime = -999f;
        }

        vel.y += Physics.gravity.y * Runner.DeltaTime;
        vel.y = Mathf.Max(vel.y, maxFallSpeed);

        // Direcciones según yaw del server (ya igual al del cliente local)
        Vector3 moveDir = Vector3.zero;
        if (input.Move.sqrMagnitude > 1e-4f)
        {
            Quaternion yawRot = Quaternion.Euler(0f, _yaw, 0f);
            Vector3 fwd = yawRot * Vector3.forward;
            Vector3 right = yawRot * Vector3.right;
            fwd.y = 0f; right.y = 0f;
            fwd.Normalize(); right.Normalize();

            Vector2 m = input.Move.sqrMagnitude > 1f ? input.Move.normalized : input.Move;
            moveDir = (fwd * m.y + right * m.x).normalized;
        }

        float speed = walkSpeed * (input.IsRunPressed ? runMultiplier : 1f);
        vel.x = moveDir.x * speed;
        vel.z = moveDir.z * speed;

        _rb.linearVelocity = vel;
    }

    // ====== RPCs ESPECTADOR ======
    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_EnterSpectator(Vector3 c0, Vector3 c1, Vector3 c2, Vector3 c3, RpcInfo info = default)
    {
        _specCorners[0] = c0;
        _specCorners[1] = c1;
        _specCorners[2] = c2;
        _specCorners[3] = c3;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ExitSpectator(RpcInfo info = default)
    {
        // limpiar UI local si fuera necesario
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestTeleport(byte cornerIndex, RpcInfo info = default)
    {
        if (!Object.HasStateAuthority) return;
        if (!IsSpectator) return;
        if (cornerIndex > 3) return;

        Vector3 dst = _specCorners[cornerIndex];
        _rb.position = dst;
        var vel = _rb.linearVelocity;
        vel.x = vel.z = 0f;
        _rb.linearVelocity = vel;
    }
}
