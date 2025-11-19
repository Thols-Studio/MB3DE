using UnityEngine;

namespace YourNamespace {
    /// <summary>
    /// Example PlayerController that uses the PlayerMovement executor component.
    ///
    /// This demonstrates the Executor Pattern:
    /// - PlayerController handles input and game logic
    /// - PlayerMovement handles movement execution
    ///
    /// This separation allows:
    /// - Easy swapping of input sources (keyboard, gamepad, touch, AI)
    /// - Clean testing (can test movement without input)
    /// - Modular design (can reuse PlayerMovement in different contexts)
    ///
    /// Usage:
    /// 1. Attach both PlayerController and PlayerMovement to player GameObject
    /// 2. Assign PlayerMovement reference in Inspector
    /// 3. Configure input keys if needed
    /// 4. Press Play and use arrow keys or A/D to move
    /// </summary>
    public class PlayerController : MonoBehaviour {

        #region Inspector Fields
        [Header("References")]
        [Tooltip("Reference to the PlayerMovement executor component")]
        [SerializeField] private PlayerMovement playerMovement;

        [Header("Input Settings")]
        [Tooltip("Use horizontal axis input (arrow keys, A/D)")]
        [SerializeField] private bool useHorizontalAxis = true;

        [Tooltip("Key for moving left")]
        [SerializeField] private KeyCode leftKey = KeyCode.A;

        [Tooltip("Key for moving right")]
        [SerializeField] private KeyCode rightKey = KeyCode.D;

        [Header("Game State")]
        [Tooltip("Is player currently alive and controllable")]
        [SerializeField] private bool isAlive = true;

        [Tooltip("Is game currently paused")]
        [SerializeField] private bool isPaused = false;

        [Header("Debug")]
        [Tooltip("Show debug information")]
        [SerializeField] private bool showDebugInfo = false;
        #endregion

        #region Unity Lifecycle
        private void Awake() {
            // Find PlayerMovement if not assigned
            if (playerMovement == null) {
                playerMovement = GetComponent<PlayerMovement>();

                if (playerMovement == null) {
                    Debug.LogError("[PlayerController] PlayerMovement component not found! Please assign or add to GameObject.");
                }
            }
        }

        private void Start() {
            // Initialize movement state
            if (playerMovement != null) {
                playerMovement.SetMovementActive(isAlive && !isPaused);
            }
        }

        private void Update() {
            // Don't process input if dead or paused
            if (!isAlive || isPaused) {
                return;
            }

            // Get horizontal input
            float horizontalInput = GetHorizontalInput();

            // Send input to movement executor
            if (playerMovement != null) {
                playerMovement.SetHorizontalInput(horizontalInput);

                if (showDebugInfo) {
                    Debug.Log($"[PlayerController] Input: {horizontalInput:F2}, Position: {playerMovement.GetHorizontalPosition():F2}");
                }
            }

            // Handle other input (pause, etc.)
            if (Input.GetKeyDown(KeyCode.Escape)) {
                TogglePause();
            }
        }
        #endregion

        #region Input Handling
        /// <summary>
        /// Get horizontal input from keyboard or gamepad.
        /// </summary>
        /// <returns>Horizontal input value between -1 and 1</returns>
        private float GetHorizontalInput() {
            if (useHorizontalAxis) {
                // Use Unity's Input.GetAxis for smooth input
                return Input.GetAxis("Horizontal");
            } else {
                // Use raw key presses
                float input = 0f;
                if (Input.GetKey(leftKey)) input -= 1f;
                if (Input.GetKey(rightKey)) input += 1f;
                return input;
            }
        }
        #endregion

        #region Game State Management
        /// <summary>
        /// Call when player dies. Disables movement.
        /// </summary>
        public void OnPlayerDeath() {
            isAlive = false;

            if (playerMovement != null) {
                playerMovement.SetMovementActive(false);
            }

            Debug.Log("[PlayerController] Player died - movement disabled");
        }

        /// <summary>
        /// Call when player respawns. Re-enables movement.
        /// </summary>
        public void OnPlayerRespawn() {
            isAlive = true;

            if (playerMovement != null) {
                playerMovement.SetMovementActive(!isPaused);
            }

            Debug.Log("[PlayerController] Player respawned - movement enabled");
        }

        /// <summary>
        /// Toggle pause state.
        /// </summary>
        public void TogglePause() {
            isPaused = !isPaused;

            if (playerMovement != null) {
                playerMovement.SetMovementActive(isAlive && !isPaused);
            }

            // Also pause/unpause time
            Time.timeScale = isPaused ? 0f : 1f;

            Debug.Log($"[PlayerController] Pause: {isPaused}");
        }

        /// <summary>
        /// Set pause state directly.
        /// </summary>
        /// <param name="paused">True to pause, false to unpause</param>
        public void SetPaused(bool paused) {
            isPaused = paused;

            if (playerMovement != null) {
                playerMovement.SetMovementActive(isAlive && !isPaused);
            }

            Time.timeScale = isPaused ? 0f : 1f;
        }

        /// <summary>
        /// Set player speed (e.g., for power-ups or slow zones).
        /// </summary>
        /// <param name="speed">New forward speed</param>
        public void SetSpeed(float speed) {
            if (playerMovement != null) {
                playerMovement.SetForwardSpeed(speed);
            }
        }

        /// <summary>
        /// Set horizontal bounds (e.g., for narrow sections).
        /// </summary>
        /// <param name="bounds">Maximum horizontal distance from center</param>
        public void SetHorizontalBounds(float bounds) {
            if (playerMovement != null) {
                playerMovement.SetHorizontalBounds(bounds);
            }
        }
        #endregion

        #region Public API - State Queries
        /// <summary>
        /// Check if player is currently alive.
        /// </summary>
        public bool IsAlive() => isAlive;

        /// <summary>
        /// Check if game is currently paused.
        /// </summary>
        public bool IsPaused() => isPaused;

        /// <summary>
        /// Get reference to PlayerMovement component.
        /// </summary>
        public PlayerMovement GetMovement() => playerMovement;
        #endregion
    }
}
