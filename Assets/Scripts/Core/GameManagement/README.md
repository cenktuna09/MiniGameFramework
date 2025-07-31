# 🎮 Game Management System

A comprehensive mini-game lifecycle and session management system that coordinates between different games, tracks player progress, and manages scoring and timing systems.

## 📁 System Overview

The Game Management System provides centralized control over mini-game lifecycle, session tracking, progress monitoring, and timer management. It serves as the coordination layer between the global state management and individual mini-games.

### 🔧 Core Components

#### **1. IGameManager** - Main Interface
- Manages mini-game lifecycle (load, start, pause, resume, end, unload)
- Tracks current game sessions and scoring
- Coordinates with global state management
- Provides async loading/unloading capabilities

#### **2. GameManager** - Central Coordinator
- Implements `IGameManager` interface
- Manages mini-game registration and instantiation
- Handles session creation and lifecycle
- Integrates with SaveSystem for persistent best scores
- Coordinates with GlobalGameStateManager

#### **3. GameSession** - Session Tracking
- Tracks individual game session data
- Manages scoring, timing, and session statistics
- Provides best score tracking
- Handles session lifecycle (start, active, end)

#### **4. ProgressSystem** - Progress Tracking
- Reusable progress tracking for goals and achievements
- Step-based and percentage-based progress
- Event-driven progress updates
- Configurable total steps and progress validation

#### **5. TimerSystem** - Timing Management
- Dual-mode timer (Countdown / Stopwatch)
- Pause/resume functionality
- Frame-independent timing
- Event-driven timer completion
- Formatted time display utilities

## 🎯 Quick Start

### Basic Setup

1. **Create GameManager Instance**:
```csharp
// GameManager is typically created by GameBootstrap
var gameManager = ServiceLocator.Instance.Resolve<IGameManager>();
```

2. **Register Mini-Games**:
```csharp
gameManager.RegisterGame<Match3Game>("Match3");
gameManager.RegisterGame<EndlessRunnerGame>("EndlessRunner");
```

3. **Load and Start Game**:
```csharp
// Load game
bool loadSuccess = await gameManager.LoadGameAsync("Match3");

if (loadSuccess)
{
    // Start game
    bool startSuccess = gameManager.StartCurrentGame();
}
```

### Session Management

```csharp
// Access current session
var session = gameManager.CurrentSession;

// Update score
session.UpdateScore(150);
session.AddScore(50);

// End session
session.EndSession(GameResult.Victory);

// Get session statistics
var stats = session.GetStats();
```

### Progress Tracking

```csharp
// Create progress system
var progress = new ProgressSystem(10); // 10 total steps

// Subscribe to events
progress.OnProgressChanged += (progress) => Debug.Log($"Progress: {progress:P}");
progress.OnProgressComplete += () => Debug.Log("Progress Complete!");

// Update progress
progress.AdvanceStep();           // Step-based
progress.SetProgress(0.75f);      // Percentage-based
```

### Timer Management

```csharp
// Create countdown timer
var timer = new TimerSystem(TimerMode.Countdown, 60f); // 60 seconds

// Subscribe to events
timer.OnTimerTick += (timeRemaining) => Debug.Log($"Time: {timeRemaining:F1}s");
timer.OnTimerComplete += () => Debug.Log("Timer finished!");

// Control timer
timer.Start();
timer.Pause();
timer.Resume();
timer.Stop();
```

## 🏗️ Game Manager States

The GameManager follows a clear state machine:

| State | Description | Valid Transitions |
|-------|-------------|------------------|
| `Idle` | No game loaded | → Loading |
| `Loading` | Loading mini-game | → Ready, → Idle |
| `Ready` | Game loaded, ready to start | → Playing, → Unloading |
| `Playing` | Game actively running | → Paused, → GameOver |
| `Paused` | Game paused | → Playing, → GameOver |
| `GameOver` | Game ended | → Unloading |
| `Unloading` | Cleaning up game | → Idle |

## 🎮 Game Results

Games can end with different results:

- **Victory**: Player completed successfully
- **Defeat**: Player failed to complete
- **Quit**: Player quit the game early
- **TimeOut**: Game ended due to time limit

## 🧪 Testing System

### Comprehensive Test Coverage

The system includes a comprehensive testing framework:

**GameManagerTester Features**:
- ✅ Bootstrap integration testing
- ✅ Game lifecycle testing
- ✅ Session tracking validation
- ✅ Progress and timer system testing
- ✅ Real-time UI displays
- ✅ Manual control interfaces
- ✅ Automated test sequences

### Creating Test Scene

Use the Unity MenuItem to create a complete test environment:

```
Unity Menu → MiniGameFramework → Setup → Create GameManager Test Scene
```

This automatically creates:
- Complete UI layout with controls
- GameBootstrap integration
- GameManagerTester component
- Real-time status displays
- Test result tracking

### Test Categories

1. **Bootstrap Integration**:
   - Service registration validation
   - Dependency resolution testing
   - EventBus integration verification

2. **Game Lifecycle**:
   - Registration and loading testing
   - State transition validation
   - Async operation testing

3. **Session Tracking**:
   - Score management testing
   - Session lifecycle validation
   - Best score tracking

4. **Progress & Timer Systems**:
   - Progress advancement testing
   - Timer functionality validation
   - Event system integration

## 🔧 Integration with Other Systems

### ServiceLocator Integration

```csharp
// Registration (done by GameBootstrap)
ServiceLocator.Instance.Register<IGameManager>(gameManagerInstance);

// Resolution
var gameManager = ServiceLocator.Instance.Resolve<IGameManager>();
```

### EventBus Integration

The GameManager publishes and subscribes to various events:

```csharp
// Subscribe to global state changes
eventBus.Subscribe<GlobalGameStateChangedEvent>(OnGlobalStateChanged);

// Listen to game manager state changes
gameManager.OnStateChanged += (newState) => Debug.Log($"State: {newState}");
gameManager.OnGameLoaded += (game) => Debug.Log($"Game loaded: {game.GameId}");
gameManager.OnGameUnloaded += (gameId) => Debug.Log($"Game unloaded: {gameId}");
```

### SaveSystem Integration

```csharp
// Best scores are automatically saved/loaded
int bestScore = gameManager.GetBestScore("Match3");

// Session results update best scores automatically
gameManager.EndCurrentGame(GameResult.Victory); // Saves if new best score
```

## 🎯 Advanced Features

### Custom Game Registration

```csharp
public class CustomMiniGame : MonoBehaviour, IMiniGame
{
    public string GameId => "CustomGame";
    public GameState State { get; private set; }
    
    public event Action<GameState> OnStateChanged;
    
    public async Task InitializeAsync()
    {
        // Custom initialization logic
        State = GameState.Ready;
        OnStateChanged?.Invoke(State);
    }
    
    // Implement other IMiniGame methods...
}

// Register custom game
gameManager.RegisterGame<CustomMiniGame>("CustomGame");
```

### Progress System Extensions

```csharp
// Dynamic progress steps
var progress = new ProgressSystem(5);
progress.SetTotalSteps(10); // Dynamically change total steps

// Progress events
progress.OnProgressChanged += (value) => {
    // Update UI progress bar
    progressBar.value = value;
};
```

### Timer System Modes

```csharp
// Countdown timer (counts down from duration)
var countdown = new TimerSystem(TimerMode.Countdown, 120f);

// Stopwatch timer (counts up from zero)
var stopwatch = new TimerSystem(TimerMode.Stopwatch);

// Switch modes at runtime
timer.SetMode(TimerMode.Countdown, resetTimer: true);
```

## 📊 Best Practices

### 1. **Lifecycle Management**
```csharp
// Always check state before operations
if (gameManager.State == GameManagerState.Ready)
{
    gameManager.StartCurrentGame();
}

// Handle async operations properly
try
{
    bool success = await gameManager.LoadGameAsync("GameId");
    if (!success)
    {
        Debug.LogError("Failed to load game");
    }
}
catch (Exception ex)
{
    Debug.LogError($"Game loading error: {ex.Message}");
}
```

### 2. **Session Management**
```csharp
// Check session validity before operations
if (gameManager.CurrentSession?.IsActive == true)
{
    gameManager.CurrentSession.UpdateScore(newScore);
}

// Always end sessions properly
gameManager.EndCurrentGame(GameResult.Victory);
```

### 3. **Progress Tracking**
```csharp
// Use events for UI updates
progress.OnProgressChanged += UpdateProgressBar;
progress.OnProgressComplete += ShowCompletionFeedback;

// Validate progress values
progress.SetProgress(Mathf.Clamp01(calculatedProgress));
```

### 4. **Timer Management**
```csharp
// Update timers in game loop
private void Update()
{
    timer?.Update(Time.deltaTime);
}

// Clean up timer events
private void OnDestroy()
{
    if (timer != null)
    {
        timer.OnTimerComplete -= OnTimerComplete;
        timer.Stop();
    }
}
```

## 🐛 Debugging

### Common Issues

1. **GameManager Not Registered**
   ```csharp
   // Check registration
   if (!ServiceLocator.Instance.IsRegistered<IGameManager>())
   {
       Debug.LogError("GameManager not registered!");
   }
   ```

2. **Invalid State Transitions**
   ```csharp
   // Check current state before operations
   Debug.Log($"Current state: {gameManager.State}");
   ```

3. **Session Null Reference**
   ```csharp
   // Always null-check sessions
   if (gameManager.CurrentSession != null)
   {
       // Safe to use session
   }
   ```

### Debug Logging

Enable debug logging for detailed information:
```csharp
// GameManager logs all state changes and operations
// Check Unity Console for [GameManager] messages

// Session logs score updates and lifecycle events
// Check Unity Console for [GameSession] messages
```

## 🚀 Performance Considerations

### 1. **Memory Management**
- GameSessions are lightweight and disposable
- Timer systems use minimal memory footprint
- Progress systems avoid unnecessary allocations

### 2. **Update Performance**
- Timer updates are frame-independent
- Progress updates only fire events when values change
- No Update() loops in core GameManager

### 3. **Async Operations**
- Game loading is fully asynchronous
- No blocking operations in main thread
- Proper exception handling for async operations

## 🔮 Future Enhancements

Planned improvements for the Game Management system:

- **Achievement System**: Integration with progress tracking
- **Leaderboards**: Extended scoring with online features
- **Game Analytics**: Session data collection and analysis
- **Save Game States**: Checkpoint and resume functionality
- **Multi-Session Support**: Concurrent session management

---

The Game Management System provides a robust foundation for mini-game coordination, session tracking, and player progress management. It follows SOLID principles, integrates seamlessly with other framework systems, and includes comprehensive testing capabilities. 