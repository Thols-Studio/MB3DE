using UnityEngine;

namespace YourNamespace {
    /// <summary>
    /// Complete integrated example showing Touch → MobileInputController → PlayerController → PlayerMovement flow.
    ///
    /// This demonstrates the full architecture:
    /// 1. User touches screen
    /// 2. MobileInputController detects and tracks touch
    /// 3. PlayerController receives touch events
    /// 4. PlayerController converts touch to movement input
    /// 5. PlayerMovement executes the actual movement
    ///
    /// Setup Requirements:
    /// - GameObject with MobileInputController (from mobile-input-controller skill)
    /// - GameObject with PlayerMovement (from player-movement-executor skill)
    /// - GameObject with this TouchPlayerController script
    /// - All three components can be on same GameObject or separate
    ///
    /// Usage:
    /// 1. Touch and hold anywhere on screen
    /// 2. Player moves forward automatically
    /// 3. Touch left/right side of screen to move horizontally
    /// 4. Release touch to slow down (optional)
    /// </summary>
    public class TouchPlayerController : MonoBehaviour {

        #region Inspector Fields
        [Header("Component References")]
        [Tooltip("Reference to MobileInputController (handles touch detection)")]
        [SerializeField] private MobileInputController mobileInputController;

        [Tooltip("Reference to PlayerMovement (handles movement execution)")]
        [SerializeField] private PlayerMovement playerMovement;

        [Header("Movement Settings")]
        [Tooltip("Normal forward speed when moving")]
        [SerializeField] private float normalSpeed = 10f;

        [Tooltip("Boosted speed when holding touch longer")]
        [SerializeField] private float boostedSpeed = 20f;

        [Tooltip("How sensitive horizontal movement is to touch position")]
        [SerializeField] private float horizontalSensitivity = 1.0f;

        [Header("Game State")]
        [Tooltip("Should player stop when touch is released?")]
        [SerializeField] private bool stopOnRelease = false;

        [Tooltip("Enable speed boost based on hold duration")]
        [SerializeField] private bool enableSpeedBoost = true;

        [Header("Debug")]
        [Tooltip("Show debug information in console")]
        [SerializeField] private bool showDebugInfo = false;
        #endregion

        #region Unity Lifecycle
        private void Awake() {
            // Auto-find components if not assigned
            if (mobileInputController == null) {
                mobileInputController = GetComponent<MobileInputController>();
                if (mobileInputController == null) {
                    Debug.LogError("[TouchPlayerController] MobileInputController not found! Please assign or add to GameObject.");
                }
            }

            if (playerMovement == null) {
                playerMovement = GetComponent<PlayerMovement>();
                if (playerMovement == null) {
                    Debug.LogError("[TouchPlayerController] PlayerMovement not found! Please assign or add to GameObject.");
                }
            }
        }

        private void OnEnable() {
            // Subscribe to touch events from MobileInputController
            if (mobileInputController != null) {
                mobileInputController.OnTouchStartedEventHandler += OnTouchStarted;
                mobileInputController.OnTouchReleasedEventHandler += OnTouchReleased;
            }
        }

        private void OnDisable() {
            // IMPORTANT: Always unsubscribe to prevent memory leaks
            if (mobileInputController != null) {
                mobileInputController.OnTouchStartedEventHandler -= OnTouchStarted;
                mobileInputController.OnTouchReleasedEventHandler -= OnTouchReleased;
            }
        }

        private void Update() {
            // Continuously update movement based on touch state
            HandleTouchInput();
        }
        #endregion

        #region Touch Event Handlers
        /// <summary>
        /// Called when user touches the screen (from MobileInputController event).
        /// </summary>
        /// <param name="normalizedHoldTime">Always 0 when touch starts</param>
        private void OnTouchStarted(float normalizedHoldTime) {
            LogDebug("Touch started - enabling movement");

            // Enable movement when touch starts
            playerMovement.SetMovementActive(true);
            playerMovement.SetForwardSpeed(normalSpeed);
        }

        /// <summary>
        /// Called when user releases touch (from MobileInputController event).
        /// </summary>
        /// <param name="normalizedHoldTime">0-1 value of how long touch was held</param>
        private void OnTouchReleased(float normalizedHoldTime) {
            LogDebug($"Touch released - hold time: {normalizedHoldTime:F2}");

            if (stopOnRelease) {
                // Stop movement entirely
                playerMovement.SetMovementActive(false);
                LogDebug("Movement stopped");
            } else {
                // Return to normal speed but keep moving
                playerMovement.SetForwardSpeed(normalSpeed);

                // Reset horizontal input to center
                playerMovement.SetHorizontalInput(0f);
            }
        }
        #endregion

        #region Input Handling
        /// <summary>
        /// Handle continuous touch input polling.
        /// Called every frame in Update().
        /// </summary>
        private void HandleTouchInput() {
            if (mobileInputController == null || playerMovement == null) {
                return;
            }

            // Check if touch is currently held
            if (mobileInputController.IsTouchHeld) {
                // Get touch position
                Vector2 touchPos = mobileInputController.GetCurrentTouchPosition();

                // Convert screen position to horizontal input (-1 to 1)
                float horizontalInput = CalculateHorizontalInput(touchPos);

                // Apply to movement
                playerMovement.SetHorizontalInput(horizontalInput);

                // Optional: Speed boost based on hold duration
                if (enableSpeedBoost) {
                    float holdTime = mobileInputController.GetCurrentNormalizedHoldTime();
                    float speed = Mathf.Lerp(normalSpeed, boostedSpeed, holdTime);
                    playerMovement.SetForwardSpeed(speed);
                }

                LogDebug($"Touch held - Input: {horizontalInput:F2}, Speed: {playerMovement.GetForwardSpeed():F1}");
            }
        }

        /// <summary>
        /// Convert screen touch position to horizontal movement input.
        /// </summary>
        /// <param name="touchPosition">Touch position in screen coordinates</param>
        /// <returns>Horizontal input from -1 (left) to 1 (right)</returns>
        private float CalculateHorizontalInput(Vector2 touchPosition) {
            // Get screen center
            float screenCenter = Screen.width / 2f;

            // Calculate distance from center
            float distanceFromCenter = touchPosition.x - screenCenter;

            // Normalize to -1 to 1 range
            float normalizedInput = distanceFromCenter / screenCenter;

            // Apply sensitivity
            normalizedInput *= horizontalSensitivity;

            // Clamp to -1 to 1
            return Mathf.Clamp(normalizedInput, -1f, 1f);
        }
        #endregion

        #region Public API - Game State Management
        /// <summary>
        /// Call when player dies. Stops all movement.
        /// </summary>
        public void OnPlayerDeath() {
            if (playerMovement != null) {
                playerMovement.SetMovementActive(false);
            }
            LogDebug("Player died - movement stopped");
        }

        /// <summary>
        /// Call when player respawns. Resets movement.
        /// </summary>
        public void OnPlayerRespawn() {
            if (playerMovement != null) {
                playerMovement.SetMovementActive(true);
                playerMovement.SetForwardSpeed(normalSpeed);
            }
            LogDebug("Player respawned - movement reset");
        }

        /// <summary>
        /// Set the forward speed dynamically (e.g., for power-ups).
        /// </summary>
        /// <param name="speed">New forward speed</param>
        public void SetSpeed(float speed) {
            normalSpeed = speed;
            if (playerMovement != null) {
                playerMovement.SetForwardSpeed(speed);
            }
        }

        /// <summary>
        /// Set horizontal bounds dynamically (e.g., narrow tunnel section).
        /// </summary>
        /// <param name="bounds">New horizontal bounds</param>
        public void SetHorizontalBounds(float bounds) {
            if (playerMovement != null) {
                playerMovement.SetHorizontalBounds(bounds);
            }
        }
        #endregion

        #region Debug Helpers
        /// <summary>
        /// Log debug message if debug logging is enabled.
        /// </summary>
        /// <param name="message">Message to log</param>
        private void LogDebug(string message) {
            if (showDebugInfo) {
                Debug.Log($"[TouchPlayerController] {message}");
            }
        }
        #endregion
    }
}
