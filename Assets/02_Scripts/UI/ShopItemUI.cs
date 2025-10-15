using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI nombreText;
    public TextMeshProUGUI precioText;
    public Image iconoImage;
    public Button addToCartButton;

    [Header("Datos del ítem")]
    public ItemData itemData;

    [Header("Managers")]
    public CartManager cartManager;

    private void Start()
    {
        if (itemData != null)
        {
            nombreText.text = itemData.itemName;
            precioText.text = itemData.price.ToString();
            iconoImage.sprite = itemData.icon;
        }

        addToCartButton.onClick.AddListener(AgregarAlCarrito);
    }

    private void AgregarAlCarrito()
    {
        if (cartManager != null && itemData != null)
        {
            cartManager.AddItem(itemData, 1);
        }
        else
        {
            Debug.LogWarning("Falta asignar CartManager o ItemData en " + gameObject.name);
        }
    }
}
