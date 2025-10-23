using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Inventory/Recipe")]
public class MixRecipe : ScriptableObject
{
    [Header("Ingredientes")]
    public ItemData liquidBase;      // Ej: Pepsi
    public ItemData solidAdditive;   // Ej: Naranja
    public ItemData result;          // Ej: Pepsi Naranja

    [Header("Ajustes del minijuego")]
    [Range(0.5f, 15f)] public float requiredTurns = 2f;
    [Range(0f, 1f)] public float decayPerSecond = 0.15f;

    [Header("Visual (para shader y color final)")]
    [Tooltip("Color final de la mezcla (si lo dejas blanco, se calcula automáticamente).")]
    public Color resultColor = Color.white;
}
