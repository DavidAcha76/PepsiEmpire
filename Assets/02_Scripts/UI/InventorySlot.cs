using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [Header("UI")]
    public Image bgImage;
    public Image itemImage;
    public TextMeshProUGUI cantidadText;

    [Header("Runtime")]
    public ItemData currentItem;
    public int cantidadActual = 0;

    UnityEngine.Color _normalBG = UnityEngine.Color.white;
    UnityEngine.Color _selectedBG;

    private void Awake()
    {
        ColorUtility.TryParseHtmlString("#ADADAD", out _selectedBG);
        _selectedBG.a = 199 / 255f;
        if (bgImage) _normalBG = bgImage.color;
        if (itemImage) itemImage.enabled = false;
        if (cantidadText) cantidadText.text = "";
    }

    public void SetItem(ItemData item, int cantidad)
    {
        currentItem = item;
        cantidadActual = Mathf.Clamp(cantidad, 1, GetMaxStackFor(item));

        if (itemImage)
        {
            itemImage.sprite = item.icon;
            UnityEngine.Color color = _normalBG;
            color.a = 1;
            itemImage.color = color;
            itemImage.enabled = true;
        }

        if (cantidadText)
            cantidadText.text = cantidadActual > 1 ? cantidadActual.ToString() : "";
    }

    public void ClearSlot()
    {
        currentItem = null;
        cantidadActual = 0;
        if (itemImage) itemImage.enabled = false;
        if (cantidadText) cantidadText.text = "";
    }

    public bool IsEmpty() => currentItem == null;

    public bool CanStack(ItemData item)
    {
        if (currentItem == null) return false;
        if (currentItem != item) return false;
        return cantidadActual < GetMaxStackFor(item);
    }

    public void AddToStack(int amount)
    {
        if (currentItem == null || amount <= 0) return;
        int max = GetMaxStackFor(currentItem);
        cantidadActual = Mathf.Min(max, cantidadActual + amount);
        if (cantidadText) cantidadText.text = cantidadActual > 1 ? cantidadActual.ToString() : "";
    }

    public int RemoveFromStack(int amount)
    {
        if (currentItem == null || amount <= 0) return 0;
        int removed = Mathf.Min(amount, cantidadActual);
        cantidadActual -= removed;

        if (cantidadActual <= 0) ClearSlot();
        else if (cantidadText) cantidadText.text = cantidadActual > 1 ? cantidadActual.ToString() : "";

        return removed;
    }

    public void SetHighlight(bool on)
    {
        if (!bgImage) return;
        bgImage.color = on ? _selectedBG : _normalBG;
    }

    int GetMaxStackFor(ItemData item)
    {
        return item && item.maxStack > 0 ? item.maxStack : 20;
    }
}
