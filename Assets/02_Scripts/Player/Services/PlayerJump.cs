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

    [Header("Control en aire")]
    public float airControlStrength = 3f;
    public float maxAirSpeed = 4f;

    private bool isGrounded = true;
    private bool jumpQueued = false;
    private float coyoteTime = 0.15f; // tolerancia para saltar poco después de caer
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

        // Si se presiona salto, guarda la intención
        if (player.GetButtonDown("Jump"))
            jumpQueued = true;
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        // Permite salto dentro del margen de coyote time
        bool canJump = jumpQueued && (isGrounded || Time.time - lastGroundedTime <= coyoteTime);

        if (canJump)
        {
            jumpQueued = false;
            TryJump();
        }

        if (!isGrounded)
            AirControl();
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDistance,
            groundMask
        );

        animator.SetBool("IsGrounded", isGrounded);

        if (!isGrounded)
            Falling();
    }

    public void TryJump()
    {
        animator.SetBool("IsJumping", true);

        // Reinicia velocidad vertical para consistencia
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float moveX = player.GetAxis("Move Horizontal");
        float moveZ = player.GetAxis("Move Vertical");
        bool isMoving = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f;

        Vector3 jumpVel = Vector3.up * jumpForce;
        if (isMoving)
            jumpVel += transform.forward * forwardJumpForce;

        // Asigna directamente la nueva velocidad (nuevo sistema Unity 6000)
        rb.linearVelocity = jumpVel;

        isGrounded = false; // evita saltos consecutivos
    }

    private void Falling()
    {
        animator.SetBool("IsJumping", false);
    }

    private void AirControl()
    {
        float moveX = player.GetAxis("Move Horizontal");
        float moveZ = player.GetAxis("Move Vertical");

        Vector3 inputDir = new Vector3(moveX, 0, moveZ).normalized;
        if (inputDir.sqrMagnitude < 0.01f)
            return;

        Vector3 desiredVelocity = transform.TransformDirection(inputDir) * maxAirSpeed;
        Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        Vector3 velocityChange = (desiredVelocity - currentVelocity) * airControlStrength * Time.fixedDeltaTime;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
}
