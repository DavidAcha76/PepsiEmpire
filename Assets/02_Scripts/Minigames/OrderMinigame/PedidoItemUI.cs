using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PedidoItemUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI label;
    public Image background;
    public Outline outline;

    private Color currentColor;

    public void SetData(ItemData item, Color c)
    {
        if (icon) icon.sprite = item.icon;
        if (label) label.text = item.itemName.ToUpper();
        if (background) background.color = c;
        currentColor = c;
        if (outline) outline.enabled = false;
    }

    public void MarkAs(Color c)
    {
        if (background) background.color = c;
        currentColor = c;
    }

    public bool IsColor(Color c)
    {
        return Mathf.Approximately(currentColor.r, c.r)
            && Mathf.Approximately(currentColor.g, c.g)
            && Mathf.Approximately(currentColor.b, c.b);
    }

    public void SetOutlineActive(bool active)
    {
        if (outline)
            outline.enabled = active;
    }
}
