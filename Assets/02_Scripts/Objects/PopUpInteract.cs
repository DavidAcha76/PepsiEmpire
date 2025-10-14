using Unity.VisualScripting;
using UnityEngine;

public class PopUpInteract : MonoBehaviour
{
    public GameObject prefabUI;
    public float arribaquetanto = 2f;

    private GameObject currentUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Vector3 vector3 = transform.position + Vector3.up * arribaquetanto;
            currentUI = Instantiate(prefabUI, vector3, Quaternion.identity);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(currentUI);
        }
    }
}
