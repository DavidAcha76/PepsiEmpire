using Fusion;
using UnityEngine;

/// Puerta de salida con fallback robusto sin NetworkObjectRef (usa NetworkId):
/// - Collider isTrigger + Rigidbody kinematic
/// - Filtra por tag "Player"
/// - Envía RPC con PlayerRef + NetworkId del avatar
/// - En Host: intenta Runner.TryGetPlayerObject; si falla, resuelve con Runner.TryFindObject(networkId)
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class NetworkExitDoor : NetworkBehaviour
{
    [Header("Trigger")]
    [Tooltip("Tag del jugador que debe colisionar (usa 'Player').")]
    public string requiredTag = "Player";

    private Collider _col;
    private Rigidbody _rb;

    private void Reset()
    {
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;

        var r = GetComponent<Rigidbody>();
        if (r) { r.isKinematic = true; r.useGravity = false; }
    }

    private void Awake()
    {
        _col = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();

        if (_col) { _col.isTrigger = true; _col.enabled = true; }
        if (_rb) { _rb.isKinematic = true; _rb.useGravity = false; }

        Debug.Log($"[NetworkExitDoor] Awake '{name}' trigger={_col?.isTrigger} rbKinematic={_rb?.isKinematic}");
    }

    public override void Spawned()
    {
        Debug.Log($"[NetworkExitDoor] Spawned. Role={(Object && Object.HasStateAuthority ? "Host(SA)" : "Client/Proxy")}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[NetworkExitDoor] OnTriggerEnter by '{other.name}' (tag={other.tag})");

        if (_col == null || other == null) return;

        // Solo procesa cuando el collider tiene el tag de jugador
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            return;

        // Sube al root con NetworkObject
        var playerNO = other.GetComponentInParent<NetworkObject>();
        if (playerNO == null)
        {
            Debug.LogWarning("[NetworkExitDoor] Collider de Player no tiene NetworkObject en sus padres.");
            return;
        }

        var who = playerNO.InputAuthority;
        if (who == PlayerRef.None)
        {
            Debug.LogWarning("[NetworkExitDoor] PlayerRef.None, abort.");
            return;
        }

        // Fallback a través del NetworkId del avatar
        NetworkId playerNetId = playerNO.Id;

        if (Object && Object.HasStateAuthority)
        {
            Host_HandleFinish(who, playerNetId);
        }
        else
        {
            RPC_RequestFinish(who, playerNetId);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestFinish(PlayerRef who, NetworkId playerNetId)
    {
        if (who == PlayerRef.None) return;
        Host_HandleFinish(who, playerNetId);
    }

    // Host: marca terminado, entra espectador, y si todos terminaron, avanza de escena
    private void Host_HandleFinish(PlayerRef who, NetworkId playerNetId)
    {
        if (Runner == null)
        {
            Debug.LogWarning("[NetworkExitDoor] Runner null en Host_HandleFinish");
            return;
        }

        // 1) Intenta vía mapeo oficial (requiere SetPlayerObject en el spawner)
        NetworkObject playerNO = null;
        if (!Runner.TryGetPlayerObject(who, out playerNO) || playerNO == null)
        {
            // 2) Fallback: intenta resolver el avatar por su NetworkId
            if (!Runner.TryFindObject(playerNetId, out playerNO) || playerNO == null)
            {
                Debug.LogWarning($"[NetworkExitDoor] No mapping y no se pudo resolver NetworkId {playerNetId} para {who}");
                return;
            }
            Debug.Log($"[NetworkExitDoor] Fallback: resuelto avatar por NetworkId: {playerNO.name}");
        }

        // PlayerStatus
        var status = playerNO.GetComponent<PlayerStatus>();
        if (status != null)
        {
            status.IsFinished = true;
            Debug.Log($"[NetworkExitDoor] IsFinished = true para {who}");
        }
        else
        {
            Debug.LogWarning("[NetworkExitDoor] PlayerStatus no encontrado en el avatar.");
        }

        // Entra espectador (solo visual de sus compañeros activos)
        var cycler = playerNO.GetComponent<SpectatorPovCycler>();
        if (cycler != null)
        {
            cycler.RPC_EnterSpectatorOnOwner();
            Debug.Log("[NetworkExitDoor] Owner entra a espectador.");
        }
        else
        {
            Debug.LogWarning("[NetworkExitDoor] SpectatorPovCycler no encontrado en el avatar.");
        }

        // Si todos terminaron → cargar escena
        if (NetworkGameFlow.Instance != null)
        {
            NetworkGameFlow.Instance.Host_TryAdvanceIfAllFinished();
        }
        else
        {
            Debug.LogWarning("[NetworkExitDoor] No hay NetworkGameFlow en escena.");
        }
    }
}
