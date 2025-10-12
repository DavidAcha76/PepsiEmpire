using Rewired;
using UnityEngine;

public class PlayerJump : MonoBehaviour, IJump
{
    private Rigidbody rb;
    private Animator animator;

    [Header("Configuración de salto")]
    public float jumpForce = 7f;
    public LayerMask groundMask;
    public float groundCheckDistance = 0.5f;
    public float groundResetDelay = 0.15f;


    private bool isGrounded = true;
    private bool isJumping = false;
    private bool jumpQueued = false;
    public bool IsJumping => isJumping;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Comprobar si está tocando el suelo
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance, groundMask);

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsJumping", isJumping);
    }

    public void TryJump()
    {
        if (isGrounded && !isJumping)
        {
            jumpQueued = true;
            animator.SetTrigger("Jump");
        }
    }

    public void StartJump()
    {
        if (jumpQueued)
        {
            isJumping = true;
            jumpQueued = false;
            animator.applyRootMotion = false;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    private void EndJump()
    {
        isJumping = false;
        animator.applyRootMotion = true;
        animator.SetBool("IsGrounded", isGrounded); 
    }

    private void FixedUpdate()
    {
        if (isJumping && isGrounded)
        {
            EndJump();
        }
    }
}
