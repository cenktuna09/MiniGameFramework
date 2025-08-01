# 🏃‍♂️ EndlessRunner Development Roadmap (3D)

## 📋 Overview

Bu roadmap, 3D Endless Runner mini-game'inin hybrid core systems yaklaşımıyla geliştirilmesini detaylandırır. **Clear code structure > amount of features** prensibiyle, modüler ve sürdürülebilir bir mimari oluşturmayı hedefler.

## 🎯 Development Philosophy

### **Core Principles**
- ✅ **Modular Design**: Her sistem bağımsız ve test edilebilir
- ✅ **Event-Driven Architecture**: Loose coupling via EventBus
- ✅ **Base Class Leverage**: Core/Common'dan base sınıfları kullan
- ✅ **Single Responsibility**: Her sınıf tek sorumluluğa sahip
- ✅ **Clean Code**: Okunabilir ve maintainable kod
- ✅ **Performance First**: Optimized systems from start

### **Success Metrics**
- 🎯 **Code Quality**: SOLID principles compliance
- 🎯 **Performance**: 60 FPS on target devices
- 🎯 **Maintainability**: Easy to modify and extend
- 🎯 **Testability**: Each system independently testable
- 🎯 **Scalability**: Easy to add new features

---

## 🏗️ Phase 1: Foundation Architecture (Week 1)

### **Step 1.1: Core/Common Base Classes (If Not Exists)**

#### **Priority 1: Essential Base Classes**
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

// Core/Common/ConfigManagement/BaseGameConfig.cs
public abstract class BaseGameConfig : ScriptableObject
{
    [Header("Base Configuration")]
    public string gameName;
    public string gameVersion;
    public bool enableDebugMode;
    
    public abstract void ValidateSettings();
    public abstract void ResetToDefaults();
}
```

#### **Priority 2: Supporting Base Classes**
```csharp
// Core/Common/PerformanceManagement/BasePerformanceManager.cs
public abstract class BasePerformanceManager
{
    protected IEventBus eventBus;
    protected bool isMonitoring = false;
    protected float frameTimeThreshold = 16.67f; // 60 FPS
    
    public event Action<string> OnPerformanceWarning;
    public event Action<string> OnPerformanceAlert;
    
    public abstract void Initialize();
    public abstract void StartMonitoring();
    public abstract void StopMonitoring();
    public abstract void UpdateMonitoring();
    public abstract string GetPerformanceStats();
}

// Core/Common/ErrorHandling/BaseErrorHandler.cs
public abstract class BaseErrorHandler
{
    protected IEventBus eventBus;
    protected bool enableLogging;
    protected bool enableValidation;
    protected int maxErrorCount;
    
    public abstract void SafeExecute(Action action, string operationName);
    public abstract void ValidateAnimation(string animationName, GameObject target, float duration);
    public abstract void ValidateConfiguration(BaseGameConfig config);
}
```

### **Step 1.2: EndlessRunner Game Foundation**

#### **Main Game Coordinator**
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
    private RunnerPerformanceManager performanceManager;
    
    protected override async Task OnInitializeAsync()
    {
        Debug.Log("[EndlessRunnerGame] 🎮 Initializing 3D EndlessRunner with Hybrid Architecture...");
        
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
        
        // Initialize performance manager (extends BasePerformanceManager)
        performanceManager = new RunnerPerformanceManager(eventBus);
        performanceManager.Initialize();
        performanceManager.StartMonitoring();
        
        Debug.Log("[EndlessRunnerGame] ✅ Game systems initialized");
    }
    
    protected override void OnStart()
    {
        Debug.Log("[EndlessRunnerGame] 🚀 Starting 3D EndlessRunner...");
        
        // Start performance monitoring
        performanceManager?.StartMonitoring();
        
        // Unlock input
        inputManager?.UnlockInput();
        
        Debug.Log("[EndlessRunnerGame] ✅ Game started successfully");
    }
    
    protected override void OnPause()
    {
        Debug.Log("[EndlessRunnerGame] ⏸️ Pausing 3D EndlessRunner...");
        
        // Lock input
        inputManager?.LockInput();
        
        // Pause performance monitoring
        performanceManager?.StopMonitoring();
        
        Debug.Log("[EndlessRunnerGame] ✅ Game paused");
    }
    
    protected override void OnResume()
    {
        Debug.Log("[EndlessRunnerGame] ▶️ Resuming 3D EndlessRunner...");
        
        // Unlock input
        inputManager?.UnlockInput();
        
        // Resume performance monitoring
        performanceManager?.StartMonitoring();
        
        Debug.Log("[EndlessRunnerGame] ✅ Game resumed");
    }
    
    protected override void OnEnd()
    {
        Debug.Log("[EndlessRunnerGame] 🏁 Ending 3D EndlessRunner...");
        
        // Lock input
        inputManager?.LockInput();
        
        // Stop performance monitoring
        performanceManager?.StopMonitoring();
        
        Debug.Log("[EndlessRunnerGame] ✅ Game ended");
    }
    
    protected override void OnCleanup()
    {
        Debug.Log("[EndlessRunnerGame] 🧹 Cleaning up 3D EndlessRunner...");
        
        // Cleanup performance systems
        performanceManager?.StopMonitoring();
        performanceManager?.Cleanup();
        
        Debug.Log("[EndlessRunnerGame] ✅ Cleanup completed");
    }
    
    public override int GetCurrentScore()
    {
        return scoreManager?.CurrentScore ?? 0;
    }
    
    public override bool IsPlayable => 
        stateManager?.CurrentState == RunnerGameState.Ready || 
        stateManager?.CurrentState == RunnerGameState.Running;
}

#### **Game States**
```csharp
// EndlessRunner/StateManagement/RunnerGameState.cs
public enum RunnerGameState
{
    Ready,      // Game is ready to start
    Running,    // Player is running
    Jumping,    // Player is jumping
    Sliding,    // Player is sliding
    Paused,     // Game is paused
    GameOver    // Game is over
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
        AddTransitionRule(RunnerGameState.Running, RunnerGameState.Paused);
        AddTransitionRule(RunnerGameState.Jumping, RunnerGameState.Running);
        AddTransitionRule(RunnerGameState.Sliding, RunnerGameState.Running);
        AddTransitionRule(RunnerGameState.Paused, RunnerGameState.Running);
        AddTransitionRule(RunnerGameState.Running, RunnerGameState.GameOver);
        AddTransitionRule(RunnerGameState.Jumping, RunnerGameState.GameOver);
        AddTransitionRule(RunnerGameState.Sliding, RunnerGameState.GameOver);
    }
    
    public override bool CanTransitionTo(RunnerGameState newState)
    {
        // Runner-specific validation logic
        if (newState == RunnerGameState.Jumping && currentState != RunnerGameState.Running)
            return false;
            
        if (newState == RunnerGameState.Sliding && currentState != RunnerGameState.Running)
            return false;
            
        return base.CanTransitionTo(newState);
    }
}
```

### **Step 1.3: Event System Foundation**

#### **Core Events**
```csharp
// EndlessRunner/Events/GameplayEvents.cs
public class GameStartedEvent : GameEvent
{
    public float StartTime { get; }
    
    public GameStartedEvent()
    {
        StartTime = Time.time;
    }
}

public class GameEndedEvent : GameEvent
{
    public float EndTime { get; }
    public int FinalScore { get; }
    
    public GameEndedEvent(int finalScore)
    {
        EndTime = Time.time;
        FinalScore = finalScore;
    }
}

public class GamePausedEvent : GameEvent
{
    public float PauseTime { get; }
    
    public GamePausedEvent()
    {
        PauseTime = Time.time;
    }
}

public class GameResumedEvent : GameEvent
{
    public float ResumeTime { get; }
    
    public GameResumedEvent()
    {
        ResumeTime = Time.time;
    }
}

// EndlessRunner/Events/PlayerEvents.cs
public class PlayerJumpEvent : GameEvent
{
    public Vector3 JumpPosition { get; }
    public float JumpForce { get; }
    
    public PlayerJumpEvent(Vector3 position, float force)
    {
        JumpPosition = position;
        JumpForce = force;
    }
}

public class PlayerSlideEvent : GameEvent
{
    public Vector3 SlidePosition { get; }
    public float SlideDuration { get; }
    
    public PlayerSlideEvent(Vector3 position, float duration)
    {
        SlidePosition = position;
        SlideDuration = duration;
    }
}

public class PlayerMovementEvent : GameEvent
{
    public Vector3 Position { get; }
    public Vector3 MovementDelta { get; }
    
    public PlayerMovementEvent(Vector3 position, Vector3 delta)
    {
        Position = position;
        MovementDelta = delta;
    }
}

public class PlayerDeathEvent : GameEvent
{
    public Vector3 DeathPosition { get; }
    public string DeathReason { get; }
    
    public PlayerDeathEvent(Vector3 position, string reason)
    {
        DeathPosition = position;
        DeathReason = reason;
    }
}

// EndlessRunner/Events/WorldEvents.cs
public class ChunkSpawnedEvent : GameEvent
{
    public GameObject Chunk { get; }
    public Vector3 SpawnPosition { get; }
    
    public ChunkSpawnedEvent(GameObject chunk, Vector3 position)
    {
        Chunk = chunk;
        SpawnPosition = position;
    }
}

public class ChunkDespawnedEvent : GameEvent
{
    public GameObject Chunk { get; }
    
    public ChunkDespawnedEvent(GameObject chunk)
    {
        Chunk = chunk;
    }
}

public class ObstacleHitEvent : GameEvent
{
    public GameObject Obstacle { get; }
    public Vector3 HitPosition { get; }
    
    public ObstacleHitEvent(GameObject obstacle, Vector3 position)
    {
        Obstacle = obstacle;
        HitPosition = position;
    }
}

public class CollectiblePickedUpEvent : GameEvent
{
    public GameObject Collectible { get; }
    public Vector3 PickupPosition { get; }
    public int CollectibleValue { get; }
    
    public CollectiblePickedUpEvent(GameObject collectible, Vector3 position, int value)
    {
        Collectible = collectible;
        PickupPosition = position;
        CollectibleValue = value;
    }
}

// EndlessRunner/Events/UIEvents.cs
public class ScoreChangedEvent : GameEvent
{
    public int NewScore { get; }
    public int ScoreDelta { get; }
    
    public ScoreChangedEvent(int newScore, int delta)
    {
        NewScore = newScore;
        ScoreDelta = delta;
    }
}

public class HighScoreEvent : GameEvent
{
    public int HighScore { get; }
    
    public HighScoreEvent(int highScore)
    {
        HighScore = highScore;
    }
}

public class GameOverEvent : GameEvent
{
    public int FinalScore { get; }
    public string GameOverReason { get; }
    
    public GameOverEvent(int finalScore, string reason)
    {
        FinalScore = finalScore;
        GameOverReason = reason;
    }
}
```

---

## 🎮 Phase 2: Core Gameplay Systems (Week 2)

### **Step 2.1: Input System (3D)**

#### **Input Manager**
```csharp
// EndlessRunner/InputManagement/RunnerInputManager.cs
public class RunnerInputManager : BaseInputManager
{
    private RunnerInputHandler inputHandler;
    private Camera mainCamera;
    
    // Input state
    private bool isDragging = false;
    private Vector3 lastMousePosition;
    private Vector3 currentMousePosition;
    
    // Configuration
    private readonly float minSwipeDistance = 50f; // Minimum swipe distance for jump
    private readonly float movementSensitivity = 1f;
    
    public RunnerInputManager(IEventBus eventBus) : base()
    {
        this.eventBus = eventBus;
        this.mainCamera = Camera.main;
        
        Initialize();
        
        Debug.Log("[RunnerInputManager] ✅ 3D Input manager initialized");
    }
    
    public override void ProcessInput()
    {
        if (isInputLocked) return;
        
        var inputResult = inputHandler?.ProcessInput() ?? ProcessInputDirectly();
        if (inputResult.HasInput)
        {
            var command = CreateCommandFromInput(inputResult);
            HandleInputCommand(command);
        }
    }
    
    private InputResult ProcessInputDirectly()
    {
        var result = new InputResult();
        
        // Mouse/Touch Down - Start tracking
        if (UnityEngine.Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePosition = UnityEngine.Input.mousePosition;
            currentMousePosition = UnityEngine.Input.mousePosition;
            
            result.InputStarted = true;
            result.StartPosition = GetWorldPosition(lastMousePosition);
            
            Debug.Log($"[RunnerInputManager] 🎯 Input started at: {result.StartPosition}");
        }
        
        // Mouse/Touch Up - End tracking
        if (UnityEngine.Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                var endPosition = GetWorldPosition(UnityEngine.Input.mousePosition);
                var swipeDistance = Vector3.Distance(lastMousePosition, UnityEngine.Input.mousePosition);
                
                // Check for jump gesture (vertical swipe)
                if (swipeDistance > minSwipeDistance)
                {
                    var verticalDelta = UnityEngine.Input.mousePosition.y - lastMousePosition.y;
                    if (Mathf.Abs(verticalDelta) > minSwipeDistance)
                    {
                        result.JumpDetected = true;
                        result.JumpDirection = verticalDelta > 0 ? JumpDirection.Up : JumpDirection.Down;
                        
                        Debug.Log($"[RunnerInputManager] 🦘 Jump detected: {result.JumpDirection}");
                    }
                }
                
                result.InputEnded = true;
                result.EndPosition = endPosition;
                
                Debug.Log($"[RunnerInputManager] 🎯 Input ended at: {endPosition}");
            }
            
            isDragging = false;
        }
        
        // Continuous movement while dragging
        if (isDragging)
        {
            currentMousePosition = UnityEngine.Input.mousePosition;
            var worldPos = GetWorldPosition(currentMousePosition);
            
            result.MovementDetected = true;
            result.CurrentPosition = worldPos;
            result.MovementDelta = worldPos - GetWorldPosition(lastMousePosition);
            
            // Publish continuous movement event
            eventBus?.Publish(new PlayerMovementEvent(worldPos, result.MovementDelta));
        }
        
        return result;
    }
    
    private Vector3 GetWorldPosition(Vector3 screenPosition)
    {
        var worldPos = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPos.z = 0; // Keep on 2D plane for 3D runner
        return worldPos;
    }
    
    protected override void HandleInputCommand(BaseInputCommand command)
    {
        if (command is RunnerJumpCommand jumpCommand)
        {
            eventBus?.Publish(new PlayerJumpEvent(jumpCommand.Position, jumpCommand.JumpForce));
        }
        else if (command is RunnerSlideCommand slideCommand)
        {
            eventBus?.Publish(new PlayerSlideEvent(slideCommand.Position, slideCommand.SlideDuration));
        }
        else if (command is RunnerMoveCommand moveCommand)
        {
            eventBus?.Publish(new PlayerMovementEvent(moveCommand.TargetPosition, moveCommand.MovementDelta));
        }
    }
    
    private BaseInputCommand CreateCommandFromInput(InputResult result)
    {
        if (result.JumpDetected)
        {
            return new RunnerJumpCommand(result.CurrentPosition, 10f);
        }
        else if (result.MovementDetected)
        {
            return new RunnerMoveCommand(result.CurrentPosition, result.MovementDelta);
        }
        
        return null;
    }
}

// EndlessRunner/InputManagement/InputResult.cs
public class InputResult
{
    public bool InputStarted { get; set; }
    public bool InputEnded { get; set; }
    public bool MovementDetected { get; set; }
    public bool JumpDetected { get; set; }
    public JumpDirection JumpDirection { get; set; }
    
    public Vector3 StartPosition { get; set; }
    public Vector3 EndPosition { get; set; }
    public Vector3 CurrentPosition { get; set; }
    public Vector3 MovementDelta { get; set; }
    
    public bool HasInput => InputStarted || InputEnded || MovementDetected || JumpDetected;
    
    public static InputResult None()
    {
        return new InputResult();
    }
}

public enum JumpDirection
{
    Up,
    Down
}

// EndlessRunner/InputManagement/Commands/RunnerJumpCommand.cs
public class RunnerJumpCommand : BaseInputCommand
{
    public Vector3 Position { get; }
    public float JumpForce { get; }
    
    public RunnerJumpCommand(Vector3 position, float force)
    {
        Position = position;
        JumpForce = force;
    }
}

// EndlessRunner/InputManagement/Commands/RunnerSlideCommand.cs
public class RunnerSlideCommand : BaseInputCommand
{
    public Vector3 Position { get; }
    public float SlideDuration { get; }
    
    public RunnerSlideCommand(Vector3 position, float duration)
    {
        Position = position;
        SlideDuration = duration;
    }
}

// EndlessRunner/InputManagement/Commands/RunnerMoveCommand.cs
public class RunnerMoveCommand : BaseInputCommand
{
    public Vector3 TargetPosition { get; }
    public Vector3 MovementDelta { get; }
    
    public RunnerMoveCommand(Vector3 targetPosition, Vector3 movementDelta)
    {
        TargetPosition = targetPosition;
        MovementDelta = movementDelta;
    }
}
```

### **Step 2.2: Player Controller (3D)**

#### **Player Controller**
```csharp
// EndlessRunner/Player/PlayerController.cs
public class PlayerController : MonoBehaviour
{
    [Header("Player Configuration")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float forwardSpeed = 10f; // Constant forward movement
    
    // Components
    private Rigidbody rb;
    private Collider playerCollider;
    private RunnerInputManager inputManager;
    private IEventBus eventBus;
    
    // State
    private bool isGrounded = true;
    private bool isSliding = false;
    private Vector3 targetPosition;
    private Vector3 initialColliderSize;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        initialColliderSize = playerCollider.bounds.size;
        
        // Get services from ServiceLocator
        eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
        inputManager = ServiceLocator.Instance.Resolve<RunnerInputManager>();
        
        // Subscribe to events
        eventBus?.Subscribe<PlayerJumpEvent>(OnPlayerJump);
        eventBus?.Subscribe<PlayerSlideEvent>(OnPlayerSlide);
        eventBus?.Subscribe<PlayerMovementEvent>(OnPlayerMovement);
    }
    
    private void Update()
    {
        // Constant forward movement
        HandleForwardMovement();
        
        // Handle lateral movement
        HandleLateralMovement();
        
        // Handle jumping
        HandleJumping();
        
        // Handle sliding
        HandleSliding();
    }
    
    private void HandleForwardMovement()
    {
        // Move forward at constant speed
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
    }
    
    private void HandleLateralMovement()
    {
        // Free movement following mouse/touch
        if (inputManager != null && inputManager.HasMovementInput)
        {
            targetPosition = inputManager.GetWorldPosition();
            var lateralPosition = new Vector3(targetPosition.x, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, lateralPosition, moveSpeed * Time.deltaTime);
        }
    }
    
    private void HandleJumping()
    {
        if (inputManager != null && inputManager.HasJumpInput && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            
            // Publish jump event
            eventBus?.Publish(new PlayerJumpEvent(transform.position, jumpForce));
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
        eventBus?.Publish(new PlayerSlideEvent(transform.position, slideDuration));
    }
    
    private void OnPlayerJump(PlayerJumpEvent jumpEvent)
    {
        // Handle jump event from input system
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpEvent.JumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }
    
    private void OnPlayerSlide(PlayerSlideEvent slideEvent)
    {
        // Handle slide event from input system
        if (!isSliding)
        {
            StartCoroutine(SlideCoroutine());
        }
    }
    
    private void OnPlayerMovement(PlayerMovementEvent movementEvent)
    {
        // Handle movement event from input system
        var lateralPosition = new Vector3(movementEvent.Position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, lateralPosition, moveSpeed * Time.deltaTime);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            // Handle obstacle collision
            eventBus?.Publish(new ObstacleHitEvent(collision.gameObject, collision.contacts[0].point));
            eventBus?.Publish(new PlayerDeathEvent(transform.position, "Obstacle Hit"));
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            // Handle collectible pickup
            var collectibleValue = other.GetComponent<Collectible>()?.Value ?? 10;
            eventBus?.Publish(new CollectiblePickedUpEvent(other.gameObject, other.transform.position, collectibleValue));
            Destroy(other.gameObject);
        }
    }
}
```

---

## 🌍 Phase 3: World Generation (Week 3)

### **Step 3.1: World Generator (3D)**

#### **World Generator**
```csharp
// EndlessRunner/World/WorldGenerator.cs
public class WorldGenerator : MonoBehaviour
{
    [Header("World Configuration")]
    [SerializeField] private float chunkSpawnDistance = 50f;
    [SerializeField] private float chunkDespawnDistance = 20f;
    [SerializeField] private GameObject[] chunkPrefabs;
    [SerializeField] private float chunkLength = 50f;
    
    private List<GameObject> activeChunks = new List<GameObject>();
    private float lastChunkZ = 0f;
    private Transform playerTransform;
    private IEventBus eventBus;
    
    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
        
        // Subscribe to player movement events
        eventBus?.Subscribe<PlayerMovementEvent>(OnPlayerMoved);
        
        // Spawn initial chunks
        SpawnInitialChunks();
    }
    
    private void SpawnInitialChunks()
    {
        // Spawn 3 initial chunks
        for (int i = 0; i < 3; i++)
        {
            SpawnNewChunk();
        }
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
        var spawnPosition = new Vector3(0, 0, lastChunkZ);
        var newChunk = Instantiate(randomChunk, spawnPosition, Quaternion.identity);
        activeChunks.Add(newChunk);
        
        lastChunkZ += chunkLength;
        
        // Publish chunk spawned event
        eventBus?.Publish(new ChunkSpawnedEvent(newChunk, spawnPosition));
        
        Debug.Log($"[WorldGenerator] 🌍 Spawned chunk at Z: {spawnPosition.z}");
    }
    
    private void DespawnOldChunks(float playerZ)
    {
        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            var chunk = activeChunks[i];
            if (chunk.transform.position.z < playerZ - chunkDespawnDistance)
            {
                // Publish chunk despawned event
                eventBus?.Publish(new ChunkDespawnedEvent(chunk));
                
                Destroy(chunk);
                activeChunks.RemoveAt(i);
                
                Debug.Log($"[WorldGenerator] 🗑️ Despawned chunk at Z: {chunk.transform.position.z}");
            }
        }
    }
}
```

---

## 🎯 Phase 4: Game Mechanics (Week 4)

### **Step 4.1: Scoring System**

#### **Score Manager**
```csharp
// EndlessRunner/Scoring/RunnerScoreManager.cs
public class RunnerScoreManager : BaseScoreManager
{
    private float distanceTraveled = 0f;
    
    public RunnerScoreManager(IEventBus eventBus) : base(eventBus)
    {
        // Subscribe to runner-specific events
        eventBus.Subscribe<PlayerMovementEvent>(OnPlayerMoved);
        eventBus.Subscribe<CollectiblePickedUpEvent>(OnCollectiblePickedUp);
        eventBus.Subscribe<GameStartedEvent>(OnGameStarted);
        eventBus.Subscribe<GameEndedEvent>(OnGameEnded);
        
        Debug.Log("[RunnerScoreManager] ✅ Runner score manager initialized");
    }
    
    protected override void LoadHighScore()
    {
        _highScore = PlayerPrefs.GetInt("RunnerHighScore", 0);
        Debug.Log($"[RunnerScoreManager] 📈 Loaded high score: {_highScore}");
    }
    
    protected override void SaveHighScore()
    {
        PlayerPrefs.SetInt("RunnerHighScore", _highScore);
        PlayerPrefs.Save();
        Debug.Log($"[RunnerScoreManager] 💾 Saved high score: {_highScore}");
    }
    
    protected override int CalculateScore(int basePoints, int multiplier = 1)
    {
        // Runner-specific score calculation with distance bonus
        var distanceBonus = Mathf.FloorToInt(distanceTraveled * 0.1f);
        return (basePoints + distanceBonus) * multiplier;
    }
    
    private void OnPlayerMoved(PlayerMovementEvent movementEvent)
    {
        // Calculate score based on distance
        distanceTraveled += movementEvent.MovementDelta.magnitude;
        var distanceScore = Mathf.FloorToInt(distanceTraveled);
        SetScore(distanceScore);
        
        Debug.Log($"[RunnerScoreManager] 📊 Distance score: {distanceScore}");
    }
    
    private void OnCollectiblePickedUp(CollectiblePickedUpEvent collectibleEvent)
    {
        // Add bonus points for collectibles
        AddScore(collectibleEvent.CollectibleValue);
        
        Debug.Log($"[RunnerScoreManager] 💰 Collected {collectibleEvent.CollectibleValue} points from collectible");
    }
    
    private void OnGameStarted(GameStartedEvent gameStartedEvent)
    {
        ResetScore();
        distanceTraveled = 0f;
        
        Debug.Log("[RunnerScoreManager] 🎮 Score reset for new game");
    }
    
    private void OnGameEnded(GameEndedEvent gameEndedEvent)
    {
        EndGame();
        
        Debug.Log($"[RunnerScoreManager] 🏁 Game ended with score: {_currentScore}");
    }
}
```

---

## 🎨 Phase 5: Polish & Integration (Week 5)

### **Step 5.1: UI Integration**

#### **UI Manager**
```csharp
// EndlessRunner/UI/RunnerUIManager.cs
public class RunnerUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    
    private IEventBus eventBus;
    
    private void Start()
    {
        eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
        
        // Subscribe to UI events
        eventBus?.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        eventBus?.Subscribe<HighScoreEvent>(OnHighScoreChanged);
        eventBus?.Subscribe<GameOverEvent>(OnGameOver);
        eventBus?.Subscribe<GamePausedEvent>(OnGamePaused);
        eventBus?.Subscribe<GameResumedEvent>(OnGameResumed);
        
        // Initialize UI
        UpdateScoreDisplay(0);
        UpdateHighScoreDisplay();
        
        Debug.Log("[RunnerUIManager] ✅ UI Manager initialized");
    }
    
    private void OnScoreChanged(ScoreChangedEvent scoreEvent)
    {
        UpdateScoreDisplay(scoreEvent.NewScore);
    }
    
    private void OnHighScoreChanged(HighScoreEvent highScoreEvent)
    {
        UpdateHighScoreDisplay(highScoreEvent.HighScore);
    }
    
    private void OnGameOver(GameOverEvent gameOverEvent)
    {
        ShowGameOverPanel(gameOverEvent.FinalScore, gameOverEvent.GameOverReason);
    }
    
    private void OnGamePaused(GamePausedEvent pauseEvent)
    {
        ShowPausePanel();
    }
    
    private void OnGameResumed(GameResumedEvent resumeEvent)
    {
        HidePausePanel();
    }
    
    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score:N0}";
        }
    }
    
    private void UpdateHighScoreDisplay(int? highScore = null)
    {
        if (highScoreText != null)
        {
            var currentHighScore = highScore ?? PlayerPrefs.GetInt("RunnerHighScore", 0);
            highScoreText.text = $"High Score: {currentHighScore:N0}";
        }
    }
    
    private void ShowGameOverPanel(int finalScore, string reason)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // Update game over panel with final score
            var finalScoreText = gameOverPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Game Over!\nFinal Score: {finalScore:N0}\nReason: {reason}";
            }
        }
    }
    
    private void ShowPausePanel()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }
    
    private void HidePausePanel()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }
}
```

---

## 📊 Success Metrics

### **Technical Metrics**
- ✅ **Performance**: 60 FPS on target devices
- ✅ **Memory Usage**: < 100MB RAM usage
- ✅ **Load Times**: < 3 seconds initial load
- ✅ **Code Quality**: SOLID principles compliance
- ✅ **Base Class Usage**: Proper inheritance from Core/Common

### **Gameplay Metrics**
- ✅ **Responsiveness**: < 100ms input lag
- ✅ **Smoothness**: No frame drops during gameplay
- ✅ **Scalability**: Easy to add new features
- ✅ **Maintainability**: Clear code structure
- ✅ **Extensibility**: Easy to add new mini-games
- ✅ **Consistency**: Same patterns across all mini-games

---

## 🚀 Implementation Priority

### **Week 1: Foundation**
1. ✅ Create Core/Common base classes (if not exists)
2. ✅ Implement EndlessRunnerGame.cs
3. ✅ Implement RunnerStateManager.cs
4. ✅ Implement RunnerInputManager.cs
5. ✅ Create event system foundation

### **Week 2: Core Gameplay**
1. 🎯 PlayerController with 3D movement
2. 🎯 WorldGenerator with chunk system
3. 🎯 Basic collision detection
4. 🎯 Input system integration

### **Week 3: Game Mechanics**
1. 🎯 ScoreManager with event integration
2. 🎯 Collectibles system
3. 🎯 Obstacles system
4. 🎯 Performance optimization

### **Week 4: Polish & Integration**
1. 🎯 UI integration
2. 🎯 Save system integration
3. 🎯 Final testing & debugging
4. 🎯 Performance optimization

---

*This roadmap provides a clear, structured approach to developing the 3D Endless Runner using hybrid core systems, focusing on clean code structure and maintainability over feature quantity.* 