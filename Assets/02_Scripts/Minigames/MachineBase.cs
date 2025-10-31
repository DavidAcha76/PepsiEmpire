using UnityEngine;

public abstract class MachineBase : MonoBehaviour
{
    [Header("Sistema de Mejora")]
    public MachineController controller; // referencia al nivel actual


    protected virtual void UpdateMachine(float deltaTime) { }

    // Llamado cuando se aplica una mejora desde FactoryUpgradeManager
    public virtual void OnUpgradeApplied(float newLevel) { }

    void Update()
    {
        UpdateMachine(Time.deltaTime);
    }
}
