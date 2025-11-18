using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class MobileInputController : MonoBehaviour {

    #region Serialized Fields
    [Header("Input Settings")]
    [SerializeField] private bool useNewInputSystem = true;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject touchIndicatorPrefab;
    [SerializeField] private Canvas touchCanvas;

    [Header("Touch Timing")]
    [SerializeField] private float minTouchHoldTime = 0.1f;
    [SerializeField] private float maxTouchHoldTime = 1.0f;
    [SerializeField] private bool enableTouchFeedback = true;
    [SerializeField] private float indicatorDepth = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
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
    // Events for dynamic jumping system
    public System.Action<float> OnTouchStartedEventHandler;
    public System.Action<float> OnTouchReleasedEventHandler;
    #endregion

    #region Public API - Polling
    // Public polling API for systems that need continuous touch state
    public bool IsTouchHeld => isTouchHeld;

    /// <summary>
    /// Get the current normalized hold time (0-1 range based on maxTouchHoldTime)
    /// </summary>
    /// <returns>Normalized hold time, or 0 if not currently held</returns>
    public float GetCurrentNormalizedHoldTime() {
        if (!isTouchHeld) return 0f;
        return GetNormalizedHoldTime(GetCurrentHoldDuration());
    }

    /// <summary>
    /// Get the raw hold duration in seconds
    /// </summary>
    /// <returns>Hold duration in seconds, or 0 if not currently held</returns>
    public float GetCurrentHoldDuration() {
        if (!isTouchHeld) return 0f;
        return Time.time - touchHoldStartTime;
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

    // Start is called before the first frame update
    void Start() {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update() {
        HandleTouchState();
    }

    void OnDisable() {
        if (useNewInputSystem)
            DisableInputActions();
    }

    void OnDestroy() {
        if (touchPressAction != null) {
            touchPressAction.started -= OnTouchStarted;
            touchPressAction.canceled -= OnTouchCanceled;
            touchPressAction.Dispose();
        }
        touchPositionAction?.Dispose();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Initialize screen dimensions
    /// </summary>
    private void InitializeScreenDimensions() {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
    }

    /// <summary>
    /// Initialize input system with PlayerInput
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
                LogDebug("No suitable action map found, will use fallback input");
            } else {
                LogDebug($"Using action map: {touchActionMap.name}");
            }
        } else {
            LogDebug("No Input Actions asset assigned to PlayerInput, using fallback input");
        }

        LogDebug("Input system initialized with PlayerInput integration");
    }

    /// <summary>
    /// Initialize Input Actions, Start, Cancel Events
    /// </summary>
    private void SetupInputActions() {
        if (touchActionMap != null) {
            // Find touch actions with flexible naming
            touchPositionAction = touchActionMap.FindAction("TouchPosition") ??
                                touchActionMap.FindAction("Position") ??
                                touchActionMap.FindAction("Point") ??
                                touchActionMap.FindAction("PrimaryPosition");

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
                LogDebug("No touch press action found");
            }

            if (touchPositionAction != null) {
                LogDebug($"Touch position action '{touchPositionAction.name}' connected");
            } else {
                LogDebug("No touch position action found");
            }

            LogDebug("Touch actions setup complete");
        } else {
            LogDebug("Using fallback touch input (Input class)");
        }
    }

    private void EnableInputActions() {
        touchPressAction?.Enable();
        touchPositionAction?.Enable();
    }

    private void DisableInputActions() {
        touchPressAction?.Disable();
        touchPositionAction?.Disable();
    }
    #endregion

    #region Touch State Management
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

        // Detect touch release for dynamic jumping
        if (wasTouchHeld && !isTouchHeld) {
            OnTouchReleaseDetected();
        }

        wasTouchHeld = isTouchHeld;
    }

    private void OnTouchStarted(InputAction.CallbackContext context) {
        LogDebug("Touch started via Input Action");

        if (isTouchHeld) return;    // Prevent multiple touches
        StartTouchHold();           //
        CreateTouchIndicator();     // Visual feedback
    }

    private void OnTouchCanceled(InputAction.CallbackContext context) {
        LogDebug("Touch canceled via Input Action");

        if (isTouchHeld) {
            EndTouchHold();
            DestroyJumpIndicator();
        }
    }


    /// <summary>
    /// StartTouchHold set isTouchHeld variable to true
    /// touchHoldStartTime is Time.time
    /// touchStartPosition is set to touchposition (touchPositionAction.ReadValue<Vector2>())
    /// currentTouchposition is set to touchStartPosition
    /// Invoke OnTouchHoldStarted Event
    /// </summary>
    private void StartTouchHold() {
        isTouchHeld = true;
        touchHoldStartTime = Time.time;

        // Get initial touch position
        if (touchPositionAction != null) {
            touchStartPosition = touchPositionAction.ReadValue<Vector2>();
            currentTouchPosition = touchStartPosition;
        }

        // Notify dynamic jump system - START JUMPING IMMEDIATELY
        OnTouchStartedEventHandler?.Invoke(0f);

        LogDebug($"Touch hold started at position: {touchStartPosition}");
    }

    /// <summary>
    /// End touch hold, calculate and store the normalized hold time
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

    private void OnTouchReleaseDetected() {
        // Use the stored normalized hold time (calculated when touch ended)
        // Only trigger release event if hold was valid (not too short)
        if (lastNormalizedHoldTime > 0f) {
            // Notify dynamic jump system - TRIGGER FAST FALL
            OnTouchReleasedEventHandler?.Invoke(lastNormalizedHoldTime);
            LogDebug($"Touch release detected for dynamic jumping");
        }
    }

    private void ForceEndTouch() {
        LogDebug("Force ending touch due to max hold time");
        EndTouchHold();
        DestroyJumpIndicator();
    }
    #endregion

    #region Visual Feedback
    private void CreateTouchIndicator() {
        if (!enableTouchFeedback || touchIndicatorPrefab == null) return;

        // Create indicator at touch position in world space
        Vector3 worldPos = ScreenToWorldPosition(touchStartPosition);

        currentJumpIndicator = Instantiate(touchIndicatorPrefab, worldPos, Quaternion.identity);

        // Parent to canvas if available
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

        LogDebug("Jump indicator created");
    }

    private void DestroyJumpIndicator() {
        if (currentJumpIndicator != null) {
            Destroy(currentJumpIndicator);
            currentJumpIndicator = null;
            LogDebug("Jump indicator destroyed");
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Helper: Calculate normalized hold time from raw duration
    /// </summary>
    private float GetNormalizedHoldTime(float holdDuration) {
        return Mathf.Clamp01(holdDuration / maxTouchHoldTime);
    }

    /// <summary>
    /// Helper: Convert screen position to world position
    /// </summary>
    private Vector3 ScreenToWorldPosition(Vector2 screenPos) {
        if (mainCamera == null) return Vector3.zero;
        return mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, indicatorDepth));
    }

    /// <summary>
    /// Helper: Find input action by trying multiple fallback names
    /// </summary>
    private InputAction FindInputAction(InputActionMap actionMap, params string[] actionNames) {
        if (actionMap == null) return null;

        foreach (string name in actionNames) {
            var action = actionMap.FindAction(name);
            if (action != null) return action;
        }
        return null;
    }

    /// <summary>
    /// Log debug message if logging is enabled
    /// </summary>
    private void LogDebug(string message) {
        if (showDebugInfo) {
            Debug.Log($"[MobileInputController] {message}");
        }
    }
    #endregion

}
