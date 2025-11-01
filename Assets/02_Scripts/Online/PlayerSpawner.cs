using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

/// <summary>
/// Spawnea jugadores al conectarse y (opcional) al cambiar de escena, sin duplicados.
/// - Mapea cada PlayerRef a su NetworkObject (SetPlayerObject) justo después del Spawn.
/// - Evita spawns durante cargas de escena (_isLoadingScene).
/// - Respawnea tras OnSceneLoadDone si respawnOnSceneLoad = true.
/// </summary>
public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Prefab del jugador (con NetworkObject)")]
    [Tooltip("Debe tener NetworkObject en raíz.")]
    public NetworkObject playerPrefab;

    [Header("Puntos de spawn opcionales")]
    [Tooltip("Si se asignan, se usa round-robin por PlayerRef.")]
    public Transform[] spawnPoints;

    [Header("Registro automático")]
    [Tooltip("Busca un NetworkRunner y se registra a sus callbacks en Awake.")]
    public bool autoRegister = true;

    [Header("Respawn en Cambio de Escena")]
    [Tooltip("Si true: tras cargar escena, respawnea a todos los jugadores activos.")]
    public bool respawnOnSceneLoad = true;

    private NetworkRunner _runner;
    private readonly Dictionary<PlayerRef, NetworkObject> _spawned = new();

    // Bandera para evitar spawns durante transición de escena
    private bool _isLoadingScene = false;

    private void Awake()
    {
        if (autoRegister)
        {
            _runner = FindObjectOfType<NetworkRunner>(includeInactive: true);
            if (_runner != null) _runner.AddCallbacks(this);
        }
    }

    /// <summary>Permite registrar el runner desde un launcher externo.</summary>
    public void Register(NetworkRunner runner)
    {
        if (_runner == runner) return;
        if (_runner != null) _runner.RemoveCallbacks(this);

        _runner = runner;
        if (_runner != null) _runner.AddCallbacks(this);
    }

    // ===================== JOIN / LEAVE =====================

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;

        // Si estamos cargando escena, difiere el spawn a OnSceneLoadDone.
        if (_isLoadingScene)
        {
            Debug.Log($"[PlayerSpawner] Cambio de escena en progreso. OnSceneLoadDone spawneará a {player}.");
            return;
        }

        // Evita duplicados si por algún motivo ya lo teníamos registrado
        if (_spawned.ContainsKey(player) && _spawned[player] != null)
        {
            Debug.Log($"[PlayerSpawner] {player} ya tiene avatar. Ignorando.");
            return;
        }

        SpawnPlayer(runner, player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // Despawns solo desde el servidor
        if (runner.IsServer && _spawned.TryGetValue(player, out var obj) && obj != null)
        {
            runner.Despawn(obj);
            Debug.Log($"[PlayerSpawner] Despawn de {player}");
        }
        _spawned.Remove(player);
    }

    // ===================== SCENE LOAD =====================

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        if (!runner.IsServer) return;

        _isLoadingScene = true;
        Debug.Log("[PlayerSpawner] Iniciando carga de escena...");
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        // Siempre limpia la bandera; decide si respawnea
        var shouldRespawn = runner.IsServer && respawnOnSceneLoad;

        if (!shouldRespawn)
        {
            _isLoadingScene = false;
            return;
        }

        Debug.Log("[PlayerSpawner] Escena cargada. Respawneando jugadores...");

        // Limpia referencias locales (los NO previos pueden ser destruidos por el flujo de escenas)
        _spawned.Clear();

        // Respawnea a todos los jugadores activos
        foreach (var player in runner.ActivePlayers)
            SpawnPlayer(runner, player);

        _isLoadingScene = false;
    }

    // ===================== CORE SPAWN =====================

    /// <summary>
    /// Spawnea un jugador con InputAuthority = player, lo registra en _spawned
    /// y hace el mapeo oficial Runner.SetPlayerObject para que otras clases (puerta, etc.) lo resuelvan.
    /// </summary>
    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[PlayerSpawner] Falta playerPrefab.");
            return;
        }

        // 1) Determinar posición/rotación de spawn
        GetSpawnTransformFor(player, out var pos, out var rot);

        // 2) Spawn con autoridad de input del jugador
        var obj = runner.Spawn(playerPrefab, pos, rot, inputAuthority: player);
        if (obj == null)
        {
            Debug.LogError($"[PlayerSpawner] runner.Spawn devolvió null para {player}.");
            return;
        }

        // 3) Registrar diccionario local
        _spawned[player] = obj;

        // 4) Mapeo oficial PlayerRef -> PlayerObject (CRÍTICO para TryGetPlayerObject en otros scripts)
        if (!runner.TryGetPlayerObject(player, out var existing) || existing == null)
        {
            runner.SetPlayerObject(player, obj);
            Debug.Log($"[PlayerSpawner] SetPlayerObject hecho para {player} -> {obj.name}");
        }
        else if (existing != obj)
        {
            // Si ya había uno distinto, preferimos el nuevo y dejamos log.
            runner.SetPlayerObject(player, obj);
            Debug.LogWarning($"[PlayerSpawner] PlayerRef {player} ya tenía PlayerObject distinto ({existing.name}). Reasignado a {obj.name}.");
        }

        Debug.Log($"[PlayerSpawner] Spawn de {player} en {pos}");
    }

    /// <summary>
    /// Calcula pos/rot para el player. Usa puntos de spawn si existen, si no separa por RawEncoded.
    /// </summary>
    private void GetSpawnTransformFor(PlayerRef player, out Vector3 pos, out Quaternion rot)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int idx = (player.RawEncoded % spawnPoints.Length + spawnPoints.Length) % spawnPoints.Length;
            var t = spawnPoints[idx];
            pos = t ? t.position : Vector3.zero;
            rot = t ? t.rotation : Quaternion.identity;
        }
        else
        {
            pos = new Vector3(player.RawEncoded * 2f, 1f, 0f);
            rot = Quaternion.identity;
        }
    }

    // ===================== INetworkRunnerCallbacks vacíos/útiles =====================

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
