using UnityEngine;

public class PlayerMovement : MonoBehaviour, IMovement
{
    private Animator animator;
    private GameObject cameraPlayer;
    public float rotationSpeed = 10f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        cameraPlayer = GameObject.FindGameObjectWithTag("MainCamera");
    }

    public void Move(Vector2 input, bool isRunning)
    {
        Vector3 forward = cameraPlayer.transform.forward;
        Vector3 right = cameraPlayer.transform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 direction = (forward * input.y + right * input.x).normalized;

        float moveAmount = Mathf.Clamp01(direction.magnitude);
        float animSpeed = isRunning ? moveAmount * 2f : moveAmount;

        animator.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void Stop(bool hard = true)
    {
        // 1) Corta el BlendTree al instante (sin damp)
        animator.SetFloat("Speed", 0f, 0f, 0f);

    }
}
