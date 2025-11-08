using Rewired;
using UnityEngine;

public class OrderPanelController : MonoBehaviour
{
    [Header("Referencias")]

    [SerializeField] public CustomOrderUI orderUI;      
    [SerializeField] public ShopQueue shopQueue;        
    [SerializeField] public Collider triggerZone;       
    [SerializeField] public KeyCode openKey = KeyCode.E;

    private bool playerInside = false;
    private bool panelOpen = false;
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

        if (player.GetButtonDown("Interact"))
        {
            if(!panelOpen)
            TryOpenPanel();
        }

        if(panelOpen && player.GetButtonDown("Close"))
            ClosePanel();
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

        if (!orderUI.gameObject.activeSelf)
            orderUI.gameObject.SetActive(true);

        orderUI.Open(currentNPC, items);
        panelOpen = true;
        Debug.Log($"🟢 [OrderPanel] Panel abierto para {currentNPC.name}");
    }

    public void ClosePanel()
    {
        if (!panelOpen) return;

        orderUI.gameObject.SetActive(false);
        panelOpen = false;
        currentNPC = null;

        Debug.Log("🔴 [OrderPanel] Panel cerrado.");
    }

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
            ClosePanel();
            Debug.Log("🔴 [OrderPanel] Jugador salió de la zona. Panel cerrado.");
        }
    }
}
