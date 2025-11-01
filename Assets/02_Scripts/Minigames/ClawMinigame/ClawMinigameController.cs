using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class ClawMinigameController : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerInventory playerInventory;
    public RectTransform clawIndicator;
    public Button entregarButton;
    public float moveSpeed = 2f; // velocidad de movimiento automático
    public float indicatorYOffset = 40f;
    public AudioSource sfx;
    public AudioClip selectSound;

    [Header("Estado")]
    public CustomOrder currentOrder;

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

        // mover de izquierda a derecha automáticamente
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
        if (!active || currentOrder == null) return;

        var slot = playerInventory.slots[currentIndex];
        if (slot.IsEmpty())
        {
            Debug.Log("❌ Slot vacío, no se puede entregar nada");
            return;
        }

        if (sfx && selectSound) sfx.PlayOneShot(selectSound);

        // comparar con el pedido
        currentOrder.CompletarPedido(slot.currentItem);

        // cerrar minijuego
        gameObject.SetActive(false);
        active = false;
        HighlightSlot();
        Debug.Log($"🎯 Entregado: {slot.currentItem.itemName}");
    }

    public void StartClawGame(CustomOrder order)
    {
        if (!playerInventory || playerInventory.slots.Length == 0)
        {
            Debug.LogError("[ClawMinigame] Falta PlayerInventory o slots vacíos.");
            return;
        }

        currentOrder = order;
        currentIndex = 0;
        movingRight = true;
        active = true;
        gameObject.SetActive(true);
        HighlightSlot();
        UpdateClawPosition();
        Debug.Log("🎮 Claw Minigame iniciado (movimiento automático).");
    }

    public void CloseGame()
    {
        gameObject.SetActive(false);
        active = false;
        HighlightSlot();
        currentOrder = null;
    }
}
