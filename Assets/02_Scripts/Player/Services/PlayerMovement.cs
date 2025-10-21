using UnityEngine;

public class PlayerMovement : MonoBehaviour, IMovement
{
    private Animator animator;
    private GameObject cameraPlayer;
    public float rotationSpeed = 10f;

    [Header("Umbrales de locomoción")]
    public float deadzoneEnter = 0.22f;
    public float deadzoneExit = 0.12f;

    [Header("Suavizados (damp)")]
    [Tooltip("Qué tan rápido responde el valor Speed al moverse.")]
    public float dampMoving = 0.1f;
    [Tooltip("Qué tan suave vuelve a Idle al frenar.")]
    public float dampStopping = 0.25f;   

    bool isMoving;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        cameraPlayer = GameObject.FindGameObjectWithTag("MainCamera");
    }

    public void Move(Vector2 input, bool isRunning)
    {
        Vector3 f = cameraPlayer.transform.forward; f.y = 0; f.Normalize();
        Vector3 r = cameraPlayer.transform.right; r.y = 0; r.Normalize();
        Vector3 dir = (f * input.y + r * input.x);
        float mag = dir.magnitude;
        Vector3 direction = (mag > 0f) ? dir / mag : Vector3.zero;
        if (isMoving)
        {
            if (mag < deadzoneExit) isMoving = false;
        }
        else
        {
            if (mag > deadzoneEnter) isMoving = true;
        }

        float targetSpeed = 0f;
        if (isMoving)
        {
            float moveAmount = Mathf.Clamp01(mag);
            targetSpeed = isRunning ? moveAmount * 2f : moveAmount;
            animator.SetFloat("Speed", targetSpeed, dampMoving, Time.deltaTime);
        }
        else
        {
            float currentSpeed = animator.GetFloat("Speed");
            if (currentSpeed > 0.01f)
            {
                animator.SetFloat("Speed", 0f, dampStopping, Time.deltaTime);
            }
            else
            {
                animator.SetFloat("Speed", 0f); 
            }
        }

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    public void Stop(bool hard = true)
    {
        isMoving = false;
        if (hard)
            animator.SetFloat("Speed", 0f, 0f, 0f); 
        else
            animator.SetFloat("Speed", 0f, dampStopping, Time.deltaTime);
    }
}
