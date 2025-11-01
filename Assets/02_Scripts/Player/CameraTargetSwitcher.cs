using Rewired;
using Unity.Cinemachine;
using UnityEngine;

public class CameraTargetSwitcher : MonoBehaviour
{
    [Header("Refs")]
    public CinemachineCamera cineCam;
    public Transform playerTarget;
    public Transform otherTarget;
    public PlayerComponentController playerController;

    [Header("Blend / Zoom")]
    public float blendTime = 0.75f;
    public float playerFOV = 55f;
    public float otherFOV = 35f;

    [Header("Fixed Angles para objetivos")]
    [Tooltip("Ángulo horizontal de la cámara (0 = frente del objetivo, 180 = detrás, etc.)")]
    public float fixedHorizontal = 0f;

    [Tooltip("Ángulo vertical de la cámara (0 = nivelado, 90 = completamente cenital)")]
    public float fixedVertical = 15f;

    [Tooltip("Distancia radial (radio 0 = centro, 1 = máximo)")]
    [Range(0f, 1f)] public float fixedRadial = 1f;

    bool lookingElsewhere;
    Coroutine switchCo;

    private Player player;

    void Awake()
    {
        player = ReInput.players.GetPlayer(0);
        if (!cineCam) cineCam = FindAnyObjectByType<CinemachineCamera>();
    }

    public void LookAtOtherTarget()
    {
        if (lookingElsewhere || otherTarget == null || cineCam == null) return;
        lookingElsewhere = true;
        playerController.DisableAll();

        // 🔹 Primero, fuerza valores fijos de los ejes
        var orbital = cineCam.GetComponent<CinemachineOrbitalFollow>();
        if (orbital)
        {
            orbital.HorizontalAxis.Value = fixedHorizontal;
            orbital.VerticalAxis.Value = fixedVertical;
            orbital.RadialAxis.Value = fixedRadial;
        }

        // 🔹 Actualiza los targets
        cineCam.Follow = otherTarget;
        cineCam.LookAt = otherTarget;

        // 🔹 Inicia el blend de zoom
        StartSwitch(otherTarget, otherFOV);

        // 🔹 (Opcional) desactiva los módulos después de un breve delay
        StartCoroutine(DisableOrbitAfterDelay(0.1f));
    }

    System.Collections.IEnumerator DisableOrbitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        var orbital = cineCam.GetComponent<CinemachineOrbitalFollow>();
        var rotComp = cineCam.GetComponent<CinemachineRotationComposer>();
        if (orbital) orbital.enabled = false;
        if (rotComp) rotComp.enabled = false;
    }

    public void ReturnToPlayer()
    {
        if (!lookingElsewhere || cineCam == null) return;
        lookingElsewhere = false;
        playerController.EnableAll();

        // 🔹 Reactiva control orbital y rotacional
        var orbital = cineCam.GetComponent<CinemachineOrbitalFollow>();
        var rotComp = cineCam.GetComponent<CinemachineRotationComposer>();
        if (orbital) orbital.enabled = true;
        if (rotComp) rotComp.enabled = true;

        cineCam.Follow = playerTarget;
        cineCam.LookAt = playerTarget;

        StartSwitch(playerTarget, playerFOV);
    }

    void StartSwitch(Transform t, float targetFOV)
    {
        if (switchCo != null) StopCoroutine(switchCo);
        switchCo = StartCoroutine(SwitchRoutine(targetFOV));
    }

    System.Collections.IEnumerator SwitchRoutine(float targetFOV)
    {
        player.controllers.maps.SetMapsEnabled(false, "UI");
        float startFOV = cineCam.Lens.FieldOfView;
        float t0 = 0f;

        while (t0 < blendTime)
        {
            t0 += Time.deltaTime;
            float k = Mathf.Clamp01(t0 / blendTime);
            cineCam.Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, k);
            yield return null;
        }

        cineCam.Lens.FieldOfView = targetFOV;
        switchCo = null;
        player.controllers.maps.SetMapsEnabled(true, "UI");
    }
}
