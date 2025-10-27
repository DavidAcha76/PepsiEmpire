using UnityEngine;

public class CamRayTest : MonoBehaviour
{
    public Transform target;
    public LayerMask mask;
    void Update()
    {
        if (!Camera.main) return;
        Vector3 camPos = Camera.main.transform.position;
        if (Physics.Linecast(camPos, target.position, out RaycastHit hit, mask))
            Debug.Log($"[CamTest] hit {hit.collider.name}");
    }
}
