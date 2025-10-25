using System.Collections.Generic;
using UnityEngine;

public class SealModelLibrary : MonoBehaviour
{
    [Header("Prefabs")]
    public List<SealPrefab> cans = new();  // mapea input → canPrefab
    public GameObject armPrefab;              // un solo brazo (o podrías tener varios)

    public GameObject GetCanPrefab(ItemData input)
    {
        foreach (var m in cans) if (m.input == input) return m.canPrefab;
        return null;
    }
}
