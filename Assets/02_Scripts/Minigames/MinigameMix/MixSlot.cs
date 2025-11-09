using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MixSlot : MonoBehaviour, IDropHandler
{
    public IngredientType acceptType = IngredientType.LiquidBase; 
    public Image preview;
    public List<ItemData> acceptedItems;

    public ItemData currentItem;

    public void OnDrop(PointerEventData eventData)
    {
        var srcGO = eventData.pointerDrag;
        if (!srcGO) return;

        var srcSlot = srcGO.GetComponent<InventorySlot>();
        if (!srcSlot || srcSlot.IsEmpty()) return;

        var data = srcSlot.currentItem;
        if (data == null || data.type != acceptType) return;

        if (!acceptedItems.Contains(data))
        {
            Debug.Log($"🚫 {data.itemName} no está permitido en este slot.");
            return;
        }

        int removed = srcSlot.RemoveFromStack(1);
        if (removed <= 0) return;

        currentItem = data;
        if (preview)
        {
            preview.sprite = data.icon;
            preview.color = Color.white;
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
