using UnityEngine;

public class SealerMachineRig : MonoBehaviour
{
    [Header("Anchors en la máquina")]
    public Transform canAnchor;     // dónde se posa el fondo de la lata
    public Transform armTipAnchor;  // dónde debe quedar la punta del brazo al inicio (arriba)
    public Transform armUpRef;      // referencia visual arriba (para lógica futura)
    public Transform armDownRef;    // referencia visual abajo (tapa)
}
