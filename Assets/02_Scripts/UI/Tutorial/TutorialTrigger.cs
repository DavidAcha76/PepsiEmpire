using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    private bool done = false;

    private void OnTriggerEnter(Collider other)
    {
        if (done) return;

        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<TutorialManager>().CompleteStep();
            done = true;
        }
    }
}
