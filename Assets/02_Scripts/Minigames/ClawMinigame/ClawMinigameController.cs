using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class ClawMinigameController : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerInventory playerInventory;
    public RectTransform clawIndicator;
    public Button entregarButton;
    public float moveSpeed = 2f;
    public float indicatorYOffset = 40f;
    public AudioSource sfx;
    public AudioClip selectSound;

    [Header("Estado")]
    public CustomOrderUI currentUI;

    private int currentIndex = 0;
    private bool movingRight = true;
    private bool active = false;
    private Player player;

    void Awake()
    {
        player = ReInput.players.GetPlayer(0);
        if (entregarButton)
            entregarButton.onClick.AddListener(OnSelectItem);
    }

    void Update()
    {
        if (!active || playerInventory == null) return;

        AutoMoveClaw();
        UpdateClawPosition();
    }

    void AutoMoveClaw()
    {
        if (playerInventory.slots.Length == 0) return;

        float t = Time.time * moveSpeed;
        float normalized = Mathf.PingPong(t, playerInventory.slots.Length - 1);
        int targetIndex = Mathf.RoundToInt(normalized);

        if (targetIndex != currentIndex)
        {
            currentIndex = targetIndex;
            HighlightSlot();
        }
    }

    void UpdateClawPosition()
    {
        if (!clawIndicator || playerInventory.slots.Length == 0) return;

        var slot = playerInventory.slots[currentIndex];
        if (!slot) return;

        Vector3 slotPos = slot.transform.position;
        clawIndicator.position = slotPos + new Vector3(0, indicatorYOffset, 0);
    }

    void HighlightSlot()
    {
        for (int i = 0; i < playerInventory.slots.Length; i++)
            playerInventory.slots[i].SetHighlight(i == currentIndex);
    }

    void OnSelectItem()
    {
        TryDeliverCurrentSlot();
    }

    /// <summary>
    /// Entrega el ítem actual (consumiendo del inventario) y notifica al CustomOrderUI.
    /// </summary>
    public void TryDeliverCurrentSlot()
    {
        if (!active || currentUI == null || playerInventory == null)
        {
            Debug.LogWarning("[ClawMinigame] No hay UI o inventario activo.");
            return;
        }

        var slot = playerInventory.slots[currentIndex];
        ItemData entregado = slot.IsEmpty() ? null : slot.currentItem;

        if (sfx && selectSound)
            sfx.PlayOneShot(selectSound);

        if (entregado != null)
        {
            slot.RemoveFromStack(1);
            Debug.Log($"📦 Consumido 1x {entregado.itemName} (slot {currentIndex}).");
        }
        else
        {
            Debug.Log("⚠️ Slot vacío, no se entregó nada.");
        }

        // ✅ Notificar resultado al UI (NO cerrar el minijuego todavía)
        currentUI.OnItemDelivered(entregado);
    }

    /// <summary>
    /// Inicia el minijuego asociado a un pedido específico.
    /// </summary>
    public void StartClawGame(CustomOrderUI ui)
    {
        if (!playerInventory || playerInventory.slots.Length == 0)
        {
            Debug.LogError("[ClawMinigame] Falta PlayerInventory o slots vacíos.");
            return;
        }

        currentUI = ui;
        currentIndex = 0;
        movingRight = true;
        active = true;
        gameObject.SetActive(true);
        HighlightSlot();
        UpdateClawPosition();

        Debug.Log("🎮 Minijuego de garra iniciado (movimiento automático).");
    }

    /// <summary>
    /// Cierra el minijuego (solo lo llama el CustomOrderUI cuando termina todo).
    /// </summary>
    public void CloseGame()
    {
        active = false;
        gameObject.SetActive(false);
        currentUI = null;
        Debug.Log("🛑 [ClawMinigame] Cerrado manualmente por el UI.");
    }
}
