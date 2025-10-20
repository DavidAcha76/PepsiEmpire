using UnityEngine;

[CreateAssetMenu(fileName = "NewSealRecipe", menuName = "Inventory/SealRecipe")]
public class SealRecipe : ScriptableObject
{
    [Header("Input - Output")]
    public ItemData liquidInput;          
    public ItemData sealedCanResult;     

    [Header("Tuning")]
    [Tooltip("Clicks ‘efectivos’ necesarios para llegar al 100%.")]
    [Range(5, 200)] public int requiredClicks = 40;

    [Tooltip("Cuánto se pierde por segundo si dejas de clickear (equivalente a clicks).")]
    [Range(0f, 100f)] public float decayClicksPerSecond = 6f;

    [Tooltip("Segundos de ventana para calcular CPS (solo UI).")]
    [Range(0.25f, 2f)] public float cpsWindow = 1.0f;
}
