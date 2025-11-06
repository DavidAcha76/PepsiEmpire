using UnityEngine;

public class BillboardMarker : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
            transform.LookAt(Camera.main.transform);
    }
}
