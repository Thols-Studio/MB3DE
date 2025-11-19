---
name: player-movement-executor
description: This skill helps create player movement systems in Unity using the Executor Pattern, where movement logic is separated from input handling. Use this skill when building modular player controllers with clean separation between input, logic, and physics. Ideal for endless runners, racing games, and any project requiring reusable movement components.
---

# Player Movement Executor

## Overview

This skill provides templates and patterns for implementing player movement in Unity using the **Executor Pattern**. This pattern separates input handling from movement execution, creating modular, testable, and reusable components.

**Key Benefit:** Your movement logic becomes independent of input source. The same movement component works with keyboard, gamepad, touch, AI, or even replay systems.

## Core Principles

1. **Separation of Concerns** - Input handling and movement execution are separate components
2. **Public API Design** - Movement component provides clean methods for controllers to call
3. **Testability** - Can test movement without input, or input without movement
4. **Reusability** - Same movement component works with different input sources
5. **Physics Ready** - Designed for easy Rigidbody integration
6. **Performance** - Efficient movement calculations, proper use of Update vs FixedUpdate

## When to Use This Skill

### Use this skill when:
- ✅ Building player controllers for 3D games (endless runners, racing, platformers)
- ✅ Need to separate input from movement logic
- ✅ Want to support multiple input types (keyboard, touch, AI)
- ✅ Building modular, testable player systems
- ✅ Need physics-based movement with Rigidbody
- ✅ Creating reusable movement components

### Don't use this skill when:
- ❌ Building simple 2D games (2D movement has different patterns)
- ❌ Using a character controller asset (they have their own patterns)
- ❌ Movement logic is trivial (one-liner Transform.Translate)

## Architecture Pattern

The Player Movement Executor uses the **Executor Pattern**:

```
┌──────────────────────────────────────────┐
│         Input Sources                     │
│                                           │
│  ┌──────────┐  ┌──────────┐  ┌────────┐ │
│  │Keyboard  │  │ Touch    │  │  AI    │ │
│  └────┬─────┘  └────┬─────┘  └───┬────┘ │
│       │             │             │      │
│       └──────────┬──┴─────────────┘      │
│                  │                       │
└──────────────────┼───────────────────────┘
                   │
                   ▼
       ┌───────────────────────┐
       │  PlayerController     │
       │                       │
       │  - Handles input      │
       │  - Game logic         │
       │  - State management   │
       └───────────┬───────────┘
                   │
                   │ SetHorizontalInput()
                   │ SetForwardSpeed()
                   │ SetMovementActive()
                   │
                   ▼
       ┌───────────────────────┐
       │  PlayerMovement       │
       │  (Executor)           │
       │                       │
       │  - Movement execution │
       │  - Physics handling   │
       │  - Position/velocity  │
       └───────────┬───────────┘
                   │
                   ▼
       ┌───────────────────────┐
       │  Transform/Rigidbody  │
       │  (Unity Components)   │
       └───────────────────────┘
```

**Why This Pattern:**
- **Modularity** - Swap input sources without changing movement code
- **Testability** - Test movement logic independently
- **Reusability** - Use same movement component in different games/contexts
- **Clean API** - Controller doesn't need to know about physics details

## Complete API Reference

### Class Description

`PlayerMovement` is a MonoBehaviour that handles player movement execution. It does NOT subscribe to input directly. Instead, it provides a clean public API that controllers call to set movement parameters.

**What it does:**
- Moves player forward at configurable speed
- Moves player left/right based on input
- Smooths horizontal movement for polished feel
- Enforces horizontal bounds (prevents going off-track)
- Supports enable/disable for pausing or death
- Ready for Rigidbody physics integration
- Provides state queries (position, speed, active state)

**What it does NOT do:**
- Read Input.GetAxis() or touch input (controller's job)
- Handle game logic like scoring or death (controller's job)
- Manage animation or visual effects (separate components)

### Inspector Fields (What You Configure)

#### Movement Settings
- **`forwardSpeed`** (float, default: 10.0) - How fast the player moves forward in units per second. Higher = faster. Use this for game speed progression or slow zones.
- **`horizontalSpeed`** (float, default: 0.5) - How fast the player moves left/right. Actually a smoothing factor - lower = smoother but slower response, higher = snappier but more jerky.
- **`maxHorizontalBounds`** (float, default: 5.0) - How far left/right the player can move from center. Defines the track width. If bounds is 5, player can move from -5 to +5.

#### Debug Settings
- **`showDebugInfo`** (bool, default: false) - Turn on console logs to see movement values. Useful for tuning and debugging.

### Public API - Configuration Methods (What You Call)

#### `SetHorizontalInput(float input)`
**What it does:** Sets the left/right movement input. This is typically called every frame from your input controller.

**Parameter:**
- `input` - Horizontal input value (automatically clamped to -1 to 1)
  - `-1.0` = full left
  - `0.0` = center
  - `+1.0` = full right

**When to use:** Call this every frame from Update() in your controller with the current input value.

**Example:**
```csharp
// From PlayerController Update()
float input = Input.GetAxis("Horizontal");
playerMovement.SetHorizontalInput(input);
```

#### `SetForwardSpeed(float speed)`
**What it does:** Changes how fast the player moves forward.

**Parameter:**
- `speed` - Forward speed in units per second (can be any positive value)

**When to use:**
- Speed power-ups (double speed for 5 seconds)
- Slow zones (mud, water)
- Progressive difficulty (speed increases over time)
- Stopping movement (set to 0)

**Example:**
```csharp
// Speed power-up
playerMovement.SetForwardSpeed(20f); // Double speed

// Slow zone
playerMovement.SetForwardSpeed(5f); // Half speed

// Stop forward movement
playerMovement.SetForwardSpeed(0f);
```

#### `SetMovementActive(bool active)`
**What it does:** Completely enables or disables all movement.

**Parameter:**
- `active` - True to enable movement, false to disable

**When to use:**
- Player death (freeze in place)
- Pause menu (stop movement)
- Cutscenes (disable player control)
- Level start countdown (wait 3...2...1...GO!)

**Example:**
```csharp
// Player died
playerMovement.SetMovementActive(false);

// Player respawned
playerMovement.SetMovementActive(true);

// Pause game
playerMovement.SetMovementActive(false);
```

#### `SetHorizontalBounds(float bounds)`
**What it does:** Changes how far left/right the player can move.

**Parameter:**
- `bounds` - Maximum distance from center (always positive)

**When to use:**
- Narrow sections of track (tunnel, bridge)
- Wide sections (open field)
- Dynamic track width based on game progression

**Example:**
```csharp
// Narrow tunnel
playerMovement.SetHorizontalBounds(2f); // Can only move ±2 units

// Wide open area
playerMovement.SetHorizontalBounds(10f); // Can move ±10 units
```

### Public API - State Query Methods (What You Check)

#### `GetForwardSpeed()` → float
**What it does:** Returns the current forward speed.

**Returns:** Current forward speed in units per second.

**When to use:** UI speedometer, checking if player has stopped, calculating time to destination.

**Example:**
```csharp
float speed = playerMovement.GetForwardSpeed();
speedometerText.text = $"{speed:F0} km/h";
```

#### `GetHorizontalPosition()` → float
**What it does:** Returns the player's current left/right position.

**Returns:** Horizontal position (0 = center, negative = left, positive = right).

**When to use:** Checking if player is in specific lane, tracking position for obstacles, analytics.

**Example:**
```csharp
float pos = playerMovement.GetHorizontalPosition();
if (pos < -4f) {
    Debug.Log("Player is far left!");
}
```

#### `IsMovementActive()` → bool
**What it does:** Checks if movement is currently enabled.

**Returns:** True if movement is active, false if disabled.

**When to use:** Conditional UI display, checking before applying power-ups, debug displays.

**Example:**
```csharp
if (!playerMovement.IsMovementActive()) {
    pauseMenuUI.SetActive(true);
}
```

### Private Fields (Internal State - For Reference)

These fields store the component's internal state. You don't access these directly, but understanding them helps you know how it works:

- **`_horizontalInput`** (float) - Current input value from -1 to 1
- **`_current`** (float) - Current horizontal position
- **`_target`** (float) - Target horizontal position (input × bounds)
- **`_isMovementActive`** (bool) - Is movement currently enabled
- **`_rigidbody`** (Rigidbody) - Cached Rigidbody reference for physics

### Unity Lifecycle Methods (When Things Happen)

#### `Awake()`
**What it does:** Runs once when created. Finds and caches the Rigidbody component.

**Why important:** Caching Rigidbody in Awake instead of getting it every frame improves performance.

#### `Update()`
**What it does:** Runs every frame. Applies movement if active.

**Current implementation:** Uses Transform.Translate for simplicity.

**Future:** Will use FixedUpdate() with Rigidbody for physics-based movement.

### Movement Implementation Details

The template provides two movement approaches:

**Current (Simple Transform-based):**
```csharp
// In Update()
_current = Mathf.MoveTowards(_current, _target, horizontalSpeed * Time.deltaTime);
transform.position = new Vector3(_current, y, z);
transform.position += Vector3.forward * forwardSpeed * Time.deltaTime;
```

**Future (Physics-based in FixedUpdate):**
```csharp
// In FixedUpdate()
_rigidbody.MovePosition(targetPosition);
```

The template includes commented-out FixedUpdate() code showing proper Rigidbody integration.

## Usage Patterns

### Pattern 1: Basic Keyboard Control

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;

    private void Update()
    {
        // Get input and pass to movement
        float input = Input.GetAxis("Horizontal");
        playerMovement.SetHorizontalInput(input);
    }
}
```

### Pattern 2: Touch Input for Mobile

```csharp
public class TouchPlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private float touchSensitivity = 0.1f;

    private Vector2 touchStartPos;

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                float delta = (touch.position.x - touchStartPos.x) * touchSensitivity;
                float input = Mathf.Clamp(delta, -1f, 1f);
                playerMovement.SetHorizontalInput(input);
                touchStartPos = touch.position;
            }
        }
    }
}
```

### Pattern 3: AI-Controlled Movement

```csharp
public class AIPlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform targetPoint;

    private void Update()
    {
        // AI calculates input to reach target
        float targetX = targetPoint.position.x;
        float currentX = playerMovement.GetHorizontalPosition();
        float difference = targetX - currentX;

        // Convert difference to -1 to 1 input
        float input = Mathf.Clamp(difference, -1f, 1f);

        playerMovement.SetHorizontalInput(input);
    }
}
```

### Pattern 4: Progressive Speed Increase

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private float startSpeed = 10f;
    [SerializeField] private float speedIncreaseRate = 0.5f;
    [SerializeField] private float maxSpeed = 30f;

    private float elapsedTime = 0f;

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        // Increase speed over time
        float currentSpeed = startSpeed + (elapsedTime * speedIncreaseRate);
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

        playerMovement.SetForwardSpeed(currentSpeed);
    }
}
```

### Pattern 5: Lane-Based Movement

```csharp
public class LaneController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private float[] lanePositions = { -4f, 0f, 4f }; // Left, Center, Right
    [SerializeField] private float laneChangeSpeed = 10f;

    private int currentLane = 1; // Start in center

    private void Update()
    {
        // Switch lanes with arrow keys
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentLane = Mathf.Max(0, currentLane - 1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentLane = Mathf.Min(lanePositions.Length - 1, currentLane + 1);
        }

        // Calculate input to reach target lane
        float targetPosition = lanePositions[currentLane];
        float currentPosition = playerMovement.GetHorizontalPosition();
        float difference = targetPosition - currentPosition;

        // Use high speed for snappy lane changes
        float input = Mathf.Sign(difference) * Mathf.Min(1f, Mathf.Abs(difference) * laneChangeSpeed);

        playerMovement.SetHorizontalInput(input);
    }
}
```

## Best Practices

### Performance Optimization

1. **Cache component references** - Done in Awake(), not in Update()
   ```csharp
   // ✅ GOOD - Cache in Awake
   private void Awake() {
       _rigidbody = GetComponent<Rigidbody>();
   }

   // ❌ BAD - Find every frame
   private void Update() {
       GetComponent<Rigidbody>().MovePosition(...);
   }
   ```

2. **Use FixedUpdate for physics** - When using Rigidbody, move in FixedUpdate
   ```csharp
   // ✅ GOOD - Physics in FixedUpdate
   private void FixedUpdate() {
       _rigidbody.MovePosition(targetPos);
   }

   // ❌ BAD - Physics in Update
   private void Update() {
       _rigidbody.MovePosition(targetPos);
   }
   ```

3. **Minimize allocations** - Reuse Vector3 instead of creating new ones
   ```csharp
   // ✅ GOOD - Reuse or use properties
   Vector3 pos = transform.position;
   pos.x = newX;
   transform.position = pos;

   // ❌ BAD - Creates garbage
   transform.position = new Vector3(newX, transform.position.y, transform.position.z);
   ```

### Movement Design

1. **Tune horizontal smoothing** - Lower = smoother, higher = snappier
   - Endless runner: 0.1-0.3 (smooth)
   - Racing game: 0.5-0.8 (responsive)
   - Lane switcher: 1.0+ (instant)

2. **Set appropriate bounds** - Match your track/level width
   - Narrow tracks: 2-3 units
   - Medium tracks: 4-6 units
   - Wide tracks: 8-10 units

3. **Handle edge cases** - What happens when movement is disabled?
   - Stop velocity: `_rigidbody.velocity = Vector3.zero`
   - Keep momentum: Don't touch velocity
   - Slow down: Apply drag or lerp velocity to zero

### Code Organization

1. **Keep movement dumb** - Movement component should only move, not make decisions
   ```csharp
   // ✅ GOOD - Movement just executes
   playerMovement.SetHorizontalInput(calculatedInput);

   // ❌ BAD - Movement making game decisions
   // Don't add score logic, death logic, etc. to PlayerMovement
   ```

2. **Controller makes decisions** - All game logic in PlayerController
   ```csharp
   // PlayerController decides what to do
   if (hitObstacle) {
       playerMovement.SetMovementActive(false);
       ShowDeathScreen();
   }
   ```

3. **Single Responsibility** - Each component has one job
   - PlayerMovement: Move the player
   - PlayerController: Handle input and game logic
   - PlayerAnimator: Handle animations
   - PlayerCollision: Handle collision detection

## Troubleshooting

### Issue: Player moves too slow/fast horizontally

**Problem:** Player slides too slowly or snaps instantly to input.

**Solutions:**
1. ✅ Adjust `horizontalSpeed` in Inspector
   - Too slow: Increase from 0.5 to 1.0 or higher
   - Too fast: Decrease from 0.5 to 0.1
2. ✅ Check if Time.deltaTime is being used correctly
3. ✅ Verify `maxHorizontalBounds` is reasonable for your scene scale

### Issue: Player goes off track

**Problem:** Player can move outside the playable area.

**Solutions:**
1. ✅ Set appropriate `maxHorizontalBounds` value
2. ✅ Add invisible collider walls at track edges
3. ✅ Check that bounds clamping is working

### Issue: Movement feels laggy

**Problem:** Input to movement has noticeable delay.

**Solutions:**
1. ✅ Increase `horizontalSpeed` for faster response
2. ✅ Use Input.GetAxisRaw() instead of Input.GetAxis() for instant response
3. ✅ Check frame rate - movement smoothing is frame-rate dependent
4. ✅ If using Rigidbody, ensure using FixedUpdate not Update

### Issue: Player jitters or stutters

**Problem:** Movement is not smooth, player position jumps.

**Solutions:**
1. ✅ Use Rigidbody.MovePosition in FixedUpdate (not transform.position in Update)
2. ✅ Enable interpolation on Rigidbody (Rigidbody → Interpolation: Interpolate)
3. ✅ Check that you're not setting position in multiple places
4. ✅ Verify Time.deltaTime is being used for frame-rate independence

### Issue: Physics collisions don't work

**Problem:** Player goes through obstacles or doesn't collide.

**Solutions:**
1. ✅ Add Rigidbody component to player
2. ✅ Add Collider component to player and obstacles
3. ✅ Use Rigidbody.MovePosition (not transform.position)
4. ✅ Ensure Rigidbody is NOT kinematic
5. ✅ Check collision layer matrix (Edit → Project Settings → Physics)

## Examples

### Example 1: Simple Runner Setup

```csharp
// MinimalController.cs
public class MinimalController : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;

    private void Update()
    {
        float input = Input.GetAxis("Horizontal");
        movement.SetHorizontalInput(input);
    }
}
```

### Example 2: Complete Game Controller

See `assets/examples/PlayerController.cs` for a complete implementation with:
- Input handling (keyboard and gamepad)
- Pause/unpause
- Death handling
- Speed control
- Bounds adjustment

### Example 3: Physics-Based Movement

See `assets/examples/PlayerMovementWithPhysics.cs` for advanced implementation with:
- Rigidbody integration
- Ground detection
- Collision handling
- Kinematic and dynamic movement modes
- Velocity-based movement

## Additional Resources

- **Template:** `assets/templates/PlayerMovement.cs` - Base executor component
- **Examples:** `assets/examples/` - Complete controller implementations
- **References:** `references/movement-patterns.md` - Movement design patterns

## Related Skills

- `mobile-input-controller` - For mobile touch input integration
- `codebase-documenter` - For documenting your player system

---

**Version:** 1.0
**Last Updated:** 2025-01-19
**Maintained By:** Claude Code Skills
