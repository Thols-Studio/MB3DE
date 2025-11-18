# Mobile Input Controller - Implementation Notes

**Version:** 1.0
**Last Updated:** 2025-01-18
**Skill:** mobile-input-controller

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Prerequisites](#prerequisites)
3. [Step-by-Step Implementation](#step-by-step-implementation)
4. [Project Structure After Implementation](#project-structure-after-implementation)
5. [Implementation Patterns](#implementation-patterns)
6. [Integration Scenarios](#integration-scenarios)
7. [Testing and Debugging Workflow](#testing-and-debugging-workflow)
8. [Deployment Checklist](#deployment-checklist)
9. [Common Implementation Pitfalls](#common-implementation-pitfalls)
10. [Real-World Examples](#real-world-examples)

---

## Quick Start

**Goal:** Get touch input working in your Unity mobile game in under 10 minutes.

### Minimal Setup

1. **Install Input System package** (Window → Package Manager → Input System → Install)
2. **Copy template to your project:**
   ```bash
   cp .claude/skills/mobile-input-controller/assets/templates/MobileInputController.cs \
      Assets/_Project/Scripts/Input/
   ```
3. **Create Input Actions asset:**
   - Right-click in Project → Create → Input Actions
   - Name: `MobileInputActions`
   - Add action map: `Touch`
   - Add actions: `TouchPress` (Button) and `TouchPosition` (Value: Vector2)
   - Generate C# Class
4. **Setup scene:**
   - Create GameObject: `InputManager`
   - Add `MobileInputController` component
   - Assign `MobileInputActions` to PlayerInput
   - Set Action Map to `Touch`
5. **Connect to your game:**
   ```csharp
   [SerializeField] private MobileInputController inputController;

   void OnEnable() {
       inputController.OnTouchReleasedEventHandler += OnJump;
   }

   void OnJump(float holdTime) {
       Debug.Log($"Jump with power: {holdTime}");
   }
   ```

**Result:** You now have working touch input that tracks hold duration!

---

## Prerequisites

### Unity Version
- **Minimum:** Unity 2021.3 LTS
- **Recommended:** Unity 2022.3 LTS or 6000.0.62f1 LTS or newer
- **Tested On:** Unity 6000.0.62f1 LTS

### Required Packages
- **Input System** (com.unity.inputsystem) version 1.5.0 or newer
  - Install via Package Manager
  - Restart Unity when prompted to switch input backend

### Project Settings
After installing Input System, Unity will ask to restart. This switches from old Input Manager to New Input System. If you need both:

```
Edit → Project Settings → Player → Active Input Handling → Input System Package (New)
```

### Target Platforms
- iOS 12.0+
- Android API Level 24+ (Android 7.0)

### Knowledge Requirements
- Basic C# understanding
- Unity MonoBehaviour lifecycle (Awake, Start, Update, OnEnable, OnDisable)
- Unity Events or C# delegates
- Basic understanding of Unity Input System (helpful but not required)

---

## Step-by-Step Implementation

### Phase 1: Unity Input System Setup (5-10 minutes)

#### 1.1 Create Input Actions Asset

1. **Create the asset:**
   ```
   Right-click in Project window
   → Create → Input Actions
   → Name: MobileInputActions
   ```

2. **Configure Touch action map:**
   - Double-click `MobileInputActions` to open Input Actions window
   - Click `+` next to "Action Maps"
   - Name: `Touch`
   - Set as default (star icon)

3. **Add TouchPress action:**
   - Click `+` next to Actions (under Touch map)
   - Name: `TouchPress`
   - Action Type: `Button`
   - Click `+` next to `<No Binding>`
   - Select binding: `Touchscreen → Primary Touch → Press`

4. **Add TouchPosition action:**
   - Click `+` next to Actions
   - Name: `TouchPosition`
   - Action Type: `Value`
   - Control Type: `Vector2`
   - Click `+` next to `<No Binding>`
   - Select binding: `Touchscreen → Primary Touch → Position`

5. **Add mouse support for editor testing:**
   - Click TouchPress → `+` next to binding
   - Select: `Mouse → Left Button`
   - Click TouchPosition → `+` next to binding
   - Select: `Mouse → Position`

6. **Generate C# class:**
   - Click "Generate C# Class" checkbox
   - Set Class Name: `MobileInputActions`
   - Set Namespace: (your project namespace or leave empty)
   - Apply

7. **Save:**
   - Click "Save Asset" button
   - Close Input Actions window

#### 1.2 Verify Input Actions

Open the generated C# file (`MobileInputActions.cs`). You should see:
```csharp
public class MobileInputActions : IInputActionCollection2, IDisposable
{
    public TouchActions Touch => new TouchActions(this);

    public struct TouchActions
    {
        public InputAction TouchPress => m_Wrapper.m_Touch_TouchPress;
        public InputAction TouchPosition => m_Wrapper.m_Touch_TouchPosition;
    }
}
```

### Phase 2: Copy and Setup MobileInputController (5 minutes)

#### 2.1 Copy Template to Project

```bash
# Navigate to your Unity project root
cd /path/to/your/UnityProject

# Create Input folder if it doesn't exist
mkdir -p Assets/_Project/Scripts/Input

# Copy the template
cp .claude/skills/mobile-input-controller/assets/templates/MobileInputController.cs \
   Assets/_Project/Scripts/Input/
```

Or manually:
1. Open `.claude/skills/mobile-input-controller/assets/templates/MobileInputController.cs`
2. Copy all content
3. Create new C# script in Unity: `Assets/_Project/Scripts/Input/MobileInputController.cs`
4. Paste content
5. Save

#### 2.2 Create Player GameObject

1. **Create GameObject:**
   - In Hierarchy: Right-click → Create Empty
   - Name: `Player`
   - Reset Transform (position 0,0,0)

2. **Add MobileInputController:**
   - Select `Player`
   - Add Component → `Mobile Input Controller`
   - This automatically adds `PlayerInput` component

3. **Configure PlayerInput:**
   - Actions: Drag `MobileInputActions` asset
   - Default Map: Select `Touch`
   - Behavior: `Send Messages` (default is fine)
   - Camera: Leave empty (will auto-find Camera.main)

4. **Configure MobileInputController:**
   - Min Touch Hold Time: `0.1`
   - Max Touch Hold Time: `1.0` (adjust for your game)
   - Enable Touch Feedback: ✓ (checked)
   - Enable Debug Logging: ✓ (for testing, uncheck later)

#### 2.3 Create Touch Indicator (Optional but Recommended)

1. **Create indicator prefab:**
   ```
   Hierarchy: Right-click → UI → Canvas
   Name: TouchIndicatorCanvas
   Render Mode: Screen Space - Overlay
   ```

2. **Create circle image:**
   ```
   Right-click TouchIndicatorCanvas → UI → Image
   Name: TouchIndicator
   Color: Yellow with Alpha 0.5
   Width: 50, Height: 50
   ```

3. **Make it a prefab:**
   ```
   Drag TouchIndicator from Hierarchy to Project window
   Delete TouchIndicator from scene (keep Canvas)
   ```

4. **Assign to controller:**
   - Select `InputManager`
   - Touch Indicator Prefab: Drag `TouchIndicator` prefab
   - Touch Canvas: Drag `TouchIndicatorCanvas` from Hierarchy

### Phase 3: Connect to Gameplay (10-15 minutes)

#### 3.1 Create a Test Script

Create `Assets/_Project/Scripts/Player/PlayerJumpTest.cs`:

```csharp
using UnityEngine;

public class PlayerJumpTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MobileInputController inputController;
    [SerializeField] private Rigidbody playerRb;

    [Header("Jump Settings")]
    [SerializeField] private float minJumpForce = 5f;
    [SerializeField] private float maxJumpForce = 15f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private void OnEnable()
    {
        // Subscribe to touch events
        if (inputController != null)
        {
            inputController.OnTouchStartedEventHandler += OnTouchStarted;
            inputController.OnTouchReleasedEventHandler += OnTouchReleased;
        }
    }

    private void OnDisable()
    {
        // IMPORTANT: Unsubscribe to prevent memory leaks
        if (inputController != null)
        {
            inputController.OnTouchStartedEventHandler -= OnTouchStarted;
            inputController.OnTouchReleasedEventHandler -= OnTouchReleased;
        }
    }

    private void OnTouchStarted(float normalizedTime)
    {
        if (showDebugInfo)
        {
            Debug.Log("[PlayerJumpTest] Touch started - preparing to jump!");
        }
    }

    private void OnTouchReleased(float normalizedHoldTime)
    {
        // Calculate jump force based on hold time
        float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, normalizedHoldTime);

        // Apply jump force
        if (playerRb != null)
        {
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (showDebugInfo)
        {
            Debug.Log($"[PlayerJumpTest] Jump! Hold time: {normalizedHoldTime:F2} (0-1), Force: {jumpForce:F1}");
        }
    }
}
```

#### 3.2 Setup Test Scene

1. **Create player:**
   ```
   Create → 3D Object → Capsule
   Name: Player
   Add Component → Rigidbody
   ```

2. **Add test script:**
   ```
   Add Component → Player Jump Test
   Input Controller: Drag InputManager
   Player Rb: Drag Player's Rigidbody
   ```

3. **Create ground:**
   ```
   Create → 3D Object → Plane
   Name: Ground
   Position: (0, -1, 0)
   Scale: (5, 1, 5)
   ```

4. **Test:**
   - Press Play
   - Click and hold (in editor with mouse)
   - Release → Player should jump
   - Check Console for debug logs

---

## Project Structure After Implementation

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Input/
│   │   │   ├── MobileInputController.cs          ← Copied from skill
│   │   │   └── MobileInputActions.cs              ← Generated by Input System
│   │   ├── Player/
│   │   │   ├── PlayerJumpTest.cs                  ← Your test script
│   │   │   ├── PlayerMovement.cs                  ← Your actual player script
│   │   │   └── PlayerAttack.cs                    ← Example
│   │   └── UI/
│   │       └── ChargeIndicator.cs                 ← Example UI
│   ├── Input/
│   │   └── MobileInputActions.inputactions        ← Input Actions asset
│   ├── Prefabs/
│   │   ├── TouchIndicator.prefab                  ← Visual feedback
│   │   └── InputManager.prefab                    ← Make prefab for reuse
│   └── Scenes/
│       ├── Main.unity
│       └── TestInput.unity
└── Plugins/
    └── (if using any input-related plugins)
```

---

## Implementation Patterns

### Pattern 1: Event-Based (One-Time Actions)

**Use for:** Jump, shoot, tap-to-interact, button-like mechanics

```csharp
public class EventBasedExample : MonoBehaviour
{
    [SerializeField] private MobileInputController input;

    private void OnEnable()
    {
        input.OnTouchStartedEventHandler += HandleTouchStart;
        input.OnTouchReleasedEventHandler += HandleTouchRelease;
    }

    private void OnDisable()
    {
        input.OnTouchStartedEventHandler -= HandleTouchStart;
        input.OnTouchReleasedEventHandler -= HandleTouchRelease;
    }

    private void HandleTouchStart(float normalizedTime)
    {
        // Instant feedback (normalizedTime is always 0)
        PlaySound("touchStart");
        ShowEffect();
    }

    private void HandleTouchRelease(float normalizedHoldTime)
    {
        // Action based on hold duration
        if (normalizedHoldTime < 0.2f)
        {
            LightAttack();
        }
        else
        {
            HeavyAttack(normalizedHoldTime);
        }
    }
}
```

### Pattern 2: Polling-Based (Continuous State)

**Use for:** Charging, aiming, power meters, continuous effects

```csharp
public class PollingBasedExample : MonoBehaviour
{
    [SerializeField] private MobileInputController input;
    [SerializeField] private Image chargeBar;

    private void Update()
    {
        if (input.IsTouchHeld)
        {
            // Get current charge level
            float charge = input.GetCurrentNormalizedHoldTime();

            // Update visuals
            chargeBar.fillAmount = charge;
            chargeBar.color = Color.Lerp(Color.white, Color.red, charge);

            // Trigger effects at thresholds
            if (charge >= 1.0f)
            {
                PlayMaxChargeEffect();
            }
        }
        else
        {
            // Reset when not held
            chargeBar.fillAmount = 0;
        }
    }
}
```

### Pattern 3: Hybrid (Events + Polling)

**Use for:** Complex mechanics that need both instant feedback and continuous updates

```csharp
public class HybridExample : MonoBehaviour
{
    [SerializeField] private MobileInputController input;
    [SerializeField] private ParticleSystem chargeEffect;

    private bool isCharging = false;

    private void OnEnable()
    {
        input.OnTouchStartedEventHandler += OnChargeStart;
        input.OnTouchReleasedEventHandler += OnChargeRelease;
    }

    private void OnDisable()
    {
        input.OnTouchStartedEventHandler -= OnChargeStart;
        input.OnTouchReleasedEventHandler -= OnChargeRelease;
    }

    private void OnChargeStart(float _)
    {
        // Event: Start visual effect immediately
        isCharging = true;
        chargeEffect.Play();
        PlaySound("chargeStart");
    }

    private void Update()
    {
        // Polling: Update effect intensity while charging
        if (isCharging)
        {
            float intensity = input.GetCurrentNormalizedHoldTime();
            var emission = chargeEffect.emission;
            emission.rateOverTime = intensity * 100f;
        }
    }

    private void OnChargeRelease(float normalizedHoldTime)
    {
        // Event: Fire the charged attack
        isCharging = false;
        chargeEffect.Stop();

        float damage = Mathf.Lerp(10f, 100f, normalizedHoldTime);
        FireAttack(damage);
    }
}
```

---

## Integration Scenarios

### Scenario 1: Adding to Existing Player Controller

**Challenge:** You already have a player controller using old Input Manager.

**Solution:**

1. **Keep existing code** - Don't delete your current input handling yet
2. **Add MobileInputController as parallel system:**

```csharp
public class ExistingPlayerController : MonoBehaviour
{
    [Header("Legacy Input")]
    [SerializeField] private bool useLegacyInput = false;

    [Header("Mobile Input")]
    [SerializeField] private MobileInputController mobileInput;
    [SerializeField] private bool useMobileInput = true;

    private void Update()
    {
        // Old input (for testing)
        if (useLegacyInput && Input.GetKeyDown(KeyCode.Space))
        {
            Jump(1.0f); // Full power
        }
    }

    private void OnEnable()
    {
        if (useMobileInput && mobileInput != null)
        {
            mobileInput.OnTouchReleasedEventHandler += OnMobileJump;
        }
    }

    private void OnDisable()
    {
        if (useMobileInput && mobileInput != null)
        {
            mobileInput.OnTouchReleasedEventHandler -= OnMobileJump;
        }
    }

    private void OnMobileJump(float holdTime)
    {
        Jump(holdTime);
    }

    private void Jump(float power)
    {
        // Your existing jump logic
        float force = Mathf.Lerp(minJumpForce, maxJumpForce, power);
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }
}
```

3. **Test both systems** - Toggle `useLegacyInput` and `useMobileInput` in Inspector
4. **Gradually migrate** - Once mobile input works, remove legacy code

### Scenario 2: Multiple Input Controllers (Different Zones)

**Challenge:** Left side of screen for movement, right side for attacking.

**Solution:**

Create two Input Managers with different screen regions:

```csharp
public class DualZoneInput : MonoBehaviour
{
    [SerializeField] private MobileInputController leftZoneInput;
    [SerializeField] private MobileInputController rightZoneInput;

    private void OnEnable()
    {
        leftZoneInput.OnTouchReleasedEventHandler += OnLeftZoneTouch;
        rightZoneInput.OnTouchReleasedEventHandler += OnRightZoneTouch;
    }

    private void OnDisable()
    {
        leftZoneInput.OnTouchReleasedEventHandler -= OnLeftZoneTouch;
        rightZoneInput.OnTouchReleasedEventHandler -= OnRightZoneTouch;
    }

    private void OnLeftZoneTouch(float holdTime)
    {
        // Handle movement
        Vector2 touchPos = leftZoneInput.GetCurrentTouchPosition();
        MoveToPosition(touchPos);
    }

    private void OnRightZoneTouch(float holdTime)
    {
        // Handle attack
        Attack(holdTime);
    }
}
```

### Scenario 3: UI Integration (Pause During Touch)

**Challenge:** Don't want game input during UI interactions.

**Solution:**

```csharp
public class InputWithUIBlocker : MonoBehaviour
{
    [SerializeField] private MobileInputController input;

    private void OnEnable()
    {
        input.OnTouchStartedEventHandler += OnTouchStart;
    }

    private void OnDisable()
    {
        input.OnTouchStartedEventHandler -= OnTouchStart;
    }

    private void OnTouchStart(float _)
    {
        // Check if touch is over UI
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Touch is over UI - ignoring game input");
            return;
        }

        // Safe to process game input
        ProcessGameInput();
    }
}
```

---

## Testing and Debugging Workflow

### Step 1: Enable Debug Logging

In Inspector:
- Select `InputManager`
- MobileInputController → Enable Debug Logging: ✓

You'll see console logs:
```
[MobileInputController] Touch started at (540.0, 960.0)
[MobileInputController] Touch released after 0.75s (normalized: 0.38)
```

### Step 2: Test in Editor (Mouse Simulation)

The Input Actions are configured to support mouse for testing:
- **Left Click** = Touch Press
- **Mouse Position** = Touch Position

**Test workflow:**
1. Press Play
2. Click and hold mouse
3. Watch Console for logs
4. Watch scene for touch indicator
5. Release mouse
6. Verify action triggered (jump, attack, etc.)

### Step 3: Test on Device (Unity Remote)

**Setup Unity Remote:**
1. Install "Unity Remote 5" on your phone (App Store / Play Store)
2. Connect phone via USB
3. Unity: Edit → Project Settings → Editor → Unity Remote → Device: Any Android/iOS Device
4. Launch Unity Remote app on phone
5. Press Play in Unity Editor
6. Touch phone screen → see result in Editor

**Benefits:**
- No need to build
- Instant iteration
- Real touch testing

### Step 4: Build and Test on Device

**For final testing:**

1. **Build Settings:**
   ```
   File → Build Settings
   Platform: iOS or Android
   Switch Platform
   Add Open Scenes
   ```

2. **Player Settings:**
   ```
   Player Settings → Other Settings
   - Package Name: com.yourcompany.yourgame
   - Minimum API Level: 24 (Android) or iOS 12.0
   ```

3. **Build and Run:**
   - Connect device
   - Click "Build and Run"
   - Wait for install
   - Test touch on device

### Common Debug Checks

| Issue | Check | Solution |
|-------|-------|----------|
| No touch detected | Enable Debug Logging | Check Console for "Touch started" logs |
| Events not firing | Check event subscription | Verify OnEnable/OnDisable subscriptions |
| Wrong hold time | Check min/max hold time | Adjust in Inspector (default: 0.1s - 2.0s) |
| Indicator not showing | Check prefab assignment | Assign TouchIndicator prefab and Canvas |
| Input not working on device | Check Input System package | Verify package installed and active |
| Multiple touches conflict | Check touch ID | Use primaryTouch binding (default) |

---

## Deployment Checklist

### Pre-Build Checklist

- [ ] Disable debug logging on MobileInputController
- [ ] Test on both iOS and Android devices
- [ ] Verify touch targets are 44x44pt minimum (iOS) or 48x48dp (Android)
- [ ] Test on low-end device (3-year-old phones)
- [ ] Check frame rate while touching (should stay 60fps)
- [ ] Verify touch works in all orientations (if supported)
- [ ] Test with UI overlays (pause menu, HUD)
- [ ] Confirm touch indicators don't block important UI

### Build Settings

**Android:**
```
Player Settings → Other Settings
- Minimum API Level: 24 (Android 7.0)
- Target API Level: Latest
- Scripting Backend: IL2CPP (for 64-bit)
```

**iOS:**
```
Player Settings → Other Settings
- Target minimum iOS Version: 12.0
- Architecture: ARM64
```

### Post-Build Testing

- [ ] Install on 3+ different devices
- [ ] Test different screen sizes (small phone, tablet)
- [ ] Test different aspect ratios (18:9, 19.5:9, etc.)
- [ ] Verify no performance issues
- [ ] Check battery drain (shouldn't be significant)
- [ ] Test with gloves (if applicable)
- [ ] Test in different lighting (screen visibility)

---

## Common Implementation Pitfalls

### Pitfall 1: Forgetting to Unsubscribe from Events

**Problem:**
```csharp
private void OnEnable()
{
    input.OnTouchReleasedEventHandler += OnJump;
}
// Missing OnDisable - memory leak!
```

**Solution:**
```csharp
private void OnEnable()
{
    input.OnTouchReleasedEventHandler += OnJump;
}

private void OnDisable()
{
    input.OnTouchReleasedEventHandler -= OnJump; // IMPORTANT!
}
```

**Why it matters:** Memory leaks, events firing on destroyed objects, crashes.

### Pitfall 2: Using Touch Position Wrong

**Problem:**
```csharp
Vector2 touchPos = input.GetCurrentTouchPosition();
transform.position = touchPos; // WRONG! Screen space vs World space
```

**Solution:**
```csharp
Vector2 screenPos = input.GetCurrentTouchPosition();
Vector3 worldPos = Camera.main.ScreenToWorldPoint(
    new Vector3(screenPos.x, screenPos.y, 10f)
);
transform.position = worldPos; // Correct!
```

### Pitfall 3: Checking IsTouchHeld Wrong

**Problem:**
```csharp
if (input.IsTouchHeld == true) // Redundant comparison
{
    float time = input.GetCurrentNormalizedHoldTime();
}
```

**Solution:**
```csharp
if (input.IsTouchHeld) // Cleaner
{
    float time = input.GetCurrentNormalizedHoldTime();
}
```

### Pitfall 4: Not Handling Max Hold Time

**Problem:** Assuming touch can be held infinitely.

**Solution:** Touch auto-releases at `maxTouchHoldTime`. Design mechanics around this:
```csharp
private void OnTouchReleased(float holdTime)
{
    if (holdTime >= 1.0f)
    {
        // Max charge reached (auto-released or manual)
        FireMaxPowerAttack();
    }
    else
    {
        // Partial charge
        FireNormalAttack(holdTime);
    }
}
```

### Pitfall 5: Touch Over UI Not Blocked

**Problem:** Touch goes through UI buttons to game world.

**Solution:**
```csharp
private void OnTouchStarted(float _)
{
    // Always check this first!
    if (EventSystem.current.IsPointerOverGameObject())
    {
        return; // Touch is on UI, ignore game input
    }

    ProcessGameInput();
}
```

---

## Real-World Examples

### Example 1: Flappy Bird Clone

```csharp
public class FlappyBirdController : MonoBehaviour
{
    [SerializeField] private MobileInputController input;
    [SerializeField] private Rigidbody2D birdRb;
    [SerializeField] private float flapForce = 5f;
    [SerializeField] private AudioClip flapSound;

    private void OnEnable()
    {
        input.OnTouchStartedEventHandler += OnFlap;
    }

    private void OnDisable()
    {
        input.OnTouchStartedEventHandler -= OnFlap;
    }

    private void OnFlap(float _)
    {
        // Reset velocity
        birdRb.velocity = Vector2.zero;

        // Apply upward force
        birdRb.AddForce(Vector2.up * flapForce, ForceMode2D.Impulse);

        // Play sound
        AudioSource.PlayClipAtPoint(flapSound, transform.position);

        // Rotate bird upward
        transform.rotation = Quaternion.Euler(0, 0, 30f);
    }
}
```

### Example 2: Angry Birds Slingshot

```csharp
public class SlingshotController : MonoBehaviour
{
    [SerializeField] private MobileInputController input;
    [SerializeField] private LineRenderer trajectory;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private float maxPower = 20f;

    private bool isAiming = false;

    private void OnEnable()
    {
        input.OnTouchStartedEventHandler += OnStartAim;
        input.OnTouchReleasedEventHandler += OnLaunch;
    }

    private void OnDisable()
    {
        input.OnTouchStartedEventHandler -= OnStartAim;
        input.OnTouchReleasedEventHandler -= OnLaunch;
    }

    private void OnStartAim(float _)
    {
        isAiming = true;
        trajectory.enabled = true;
    }

    private void Update()
    {
        if (isAiming && input.IsTouchHeld)
        {
            // Get current power
            float power = input.GetCurrentNormalizedHoldTime();

            // Show trajectory
            DrawTrajectory(power);
        }
    }

    private void OnLaunch(float normalizedPower)
    {
        isAiming = false;
        trajectory.enabled = false;

        // Spawn projectile
        GameObject projectile = Instantiate(projectilePrefab, launchPoint.position, Quaternion.identity);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        // Calculate launch force
        Vector3 direction = (launchPoint.position - transform.position).normalized;
        float force = normalizedPower * maxPower;

        // Launch!
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void DrawTrajectory(float power)
    {
        // Simplified trajectory visualization
        Vector3 velocity = transform.up * power * maxPower;

        for (int i = 0; i < trajectory.positionCount; i++)
        {
            float t = i * 0.1f;
            Vector3 point = launchPoint.position + velocity * t + 0.5f * Physics.gravity * t * t;
            trajectory.SetPosition(i, point);
        }
    }
}
```

### Example 3: RPG Charge Attack

```csharp
public class RPGChargeAttack : MonoBehaviour
{
    [SerializeField] private MobileInputController input;
    [SerializeField] private ParticleSystem[] chargeLevelEffects; // 3 levels
    [SerializeField] private float[] chargeLevelDurations = { 0.5f, 1.0f, 2.0f };

    private int currentChargeLevel = 0;

    private void OnEnable()
    {
        input.OnTouchStartedEventHandler += OnChargeStart;
        input.OnTouchReleasedEventHandler += OnChargeRelease;
    }

    private void OnDisable()
    {
        input.OnTouchStartedEventHandler -= OnChargeStart;
        input.OnTouchReleasedEventHandler -= OnChargeRelease;
    }

    private void OnChargeStart(float _)
    {
        currentChargeLevel = 0;
        StopAllEffects();
    }

    private void Update()
    {
        if (input.IsTouchHeld)
        {
            float duration = input.GetCurrentHoldDuration();

            // Determine charge level
            int newLevel = 0;
            if (duration >= chargeLevelDurations[2]) newLevel = 2;
            else if (duration >= chargeLevelDurations[1]) newLevel = 1;
            else if (duration >= chargeLevelDurations[0]) newLevel = 0;

            // Update effects if level changed
            if (newLevel != currentChargeLevel)
            {
                currentChargeLevel = newLevel;
                UpdateChargeEffects();
            }
        }
    }

    private void OnChargeRelease(float normalizedHoldTime)
    {
        StopAllEffects();

        // Execute attack based on charge level
        switch (currentChargeLevel)
        {
            case 0:
                ExecuteLightAttack();
                break;
            case 1:
                ExecuteMediumAttack();
                break;
            case 2:
                ExecuteHeavyAttack();
                break;
        }
    }

    private void UpdateChargeEffects()
    {
        for (int i = 0; i < chargeLevelEffects.Length; i++)
        {
            if (i <= currentChargeLevel)
            {
                if (!chargeLevelEffects[i].isPlaying)
                    chargeLevelEffects[i].Play();
            }
            else
            {
                chargeLevelEffects[i].Stop();
            }
        }
    }

    private void StopAllEffects()
    {
        foreach (var effect in chargeLevelEffects)
        {
            effect.Stop();
        }
    }

    private void ExecuteLightAttack() => Debug.Log("Light Attack!");
    private void ExecuteMediumAttack() => Debug.Log("Medium Attack!");
    private void ExecuteHeavyAttack() => Debug.Log("Heavy Attack!");
}
```

---

## Next Steps

After implementing the mobile-input-controller:

1. **Test thoroughly** on real devices
2. **Tune parameters** (min/max hold times) for your game feel
3. **Add juice** - animations, particles, screen shake on touch
4. **Profile performance** - ensure 60fps on target devices
5. **Iterate on UX** - get player feedback on touch responsiveness
6. **Consider advanced features:**
   - Multi-touch support (two-finger gestures)
   - Swipe detection
   - Virtual joystick integration
   - Haptic feedback (vibration)

---

## Additional Resources

- **SKILL.md** - Complete API reference
- **mobile-input-best-practices.md** - Performance and UX guidelines
- **Template Files:**
  - `MobileInputController.cs` - Base template
  - `PlayerJumpController.cs` - Event-based example
  - `ChargeAttackController.cs` - Polling-based example

---

## Support and Feedback

If you encounter issues not covered here:
1. Check SKILL.md for API details
2. Review examples in `assets/examples/`
3. Enable debug logging for detailed console output
4. Test with Unity Remote before building

**Version History:**
- v1.0 (2025-01-18) - Initial implementation notes

---

**Happy implementing! 🎮**
