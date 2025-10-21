using Unity.Cinemachine;
using UnityEngine;

public class CameraTargetSwitcher : MonoBehaviour
{
    [Header("Refs")]
    public CinemachineCamera cineCam;    
    public Transform playerTarget;        // TPTarget del jugador
    public Transform otherTarget;         // punto u objeto a enfocar

    [Header("Blend / Zoom")]
    [Tooltip("Tiempo total de transición de foco/zoom.")]
    public float blendTime = 0.75f;

    [Tooltip("FOV cuando estás con el jugador (más abierto = más lejos).")]
    public float playerFOV = 55f;

    [Tooltip("FOV cuando miras el otro objetivo (más cerrado = más cerca).")]
    public float otherFOV = 35f;

    bool lookingElsewhere;
    Coroutine switchCo;

    void Awake()
    {
        if (!cineCam) cineCam = FindAnyObjectByType<CinemachineCamera>();
    }

    public void LookAtOtherTarget()
    {
        if (lookingElsewhere || otherTarget == null || cineCam == null) return;
        lookingElsewhere = true;
        StartSwitch(otherTarget, otherFOV);
    }

    public void ReturnToPlayer()
    {
        if (!lookingElsewhere || cineCam == null) return;
        lookingElsewhere = false;
        StartSwitch(playerTarget, playerFOV);
    }

    void StartSwitch(Transform newTarget, float targetFOV)
    {
        if (switchCo != null) StopCoroutine(switchCo);
        switchCo = StartCoroutine(SwitchRoutine(newTarget, targetFOV));
    }

    System.Collections.IEnumerator SwitchRoutine(Transform t, float targetFOV)
    {
        // Cambiamos los targets de la cámara
        cineCam.Follow = t;
        cineCam.LookAt = t;

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
    }
}
