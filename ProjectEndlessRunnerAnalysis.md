# 🎯 Project EndlessRunner Analysis & Roadmap (Hybrid Core Systems)

## 📋 Table of Contents
1. [Hybrid Architecture Integration](#hybrid-architecture-integration)
2. [Match3 Architecture Analysis](#match3-architecture-analysis)
3. [Core Architecture Review](#core-architecture-review)
4. [EndlessRunner Roadmap (Hybrid)](#endlessrunner-roadmap-hybrid)
5. [Implementation Timeline](#implementation-timeline)
6. [Cursor Prompt Templates](#cursor-prompt-templates)
7. [Best Practices & Recommendations](#best-practices--recommendations)

---

## 🏗️ Hybrid Architecture Integration

### **Hybrid Core Systems Approach**

EndlessRunner will be implemented using the hybrid core systems architecture, leveraging base classes from `Core/Common` while maintaining game-specific implementations.

#### **Core/Common Base Classes (Leveraged)**
```
Assets/Scripts/Core/Common/
├── StateManagement/
│   ├── BaseGameStateManager.cs      # Used by RunnerStateManager
│   ├── BaseGameState.cs             # Extended by RunnerGameState
│   └── IGameStateManager.cs         # Interface for state management
├── InputManagement/
│   ├── BaseInputManager.cs          # Used by RunnerInputManager
│   ├── BaseInputHandler.cs          # Extended by RunnerInputHandler
│   ├── BaseInputCommand.cs          # Extended by Runner commands
│   └── IInputManager.cs             # Interface for input management
├── ConfigManagement/
│   ├── BaseGameConfig.cs            # Extended by RunnerConfig
│   ├── ConfigValidator.cs           # Used for validation
│   └── IGameConfig.cs               # Interface for configuration
├── DataManagement/
│   ├── BaseGameData.cs              # Extended by RunnerGameData
│   ├── BasePositionCache.cs         # Extended by RunnerPositionCache
│   └── IGameData.cs                 # Interface for data management
├── PerformanceManagement/
│   ├── BasePerformanceManager.cs    # Used by RunnerPerformanceManager
│   ├── BaseLazyEvaluator.cs         # Extended by RunnerLazyEvaluator
│   └── IPerformanceManager.cs       # Interface for performance
├── ErrorHandling/
│   ├── BaseErrorHandler.cs          # Used by RunnerErrorHandler
│   ├── ErrorValidator.cs            # Used for validation
│   └── IErrorHandler.cs             # Interface for error handling
├── AnimationManagement/
│   ├── BaseAnimationManager.cs      # Used by RunnerAnimationManager
│   ├── BaseAnimationStrategy.cs     # Extended by Runner strategies
│   └── IAnimationManager.cs         # Interface for animation
├── Pooling/
│   ├── BaseObjectPool.cs            # Used by Runner pools
│   ├── IPoolable.cs                 # Interface for poolable objects
│   └── IObjectPool.cs               # Interface for object pools
└── Utils/
    ├── BaseFoundationManager.cs     # Used by RunnerFoundationManager
    ├── BaseMemoryManager.cs         # Used by RunnerMemoryManager
    └── IGameUtils.cs                # Interface for utilities
```

#### **EndlessRunner Game-Specific Implementations**
```
EndlessRunner/
├── Core/
│   ├── EndlessRunnerGame.cs         # Main game coordinator (extends MiniGameBase)
│   ├── RunnerGameStates.cs          # Game states (extends BaseGameState)
│   └── RunnerConstants.cs           # Constants
├── StateManagement/
│   ├── RunnerStateManager.cs        # Extends BaseGameStateManager<RunnerGameState>
│   └── RunnerGameState.cs           # Extends BaseGameState
├── InputManagement/
│   ├── RunnerInputManager.cs        # Extends BaseInputManager
│   ├── RunnerInputHandler.cs        # Extends BaseInputHandler
│   └── Commands/
│       ├── RunnerJumpCommand.cs     # Extends BaseInputCommand
│       ├── RunnerSlideCommand.cs    # Extends BaseInputCommand
│       └── RunnerMoveCommand.cs     # Extends BaseInputCommand
├── ConfigManagement/
│   └── RunnerConfig.cs              # Extends BaseGameConfig
├── DataManagement/
│   ├── RunnerGameData.cs            # Extends BaseGameData
│   └── RunnerPositionCache.cs       # Extends BasePositionCache
├── Player/
│   ├── PlayerController.cs          # Game-specific player control
│   ├── PlayerAnimator.cs            # Game-specific animation
│   ├── PlayerCollisionHandler.cs    # Game-specific collision
│   └── PlayerStats.cs               # Game-specific statistics
├── World/
│   ├── WorldGenerator.cs            # Game-specific world generation
│   ├── ChunkManager.cs              # Game-specific chunk system
│   ├── MovementSystem.cs            # Game-specific movement
│   └── BackgroundManager.cs         # Game-specific background
├── Obstacles/
│   ├── ObstacleSpawner.cs           # Game-specific spawning
│   ├── ObstaclePool.cs              # Extends BaseObjectPool
│   ├── ObstacleTypes.cs             # Game-specific types
│   └── ObstacleBehavior.cs          # Game-specific behaviors
├── Collectibles/
│   ├── CollectibleSpawner.cs        # Game-specific spawning
│   ├── CollectiblePool.cs           # Extends BaseObjectPool
│   ├── CollectibleTypes.cs          # Game-specific types
│   └── CollectibleEffects.cs        # Game-specific effects
├── PowerUps/
│   ├── PowerUpManager.cs            # Game-specific management
│   ├── PowerUpTypes.cs              # Game-specific types
│   ├── PowerUpEffects.cs            # Game-specific effects
│   └── PowerUpUI.cs                 # Game-specific UI
├── Scoring/
│   ├── ScoreManager.cs              # Game-specific scoring
│   ├── ScoreCalculator.cs           # Game-specific calculation
│   ├── HighScoreManager.cs          # Game-specific high scores
│   └── ScoreEvents.cs               # Game-specific events
├── Visual/
│   ├── RunnerAnimationManager.cs    # Extends BaseAnimationManager
│   ├── EffectsManager.cs            # Game-specific effects
│   ├── ParticleManager.cs           # Game-specific particles
│   └── CameraController.cs          # Game-specific camera
├── Audio/
│   ├── RunnerAudioManager.cs        # Game-specific audio
│   ├── AudioEvents.cs               # Game-specific events
│   └── MusicManager.cs              # Game-specific music
├── Events/
│   ├── GameplayEvents.cs            # Game-specific gameplay events
│   ├── PlayerEvents.cs              # Game-specific player events
│   ├── WorldEvents.cs               # Game-specific world events
│   └── UIEvents.cs                  # Game-specific UI events
├── Utils/
│   ├── RunnerFoundationManager.cs   # Extends BaseFoundationManager
│   ├── RunnerMemoryManager.cs       # Extends BaseMemoryManager
│   ├── RunnerMath.cs                # Game-specific math utilities
│   └── RunnerExtensions.cs          # Game-specific extensions
├── Performance/
│   ├── RunnerPerformanceManager.cs  # Extends BasePerformanceManager
│   ├── RunnerLazyEvaluator.cs       # Extends BaseLazyEvaluator
│   ├── LODManager.cs                # Game-specific LOD
│   └── CullingManager.cs            # Game-specific culling
└── Testing/
    ├── RunnerTester.cs              # Main test class
    ├── PlayerTester.cs              # Player tests
    └── WorldTester.cs               # World tests
```

### **Key Benefits of Hybrid Approach for EndlessRunner**

#### **✅ Code Reuse**
- **Base Classes**: Leverage existing Core/Common base classes
- **Consistent Patterns**: Follow established patterns from Match3
- **Reduced Development Time**: Focus on game-specific logic only

#### **✅ Maintainability**
- **Single Source of Truth**: Base classes in Core/Common
- **Easy Updates**: Base changes affect all mini-games
- **Consistent Architecture**: Same patterns across all games

#### **✅ Extensibility**
- **Game-Specific Customization**: Each game can extend base classes
- **Easy Addition**: New mini-games can reuse base classes
- **Flexible Implementation**: Games can override base behavior

#### **✅ Testing**
- **Base Class Testing**: Test common functionality once
- **Game-Specific Testing**: Focus on unique game logic
- **Integration Testing**: Test base + game-specific interaction

---

## 🎮 Match3 Architecture Analysis

### ✅ **Strengths (9/10 Score)**

#### **1. Excellent Folder Organization**
```
Match3/
├── Board/          # Board generation logic
├── Config/         # Configuration management  
├── Data/           # Data structures (TileData, etc.)
├── ErrorHandling/  # Error management
├── Events/         # Game-specific events
├── Input/          # Input handling + Commands
├── Logic/          # Core game logic
├── Performance/    # Performance optimization
├── Pooling/        # Object pooling
├── Utils/          # Utility classes
└── Visual/         # Visual components + Strategies
```

**Why This Works:**
- ✅ **Single Responsibility**: Each folder has one clear purpose
- ✅ **Modular Design**: Independent, replaceable modules
- ✅ **Scalable**: Easy to add new features
- ✅ **Testable**: Each module can be tested independently

#### **2. SOLID Principles Implementation**
- ✅ **SRP**: Each class has single responsibility
- ✅ **OCP**: Open for extension via interfaces
- ✅ **LSP**: Proper inheritance hierarchy
- ✅ **ISP**: Focused interfaces
- ✅ **DIP**: Dependency injection via ServiceLocator

#### **3. Event-Driven Architecture**
- ✅ **Loose Coupling**: EventBus enables decoupled communication
- ✅ **Game-Specific Events**: Match3Events.cs with focused events
- ✅ **Clean Communication**: Systems communicate via events

### 🔧 **Areas for Improvement**

#### **1. Match3Game.cs is Too Large (1713 lines)**
**Problem**: Approaching God Object anti-pattern

**Recommended Refactoring:**
```csharp
// Split into focused classes:
Match3GameOrchestrator.cs    // Main coordinator (200-300 lines)
Match3GameplayManager.cs     // Game flow management
Match3BoardManager.cs        // Board management  
Match3ScoreManager.cs        // Score system
Match3HintSystem.cs          // Hint system
```

#### **2. Events Organization**
**Current**: All events in single file
**Recommended**:
```
Events/
├── GameplayEvents.cs      # TileSelectedEvent, TilesSwappedEvent
├── MatchEvents.cs         # MatchFoundEvent, ComboEvent
├── BoardEvents.cs         # BoardRefillEvent, GravityEvent
└── UIEvents.cs           # HintShownEvent, GameOverEvent
```

#### **3. Configuration Management**
**Enhanced Structure**:
```
Config/
├── Match3GameConfig.cs    # Main game settings
├── Match3VisualConfig.cs  # Visual settings
├── Match3AudioConfig.cs   # Audio settings
└── Match3BalanceConfig.cs # Game balance
```

---

## 🏗️ Core Architecture Review

### **IMiniGame Interface Analysis**

#### **✅ Excellent Design Patterns**
```csharp
public interface IMiniGame
{
    string GameId { get; }
    string DisplayName { get; }
    GameState CurrentState { get; }
    event Action<GameState> OnStateChanged;
    
    Task InitializeAsync();
    void Start();
    void Pause();
    void Resume();
    void End();
    void Cleanup();
    
    int GetCurrentScore();
    bool IsPlayable { get; }
}
```

**Strengths:**
- ✅ **Clear Contract**: Well-defined lifecycle methods
- ✅ **Async Support**: Modern async/await pattern
- ✅ **Event-Driven**: State change notifications
- ✅ **Extensible**: Easy to add new mini-games

### **MiniGameBase Implementation**

#### **✅ Template Method Pattern**
```csharp
public abstract class MiniGameBase : MonoBehaviour, IMiniGame
{
    // Template methods for subclasses to override
    protected virtual Task OnInitializeAsync() { return Task.CompletedTask; }
    protected virtual void OnStart() { }
    protected virtual void OnPause() { }
    protected virtual void OnResume() { }
    protected virtual void OnEnd() { }
    protected virtual void OnCleanup() { }
}
```

**Benefits:**
- ✅ **Consistent Lifecycle**: All games follow same pattern
- ✅ **Flexible Override**: Games can customize behavior
- ✅ **State Management**: Built-in state validation
- ✅ **Error Handling**: Proper exception management

### **EventBus System**

#### **✅ Observer Pattern Implementation**
```csharp
// Publishing events
eventBus?.Publish(new MatchFoundEvent(match.Positions, match.TileType));

// Subscribing to events
eventBus.Subscribe<MatchFoundEvent>(OnMatchFound);
```

**Advantages:**
- ✅ **Decoupled Communication**: Systems don't know each other
- ✅ **Type Safety**: Generic event system
- ✅ **Memory Management**: IDisposable subscriptions
- ✅ **Thread Safety**: Safe during publishing

---

## 🏃‍♂️ EndlessRunner Roadmap (Hybrid)

### **Phase 1: Foundation Architecture (Hybrid)**

#### **Step 1: Core/Common Base Classes (If Not Exists)**
```csharp
// Core/Common/StateManagement/BaseGameStateManager.cs
public abstract class BaseGameStateManager<T> where T : Enum
{
    protected IEventBus eventBus;
    protected T currentState;
    protected Dictionary<T, List<T>> transitionRules;
    
    public T CurrentState => currentState;
    public event Action<T> OnStateChanged;
    
    protected virtual void Initialize()
    {
        transitionRules = new Dictionary<T, List<T>>();
        SetupDefaultTransitionRules();
    }
    
    protected abstract void SetupDefaultTransitionRules();
    public abstract bool CanTransitionTo(T newState);
    
    public virtual void TransitionTo(T newState)
    {
        if (!CanTransitionTo(newState))
        {
            Debug.LogWarning($"[{GetType().Name}] Invalid transition: {currentState} -> {newState}");
            return;
        }
        
        var oldState = currentState;
        currentState = newState;
        OnStateChanged?.Invoke(newState);
        
        Debug.Log($"[{GetType().Name}] State transition: {oldState} -> {newState}");
    }
}

// Core/Common/InputManagement/BaseInputManager.cs
public abstract class BaseInputManager
{
    protected IEventBus eventBus;
    protected bool isInputLocked = false;
    protected List<BaseInputCommand> commandQueue;
    
    public bool IsInputLocked => isInputLocked;
    
    protected virtual void Initialize()
    {
        commandQueue = new List<BaseInputCommand>();
    }
    
    public virtual void LockInput() => isInputLocked = true;
    public virtual void UnlockInput() => isInputLocked = false;
    
    public abstract void ProcessInput();
    protected abstract void HandleInputCommand(BaseInputCommand command);
}
```

#### **Step 2: EndlessRunner Game-Specific Implementation**
```csharp
// EndlessRunner/Core/EndlessRunnerGame.cs
public class EndlessRunnerGame : MiniGameBase
{
    [Header("Runner Configuration")]
    [SerializeField] private RunnerConfig runnerConfig;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private WorldGenerator worldGenerator;
    
    // Service instances
    private IEventBus eventBus;
    private RunnerStateManager stateManager;
    private RunnerInputManager inputManager;
    private ScoreManager scoreManager;
    
    protected override async Task OnInitializeAsync()
    {
        Debug.Log("[EndlessRunnerGame] 🎮 Initializing EndlessRunner with Hybrid Architecture...");
        
        try
        {
            // Resolve dependencies from ServiceLocator
            eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            if (eventBus == null)
            {
                throw new InvalidOperationException("EventBus is required for EndlessRunnerGame");
            }
            
            // Initialize game-specific systems
            InitializeGameSystems();
            
            // Initialize components
            await InitializeComponents();
            
            Debug.Log("[EndlessRunnerGame] ✅ Initialization complete!");
        }
        catch (Exception e)
        {
            Debug.LogError($"[EndlessRunnerGame] ❌ Initialization failed: {e.Message}");
            throw;
        }
    }
    
    private void InitializeGameSystems()
    {
        // Initialize state manager (extends BaseGameStateManager)
        stateManager = new RunnerStateManager(eventBus);
        
        // Initialize input manager (extends BaseInputManager)
        inputManager = new RunnerInputManager(eventBus);
        
        // Initialize score manager
        scoreManager = new ScoreManager(eventBus);
        
        Debug.Log("[EndlessRunnerGame] ✅ Game systems initialized");
    }
}

// EndlessRunner/StateManagement/RunnerStateManager.cs
public class RunnerStateManager : BaseGameStateManager<RunnerGameState>
{
    protected override void SetupDefaultTransitionRules()
    {
        // Runner-specific transition rules
        AddTransitionRule(RunnerGameState.Ready, RunnerGameState.Running);
        AddTransitionRule(RunnerGameState.Running, RunnerGameState.Jumping);
        AddTransitionRule(RunnerGameState.Running, RunnerGameState.Sliding);
        AddTransitionRule(RunnerGameState.Jumping, RunnerGameState.Running);
        AddTransitionRule(RunnerGameState.Sliding, RunnerGameState.Running);
        AddTransitionRule(RunnerGameState.Running, RunnerGameState.GameOver);
    }
    
    public override bool CanTransitionTo(RunnerGameState newState)
    {
        // Runner-specific validation logic
        if (newState == RunnerGameState.Jumping && currentState != RunnerGameState.Running)
            return false;
            
        return base.CanTransitionTo(newState);
    }
}

// EndlessRunner/InputManagement/RunnerInputManager.cs
public class RunnerInputManager : BaseInputManager
{
    private RunnerInputHandler inputHandler;
    
    public override void ProcessInput()
    {
        if (isInputLocked) return;
        
        var inputResult = inputHandler.ProcessInput();
        if (inputResult.HasInput)
        {
            var command = CreateCommandFromInput(inputResult);
            HandleInputCommand(command);
        }
    }
    
    private BaseInputCommand CreateCommandFromInput(RunnerInputResult result)
    {
        // Runner-specific command creation
        switch (result.InputType)
        {
            case RunnerInputType.Jump:
                return new RunnerJumpCommand();
            case RunnerInputType.Slide:
                return new RunnerSlideCommand();
            default:
                return new RunnerMoveCommand(result.MovementVector);
        }
    }
}
```

### **Phase 2: Core Gameplay Systems (Hybrid)**

#### **Player Controller (Free Movement)**
```csharp
// EndlessRunner/Player/PlayerController.cs
public class PlayerController : MonoBehaviour
{
    [Header("Player Configuration")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float slideDuration = 0.5f;
    
    // Components
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private RunnerInputManager inputManager;
    
    // State
    private bool isGrounded = true;
    private bool isSliding = false;
    private Vector3 targetPosition;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        
        // Get input manager from ServiceLocator
        inputManager = ServiceLocator.Instance.Resolve<RunnerInputManager>();
    }
    
    private void Update()
    {
        // Handle continuous movement
        HandleMovement();
        
        // Handle jumping
        HandleJumping();
        
        // Handle sliding
        HandleSliding();
    }
    
    private void HandleMovement()
    {
        // Free movement following mouse/touch
        if (inputManager != null && inputManager.HasMovementInput)
        {
            targetPosition = inputManager.GetWorldPosition();
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }
    
    private void HandleJumping()
    {
        if (inputManager != null && inputManager.HasJumpInput && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            
            // Publish jump event
            ServiceLocator.Instance.Resolve<IEventBus>()?.Publish(new PlayerJumpEvent());
        }
    }
    
    private void HandleSliding()
    {
        if (inputManager != null && inputManager.HasSlideInput && !isSliding)
        {
            StartCoroutine(SlideCoroutine());
        }
    }
    
    private IEnumerator SlideCoroutine()
    {
        isSliding = true;
        
        // Reduce collider size for sliding
        var originalSize = playerCollider.bounds.size;
        playerCollider.transform.localScale = new Vector3(originalSize.x, originalSize.y * 0.5f, originalSize.z);
        
        yield return new WaitForSeconds(slideDuration);
        
        // Restore collider size
        playerCollider.transform.localScale = Vector3.one;
        isSliding = false;
        
        // Publish slide event
        ServiceLocator.Instance.Resolve<IEventBus>()?.Publish(new PlayerSlideEvent());
    }
}
```

#### **World Generation System**
```csharp
// EndlessRunner/World/WorldGenerator.cs
public class WorldGenerator : MonoBehaviour
{
    [Header("World Configuration")]
    [SerializeField] private float chunkSpawnDistance = 50f;
    [SerializeField] private float chunkDespawnDistance = 20f;
    [SerializeField] private GameObject[] chunkPrefabs;
    
    private List<GameObject> activeChunks = new List<GameObject>();
    private float lastChunkZ = 0f;
    private Transform playerTransform;
    
    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Subscribe to player movement events
        var eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
        eventBus?.Subscribe<PlayerMovementEvent>(OnPlayerMoved);
    }
    
    private void OnPlayerMoved(PlayerMovementEvent movementEvent)
    {
        // Spawn new chunks ahead of player
        if (movementEvent.Position.z > lastChunkZ - chunkSpawnDistance)
        {
            SpawnNewChunk();
        }
        
        // Despawn chunks behind player
        DespawnOldChunks(movementEvent.Position.z);
    }
    
    private void SpawnNewChunk()
    {
        var randomChunk = chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];
        var newChunk = Instantiate(randomChunk, new Vector3(0, 0, lastChunkZ), Quaternion.identity);
        activeChunks.Add(newChunk);
        
        lastChunkZ += 50f; // Chunk length
        
        // Publish chunk spawned event
        ServiceLocator.Instance.Resolve<IEventBus>()?.Publish(new ChunkSpawnedEvent(newChunk));
    }
    
    private void DespawnOldChunks(float playerZ)
    {
        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            var chunk = activeChunks[i];
            if (chunk.transform.position.z < playerZ - chunkDespawnDistance)
            {
                // Publish chunk despawned event
                ServiceLocator.Instance.Resolve<IEventBus>()?.Publish(new ChunkDespawnedEvent(chunk));
                
                Destroy(chunk);
                activeChunks.RemoveAt(i);
            }
        }
    }
}
```

### **Phase 3: Game Mechanics (Hybrid)**

#### **Scoring System**
```csharp
// EndlessRunner/Scoring/ScoreManager.cs
public class ScoreManager
{
    private IEventBus eventBus;
    private int currentScore = 0;
    private int highScore = 0;
    private float distanceTraveled = 0f;
    
    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    
    public ScoreManager(IEventBus eventBus)
    {
        this.eventBus = eventBus;
        LoadHighScore();
        
        // Subscribe to relevant events
        eventBus.Subscribe<PlayerMovementEvent>(OnPlayerMoved);
        eventBus.Subscribe<CollectiblePickedUpEvent>(OnCollectiblePickedUp);
    }
    
    private void OnPlayerMoved(PlayerMovementEvent movementEvent)
    {
        // Calculate score based on distance
        distanceTraveled += movementEvent.MovementDelta.magnitude;
        var distanceScore = Mathf.FloorToInt(distanceTraveled);
        
        UpdateScore(distanceScore);
    }
    
    private void OnCollectiblePickedUp(CollectiblePickedUpEvent collectibleEvent)
    {
        // Add bonus points for collectibles
        var bonusScore = collectibleEvent.CollectibleValue;
        UpdateScore(currentScore + bonusScore);
    }
    
    private void UpdateScore(int newScore)
    {
        currentScore = newScore;
        
        // Check for high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            
            // Publish high score event
            eventBus?.Publish(new HighScoreEvent(highScore));
        }
        
        // Publish score changed event
        eventBus?.Publish(new ScoreChangedEvent(currentScore));
    }
    
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("RunnerHighScore", 0);
    }
    
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("RunnerHighScore", highScore);
        PlayerPrefs.Save();
    }
}
```

### **Phase 4: Performance Optimization (Hybrid)**

#### **Object Pooling System**
```csharp
// EndlessRunner/Obstacles/ObstaclePool.cs
public class ObstaclePool : BaseObjectPool<GameObject>
{
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private int initialPoolSize = 20;
    
    protected override GameObject CreatePooledObject()
    {
        var obstacle = Instantiate(obstaclePrefab);
        obstacle.SetActive(false);
        return obstacle;
    }
    
    protected override void OnGetFromPool(GameObject obstacle)
    {
        obstacle.SetActive(true);
    }
    
    protected override void OnReturnToPool(GameObject obstacle)
    {
        obstacle.SetActive(false);
    }
    
    protected override void OnDestroyPoolObject(GameObject obstacle)
    {
        Destroy(obstacle);
    }
}

// EndlessRunner/Collectibles/CollectiblePool.cs
public class CollectiblePool : BaseObjectPool<GameObject>
{
    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField] private int initialPoolSize = 30;
    
    protected override GameObject CreatePooledObject()
    {
        var collectible = Instantiate(collectiblePrefab);
        collectible.SetActive(false);
        return collectible;
    }
    
    protected override void OnGetFromPool(GameObject collectible)
    {
        collectible.SetActive(true);
    }
    
    protected override void OnReturnToPool(GameObject collectible)
    {
        collectible.SetActive(false);
    }
    
    protected override void OnDestroyPoolObject(GameObject collectible)
    {
        Destroy(collectible);
    }
}
```

---

## 📅 Implementation Timeline

### **Week 1: Foundation (Hybrid)**
- ✅ Create Core/Common base classes (if not exists)
- ✅ Setup EndlessRunnerGame inheriting from MiniGameBase
- ✅ Implement RunnerStateManager extending BaseGameStateManager
- ✅ Implement RunnerInputManager extending BaseInputManager
- ✅ Basic event system integration

### **Week 2: Core Gameplay (Hybrid)**
- 🎯 PlayerController with free movement
- 🎯 WorldGenerator with chunk system
- 🎯 Basic collision detection
- 🎯 Object pooling integration

### **Week 3: Game Mechanics (Hybrid)**
- 🎯 ScoreManager with event integration
- 🎯 Collectibles system with pooling
- 🎯 Obstacles system with pooling
- 🎯 Power-ups system

### **Week 4: Polish & Integration (Hybrid)**
- 🎯 UI integration with existing framework
- 🎯 Save system integration
- 🎯 Performance optimization
- 🎯 Testing & debugging

---

## 🤖 Cursor Prompt Templates (Hybrid)

### **Week 1: Foundation Setup (Hybrid)**
```
Create the complete foundation for EndlessRunner using hybrid core systems:

1. **EndlessRunnerGame Class**: Create a class that inherits from MiniGameBase with:
   - gameId "EndlessRunner" and displayName "Endless Runner"
   - Implementation of all lifecycle methods (OnInitializeAsync, OnStart, OnPause, OnResume, OnEnd, OnCleanup)
   - ServiceLocator dependency resolution for IEventBus
   - Proper error handling and logging with Debug.Log statements
   - IsPlayable override returning true for Ready/Playing states
   - GetCurrentScore override returning current score (initially 0)
   - Follow Match3Game.cs patterns but simplified for initial implementation
   - Integration with hybrid core systems (use base classes from Core/Common)

2. **RunnerStateManager**: Create class extending BaseGameStateManager<RunnerGameState>:
   - Implement RunnerGameState enum with: Ready, Running, Jumping, Sliding, GameOver
   - Override SetupDefaultTransitionRules() with runner-specific transitions
   - Override CanTransitionTo() with runner-specific validation logic
   - Proper event publishing for state changes
   - Integration with ServiceLocator for dependency injection

3. **RunnerInputManager**: Create class extending BaseInputManager:
   - Implement free movement input handling using UnityEngine.Input
   - Handle mouse/touch input with continuous movement tracking
   - Detect jump gestures based on vertical swipe distance (minimum 50f pixels)
   - Detect slide gestures based on horizontal swipe distance
   - Publish PlayerMovementEvent for continuous movement while dragging
   - Publish PlayerJumpEvent for detected jump gestures
   - Publish PlayerSlideEvent for detected slide gestures
   - Convert screen coordinates to world coordinates using Camera.main.ScreenToWorldPoint
   - Include proper initialization, cleanup, and input locking/unlocking methods
   - Integration with ServiceLocator for EventBus dependency

4. **Event System**: Create comprehensive event classes following Match3Events.cs pattern:
   - GameplayEvents.cs: GameStartedEvent, GameEndedEvent, GamePausedEvent, GameResumedEvent
   - PlayerEvents.cs: PlayerJumpEvent, PlayerSlideEvent, PlayerMovementEvent, PlayerDeathEvent, PlayerSpawnEvent
   - WorldEvents.cs: ChunkSpawnedEvent, ChunkDespawnedEvent, ObstacleHitEvent, CollectiblePickedUpEvent, WorldGeneratedEvent
   - UIEvents.cs: ScoreChangedEvent, PowerUpActivatedEvent, GameOverEvent, PauseMenuEvent
   - All events should inherit from GameEvent with relevant data properties and proper constructors
   - Integration with hybrid core systems for consistent event handling
```

### **Week 2: Core Gameplay Systems (Hybrid)**
```
Implement the complete core gameplay system for EndlessRunner using hybrid architecture:

1. **Player Controller (Free Movement)**: Create PlayerController class that:
   - Manages player movement with free positioning (not lane-based)
   - Handles jumping with physics-based movement using Rigidbody2D
   - Implements sliding mechanics with reduced hitbox
   - Uses smooth interpolation to target positions with configurable speed
   - Responds to events from RunnerInputManager (PlayerMovementEvent, PlayerJumpEvent, PlayerSlideEvent)
   - Includes comprehensive collision detection with obstacles using OnTriggerEnter2D
   - Has proper state management (running, jumping, sliding, dead) with state machine
   - Uses object pooling for performance optimization (extends BaseObjectPool)
   - Supports continuous movement following mouse/touch position
   - Has movement boundaries to keep player in playable area (screen bounds)
   - Includes proper animation state management and visual feedback
   - Integration with ServiceLocator for dependency injection

2. **World Generation System**: Create WorldGenerator class that:
   - Manages world chunks for infinite runner with seamless generation
   - Handles chunk spawning ahead of player with configurable spawn distance
   - Manages chunk despawning behind player with proper cleanup
   - Uses object pooling for memory efficiency with reusable chunk templates
   - Supports different chunk types (easy, medium, hard) with difficulty progression
   - Includes difficulty progression based on distance traveled
   - Has proper cleanup and memory management with automatic garbage collection
   - Supports async loading for smooth performance
   - Includes chunk validation and error handling
   - Publishes ChunkSpawnedEvent and ChunkDespawnedEvent for event-driven architecture
   - Integration with ServiceLocator for EventBus dependency

3. **Movement System (Free Movement)**: Create MovementSystem class that:
   - Manages free movement within playable boundaries using screen coordinates
   - Handles smooth interpolation to target positions with easing functions
   - Validates movement boundaries (keep player in screen bounds)
   - Provides position calculations and constraints with boundary checking
   - Includes boundary collision detection and response
   - Supports different movement modes (continuous, discrete) with configurable settings
   - Has proper initialization and cleanup methods
   - Handles movement speed and acceleration with smooth transitions
   - Includes movement history for gesture recognition
   - Integration with hybrid core systems for consistent movement handling
```

### **Week 3: Game Mechanics & Systems (Hybrid)**
```
Implement complete game mechanics and supporting systems using hybrid architecture:

1. **Scoring System**: Create ScoreManager class that:
   - Tracks distance traveled as base score with continuous calculation
   - Handles coin collection for bonus points with multiplier system
   - Manages combo systems with streak tracking
   - Includes high score tracking with persistent storage
   - Publishes score events to EventBus (ScoreChangedEvent, HighScoreEvent)
   - Supports different scoring multipliers (power-ups, combos)
   - Has proper save/load integration with PlayerPrefs
   - Includes score validation and anti-cheat measures
   - Supports achievement system integration
   - Integration with ServiceLocator for EventBus dependency

2. **Collectibles System**: Create CollectibleSpawner and CollectibleManager that:
   - Spawns coins and power-ups in world with procedural placement
   - Uses object pooling for performance with CollectiblePool (extends BaseObjectPool)
   - Handles collection detection with trigger-based system
   - Manages different collectible types (coins, gems, power-ups)
   - Includes visual feedback with particle effects and animations
   - Supports magnet power-up effects with attraction mechanics
   - Has proper cleanup and memory management
   - Includes collectible value and rarity system
   - Supports special collectibles with unique effects
   - Publishes CollectiblePickedUpEvent for event-driven architecture
   - Integration with ServiceLocator for dependency injection

3. **Obstacles System**: Create ObstacleSpawner and ObstacleManager that:
   - Spawns obstacles in chunks based on difficulty with procedural generation
   - Uses object pooling for performance with ObstaclePool (extends BaseObjectPool)
   - Supports different obstacle types (static, moving, lane-specific, multi-lane)
   - Includes spawn rate management with dynamic difficulty adjustment
   - Has proper collision setup with trigger colliders
   - Supports procedural generation with seed-based randomness
   - Includes difficulty scaling based on player progress
   - Has obstacle lifecycle management with proper cleanup
   - Includes obstacle behavior patterns and movement
   - Publishes ObstacleHitEvent for event-driven architecture
   - Integration with ServiceLocator for dependency injection
```

### **Week 4: Polish, Integration & Performance (Hybrid)**
```
Complete the EndlessRunner with polish, integration, and performance optimization using hybrid architecture:

1. **UI Integration**: Create comprehensive EndlessRunner UI components that:
   - Integrate with existing UI framework using UI panels
   - Show score, distance, and power-ups with real-time updates
   - Include pause and game over panels with proper state management
   - Support high score display with persistent data
   - Have proper event subscriptions with EventBus integration
   - Include smooth transitions with animation system
   - Support mobile and PC layouts with responsive design
   - Include settings panel with configurable options
   - Support accessibility features and localization
   - Integration with hybrid core systems for consistent UI patterns

2. **Save System Integration**: Create EndlessRunner save data that:
   - Tracks high scores and achievements with persistent storage
   - Saves player progress and statistics with comprehensive data
   - Integrates with existing SaveSystem using ISaveSystem interface
   - Includes proper data validation and integrity checks
   - Supports cloud save preparation for future implementation
   - Has proper error handling and recovery mechanisms
   - Includes data migration support for version updates
   - Supports multiple save slots and profile management
   - Includes backup and restore functionality
   - Integration with ServiceLocator for SaveSystem dependency

3. **Performance Optimization**: Implement comprehensive performance optimizations:
   - LOD system for distant objects with detail reduction
   - Culling for off-screen elements with frustum culling
   - Particle system optimization with pool management
   - Audio source pooling with spatial audio optimization
   - Memory leak prevention with proper disposal patterns
   - Frame rate monitoring with real-time performance metrics
   - Performance profiling tools with built-in profiler integration
   - Async loading for smooth gameplay experience
   - Texture and mesh optimization with compression
   - Script optimization with efficient update patterns
   - Integration with BasePerformanceManager for consistent performance monitoring
```

---

## 🎯 Best Practices & Recommendations (Hybrid)

### **Architecture Principles**
1. **Follow SOLID Principles**: Each class has single responsibility
2. **Use Event-Driven Communication**: Loose coupling via EventBus
3. **Implement Object Pooling**: For frequently spawned objects
4. **Use Async/Await**: For non-blocking operations
5. **Follow Unity Best Practices**: Proper lifecycle management
6. **Leverage Base Classes**: Use Core/Common base classes for consistency
7. **Extend, Don't Modify**: Extend base classes instead of modifying them

### **Performance Guidelines**
1. **Minimize GC Allocations**: Use object pooling and structs
2. **Optimize Update Loops**: Only update when necessary
3. **Use LOD Systems**: Reduce detail for distant objects
4. **Implement Culling**: Hide off-screen objects
5. **Profile Regularly**: Monitor performance metrics
6. **Use Base Performance Classes**: Leverage BasePerformanceManager

### **Code Quality Standards**
1. **Comprehensive Documentation**: XML comments for public APIs
2. **Unit Testing**: Test critical systems
3. **Error Handling**: Proper exception management
4. **Logging**: Debug information for troubleshooting
5. **Code Reviews**: Regular review process
6. **Base Class Compliance**: Follow base class contracts

### **Extensibility Considerations**
1. **Modular Design**: Easy to add new features
2. **Interface-Based**: Depend on abstractions
3. **Configuration-Driven**: Settings in ScriptableObjects
4. **Event-Driven**: Loose coupling between systems
5. **Plugin Architecture**: Easy to extend with new modules
6. **Base Class Extension**: Easy to add new mini-games

---

## 📊 Success Metrics (Hybrid)

### **Technical Metrics**
- ✅ **Performance**: 60 FPS on target devices
- ✅ **Memory Usage**: < 100MB RAM usage
- ✅ **Load Times**: < 3 seconds initial load
- ✅ **Code Quality**: > 80% test coverage
- ✅ **Architecture**: SOLID principles compliance
- ✅ **Base Class Usage**: Proper inheritance from Core/Common

### **Gameplay Metrics**
- ✅ **Responsiveness**: < 100ms input lag
- ✅ **Smoothness**: No frame drops during gameplay
- ✅ **Scalability**: Easy to add new features
- ✅ **Maintainability**: Clear code structure
- ✅ **Extensibility**: Easy to add new mini-games
- ✅ **Consistency**: Same patterns across all mini-games

---

*This document serves as a comprehensive guide for implementing EndlessRunner using the hybrid core systems approach, leveraging base classes from Core/Common while maintaining game-specific implementations.*