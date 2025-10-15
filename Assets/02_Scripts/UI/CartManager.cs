using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CartManager : MonoBehaviour
{
    public static CartManager Instance;

    public Transform cartContent;
    public GameObject cartItemPrefab;
    public TextMeshProUGUI totalText;
    
    private PlayerInventory playerInventory;
    private List<CartItemUI> cartItems = new();

    private void Awake()
    {
        Instance = this;
        playerInventory = GameObject.FindGameObjectWithTag("Player").gameObject.GetComponent<PlayerInventory>();
    }

    public void AddItem(ItemData item, int price)
    {
        // Buscar si ya existe en el carrito
        foreach (var i in cartItems)
        {
            if (i.Item == item)
            {
                i.ChangeQuantity(1);
                UpdateTotal();
                return;
            }
        }

        GameObject obj = Instantiate(cartItemPrefab, cartContent);
        CartItemUI newItem = obj.GetComponent<CartItemUI>();
        newItem.Setup(item, (int)item.price);
        cartItems.Add(newItem);
        UpdateTotal();
    }

    public void UpdateTotal()
    {
        int total = 0;
        cartItems.RemoveAll(x => x == null);

        foreach (var item in cartItems)
            total += item.GetTotal();

        totalText.text = total.ToString();
    }

    public void BuyAll()
    {
        foreach (var item in cartItems)
        {
            playerInventory.AddItem(item.Item, item.Quantity);
            Destroy(item.gameObject);
        }

        cartItems.Clear();
        UpdateTotal();
    }
}
