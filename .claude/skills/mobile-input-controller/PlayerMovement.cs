using UnityEngine;

namespace com.TholsStudio.MusicalBall3DExtended {
    /// <summary>
    /// Movement executor - provides public API for PlayerController to call.
    /// Handles actual Rigidbody physics and movement execution.
    /// Does not subscribe to input directly - called by PlayerController.
    /// </summary>
    public class PlayerMovement : MonoBehaviour {
        [Header("Movement Settings")]
        [SerializeField] private float forwardSpeed = 10f;
        [SerializeField] private float horizontalSpeed = 0.5f;


        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        // Internal state
        private float _horizontalInput;
        private bool _isMovementActive = true;
        private float _current, target;

        // Future: Add Rigidbody reference
        // private Rigidbody _rigidbody;

        // === PUBLIC API (Called by PlayerController) ===

        /// <summary>
        /// Set horizontal input for left/right movement
        /// </summary>
        /// <param name="input">Horizontal input value (clamped -1 to 1)</param>
        public void SetHorizontalInput(float input) {
            _horizontalInput = Mathf.Clamp(input, -1f, 1f);
            LogDebug($"Horizontal input set: {_horizontalInput}");
        }

        /// <summary>
        /// Set forward movement speed
        /// </summary>
        /// <param name="speed">Forward speed value</param>
        public void SetForwardSpeed(float speed) {
            forwardSpeed = speed;
            LogDebug($"Forward speed set: {forwardSpeed}");
        }

        /// <summary>
        /// Enable or disable movement
        /// </summary>
        /// <param name="active">True to enable movement, false to disable</param>
        public void SetMovementActive(bool active) {
            _isMovementActive = active;
            LogDebug($"Movement active: {_isMovementActive}");
        }

        // === INTERNAL LOGIC ===

        void Update() {
            // Future: Apply movement using Rigidbody in FixedUpdate
            // This will be implemented when integrating with ground detection and physics

            if (_isMovementActive) {
                _current = Mathf.MoveTowards(_current, target, horizontalSpeed);
            }
        }

        /// <summary>
        /// Log debug message if debug logging is enabled
        /// </summary>
        private void LogDebug(string message) {
            if (showDebugInfo) {
                Debug.Log($"[PlayerMovement] {message}");
            }
        }
    }
}
