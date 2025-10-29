using UnityEngine;

public class MachineController : MonoBehaviour
{
    [Header("Datos de mejora")]
    public UpgradeData upgradeData;
    public int currentLevel = 1;

    [Header("Referencias visuales")]
    public Transform modelParent;     // dónde colocar los modelos (si no, se usa transform raíz)
    public GameObject currentModel;   // referencia directa al modelo activo

    [Header("Atributos base")]
    public float baseProductionSpeed = 1f;
    public float baseEfficiency = 1f;

    float currentProductionSpeed;
    float currentEfficiency;

    void Start()
    {
        // inicializa valores base
        ApplyUpgrade(currentLevel);

        if (currentModel == null && transform.childCount > 0)
        {
            // si no se asignó manualmente, toma el primer hijo
            currentModel = transform.GetChild(0).gameObject;
        }
    }

    public int GetCurrentLevel() => currentLevel;

    public void ApplyUpgrade(int newLevel)
    {
        currentLevel = newLevel;
        if (upgradeData == null) return;

        float speedMult = 1;
        float effMult = 1;

        currentProductionSpeed = baseProductionSpeed * speedMult;
        currentEfficiency = baseEfficiency * effMult;

        Debug.Log($"⚙️ Mejora aplicada a '{upgradeData.machineName}' — Nivel {newLevel}");
        Debug.Log($"   🔸 Producción: {currentProductionSpeed:F2}");
        Debug.Log($"   🔸 Eficiencia: {currentEfficiency:F2}");
    }
}
