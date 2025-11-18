using UnityEngine;

/// <summary>
/// Example: Variable jump height based on touch hold duration.
/// Hold longer = jump higher (like Super Mario Bros or Celeste).
///
/// Integration Pattern: Event-Based
/// Use this pattern when you need discrete actions triggered on touch press/release.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerJumpController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MobileInputController inputController;

    [Header("Jump Settings")]
    [SerializeField] private float minJumpForce = 5f;
    [SerializeField] private float maxJumpForce = 15f;
    [SerializeField] private float gravity = -20f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isJumping;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // We'll handle gravity manually for variable jump
    }

    private void OnEnable()
    {
        // Subscribe to touch events
        if (inputController != null)
        {
            inputController.OnTouchStartedEventHandler += OnJumpStarted;
            inputController.OnTouchReleasedEventHandler += OnJumpReleased;
        }
    }

    private void OnDisable()
    {
        // CRITICAL: Unsubscribe to prevent memory leaks
        if (inputController != null)
        {
            inputController.OnTouchStartedEventHandler -= OnJumpStarted;
            inputController.OnTouchReleasedEventHandler -= OnJumpReleased;
        }
    }

    private void FixedUpdate()
    {
        // Check if grounded
        isGrounded = Physics.CheckSphere(
            transform.position,
            groundCheckRadius,
            groundLayer
        );

        // Apply custom gravity
        if (!isGrounded && rb.velocity.y > 0 && !isJumping)
        {
            // Falling - apply gravity
            rb.velocity += Vector3.up * gravity * Time.fixedDeltaTime;
        }
    }

    /// <summary>
    /// Called when touch starts - Begin jump immediately.
    /// </summary>
    private void OnJumpStarted(float normalizedTime)
    {
        // Only jump if grounded
        if (!isGrounded) return;

        // Apply minimum jump force immediately
        rb.velocity = Vector3.up * minJumpForce;
        isJumping = true;

        Debug.Log($"Jump started! Initial force: {minJumpForce}");
    }

    /// <summary>
    /// Called when touch released - Apply final jump force based on hold time.
    /// </summary>
    private void OnJumpReleased(float normalizedHoldTime)
    {
        if (!isJumping) return;

        // Calculate final jump force based on hold duration
        float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, normalizedHoldTime);

        // Apply additional upward force for variable jump height
        float additionalForce = jumpForce - minJumpForce;
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);

        // Stop adding upward velocity
        isJumping = false;

        Debug.Log($"Jump released! Final force: {jumpForce} (hold: {normalizedHoldTime:F2})");
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize ground check radius
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
    }
}
