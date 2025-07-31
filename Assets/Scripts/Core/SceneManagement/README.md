# Scene Management System

Unity 6 compatible scene management framework with async/await support, progress tracking, and smooth transitions.

## Features

- ✅ **Async Scene Loading**: Unity 6 Task-based scene management with proper async/await patterns
- ✅ **Progress Tracking**: Real-time loading progress with events and UI integration
- ✅ **Smooth Transitions**: Fade in/out transitions with customizable animations
- ✅ **Loading Screen**: Automatic loading screen with progress bar
- ✅ **Event-Driven**: Decoupled communication via EventBus
- ✅ **Preloading**: Scene preloading and activation for optimized performance
- ✅ **Dependency Injection**: Full DI support via ServiceLocator
- ✅ **Error Handling**: Robust error handling and validation

## Quick Start

### 1. Setup Scene Management System

```csharp
// Add SceneManagementBootstrapper to a GameObject in your scene
var bootstrapper = gameObject.AddComponent<SceneManagementBootstrapper>();

// Or manually initialize
var eventBus = new EventBus();
var transitionManager = GetComponent<SceneTransitionManager>();
var loadingScreen = GetComponent<LoadingScreen>();

bootstrapper.Setup(eventBus, transitionManager, loadingScreen);
```

### 2. Use Scene Manager

```csharp
// Get scene manager from ServiceLocator
var sceneManager = ServiceLocator.Instance.GetService<ISceneManager>();

// Load scene with transition
await sceneManager.LoadSceneAsync("GameScene");

// Reload current scene
await sceneManager.ReloadCurrentSceneAsync();

// Preload and activate
await sceneManager.PreloadSceneAsync("NextLevel");
// ... do other work ...
await sceneManager.ActivatePreloadedSceneAsync("NextLevel");
```

### 3. Listen to Events

```csharp
eventBus.Subscribe<SceneLoadingStartedEvent>(OnSceneLoadingStarted);
eventBus.Subscribe<SceneLoadingProgressEvent>(OnSceneLoadingProgress);
eventBus.Subscribe<SceneLoadingCompletedEvent>(OnSceneLoadingCompleted);
```

## Architecture

### Core Components

1. **ISceneManager** - Main interface for scene operations
2. **SceneManagerImpl** - Unity 6 implementation with async support
3. **SceneTransitionManager** - Manages different transition types
4. **FadeTransition** - Fade in/out transition implementation
5. **LoadingScreen** - UI component for loading progress
6. **SceneManagementBootstrapper** - Initializes and coordinates all components

### Event System

- `SceneLoadingStartedEvent` - Scene loading begins
- `SceneLoadingProgressEvent` - Loading progress updates
- `SceneLoadingCompletedEvent` - Scene loading finished
- `SceneTransitionEvent` - Transition state changes

### Data Structures

- `SceneTransitionData` - Configuration for transitions
- `SceneReference` - Scene reference with Addressables support
- `TransitionType` - Available transition types (Fade, Slide, Zoom, Custom)

## Testing

Use the `SceneManagerDemo` component to test all functionality:

### Keyboard Controls
- **R** - Reload current scene
- **L** - Load scene by name
- **T** - Test transition only
- **P** - Test preload and activate
- **H** - Show help

### Context Menu Tests
- Right-click on `SceneManagementBootstrapper` or `SceneManagerDemo`
- Select test methods from context menu

## Configuration

### Default Transition Settings
```csharp
var transitionData = new SceneTransitionData
{
    transitionType = TransitionType.Fade,
    transitionDuration = 0.5f,
    transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
    fadeColor = Color.black,
    showLoadingScreen = true,
    loadingText = "Loading..."
};
```

### Loading Screen Customization
```csharp
loadingScreen.SetProgress(0.5f);
loadingScreen.SetLoadingText("Loading Assets...");
loadingScreen.UpdateProgress(0.75f, "Almost Ready...");
```

## Best Practices

1. **Always use async/await** for scene operations
2. **Handle exceptions** properly in async methods
3. **Subscribe/Unsubscribe** to events correctly
4. **Use preloading** for performance-critical transitions
5. **Test thoroughly** with different scenarios

## Dependencies

- Unity 6 (required for Task-based SceneManager)
- TextMeshPro (for UI text components)
- Unity Input System (included in Core assembly)
- Unity Addressables (included in Core assembly)

## Performance Considerations

- Scene preloading uses additional memory
- Smooth progress updates run in Update loop
- Transition animations use unscaled time
- EventBus uses dictionaries for fast lookups

## Error Handling

The system includes comprehensive error handling:

- Validation of scene names and indices
- Protection against concurrent loading operations
- Proper cleanup of resources
- Graceful fallbacks for missing components

## Integration with MiniGame Framework

This system integrates seamlessly with the MiniGame Framework:

- Uses Core.Architecture interfaces
- Follows SOLID principles
- Implements dependency injection
- Maintains loose coupling via EventBus

## Extending the System

### Custom Transitions
```csharp
public class SlideTransition : MonoBehaviour, ISceneTransition
{
    public TransitionType TransitionType => TransitionType.Slide;
    // Implement interface methods...
}

// Register custom transition
transitionManager.RegisterTransition(TransitionType.Slide, slideTransition);
```

### Custom Loading Screens
Inherit from `LoadingScreen` or implement your own UI that listens to scene loading events.

### Addressables Integration
Use `SceneReference` with addressable keys for runtime loading of scene assets. 