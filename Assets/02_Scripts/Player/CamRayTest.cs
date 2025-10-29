using UnityEngine;

public class CamRayTest : MonoBehaviour
{
    public Transform target;
    public LayerMask mask;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            MoneyController.Instance.AddMoney(10);
    }
}
