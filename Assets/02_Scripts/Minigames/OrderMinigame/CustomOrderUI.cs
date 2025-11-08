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
    public Color colorPendiente = new Color(1f, 0.92f, 0.16f);
    public Color colorCorrecto = new Color(0.26f, 0.83f, 0.26f);
    public Color colorIncorrecto = new Color(0.85f, 0.1f, 0.1f);

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
            Debug.LogError("❌ [CustomOrderUI] Botones no asignados. Asigna btnEmpezar y btnEscoger en el inspector.");
            return;
        }

        btnEmpezar.onClick.AddListener(OnStartEntrega);
        btnEscoger.onClick.AddListener(OnEscogerItem);

        btnEscoger.interactable = false;
        gameObject.SetActive(false);
    }

    // ---------------------------------------------------------------------
    public void Open(NPCController npc, List<ItemData> items)
    {
        Debug.Log($"🟢 [CustomOrderUI] OPEN llamado por {npc?.name ?? "null"} con {items?.Count ?? 0} items");

        if (npc == null || items == null || items.Count == 0)
        {
            Debug.LogWarning("[CustomOrderUI] No hay pedido válido para mostrar.");
            return;
        }

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
            Debug.Log($"➕ Agregado UI para {item.itemName}");
        }

        btnEmpezar.interactable = true;
        btnEscoger.interactable = false;
        HighlightCurrentItem();
    }

    // ---------------------------------------------------------------------
    private void OnStartEntrega()
    {
        if (npcRef == null)
        {
            Debug.LogWarning("[CustomOrderUI] NPC de referencia es nulo.");
            return;
        }

        if (cerradoPorRetiro)
        {
            Debug.Log("🚫 Pedido expirado, no se puede entregar.");
            return;
        }

        entregando = true;
        btnEmpezar.interactable = false;
        btnEscoger.interactable = true;

        Debug.Log($"🎮 [UI] Iniciando entrega #{currentIndex + 1}");
        clawMinigame.StartClawGame(this);
    }

    // ---------------------------------------------------------------------
    public void OnEscogerItem()
    {
        if (!entregando)
        {
            Debug.Log("⚠️ [UI] Intento de entregar sin estar entregando.");
            return;
        }

        clawMinigame.TryDeliverCurrentSlot();
    }

    // ---------------------------------------------------------------------
    public void OnItemDelivered(ItemData entregado)
    {
        if (cerradoPorRetiro) return;
        if (currentIndex >= pedidos.Count) return;

        var esperado = pedidos[currentIndex];
        var ui = pedidoUIRefs[currentIndex];

        bool correcto = (entregado != null && entregado == esperado);
        if (entregado == null)
        {
            Debug.Log("❌ Slot vacío — error automático.");
            correcto = false;
        }

        ui.MarkAs(correcto ? colorCorrecto : colorIncorrecto);

        Debug.Log(correcto
            ? $"✅ [Pedido] {entregado?.itemName ?? "???"} correcto."
            : $"❌ [Pedido] Error, esperaba {esperado.itemName}.");

        if (correcto)
            MoneyController.Instance.AddMoney(15);

        // Avanza al siguiente ítem
        currentIndex++;

        // ⚙️ Si todavía faltan ítems → continuar flujo normal
        if (currentIndex < pedidos.Count)
        {
            entregando = false;
            btnEmpezar.interactable = true;
            btnEscoger.interactable = false;
            HighlightCurrentItem();
            return;
        }

        // ⚙️ Si ya se entregaron todos los ítems, evaluar resultado total:
        bool success = true;
        foreach (var itemUI in pedidoUIRefs)
        {
            if (itemUI.IsColor(colorIncorrecto))
            {
                success = false;
                break;
            }
        }

        Debug.Log(success
            ? "🎉 [Pedido] Todos los ítems correctos. Pedido exitoso."
            : "⚠️ [Pedido] Algunos ítems fueron incorrectos. Pedido fallido.");

        npcRef?.OnOrderCompleted(success);

        // Cerrar el minijuego y panel
        clawMinigame.CloseGame();
        CloseAndClear();
    }

    // ---------------------------------------------------------------------
    public void OnNPCLeft()
    {
        if (!gameObject.activeSelf || cerradoPorRetiro) return;

        cerradoPorRetiro = true;
        Debug.Log("🚪 NPC se retiró. Cierre automático del panel.");
        CloseAndClear();
    }

    // ---------------------------------------------------------------------
    private void HighlightCurrentItem()
    {
        for (int i = 0; i < pedidoUIRefs.Count; i++)
        {
            bool active = i == currentIndex;
            pedidoUIRefs[i].SetOutlineActive(active);
        }

        Debug.Log($"🔸 [UI] Resaltando item #{currentIndex + 1}");
    }

    // ---------------------------------------------------------------------
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
        camTargetSwitcher?.ReturnToPlayer();

        Debug.Log("🔴 [UI] Panel cerrado y limpiado.");
    }

    // ---------------------------------------------------------------------
    private void ClearContainer()
    {
        foreach (Transform c in containerPedidos)
            Destroy(c.gameObject);

        pedidoUIRefs.Clear();
        currentIndex = 0;
    }

}
