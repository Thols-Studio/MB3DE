# Player Movement Executor - Implementation Notes

**Version:** 1.0
**Last Updated:** 2025-01-19
**Skill:** player-movement-executor

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Prerequisites](#prerequisites)
3. [Step-by-Step Implementation](#step-by-step-implementation)
4. [Project Structure After Implementation](#project-structure-after-implementation)
5. [Implementation Patterns](#implementation-patterns)
6. [Integration Scenarios](#integration-scenarios)
7. [Testing Workflow](#testing-workflow)
8. [Common Implementation Pitfalls](#common-implementation-pitfalls)
9. [Real-World Examples](#real-world-examples)

---

## Quick Start

**Goal:** Get player movement working in under 5 minutes.

### Minimal Setup

1. **Copy template to your project:**
   ```bash
   cp .claude/skills/player-movement-executor/assets/templates/PlayerMovement.cs \
      Assets/_Project/Scripts/Player/
   ```

2. **Create player GameObject:**
   - Create → 3D Object → Capsule
   - Name: Player
   - Add Component → Player Movement

3. **Configure in Inspector:**
   - Forward Speed: 10
   - Horizontal Speed: 0.5
   - Max Horizontal Bounds: 5

4. **Create simple controller:**
   ```csharp
   using UnityEngine;

   public class SimpleController : MonoBehaviour {
       [SerializeField] private PlayerMovement movement;

       void Update() {
           float input = Input.GetAxis("Horizontal");
           movement.SetHorizontalInput(input);
       }
   }
   ```

5. **Add to player, assign reference, press Play!**

**Result:** Player moves forward automatically, use arrow keys to move left/right.

---

## Prerequisites

### Unity Version
- **Minimum:** Unity 2020.3 LTS
- **Recommended:** Unity 2022.3 LTS or newer

### Project Type
- 3D project
- Any render pipeline (Built-in, URP, HDRP)

### Knowledge Requirements
- Basic C# (classes, methods, variables)
- Unity MonoBehaviour lifecycle (Awake, Update)
- SerializeField and Inspector usage

### Optional (For Physics Version)
- Rigidbody component
- Basic physics understanding

---

## Step-by-Step Implementation

### Phase 1: Copy Template and Setup Player (5 minutes)

#### 1.1 Copy PlayerMovement.cs

**Option A: Command Line**
```bash
cd /path/to/YourUnityProject
mkdir -p Assets/_Project/Scripts/Player
cp .claude/skills/player-movement-executor/assets/templates/PlayerMovement.cs \
   Assets/_Project/Scripts/Player/
```

**Option B: Manual**
1. Open `.claude/skills/player-movement-executor/assets/templates/PlayerMovement.cs`
2. Copy all content
3. In Unity: Assets → Create → C# Script → `PlayerMovement`
4. Paste content, save

#### 1.2 Update Namespace

Open `PlayerMovement.cs` and change:
```csharp
namespace YourNamespace {  // Change this to your project namespace
```

To:
```csharp
namespace YourCompany.YourGame {  // Example: MyStudio.EndlessRunner
```

#### 1.3 Create Player GameObject

**Option A: 3D Capsule (Simple)**
```
Hierarchy → Right-click → 3D Object → Capsule
Name: Player
Position: (0, 1, 0)
```

**Option B: Import Your Model**
```
Drag your player model into scene
Add Capsule Collider if needed
```

#### 1.4 Add PlayerMovement Component

```
Select Player GameObject
Add Component → Player Movement
```

#### 1.5 Configure PlayerMovement

In Inspector, set:
- **Forward Speed:** 10 (adjust to taste)
- **Horizontal Speed:** 0.5 (lower = smoother)
- **Max Horizontal Bounds:** 5 (track width)
- **Show Debug Info:** ✓ (for testing)

### Phase 2: Create Controller (5 minutes)

#### 2.1 Create PlayerController Script

Create `Assets/_Project/Scripts/Player/PlayerController.cs`:

```csharp
using UnityEngine;

namespace YourCompany.YourGame {
    public class PlayerController : MonoBehaviour {
        [SerializeField] private PlayerMovement playerMovement;

        private void Awake() {
            if (playerMovement == null) {
                playerMovement = GetComponent<PlayerMovement>();
            }
        }

        private void Update() {
            // Get input
            float input = Input.GetAxis("Horizontal");

            // Send to movement
            playerMovement.SetHorizontalInput(input);
        }
    }
}
```

#### 2.2 Attach Controller to Player

```
Select Player GameObject
Add Component → Player Controller
Player Movement field should auto-fill (same GameObject)
```

#### 2.3 Test Basic Movement

```
Press Play
Use Arrow Keys or A/D to move left/right
Player should move forward automatically
Check Console for debug logs
```

### Phase 3: Add Ground and Camera (5 minutes)

#### 3.1 Create Ground

```
Create → 3D Object → Plane
Name: Ground
Position: (0, 0, 0)
Scale: (10, 1, 10)
```

#### 3.2 Setup Camera

**Option A: Simple Follow**
```
Select Main Camera
Position: (0, 5, -5)
Rotation: (30, 0, 0)
Set as child of Player (drag onto Player in Hierarchy)
```

**Option B: Smooth Follow (Better)**

Create `CameraFollow.cs`:
```csharp
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -5);
    [SerializeField] private float smoothSpeed = 0.125f;

    private void LateUpdate() {
        if (target == null) return;

        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed);
        transform.LookAt(target);
    }
}
```

Attach to Main Camera, assign Player as target.

### Phase 4: Add Visual Bounds (Optional)

#### 4.1 Create Boundary Markers

```csharp
// Add to PlayerMovement.cs for visualization
private void OnDrawGizmos() {
    Gizmos.color = Color.yellow;
    Vector3 left = transform.position + Vector3.left * maxHorizontalBounds;
    Vector3 right = transform.position + Vector3.right * maxHorizontalBounds;

    Gizmos.DrawLine(left + Vector3.up * 2, left - Vector3.up);
    Gizmos.DrawLine(right + Vector3.up * 2, right - Vector3.up);
}
```

You'll see yellow lines in Scene view showing movement bounds.

---

## Project Structure After Implementation

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Player/
│   │   │   ├── PlayerMovement.cs          ← Template
│   │   │   ├── PlayerController.cs        ← Your controller
│   │   │   └── PlayerHealth.cs            ← Example
│   │   ├── Camera/
│   │   │   └── CameraFollow.cs
│   │   └── Game/
│   │       └── GameManager.cs
│   ├── Prefabs/
│   │   └── Player.prefab                   ← Prefab your player
│   └── Scenes/
│       ├── Main.unity
│       └── TestMovement.unity
```

---

## Implementation Patterns

### Pattern 1: Keyboard Input (Desktop)

```csharp
public class KeyboardController : MonoBehaviour {
    [SerializeField] private PlayerMovement movement;

    void Update() {
        float input = Input.GetAxis("Horizontal");
        movement.SetHorizontalInput(input);
    }
}
```

### Pattern 2: Touch Input (Mobile)

```csharp
public class TouchController : MonoBehaviour {
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private float touchSensitivity = 0.01f;

    private Vector2 lastTouchPos;

    void Update() {
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved) {
                float delta = touch.deltaPosition.x * touchSensitivity;
                movement.SetHorizontalInput(Mathf.Clamp(delta, -1f, 1f));
            }
        } else {
            // Release input when not touching
            movement.SetHorizontalInput(0f);
        }
    }
}
```

### Pattern 3: Lane-Based Movement

```csharp
public class LaneController : MonoBehaviour {
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private float[] lanes = { -4f, 0f, 4f };

    private int currentLane = 1;

    void Update() {
        // Switch lanes
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            currentLane = Mathf.Max(0, currentLane - 1);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            currentLane = Mathf.Min(lanes.Length - 1, currentLane + 1);
        }

        // Calculate input to reach target lane
        float target = lanes[currentLane];
        float current = movement.GetHorizontalPosition();
        float input = Mathf.Clamp((target - current) * 5f, -1f, 1f);

        movement.SetHorizontalInput(input);
    }
}
```

---

## Integration Scenarios

### Scenario 1: Adding to Existing Player

**Challenge:** You already have a player script with movement.

**Solution:** Gradually migrate to executor pattern.

```csharp
public class ExistingPlayer : MonoBehaviour {
    // Old movement code
    [Header("Legacy (Disable when ready)")]
    [SerializeField] private bool useLegacyMovement = false;

    // New executor
    [Header("New Executor Pattern")]
    [SerializeField] private PlayerMovement newMovement;
    [SerializeField] private bool useExecutorPattern = true;

    void Update() {
        float input = Input.GetAxis("Horizontal");

        if (useLegacyMovement) {
            // Old code
            transform.position += Vector3.right * input * speed * Time.deltaTime;
        }

        if (useExecutorPattern && newMovement != null) {
            // New code
            newMovement.SetHorizontalInput(input);
        }
    }
}
```

Test both, then remove legacy code when confident.

### Scenario 2: Multiple Input Sources

**Challenge:** Support keyboard AND touch AND gamepad.

**Solution:** Input abstraction layer.

```csharp
public class MultiInputController : MonoBehaviour {
    [SerializeField] private PlayerMovement movement;

    void Update() {
        float input = GetInputFromAnySource();
        movement.SetHorizontalInput(input);
    }

    float GetInputFromAnySource() {
        // Keyboard
        if (Input.anyKey) {
            return Input.GetAxis("Horizontal");
        }

        // Touch
        if (Input.touchCount > 0) {
            return GetTouchInput();
        }

        // Gamepad
        if (Input.GetJoystickNames().Length > 0) {
            return Input.GetAxis("Horizontal");
        }

        return 0f;
    }

    float GetTouchInput() {
        Touch touch = Input.GetTouch(0);
        float screenCenter = Screen.width / 2f;
        return (touch.position.x - screenCenter) / screenCenter;
    }
}
```

---

## Testing Workflow

### Test 1: Basic Movement

**Steps:**
1. Press Play
2. Press Left Arrow → Player moves left
3. Press Right Arrow → Player moves right
4. Release → Player continues forward

**Expected:**
- Smooth left/right movement
- Forward movement constant
- No stuttering or jitter

### Test 2: Boundary Limits

**Steps:**
1. Hold Left Arrow until hitting left bound
2. Player should stop at `-maxHorizontalBounds`
3. Repeat for right side

**Expected:**
- Player stops at boundaries
- No weird behavior at edges
- Can still move away from edge

### Test 3: Speed Changes

**Steps:**
1. While playing, change Forward Speed in Inspector
2. Player speed should change immediately

**Expected:**
- Speed adjusts in real-time
- No sudden jumps in position

### Test 4: Movement Toggle

**Code:**
```csharp
// Add to controller
if (Input.GetKeyDown(KeyCode.Space)) {
    bool active = movement.IsMovementActive();
    movement.SetMovementActive(!active);
    Debug.Log($"Movement: {!active}");
}
```

**Expected:**
- Space toggles movement on/off
- When off, player freezes
- When on, resumes smoothly

---

## Common Implementation Pitfalls

### Pitfall 1: Forgetting Time.deltaTime

**Problem:**
```csharp
// WRONG - Frame-rate dependent
transform.position += velocity;
```

**Solution:**
```csharp
// RIGHT - Frame-rate independent
transform.position += velocity * Time.deltaTime;
```

### Pitfall 2: Not Caching Component Reference

**Problem:**
```csharp
void Update() {
    GetComponent<PlayerMovement>().SetHorizontalInput(input); // SLOW!
}
```

**Solution:**
```csharp
[SerializeField] private PlayerMovement movement; // Cached

void Update() {
    movement.SetHorizontalInput(input); // Fast
}
```

### Pitfall 3: Using Wrong Update Method

**Problem:**
```csharp
void Update() {
    rb.MovePosition(...); // WRONG - physics in Update
}
```

**Solution:**
```csharp
void FixedUpdate() {
    rb.MovePosition(...); // RIGHT - physics in FixedUpdate
}
```

### Pitfall 4: Bounds Too Tight/Wide

**Problem:** Player keeps hitting boundaries or has too much freedom.

**Solution:** Adjust `maxHorizontalBounds` to match your level design:
- Tight: 2-3 units
- Medium: 4-6 units
- Wide: 8-10 units

### Pitfall 5: Input and Movement in Same Script

**Problem:** Monolithic controller, hard to test or swap input.

**Solution:** Use executor pattern - controller handles input, executor handles movement.

---

## Real-World Examples

### Example 1: Endless Runner

```csharp
public class EndlessRunnerController : MonoBehaviour {
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private float startSpeed = 5f;
    [SerializeField] private float speedIncreaseRate = 0.1f;

    private float gameTime = 0f;

    void Start() {
        movement.SetForwardSpeed(startSpeed);
    }

    void Update() {
        // Increase speed over time
        gameTime += Time.deltaTime;
        float newSpeed = startSpeed + (gameTime * speedIncreaseRate);
        movement.SetForwardSpeed(newSpeed);

        // Handle input
        float input = Input.GetAxis("Horizontal");
        movement.SetHorizontalInput(input);
    }

    public void OnDeath() {
        movement.SetMovementActive(false);
    }
}
```

### Example 2: Racing Game

```csharp
public class RacingController : MonoBehaviour {
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private float normalSpeed = 20f;
    [SerializeField] private float boostSpeed = 40f;

    void Update() {
        // Steering
        float steering = Input.GetAxis("Horizontal");
        movement.SetHorizontalInput(steering);

        // Boost
        if (Input.GetKey(KeyCode.LeftShift)) {
            movement.SetForwardSpeed(boostSpeed);
        } else {
            movement.SetForwardSpeed(normalSpeed);
        }
    }
}
```

### Example 3: Tutorial/Guided Movement

```csharp
public class TutorialController : MonoBehaviour {
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Transform[] waypoints;

    private int currentWaypoint = 0;

    void Update() {
        if (currentWaypoint >= waypoints.Length) return;

        // Auto-navigate to waypoints
        Transform target = waypoints[currentWaypoint];
        float targetX = target.position.x;
        float currentX = movement.GetHorizontalPosition();

        float input = Mathf.Clamp(targetX - currentX, -1f, 1f);
        movement.SetHorizontalInput(input);

        // Check if reached
        if (Mathf.Abs(targetX - currentX) < 0.5f) {
            currentWaypoint++;
        }
    }
}
```

---

## Next Steps

After implementing player movement:

1. **Add obstacles** - Create objects to avoid
2. **Add collectibles** - Coins, power-ups, etc.
3. **Add animations** - Idle, run, turn animations
4. **Add visual effects** - Dust particles, speed lines
5. **Add sound effects** - Footsteps, wind, etc.
6. **Add UI** - Speed display, score, etc.
7. **Tune feel** - Adjust speeds and smoothing
8. **Test on device** - If targeting mobile
9. **Profile performance** - Use Unity Profiler
10. **Iterate based on feedback**

---

## Additional Resources

- **SKILL.md** - Complete API reference
- **movement-patterns.md** - Design patterns and formulas
- **Examples folder** - Complete implementations

---

**Happy implementing! 🎮**
