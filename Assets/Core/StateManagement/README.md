# Game State Management System

A robust and flexible state management system for the MiniGameFramework, providing centralized control over game flow and state transitions.

## 🎯 Overview

The Game State Management system enables:
- **Centralized State Control**: Single source of truth for game state
- **Validation Rules**: Prevent invalid state transitions
- **Event-Driven Architecture**: Loose coupling through events
- **Custom Rules**: Add runtime validation logic
- **State History**: Track state changes for debugging

## 📁 Components

### Core Files
- `GameState.cs` - State enum and events
- `IGameStateManager.cs` - Interface definition
- `GameStateManager.cs` - Main implementation
- `GameStateManagerTester.cs` - Comprehensive test system
- `GameStateManagerTestSetup.cs` - Scene setup utility

## 🚀 Quick Start

### 1. Basic Usage

```csharp
// Get state manager from ServiceLocator
var stateManager = ServiceLocator.Instance.Resolve<IGameStateManager>();

// Transition to new state
bool success = stateManager.TransitionToState(GlobalGameState.Loading, "Starting new game");

// Check current state
GlobalGameState current = stateManager.CurrentState;

// Get valid transitions
var validStates = stateManager.GetValidTransitions();
```

### 2. Event Handling

```csharp
// Subscribe to state changes
var eventBus = ServiceLocator.Instance.Resolve<IEventBus>();

var subscription = eventBus.Subscribe<GlobalGameStateChangedEvent>(OnStateChanged);

private void OnStateChanged(GlobalGameStateChangedEvent stateEvent)
{
    Debug.Log($"State changed: {stateEvent.PreviousState} -> {stateEvent.CurrentState}");
}

// Don't forget to dispose subscription
subscription.Dispose();
```

### 3. Custom Validation Rules

```csharp
// Add custom rule to prevent certain transitions
stateManager.AddTransitionRule(GlobalGameState.Playing, GlobalGameState.Menu, () => 
{
    // Only allow if player isn't in middle of action
    return !PlayerController.IsPerformingAction;
});

// Remove custom rule when no longer needed
stateManager.RemoveTransitionRule(GlobalGameState.Playing, GlobalGameState.Menu);
```

## 🎮 Game States

### Available States

| State | Description |
|-------|-------------|
| `Menu` | Main menu state - game selection and options |
| `Loading` | Transitioning between scenes or initializing |
| `Playing` | Active gameplay state |
| `Paused` | Game paused but can be resumed |
| `GameOver` | Game ended, showing results |

### Default Transition Rules

```
Menu ──────► Loading
Loading ───► Playing, Menu
Playing ───► Paused, GameOver, Loading, Menu
Paused ────► Playing, Menu, GameOver
GameOver ──► Menu, Loading, Playing
```

## 🧪 Testing

### Automatic Scene Setup

1. In Unity Editor, go to `MiniGameFramework/Setup/Create GameStateManager Test Scene`
2. This creates a fully configured test scene with UI and tester component
3. Run the scene to see automatic tests and manual controls

### Manual Testing

The test scene provides:
- **State Buttons**: Manual state transitions (Menu, Loading, Playing, Paused, GameOver)
- **Test Buttons**: Automated test sequences
- **Real-time Display**: Current state, valid transitions, state history
- **Result Tracking**: Pass/fail counts for all tests

### Test Categories

1. **Bootstrap Integration**: Verify services are properly initialized
2. **Valid Transitions**: Test all allowed state changes
3. **Invalid Transitions**: Ensure blocked transitions fail correctly
4. **Custom Rules**: Test runtime validation logic
5. **Event System**: Verify events are published correctly
6. **State History**: Check history tracking functionality

## 🔧 Integration

### With GameBootstrap

The system is automatically initialized and registered with ServiceLocator:

```csharp
// GameStateManager is registered in GameBootstrap
var stateManager = ServiceLocator.Instance.Resolve<IGameStateManager>();
```

### With MiniGames

```csharp
public class MyMiniGame : MiniGameBase
{
    private IGameStateManager stateManager;
    
    protected override void OnInitialize()
    {
        stateManager = ServiceLocator.Instance.Resolve<IGameStateManager>();
        
        // Listen for state changes
        var eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
        eventBus.Subscribe<GlobalGameStateChangedEvent>(OnGameStateChanged);
    }
    
    private void OnGameStateChanged(GlobalGameStateChangedEvent stateEvent)
    {
        if (stateEvent.CurrentState == GlobalGameState.Paused)
        {
            PauseGame();
        }
        else if (stateEvent.CurrentState == GlobalGameState.Playing)
        {
            ResumeGame();
        }
    }
}
```

### With UI System

```csharp
public class PausePanel : UIPanel
{
    private IGameStateManager stateManager;
    
    protected override void OnShow()
    {
        stateManager = ServiceLocator.Instance.Resolve<IGameStateManager>();
        stateManager.TransitionToState(GlobalGameState.Paused);
    }
    
    public void OnResumeButton()
    {
        stateManager.TransitionToState(GlobalGameState.Playing);
        HideAsync();
    }
}
```

## 📊 Events

### GlobalGameStateChangedEvent

Fired when state successfully changes:

```csharp
public class GlobalGameStateChangedEvent : GameEvent
{
    public GlobalGameState PreviousState { get; }
    public GlobalGameState CurrentState { get; }
    public object StateData { get; }        // Optional data
    public float Timestamp { get; }         // When event occurred
    public object Source { get; }           // What triggered the change
}
```

### StateTransitionFailedEvent

Fired when a transition is rejected:

```csharp
public class StateTransitionFailedEvent : GameEvent
{
    public GlobalGameState CurrentState { get; }
    public GlobalGameState RequestedState { get; }
    public string FailureReason { get; }
}
```

## 🛠️ Advanced Features

### State History

```csharp
// Get complete state history
var history = stateManager.GetStateHistory();
Debug.Log($"State path: {string.Join(" -> ", history)}");

// Reset to clear history
stateManager.Reset();
```

### Conditional Transitions

```csharp
// Complex validation example
stateManager.AddTransitionRule(GlobalGameState.Playing, GlobalGameState.GameOver, () =>
{
    var playerHealth = PlayerStats.Health;
    var gameTime = GameManager.ElapsedTime;
    
    // Only allow game over if player is dead OR time is up
    return playerHealth <= 0 || gameTime >= GameManager.MaxTime;
});
```

### State Data

```csharp
// Pass data with state transitions
var gameOverData = new { Score = 1500, Reason = "Victory" };
stateManager.TransitionToState(GlobalGameState.GameOver, gameOverData);
```

## 🎯 Best Practices

1. **Single Responsibility**: Use for global game flow, not mini-game specific states
2. **Event-Driven**: Subscribe to events rather than polling current state
3. **Validation**: Add custom rules for complex business logic
4. **Testing**: Always test state transitions in your mini-games
5. **Cleanup**: Dispose event subscriptions to prevent memory leaks

## 🔍 Debugging

### Console Output

The system provides detailed logging:
```
[GameStateManager] State transition: Menu -> Loading
[GameStateManager] Added custom transition rule: Playing -> GameOver
[GameStateManager] Invalid transition from Loading to Paused
```

### State Display

In test scene, real-time display shows:
- Current and previous states
- Valid transition options
- Complete state history
- Event notifications

## 🚧 Architecture Notes

- **Dependency Injection**: Uses ServiceLocator pattern
- **SOLID Principles**: Interface-based design for testability
- **Event-Driven**: Loose coupling through EventBus
- **Validation**: Flexible rule system for business logic
- **Thread-Safe**: Safe for use from main thread (Unity requirement)

---

For more examples and advanced usage, check the `GameStateManagerTester.cs` implementation. 