using UnityEngine;
using UnityEngine.Rendering;

public class FloatingUI : MonoBehaviour
{
    [Header("Movimiento flotante")]
    public float amplitude = 0.25f;     
    public float frequency = 2f;        

    [Header("Mirar cámara")]
    public bool faceCamera = true;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.localPosition;
    }

    private void Update()
    {
        // Rebote
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);

        if (faceCamera && Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
        }

    }
}
