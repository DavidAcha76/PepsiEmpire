using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MixSlot : MonoBehaviour, IDropHandler
{
    public IngredientType acceptType = IngredientType.LiquidBase; 
    public Image preview;

    public ItemData currentItem;

    public void OnDrop(PointerEventData eventData)
    {
        var srcGO = eventData.pointerDrag;
        if (!srcGO) return;

        var srcSlot = srcGO.GetComponent<InventorySlot>();
        if (!srcSlot || srcSlot.IsEmpty()) return;

        var data = srcSlot.currentItem;
        if (data == null || data.type != acceptType) return;

        int removed = srcSlot.RemoveFromStack(1);
        if (removed <= 0) return;

        currentItem = data;
        if (preview)
        {
            preview.sprite = data.icon;
            preview.enabled = true;
        }
    }

    public void Clear()
    {
        currentItem = null;
        if (preview) preview.enabled = false;
    }

    public bool IsFilled() => currentItem != null;
}
