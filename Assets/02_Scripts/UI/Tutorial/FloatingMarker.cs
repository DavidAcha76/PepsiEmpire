using UnityEngine;

public class FloatingMarker : MonoBehaviour
{
    public float floatSpeed = 2f;
    public float floatHeight = 0.25f;

    private Vector3 startPos;

    void Start() => startPos = transform.position;

    void Update()
    {
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * floatSpeed) * floatHeight;
    }
}
