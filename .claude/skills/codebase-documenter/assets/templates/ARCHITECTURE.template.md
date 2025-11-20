# Unity3D Architecture Overview

## What This Document Covers

This document explains the high-level architecture of [Project Name], a Unity3D game/application. It covers how different systems interact, key design patterns, scene organization, and where to make common changes.

**Target audience:** Unity developers who need to understand the project structure before making significant changes.

**Unity Version:** [e.g., Unity 2022.3 LTS]

## System Design

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   Unity Application                     │
│                                                         │
│  ┌──────────────┐    ┌──────────────┐    ┌───────────┐  │
│  │   Scenes     │───►│   Managers   │───►│   Data    │  │
│  │  (Gameplay)  │    │  (Singleton) │    │ (ScriptSO)│  │
│  └──────┬───────┘    └──────┬───────┘    └─────┬─────┘  │
│         │                   │                    │      │
│         ▼                   ▼                    ▼      │
│  ┌──────────────┐    ┌──────────────┐    ┌───────────┐  │
│  │  GameObjects │───►│  Components  │───►│  Systems  │  │
│  │  (Prefabs)   │    │ (Behaviour)  │    │ (Logic)   │  │
│  └──────────────┘    └──────────────┘    └───────────┘  │
│         │                   │                           │
│         └───────────────────┴──────────►                │
│                       ┌──────────────┐                  │
│                       │ Unity Engine │                  │
│                       │ (Physics/UI) │                  │
│                       └──────────────┘                  │
└─────────────────────────────────────────────────────────┘
```

**Core Systems:**

1. **Scenes** - Individual game levels/menus loaded and unloaded at runtime
2. **Managers** - Singleton systems that persist across scenes (GameManager, AudioManager, etc.)
3. **GameObjects** - Entity instances in scenes, built from Prefabs
4. **Components** - MonoBehaviour scripts attached to GameObjects
5. **ScriptableObjects** - Data containers for configuration and game data
6. **Systems** - Non-MonoBehaviour classes containing pure game logic

### Architecture Pattern

**Pattern Used:** [e.g., MVC, MVP, ECS (Unity DOTS), Hybrid Component-System]

**Why we chose it:**
- [Reason 1: e.g., Decouples game logic from Unity lifecycle]
- [Reason 2: e.g., Makes testing easier]
- [Reason 3: e.g., Scales well for large teams]

### Technology Stack

| Layer | Technology | Why We Chose It |
|-------|-----------|-----------------|
| Game Engine | Unity 6000.0.62f1 LTS | Long-term support, stability for production |
| Scripting | C# .NET Standard 2.1 | Unity's primary language, strong typing |
| Input | [New Input System / Legacy] | [Better multi-platform support / Simpler API] |
| Physics | [2D / 3D / Both] | [Project requirements] |
| UI | [UI Toolkit / UGUI / Both] | [Modern/Legacy approach] |
| Asset Management | Addressables / Resources | [Dynamic loading requirements] |

## Directory Structure

### Standard Unity Project Layout

```
ProjectRoot/
├── Assets/                          # All game assets (version controlled)
│   ├── _Project/                   # Project-specific assets (prefix with _ to sort first)
│   │   ├── Scenes/                 # Game scenes
│   │   │   ├── _Main.unity        # Bootstrap/initialization scene
│   │   │   ├── MainMenu.unity     # Main menu scene
│   │   │   ├── Gameplay/          # Gameplay scenes
│   │   │   │   ├── Level01.unity
│   │   │   │   └── Level02.unity
│   │   │   └── Testing/           # Test/debug scenes
│   │   │
│   │   ├── Scripts/                # C# scripts
│   │   │   ├── Core/              # Core systems and managers
│   │   │   │   ├── GameManager.cs
│   │   │   │   ├── SceneLoader.cs
│   │   │   │   └── SaveSystem.cs
│   │   │   ├── Gameplay/          # Gameplay-specific scripts
│   │   │   │   ├── Player/
│   │   │   │   ├── Enemies/
│   │   │   │   └── Interactables/
│   │   │   ├── UI/                # UI controllers
│   │   │   │   ├── MainMenuUI.cs
│   │   │   │   └── HUD.cs
│   │   │   ├── Data/              # ScriptableObject definitions
│   │   │   │   ├── GameConfig.cs
│   │   │   │   └── WeaponData.cs
│   │   │   └── Utilities/         # Helper classes and extensions
│   │   │       ├── ObjectPool.cs
│   │   │       └── Extensions.cs
│   │   │
│   │   ├── Prefabs/                # Reusable GameObjects
│   │   │   ├── Characters/
│   │   │   ├── Items/
│   │   │   ├── UI/
│   │   │   └── VFX/
│   │   │
│   │   ├── ScriptableObjects/      # Data assets
│   │   │   ├── GameConfig/
│   │   │   ├── Items/
│   │   │   └── Levels/
│   │   │
│   │   ├── Materials/              # Shaders and materials
│   │   │   ├── Characters/
│   │   │   └── Environment/
│   │   │
│   │   ├── Sprites/                # 2D artwork (if applicable)
│   │   │   ├── Characters/
│   │   │   ├── UI/
│   │   │   └── Environment/
│   │   │
│   │   ├── Models/                 # 3D models (if applicable)
│   │   │   ├── Characters/
│   │   │   └── Environment/
│   │   │
│   │   ├── Audio/                  # Sound effects and music
│   │   │   ├── Music/
│   │   │   ├── SFX/
│   │   │   └── Mixers/
│   │   │
│   │   ├── Animations/             # Animation controllers and clips
│   │   │   ├── Player/
│   │   │   └── Enemies/
│   │   │
│   │   └── Resources/              # Runtime-loadable assets (use sparingly)
│   │       └── Config/
│   │
│   ├── Plugins/                    # Third-party plugins and native code
│   │   ├── Android/
│   │   ├── iOS/
│   │   └── [PluginName]/
│   │
│   ├── StreamingAssets/            # Assets that need to be accessed by path
│   │   └── [Platform-specific assets]
│   │
│   └── TextMesh Pro/               # TextMesh Pro assets (auto-generated)
│
├── Packages/                        # Unity Package Manager packages
│   ├── manifest.json               # Package dependencies
│   └── packages-lock.json          # Locked package versions
│
├── ProjectSettings/                 # Unity project settings (version controlled)
│   ├── ProjectSettings.asset
│   ├── TagManager.asset
│   └── [Other settings files]
│
├── UserSettings/                    # User-specific settings (gitignored)
└── Library/                         # Unity cache (gitignored)
```

### Directory Purpose and Rules

#### Assets/_Project/Scripts/Core/
**Purpose:** Core systems that manage game-wide functionality.

**What goes here:**
- Manager classes (GameManager, AudioManager, InputManager)
- Scene loading and management
- Save/load systems
- Event systems and message buses
- Service locator or dependency injection

**What doesn't go here:**
- Gameplay-specific logic (put in `Gameplay/`)
- UI-specific code (put in `UI/`)
- Data definitions (put in `Data/`)

**When to add a file:** When creating a system that needs to persist across scenes or manage global state.

**Pattern to follow:** Singleton MonoBehaviours or Static service locators

#### Assets/_Project/Scripts/Gameplay/
**Purpose:** Gameplay-specific logic for player, enemies, items, etc.

**What goes here:**
- Player controllers and abilities
- Enemy AI and behavior
- Interactive objects (doors, pickups, triggers)
- Game mechanics implementation
- Combat systems

**What doesn't go here:**
- UI logic (put in `UI/`)
- Manager systems (put in `Core/`)
- Data-only classes (put in `Data/`)

**When to add a file:** When implementing game-specific features and mechanics.

#### Assets/_Project/Scripts/Data/
**Purpose:** ScriptableObject definitions and data containers.

**What goes here:**
- ScriptableObject class definitions
- Configuration classes
- Stat definitions
- Item/weapon/ability data structures

**What doesn't go here:**
- MonoBehaviour scripts (put in appropriate folder)
- Actual data instances (those go in `ScriptableObjects/` as .asset files)

**When to add a file:** When creating new data types that will be defined in the Unity editor.

#### Assets/_Project/Prefabs/
**Purpose:** Reusable GameObject templates.

**Naming convention:** PascalCase, descriptive names (e.g., `EnemyZombie`, `WeaponSword`, `UIHealthBar`)

**Organization:**
- Organize by category (Characters, Items, UI, VFX)
- Use prefab variants for variations
- Keep prefabs as small and modular as possible

**When to create:** When a GameObject will be instantiated multiple times or needs to be shared across scenes.

#### Assets/_Project/ScriptableObjects/
**Purpose:** Instances of ScriptableObject data assets.

**Organization:**
- Mirror the structure of `Scripts/Data/`
- Use folders to categorize by type
- Use descriptive names for easy identification

**When to create:** For configuration data, item definitions, game balance values, etc.

## Unity Execution Flow

### Game Loop and Lifecycle

```
Application Start
      │
      ▼
[Awake] ────────────────────► Called once per GameObject (on scene load)
      │                       • Initialize references
      │                       • Set up dependencies
      ▼
[OnEnable] ─────────────────► Called when GameObject becomes active
      │                       • Subscribe to events
      │                       • Register with managers
      ▼
[Start] ────────────────────► Called once after all Awake calls
      │                       • Initialize state
      │                       • Start coroutines
      ▼
┌─────────────────────────┐
│   Game Loop (Running)   │
├─────────────────────────┤
│ [Update] ──────────────► Called every frame
│    │                     • Handle input
│    │                     • Update game state
│    ▼
│ [FixedUpdate] ─────────► Called at fixed time intervals
│    │                     • Physics calculations
│    │                     • Movement
│    ▼
│ [LateUpdate] ──────────► Called after all Updates
│                          • Camera following
│                          • Final position adjustments
└─────────┬───────────────┘
          │
          ▼
[OnDisable] ────────────────► Called when GameObject becomes inactive
      │                       • Unsubscribe from events
      │                       • Unregister from managers
      ▼
[OnDestroy] ────────────────► Called when GameObject is destroyed
                              • Cleanup resources
                              • Final state saving
```

### Scene Loading Flow

```
Current Scene
      │
      ▼
[Request Scene Load] ──────► SceneLoader.LoadScene("LevelName")
      │
      ▼
[Loading Screen] ──────────► Display loading UI (optional)
      │
      ▼
[Unload Current Scene] ────► OnDisable/OnDestroy called on old GameObjects
      │
      ▼
[Load New Scene] ──────────► Unity loads new scene
      │
      ▼
[Awake/OnEnable/Start] ────► New scene GameObjects initialize
      │
      ▼
[Scene Ready] ─────────────► Gameplay begins
```

## Key Design Patterns

### Pattern 1: Singleton Manager Pattern

**What we decided:** Use persistent Singleton MonoBehaviours for core managers.

**Context:** Need centralized systems accessible from anywhere in the game.

**Implementation:**
```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
```

**Why we decided this:**
- **Easy access:** Any script can call `GameManager.Instance`
- **Persistent state:** Survives scene transitions
- **Unity-friendly:** Works well with MonoBehaviour lifecycle

**Trade-offs:**
- ✅ **Pros:** Simple, works well for small to medium projects
- ❌ **Cons:** Global state, harder to test, can lead to tight coupling
- 🤔 **When to reconsider:** If project grows to 10+ managers, consider Service Locator or Dependency Injection

**Alternatives considered:**
- Static classes: Rejected because can't use MonoBehaviour features (Coroutines, Inspector)
- Service Locator: Rejected for simplicity, but good for larger projects
- Dependency Injection (Zenject/VContainer): Too complex for current project size

### Pattern 2: ScriptableObject Data Architecture

**What we decided:** Use ScriptableObjects for all configuration and stat data.

**Context:** Need designer-friendly way to create and modify game data without code changes.

**Why we decided this:**
- **Designer-friendly:** Non-programmers can create and edit data in Unity Editor
- **Memory efficient:** Single instance shared across all references
- **Version control friendly:** Text-based .asset files
- **Decoupled:** Game logic separated from data

**Trade-offs:**
- ✅ **Pros:** Easy iteration, data-driven design, no hardcoded values
- ❌ **Cons:** Runtime data needs to be cloned (can't modify originals)
- 🤔 **When to reconsider:** If need runtime-generated data, use regular classes

**Example usage:**
```csharp
// Data definition
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public int damage;
    public float fireRate;
    public GameObject projectilePrefab;
}

// Usage in gameplay
public class WeaponController : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;

    private void Fire()
    {
        // Use weaponData.damage, weaponData.fireRate, etc.
    }
}
```

### Pattern 3: [Project-Specific Pattern]

**What we decided:** [e.g., Event-driven communication, Object pooling, State machine for AI]

**Context:** [Explain the problem this solves]

**Implementation:** [Code example or description]

**Why we decided this:**
- [Reason 1]
- [Reason 2]

**Trade-offs:**
- ✅ **Pros:** [Benefits]
- ❌ **Cons:** [Drawbacks]

## Component Communication

### Method 1: Direct References

**When to use:** When components have parent-child or sibling relationships.

```csharp
// Get component on same GameObject
private Rigidbody rb;
private void Awake() { rb = GetComponent<Rigidbody>(); }

// Get component on child
private Animator animator;
private void Awake() { animator = GetComponentInChildren<Animator>(); }

// Serialized reference set in Inspector
[SerializeField] private HealthSystem healthSystem;
```

**Pros:** Fast, simple, no overhead
**Cons:** Creates tight coupling

### Method 2: Events / UnityEvents

**When to use:** When components don't know about each other or one-to-many communication.

```csharp
// Using C# events
public class HealthSystem : MonoBehaviour
{
    public event Action<int> OnHealthChanged;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth);
    }
}

// Listener
public class HealthUI : MonoBehaviour
{
    [SerializeField] private HealthSystem healthSystem;

    private void OnEnable()
    {
        healthSystem.OnHealthChanged += UpdateHealthBar;
    }

    private void OnDisable()
    {
        healthSystem.OnHealthChanged -= UpdateHealthBar;
    }
}
```

**Pros:** Decoupled, flexible, multiple listeners
**Cons:** Harder to debug, need to unsubscribe properly

### Method 3: [Project-Specific Method]

**When to use:** [e.g., Message bus for global events, Service Locator for cross-system communication]

## Scene Architecture

### Scene Loading Strategy

**Approach:** [Single scene / Multiple scenes / Additive scene loading]

**Scene organization:**
```
Scenes/
├── _Main.unity              # Bootstrap scene (always loaded first)
│                            • Contains persistent managers
│                            • DontDestroyOnLoad GameObjects
│                            • Initializes game systems
│
├── MainMenu.unity           # Main menu UI
├── Gameplay/                # Gameplay scenes
│   ├── Level01.unity
│   └── Level02.unity
├── Shared/                  # Additively loaded scenes
│   ├── UI.unity            # UI layer (loaded with gameplay)
│   └── Lighting.unity      # Lighting setup
└── Testing/                 # Debug/test scenes
    └── TestScene.unity
```

**Loading pattern:**
1. `_Main` scene loads on startup (never unloaded)
2. Gameplay scenes loaded on top of `_Main`
3. UI scene additively loaded when needed
4. Old gameplay scene unloaded when switching levels

### Scene Responsibilities

**Bootstrap Scene (_Main):**
- Initialize managers (GameManager, AudioManager, InputManager)
- Set up DontDestroyOnLoad objects
- Load first gameplay scene
- Never unloaded during game session

**Gameplay Scenes:**
- Level-specific GameObjects (environment, enemies, etc.)
- No persistent managers (those are in Bootstrap)
- Can be loaded/unloaded freely

**Additive Scenes:**
- UI overlays that persist across level changes
- Shared systems like lighting or audio listeners
- Loaded once and kept in memory

## Performance Considerations

### Critical Performance Paths

#### 1. Frame Rate Optimization

**Target:** [e.g., 60 FPS on PC, 30 FPS on mobile]

**Current bottlenecks:**
- Draw calls: [number] per frame
- Batching: [Dynamic / Static batching enabled]
- Occlusion culling: [Enabled/Disabled]

**Optimization strategies:**
- **Object pooling:** Reuse GameObjects instead of Instantiate/Destroy
- **Mesh combining:** Combine static meshes to reduce draw calls
- **LOD (Level of Detail):** Use lower poly models at distance
- **Occlusion culling:** Don't render objects not visible to camera

#### 2. Garbage Collection

**Strategy:** Minimize allocations in Update/FixedUpdate

**Common issues and solutions:**
- ❌ `GetComponent<T>()` in Update → ✅ Cache reference in Awake
- ❌ String concatenation → ✅ Use StringBuilder
- ❌ LINQ queries → ✅ Use for loops in hot paths
- ❌ Boxing value types → ✅ Use generic collections

#### 3. Physics Performance

**Settings:**
- Fixed Timestep: [e.g., 0.02 = 50 Hz]
- Solver Iterations: [e.g., 6]
- Layer collision matrix: Optimized to avoid unnecessary checks

**Optimization:**
- Use simplified collision shapes (box/sphere) over mesh colliders
- Disable physics for distant objects
- Use triggers instead of collision detection where possible

### Memory Management

**Texture compression:**
- [Format: e.g., ASTC, ETC2, DXT]
- Max texture size: [e.g., 2048x2048 for characters, 1024x1024 for environment]

**Audio compression:**
- Music: Vorbis streaming
- SFX: Compressed in memory (Vorbis/ADPCM)

**Asset loading:**
- [Addressables / AssetBundles / Resources]
- Async loading for large assets
- Unload unused assets after scene transitions

## Build Pipeline

### Platform Targets

| Platform | Build Target | Architecture | Notes |
|----------|-------------|--------------|-------|
| PC | Windows/Mac/Linux | x64 | Primary development platform |
| Mobile | Android / iOS | ARM64 | [Specific requirements] |
| Console | [PS5/Xbox/Switch] | [Architecture] | [Platform-specific notes] |
| WebGL | Browser | [Version] | [Performance considerations] |

### Build Process

```bash
# Development build (faster iteration)
Unity -quit -batchmode -executeMethod BuildScript.DevelopmentBuild

# Production build (optimized)
Unity -quit -batchmode -executeMethod BuildScript.ProductionBuild
```

**Build steps:**
1. Clean previous build folder
2. Set build target and configuration
3. Run preprocessor scripts (asset optimization, etc.)
4. Build player
5. Post-process (copy files, create installer)

### Build Configurations

**Development:**
- Development build flag enabled
- Script debugging enabled
- Profiler enabled
- No IL2CPP (Mono for faster builds)

**Production:**
- Optimization: Maximum
- IL2CPP enabled (performance + code protection)
- Strip engine code
- Compress assets

## Unity Package Dependencies

### Essential Packages

| Package | Version | Purpose | Notes |
|---------|---------|---------|-------|
| Input System | 1.5.0 | Modern input handling | New Input System |
| Cinemachine | 2.9.0 | Camera control | Virtual cameras |
| TextMesh Pro | 3.0.6 | Text rendering | Built-in to Unity |
| [Package name] | [version] | [purpose] | [notes] |

### Optional Packages

| Package | Version | Purpose | When to use |
|---------|---------|---------|-------------|
| Addressables | 1.21.0 | Asset management | For dynamic asset loading |
| Burst Compiler | 1.8.0 | Performance | DOTS/Job System optimization |
| Timeline | 1.7.0 | Cutscenes | Cinematic sequences |

### Third-Party Assets

| Asset | Version | Purpose | License |
|-------|---------|---------|---------|
| [Asset name] | [version] | [purpose] | [license type] |

## Testing Strategy

### Play Mode Tests

**Location:** `Assets/_Project/Tests/PlayMode/`

**What to test:**
- Gameplay mechanics (player movement, combat, etc.)
- Scene loading and transitions
- Manager initialization

**Framework:** Unity Test Framework (NUnit)

### Edit Mode Tests

**Location:** `Assets/_Project/Tests/EditMode/`

**What to test:**
- Utility functions
- Data validation
- Non-MonoBehaviour classes

## Extension Points

### Adding a New Enemy Type

1. **Create ScriptableObject data:**
   - Create new `EnemyData` asset in `ScriptableObjects/Enemies/`
   - Configure stats (health, damage, speed, etc.)

2. **Create prefab:**
   - Duplicate existing enemy prefab or create new
   - Attach necessary components (movement, combat, AI)
   - Configure animator and visuals

3. **Configure AI:**
   - Create or assign AI behavior script
   - Set up state machine or behavior tree
   - Test in isolated scene first

4. **Add to spawn system:**
   - Register enemy type in `EnemySpawner`
   - Configure spawn weights/rules

### Adding a New Scene/Level

1. **Create scene:**
   - Duplicate template scene or create from scratch
   - Follow naming convention: `LevelXX.unity`

2. **Set up scene:**
   - Add environment geometry
   - Place spawn points
   - Configure lighting and post-processing
   - Add level-specific GameObjects

3. **Register scene:**
   - Add to Build Settings (File → Build Settings)
   - Add to `SceneLoader` configuration
   - Create scene data ScriptableObject if needed

4. **Test scene loading:**
   - Test loading from main menu
   - Test loading from previous level
   - Verify proper cleanup on unload

### Adding a New Manager System

1. **Create manager script:**
   ```csharp
   public class [Name]Manager : MonoBehaviour
   {
       public static [Name]Manager Instance { get; private set; }

       private void Awake()
       {
           if (Instance == null)
           {
               Instance = this;
               DontDestroyOnLoad(gameObject);
               Initialize();
           }
           else
           {
               Destroy(gameObject);
           }
       }

       private void Initialize()
       {
           // Setup code
       }
   }
   ```

2. **Add to bootstrap scene:**
   - Place manager GameObject in `_Main` scene
   - Configure inspector properties
   - Test initialization order

3. **Register with game manager:**
   - Add reference to GameManager if needed
   - Ensure proper initialization order

## Debugging and Profiling

### Common Debug Tools

**Unity Profiler:**
- CPU Usage: Find expensive operations
- Memory: Detect leaks and excessive allocations
- Rendering: Analyze draw calls and batching

**Frame Debugger:**
- View rendering order
- Identify overdraw issues
- Analyze shader performance

**Console Commands:**
[If you have a debug console system]
```
/spawn [prefabName] - Spawn object at player position
/tp [x] [y] [z] - Teleport player
/god - Toggle invincibility
```

### Common Issues and Solutions

**Issue: Low frame rate**
- **Symptoms:** FPS drops below target, choppy gameplay
- **Causes:** Too many draw calls, complex shaders, physics calculations
- **Solution:** Use Profiler to identify bottleneck, optimize accordingly

**Issue: Memory leaks**
- **Symptoms:** Memory usage increases over time, eventual crash
- **Causes:** Forgot to unsubscribe from events, holding references to destroyed objects
- **Solution:** Use Memory Profiler, ensure proper cleanup in OnDestroy/OnDisable

**Issue: Physics behaving incorrectly**
- **Symptoms:** Objects passing through walls, jittery movement
- **Causes:** Incorrect Fixed Timestep, movement in Update instead of FixedUpdate
- **Solution:** Move physics operations to FixedUpdate, adjust collision settings

## Additional Resources

- [Unity Documentation](https://docs.unity3d.com/)
- [Project-specific wiki or documentation]
- [Art pipeline documentation]
- [Audio implementation guide]

## Questions and Feedback

If you have questions about the architecture or suggestions for improvements:
- [Contact method]
- [Issue tracker link]
- [Team chat/Discord link]

---

**Document Version:** 1.0
**Last Updated:** [Date]
**Maintained By:** [Team/Person]
