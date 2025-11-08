using Unity.Cinemachine;
using UnityEngine;

public class OrbitalFollowFixer : MonoBehaviour
{
    [Header("Referencias")]
    public CinemachineOrbitalFollow orbitalFollow;
    public PlayerComponentController playerController;

    [Header("Valores fijos (modo cinemática)")]
    public float fixedHorizontal = -177.8445f;
    public float fixedVertical = 0.03711254f;
    public float fixedRadial = 1f;

    [Header("Valores originales (para restaurar)")]
    private float originalHorizontal;
    private float originalVertical;
    private float originalRadial;
    private bool hasBackup = false;

    void Reset()
    {
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
    }

    /// <summary>
    /// Fija los valores del OrbitalFollow y desactiva su control automático.
    /// </summary>
    public void SetExactOrbitalPose()
    {
        if (!orbitalFollow)
        {
            Debug.LogWarning("⚠️ [OrbitalFollowFixer] No se asignó CinemachineOrbitalFollow.");
            return;
        }

        playerController.DisableAllWithoutRenders();

        // Guardar valores originales (solo una vez)
        if (!hasBackup)
        {
            originalHorizontal = orbitalFollow.HorizontalAxis.Value;
            originalVertical = orbitalFollow.VerticalAxis.Value;
            originalRadial = orbitalFollow.RadialAxis.Value;
            hasBackup = true;
        }

        orbitalFollow.enabled = true;

        orbitalFollow.HorizontalAxis.Value = fixedHorizontal;
        orbitalFollow.VerticalAxis.Value = fixedVertical;
        orbitalFollow.RadialAxis.Value = fixedRadial;

        Debug.Log($"🎯 [OrbitalFollowFixer] Cámara fijada. H:{fixedHorizontal}, V:{fixedVertical}, R:{fixedRadial}");

        // Desactivar para congelar posición
        orbitalFollow.enabled = false;
    }

    /// <summary>
    /// Restaura los valores originales del OrbitalFollow y reactiva el control normal.
    /// </summary>
    public void RestoreDefaultPose()
    {
        if (!orbitalFollow)
        {
            Debug.LogWarning("⚠️ [OrbitalFollowFixer] No se asignó CinemachineOrbitalFollow.");
            return;
        }

        if (!hasBackup)
        {
            Debug.Log("ℹ️ [OrbitalFollowFixer] No hay valores originales guardados, restaurando con valores actuales.");
            orbitalFollow.enabled = true;
            return;
        }
        playerController.EnableAllWithoutRenders();

        orbitalFollow.enabled = true;

        orbitalFollow.HorizontalAxis.Value = originalHorizontal;
        orbitalFollow.VerticalAxis.Value = originalVertical;
        orbitalFollow.RadialAxis.Value = originalRadial;

        Debug.Log($"🔄 [OrbitalFollowFixer] Cámara restaurada. H:{originalHorizontal}, V:{originalVertical}, R:{originalRadial}");
    }
}
