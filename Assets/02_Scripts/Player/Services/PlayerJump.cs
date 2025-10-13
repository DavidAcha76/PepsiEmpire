using Rewired;
using UnityEngine;

public class PlayerJump : MonoBehaviour, IJump
{
    private Rigidbody rb;
    private Animator animator;
    private Player player;

    [Header("Configuración de salto")]
    public float jumpForce = 7f;
    public float forwardJumpForce = 4f;
    public LayerMask groundMask;
    public float groundCheckDistance = 0.6f;

    [Header("Movimiento en aire")]
    public float airMoveSpeed = 4f;

    private bool isGrounded = true;
    private bool jumpQueued = false;
    private float coyoteTime = 0.15f;
    private float lastGroundedTime = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        player = ReInput.players.GetPlayer(0);
    }

    private void Update()
    {
        if (isGrounded)
            lastGroundedTime = Time.time;

        if (player.GetButtonDown("Jump"))
            jumpQueued = true;
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        bool canJump = jumpQueued && (isGrounded || Time.time - lastGroundedTime <= coyoteTime);
        if (canJump)
        {
            jumpQueued = false;
            TryJump();
        }

        if (!isGrounded)
            AirMove();
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance, groundMask);
        animator.SetBool("IsGrounded", isGrounded);
        animator.applyRootMotion = isGrounded;

        if (!isGrounded)
            animator.SetBool("IsJumping", false);
    }

    public void TryJump()
    {
        animator.SetBool("IsJumping", true);
        animator.applyRootMotion = false;

        Vector3 jumpVel = Vector3.up * jumpForce + transform.forward * forwardJumpForce;
        rb.position += Vector3.up * 0.05f; // evitar colisión inmediata
        rb.linearVelocity = jumpVel;

        isGrounded = false;
    }

    private void AirMove()
    {
        float moveX = player.GetAxis("Move Horizontal");
        float moveZ = player.GetAxis("Move Vertical");

        Vector3 inputDir = new Vector3(moveX, 0, moveZ).normalized;
        if (inputDir.sqrMagnitude < 0.01f)
            return;

        Vector3 currentVelocity = rb.linearVelocity;

        Vector3 targetVelocity = new Vector3(
            inputDir.x * airMoveSpeed,
            currentVelocity.y, // mantenemos Y
            inputDir.z * airMoveSpeed
        );

        Vector3 newVelocity = Vector3.Lerp(
            new Vector3(currentVelocity.x, 0, currentVelocity.z),
            new Vector3(targetVelocity.x, 0, targetVelocity.z),
            0.1f 
        );

        rb.linearVelocity = new Vector3(newVelocity.x, currentVelocity.y, newVelocity.z);
    }
}
