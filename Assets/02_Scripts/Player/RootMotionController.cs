using Rewired;
using UnityEngine;

public class RootMotionController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    [Header("Escala del Root Motion")]
    [Range(0.5f, 3f)] public float rootMotionMultiplier = 1.5f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        animator.applyRootMotion = true;
        rb.freezeRotation = true;
    }

    void OnAnimatorMove()
    {
        if (animator)
        {
            Vector3 delta = animator.deltaPosition * rootMotionMultiplier;
            delta.y = 0; 

            rb.MovePosition(rb.position + delta);
            rb.MoveRotation(animator.rootRotation);
        }
    }
}
