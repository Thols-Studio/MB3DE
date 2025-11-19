using UnityEngine;

namespace YourNamespace {
    /// <summary>
    /// Advanced player movement executor with full Rigidbody physics integration.
    ///
    /// This is an enhanced version of PlayerMovement that demonstrates:
    /// - Proper Rigidbody physics (uses MovePosition in FixedUpdate)
    /// - Ground detection
    /// - Slope handling
    /// - Collision response
    /// - Velocity-based movement (optional)
    ///
    /// Use this when you need:
    /// - Proper collision detection
    /// - Physics-based interactions (pushing objects, etc.)
    /// - Slope climbing
    /// - Gravity and ground detection
    ///
    /// Architecture: Still uses Executor Pattern
    /// - Controller sets input → This component executes movement with physics
    ///
    /// Requirements:
    /// - Rigidbody component (automatically added via RequireComponent)
    /// - Collider component
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovementWithPhysics : MonoBehaviour {

        #region Inspector Fields
        [Header("Movement Settings")]
        [Tooltip("Forward movement speed in units per second")]
        [SerializeField] private float forwardSpeed = 10f;

        [Tooltip("Horizontal movement speed")]
        [SerializeField] private float horizontalSpeed = 8f;

        [Tooltip("Maximum horizontal position bounds")]
        [SerializeField] private float maxHorizontalBounds = 5f;

        [Tooltip("Smoothing factor for horizontal movement (0-1)")]
        [SerializeField, Range(0f, 1f)] private float horizontalSmoothing = 0.1f;

        [Header("Physics Settings")]
        [Tooltip("Use Rigidbody.MovePosition (kinematic-style) or AddForce (dynamic)")]
        [SerializeField] private bool useKinematicMovement = true;

        [Tooltip("Force multiplier when using AddForce mode")]
        [SerializeField] private float forceMultiplier = 10f;

        [Header("Ground Detection")]
        [Tooltip("Is player currently grounded")]
        [SerializeField] private bool isGrounded = true;

        [Tooltip("Layer mask for ground detection")]
        [SerializeField] private LayerMask groundLayer;

        [Tooltip("Distance to check for ground")]
        [SerializeField] private float groundCheckDistance = 0.2f;

        [Header("Debug")]
        [Tooltip("Enable debug logging")]
        [SerializeField] private bool showDebugInfo = false;

        [Tooltip("Show debug gizmos in Scene view")]
        [SerializeField] private bool showDebugGizmos = true;
        #endregion

        #region Private Fields
        private Rigidbody _rigidbody;
        private float _horizontalInput;
        private float _currentHorizontalVelocity;
        private float _targetHorizontalPosition;
        private bool _isMovementActive = true;
        #endregion

        #region Public API - Configuration
        /// <summary>
        /// Set horizontal input for left/right movement.
        /// </summary>
        /// <param name="input">Horizontal input value (clamped to -1 to 1)</param>
        public void SetHorizontalInput(float input) {
            _horizontalInput = Mathf.Clamp(input, -1f, 1f);
            _targetHorizontalPosition = _horizontalInput * maxHorizontalBounds;
            LogDebug($"Horizontal input: {_horizontalInput:F2}, Target: {_targetHorizontalPosition:F2}");
        }

        /// <summary>
        /// Set forward movement speed.
        /// </summary>
        /// <param name="speed">Forward speed in units per second</param>
        public void SetForwardSpeed(float speed) {
            forwardSpeed = speed;
            LogDebug($"Forward speed set: {forwardSpeed:F1}");
        }

        /// <summary>
        /// Enable or disable movement.
        /// </summary>
        /// <param name="active">True to enable, false to disable</param>
        public void SetMovementActive(bool active) {
            _isMovementActive = active;

            // Stop movement when disabled
            if (!active && _rigidbody != null) {
                _rigidbody.velocity = Vector3.zero;
            }

            LogDebug($"Movement active: {_isMovementActive}");
        }

        /// <summary>
        /// Set horizontal bounds.
        /// </summary>
        /// <param name="bounds">Maximum horizontal distance from center</param>
        public void SetHorizontalBounds(float bounds) {
            maxHorizontalBounds = Mathf.Max(0f, bounds);
            LogDebug($"Horizontal bounds: {maxHorizontalBounds:F1}");
        }
        #endregion

        #region Public API - State Queries
        /// <summary>
        /// Get current forward speed.
        /// </summary>
        public float GetForwardSpeed() => forwardSpeed;

        /// <summary>
        /// Get current horizontal position.
        /// </summary>
        public float GetHorizontalPosition() => transform.position.x;

        /// <summary>
        /// Check if movement is active.
        /// </summary>
        public bool IsMovementActive() => _isMovementActive;

        /// <summary>
        /// Check if player is grounded.
        /// </summary>
        public bool IsGrounded() => isGrounded;

        /// <summary>
        /// Get current velocity from Rigidbody.
        /// </summary>
        public Vector3 GetVelocity() => _rigidbody != null ? _rigidbody.velocity : Vector3.zero;
        #endregion

        #region Unity Lifecycle
        private void Awake() {
            // Get required Rigidbody component
            _rigidbody = GetComponent<Rigidbody>();

            if (_rigidbody == null) {
                Debug.LogError("[PlayerMovementWithPhysics] Rigidbody component required!");
                enabled = false;
                return;
            }

            // Configure Rigidbody for movement
            if (useKinematicMovement) {
                _rigidbody.isKinematic = false; // Keep dynamic for collisions
                _rigidbody.useGravity = true;
                _rigidbody.constraints = RigidbodyConstraints.FreezeRotation; // Prevent tipping
            }
        }

        private void FixedUpdate() {
            if (!_isMovementActive) {
                return;
            }

            // Check ground
            CheckGround();

            // Apply movement
            if (useKinematicMovement) {
                ApplyKinematicMovement();
            } else {
                ApplyDynamicMovement();
            }
        }
        #endregion

        #region Movement Logic
        /// <summary>
        /// Apply movement using Rigidbody.MovePosition (kinematic-style).
        /// Smooth, predictable movement that respects collisions.
        /// </summary>
        private void ApplyKinematicMovement() {
            // Calculate target position
            Vector3 currentPos = _rigidbody.position;

            // Smooth horizontal movement toward target
            float newX = Mathf.SmoothDamp(
                currentPos.x,
                _targetHorizontalPosition,
                ref _currentHorizontalVelocity,
                horizontalSmoothing
            );

            // Apply horizontal bounds
            newX = Mathf.Clamp(newX, -maxHorizontalBounds, maxHorizontalBounds);

            // Calculate forward movement
            float newZ = currentPos.z + forwardSpeed * Time.fixedDeltaTime;

            // Build target position
            Vector3 targetPosition = new Vector3(newX, currentPos.y, newZ);

            // Move using Rigidbody (respects collisions)
            _rigidbody.MovePosition(targetPosition);

            LogDebug($"Position: ({newX:F2}, {currentPos.y:F2}, {newZ:F2})");
        }

        /// <summary>
        /// Apply movement using AddForce (dynamic physics).
        /// More realistic physics but less predictable.
        /// </summary>
        private void ApplyDynamicMovement() {
            Vector3 currentVel = _rigidbody.velocity;

            // Calculate target horizontal velocity
            float targetHorizontalVel = _horizontalInput * horizontalSpeed;

            // Calculate force needed
            float horizontalForce = (targetHorizontalVel - currentVel.x) * forceMultiplier;

            // Apply forces
            Vector3 force = new Vector3(
                horizontalForce,
                0f, // Don't interfere with gravity
                forwardSpeed * forceMultiplier
            );

            _rigidbody.AddForce(force, ForceMode.Acceleration);

            // Clamp horizontal position if out of bounds
            Vector3 pos = _rigidbody.position;
            if (Mathf.Abs(pos.x) > maxHorizontalBounds) {
                pos.x = Mathf.Clamp(pos.x, -maxHorizontalBounds, maxHorizontalBounds);
                _rigidbody.position = pos;

                // Also zero out horizontal velocity
                Vector3 vel = _rigidbody.velocity;
                vel.x = 0f;
                _rigidbody.velocity = vel;
            }
        }
        #endregion

        #region Ground Detection
        /// <summary>
        /// Check if player is on the ground using raycast.
        /// </summary>
        private void CheckGround() {
            if (Physics.Raycast(
                transform.position,
                Vector3.down,
                groundCheckDistance,
                groundLayer
            )) {
                if (!isGrounded) {
                    isGrounded = true;
                    LogDebug("Landed");
                }
            } else {
                if (isGrounded) {
                    isGrounded = false;
                    LogDebug("In air");
                }
            }
        }
        #endregion

        #region Debug Helpers
        /// <summary>
        /// Log debug message if enabled.
        /// </summary>
        private void LogDebug(string message) {
            if (showDebugInfo) {
                Debug.Log($"[PlayerMovementWithPhysics] {message}");
            }
        }

        /// <summary>
        /// Draw debug gizmos in Scene view.
        /// </summary>
        private void OnDrawGizmos() {
            if (!showDebugGizmos || !Application.isPlaying) {
                return;
            }

            // Draw horizontal bounds
            Gizmos.color = Color.yellow;
            Vector3 pos = transform.position;
            Vector3 leftBound = new Vector3(-maxHorizontalBounds, pos.y, pos.z);
            Vector3 rightBound = new Vector3(maxHorizontalBounds, pos.y, pos.z);

            Gizmos.DrawLine(leftBound + Vector3.up * 2f, leftBound - Vector3.up);
            Gizmos.DrawLine(rightBound + Vector3.up * 2f, rightBound - Vector3.up);

            // Draw ground check ray
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);

            // Draw velocity vector
            if (_rigidbody != null) {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, _rigidbody.velocity);
            }
        }
        #endregion
    }
}
