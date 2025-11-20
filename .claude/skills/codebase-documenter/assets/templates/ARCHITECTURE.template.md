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
| Input | New Input System | Better multi-platform support, mobile touch controls |
| Physics | [2D / 3D / Both] | [Project requirements] |
| UI | [UI Toolkit / UGUI / Both] | [Modern/Legacy approach] |
| Animation/Tweening | DOTween Pro | Professional tweening engine, powerful and performant |
| Inspector | Odin Inspector | Enhanced editor workflow, custom drawers, serialization |
| Game Feel/VFX | Feel | Streamlined feedback system for polish and juice |
| Networking | [Mirror / Netcode / Photon / None] | [Multiplayer requirements] |
| Asset Management | Addressables / Resources | [Dynamic loading requirements] |

## Directory Structure

### Standard Unity Project Layout

```
ProjectRoot/
├── Assets/                          # All game assets (version controlled)
│   ├── _Project/                   # Project-specific assets (prefix with _ to sort first)
│   │   ├── Editor/                 # Editor-only scripts and tools
│   │   │   └── [Custom editor scripts, tools, and inspector drawers]
│   │   │
│   │   └── Runtime/                # Runtime game assets
│   │       ├── Art/                # All visual assets
│   │       │   ├── Models/         # 3D models and meshes
│   │       │   │   ├── Characters/
│   │       │   │   ├── Environment/
│   │       │   │   └── Props/
│   │       │   ├── Textures/       # Texture maps and sprites
│   │       │   │   ├── Characters/
│   │       │   │   ├── Environment/
│   │       │   │   └── UI/
│   │       │   └── Materials/      # Materials and shaders
│   │       │       ├── Characters/
│   │       │       └── Environment/
│   │       │
│   │       ├── Scripts/            # C# runtime scripts
│   │       │   ├── Core/           # Core systems and managers
│   │       │   │   ├── GameManager.cs
│   │       │   │   ├── SceneLoader.cs
│   │       │   │   └── SaveSystem.cs
│   │       │   ├── Gameplay/       # Gameplay-specific scripts
│   │       │   │   ├── Player/
│   │       │   │   ├── Input/
│   │       │   │   └── Movement/
│   │       │   ├── UI/             # UI controllers and components
│   │       │   │   ├── MainMenuUI.cs
│   │       │   │   └── HUD.cs
│   │       │   ├── Data/           # ScriptableObject definitions
│   │       │   │   ├── GameConfig.cs
│   │       │   │   └── LevelData.cs
│   │       │   └── Utilities/      # Helper classes and extensions
│   │       │       ├── ObjectPool.cs
│   │       │       └── Extensions.cs
│   │       │
│   │       ├── Levels/             # Level-related assets
│   │       │   ├── Scenes/         # Unity scene files
│   │       │   │   ├── _Main.unity        # Bootstrap scene
│   │       │   │   ├── MainMenu.unity     # Main menu
│   │       │   │   ├── Gameplay/          # Gameplay scenes
│   │       │   │   │   ├── Level01.unity
│   │       │   │   │   └── Level02.unity
│   │       │   │   └── Testing/           # Test/debug scenes
│   │       │   │
│   │       │   └── Prefabs/        # Level prefabs and reusable GameObjects
│   │       │       ├── Characters/
│   │       │       ├── Environment/
│   │       │       ├── UI/
│   │       │       └── VFX/
│   │       │
│   │       ├── Scriptable Objects/ # ScriptableObject data assets
│   │       │   ├── GameConfig/
│   │       │   ├── Levels/
│   │       │   └── Audio/
│   │       │
│   │       ├── Fonts/              # Font assets for UI and text
│   │       │   ├── TextMeshPro/
│   │       │   └── [Custom fonts]
│   │       │
│   │       ├── docs/               # Project documentation
│   │       │   ├── Architecture.md
│   │       │   └── [Design documents]
│   │       │
│   │       ├── Packages/           # Project-specific package resources
│   │       │   └── [Custom package assets]
│   │       │
│   │       ├── MIDI/               # MIDI files for music sequencing
│   │       │   ├── Tracks/
│   │       │   └── [MIDI sequences]
│   │       │
│   │       └── Audio/              # Audio assets
│   │           ├── Music/          # Background music tracks
│   │           ├── SFX/            # Sound effects
│   │           └── Mixers/         # Audio mixer assets
│   │
│   ├── Plugins/                    # Third-party plugins and native code
│   │   ├── Android/                # Android-specific plugins
│   │   ├── iOS/                    # iOS-specific plugins
│   │   └── [PluginName]/          # Other third-party plugins
│   │
│   ├── StreamingAssets/            # Assets accessed by file path at runtime
│   │   └── [Platform-specific assets]
│   │
│   └── TextMesh Pro/               # TextMesh Pro package assets (auto-generated)
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

#### Assets/_Project/Editor/
**Purpose:** Editor-only scripts and custom tools that enhance the Unity Editor workflow.

**What goes here:**
- Custom Inspector drawers (using Odin Inspector or Unity's PropertyDrawer)
- Editor window tools and utilities
- Custom menu items and shortcuts
- Build scripts and automation tools
- Asset post-processors and import pipelines

**What doesn't go here:**
- Runtime scripts (put in `Runtime/Scripts/`)
- Game logic or gameplay code

**When to add a file:** When creating editor tools, custom inspectors, or build automation.

**Note:** Files in this folder are automatically excluded from builds.

#### Assets/_Project/Runtime/Scripts/Core/
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

#### Assets/_Project/Runtime/Scripts/Gameplay/
**Purpose:** Gameplay-specific logic for player, input, movement, and game mechanics.

**What goes here:**
- Player controllers and abilities
- Input handling (MobileInputController)
- Movement systems (PlayerMovement)
- Interactive objects (doors, pickups, triggers)
- Game mechanics implementation

**What doesn't go here:**
- UI logic (put in `UI/`)
- Manager systems (put in `Core/`)
- Data-only classes (put in `Data/`)

**When to add a file:** When implementing game-specific features and mechanics.

#### Assets/_Project/Runtime/Scripts/Data/
**Purpose:** ScriptableObject definitions and data containers.

**What goes here:**
- ScriptableObject class definitions
- Configuration classes
- Stat definitions
- Level/Game data structures

**What doesn't go here:**
- MonoBehaviour scripts (put in appropriate folder)
- Actual data instances (those go in `Runtime/Scriptable Objects/` as .asset files)

**When to add a file:** When creating new data types that will be defined in the Unity editor.

#### Assets/_Project/Runtime/Art/
**Purpose:** All visual assets including models, textures, and materials.

**Organization:**
- **Models/**: 3D meshes organized by category (Characters, Environment, Props)
- **Textures/**: All texture maps (albedo, normal, metallic, etc.)
- **Materials/**: Material assets that reference textures and shaders

**Best practices:**
- Use consistent naming conventions (e.g., `Character_Hero_Albedo.png`)
- Optimize texture sizes for mobile (2048x2048 max for characters)
- Use texture atlases where possible to reduce draw calls

#### Assets/_Project/Runtime/Levels/
**Purpose:** Level-related assets including scenes and prefabs.

**Organization:**
- **Scenes/**: Unity scene files organized by type (Main, MainMenu, Gameplay, Testing)
- **Prefabs/**: Reusable GameObject templates categorized by type

**Prefab naming convention:** PascalCase, descriptive names (e.g., `Player_Character`, `Environment_Platform`, `UI_MainMenu`)

**When to create:**
- Scenes: For new game levels, menus, or testing environments
- Prefabs: When a GameObject will be instantiated multiple times or shared across scenes

#### Assets/_Project/Runtime/Scriptable Objects/
**Purpose:** Instances of ScriptableObject data assets.

**Organization:**
- Mirror the structure of `Runtime/Scripts/Data/`
- Use folders to categorize by type (GameConfig, Levels, Audio)
- Use descriptive names for easy identification

**When to create:** For configuration data, level definitions, game balance values, audio settings, etc.

#### Assets/_Project/Runtime/Audio/
**Purpose:** All audio assets including music, sound effects, and audio mixers.

**Organization:**
- **Music/**: Background music tracks (use Vorbis streaming)
- **SFX/**: Sound effects (use compressed in-memory format)
- **Mixers/**: Unity Audio Mixer assets for volume control and effects

**Best practices:**
- Use 22050 Hz for SFX, 44100 Hz for music
- Mono audio for positional sounds, stereo for music
- Configure compression per platform (Vorbis for Android, ADPCM for iOS)

#### Assets/_Project/Runtime/MIDI/
**Purpose:** MIDI files for music sequencing and procedural music generation.

**What goes here:**
- MIDI track files
- Musical sequences
- Procedural music data

**When to use:** When implementing dynamic or procedural music systems.

#### Assets/_Project/Runtime/Fonts/
**Purpose:** Font assets for UI and text rendering.

**Organization:**
- TextMesh Pro font assets
- Custom font files

**Best practices:**
- Use TextMesh Pro for better text rendering on mobile
- Include font atlases optimized for needed characters

#### Assets/_Project/Runtime/docs/
**Purpose:** Project documentation and design documents.

**What goes here:**
- Architecture documentation
- Design documents
- Feature specifications
- API documentation

**Best practices:**
- Keep documentation up-to-date with code changes
- Use markdown format for easy version control

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

**Target:** 60 FPS on high-end mobile devices, 30 FPS minimum on mid-range devices
**Platform:** iOS (iPhone 8+), Android (Snapdragon 660+ or equivalent)

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

**Texture compression (Mobile-optimized):**
- **Android:** ASTC 6x6 (balanced quality/size), ETC2 fallback
- **iOS:** ASTC 6x6 (iOS 11+), PVRTC 4-bit (legacy devices)
- Max texture size: 2048x2048 for characters, 1024x1024 for environment
- Use texture atlases to reduce draw calls
- Mipmaps enabled for 3D textures

**Audio compression (Mobile-optimized):**
- **Music:** Vorbis streaming (reduces memory footprint)
- **SFX:** Compressed in memory (Vorbis for Android, ADPCM for iOS)
- Sample rate: 22050 Hz for SFX, 44100 Hz for music
- Mono audio for positional sounds, stereo for music/ambience

**Asset loading:**
- [Addressables / AssetBundles / Resources]
- Async loading for large assets
- Unload unused assets after scene transitions

## Build Pipeline

### Platform Targets

| Platform | Build Target | Min Version | Architecture | Notes |
|----------|-------------|-------------|--------------|-------|
| Android | Android | API Level 24 (Android 7.0) | ARM64 (arm64-v8a) | Primary mobile platform, supports ARMv7 fallback |
| iOS | iOS | iOS 12.0+ | ARM64 | iPhone 6s and newer, Metal graphics API required |

**Development Platform:** Windows/Mac/Linux (Unity Editor)
**Deployment Platforms:** Android and iOS only

**Platform-Specific Requirements:**

**Android:**
- Minimum SDK: API 24 (Android 7.0 Nougat)
- Target SDK: API 34 (Android 14) or latest
- Graphics API: Vulkan (primary), OpenGL ES 3.0 (fallback)
- IL2CPP scripting backend (ARM64 required for Google Play)
- Gradle build system
- Permissions: Configure in AndroidManifest.xml as needed

**iOS:**
- Minimum iOS: 12.0
- Target iOS: Latest stable release
- Graphics API: Metal (required)
- IL2CPP scripting backend (mandatory)
- Xcode project export
- Device support: iPhone, iPad (Universal build recommended)
- Signing: Development/Distribution certificates required

### Build Process

```bash
# Android Development build (faster iteration, testing)
Unity -quit -batchmode -buildTarget Android -executeMethod BuildScript.AndroidDevelopmentBuild

# Android Production build (Google Play release)
Unity -quit -batchmode -buildTarget Android -executeMethod BuildScript.AndroidProductionBuild

# iOS Development build (testing on device)
Unity -quit -batchmode -buildTarget iOS -executeMethod BuildScript.iOSDevelopmentBuild

# iOS Production build (App Store submission)
Unity -quit -batchmode -buildTarget iOS -executeMethod BuildScript.iOSProductionBuild
```

**Android Build Steps:**
1. Clean previous build folder
2. Set build target to Android
3. Configure keystore and signing (production only)
4. Run preprocessor scripts (texture compression, asset optimization)
5. Build APK or AAB (App Bundle for Google Play)
6. Post-process (copy to distribution folder, version tracking)

**iOS Build Steps:**
1. Clean previous build folder
2. Set build target to iOS
3. Export Xcode project
4. Run preprocessor scripts (asset optimization, icon generation)
5. Open Xcode project for signing and archiving
6. Post-process (submit to TestFlight or App Store Connect)

### Build Configurations

**Development (Testing):**
- Development build flag enabled
- Script debugging enabled
- Profiler enabled
- IL2CPP scripting backend (required for ARM64)
- Faster build times (lower optimization)
- Debug symbols included
- **Android:** Debug keystore, APK format
- **iOS:** Development provisioning profile

**Production (Release):**
- Optimization: Maximum (size or speed)
- IL2CPP scripting backend (mandatory for both platforms)
- Strip engine code (reduce build size)
- Compress assets (ASTC for Android, PVRTC/ASTC for iOS)
- Code obfuscation enabled
- **Android:** Release keystore, AAB format for Google Play
- **iOS:** Distribution provisioning profile, App Store submission

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
| DOTween Pro | Latest | Animation and tweening engine for smooth transitions, UI animations, and object motion | Commercial (Unity Asset Store) |
| Odin Inspector | Latest | Advanced inspector and serialization system for enhanced editor workflow and custom property drawers | Commercial (Unity Asset Store) |
| Feel | Latest | Game feel and feedback system for polish, juice, camera shake, haptics, and visual effects | Commercial (Unity Asset Store) |

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
