using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

[DisallowMultipleComponent]
public class GameInputCallbacks : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Fuentes de input")]
    public InputController input;

    [Tooltip("Yaw absoluto a mandar en el input. Debe apuntar al transform del player local (root o yaw-pivot).")]
    public Transform yawSource;

    private NetworkInputData _lastInput;

    void Awake()
    {
        if (input == null)
            input = FindObjectOfType<InputController>();
        // yawSource se setea dinámicamente cuando spawnea el avatar local
        // (ver PlayerNetworkController.Spawned -> SetYawSource)
    }

    /// <summary>
    /// Lo llama el PlayerNetworkController local al spawnear para registrar su transform como yawSource.
    /// </summary>
    public void SetYawSource(Transform t)
    {
        yawSource = t;
        // Debug opcional:
        // Debug.Log($"[GameInputCallbacks] yawSource asignado: {t?.name}");
    }

    public void OnInput(NetworkRunner runner, NetworkInput inputContainer)
    {
        if (input == null || !input.IsInitialized)
        {
            Debug.LogWarning("[GameInputCallbacks] Input no listo");
            return;
        }

        var data = new NetworkInputData();

        // Movimiento
        float h = input.GetAxis(InputController.IdMoveH);
        float v = input.GetAxis(InputController.IdMoveV);
        data.Move = new Vector2(h, v);
        if (data.Move.sqrMagnitude > 1f) data.Move = data.Move.normalized;

        // Deltas de cámara (útiles como fallback si no hubiese yaw absoluto)
        float lookH = input.GetAxis(InputController.IdLookH);
        float lookV = input.GetAxis(InputController.IdLookV);
        data.LookDelta = new Vector2(lookH, lookV);

        // Botones
        if (input.GetButton(InputController.IdJump))
            data.Buttons.Set(NetworkInputData.ButtonsMask.Jump, true);
        if (input.GetButton(InputController.IdRun))
            data.Buttons.Set(NetworkInputData.ButtonsMask.Run, true);
        if (input.GetButton(InputController.IdCrouch))
            data.Buttons.Set(NetworkInputData.ButtonsMask.Crouch, true);
        if (input.GetButtonDown(InputController.IdInteract))
            data.Buttons.Set(NetworkInputData.ButtonsMask.Interact, true);

        // Yaw absoluto desde el pivot real del player local (root que estás rotando en Render)
        if (yawSource != null)
            data.CameraYaw = yawSource.eulerAngles.y;

        inputContainer.Set(data);
        _lastInput = data;
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        input.Set(_lastInput);
    }

    // Callbacks vacíos que no usas
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
