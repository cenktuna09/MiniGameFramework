# 🎯 Hybrid Core Systems Architecture

## 📋 Overview

Bu doküman, MiniGameFramework'te hibrit core sistemler yaklaşımını detaylandırır. Mevcut Match3 ve EndlessRunner yapılarını analiz ederek, ortak sistemleri Core'a taşıma ve mini-game özel sistemlerini koruma stratejisini açıklar.

## 🏗️ Current State Analysis

### Match3 Mevcut Yapısı
```
Match3/
├── Input/
│   ├── Match3InputHandler.cs
│   ├── Match3InputManager.cs
│   └── Commands/
├── Config/
│   └── Match3Config.cs
├── Data/
│   ├── TileData.cs
│   └── TilePositionCache.cs
├── Logic/
│   ├── Match3GameLogicManager.cs
│   └── MatchDetector.cs
├── Visual/
│   ├── Match3AnimationManager.cs
│   ├── TileVisual.cs
│   └── Strategies/
├── Performance/
│   ├── Match3PerformanceManager.cs
│   ├── Match3LazyEvaluator.cs
│   └── Match3OptimizedSwapDetector.cs
├── ErrorHandling/
│   └── Match3ErrorHandler.cs
├── Events/
│   └── Match3GravityEvents.cs
├── Utils/
│   ├── Match3FoundationManager.cs
│   └── Match3MemoryManager.cs
├── Board/
│   └── BoardGenerator.cs
└── Pooling/
    ├── TilePool.cs
    └── PooledTile.cs
```

### EndlessRunner Mevcut Yapısı
```
EndlessRunner/
├── RunnerCore/     (boş)
├── Player/         (boş)
├── Input/          (boş)
├── Config/         (boş)
├── Data/           (boş)
├── Utils/          (boş)
├── Performance/    (boş)
├── Testing/        (boş)
├── Scoring/        (boş)
├── Collectibles/   (boş)
├── Obstacles/      (boş)
└── World/          (boş)
```

## 🎯 Hybrid Architecture Plan

### 1. Core/Common Sistemleri (Ortak Base Sınıflar)

```
Assets/Scripts/Core/Common/
├── StateManagement/
│   ├── BaseGameStateManager.cs
│   ├── BaseGameState.cs
│   └── IGameStateManager.cs
├── InputManagement/
│   ├── BaseInputManager.cs
│   ├── BaseInputHandler.cs
│   ├── BaseInputCommand.cs
│   └── IInputManager.cs
├── ConfigManagement/
│   ├── BaseGameConfig.cs
│   ├── ConfigValidator.cs
│   └── IGameConfig.cs
├── DataManagement/
│   ├── BaseGameData.cs
│   ├── BasePositionCache.cs
│   └── IGameData.cs
├── PerformanceManagement/
│   ├── BasePerformanceManager.cs
│   ├── BaseLazyEvaluator.cs
│   └── IPerformanceManager.cs
├── ScoringManagement/
│   ├── BaseScoreManager.cs
│   ├── ScoreValidator.cs
│   └── IScoreManager.cs
├── ErrorHandling/
│   ├── BaseErrorHandler.cs
│   ├── ErrorValidator.cs
│   └── IErrorHandler.cs
├── AnimationManagement/
│   ├── BaseAnimationManager.cs
│   ├── BaseAnimationStrategy.cs
│   └── IAnimationManager.cs
├── Pooling/
│   ├── BaseObjectPool.cs
│   ├── IPoolable.cs
│   └── IObjectPool.cs
└── Utils/
    ├── BaseFoundationManager.cs
    ├── BaseMemoryManager.cs
    └── IGameUtils.cs
```

### 2. Mini-Game Özel Sistemleri

#### Match3 Özel Sistemleri
```
Match3/
├── StateManagement/
│   ├── Match3StateManager.cs (extends BaseGameStateManager)
│   └── Match3GameState.cs (extends BaseGameState)
├── InputManagement/
│   ├── Match3InputManager.cs (extends BaseInputManager)
│   ├── Match3InputHandler.cs (extends BaseInputHandler)
│   └── Commands/
│       ├── Match3SwapCommand.cs (extends BaseInputCommand)
│       └── Match3SelectCommand.cs (extends BaseInputCommand)
├── ConfigManagement/
│   └── Match3Config.cs (extends BaseGameConfig)
├── DataManagement/
│   ├── TileData.cs (extends BaseGameData)
│   └── TilePositionCache.cs (extends BasePositionCache)
├── Logic/
│   ├── Match3GameLogicManager.cs (game-specific)
│   └── MatchDetector.cs (game-specific)
├── Visual/
│   ├── Match3AnimationManager.cs (extends BaseAnimationManager)
│   ├── TileVisual.cs (game-specific)
│   └── Strategies/
│       └── Match3AnimationStrategy.cs (extends BaseAnimationStrategy)
├── Performance/
│   ├── Match3PerformanceManager.cs (extends BasePerformanceManager)
│   ├── Match3LazyEvaluator.cs (extends BaseLazyEvaluator)
│   └── Match3OptimizedSwapDetector.cs (game-specific)
├── ErrorHandling/
│   └── Match3ErrorHandler.cs (extends BaseErrorHandler)
├── Events/
│   └── Match3GravityEvents.cs (game-specific)
├── Utils/
│   ├── Match3FoundationManager.cs (extends BaseFoundationManager)
│   └── Match3MemoryManager.cs (extends BaseMemoryManager)
├── Board/
│   └── BoardGenerator.cs (game-specific)
└── Pooling/
    ├── TilePool.cs (extends BaseObjectPool)
    └── PooledTile.cs (implements IPoolable)
```

#### EndlessRunner Özel Sistemleri
```
EndlessRunner/
├── StateManagement/
│   ├── RunnerStateManager.cs (extends BaseGameStateManager)
│   └── RunnerGameState.cs (extends BaseGameState)
├── InputManagement/
│   ├── RunnerInputManager.cs (extends BaseInputManager)
│   ├── RunnerInputHandler.cs (extends BaseInputHandler)
│   └── Commands/
│       ├── RunnerJumpCommand.cs (extends BaseInputCommand)
│       ├── RunnerSlideCommand.cs (extends BaseInputCommand)
│       └── RunnerMoveCommand.cs (extends BaseInputCommand)
├── ConfigManagement/
│   └── RunnerConfig.cs (extends BaseGameConfig)
├── DataManagement/
│   ├── RunnerGameData.cs (extends BaseGameData)
│   └── RunnerPositionCache.cs (extends BasePositionCache)
├── RunnerCore/
│   ├── RunnerGameLogicManager.cs (game-specific)
│   └── RunnerLevelGenerator.cs (game-specific)
├── Player/
│   ├── RunnerPlayerController.cs (game-specific)
│   └── RunnerPlayerData.cs (game-specific)
├── World/
│   ├── RunnerWorldManager.cs (game-specific)
│   └── RunnerChunkGenerator.cs (game-specific)
├── Obstacles/
│   ├── RunnerObstacleManager.cs (game-specific)
│   └── RunnerObstaclePool.cs (extends BaseObjectPool)
├── Collectibles/
│   ├── RunnerCollectibleManager.cs (game-specific)
│   └── RunnerCollectiblePool.cs (extends BaseObjectPool)
├── Scoring/
│   ├── RunnerScoreManager.cs (extends BaseScoreManager)
│   └── RunnerScoreCalculator.cs (game-specific)
├── Performance/
│   ├── RunnerPerformanceManager.cs (extends BasePerformanceManager)
│   └── RunnerLazyEvaluator.cs (extends BaseLazyEvaluator)
├── ErrorHandling/
│   └── RunnerErrorHandler.cs (extends BaseErrorHandler)
├── Events/
│   ├── RunnerGameEvents.cs (game-specific)
│   └── RunnerPlayerEvents.cs (game-specific)
└── Utils/
    ├── RunnerFoundationManager.cs (extends BaseFoundationManager)
    └── RunnerMemoryManager.cs (extends BaseMemoryManager)
```

## 🔧 Implementation Strategy

### Phase 1: Core Base Sınıfları Oluşturma

#### 1.1 BaseGameStateManager
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
```

#### 1.2 BaseInputManager
```csharp
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

#### 1.3 BaseGameConfig
```csharp
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

#### 1.4 BasePerformanceManager
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
```

#### 1.5 BaseScoreManager
```csharp
// Core/Common/ScoringManagement/BaseScoreManager.cs
public abstract class BaseScoreManager
{
    protected IEventBus eventBus;
    protected int currentScore = 0;
    protected int highScore = 0;
    protected int scoreMultiplier = 1;
    protected List<int> scoreHistory;
    
    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    public int ScoreMultiplier => scoreMultiplier;
    
    public event Action<int, int> OnScoreChanged;
    public event Action<int> OnHighScoreAchieved;
    
    protected abstract void LoadHighScore();
    protected abstract void SaveHighScore();
    protected abstract int CalculateScore(int basePoints, int multiplier = 1);
    
    public virtual void AddScore(int points)
    {
        var calculatedPoints = CalculateScore(points, scoreMultiplier);
        currentScore += calculatedPoints;
        OnScoreChanged?.Invoke(currentScore, calculatedPoints);
    }
    
    public virtual void UpdateHighScore()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            OnHighScoreAchieved?.Invoke(highScore);
        }
    }
}
```

#### 1.6 BaseErrorHandler
```csharp
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

### Phase 2: Mini-Game Özel Implementasyonları

#### 2.1 Match3 Özel Sistemleri
```csharp
// Match3/StateManagement/Match3StateManager.cs
public class Match3StateManager : BaseGameStateManager<Match3GameState>
{
    protected override void SetupDefaultTransitionRules()
    {
        // Match3-specific transition rules
        AddTransitionRule(Match3GameState.Ready, Match3GameState.Playing);
        AddTransitionRule(Match3GameState.Playing, Match3GameState.Paused);
        AddTransitionRule(Match3GameState.Playing, Match3GameState.Matching);
        AddTransitionRule(Match3GameState.Matching, Match3GameState.Playing);
        AddTransitionRule(Match3GameState.Paused, Match3GameState.Playing);
        AddTransitionRule(Match3GameState.Playing, Match3GameState.GameOver);
    }
    
    public override bool CanTransitionTo(Match3GameState newState)
    {
        // Match3-specific validation logic
        if (newState == Match3GameState.Matching && currentState != Match3GameState.Playing)
            return false;
            
        return base.CanTransitionTo(newState);
    }
}

// Match3/InputManagement/Match3InputManager.cs
public class Match3InputManager : BaseInputManager
{
    private Match3InputHandler inputHandler;
    
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
    
    private BaseInputCommand CreateCommandFromInput(Match3InputResult result)
    {
        // Match3-specific command creation
        return new Match3SwapCommand(result.TileA, result.TileB);
    }
}
```

#### 2.2 EndlessRunner Özel Sistemleri
```csharp
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
    }
    
    protected override void LoadHighScore()
    {
        _highScore = PlayerPrefs.GetInt("RunnerHighScore", 0);
    }
    
    protected override void SaveHighScore()
    {
        PlayerPrefs.SetInt("RunnerHighScore", _highScore);
        PlayerPrefs.Save();
    }
    
    protected override int CalculateScore(int basePoints, int multiplier = 1)
    {
        // Runner-specific score calculation
        return basePoints * multiplier;
    }
    
    private void OnPlayerMoved(PlayerMovementEvent movementEvent)
    {
        // Calculate score based on distance
        distanceTraveled += movementEvent.MovementDelta.magnitude;
        var distanceScore = Mathf.FloorToInt(distanceTraveled);
        SetScore(distanceScore);
    }
    
    private void OnCollectiblePickedUp(CollectiblePickedUpEvent collectibleEvent)
    {
        // Add bonus points for collectibles
        AddScore(collectibleEvent.CollectibleValue);
    }
    
    private void OnGameStarted(GameStartedEvent gameStartedEvent)
    {
        ResetScore();
        distanceTraveled = 0f;
    }
    
    private void OnGameEnded(GameEndedEvent gameEndedEvent)
    {
        EndGame();
    }
}
```

## 🎯 Migration Plan

### Step 1: Core Base Sınıfları Oluştur
1. `Core/Common/StateManagement/` klasörünü oluştur
2. `BaseGameStateManager.cs` implement et
3. `Core/Common/InputManagement/` klasörünü oluştur
4. `BaseInputManager.cs` implement et
5. Diğer base sınıfları sırayla oluştur

### Step 2: Match3 Sistemlerini Migrate Et
1. `Match3StateManager` oluştur (BaseGameStateManager'dan extend et)
2. `Match3InputManager` güncelle (BaseInputManager'dan extend et)
3. `Match3Config` güncelle (BaseGameConfig'den extend et)
4. Diğer Match3 sistemlerini sırayla migrate et

### Step 3: EndlessRunner Sistemlerini Oluştur
1. `RunnerStateManager` oluştur (BaseGameStateManager'dan extend et)
2. `RunnerInputManager` oluştur (BaseInputManager'dan extend et)
3. `RunnerConfig` oluştur (BaseGameConfig'den extend et)
4. Diğer Runner sistemlerini sırayla oluştur

### Step 4: ServiceLocator Entegrasyonu
1. Base sınıfları ServiceLocator'a kaydet
2. Mini-game özel sistemleri ServiceLocator'dan resolve et
3. Dependency injection'ı test et

## 🎯 Benefits

### ✅ Avantajlar
- **Kod Tekrarı Yok**: Ortak sistemler Core'da
- **Tutarlılık**: Tüm mini-gameler aynı base sistemleri kullanır
- **Esneklik**: Her mini-game özelleştirebilir
- **Bakım Kolaylığı**: Base değişiklikler her yerde etkili
- **Test Edilebilirlik**: Base sınıflar ayrı test edilebilir
- **Genişletilebilirlik**: Yeni mini-gameler kolayca eklenebilir

### ⚠️ Dikkat Edilecekler
- **Interface Segregation**: Base sınıflar çok büyük olmasın
- **Dependency Inversion**: Abstract sınıflar concrete'lere bağımlı olmasın
- **Single Responsibility**: Her base sınıf tek sorumluluğa sahip olsun
- **Open/Closed Principle**: Base sınıflar genişletmeye açık, değişime kapalı olsun

## 🚀 Next Steps

1. **Core/Common klasör yapısını oluştur**
2. **İlk base sınıfı (BaseGameStateManager) implement et**
3. **Match3StateManager'ı migrate et**
4. **Test et ve iterasyon yap**
5. **Diğer base sınıfları sırayla ekle**

Bu hibrit yaklaşım, hem kod tekrarını önler hem de mini-gamelerin özel ihtiyaçlarını karşılar. Modüler ve sürdürülebilir bir mimari sağlar. 