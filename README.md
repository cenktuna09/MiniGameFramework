MiniGameFramework – Quick Overview
A modular Unity project for building and hosting multiple mini‑games under a shared core framework.

## Architecture

### Core Principles

1. **SOLID Design**: Each mini-game is decoupled from the core framework
2. **Event-Driven Communication**: All cross-module communication uses EventBus
3. **Service Locator Pattern**: Centralized dependency management
4. **Lifecycle Management**: Standardized initialization, start, pause, resume, end cycles
5. **Scene-Scoped Services**: Services are cleared on scene unload to prevent memory leaks

### Key Components

#### 1. Service Locator (`Core/DI/ServiceLocator.cs`)
- **Purpose**: Centralized dependency injection container
- **Features**:
  - Global services (persistent across scenes)
  - Scene-scoped services (cleared on scene change)
  - Factory pattern support for lazy instantiation
  - Thread-safe singleton implementation

```csharp
// Global service registration
ServiceLocator.Instance.RegisterGlobal<IEventBus>(eventBus);

// Scene-scoped service registration
ServiceLocator.Instance.RegisterScene<BoardManager>(boardManager);

// Service resolution
var eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
```

#### 2. Event Bus (`Core/Events/EventBus.cs`)
- **Purpose**: Decoupled communication between systems
- **Features**:
  - Type-safe event publishing and subscription
  - Zero-allocation event handling
  - Automatic subscription cleanup via IDisposable
  - Thread-safe publishing with pending subscription management

```csharp
// Subscribe to events
var subscription = eventBus.Subscribe<GameOverEvent>(OnGameOver);

// Publish events
eventBus.Publish(new GameOverEvent { Score = 1000 });

// Automatic cleanup
subscription.Dispose();
```

#### 3. MiniGame Base (`Core/Architecture/MiniGameBase.cs`)
- **Purpose**: Base class for all mini-games
- **Features**:
  - Standardized lifecycle management
  - Service initialization and cleanup
  - State management (Uninitialized → Initializing → Ready → Playing → Paused → Ended)
  - Framework compliance enforcement

#### 4. Game Bootstrap (`Core/Bootstrap/GameBootstrap.cs`)
- **Purpose**: Application startup and service initialization
- **Features**:
  - Core service initialization (EventBus, SaveSystem)
  - Service registration with ServiceLocator
  - Auto-loading configuration
  - Bootstrap event publishing

#### 5. MiniGame Loader (`Core/Bootstrap/MiniGameLoader.cs`)
- **Purpose**: Scene loading and mini-game initialization
- **Features**:
  - Async scene loading with progress tracking
  - Mini-game initialization after scene load
  - Loading screen support
  - Error handling and event publishing

 # Mini-Games

### Match3 Game

**Location**: `Assets/Scripts/MiniGames/Match3/`

**Key Features**:
- Constraint-based board generation
- Possible swaps detection
- Cascade effects with gravity
- Smooth animations using LeanTween
- Move counter system
- Hint system for player assistance

**Architecture**:
- **Foundation Systems**: Board management, tile pooling
- **Game Logic**: Match detection, swap validation
- **Performance**: Object pooling, optimized algorithms
- **Input**: Command pattern for input handling
- **Visual**: Animation strategies, visual feedback

- ## Endless Runner Game

**Location**: `Assets/Scripts/MiniGames/EndlessRunner/`

**Key Features**:
- 3D endless runner gameplay
- Procedural world generation
- Obstacle and collectible systems
- Player movement and collision detection
- Score tracking and survival mechanics

**Architecture**:
- **Core Systems**: State management, scoring, input handling
- **Game Systems**: Player controller, world generation, obstacle management
- **Performance**: Object pooling, efficient collision detection
- **Events**: Comprehensive event system for game state changes

## Design Choices

### 1. Service Locator vs Pure DI

**Choice**: Service Locator Pattern
**Rationale**:
- Unity-friendly: Works well with MonoBehaviour lifecycle
- Simple implementation: Easy to understand and use
- Flexible: Supports both instance and factory registration
- Testing: Easy to mock services for unit tests

**Alternative Considered**: Constructor Injection
**Why Rejected**: Difficult to implement with MonoBehaviour lifecycle

### 2. Event-Driven Communication

**Choice**: EventBus for all cross-module communication
**Rationale**:
- Loose coupling: Systems don't directly reference each other
- Extensibility: Easy to add new event handlers
- Testability: Events can be easily mocked
- Performance: Zero-allocation implementation

### 3. Scene-Scoped Services

**Choice**: Clear scene services on scene unload
**Rationale**:
- Memory management: Prevents memory leaks
- Clean state: Each scene starts with fresh services
- Predictable behavior: No stale references between scenes

### 4. MiniGame Loader Pattern

**Choice**: Centralized game loading with initialization
**Rationale**:
- Consistent initialization: All games follow same pattern
- Error handling: Centralized error management
- Loading screens: Built-in loading screen support
- Event publishing: Loading progress and completion events
