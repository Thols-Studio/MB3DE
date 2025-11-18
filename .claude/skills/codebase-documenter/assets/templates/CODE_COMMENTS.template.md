# Unity3D Code Comments Guide

This document provides examples and patterns for writing effective inline documentation in Unity3D C# scripts that helps new developers understand code quickly.

## Core Principles

1. **Explain "Why" not "What"** - The code shows what it does; comments should explain why it exists
2. **Provide Context** - Help readers understand the bigger picture and Unity-specific behavior
3. **Document Edge Cases** - Explain non-obvious Unity behavior and special handling
4. **Include Examples** - Show how to use complex functions and systems
5. **Keep Comments Current** - Update comments when code changes
6. **Document Unity Lifecycle** - Explain when methods are called in Unity's execution order

## MonoBehaviour Documentation

### Basic MonoBehaviour Class

```csharp
/// <summary>
/// Manages player health, damage, and death behavior.
///
/// Why this exists: Centralized health management allows UI, enemies, and
/// game systems to interact with player state through a consistent interface.
///
/// Unity Lifecycle:
/// - Awake: Caches references and sets initial health
/// - OnEnable: Subscribes to damage events
/// - OnDisable: Unsubscribes from damage events to prevent memory leaks
///
/// Usage:
/// Attach to player GameObject. Configure maxHealth in Inspector.
/// Listen to OnDeath event for game-over logic.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class HealthSystem : MonoBehaviour
{
    /// <summary>Maximum health points. Designer-configurable via Inspector.</summary>
    [SerializeField, Tooltip("Maximum health. Player dies when health reaches 0.")]
    private int maxHealth = 100;

    /// <summary>
    /// Current health value. Clamped between 0 and maxHealth.
    /// Private setter prevents external code from modifying without going through TakeDamage/Heal.
    /// </summary>
    public int CurrentHealth { get; private set; }

    /// <summary>
    /// Invoked when player dies (health reaches 0).
    /// Subscribe in OnEnable, unsubscribe in OnDisable to prevent memory leaks.
    /// </summary>
    public event Action OnDeath;

    /// <summary>
    /// Invoked when health changes. Passes current and max health for UI updates.
    /// </summary>
    public event Action<int, int> OnHealthChanged;

    /// <summary>
    /// Called before Start. Use for initialization that doesn't depend on other objects.
    ///
    /// Why here and not Start: We need CurrentHealth initialized before any
    /// other scripts try to read it in their Start methods.
    /// </summary>
    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    /// <summary>
    /// Applies damage to the health system.
    ///
    /// Why clamp: Prevents negative health values which could cause UI bugs.
    /// Why invoke events: Allows decoupled systems (UI, audio, VFX) to react.
    ///
    /// Performance note: This is called frequently in combat, so we avoid
    /// allocations and keep logic minimal.
    /// </summary>
    /// <param name="damage">Amount of damage to apply. Negative values are treated as 0.</param>
    public void TakeDamage(int damage)
    {
        if (damage < 0)
        {
            Debug.LogWarning($"TakeDamage called with negative value: {damage}. Treating as 0.");
            damage = 0;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handles death logic. Called once when health reaches 0.
    ///
    /// Why check isDead: Prevents multiple death triggers if TakeDamage
    /// is called multiple times in same frame (e.g., multiple projectiles hit).
    /// </summary>
    private bool isDead = false;

    private void Die()
    {
        if (isDead) return; // Prevent multiple death triggers

        isDead = true;
        OnDeath?.Invoke();

        // Disable further damage
        enabled = false;
    }
}
```

### Unity Lifecycle Methods

```csharp
/// <summary>
/// Spawns enemies at regular intervals during gameplay.
///
/// Unity Execution Order:
/// 1. Awake - Initialize spawn timer
/// 2. Start - Find spawn points in scene (after all Awakes complete)
/// 3. Update - Check timer and spawn enemies
/// 4. OnDisable - Clean up spawned enemies when disabled
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 2f;

    private Transform[] spawnPoints;
    private float nextSpawnTime;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    /// <summary>
    /// Called first, before Start, even if script is disabled.
    ///
    /// Use for: Setting up references that don't depend on other objects.
    /// Don't use for: Finding objects in scene (might not exist yet).
    /// </summary>
    private void Awake()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }

    /// <summary>
    /// Called after all Awake calls, before first Update.
    ///
    /// Use for: Finding objects in scene, initializing systems that depend on other objects.
    /// Why not Awake: SpawnPoints might be created in another script's Awake.
    /// </summary>
    private void Start()
    {
        // Find spawn points after all GameObjects are initialized
        GameObject spawnPointContainer = GameObject.Find("SpawnPoints");
        if (spawnPointContainer != null)
        {
            spawnPoints = spawnPointContainer.GetComponentsInChildren<Transform>();
        }
        else
        {
            Debug.LogError("SpawnPoints container not found in scene!");
        }
    }

    /// <summary>
    /// Called every frame. Use for input, timers, and non-physics updates.
    ///
    /// Performance: Avoid expensive operations here. Runs ~60 times per second.
    /// For physics: Use FixedUpdate instead.
    /// </summary>
    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    /// <summary>
    /// Called when object is disabled or destroyed.
    ///
    /// Critical: Always unsubscribe from events here to prevent memory leaks.
    /// Why clean up spawns: Prevents orphaned enemies when scene reloads.
    /// </summary>
    private void OnDisable()
    {
        // Clean up spawned enemies
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
    }

    private void SpawnEnemy()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedEnemies.Add(enemy);
    }
}
```

## ScriptableObject Documentation

```csharp
/// <summary>
/// Defines weapon properties and behavior configuration.
///
/// Why ScriptableObject: Allows designers to create weapon variants without code.
/// Each .asset file is a different weapon that can be assigned to players/enemies.
///
/// Usage:
/// 1. Right-click in Project -> Create -> Game/Weapon
/// 2. Configure properties in Inspector
/// 3. Assign to WeaponController component
///
/// Design Pattern: Data-driven design separates data from logic.
/// Benefits: Easy balancing, no hardcoded values, version control friendly.
/// </summary>
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    /// <summary>Display name shown in UI.</summary>
    [Header("Identity")]
    public string weaponName = "Unnamed Weapon";

    /// <summary>
    /// Weapon description for tooltips.
    /// Use [TextArea] to allow multi-line editing in Inspector.
    /// </summary>
    [TextArea(3, 5)]
    public string description;

    /// <summary>Base damage per hit. Can be modified by player upgrades.</summary>
    [Header("Combat Stats")]
    [Range(1, 100)]
    public int baseDamage = 10;

    /// <summary>
    /// Time between shots in seconds.
    /// Lower = faster fire rate. Minimum 0.1 to prevent performance issues.
    /// </summary>
    [Range(0.1f, 5f)]
    public float fireRate = 0.5f;

    /// <summary>
    /// Projectile prefab to spawn when firing.
    ///
    /// Requirements:
    /// - Must have Rigidbody component
    /// - Should have Projectile script attached
    /// - Collider for hit detection
    /// </summary>
    [Header("Visuals & Effects")]
    public GameObject projectilePrefab;

    /// <summary>
    /// Muzzle flash particle effect. Plays at weapon fire point.
    /// Optional: Leave null if no muzzle flash desired.
    /// </summary>
    public ParticleSystem muzzleFlash;

    /// <summary>Audio clip played when weapon fires.</summary>
    public AudioClip fireSound;

    /// <summary>
    /// Validates weapon data to catch configuration errors early.
    ///
    /// Called: When ScriptableObject is loaded or modified in Editor.
    /// Use: Ensures designers don't create invalid weapon configs.
    /// </summary>
    private void OnValidate()
    {
        // Enforce minimum fire rate to prevent performance issues
        if (fireRate < 0.1f)
        {
            Debug.LogWarning($"{weaponName}: Fire rate too low. Setting to minimum 0.1s");
            fireRate = 0.1f;
        }

        // Warn if projectile prefab is missing required components
        if (projectilePrefab != null && projectilePrefab.GetComponent<Rigidbody>() == null)
        {
            Debug.LogError($"{weaponName}: Projectile prefab missing Rigidbody component!");
        }
    }
}
```

## Physics and Collision Documentation

```csharp
/// <summary>
/// Handles player movement using physics-based character controller.
///
/// Why Rigidbody: Using physics allows proper collision response and ground detection.
/// Alternative: CharacterController (simpler but less physically accurate).
///
/// Physics Settings:
/// - Use FixedUpdate for all physics operations (consistent timestep)
/// - Rigidbody must be on "Player" layer with proper collision matrix setup
/// - Ground check uses raycast (more reliable than OnCollisionStay)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;

    private Rigidbody rb;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Prevent rigidbody from rotating (common player controller setup)
        rb.freezeRotation = true;
    }

    /// <summary>
    /// Use FixedUpdate for physics operations.
    ///
    /// Why FixedUpdate: Called at fixed time intervals (default 0.02s = 50Hz).
    /// This ensures consistent physics behavior regardless of frame rate.
    ///
    /// Update vs FixedUpdate:
    /// - Update: Variable timestep, use for input and visuals (~60 FPS)
    /// - FixedUpdate: Fixed timestep, use for physics (~50 FPS)
    /// </summary>
    private void FixedUpdate()
    {
        // Ground check using raycast (more reliable than collision callbacks)
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // Get input in Update, apply forces in FixedUpdate
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed;

        // Use MovePosition for kinematic-like movement with physics
        // Alternative: rb.AddForce (more physics-based but harder to control)
        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Called when this collider/rigidbody begins touching another.
    ///
    /// Requirements:
    /// - Both objects need colliders
    /// - At least one must have Rigidbody
    /// - Both must be on layers that can collide (Edit -> Project Settings -> Physics)
    ///
    /// Gotcha: OnCollisionEnter is called in FixedUpdate, not Update.
    /// Don't rely on exact timing with Update-based code.
    /// </summary>
    /// <param name="collision">Information about the collision</param>
    private void OnCollisionEnter(Collision collision)
    {
        // Check layer using bitwise operation (more efficient than CompareTag)
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // Landed on ground
            Debug.Log("Landed on ground");
        }
    }

    /// <summary>
    /// Called when this collider enters a trigger collider.
    ///
    /// Trigger vs Collision:
    /// - Trigger: Passes through, no physics response (OnTriggerEnter/Stay/Exit)
    /// - Collision: Physical collision with response (OnCollisionEnter/Stay/Exit)
    ///
    /// Use triggers for: Pickups, damage zones, detection areas
    /// Use collisions for: Walls, floors, physical objects
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Check for pickup items
        if (other.CompareTag("Pickup"))
        {
            // Process pickup
            Destroy(other.gameObject);
        }
    }
}
```

## Coroutine Documentation

```csharp
/// <summary>
/// Manages weapon firing with cooldown using coroutines.
///
/// Coroutines: Functions that can pause execution and resume next frame.
/// Use cases: Timers, sequences, animations, async operations.
///
/// Important: Coroutines stop when:
/// - GameObject is disabled (SetActive(false))
/// - Component is disabled (enabled = false)
/// - GameObject is destroyed
/// - StopCoroutine is called
/// </summary>
public class WeaponController : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private Transform firePoint;

    private bool canFire = true;
    private Coroutine fireCoroutine;

    private void Update()
    {
        if (Input.GetButton("Fire1") && canFire)
        {
            // Start coroutine - only one instance runs at a time due to canFire check
            fireCoroutine = StartCoroutine(FireWeapon());
        }
    }

    /// <summary>
    /// Fires weapon and enforces cooldown.
    ///
    /// Coroutine Pattern: Returns IEnumerator, uses yield statements.
    ///
    /// yield return null: Wait one frame
    /// yield return new WaitForSeconds(time): Wait for time
    /// yield return new WaitForFixedUpdate(): Wait for next FixedUpdate
    /// yield return StartCoroutine(other): Wait for another coroutine to finish
    ///
    /// Performance: WaitForSeconds allocates garbage. For frequently called
    /// coroutines, cache: private WaitForSeconds waitTime = new WaitForSeconds(1f);
    /// </summary>
    private IEnumerator FireWeapon()
    {
        canFire = false;

        // Spawn projectile
        if (weaponData.projectilePrefab != null)
        {
            Instantiate(weaponData.projectilePrefab, firePoint.position, firePoint.rotation);
        }

        // Play muzzle flash
        if (weaponData.muzzleFlash != null)
        {
            weaponData.muzzleFlash.Play();
        }

        // Wait for fire rate cooldown
        // Performance: For better performance, cache WaitForSeconds instance
        yield return new WaitForSeconds(weaponData.fireRate);

        canFire = true;
    }

    /// <summary>
    /// Example: Complex coroutine with multiple yield patterns.
    ///
    /// Use case: Multi-stage reload animation with delays.
    /// </summary>
    private IEnumerator ReloadSequence()
    {
        Debug.Log("Reload started");

        // Wait for animation to reach reload point
        yield return new WaitForSeconds(0.5f);

        Debug.Log("Ejecting magazine");

        // Wait one frame for animation event
        yield return null;

        Debug.Log("Inserting new magazine");

        // Wait for reload to finish
        yield return new WaitForSeconds(1f);

        Debug.Log("Reload complete");
    }

    /// <summary>
    /// Cleanup coroutines when object is disabled.
    ///
    /// Important: Unity automatically stops all coroutines on disable,
    /// but explicit cleanup is good practice for clarity.
    /// </summary>
    private void OnDisable()
    {
        if (fireCoroutine != null)
        {
            StopCoroutine(fireCoroutine);
        }

        // Alternative: Stop all coroutines on this MonoBehaviour
        // StopAllCoroutines();
    }
}
```

## Unity Editor Attributes

```csharp
/// <summary>
/// Demonstrates Unity's serialization attributes for Inspector customization.
///
/// Serialization: Unity saves/loads these values with the scene.
/// Inspector: Visual editor for modifying values without code changes.
/// </summary>
public class AttributeExamples : MonoBehaviour
{
    // ===== BASIC SERIALIZATION =====

    /// <summary>
    /// Private field shown in Inspector.
    /// Default: private fields are hidden, public fields are shown.
    /// </summary>
    [SerializeField]
    private int hiddenButSerializedValue;

    /// <summary>
    /// Public field hidden from Inspector.
    /// Use when you need public access but don't want designers to modify it.
    /// </summary>
    [HideInInspector]
    public int publicButHiddenValue;

    // ===== ORGANIZATION =====

    /// <summary>Header creates a bold label above fields in Inspector.</summary>
    [Header("Combat Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackDamage = 10;

    /// <summary>Space adds vertical spacing in Inspector for readability.</summary>
    [Space(20)]
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    // ===== CONSTRAINTS =====

    /// <summary>
    /// Range creates a slider in Inspector.
    /// Clamps value between min and max.
    /// Good for: percentages, spawn rates, difficulty settings.
    /// </summary>
    [Range(0f, 1f)]
    [Tooltip("Volume level. 0 = mute, 1 = max volume.")]
    private float volume = 0.7f;

    /// <summary>
    /// Min enforces minimum value (no maximum).
    /// </summary>
    [Min(0)]
    private int enemyCount = 5;

    // ===== TEXT INPUT =====

    /// <summary>
    /// TextArea creates multi-line text field in Inspector.
    /// Parameters: (minLines, maxLines)
    /// Good for: descriptions, dialogue, quest text.
    /// </summary>
    [TextArea(3, 10)]
    private string questDescription;

    /// <summary>
    /// Multiline creates resizable text area.
    /// Parameter: number of lines to show initially.
    /// </summary>
    [Multiline(5)]
    private string playerDialogue;

    // ===== TOOLTIPS =====

    /// <summary>
    /// Tooltip shows help text when hovering in Inspector.
    /// Always add tooltips for designer-facing values!
    /// </summary>
    [Tooltip("Time in seconds between enemy spawns. Lower = more enemies.")]
    [SerializeField] private float spawnInterval = 2f;

    // ===== CONTEXT MENU =====

    /// <summary>
    /// ContextMenu adds right-click menu item in Inspector.
    /// Good for: debugging, testing, quick actions.
    /// </summary>
    [ContextMenu("Reset Health")]
    private void ResetHealth()
    {
        maxHealth = 100;
        Debug.Log("Health reset to 100");
    }

    /// <summary>
    /// RequireComponent ensures this GameObject has the specified component.
    /// Unity auto-adds the component if missing.
    /// Prevents runtime errors from missing dependencies.
    /// </summary>
    // Applied at class level: [RequireComponent(typeof(Rigidbody))]
}
```

## Performance-Critical Code Documentation

```csharp
/// <summary>
/// Manages object pooling for projectiles to reduce garbage collection.
///
/// Why pooling: Instantiate/Destroy causes GC spikes and stuttering.
/// With pooling: Reuse objects instead of creating/destroying.
///
/// Performance Impact:
/// - Without pooling: 50ms GC spike every 100 projectiles
/// - With pooling: 0.1ms, no GC spikes
///
/// Trade-off: Uses more memory (pooled objects stay in memory).
/// Acceptable for: Frequently spawned objects (bullets, particles, enemies).
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialPoolSize = 20;

    /// <summary>
    /// Queue for pooled objects. FIFO = objects get reused in spawn order.
    ///
    /// Why Queue: Better for pooling than List (O(1) dequeue vs O(n) remove).
    /// Alternative: Stack (LIFO) works too, slightly faster but different reuse pattern.
    /// </summary>
    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        // Pre-populate pool to avoid runtime allocations
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// Gets object from pool or creates new one if pool is empty.
    ///
    /// Performance: Cache this reference in frequently-called code.
    /// Don't call GetFromPool() every frame - store result.
    /// </summary>
    public GameObject GetFromPool()
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            // Pool exhausted - create new object
            // This is rare if pool is sized correctly
            Debug.LogWarning($"Pool exhausted for {prefab.name}. Consider increasing pool size.");
            obj = Instantiate(prefab);
        }

        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// Returns object to pool for reuse.
    ///
    /// Important: Call this instead of Destroy() for pooled objects.
    /// Make sure object state is reset before returning to pool.
    /// </summary>
    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

## Unity-Specific Gotchas and Workarounds

```csharp
/// <summary>
/// Examples of common Unity gotchas and their solutions.
/// </summary>
public class UnityGotchas : MonoBehaviour
{
    // ===== GOTCHA 1: Finding objects in Awake =====

    /// <summary>
    /// GOTCHA: FindObjectOfType in Awake can find objects not yet initialized.
    ///
    /// Problem: Execution order is not guaranteed. Object might exist but
    /// its Awake hasn't run yet, so it's in invalid state.
    ///
    /// Solution: Use FindObjectOfType in Start, not Awake.
    /// Start runs after ALL Awakes are complete.
    /// </summary>
    private GameManager gameManager;

    private void Start() // Not Awake!
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // ===== GOTCHA 2: Comparing destroyed objects =====

    /// <summary>
    /// GOTCHA: Unity's null check works differently than C# null.
    ///
    /// Problem: Destroyed Unity objects aren't "true" null.
    /// They're "fake null" - Unity overloads == operator.
    ///
    /// Result: (obj == null) works, but (obj?.someProperty) crashes!
    ///
    /// Solution: Use (obj == null) for Unity objects, not null-conditional (?.
    /// </summary>
    private GameObject myObject;

    private void CheckObject()
    {
        // ❌ WRONG: Will throw exception if object was destroyed
        // var name = myObject?.name;

        // ✅ CORRECT: Unity's overloaded == handles destroyed objects
        if (myObject == null)
        {
            Debug.Log("Object is null or destroyed");
        }
    }

    // ===== GOTCHA 3: GetComponent in Update =====

    /// <summary>
    /// GOTCHA: GetComponent is expensive (~10x slower than cached reference).
    ///
    /// Problem: Calling GetComponent every frame causes performance issues.
    ///
    /// Benchmark:
    /// - GetComponent in Update: 0.5ms per frame (30,000 FPS)
    /// - Cached reference: 0.05ms per frame (300,000 FPS)
    ///
    /// Solution: Cache references in Awake/Start.
    /// </summary>
    private Rigidbody rb; // Cached reference

    private void Awake()
    {
        // ✅ Cache once in Awake
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // ❌ NEVER DO THIS: GetComponent every frame
        // var rb = GetComponent<Rigidbody>();

        // ✅ Use cached reference
        rb.AddForce(Vector3.up);
    }

    // ===== GOTCHA 4: String comparison and tags =====

    /// <summary>
    /// GOTCHA: CompareTag is faster and safer than string comparison.
    ///
    /// Problem:
    /// 1. String comparison allocates memory (GC)
    /// 2. Typos in tag strings cause silent bugs
    ///
    /// Solution: Use CompareTag for better performance and type safety.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // ❌ AVOID: String comparison, prone to typos
        // if (other.gameObject.tag == "Enemy")

        // ✅ BETTER: CompareTag is faster and catches invalid tags in Editor
        if (other.CompareTag("Player"))
        {
            // Handle player collision
        }
    }

    // ===== GOTCHA 5: Instantiate position/rotation =====

    /// <summary>
    /// GOTCHA: Instantiate uses world space, not local space.
    ///
    /// Problem: Setting local position after Instantiate causes one-frame flicker
    /// at world origin before moving to correct local position.
    ///
    /// Solution: Use Instantiate overload with parent parameter.
    /// </summary>
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform parentTransform;

    private void SpawnObject()
    {
        // ❌ WRONG: Object appears at origin for one frame
        // var obj = Instantiate(prefab);
        // obj.transform.SetParent(parentTransform);
        // obj.transform.localPosition = Vector3.zero;

        // ✅ CORRECT: Instantiate with parent, no flicker
        GameObject obj = Instantiate(prefab, parentTransform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    // ===== GOTCHA 6: Time.deltaTime in FixedUpdate =====

    /// <summary>
    /// GOTCHA: Use Time.fixedDeltaTime in FixedUpdate, not Time.deltaTime.
    ///
    /// Problem: Time.deltaTime varies with frame rate.
    /// FixedUpdate runs at fixed intervals, so Time.deltaTime gives wrong value.
    ///
    /// Solution:
    /// - Update: Use Time.deltaTime
    /// - FixedUpdate: Use Time.fixedDeltaTime
    /// </summary>
    private void FixedUpdate()
    {
        // ❌ WRONG: Time.deltaTime in FixedUpdate
        // transform.position += Vector3.forward * Time.deltaTime;

        // ✅ CORRECT: Time.fixedDeltaTime in FixedUpdate
        transform.position += Vector3.forward * Time.fixedDeltaTime;
    }

    // ===== GOTCHA 7: Destroying objects in foreach =====

    /// <summary>
    /// WORKAROUND: Can't modify collection while iterating with foreach.
    ///
    /// Problem: Destroying objects in foreach loop throws exception.
    ///
    /// Solution: Iterate backwards with for loop, or use ToList().
    /// </summary>
    private List<GameObject> enemies = new List<GameObject>();

    private void DestroyAllEnemies()
    {
        // ❌ WRONG: Modifying collection during foreach
        // foreach (var enemy in enemies)
        // {
        //     Destroy(enemy);
        //     enemies.Remove(enemy); // Exception!
        // }

        // ✅ SOLUTION 1: Reverse for loop
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Destroy(enemies[i]);
            enemies.RemoveAt(i);
        }

        // ✅ SOLUTION 2: ToList() creates copy to iterate
        // foreach (var enemy in enemies.ToList())
        // {
        //     Destroy(enemy);
        //     enemies.Remove(enemy);
        // }
    }
}
```

## TODO Comments for Unity

```csharp
public class WeaponSystem : MonoBehaviour
{
    /// <summary>
    /// TODO(username, 2024-12-01): Add weapon recoil system
    ///
    /// Why: Weapons feel too accurate, need recoil for balance
    /// Impact: Medium - affects game feel but not core functionality
    /// Complexity: Low - just add random spread to fire direction
    /// Dependencies: None
    /// Designer request: Issue #GAME-456
    /// </summary>
    private void Fire()
    {
        // Current implementation - no recoil
    }

    /// <summary>
    /// FIXME(username): Rapid fire causes bullet spawn to lag
    ///
    /// Bug: When fireRate < 0.1, bullets spawn behind player instead of at muzzle
    /// Reproduce: Set fireRate to 0.05, hold fire button
    /// Cause: Transform position updates after Instantiate in same frame
    /// Temporary workaround: Enforced minimum 0.1s fire rate in WeaponData
    /// Proper fix: Use queued spawn system or WaitForEndOfFrame
    /// Bug report: GAME-789
    /// </summary>

    /// <summary>
    /// HACK(username): Using layer 31 for projectile ignore
    ///
    /// Why hack: Unity only has 32 layers and we've used all others
    /// Problem: Projectiles shouldn't collide with shooter
    /// Proper solution: Physics.IgnoreCollision between shooter and projectile
    /// Can't use proper solution because: Performance (100+ projectiles active)
    /// Remove this hack when: We refactor layer system or implement pooling
    ///
    /// Unity limitation: https://docs.unity3d.com/Manual/Layers.html
    /// </summary>

    /// <summary>
    /// TODO(username): Optimize for mobile
    ///
    /// Current performance: 45 FPS on low-end Android
    /// Target: 60 FPS on low-end devices
    ///
    /// Optimization opportunities:
    /// 1. Object pooling for projectiles (saves ~15ms per shot)
    /// 2. Use simpler particle effects (saves ~5ms per frame)
    /// 3. Reduce draw calls by batching projectiles (saves ~10ms per frame)
    ///
    /// Blocked by: Need to finish weapon variants first (GAME-234)
    /// Priority: High - mobile is 60% of our player base
    /// </summary>
}
```

## When NOT to Write Comments in Unity

### Don't Explain Obvious Unity Code

```csharp
// ❌ BAD: Obvious Unity API call
// Set position to zero
transform.position = Vector3.zero;

// ✅ GOOD: No comment needed
transform.position = Vector3.zero;
```

### Don't Describe Clear Unity Patterns

```csharp
// ❌ BAD: Describing standard Unity pattern
// Get all enemies in scene and loop through them
var enemies = FindObjectsOfType<Enemy>();
foreach (var enemy in enemies)
{
    // Process enemy
}

// ✅ GOOD: No comment needed for standard pattern
var enemies = FindObjectsOfType<Enemy>();
foreach (var enemy in enemies)
{
    enemy.TakeDamage(10);
}
```

### Don't Leave Commented-Out Code

```csharp
// ❌ BAD: Leaving old experiments in code
private void Update()
{
    // transform.Rotate(Vector3.up * Time.deltaTime);
    // rb.AddForce(Vector3.forward);
    transform.position += transform.forward * moveSpeed * Time.deltaTime;
}

// ✅ GOOD: Remove old code, use version control
private void Update()
{
    transform.position += transform.forward * moveSpeed * Time.deltaTime;
}
```

## Summary

**Good Unity comments:**
- Explain WHY, not WHAT
- Document Unity lifecycle expectations (Awake/Start order)
- Note performance implications (pooling, caching, GC)
- Explain Unity-specific gotchas and workarounds
- Document Inspector configuration requirements
- Include examples for complex systems
- Warn about common pitfalls (null checks, deltaTime, etc.)

**Avoid:**
- Describing obvious Unity API calls
- Leaving commented-out code
- Outdated information
- Comments that duplicate what code clearly shows
- Over-documenting simple serialized fields
