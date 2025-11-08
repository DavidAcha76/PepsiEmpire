using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PedidoItemUI : MonoBehaviour
{
    public Image icon;
    public Image background;
    public TextMeshProUGUI itemName;
    public Outline outline;

    private Color currentColor;

    public void SetData(ItemData item, Color baseColor)
    {
        if (icon) icon.sprite = item.icon;
        if (itemName) itemName.text = item.itemName;
        MarkAs(baseColor);
    }

    public void MarkAs(Color c)
    {
        currentColor = c;
        if (background)
            background.color = c;
    }

    public bool IsColor(Color c)
    {
        return currentColor == c;
    }

    public void SetOutlineActive(bool active)
    {
        if (outline)
            outline.enabled = active;
    }
}
