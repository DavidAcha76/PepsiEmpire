using UnityEngine;

[CreateAssetMenu(menuName = "Factory/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Info General")]
    public string machineName;
    public Sprite[] icons = new Sprite[2];

    [Header("Niveles de Mejora")]
    [Range(1, 9)] public int maxLevel = 9;

    [Tooltip("Costo por cada nivel (index = nivel-1)")]
    public int[] costs = new int[9];

    [Tooltip("Descripción por nivel")]
    [TextArea] public string[] descriptions = new string[9];

    [Header("Prefabs de Máquina por Fase")]
    [Tooltip("Modelos que cambian visualmente (niveles 1-3 usan prefab[0], 4-6 usan prefab[1], 7-9 usan prefab[2])")]
    public GameObject[] machinePrefabs = new GameObject[3];

    [Header("Mejoras de Variables")]
    [Tooltip("Cada nivel modifica variables específicas de la máquina")]
    public float[] newValue = new float[9];
}
