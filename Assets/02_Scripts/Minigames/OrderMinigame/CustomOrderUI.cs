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
            ui.SetData(item, colorPendiente);
            pedidoUIRefs.Add(ui);
        }

        currentIndex = 0;
        btnEmpezar.interactable = true;
        btnEscoger.interactable = false;
        entregando = false;
        HighlightCurrentItem();

        Debug.Log($"📜 [UI] Pedido abierto para {npc.name}: {pedidos.Count} ítems.");
    }

    private void OnStartEntrega()
    {
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

        ui.MarkAs(correcto ? colorCorrecto : colorIncorrecto);
        Debug.Log(correcto
            ? $"✅ [Pedido] {entregado.itemName} correcto."
            : $"❌ [Pedido] Error, esperaba {esperado.itemName}.");

        currentIndex++;

        if (currentIndex >= pedidos.Count)
        {
            Debug.Log("🎉 Pedido completo. Cerrando panel...");
            CloseAndClear();
        }
        else
        {
            entregando = false;
            btnEmpezar.interactable = true;
            btnEscoger.interactable = false;
            HighlightCurrentItem();
        }
    }

    private void HighlightCurrentItem()
    {
        for (int i = 0; i < pedidoUIRefs.Count; i++)
            pedidoUIRefs[i].SetOutlineActive(i == currentIndex);
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
}
