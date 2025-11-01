using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public sealed class NetworkGameLauncher : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Escena")]
    public int gameplaySceneBuildIndex = 1;

    [Header("Sesión")]
    [Min(1)] public int maxPlayers = 4;
    public string defaultSessionName = "Room-01";

    [Header("Render/Frame")]
    public int targetFrameRate = 120;

    [Header("Input")]
    [Tooltip("TRUE si usas GameInputCallbacks en el Player prefab")]
    public bool disableOnInputHereIfExternalProvider = true;
    public InputController optionalLocalInputController;

    [Header("Refs")]
    public PlayerSpawner playerSpawner;
    public MonoBehaviour mapGeneratorOverride;

    private NetworkRunner _runner;
    private NetworkSceneManagerDefault _sceneMgr;

    private MonoBehaviour _mapGenerator;
    private MethodInfo _miHasValidStartGet;
    private MethodInfo _miGetSafeSpawn;
    private MethodInfo _miHostBroadcastSeedGen;

    void Awake()
    {
        DontDestroyOnLoad(this);
        if (targetFrameRate > 0) Application.targetFrameRate = targetFrameRate;
        Time.fixedDeltaTime = 1f / 60f;
        QualitySettings.vSyncCount = 0;
    }

    void OnDestroy()
    {
        if (_runner != null) _runner.RemoveCallbacks(this);
    }

    public Task StartHost(string sessionName) => StartRunner(GameMode.Host, sessionName);
    public Task StartClientAndJoin(string sessionName) => StartRunner(GameMode.Client, sessionName);

    public async Task QuickJoinOrCreate(string sessionNameIfCreate = "Room-01")
    {
        if (_runner != null) return;

        CreateRunner();
        _runner.ProvideInput = true;
        RegisterSpawner();

        var quick = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = null,
            SceneManager = _sceneMgr
        });

        if (!quick.Ok)
        {
            var create = await _runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Host,
                SessionName = string.IsNullOrWhiteSpace(sessionNameIfCreate) ? defaultSessionName : sessionNameIfCreate,
                SceneManager = _sceneMgr,
                SessionProperties = NewSessionProps()
            });
            if (!create.Ok) { Debug.LogError($"StartGame Host: {create.ShutdownReason}"); return; }
        }

        TryLoadGameplayScene();
    }

    public async Task ShutdownAndGoTo(int menuBuildIndex = 0)
    {
        try
        {
            if (_runner != null)
            {
                _runner.RemoveCallbacks(this);
                await _runner.Shutdown();
            }
        }
        catch (Exception e) { Debug.LogWarning($"Shutdown: {e.Message}"); }
        finally
        {
            _runner = null;
            if (menuBuildIndex >= 0) SceneManager.LoadScene(menuBuildIndex);
        }
    }

    private async Task StartRunner(GameMode mode, string sessionName)
    {
        if (_runner != null) return;

        CreateRunner();
        _runner.ProvideInput = true;
        RegisterSpawner();

        var args = new StartGameArgs
        {
            GameMode = mode,
            SessionName = string.IsNullOrWhiteSpace(sessionName) ? defaultSessionName : sessionName,
            SceneManager = _sceneMgr,
            SessionProperties = (mode == GameMode.Host) ? NewSessionProps() : null
        };

        var result = await _runner.StartGame(args);
        if (!result.Ok) { Debug.LogError($"StartGame {mode}: {result.ShutdownReason}"); return; }

        TryLoadGameplayScene();
    }

    private void CreateRunner()
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _sceneMgr = gameObject.AddComponent<NetworkSceneManagerDefault>();
        _runner.AddCallbacks(this);
    }

    private Dictionary<string, SessionProperty> NewSessionProps() => new Dictionary<string, SessionProperty>(2) {
        { "MaxPlayers", (SessionProperty)maxPlayers },
        { "Build", (SessionProperty)Application.version }
    };

    private void TryLoadGameplayScene()
    {
        if (_runner == null || !_runner.IsSceneAuthority) return;
        if (gameplaySceneBuildIndex < 0) { Debug.LogWarning("BuildIndex inválido."); return; }
        _runner.LoadScene(SceneRef.FromIndex(gameplaySceneBuildIndex), LoadSceneMode.Single);
    }

    private void RegisterSpawner()
    {
        if (playerSpawner != null) { playerSpawner.Register(_runner); return; }
        var found = FindFirstComponent<PlayerSpawner>();
        if (found != null) { playerSpawner = found; playerSpawner.Register(_runner); }
    }

    private static T FindFirstComponent<T>() where T : Component
    {
        var arr = FindObjectsOfType<T>(true);
        return arr != null && arr.Length > 0 ? arr[0] : null;
    }

    // -------- INetworkRunnerCallbacks COMPLETOS --------
    public void OnConnectedToServer(NetworkRunner runner) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    { Debug.LogWarning($"Disconnected: {reason}"); }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        int count = 0; foreach (var _ in runner.ActivePlayers) count++;
        if (count >= maxPlayers) request.Refuse(); else request.Accept();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    { Debug.LogError($"ConnectFailed: {reason}"); }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    { Debug.LogWarning($"Shutdown: {shutdownReason}"); }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer) return;
        TryBindMapGenerator();
        TryHostBroadcastSeedAndGen();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    // CORREGIDO: Firmas correctas para Reliable Data
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    // -------- Input local --------
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (disableOnInputHereIfExternalProvider) return;

        if (optionalLocalInputController == null)
        {
            optionalLocalInputController = FindFirstComponent<InputController>();
        }

        var data = new NetworkInputData();

        if (optionalLocalInputController != null &&
            optionalLocalInputController.isActiveAndEnabled &&
            optionalLocalInputController.IsInitialized)
        {
            float h = optionalLocalInputController.GetAxis(InputController.IdMoveH);
            float v = optionalLocalInputController.GetAxis(InputController.IdMoveV);
            data.Move = new Vector2(h, v);
            if (data.Move.sqrMagnitude > 1f) data.Move = data.Move.normalized;

            if (optionalLocalInputController.GetButton(InputController.IdJump))
                data.Buttons.Set(NetworkInputData.ButtonsMask.Jump, true);

            if (optionalLocalInputController.GetButton(InputController.IdRun))
                data.Buttons.Set(NetworkInputData.ButtonsMask.Run, true);

            if (optionalLocalInputController.GetButton(InputController.IdCrouch))
                data.Buttons.Set(NetworkInputData.ButtonsMask.Crouch, true);

            if (optionalLocalInputController.GetButtonDown(InputController.IdInteract))
                data.Buttons.Set(NetworkInputData.ButtonsMask.Interact, true);
        }
        else
        {
            data.Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (Input.GetKey(KeyCode.Space))
                data.Buttons.Set(NetworkInputData.ButtonsMask.Jump, true);

            if (Input.GetKey(KeyCode.LeftShift))
                data.Buttons.Set(NetworkInputData.ButtonsMask.Run, true);

            if (Input.GetKey(KeyCode.C))
                data.Buttons.Set(NetworkInputData.ButtonsMask.Crouch, true);

            if (Input.GetKeyDown(KeyCode.E))
                data.Buttons.Set(NetworkInputData.ButtonsMask.Interact, true);
        }

        data.CameraYaw = GetLocalPlayerYaw();

        input.Set(data);
    }

    // -------- Generador opcional --------
    private void TryBindMapGenerator()
    {
        if (mapGeneratorOverride != null) { _mapGenerator = mapGeneratorOverride; CacheGeneratorAPIs(_mapGenerator.GetType()); return; }
        if (_mapGenerator != null) return;

        var t = Type.GetType("RogueLikeMiniMazesFusion") ?? Type.GetType("MazesFusion");
        if (t == null)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType("RogueLikeMiniMazesFusion", false) ?? asm.GetType("MazesFusion", false);
                if (t != null) break;
            }
        }
        if (t == null) return;

        var any = FindObjectsOfType(t, true);
        if (any != null && any.Length > 0)
        {
            _mapGenerator = any[0] as MonoBehaviour;
            CacheGeneratorAPIs(t);
        }
    }

    private void CacheGeneratorAPIs(Type t)
    {
        var prop = t.GetProperty("HasValidStart", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        _miHasValidStartGet = prop?.GetGetMethod(true) ?? t.GetMethod("HasValidStart", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        _miGetSafeSpawn = t.GetMethod("GetSafePlayerSpawnWorld", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        _miHostBroadcastSeedGen = t.GetMethod("HostBroadcastSeedAndGenerate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    private bool TryHasValidStart()
    {
        if (_mapGenerator == null || _miHasValidStartGet == null) return false;
        try { var v = _miHasValidStartGet.Invoke(_mapGenerator, null); return v is bool b && b; }
        catch { return false; }
    }

    private void TryHostBroadcastSeedAndGen()
    {
        if (_mapGenerator == null || _miHostBroadcastSeedGen == null) return;
        if (!TryHasValidStart()) return;
        try { _miHostBroadcastSeedGen.Invoke(_mapGenerator, null); }
        catch (Exception e) { Debug.LogWarning($"SeedGen: {e.Message}"); }
    }

    private float GetLocalPlayerYaw()
    {
        var players = FindObjectsOfType<PlayerNetworkController>();
        foreach (var p in players)
        {
            var netObj = p.GetComponent<NetworkObject>();
            if (netObj != null && netObj.HasInputAuthority)
            {
                return p.transform.eulerAngles.y;
            }
        }

        var cam = Camera.main;
        return cam ? cam.transform.eulerAngles.y : 0f;
    }
}
