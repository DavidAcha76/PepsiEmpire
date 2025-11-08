using Rewired;
using UnityEngine;

public class OrderPanelController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] public CustomOrderUI orderUI;
    [SerializeField] public ShopQueue shopQueue;
    [SerializeField] public Collider triggerZone;
    [SerializeField] public CameraTargetSwitcher cameraTargetSwitcher;

    private bool playerInside = false;
    public bool panelOpen = false;
    private NPCController currentNPC;
    private Player player;

    void Awake()
    {
        player = ReInput.players.GetPlayer(0);

        if (!triggerZone)
        {
            triggerZone = GetComponent<Collider>();
            if (triggerZone)
                triggerZone.isTrigger = true;
        }
    }

    void Update()
    {
        if (!playerInside) return;

        // 🔹 Solo abrir el panel con la tecla Interact
        if (player.GetButtonDown("Interact") && !panelOpen)
        {
            TryOpenPanel();
        }
    }

    private void TryOpenPanel()
    {
        currentNPC = shopQueue?.GetCurrentFrontNPC();

        if (currentNPC == null)
        {
            Debug.Log("⚠️ [OrderPanel] No hay NPC esperando al frente.");
            return;
        }

        var items = currentNPC.GetCurrentOrderItems();
        if (items == null || items.Count == 0)
        {
            Debug.Log("⚠️ [OrderPanel] El NPC no tiene pedido activo.");
            return;
        }

        // 🔹 Solo abre el panel, sin reset ni limpieza
        if (!orderUI.gameObject.activeSelf)
            orderUI.gameObject.SetActive(true);

        cameraTargetSwitcher.LookAtOtherTarget();
        orderUI.Open(currentNPC, items);

        panelOpen = true;
        Debug.Log($"🟢 [OrderPanel] Panel abierto para {currentNPC.name}");
    }

    // 🔹 Ya no hay ClosePanel manual desde Rewired ni por tecla
    // 🔹 Si el jugador sale del área, solo se desactiva visualmente

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log("🟢 [OrderPanel] Jugador entró a la zona.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            // Si sale del trigger pero hay pedido pendiente, solo ocultar visualmente el panel
            if (orderUI != null && orderUI.gameObject.activeSelf)
            {
                orderUI.gameObject.SetActive(false);
                Debug.Log("🔸 [OrderPanel] Jugador salió de la zona — Panel ocultado (pedido sigue pendiente).");
            }

            // La cámara siempre vuelve al jugador
            cameraTargetSwitcher.ReturnToPlayer();
        }
    }
}
