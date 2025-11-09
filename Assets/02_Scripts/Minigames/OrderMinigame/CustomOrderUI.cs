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
    public Button btnClose;
    public ClawMinigameController clawMinigame;
    public CameraTargetSwitcher camTargetSwitcher;
    public OrderPanelController orderPanelController;

    [Header("Colores de estado")]
    public Color colorPendiente = new Color(1f, 0.92f, 0.16f); // Amarillo
    public Color colorCorrecto = new Color(0.26f, 0.83f, 0.26f); // Verde
    public Color colorIncorrecto = new Color(0.85f, 0.1f, 0.1f); // Rojo

    private readonly List<ItemData> pedidos = new();
    private readonly List<PedidoItemUI> pedidoUIRefs = new();
    private NPCController npcRef;
    private int currentIndex;
    private bool entregando;
    private bool cerradoPorRetiro;

    void Awake()
    {
        if (btnEmpezar == null || btnEscoger == null)
        {
            Debug.LogError("❌ [CustomOrderUI] Faltan referencias de botones.");
            return;
        }

        btnEmpezar.onClick.AddListener(OnStartEntrega);
        btnEscoger.onClick.AddListener(OnEscogerItem);

        btnEscoger.interactable = false;
        gameObject.SetActive(false);
    }

    // ---------------------------------------------------------------------
    // ABRIR PANEL Y CARGAR PEDIDO
    // ---------------------------------------------------------------------
    public void Open(NPCController npc, List<ItemData> items)
    {
        Debug.Log($"🟢 [CustomOrderUI] OPEN por {npc?.name ?? "null"} ({items?.Count ?? 0} ítems)");

        if (npc == null || items == null || items.Count == 0)
        {
            Debug.LogWarning("[CustomOrderUI] Pedido inválido o vacío.");
            return;
        }

        // 🔹 Si es el mismo NPC y ya hay ítems cargados, no limpiar (solo volver a mostrar)
        if (npcRef == npc && pedidoUIRefs.Count > 0)
        {
            gameObject.SetActive(true);
            Debug.Log("♻️ [CustomOrderUI] Reabriendo panel existente sin limpiar.");
            orderPanelController.panelOpen = true;
            return;
        }

        // 🔹 Si es un nuevo NPC o pedido, limpiar normalmente
        orderPanelController.panelOpen = true;
        npcRef = npc;
        cerradoPorRetiro = false;
        entregando = false;
        currentIndex = 0;

        gameObject.SetActive(true);
        ClearContainer();

        foreach (var item in items)
        {
            var ui = Instantiate(pedidoItemPrefab, containerPedidos);
            ui.SetData(item, colorPendiente);
            pedidoUIRefs.Add(ui);
            Debug.Log($"📦 Añadido {item.itemName}");
        }

        btnEmpezar.interactable = true;
        btnEscoger.interactable = false;
        HighlightCurrentItem();
    }

    // ---------------------------------------------------------------------
    // EMPIEZA EL MINIJUEGO
    // ---------------------------------------------------------------------
    private void OnStartEntrega()
    {
        if (npcRef == null || cerradoPorRetiro)
        {
            Debug.Log("🚫 No se puede iniciar entrega.");
            return;
        }

        entregando = true;
        btnEmpezar.interactable = false;
        btnEscoger.interactable = true;
        btnClose.interactable = false;

        Debug.Log($"🎮 Iniciando entrega #{currentIndex + 1}");
        clawMinigame.StartClawGame(this);
    }

    // ---------------------------------------------------------------------
    // JUGADOR PRESIONA “ESCOGER”
    // ---------------------------------------------------------------------
    public void OnEscogerItem()
    {
        if (!entregando)
        {
            Debug.Log("⚠️ [UI] Intento de entregar sin estar en modo entrega.");
            return;
        }

        clawMinigame.TryDeliverCurrentSlot();
    }

    // ---------------------------------------------------------------------
    // RESULTADO DE ENTREGA (llamado por ClawMinigame)
    // ---------------------------------------------------------------------
    public void OnItemDelivered(ItemData entregado)
    {
        if (cerradoPorRetiro) return;
        if (currentIndex >= pedidoUIRefs.Count) return;

        var esperado = npcRef.GetCurrentOrderItems()[currentIndex];
        var ui = pedidoUIRefs[currentIndex];
        bool correcto = (entregado != null && entregado == esperado);

        ui.MarkAs(correcto ? colorCorrecto : colorIncorrecto);

        Debug.Log(correcto
            ? $"✅ {entregado?.itemName ?? "???"} correcto."
            : $"❌ Error, esperaba {esperado.itemName}.");

        if (correcto)
            MoneyController.Instance.AddMoney(15);
        else
            LifeSystemManager.Instance.LoseLifeByOrderError();

            currentIndex++;
        btnClose.interactable = true;

        // 🔹 Si aún hay ítems → continuar
        if (currentIndex < pedidoUIRefs.Count)
        {
            entregando = false;
            btnEmpezar.interactable = true;
            btnEscoger.interactable = false;
            HighlightCurrentItem();
            return;
        }

        // 🔹 Si ya terminó todo el pedido
        bool success = true;
        foreach (var p in pedidoUIRefs)
        {
            if (p.IsColor(colorIncorrecto))
            {
                success = false;
                break;
            }
        }

        npcRef?.OnOrderCompleted(success);

        Debug.Log(success
            ? "🎉 Pedido completado correctamente."
            : "⚠️ Pedido completado con errores.");

        clawMinigame.CloseGame();
        CloseAndClear();
    }

    // ---------------------------------------------------------------------
    // NPC SE RETIRA AUTOMÁTICAMENTE
    // ---------------------------------------------------------------------
    public void OnNPCLeft()
    {
        if (!gameObject.activeSelf || cerradoPorRetiro) return;

        cerradoPorRetiro = true;
        Debug.Log("🚪 [UI] NPC se retiró. Cerrando panel automáticamente.");
        CloseAndClear();
    }

    // ---------------------------------------------------------------------
    // RESALTAR EL ITEM ACTUAL
    // ---------------------------------------------------------------------
    private void HighlightCurrentItem()
    {
        for (int i = 0; i < pedidoUIRefs.Count; i++)
            pedidoUIRefs[i].SetOutlineActive(i == currentIndex);
    }

    // ---------------------------------------------------------------------
    // CIERRE Y LIMPIEZA DEL PANEL
    // ---------------------------------------------------------------------
    private void CloseAndClear()
    {
        npcRef = null;
        ClearContainer();
        gameObject.SetActive(false);
        camTargetSwitcher?.ReturnToPlayer();
        orderPanelController.panelOpen = false;
        btnEmpezar.interactable = false;
        btnEscoger.interactable = false;

        Debug.Log("🔴 [UI] Panel cerrado y limpiado.");
    }

    // ---------------------------------------------------------------------
    // LIMPIA EL CONTENEDOR VISUAL
    // ---------------------------------------------------------------------
    private void ClearContainer()
    {
        foreach (Transform c in containerPedidos)
            Destroy(c.gameObject);

        pedidoUIRefs.Clear();
        currentIndex = 0;
    }

    public void ClosePanel()
    {

        // 🔹 Solo ocultar el panel visualmente, sin limpiar
        gameObject.SetActive(false);
        camTargetSwitcher?.ReturnToPlayer();

        // 🔹 Marcamos que el panel ya no está visible,
        // pero mantenemos los pedidos y colores como están
        if (orderPanelController != null)
            orderPanelController.panelOpen = false;

        Debug.Log("🟠 [UI] Panel ocultado (pedido incompleto permanece intacto).");
    }

}
