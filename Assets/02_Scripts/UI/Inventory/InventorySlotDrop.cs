using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotDrop : MonoBehaviour, IDropHandler
{
    private InventorySlot slot;

    private void Awake()
    {
        slot = GetComponent<InventorySlot>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Origen del drag
        var pickup = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<PickupSlot>() : null;
        if (pickup && !pickup.IsEmpty())
        {
            TryReceiveFromPickup(pickup);
            return;
        }

        // (en el futuro podrías soportar arrastrar entre slots también)
    }

    private void TryReceiveFromPickup(PickupSlot pickup)
    {
        if (pickup == null || pickup.IsEmpty()) return;

        var item = pickup.currentItem;
        int cantidad = pickup.cantidadActual;

        // Buscamos PlayerInventory
        var inv = FindAnyObjectByType<PlayerInventory>();
        if (!inv)
        {
            Debug.LogWarning("No se encontró PlayerInventory en escena.");
            return;
        }

        // Intentar agregar
        if (inv.HasSpaceFor(item, cantidad))
        {
            inv.AddItem(item, cantidad);
            pickup.Clear();
        }
        else
        {
            Debug.Log("Inventario lleno. No se pudo agregar el ítem.");
        }
    }
}
