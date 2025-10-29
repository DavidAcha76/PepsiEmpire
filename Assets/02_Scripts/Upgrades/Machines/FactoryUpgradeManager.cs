using System.Linq;
using UnityEngine;

public class FactoryUpgradeManager : MonoBehaviour
{
    public static FactoryUpgradeManager Instance;
    public Transform factoryRoot; // raíz donde están las máquinas instanciadas

    private void Awake() => Instance = this;

    public void UpgradeMachine(MachineController machine)
    {
        int newLevel = machine.GetCurrentLevel() + 1;
        if (newLevel > machine.upgradeData.maxLevel)
        {
            Debug.LogWarning("🧱 Ya está en el nivel máximo.");
            return;
        }

        // detectar si hay cambio visual (niveles 4 y 7)
        bool cambioVisual = (newLevel == 4 || newLevel == 7);

        // aplicar valores numéricos
        machine.ApplyUpgrade(newLevel);

        // aplicar cambio visual si corresponde
        if (cambioVisual)
            CambiarModelo(machine, newLevel);
    }

    private void CambiarModelo(MachineController machine, int level)
    {
        if (!machine.upgradeData || machine.upgradeData.machinePrefabs.Count() == 0)
        {
            Debug.LogWarning("⚠️ No hay prefabs definidos en la base de datos de mejoras.");
            return;
        }

        int index = (level <= 3) ? 0 : (level <= 6) ? 1 : 2;
        GameObject newPrefab = machine.upgradeData.machinePrefabs[index];
        if (newPrefab == null)
        {
            Debug.LogWarning($"⚠️ Prefab faltante para nivel {level} en {machine.upgradeData.machineName}");
            return;
        }

        // 🔹 Eliminamos el modelo visual actual (solo el hijo asignado)
        if (machine.currentModel != null)
        {
            Destroy(machine.currentModel);
            Debug.Log($"💥 Modelo anterior '{machine.currentModel.name}' eliminado.");
        }

        // 🔹 Instanciamos el nuevo modelo como hijo
        GameObject newModel = Instantiate(newPrefab, machine.modelParent ?? machine.transform);
        //newModel.transform.localPosition = Vector3.zero;
        //newModel.transform.localRotation = Quaternion.identity;

        // 🔹 Guardamos la nueva referencia
        machine.currentModel = newModel;

        Debug.Log($"🆕 Modelo visual actualizado: {newModel.name} (nivel {level})");
    }
}
