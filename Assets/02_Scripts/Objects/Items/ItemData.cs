using UnityEngine;

public enum IngredientType { LiquidBase, SolidAdditive }

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public IngredientType type;
    public Sprite icon;
    public float price;
    public bool isStackable = true;
    public int maxStack = 20;
}
