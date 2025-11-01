using UnityEngine;

/// <summary>
/// Ancla que expone la cámara usada por el jugador activo.
/// Asegúrate de asignar la referencia en el prefab del player.
/// </summary>
[DisallowMultipleComponent]
public class PlayerCameraAnchor : MonoBehaviour
{
    [Tooltip("Cámara en primera/tercera persona del jugador activo.")]
    public Camera PlayerCamera;
}
