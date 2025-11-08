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
    public AudioClip changeSlotSound;
    [Header("Estado")]
    public CustomOrderUI currentUI;
    private int currentIndex;
    private bool active;
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
        AutoMoveClaw(); UpdateClawPosition();
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
            sfx.PlayOneShot(changeSlotSound);
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

    public void TryDeliverCurrentSlot()
    {
        if (!active || currentUI == null || playerInventory == null) return;

        var slot = playerInventory.slots[currentIndex];
        if (slot == null || slot.IsEmpty())
        {
            Debug.Log("⚠️ Slot vacío, no se entregará nada.");
            if (sfx && selectSound)
                sfx.PlayOneShot(selectSound);
            return;
        }

        ItemData entregado = slot.currentItem;
        slot.RemoveFromStack(1);

        if (sfx && selectSound)
            sfx.PlayOneShot(selectSound);

        Debug.Log($"📦 Entregado 1x {entregado.itemName} (slot {currentIndex}).");
        active = false;

        // Reportar entrega al UI
        currentUI.OnItemDelivered(entregado);
    }

    public void StartClawGame(CustomOrderUI ui)
    {
        currentUI = ui;
        currentIndex = 0;
        active = true;

        gameObject.SetActive(true);
        HighlightSlot();
        UpdateClawPosition();
    }

    public void CloseGame()
    {
        active = false;
        gameObject.SetActive(false);
        currentUI = null;
    }
}