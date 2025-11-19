# Unity Player Movement Patterns - Design Reference

**Version:** 1.0
**Last Updated:** 2025-01-19
**Skill:** player-movement-executor

---

## Table of Contents

1. [Movement Architecture Patterns](#movement-architecture-patterns)
2. [Movement Types](#movement-types)
3. [Input Integration Patterns](#input-integration-patterns)
4. [Physics vs Transform Movement](#physics-vs-transform-movement)
5. [Smoothing and Feel](#smoothing-and-feel)
6. [Common Movement Formulas](#common-movement-formulas)
7. [Performance Considerations](#performance-considerations)

---

## Movement Architecture Patterns

### Pattern 1: Executor Pattern (Recommended)

**Structure:**
- **Controller** - Handles input and game logic
- **Executor** - Handles movement execution
- Clear separation of concerns

**Pros:**
- ✅ Testable (can test movement without input)
- ✅ Reusable (same executor, different controllers)
- ✅ Modular (swap input sources easily)
- ✅ Clean API between components

**Cons:**
- ❌ More files to manage
- ❌ Slightly more complex setup

**When to use:** Most player movement systems, especially for reusable components.

```csharp
// Controller handles input
public class PlayerController : MonoBehaviour {
    [SerializeField] private PlayerMovement movement;

    void Update() {
        float input = Input.GetAxis("Horizontal");
        movement.SetHorizontalInput(input);
    }
}

// Executor handles movement
public class PlayerMovement : MonoBehaviour {
    public void SetHorizontalInput(float input) {
        // Apply movement
    }
}
```

### Pattern 2: Monolithic Controller

**Structure:**
- Single component handles everything
- Input, logic, and movement in one class

**Pros:**
- ✅ Simple to understand
- ✅ All code in one place
- ✅ Good for prototypes

**Cons:**
- ❌ Hard to test
- ❌ Can't reuse movement code
- ❌ Difficult to swap input types
- ❌ Gets messy as features grow

**When to use:** Rapid prototypes, game jams, very simple games.

```csharp
public class Player : MonoBehaviour {
    void Update() {
        // Input
        float input = Input.GetAxis("Horizontal");

        // Movement
        transform.position += Vector3.right * input * speed * Time.deltaTime;

        // Game logic
        if (hitObstacle) Die();
    }
}
```

### Pattern 3: Component-Based Architecture

**Structure:**
- Multiple specialized components
- Each handles one aspect (input, movement, animation, etc.)

**Pros:**
- ✅ Maximum modularity
- ✅ Each component single-purpose
- ✅ Easy to add/remove features
- ✅ Clean separation

**Cons:**
- ❌ Many components to manage
- ❌ Component communication overhead
- ❌ Can be over-engineered for simple games

**When to use:** Large, complex player systems with many features.

```csharp
// GameObject has:
// - PlayerInputHandler
// - PlayerMovement
// - PlayerAnimator
// - PlayerCollisionHandler
// - PlayerHealthSystem
// etc.
```

---

## Movement Types

### Type 1: Continuous Forward Movement

**Description:** Player always moves forward (endless runner, racing).

**Implementation:**
```csharp
void Update() {
    transform.position += Vector3.forward * forwardSpeed * Time.deltaTime;
}
```

**Use Cases:**
- Endless runners (Subway Surfers, Temple Run)
- Racing games
- On-rails shooters

**Design Tips:**
- Make forward speed configurable
- Support speed power-ups
- Consider progressive speed increase
- Allow stopping for cutscenes/death

### Type 2: Free Directional Movement

**Description:** Player controls both direction and speed (open world).

**Implementation:**
```csharp
void Update() {
    Vector3 input = new Vector3(
        Input.GetAxis("Horizontal"),
        0,
        Input.GetAxis("Vertical")
    );

    transform.position += input.normalized * speed * Time.deltaTime;
}
```

**Use Cases:**
- Open-world games
- Top-down shooters
- Exploration games

**Design Tips:**
- Normalize input to prevent faster diagonal movement
- Consider acceleration/deceleration
- Add sprint functionality
- Handle camera-relative movement

### Type 3: Lane-Based Movement

**Description:** Player switches between discrete lanes.

**Implementation:**
```csharp
int currentLane = 1; // Center
float[] lanePositions = { -4f, 0f, 4f };

void SwitchLane(int direction) {
    currentLane = Mathf.Clamp(currentLane + direction, 0, lanePositions.Length - 1);
    targetPosition.x = lanePositions[currentLane];
}

void Update() {
    // Lerp to target lane
    float newX = Mathf.Lerp(transform.position.x, targetPosition.x, laneChangeSpeed * Time.deltaTime);
    transform.position = new Vector3(newX, transform.position.y, transform.position.z);
}
```

**Use Cases:**
- Subway Surfers style runners
- Racing games with lanes
- Rhythm games

**Design Tips:**
- Use snappy transitions (high lerp speed)
- Visual feedback for lane switches
- Prevent switching during obstacles
- Consider diagonal lane changes

### Type 4: Constrained Horizontal Movement

**Description:** Player moves forward automatically, controls only left/right.

**Implementation:**
```csharp
void Update() {
    // Forward (constant)
    transform.position += Vector3.forward * forwardSpeed * Time.deltaTime;

    // Horizontal (controlled)
    float input = Input.GetAxis("Horizontal");
    float newX = Mathf.Clamp(
        transform.position.x + input * horizontalSpeed * Time.deltaTime,
        -maxBounds,
        maxBounds
    );

    transform.position = new Vector3(newX, transform.position.y, transform.position.z);
}
```

**Use Cases:**
- Temple Run style runners
- Skiing/snowboarding games
- Flight games (limited to screen width)

**Design Tips:**
- Smooth horizontal movement
- Clear visual bounds
- Gentle auto-centering (optional)
- Prevent jarring edge hits

---

## Input Integration Patterns

### Keyboard/Gamepad

```csharp
// Smooth analog input
float input = Input.GetAxis("Horizontal"); // -1 to 1, smoothed

// Digital input
float input = Input.GetAxisRaw("Horizontal"); // -1, 0, or 1, instant

// Key-based
float input = 0f;
if (Input.GetKey(KeyCode.A)) input -= 1f;
if (Input.GetKey(KeyCode.D)) input += 1f;
```

**Best for:** PC, console games

### Touch (Absolute Position)

```csharp
// Touch current screen position
if (Input.touchCount > 0) {
    Touch touch = Input.GetTouch(0);
    float screenCenter = Screen.width / 2f;
    float touchX = touch.position.x;

    // Convert to -1 to 1
    float input = (touchX - screenCenter) / screenCenter;
}
```

**Best for:** Mobile games where you touch where you want to be

### Touch (Delta Movement)

```csharp
// Touch drag amount
if (Input.touchCount > 0) {
    Touch touch = Input.GetTouch(0);

    if (touch.phase == TouchPhase.Moved) {
        float deltaX = touch.deltaPosition.x;
        float input = Mathf.Clamp(deltaX * sensitivity, -1f, 1f);
    }
}
```

**Best for:** Mobile games with swipe-style controls

### Tilt/Accelerometer

```csharp
// Device tilt
Vector3 tilt = Input.acceleration;
float input = tilt.x; // -1 to 1 based on device tilt
```

**Best for:** Mobile casual games

### AI/Automated

```csharp
// Calculate input to reach target
float targetX = targetPoint.position.x;
float currentX = transform.position.x;
float input = Mathf.Clamp(targetX - currentX, -1f, 1f);
```

**Best for:** AI players, replays, tutorials

---

## Physics vs Transform Movement

### Transform-Based Movement

**Method:** Directly set `transform.position`

**Pros:**
- ✅ Simple and predictable
- ✅ No physics overhead
- ✅ Exact positioning
- ✅ Good for 2D/arcade games

**Cons:**
- ❌ No automatic collision resolution
- ❌ No physics interactions
- ❌ Can go through walls if moving too fast
- ❌ No momentum/inertia

**Code:**
```csharp
void Update() {
    transform.position += velocity * Time.deltaTime;
}
```

**Use when:**
- Simple arcade games
- No physics interactions needed
- Performance critical (mobile)
- 2D games

### Rigidbody-Based Movement

**Method:** Use `Rigidbody.MovePosition()` or `Rigidbody.velocity`

**Pros:**
- ✅ Automatic collision resolution
- ✅ Physics interactions work
- ✅ Momentum and inertia
- ✅ Proper continuous collision detection

**Cons:**
- ❌ More complex
- ❌ Physics overhead
- ❌ Can feel "floaty" if not tuned
- ❌ Requires Rigidbody component

**Code (MovePosition - Recommended):**
```csharp
void FixedUpdate() {
    Vector3 targetPos = rb.position + velocity * Time.fixedDeltaTime;
    rb.MovePosition(targetPos);
}
```

**Code (Velocity):**
```csharp
void FixedUpdate() {
    rb.velocity = new Vector3(horizontal * speed, rb.velocity.y, forward * speed);
}
```

**Use when:**
- Need physics collisions
- Interacting with physics objects
- 3D games
- Realistic movement

### Hybrid Approach

**Method:** Transform for simple movement, trigger-based collision

**Code:**
```csharp
void Update() {
    transform.position += velocity * Time.deltaTime;
}

void OnTriggerEnter(Collider other) {
    // Handle collisions manually
    if (other.CompareTag("Obstacle")) {
        // Stop, bounce, or die
    }
}
```

**Use when:**
- Simple movement needed
- Basic collision detection okay
- Mobile games (performance matters)

---

## Smoothing and Feel

### Linear Movement (No Smoothing)

```csharp
// Direct, instant response
transform.position += input * speed * Time.deltaTime;
```

**Feel:** Robotic, instant, precise
**Use for:** Competitive games, platformers

### Lerp Smoothing

```csharp
// Smooth toward target
current = Mathf.Lerp(current, target, smoothSpeed * Time.deltaTime);
```

**Feel:** Smooth, slightly delayed
**Use for:** Casual games, UI movement

### SmoothDamp

```csharp
// Smooth with velocity tracking
current = Mathf.SmoothDamp(current, target, ref velocity, smoothTime);
```

**Feel:** Very smooth, natural deceleration
**Use for:** Camera follow, polished movement

### Exponential Smoothing

```csharp
// Gradual approach to target
current += (target - current) * smoothFactor * Time.deltaTime;
```

**Feel:** Smooth acceleration/deceleration
**Use for:** Floating objects, smooth transitions

### MoveTowards (Constant Speed)

```csharp
// Move at constant speed toward target
current = Mathf.MoveTowards(current, target, speed * Time.deltaTime);
```

**Feel:** Consistent speed, predictable
**Use for:** Linear movement, elevators

---

## Common Movement Formulas

### Acceleration/Deceleration

```csharp
// Gradual speed change
if (hasInput) {
    currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
} else {
    currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
}
```

### Speed Clamping

```csharp
// Limit maximum speed
rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
```

### Boundary Clamping

```csharp
// Keep within bounds
float x = Mathf.Clamp(transform.position.x, minX, maxX);
transform.position = new Vector3(x, transform.position.y, transform.position.z);
```

### Smooth Follow

```csharp
// Camera or object following player
Vector3 targetPos = player.position + offset;
transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
```

### Circular Movement

```csharp
// Move in circle or arc
float angle = speed * Time.time;
float x = radius * Mathf.Cos(angle);
float z = radius * Mathf.Sin(angle);
transform.position = new Vector3(x, transform.position.y, z);
```

---

## Performance Considerations

### Update vs FixedUpdate

**Update (Variable):**
- Use for input reading
- Use for non-physics movement
- Runs every frame (variable rate)

**FixedUpdate (Fixed):**
- Use for physics (Rigidbody)
- Runs at fixed intervals (50fps default)
- Consistent physics simulation

**Pattern:**
```csharp
float _input;

void Update() {
    // Read input (every frame)
    _input = Input.GetAxis("Horizontal");
}

void FixedUpdate() {
    // Apply physics (fixed timestep)
    rb.MovePosition(rb.position + Vector3.right * _input * speed * Time.fixedDeltaTime);
}
```

### Optimization Tips

1. **Cache references:**
   ```csharp
   // ✅ Cache in Awake
   private Transform _transform;
   void Awake() { _transform = transform; }

   // ❌ Don't access transform property repeatedly
   void Update() {
       transform.position = ... // Creates property call overhead
   }
   ```

2. **Avoid unnecessary allocations:**
   ```csharp
   // ✅ Reuse Vector3
   Vector3 pos = _transform.position;
   pos.x += deltaX;
   _transform.position = pos;

   // ❌ Creates new Vector3 every frame
   _transform.position = new Vector3(x, y, z);
   ```

3. **Use squared magnitude for distance checks:**
   ```csharp
   // ✅ No square root
   if ((target - current).sqrMagnitude < thresholdSquared)

   // ❌ Expensive square root
   if (Vector3.Distance(target, current) < threshold)
   ```

4. **Profile your code:**
   - Use Unity Profiler (Window → Analysis → Profiler)
   - Check CPU usage of Update/FixedUpdate
   - Look for GC allocations

---

## Tuning Guide

### Movement Feel Checklist

- [ ] **Responsiveness** - Does input feel instant or delayed?
- [ ] **Precision** - Can player make fine adjustments?
- [ ] **Smoothness** - Any jitter or stuttering?
- [ ] **Speed** - Too slow/fast for game pace?
- [ ] **Bounds** - Can player reach all necessary positions?
- [ ] **Edge behavior** - What happens at boundaries?

### Recommended Values (Starting Points)

| Parameter | Endless Runner | Racing Game | Platformer |
|-----------|---------------|-------------|------------|
| Forward Speed | 10-15 | 20-50 | 0 (player-controlled) |
| Horizontal Speed | 0.3-0.5 (smooth) | 8-12 (responsive) | 5-8 |
| Max Bounds | 4-6 units | 10-20 units | N/A |
| Smoothing | 0.1-0.3 | 0.05-0.1 | 0 (instant) |

### Playtesting Focus

1. **First 10 seconds** - Does movement feel good immediately?
2. **Precision tasks** - Can player navigate narrow gaps?
3. **Quick reactions** - Can player dodge sudden obstacles?
4. **Sustained play** - Does it feel good after 5+ minutes?

---

## Additional Resources

- **Unity Manual:** [Rigidbody](https://docs.unity3d.com/Manual/class-Rigidbody.html)
- **Unity Manual:** [Transform](https://docs.unity3d.com/ScriptReference/Transform.html)
- **SKILL.md:** Complete API reference for PlayerMovement component
- **Implementation_Notes.md:** Step-by-step setup guide

---

**Version History:**
- v1.0 (2025-01-19) - Initial movement patterns reference
