---
name: mobile-input-controller
description: This skill helps create mobile input controllers for Unity games using the New Input System. Use this skill when implementing touch controls, virtual joysticks, button inputs, or gesture-based mechanics for mobile platforms. Provides templates, best practices, and patterns for responsive mobile gameplay.
---

# Mobile Input Controller

## Overview

This skill provides comprehensive templates and patterns for implementing mobile input controls in Unity using the New Input System. It includes ready-to-use controllers for touch-based mechanics like jumping, movement, shooting, and gestures, along with best practices for mobile UX and performance.

## Core Principles for Mobile Input

1. **Responsive Feedback** - Provide immediate visual/haptic feedback for all touch interactions
2. **Configurable Timing** - Allow designers to tune min/max hold times, swipe thresholds, etc.
3. **Event + Polling Hybrid** - Support both event-driven and polling patterns for flexibility
4. **New Input System Integration** - Leverage Unity's PlayerInput for cross-platform support
5. **Visual Indicators** - Show touch points, drag paths, and hold durations
6. **Graceful Fallbacks** - Handle edge cases like max hold time, multi-touch conflicts
7. **Performance Optimized** - Minimize allocations, use object pooling for indicators

## When to Use This Skill

### Use this skill when:
- ✅ Implementing touch controls for mobile games (iOS, Android)
- ✅ Creating virtual joysticks, buttons, or gesture systems
- ✅ Converting PC/console controls to mobile-friendly input
- ✅ Building touch-based mechanics (hold-to-charge, swipe-to-attack, etc.)
- ✅ Integrating Unity's New Input System with mobile platforms
- ✅ Need responsive, designer-friendly mobile controls

### Don't use this skill when:
- ❌ Building PC/console-only games (use standard Input System)
- ❌ Simple UI interactions (use Unity UI events instead)
- ❌ Legacy Input.GetTouch() systems (this uses New Input System)

## Architecture Pattern

The Mobile Input Controller uses a hybrid **Event-Driven + Polling** architecture:

```
┌─────────────────────────────────────────┐
│   MobileInputController (MonoBehaviour) │
│                                         │
│  ┌─────────────────────────────────┐  │
│  │   Unity New Input System         │  │
│  │   (PlayerInput Component)        │  │
│  └──────────┬──────────────────────┘  │
│             │                          │
│             ▼                          │
│  ┌─────────────────────────────────┐  │
│  │   Input Actions                  │  │
│  │   - TouchPress                   │  │
│  │   - TouchPosition                │  │
│  └──────────┬──────────────────────┘  │
│             │                          │
│             ▼                          │
│  ┌─────────────────────────────────┐  │
│  │   Touch State Management         │  │
│  │   - Hold Duration                │  │
│  │   - Position Tracking            │  │
│  │   - Min/Max Constraints          │  │
│  └──────────┬──────────────────────┘  │
│             │                          │
│      ┌──────┴──────┐                  │
│      ▼             ▼                  │
│  ┌─────┐      ┌─────────┐            │
│  │Events│      │Polling  │            │
│  │API   │      │API      │            │
│  └──┬───┘      └────┬────┘            │
│     │               │                 │
└─────┼───────────────┼─────────────────┘
      │               │
      ▼               ▼
┌──────────┐    ┌──────────┐
│Game      │    │UI        │
│Systems   │    │Systems   │
└──────────┘    └──────────┘
```

**Why This Pattern:**
- **Events** - Fire-and-forget actions (jump trigger, shoot button)
- **Polling** - Continuous state queries (is touch held? current position?)
- **PlayerInput** - Unified input across platforms, rebindable controls
- **Visual Feedback** - Immediate player response, improved UX

## Key Components

### 1. Touch Hold Controller (Base Template)

**Purpose:** Track touch hold duration and trigger events on press/release.

**Use Cases:**
- Hold-to-jump mechanic (longer hold = higher jump)
- Charge attacks (hold to power up)
- Timing-based interactions

**Features:**
- Min/max hold time constraints
- Normalized hold time (0-1 range)
- Auto-release on max hold
- Visual touch indicators
- Debug logging

**Template:** `assets/templates/MobileInputController.cs`

### 2. Virtual Joystick Controller

**Purpose:** Analog movement input for mobile (drag-based directional control).

**Use Cases:**
- Character movement (8-directional or analog)
- Camera rotation
- Vehicle steering

**Template:** `assets/templates/VirtualJoystickController.cs` *(To be implemented)*

### 3. Swipe Gesture Controller

**Purpose:** Detect directional swipes (up/down/left/right).

**Use Cases:**
- Menu navigation
- Attack combos (swipe patterns)
- Quick dodge/dash mechanics

**Template:** `assets/templates/SwipeGestureController.cs` *(To be implemented)*

### 4. Multi-Touch Controller

**Purpose:** Handle simultaneous touch inputs (pinch-to-zoom, two-finger rotation).

**Use Cases:**
- Camera controls (pinch-to-zoom)
- Rotation gestures
- Multi-finger combos

**Template:** `assets/templates/MultiTouchController.cs` *(To be implemented)*

## Usage Guide

### Step 1: Set Up Unity Input System

1. **Install Input System package:**
   - Window → Package Manager
   - Find "Input System" → Install
   - Restart Unity when prompted

2. **Create Input Actions Asset:**
   - Right-click in Project → Create → Input Actions
   - Name it `MobileInputActions`

3. **Configure Touch Actions:**
   ```
   Action Maps:
   ├── Touch
   │   ├── TouchPress (Button)
   │   │   └── Binding: <Touchscreen>/primaryTouch/press
   │   └── TouchPosition (Value: Vector2)
   │       └── Binding: <Touchscreen>/primaryTouch/position
   ```

4. **Generate C# Class:**
   - Select InputActions asset → "Generate C# Class"
   - This creates strongly-typed input wrappers

### Step 2: Add Mobile Input Controller

1. **Copy template to your project:**
   ```bash
   # Copy from skill to your project
   cp .claude/skills/mobile-input-controller/assets/templates/MobileInputController.cs \
      Assets/_Project/Scripts/Input/
   ```

2. **Attach to GameObject:**
   - Create GameObject: `InputManager`
   - Add Component → `MobileInputController`
   - Add Component → `PlayerInput` (auto-added by RequireComponent)

3. **Configure in Inspector:**
   - Assign Input Actions asset to PlayerInput
   - Set action map to "Touch"
   - Configure min/max hold times
   - Assign touch indicator prefab (optional)
   - Enable debug logging for testing

### Step 3: Connect to Gameplay Systems

**Method 1: Event-Based (Recommended for discrete actions)**

```csharp
public class PlayerJump : MonoBehaviour
{
    [SerializeField] private MobileInputController inputController;
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private float minJumpForce = 5f;
    [SerializeField] private float maxJumpForce = 15f;

    private void OnEnable()
    {
        // Subscribe to touch events
        inputController.OnTouchStartedEventHandler += OnJumpStarted;
        inputController.OnTouchReleasedEventHandler += OnJumpReleased;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        inputController.OnTouchStartedEventHandler -= OnJumpStarted;
        inputController.OnTouchReleasedEventHandler -= OnJumpReleased;
    }

    private void OnJumpStarted(float normalizedTime)
    {
        // Start jump immediately (normalizedTime = 0)
        Debug.Log("Jump started!");
    }

    private void OnJumpReleased(float normalizedHoldTime)
    {
        // Calculate jump force based on hold time
        float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, normalizedHoldTime);
        playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        Debug.Log($"Jump released! Force: {jumpForce} (hold: {normalizedHoldTime:F2})");
    }
}
```

**Method 2: Polling-Based (Recommended for continuous state)**

```csharp
public class ChargeAttack : MonoBehaviour
{
    [SerializeField] private MobileInputController inputController;
    [SerializeField] private ParticleSystem chargeEffect;
    [SerializeField] private float maxChargeDamage = 100f;

    private void Update()
    {
        // Poll current touch state
        if (inputController.IsTouchHeld)
        {
            // Get current charge level (0-1)
            float chargeLevel = inputController.GetCurrentNormalizedHoldTime();

            // Update visual effect based on charge
            var emission = chargeEffect.emission;
            emission.rateOverTime = chargeLevel * 100f;

            // Display charge percentage
            Debug.Log($"Charging: {chargeLevel * 100f:F0}%");
        }
        else if (chargeEffect.isPlaying)
        {
            chargeEffect.Stop();
        }
    }
}
```

### Step 4: Create Visual Feedback (Optional)

1. **Create Touch Indicator Prefab:**
   ```
   TouchIndicator (GameObject)
   ├── Canvas (RenderMode: Screen Space - Overlay)
   │   └── Circle Image (UI Image)
   │       - Color: White with transparency
   │       - Size: 50x50 pixels
   │       - Add Animator for pulse effect
   ```

2. **Assign to Controller:**
   - Drag prefab to `Touch Indicator Prefab` field
   - Assign Canvas to `Touch Canvas` field
   - Enable `Enable Touch Feedback`

## Best Practices

### Performance Optimization

1. **Object Pooling for Indicators:**
   ```csharp
   // Instead of Instantiate/Destroy every touch
   private ObjectPool<GameObject> indicatorPool;

   private void CreateTouchIndicator()
   {
       currentIndicator = indicatorPool.Get();
       currentIndicator.transform.position = touchPosition;
   }

   private void DestroyTouchIndicator()
   {
       indicatorPool.Release(currentIndicator);
   }
   ```

2. **Cache Component References:**
   ```csharp
   // ✅ GOOD: Cache in Awake
   private Camera mainCamera;
   private void Awake() {
       mainCamera = Camera.main;
   }

   // ❌ BAD: Find every frame
   private void Update() {
       Camera.main.ScreenToWorldPoint(...);
   }
   ```

3. **Minimize Update Allocations:**
   ```csharp
   // ✅ GOOD: Reuse Vector2
   private Vector2 currentTouchPosition;

   // ❌ BAD: Creates garbage every frame
   Vector2 pos = new Vector2(x, y);
   ```

### Touch UX Guidelines

1. **Minimum Touch Target Size:**
   - Minimum: 44x44 points (iOS), 48x48 dp (Android)
   - Comfortable: 60x60 pixels or larger
   - Add padding around interactive areas

2. **Visual Feedback Timing:**
   - Instant feedback: 0ms (press down)
   - Quick feedback: <100ms (touch highlight)
   - Loading states: >200ms (show spinner)

3. **Hold Time Tuning:**
   - Min hold time: 0.1s (prevent accidental taps)
   - Max hold time: 1-3s (depends on mechanic)
   - Provide visual progress indicator

4. **Touch Tolerance:**
   - Allow small finger drift (5-10 pixels)
   - Cancel action if finger moves too far
   - Consider finger size (30-40 pixel touch radius)

### Input System Configuration

1. **Action Binding Best Practices:**
   ```
   ✅ Use descriptive action names: "Jump", "Fire", "Move"
   ✅ Group related actions: "Gameplay", "UI", "Debug"
   ✅ Support multiple bindings: Touch + Mouse for testing
   ✅ Use composites for complex input (2D Vector, Hold)
   ```

2. **PlayerInput Settings:**
   ```
   Behavior: Send Messages (for Unity Events)
   Default Action Map: Touch (or Gameplay)
   UI Input Module: Replace (for UI interaction)
   ```

3. **Testing on Multiple Devices:**
   ```csharp
   #if UNITY_EDITOR
       // Simulate touch with mouse in editor
       if (Input.GetMouseButtonDown(0)) {
           // Trigger touch logic
       }
   #endif
   ```

## Common Patterns

### Pattern 1: Hold-to-Charge

```csharp
// Example: Bow & Arrow charge mechanic
private void OnTouchReleased(float normalizedHoldTime)
{
    // Calculate arrow power (0-1)
    float power = normalizedHoldTime;

    // Clamp to min/max range
    float arrowSpeed = Mathf.Lerp(minSpeed, maxSpeed, power);

    // Fire arrow
    FireArrow(arrowSpeed);
}
```

### Pattern 2: Tap vs Hold Detection

```csharp
// Differentiate quick tap from hold
private void OnTouchReleased(float normalizedHoldTime)
{
    if (normalizedHoldTime < 0.2f) {
        // Quick tap - Light attack
        PerformLightAttack();
    } else {
        // Long hold - Heavy attack
        PerformHeavyAttack();
    }
}
```

### Pattern 3: Touch Position for Aiming

```csharp
// Use touch position for directional input
private void Update()
{
    if (inputController.IsTouchHeld)
    {
        Vector2 touchPos = inputController.currentTouchPosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(touchPos);

        // Aim weapon at touch point
        AimAtPosition(worldPos);
    }
}
```

### Pattern 4: Progressive Power-Up

```csharp
// Increase power over time while held
private void Update()
{
    if (inputController.IsTouchHeld)
    {
        float holdDuration = inputController.GetCurrentHoldDuration();

        // Power increases in stages
        if (holdDuration > 0.5f && holdDuration < 1.0f) {
            // Stage 1: 50% power
            SetPowerLevel(0.5f);
        } else if (holdDuration >= 1.0f) {
            // Stage 2: 100% power
            SetPowerLevel(1.0f);
        }
    }
}
```

## Troubleshooting

### Issue: Touch Not Detected

**Problem:** Touch events don't fire, actions not triggered.

**Solutions:**
1. ✅ Check Input Actions asset assigned to PlayerInput
2. ✅ Verify action map name matches (case-sensitive)
3. ✅ Ensure actions are enabled in OnEnable
4. ✅ Check PlayerInput is set to "Send Messages" or "Invoke Unity Events"
5. ✅ Enable debug logging to see touch state

### Issue: Touch Position Incorrect

**Problem:** Touch indicator appears at wrong position.

**Solutions:**
1. ✅ Check Canvas RenderMode (Screen Space - Overlay vs Camera)
2. ✅ Verify Camera reference is set (Camera.main)
3. ✅ Use RectTransformUtility for UI positioning
4. ✅ Check indicator depth for world space positioning

### Issue: Multiple Touches Conflict

**Problem:** Second touch cancels first touch.

**Solutions:**
1. ✅ Track touches by ID (primaryTouch vs touch0/touch1)
2. ✅ Use multi-touch controller template
3. ✅ Implement touch priority system
4. ✅ Ignore touches in UI areas (use EventSystem.IsPointerOverGameObject)

### Issue: Performance Lag on Mobile

**Problem:** Input feels sluggish, frame drops on touch.

**Solutions:**
1. ✅ Use object pooling for touch indicators
2. ✅ Cache all component references
3. ✅ Minimize Update() allocations (no `new Vector2`)
4. ✅ Profile with Unity Profiler on device
5. ✅ Reduce particle effects on low-end devices

## Examples

### Example 1: Flappy Bird Style Jump

```csharp
public class FlappyJump : MonoBehaviour
{
    [SerializeField] private MobileInputController input;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float jumpForce = 5f;

    private void OnEnable()
    {
        input.OnTouchStartedEventHandler += OnTap;
    }

    private void OnDisable()
    {
        input.OnTouchStartedEventHandler -= OnTap;
    }

    private void OnTap(float _)
    {
        // Reset velocity and jump
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}
```

### Example 2: Angry Birds Slingshot

```csharp
public class Slingshot : MonoBehaviour
{
    [SerializeField] private MobileInputController input;
    [SerializeField] private LineRenderer trajectoryLine;

    private void Update()
    {
        if (input.IsTouchHeld)
        {
            // Show trajectory while holding
            float power = input.GetCurrentNormalizedHoldTime();
            ShowTrajectory(power);
        }
    }

    private void OnEnable()
    {
        input.OnTouchReleasedEventHandler += OnRelease;
    }

    private void OnRelease(float power)
    {
        // Launch projectile with power
        LaunchProjectile(power);
        trajectoryLine.enabled = false;
    }
}
```

## Additional Resources

- [Unity Input System Documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [Mobile Input Best Practices](assets/references/mobile-input-best-practices.md)
- [Touch Gesture Patterns](assets/references/touch-gesture-patterns.md)
- [Performance Optimization Guide](assets/references/performance-optimization.md)

## Related Skills

- `codebase-documenter` - For documenting input systems
- `unity-component-generator` - For creating new input component types *(if available)*
- `unity-testing-helper` - For testing input controllers *(if available)*

---

**Version:** 1.0
**Last Updated:** 2025-01-18
**Maintained By:** Claude Code Skills
