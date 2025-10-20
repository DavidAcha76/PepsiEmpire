using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SealSlot : MonoBehaviour, IDropHandler
{
    public Image preview;
    public ItemData currentItem;

    public void OnDrop(PointerEventData e)
    {
        var src = e.pointerDrag ? e.pointerDrag.GetComponent<InventorySlot>() : null;
        if (!src || src.IsEmpty()) return;
        var data = src.currentItem;
        if (data == null || data.type != IngredientType.LiquidBase) return;

        if (src.RemoveFromStack(1) <= 0) return;

        currentItem = data;
        if (preview) { preview.sprite = data.icon; preview.enabled = true; }
    }

    public void Clear()
    {
        currentItem = null;
        if (preview) preview.enabled = false;
    }

    public bool IsFilled() => currentItem != null;
}
