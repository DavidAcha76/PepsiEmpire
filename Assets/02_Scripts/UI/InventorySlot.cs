using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image bgImage;
    public Image itemImage;
    public TextMeshProUGUI cantidadText;

    public ItemData currentItem;
    public int cantidadActual = 0;

    public void SetItem(ItemData item, int cantidad)
    {
        currentItem = item;
        cantidadActual = cantidad;

        itemImage.sprite = item.icon;
        itemImage.enabled = true;
        UnityEngine.Color trans = itemImage.color;
        trans.a = Mathf.Clamp01(1);
        itemImage.color = trans;
        cantidadText.text = cantidad > 1 ? cantidad.ToString() : "";
    }

    public void ClearSlot()
    {
        currentItem = null;
        cantidadActual = 0;
        itemImage.enabled = false;
        cantidadText.text = "";
    }

    public bool IsEmpty()
    {
        return currentItem == null;
    }

    public bool CanStack(ItemData item)
    {
        return currentItem != null && currentItem == item && cantidadActual < 20;
    }

    public void AddToStack(int amount)
    {
        cantidadActual = Mathf.Min(20, cantidadActual + amount);
        cantidadText.text = cantidadActual > 1 ? cantidadActual.ToString() : "";
    }
}
