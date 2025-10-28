using UnityEngine;

public class FactoryUpgradeManager : MonoBehaviour
{
    public static FactoryUpgradeManager Instance;

    private void Awake() => Instance = this;

    public Transform factoryRoot; // donde están los objetos actuales

    public void ApplyUpgrade(UpgradeData data, int newLevel)
    {
        // Destruir los anteriores
        foreach (Transform child in factoryRoot)
            if (child.name.Contains(data.title))
                Destroy(child.gameObject);

        // Instanciar el prefab correspondiente
        if (data.prefabsPorNivel.Length >= newLevel)
        {
            var newPrefab = data.prefabsPorNivel[newLevel - 1];
            Instantiate(newPrefab, factoryRoot);
        }

        Debug.Log($"Aplicada mejora de '{data.title}' al nivel {newLevel}");
    }
}
