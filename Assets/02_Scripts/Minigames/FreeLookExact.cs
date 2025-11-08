using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class FreeLookExact : MonoBehaviour
{
    [Header("Referencias")]
    public CinemachineCamera freeLookCam;
    public Transform playerFollow;
    public Transform playerLookAt;

    [Header("Ajustes")]
    public float blendDuration = 0.75f;  // duración de transición
    public bool smoothTransition = true;

    private Transform originalFollow;
    private Transform originalLookAt;
    private Coroutine moveRoutine;

    // --------------------------------------------------------------------
    public void FocusOnExact(Transform lookAtTarget, Transform cameraSpot)
    {
        if (!freeLookCam || !lookAtTarget || !cameraSpot)
        {
            Debug.LogWarning("⚠️ [FreeLookExact] Faltan referencias.");
            return;
        }

        // Guardar estado original
        originalFollow = freeLookCam.Follow;
        originalLookAt = freeLookCam.LookAt;

        // Desactivar el control de Cinemachine
        freeLookCam.enabled = false;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveExactRoutine(cameraSpot.position, lookAtTarget.position));
    }

    // --------------------------------------------------------------------
    public void ReturnToPlayer()
    {
        if (!freeLookCam || !playerFollow || !playerLookAt)
        {
            Debug.LogWarning("⚠️ [FreeLookExact] Faltan referencias de jugador.");
            return;
        }

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(ReturnRoutine());
    }

    // --------------------------------------------------------------------
    private IEnumerator MoveExactRoutine(Vector3 targetPos, Vector3 lookAt)
    {
        Transform cam = freeLookCam.transform;

        Vector3 startPos = cam.position;
        Quaternion startRot = cam.rotation;
        Quaternion targetRot = Quaternion.LookRotation(lookAt - targetPos, Vector3.up);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / blendDuration;
            float k = smoothTransition ? Mathf.SmoothStep(0, 1, t) : t;
            cam.position = Vector3.Lerp(startPos, targetPos, k);
            cam.rotation = Quaternion.Slerp(startRot, targetRot, k);
            yield return null;
        }

        cam.position = targetPos;
        cam.rotation = targetRot;
        Debug.Log("🎯 [FreeLookExact] Cámara movida a posición exacta y mirando objetivo.");

        moveRoutine = null;
    }

    // --------------------------------------------------------------------
    private IEnumerator ReturnRoutine()
    {
        Transform cam = freeLookCam.transform;
        Vector3 targetPos = playerFollow.position + Vector3.up * 1.5f;
        Quaternion targetRot = Quaternion.LookRotation(playerLookAt.position - targetPos, Vector3.up);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / blendDuration;
            float k = smoothTransition ? Mathf.SmoothStep(0, 1, t) : t;
            cam.position = Vector3.Lerp(cam.position, targetPos, k);
            cam.rotation = Quaternion.Slerp(cam.rotation, targetRot, k);
            yield return null;
        }

        // Rehabilitar Cinemachine
        freeLookCam.enabled = true;
        freeLookCam.Follow = playerFollow;
        freeLookCam.LookAt = playerLookAt;
        Debug.Log("🎥 [FreeLookExact] Cámara restaurada al jugador.");

        moveRoutine = null;
    }
}
