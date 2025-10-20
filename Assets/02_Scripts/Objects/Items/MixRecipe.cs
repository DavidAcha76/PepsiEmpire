using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Inventory/Recipe")]
public class MixRecipe : ScriptableObject
{
    public ItemData liquidBase;
    public ItemData solidAdditive;
    public ItemData result;

    [Header("Minigame tuning")]
    [Tooltip("Vueltas necesarias en el knob (>=1).")]
    [Range(0.5f, 15f)] public float requiredTurns = 2f;  // 2 vueltas = 720°
    [Tooltip("Cuánto retrocede el progreso por segundo si no giras.")]
    [Range(0f, 1f)] public float decayPerSecond = 0.15f;
}
