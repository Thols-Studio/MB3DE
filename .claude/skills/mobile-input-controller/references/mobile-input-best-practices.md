# Mobile Input Best Practices for Unity

## Table of Contents
1. [Touch UX Guidelines](#touch-ux-guidelines)
2. [Performance Optimization](#performance-optimization)
3. [Unity Input System Configuration](#unity-input-system-configuration)
4. [Testing on Mobile Devices](#testing-on-mobile-devices)
5. [Common Pitfalls](#common-pitfalls)

---

## Touch UX Guidelines

### Touch Target Sizing

**Minimum Touch Sizes:**
- **iOS:** 44x44 points minimum (Apple Human Interface Guidelines)
- **Android:** 48x48 dp minimum (Material Design Guidelines)
- **Recommended:** 60x60 pixels or larger for comfortable tapping

**Implementation:**
```csharp
// Ensure touch hitbox is large enough
[SerializeField] private Vector2 touchTargetSize = new Vector2(60f, 60f);

private bool IsTouchInTarget(Vector2 touchPos, Vector2 targetPos)
{
    return Vector2.Distance(touchPos, targetPos) <= touchTargetSize.x / 2f;
}
```

### Visual Feedback Timing

**Response Times:**
- **Instant (0ms):** Touch down highlight
- **Quick (<100ms):** Button press feedback
- **Normal (100-300ms):** Loading states, transitions
- **Long (>300ms):** Show progress indicator

**Example:**
```csharp
private IEnumerator ShowTouchFeedback()
{
    // Instant highlight on touch
    buttonImage.color = pressedColor;

    // Short vibration (haptic feedback)
    Handheld.Vibrate();

    // Wait for visual feedback duration
    yield return new WaitForSeconds(0.1f);

    // Return to normal state
    buttonImage.color = normalColor;
}
```

### Touch Hold Duration Tuning

**Recommended Values:**
```csharp
// Prevent accidental taps
[SerializeField] private float minTouchHoldTime = 0.1f;

// Maximum hold time for mechanics
// - Quick actions (tap): 0.3s
// - Medium charge: 1.0s
// - Long charge: 2-3s
[SerializeField] private float maxTouchHoldTime = 1.0f;
```

### Touch Tolerance and Drift

**Allow Small Finger Movement:**
```csharp
[SerializeField] private float touchDriftThreshold = 10f; // pixels

private bool HasFingerDrifted(Vector2 startPos, Vector2 currentPos)
{
    float drift = Vector2.Distance(startPos, currentPos);
    return drift > touchDriftThreshold;
}

// Cancel action if finger moves too far
if (HasFingerDrifted(touchStartPosition, currentTouchPosition))
{
    CancelCurrentAction();
}
```

**Why:** Fingers naturally shift during holds. Allow 5-10 pixel drift before canceling.

### Multi-Touch Considerations

**Prevent Unintended Multi-Touch:**
```csharp
// Ignore new touches while one is active
if (isTouchHeld) return;

// Or track touches by ID
private Dictionary<int, TouchData> activeTouches = new Dictionary<int, TouchData>();
```

**UI Overlay Protection:**
```csharp
using UnityEngine.EventSystems;

// Don't register touch if over UI
if (EventSystem.current.IsPointerOverGameObject(touchId))
{
    return; // Ignore touch
}
```

---

## Performance Optimization

### Object Pooling for Touch Indicators

**Problem:** Instantiate/Destroy creates garbage, causes frame drops.

**Solution:** Use object pooling.

```csharp
using UnityEngine.Pool;

public class TouchIndicatorPool : MonoBehaviour
{
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private int poolSize = 10;

    private ObjectPool<GameObject> pool;

    private void Awake()
    {
        pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(indicatorPrefab),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: false,
            defaultCapacity: poolSize,
            maxSize: poolSize * 2
        );
    }

    public GameObject GetIndicator()
    {
        return pool.Get();
    }

    public void ReleaseIndicator(GameObject indicator)
    {
        pool.Release(indicator);
    }
}
```

### Cache Component References

**❌ BAD: Find/GetComponent every frame**
```csharp
private void Update()
{
    // 10x slower than caching!
    var rb = GetComponent<Rigidbody>();
    rb.AddForce(Vector3.up);
}
```

**✅ GOOD: Cache in Awake**
```csharp
private Rigidbody rb;

private void Awake()
{
    rb = GetComponent<Rigidbody>();
}

private void Update()
{
    rb.AddForce(Vector3.up);
}
```

### Minimize Update Allocations

**❌ BAD: Allocates garbage every frame**
```csharp
private void Update()
{
    Vector2 pos = new Vector2(x, y); // GC allocation!
    transform.position = pos;
}
```

**✅ GOOD: Reuse variables**
```csharp
private Vector2 touchPosition; // Field, allocated once

private void Update()
{
    touchPosition.x = x;
    touchPosition.y = y;
    transform.position = touchPosition;
}
```

### Optimize Touch Position Queries

**Use cached values instead of repeated queries:**
```csharp
// Cache touch position at start of Update
private void Update()
{
    if (inputController.IsTouchHeld)
    {
        Vector2 cachedPos = inputController.GetCurrentTouchPosition();

        // Use cachedPos multiple times without re-querying
        UpdateAimDirection(cachedPos);
        UpdateCrosshair(cachedPos);
        UpdateTrajectory(cachedPos);
    }
}
```

---

## Unity Input System Configuration

### Input Actions Asset Setup

**Create Touch Action Map:**
```
Input Actions Asset: MobileInputActions
├── Touch (Action Map)
│   ├── TouchPress (Action)
│   │   ├── Type: Button
│   │   ├── Binding: <Touchscreen>/primaryTouch/press
│   │   └── Interactions: (none)
│   │
│   ├── TouchPosition (Action)
│   │   ├── Type: Value (Vector2)
│   │   ├── Binding: <Touchscreen>/primaryTouch/position
│   │   └── Control Type: Vector2
│   │
│   └── TouchDelta (Action) [optional]
│       ├── Type: Value (Vector2)
│       ├── Binding: <Touchscreen>/primaryTouch/delta
│       └── Control Type: Vector2
```

### PlayerInput Component Configuration

**Settings:**
```
PlayerInput Component:
├── Actions: MobileInputActions (assign asset)
├── Default Action Map: Touch
├── Behavior: Send Messages (or Invoke Unity Events)
├── Camera: Main Camera (optional)
└── UI Input Module: None (or assign for UI)
```

### Supporting Mouse for Editor Testing

**Add Mouse Bindings:**
```
TouchPress Action:
├── Binding 1: <Touchscreen>/primaryTouch/press
└── Binding 2: <Mouse>/leftButton

TouchPosition Action:
├── Binding 1: <Touchscreen>/primaryTouch/position
└── Binding 2: <Mouse>/position
```

**Or use conditional compilation:**
```csharp
#if UNITY_EDITOR
    // Simulate touch with mouse in editor
    if (Input.GetMouseButtonDown(0))
    {
        Vector2 mousePos = Input.mousePosition;
        SimulateTouchStart(mousePos);
    }
#endif
```

---

## Testing on Mobile Devices

### Unity Remote for Quick Testing

**Setup:**
1. Download Unity Remote 5 on device (App Store/Play Store)
2. Connect device to PC via USB
3. Unity → Edit → Project Settings → Editor → Device: [Your Device]
4. Run game in editor - input streams from device

**Limitations:**
- Compressed video feed (laggy)
- Not representative of final performance
- Good for quick input testing only

### Building for Device

**Best Practice: Test on Real Hardware**
```bash
# Android build
Unity -quit -batchmode -buildTarget Android -executeMethod BuildScript.BuildAndroid

# iOS build (requires macOS)
Unity -quit -batchmode -buildTarget iOS -executeMethod BuildScript.BuildiOS
```

**Test on Multiple Devices:**
- Low-end: Minimum spec device
- Mid-range: Popular mid-tier device
- High-end: Flagship device
- Different screen sizes: Phone, tablet, foldable

### Performance Profiling on Device

**Unity Profiler over WiFi:**
```csharp
// In build settings, enable "Development Build" + "Autoconnect Profiler"
// Connect profiler: Window → Analysis → Profiler → [Device Name]
```

**Check:**
- Frame rate (target 60 FPS)
- GC allocations (minimize to zero in Update)
- Touch latency (< 50ms ideal)
- Battery drain (monitor temperature)

---

## Common Pitfalls

### Pitfall 1: Not Unsubscribing from Events

**❌ PROBLEM: Memory Leak**
```csharp
private void OnEnable()
{
    inputController.OnTouchStarted += HandleTouch;
}

// Missing OnDisable! Event never unsubscribed
```

**✅ SOLUTION: Always Unsubscribe**
```csharp
private void OnEnable()
{
    inputController.OnTouchStarted += HandleTouch;
}

private void OnDisable()
{
    inputController.OnTouchStarted -= HandleTouch;
}
```

### Pitfall 2: Using Unity Null Check with ?. Operator

**❌ PROBLEM: Crashes on Destroyed Objects**
```csharp
// Unity's "fake null" breaks null-conditional operator
myGameObject?.SetActive(true); // Crashes if destroyed!
```

**✅ SOLUTION: Use Unity's == Operator**
```csharp
if (myGameObject != null)
{
    myGameObject.SetActive(true);
}
```

### Pitfall 3: Touch Input in Wrong Lifecycle Method

**❌ PROBLEM: Missed Input**
```csharp
// Input should be in Update, not FixedUpdate
private void FixedUpdate()
{
    if (inputController.IsTouchHeld) { ... } // May miss quick taps!
}
```

**✅ SOLUTION: Input in Update, Physics in FixedUpdate**
```csharp
private void Update()
{
    // Check input every frame
    if (inputController.IsTouchHeld)
    {
        shouldJump = true;
    }
}

private void FixedUpdate()
{
    // Apply physics in fixed timestep
    if (shouldJump)
    {
        rb.AddForce(Vector3.up * jumpForce);
        shouldJump = false;
    }
}
```

### Pitfall 4: Not Handling Touch Over UI

**❌ PROBLEM: Gameplay responds to UI touches**
```csharp
// Touch on UI button also triggers jump
private void OnTouchStarted(float time)
{
    Jump(); // Oops! Jumped while clicking UI
}
```

**✅ SOLUTION: Check for UI Overlap**
```csharp
using UnityEngine.EventSystems;

private void OnTouchStarted(float time)
{
    // Ignore if touch is over UI
    if (EventSystem.current.IsPointerOverGameObject())
    {
        return;
    }

    Jump(); // Safe to jump
}
```

### Pitfall 5: Not Testing on Low-End Devices

**❌ PROBLEM: Works on PC, lags on phone**
```csharp
// Instantiate hundreds of particles
for (int i = 0; i < 100; i++)
{
    Instantiate(particlePrefab); // Destroys mobile performance
}
```

**✅ SOLUTION: Quality Settings + Optimization**
```csharp
// Use quality settings to reduce effects on mobile
int maxParticles = QualitySettings.GetQualityLevel() == 0 ? 10 : 100;

for (int i = 0; i < maxParticles; i++)
{
    pool.GetParticle(); // Use object pooling
}
```

---

## Checklist for Mobile Input Implementation

**Before Shipping:**
- [ ] Minimum touch target size: 60x60 pixels
- [ ] Visual feedback < 100ms
- [ ] All events properly unsubscribed
- [ ] No GC allocations in Update
- [ ] Component references cached
- [ ] Touch over UI handled
- [ ] Tested on 3+ device tiers
- [ ] Frame rate ≥ 60 FPS on mid-range device
- [ ] Touch latency < 50ms
- [ ] Object pooling for frequent spawns
- [ ] Quality settings for low-end devices

---

## Additional Resources

- [Unity Input System Documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)
- [Apple Human Interface Guidelines - Touch](https://developer.apple.com/design/human-interface-guidelines/inputs/touchscreen-gestures/)
- [Material Design - Touch Targets](https://m3.material.io/foundations/interaction/gestures)
- [Unity Mobile Optimization Guide](https://docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html)
