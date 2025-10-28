using UnityEngine;

[CreateAssetMenu(menuName = "Factory/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string title;
    public Sprite icon;
    [TextArea] public string[] descriptions;
    public int[] costs;
    public int maxLevel => Mathf.Min(descriptions.Length, costs.Length);

    [Header("Prefabs a actualizar")]
    public GameObject[] prefabsPorNivel;
}
