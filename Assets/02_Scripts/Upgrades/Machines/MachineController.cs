using UnityEngine;

public class MachineController : MonoBehaviour
{
    [Header("Referencias")]
    public MachineBase machine;

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

        machine.OnUpgradeApplied(upgradeData.newValue[newLevel - 1]);


        Debug.Log($"⚙️ Mejora aplicada a '{upgradeData.machineName}' — Nivel {newLevel}");
    }
}
