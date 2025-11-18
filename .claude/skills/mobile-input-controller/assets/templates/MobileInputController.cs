using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Mobile input controller for touch-based mechanics using Unity's New Input System.
///
/// Features:
/// - Touch hold duration tracking with min/max constraints
/// - Normalized hold time (0-1 range)
/// - Visual touch feedback with indicators
/// - Event-based architecture for discrete actions
/// - Polling API for continuous state queries
/// - Auto-release on max hold time
/// - Debug logging for testing
///
/// Usage:
/// 1. Attach to GameObject with PlayerInput component
/// 2. Configure Input Actions asset with Touch action map
/// 3. Subscribe to OnTouchStarted/Released events OR poll IsTouchHeld
/// 4. Optionally assign touch indicator prefab for visual feedback
///
/// Example Event-Based:
/// <code>
/// inputController.OnTouchStartedEventHandler += OnJumpStart;
/// inputController.OnTouchReleasedEventHandler += (holdTime) => Jump(holdTime);
/// </code>
///
/// Example Polling-Based:
/// <code>
/// if (inputController.IsTouchHeld) {
///     float chargeLevel = inputController.GetCurrentNormalizedHoldTime();
///     UpdateChargeVisual(chargeLevel);
/// }
/// </code>
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class MobileInputController : MonoBehaviour {

    #region Serialized Fields
    [Header("Input Settings")]
    [SerializeField, Tooltip("Use Unity's New Input System (recommended)")]
    private bool useNewInputSystem = true;

    [Header("Visual Feedback")]
    [SerializeField, Tooltip("Prefab to show at touch position (optional)")]
    private GameObject touchIndicatorPrefab;

    [SerializeField, Tooltip("Canvas to parent touch indicators to (optional)")]
    private Canvas touchCanvas;

    [Header("Touch Timing")]
    [SerializeField, Tooltip("Minimum hold time to register as valid touch (seconds)")]
    private float minTouchHoldTime = 0.1f;

    [SerializeField, Tooltip("Maximum hold time before auto-release (seconds)")]
    private float maxTouchHoldTime = 1.0f;

    [SerializeField, Tooltip("Enable visual touch feedback")]
    private bool enableTouchFeedback = true;

    [SerializeField, Tooltip("Depth for world space indicator positioning")]
    private float indicatorDepth = 10f;

    [Header("Debug")]
    [SerializeField, Tooltip("Show debug logs in console")]
    private bool showDebugInfo = false;
    #endregion

    #region Private Fields
    // Screen dimensions
    private float screenWidth;
    private float screenHeight;

    // Touch state
    private bool isTouchHeld = false;
    private bool wasTouchHeld = false;
    private float touchHoldStartTime = 0f;
    private float lastNormalizedHoldTime = 0f;  // Store result when touch ends
    private Vector2 touchStartPosition;
    private Vector2 currentTouchPosition;

    // Input Actions
    [SerializeField] private PlayerInput playerInput;
    private InputActionMap touchActionMap;
    private InputAction touchPressAction;
    private InputAction touchPositionAction;

    // Visual feedback
    private Camera mainCamera;
    private GameObject currentJumpIndicator;
    #endregion

    #region Public Events
    /// <summary>
    /// Invoked when touch starts. Parameter is normalized hold time (always 0 on start).
    /// Use for immediate actions like starting a jump or activating a power-up.
    /// </summary>
    public System.Action<float> OnTouchStartedEventHandler;

    /// <summary>
    /// Invoked when touch is released. Parameter is normalized hold time (0-1).
    /// Use for actions that depend on hold duration like jump height or attack power.
    /// </summary>
    public System.Action<float> OnTouchReleasedEventHandler;
    #endregion

    #region Public API - Polling
    /// <summary>
    /// Returns true if touch is currently held down.
    /// Use for continuous queries like charging animations or power meters.
    /// </summary>
    public bool IsTouchHeld => isTouchHeld;

    /// <summary>
    /// Get the current normalized hold time (0-1 range based on maxTouchHoldTime).
    /// Returns 0 if touch is not currently held.
    ///
    /// Example: 0.5 = held for 50% of max hold time
    /// </summary>
    /// <returns>Normalized hold time between 0 and 1</returns>
    public float GetCurrentNormalizedHoldTime() {
        if (!isTouchHeld) return 0f;
        return GetNormalizedHoldTime(GetCurrentHoldDuration());
    }

    /// <summary>
    /// Get the raw hold duration in seconds.
    /// Returns 0 if touch is not currently held.
    /// </summary>
    /// <returns>Hold duration in seconds</returns>
    public float GetCurrentHoldDuration() {
        if (!isTouchHeld) return 0f;
        return Time.time - touchHoldStartTime;
    }

    /// <summary>
    /// Get the current touch position in screen coordinates.
    /// Returns Vector2.zero if touch is not currently held.
    /// </summary>
    /// <returns>Touch position in screen space (pixels)</returns>
    public Vector2 GetCurrentTouchPosition() {
        return isTouchHeld ? currentTouchPosition : Vector2.zero;
    }
    #endregion

    #region Unity Lifecycle
    void OnEnable() {
        if (useNewInputSystem)
            EnableInputActions();
    }

    private void Awake() {
        InitializeScreenDimensions();
        InitializeInputSystem();
        SetupInputActions();
    }

    void Start() {
        mainCamera = Camera.main;

        if (mainCamera == null) {
            Debug.LogWarning("[MobileInputController] No main camera found. Touch indicators may not work correctly.");
        }
    }

    void Update() {
        HandleTouchState();
    }

    void OnDisable() {
        if (useNewInputSystem)
            DisableInputActions();
    }

    void OnDestroy() {
        // Clean up input action subscriptions
        if (touchPressAction != null) {
            touchPressAction.started -= OnTouchStarted;
            touchPressAction.canceled -= OnTouchCanceled;
            touchPressAction.Dispose();
        }
        touchPositionAction?.Dispose();

        // Clean up visual feedback
        if (currentJumpIndicator != null) {
            Destroy(currentJumpIndicator);
        }
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Initialize screen dimensions for touch position calculations.
    /// </summary>
    private void InitializeScreenDimensions() {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        LogDebug($"Screen dimensions: {screenWidth}x{screenHeight}");
    }

    /// <summary>
    /// Initialize Unity's New Input System with PlayerInput component.
    /// Attempts to find appropriate action maps with flexible naming.
    /// </summary>
    private void InitializeInputSystem() {
        // Get or add PlayerInput component
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null) {
            playerInput = gameObject.AddComponent<PlayerInput>();
            LogDebug("PlayerInput component added automatically");
        }

        // Setup for touch input
        if (playerInput.actions != null) {
            // Try to find Touch action map first, then fall back to others
            touchActionMap = playerInput.actions.FindActionMap("Touch") ??
                           playerInput.actions.FindActionMap("UI") ??
                           playerInput.actions.FindActionMap("Player");

            if (touchActionMap == null) {
                Debug.LogWarning("[MobileInputController] No suitable action map found. Please assign an Input Actions asset with a 'Touch', 'UI', or 'Player' action map.");
            } else {
                LogDebug($"Using action map: {touchActionMap.name}");
            }
        } else {
            Debug.LogWarning("[MobileInputController] No Input Actions asset assigned to PlayerInput. Please assign one in the Inspector.");
        }

        LogDebug("Input system initialized with PlayerInput integration");
    }

    /// <summary>
    /// Setup Input Action subscriptions for touch press and position.
    /// Attempts multiple naming conventions for compatibility.
    /// </summary>
    private void SetupInputActions() {
        if (touchActionMap != null) {
            // Find touch position action with flexible naming
            touchPositionAction = touchActionMap.FindAction("TouchPosition") ??
                                touchActionMap.FindAction("Position") ??
                                touchActionMap.FindAction("Point") ??
                                touchActionMap.FindAction("PrimaryPosition");

            // Find touch press action with flexible naming
            touchPressAction = touchActionMap.FindAction("TouchPress") ??
                             touchActionMap.FindAction("Press") ??
                             touchActionMap.FindAction("Click") ??
                             touchActionMap.FindAction("PrimaryContact") ??
                             touchActionMap.FindAction("Jump");

            // Subscribe to touch events
            if (touchPressAction != null) {
                touchPressAction.started += OnTouchStarted;
                touchPressAction.canceled += OnTouchCanceled;
                LogDebug($"Touch press action '{touchPressAction.name}' connected");
            } else {
                Debug.LogWarning("[MobileInputController] No touch press action found. Expected action names: 'TouchPress', 'Press', 'Click', 'PrimaryContact', or 'Jump'");
            }

            if (touchPositionAction != null) {
                LogDebug($"Touch position action '{touchPositionAction.name}' connected");
            } else {
                Debug.LogWarning("[MobileInputController] No touch position action found. Expected action names: 'TouchPosition', 'Position', 'Point', or 'PrimaryPosition'");
            }

            LogDebug("Touch actions setup complete");
        } else {
            LogDebug("Using fallback touch input (Input class)");
        }
    }

    private void EnableInputActions() {
        touchPressAction?.Enable();
        touchPositionAction?.Enable();
        LogDebug("Input actions enabled");
    }

    private void DisableInputActions() {
        touchPressAction?.Disable();
        touchPositionAction?.Disable();
        LogDebug("Input actions disabled");
    }
    #endregion

    #region Touch State Management
    /// <summary>
    /// Main touch state handling loop.
    /// Runs every frame to update touch position and handle auto-release.
    /// </summary>
    private void HandleTouchState() {
        // Update current touch position if available
        if (touchPositionAction != null && isTouchHeld) {
            currentTouchPosition = touchPositionAction.ReadValue<Vector2>();
        }

        // Handle dynamic touch updates
        if (isTouchHeld) {
            // Auto-release if held too long
            if (GetCurrentHoldDuration() >= maxTouchHoldTime) {
                ForceEndTouch();
            }
        }

        // Detect touch release for event notification
        if (wasTouchHeld && !isTouchHeld) {
            OnTouchReleaseDetected();
        }

        wasTouchHeld = isTouchHeld;
    }

    /// <summary>
    /// Called by Input System when touch press action starts.
    /// </summary>
    private void OnTouchStarted(InputAction.CallbackContext context) {
        LogDebug("Touch started via Input Action");

        if (isTouchHeld) return;    // Prevent multiple simultaneous touches

        StartTouchHold();
        CreateTouchIndicator();
    }

    /// <summary>
    /// Called by Input System when touch press action is canceled (released).
    /// </summary>
    private void OnTouchCanceled(InputAction.CallbackContext context) {
        LogDebug("Touch canceled via Input Action");

        if (isTouchHeld) {
            EndTouchHold();
            DestroyJumpIndicator();
        }
    }

    /// <summary>
    /// Start tracking touch hold duration and position.
    /// Fires OnTouchStarted event immediately.
    /// </summary>
    private void StartTouchHold() {
        isTouchHeld = true;
        touchHoldStartTime = Time.time;

        // Get initial touch position
        if (touchPositionAction != null) {
            touchStartPosition = touchPositionAction.ReadValue<Vector2>();
            currentTouchPosition = touchStartPosition;
        }

        // Notify listeners - START IMMEDIATELY (normalized time = 0)
        OnTouchStartedEventHandler?.Invoke(0f);

        LogDebug($"Touch hold started at position: {touchStartPosition}");
    }

    /// <summary>
    /// End touch hold and calculate final normalized hold time.
    /// Enforces minimum hold time - rejects touches that are too short.
    /// </summary>
    private void EndTouchHold() {
        if (!isTouchHeld) return;

        float holdDuration = GetCurrentHoldDuration();

        // Enforce minimum hold time - reject touches that are too short
        if (holdDuration < minTouchHoldTime) {
            LogDebug($"Touch too short ({holdDuration:F3}s < {minTouchHoldTime:F2}s minimum), ignoring");
            lastNormalizedHoldTime = 0f;  // Mark as invalid
            isTouchHeld = false;
            return;
        }

        // Calculate and store normalized time for valid holds
        lastNormalizedHoldTime = GetNormalizedHoldTime(holdDuration);

        // Clean up state
        isTouchHeld = false;

        LogDebug($"Touch released after {holdDuration:F2}s (normalized: {lastNormalizedHoldTime:F2})");
    }

    /// <summary>
    /// Called when touch release is detected (transition from held to not held).
    /// Fires OnTouchReleased event with final normalized hold time.
    /// </summary>
    private void OnTouchReleaseDetected() {
        // Use the stored normalized hold time (calculated when touch ended)
        // Only trigger release event if hold was valid (not too short)
        if (lastNormalizedHoldTime > 0f) {
            OnTouchReleasedEventHandler?.Invoke(lastNormalizedHoldTime);
            LogDebug($"Touch release event fired (normalized: {lastNormalizedHoldTime:F2})");
        }
    }

    /// <summary>
    /// Force end touch due to max hold time reached.
    /// Used for auto-release functionality.
    /// </summary>
    private void ForceEndTouch() {
        LogDebug("Force ending touch due to max hold time");
        EndTouchHold();
        DestroyJumpIndicator();
    }
    #endregion

    #region Visual Feedback
    /// <summary>
    /// Create visual indicator at touch position.
    /// Supports both world space and UI canvas positioning.
    /// </summary>
    private void CreateTouchIndicator() {
        if (!enableTouchFeedback || touchIndicatorPrefab == null) return;

        // Create indicator at touch position in world space
        Vector3 worldPos = ScreenToWorldPosition(touchStartPosition);

        currentJumpIndicator = Instantiate(touchIndicatorPrefab, worldPos, Quaternion.identity);

        // Parent to canvas if available (UI-based indicators)
        if (touchCanvas != null) {
            currentJumpIndicator.transform.SetParent(touchCanvas.transform, false);

            // Convert to canvas coordinates
            RectTransform rectTransform = currentJumpIndicator.GetComponent<RectTransform>();
            if (rectTransform != null) {
                Vector2 canvasPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    touchCanvas.transform as RectTransform,
                    touchStartPosition,
                    touchCanvas.worldCamera,
                    out canvasPos
                );
                rectTransform.localPosition = canvasPos;
            }
        }

        LogDebug("Touch indicator created");
    }

    /// <summary>
    /// Destroy touch indicator GameObject.
    /// </summary>
    private void DestroyJumpIndicator() {
        if (currentJumpIndicator != null) {
            Destroy(currentJumpIndicator);
            currentJumpIndicator = null;
            LogDebug("Touch indicator destroyed");
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Calculate normalized hold time from raw duration (0-1 range).
    /// </summary>
    /// <param name="holdDuration">Raw hold duration in seconds</param>
    /// <returns>Normalized value between 0 and 1</returns>
    private float GetNormalizedHoldTime(float holdDuration) {
        return Mathf.Clamp01(holdDuration / maxTouchHoldTime);
    }

    /// <summary>
    /// Convert screen position to world position using main camera.
    /// </summary>
    /// <param name="screenPos">Screen position in pixels</param>
    /// <returns>World position</returns>
    private Vector3 ScreenToWorldPosition(Vector2 screenPos) {
        if (mainCamera == null) return Vector3.zero;
        return mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, indicatorDepth));
    }

    /// <summary>
    /// Log debug message if debug logging is enabled.
    /// </summary>
    /// <param name="message">Debug message to log</param>
    private void LogDebug(string message) {
        if (showDebugInfo) {
            Debug.Log($"[MobileInputController] {message}");
        }
    }
    #endregion
}
