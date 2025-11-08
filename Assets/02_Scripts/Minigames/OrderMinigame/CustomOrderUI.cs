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

    [Header("Colores de estado")]
    public Color colorPendiente = Color.yellow;
    public Color colorCorrecto = Color.green;
    public Color colorIncorrecto = Color.red;

    private List<ItemData> pedidos = new();
    private List<PedidoItemUI> pedidoUIRefs = new();
    private NPCController npcRef;
    private int currentIndex = 0;
    private bool entregando = false;

    void Awake()
    {
        btnEmpezar.onClick.AddListener(OnStartEntrega);
        btnEscoger.onClick.AddListener(OnEscogerItem);
        btnEscoger.interactable = false;
        gameObject.SetActive(false);
    }

    public void Open(NPCController npc, List<ItemData> items)
    {
        gameObject.SetActive(true);
        npcRef = npc;
        pedidos = items;

        ClearContainer();

        foreach (var item in pedidos)
        {
            var ui = Instantiate(pedidoItemPrefab, containerPedidos);
            ui.SetData(item, colorPendiente); // fondo amarillo = pendiente
            pedidoUIRefs.Add(ui);
        }

        currentIndex = 0;
        entregando = false;
        btnEmpezar.interactable = true;
        btnEscoger.interactable = false;
        HighlightCurrentItem();

        Debug.Log($"📜 [UI] Pedido abierto para {npc.name}: {pedidos.Count} ítems.");
    }

    private void OnStartEntrega()
    {
        if (currentIndex >= pedidos.Count)
        {
            Debug.Log("⚠️ [UI] No hay más ítems pendientes.");
            return;
        }

        entregando = true;
        btnEmpezar.interactable = false;
        btnEscoger.interactable = true;

        Debug.Log($"🎮 [UI] Empezando entrega del ítem #{currentIndex + 1}");
        clawMinigame.StartClawGame(this);
    }

    public void OnEscogerItem()
    {
        if (!entregando) return;
        clawMinigame.TryDeliverCurrentSlot();
    }

    public void OnItemDelivered(ItemData entregado)
    {
        if (currentIndex >= pedidos.Count) return;

        var esperado = pedidos[currentIndex];
        var ui = pedidoUIRefs[currentIndex];
        bool correcto = (entregado != null && entregado == esperado);

        // Cambiar color de fondo según resultado
        ui.MarkAs(correcto ? colorCorrecto : colorIncorrecto);

        Debug.Log(correcto
            ? $"✅ [Pedido] {entregado.itemName} correcto."
            : $"❌ [Pedido] Error, esperaba {esperado.itemName}.");

        currentIndex++;
        entregando = false;

        // Si todavía hay ítems pendientes
        if (currentIndex < pedidos.Count)
        {
            btnEmpezar.interactable = true;
            btnEscoger.interactable = false;
            HighlightCurrentItem();
        }
        else
        {
            // Todos los ítems fueron entregados (bien o mal)
            Debug.Log("🎉 Pedido completo. Cerrando panel...");
            CloseAndClear();
        }
    }

    private void HighlightCurrentItem()
    {
        for (int i = 0; i < pedidoUIRefs.Count; i++)
        {
            bool isCurrent = (i == currentIndex);
            pedidoUIRefs[i].SetOutlineActive(isCurrent);

            // También destacar visualmente el pendiente actual
            if (isCurrent)
                pedidoUIRefs[i].MarkAs(colorPendiente);
        }
    }

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
        gameObject.SetActive(false);
    }

    private void ClearContainer()
    {
        foreach (Transform c in containerPedidos)
            Destroy(c.gameObject);
        pedidoUIRefs.Clear();
        currentIndex = 0;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
