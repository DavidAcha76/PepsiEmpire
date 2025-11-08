using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomOrderUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Transform containerPedidos;
    public PedidoItemUI pedidoItemPrefab;
    public Button btnEmpezar;
    public Button btnEscoger;
    public ClawMinigameController clawMinigame;
    public CameraTargetSwitcher camTargetSwitcher;

    [Header("Colores de estado")]
    public Color colorPendiente = new Color(1f, 0.92f, 0.16f); // Amarillo
    public Color colorCorrecto = new Color(0.26f, 0.83f, 0.26f); // Verde
    public Color colorIncorrecto = new Color(0.85f, 0.1f, 0.1f); // Rojo

    private List<ItemData> pedidos = new();
    private List<PedidoItemUI> pedidoUIRefs = new();
    private NPCController npcRef;
    private int currentIndex = 0;
    private bool entregando = false;
    private bool cerradoPorRetiro = false;

    void Awake()
    {
        btnEmpezar.onClick.AddListener(OnStartEntrega);
        btnEscoger.onClick.AddListener(OnEscogerItem);
        btnEscoger.interactable = false;
        gameObject.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // APERTURA Y CARGA DEL PEDIDO
    // -------------------------------------------------------------------------
    public void Open(NPCController npc, List<ItemData> items)
    {
        if (npc == null || items == null || items.Count == 0)
        {
            Debug.LogWarning("[CustomOrderUI] No hay pedido para mostrar.");
            return;
        }

        gameObject.SetActive(true);
        npcRef = npc;
        pedidos = new(items);
        cerradoPorRetiro = false;

        ClearContainer();

        foreach (var item in pedidos)
        {
            var ui = Instantiate(pedidoItemPrefab, containerPedidos);
            ui.SetData(item, colorPendiente);
            pedidoUIRefs.Add(ui);
        }

        currentIndex = 0;
        entregando = false;
        btnEmpezar.interactable = true;
        btnEscoger.interactable = false;
        HighlightCurrentItem();

        Debug.Log($"📜 [UI] Pedido abierto para {npc.name}: {pedidos.Count} ítems.");
    }

    // -------------------------------------------------------------------------
    // INICIO DEL MINIJUEGO
    // -------------------------------------------------------------------------
    private void OnStartEntrega()
    {
        if (npcRef == null)
        {
            Debug.LogWarning("⚠️ No hay NPC asociado a este pedido.");
            return;
        }

        if (cerradoPorRetiro)
        {
            Debug.Log("❌ El pedido ya expiró. No se puede entregar.");
            return;
        }

        entregando = true;
        btnEmpezar.interactable = false;
        btnEscoger.interactable = true;

        clawMinigame.StartClawGame(this);
    }

    // -------------------------------------------------------------------------
    // ENTREGA DE UN ÍTEM
    // -------------------------------------------------------------------------
    public void OnEscogerItem()
    {
        if (!entregando) return;
        clawMinigame.TryDeliverCurrentSlot();
    }

    public void OnItemDelivered(ItemData entregado)
    {
        if (cerradoPorRetiro) return;
        if (currentIndex >= pedidos.Count) return;

        var esperado = pedidos[currentIndex];
        var ui = pedidoUIRefs[currentIndex];

        // 🔹 Caso de slot vacío también se marca como error
        bool correcto = (entregado != null && entregado == esperado);
        if (entregado == null)
        {
            Debug.Log("❌ [Pedido] Slot vacío — marcado como error.");
            correcto = false;
        }

        ui.MarkAs(correcto ? colorCorrecto : colorIncorrecto);

        Debug.Log(correcto
            ? $"✅ [Pedido] {entregado?.itemName ?? "???"} correcto."
            : $"❌ [Pedido] Error, esperaba {esperado.itemName}.");

        if (correcto)
            MoneyController.Instance.AddMoney(15);

        currentIndex++;

        // 🔹 Si aún faltan ítems, NO cerrar, preparar siguiente
        if (currentIndex < pedidos.Count)
        {
            entregando = false;
            btnEmpezar.interactable = true;
            btnEscoger.interactable = false;
            HighlightCurrentItem();
            return;
        }

        // 🔹 Si completó todos los ítems (bien o mal)
        Debug.Log("🎉 Pedido completo. Cerrando panel...");
        CloseAndClear();
    }

    // -------------------------------------------------------------------------
    // DETECTA RETIRO DEL NPC
    // -------------------------------------------------------------------------
    public void OnNPCLeft()
    {
        if (gameObject.activeSelf && !cerradoPorRetiro)
        {
            cerradoPorRetiro = true;
            Debug.Log("🚪 [UI] NPC se retiró. Cerrando panel automáticamente.");
            CloseAndClear();
        }
    }

    // -------------------------------------------------------------------------
    // DESTACAR ÍTEM ACTUAL EN PANTALLA
    // -------------------------------------------------------------------------
    private void HighlightCurrentItem()
    {
        for (int i = 0; i < pedidoUIRefs.Count; i++)
            pedidoUIRefs[i].SetOutlineActive(i == currentIndex);
    }

    // -------------------------------------------------------------------------
    // CIERRE Y LIMPIEZA
    // -------------------------------------------------------------------------
    private void CloseAndClear()
    {
        bool success = true;
        foreach (var ui in pedidoUIRefs)
        {
            if (ui.IsColor(colorIncorrecto))
            {
                success = false;
                break;
            }
        }

        npcRef?.OnOrderCompleted(success);
        ClearContainer();
        npcRef = null;
        gameObject.SetActive(false);
        camTargetSwitcher.ReturnToPlayer();
    }

    private void ClearContainer()
    {
        foreach (Transform c in containerPedidos)
            Destroy(c.gameObject);
        pedidoUIRefs.Clear();
        currentIndex = 0;
    }

}
