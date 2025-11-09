using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CartItemUI : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI totalPriceText;
    public TextMeshProUGUI quantityText;

    public Button plusButton;
    public Button minusButton;
    public Button removeButton;

    public ItemData Item { get; private set; }
    public int Quantity { get; private set; } = 1;
    private int price;

    public void Setup(ItemData item, int price)
    {
        Item = item;
        this.price = price;
        UpdateUI();

        plusButton.onClick.AddListener(() => ChangeQuantity(1));
        minusButton.onClick.AddListener(() => ChangeQuantity(-1));
        removeButton.onClick.AddListener(RemoveItem);
    }

    public void ChangeQuantity(int delta)
    {
        Quantity = Mathf.Clamp(Quantity + delta, 1, 99);
        UpdateUI();
        CartManager.Instance.UpdateTotal();
    }

    public int GetTotal() => price * Quantity;

    private void UpdateUI()
    {
        itemNameText.text = Quantity.ToString() + "x " + Item.itemName;
        //quantityText.text = Quantity.ToString() + "x";
        //totalPriceText.text = (price * Quantity).ToString();
    }

    private void RemoveItem()
    {
        Destroy(gameObject);
        CartManager.Instance.UpdateTotal();
    }
}
