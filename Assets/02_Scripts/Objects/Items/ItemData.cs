using UnityEngine;

public enum IngredientType { LiquidBase, SolidAdditive }

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Datos Generales")]
    public string itemName;
    public IngredientType type;
    public Sprite icon;
    public float price;
    public bool isStackable = true;
    public int maxStack = 20;

    [Header("Visual (para mezcla y shader)")]
    [Tooltip("Color representativo del ingrediente (usado en el shader).")]
    public Color visualColor = Color.white;
}
