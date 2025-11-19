using UnityEngine;

namespace YourNamespace {
    /// <summary>
    /// Player movement executor - provides public API for PlayerController to call.
    ///
    /// This component handles the actual movement execution and physics, but does NOT
    /// subscribe to input directly. Instead, it provides a clean API that controllers
    /// can call to set movement parameters.
    ///
    /// Architecture Pattern: Executor Pattern
    /// - Input handling → PlayerController (or InputController)
    /// - Movement execution → PlayerMovement (this class)
    /// - This separation allows easy swapping of input sources (keyboard, touch, AI, replay)
    ///
    /// Features:
    /// - Horizontal left/right movement with smoothing
    /// - Configurable forward speed
    /// - Enable/disable movement control
    /// - Debug logging for testing
    /// - Ready for Rigidbody physics integration
    ///
    /// Usage:
    /// 1. Attach to player GameObject (alongside Rigidbody)
    /// 2. Configure speeds in Inspector
    /// 3. Call SetHorizontalInput() from your controller
    /// 4. Call SetMovementActive() to enable/disable movement
    ///
    /// Example:
    /// <code>
    /// // From PlayerController or InputController
    /// playerMovement.SetHorizontalInput(Input.GetAxis("Horizontal"));
    /// playerMovement.SetForwardSpeed(10f);
    /// playerMovement.SetMovementActive(true);
    /// </code>
    /// </summary>
    public class PlayerMovement : MonoBehaviour {

        #region Inspector Fields
        [Header("Movement Settings")]
        [Tooltip("Forward movement speed in units per second")]
        [SerializeField] private float forwardSpeed = 10f;

        [Tooltip("Horizontal movement speed (smoothing factor)")]
        [SerializeField] private float horizontalSpeed = 0.5f;

        [Tooltip("Maximum horizontal position bounds (left/right limits)")]
        [SerializeField] private float maxHorizontalBounds = 5f;

        [Header("Debug")]
        [Tooltip("Enable debug logging to console")]
        [SerializeField] private bool showDebugInfo = false;
        #endregion

        #region Private Fields
        // Current horizontal input value (-1 to 1)
        private float _horizontalInput;

        // Current horizontal position
        private float _current;

        // Target horizontal position
        private float _target;

        // Is movement currently active
        private bool _isMovementActive = true;

        // Cached Rigidbody reference (for future physics integration)
        private Rigidbody _rigidbody;
        #endregion

        #region Public API - Configuration
        /// <summary>
        /// Set horizontal input for left/right movement.
        /// This is typically called every frame from an input controller.
        /// </summary>
        /// <param name="input">Horizontal input value (will be clamped to -1 to 1)</param>
        public void SetHorizontalInput(float input) {
            _horizontalInput = Mathf.Clamp(input, -1f, 1f);
            _target = _horizontalInput * maxHorizontalBounds;
            LogDebug($"Horizontal input set: {_horizontalInput:F2}, Target position: {_target:F2}");
        }

        /// <summary>
        /// Set the forward movement speed.
        /// Use this to change speed during gameplay (e.g., speed power-ups, slow zones).
        /// </summary>
        /// <param name="speed">Forward speed in units per second</param>
        public void SetForwardSpeed(float speed) {
            forwardSpeed = speed;
            LogDebug($"Forward speed set: {forwardSpeed:F1}");
        }

        /// <summary>
        /// Enable or disable all movement.
        /// Use this for pausing, cutscenes, or when player dies.
        /// </summary>
        /// <param name="active">True to enable movement, false to disable</param>
        public void SetMovementActive(bool active) {
            _isMovementActive = active;
            LogDebug($"Movement active: {_isMovementActive}");
        }

        /// <summary>
        /// Set the maximum horizontal bounds (how far left/right player can move).
        /// Useful for different track widths or level sections.
        /// </summary>
        /// <param name="bounds">Maximum distance from center in units</param>
        public void SetHorizontalBounds(float bounds) {
            maxHorizontalBounds = Mathf.Max(0f, bounds);
            LogDebug($"Horizontal bounds set: {maxHorizontalBounds:F1}");
        }
        #endregion

        #region Public API - State Queries
        /// <summary>
        /// Get current forward speed.
        /// </summary>
        /// <returns>Current forward speed in units per second</returns>
        public float GetForwardSpeed() {
            return forwardSpeed;
        }

        /// <summary>
        /// Get current horizontal position.
        /// </summary>
        /// <returns>Current horizontal position (0 = center)</returns>
        public float GetHorizontalPosition() {
            return _current;
        }

        /// <summary>
        /// Check if movement is currently active.
        /// </summary>
        /// <returns>True if movement is active, false otherwise</returns>
        public bool IsMovementActive() {
            return _isMovementActive;
        }
        #endregion

        #region Unity Lifecycle
        private void Awake() {
            // Cache Rigidbody reference for future physics integration
            _rigidbody = GetComponent<Rigidbody>();

            if (_rigidbody == null) {
                Debug.LogWarning("[PlayerMovement] No Rigidbody found. Add Rigidbody component for physics-based movement.");
            }
        }

        private void Update() {
            if (!_isMovementActive) {
                return;
            }

            // Smooth horizontal movement toward target position
            _current = Mathf.MoveTowards(_current, _target, horizontalSpeed * Time.deltaTime);

            // Apply horizontal position
            // Note: This is transform-based movement for simplicity
            // For physics-based movement, use Rigidbody in FixedUpdate (see below)
            transform.position = new Vector3(_current, transform.position.y, transform.position.z);

            // Apply forward movement
            transform.position += Vector3.forward * forwardSpeed * Time.deltaTime;
        }

        // Future: Physics-based movement
        // private void FixedUpdate() {
        //     if (!_isMovementActive || _rigidbody == null) {
        //         return;
        //     }
        //
        //     // Smooth horizontal movement
        //     _current = Mathf.MoveTowards(_current, _target, horizontalSpeed * Time.fixedDeltaTime);
        //
        //     // Calculate target position
        //     Vector3 targetPosition = new Vector3(
        //         _current,
        //         _rigidbody.position.y,
        //         _rigidbody.position.z + forwardSpeed * Time.fixedDeltaTime
        //     );
        //
        //     // Move using Rigidbody for proper physics
        //     _rigidbody.MovePosition(targetPosition);
        // }
        #endregion

        #region Debug Helpers
        /// <summary>
        /// Log debug message if debug logging is enabled.
        /// </summary>
        /// <param name="message">Message to log</param>
        private void LogDebug(string message) {
            if (showDebugInfo) {
                Debug.Log($"[PlayerMovement] {message}");
            }
        }

        /// <summary>
        /// Draw debug gizmos in Scene view.
        /// Shows horizontal bounds and current position.
        /// </summary>
        private void OnDrawGizmos() {
            if (!Application.isPlaying) {
                return;
            }

            // Draw horizontal bounds
            Gizmos.color = Color.yellow;
            Vector3 leftBound = transform.position + Vector3.left * maxHorizontalBounds;
            Vector3 rightBound = transform.position + Vector3.right * maxHorizontalBounds;
            Gizmos.DrawLine(leftBound + Vector3.up, leftBound - Vector3.up);
            Gizmos.DrawLine(rightBound + Vector3.up, rightBound - Vector3.up);

            // Draw current position indicator
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
        #endregion
    }
}
