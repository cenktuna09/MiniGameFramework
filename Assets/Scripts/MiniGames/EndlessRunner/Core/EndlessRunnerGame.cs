using System;
using System.Threading.Tasks;
using UnityEngine;
using Core.Common.StateManagement;
using Core.Common.InputManagement;
using Core.Common.PerformanceManagement;
using Core.Common.ConfigManagement;
using Core.Common.ScoringManagement;
using Core.Events;
using Core.Architecture;
using Core.DI;
using EndlessRunner.StateManagement;
using EndlessRunner.Input;
using EndlessRunner.Scoring;
using EndlessRunner.Performance;
using EndlessRunner.ErrorHandling;
using EndlessRunner.Events;
using EndlessRunner.Config;
using EndlessRunner.Player;
using EndlessRunner.World;

namespace EndlessRunner.Core
{
    /// <summary>
    /// Main game controller for 3D Endless Runner
    /// Manages all game systems and coordinates gameplay
    /// Extends MiniGameBase for framework compliance
    /// </summary>
    public class EndlessRunnerGame : MiniGameBase
    {
        #region Serialized Fields
        
        [Header("Game Systems")]
        [SerializeField] private bool _autoStartGame = false; // Changed to false - game starts on first tap
        [SerializeField] private bool _enableDebugLogging = true;
        
        #endregion
        
        #region Private Fields
        
        // Core systems - using framework EventBus
        private RunnerStateManager _stateManager;
        private RunnerScoreManager _scoreManager;
        private RunnerInputManager _inputManager;
        private RunnerPerformanceManager _performanceManager;
        private RunnerErrorHandler _errorHandler;
        
        // Game systems
        private PlayerController _playerController;
        
        // Game state
        private bool _isInitialized = false;
        private bool _isGameRunning = false;
        private float _gameStartTime = 0f;
        private float _currentGameTime = 0f;
        
        // Event subscriptions
        private System.IDisposable _gameStateSubscription;
        private System.IDisposable _playerDeathSubscription;
        private System.IDisposable _scoreUpdateSubscription;
        private System.IDisposable _collectibleCollectedSubscription;
        private System.IDisposable _obstacleCollisionSubscription;
        
        #endregion
        
        #region MiniGameBase Overrides
        
        /// <summary>
        /// Check if the game is currently playable
        /// </summary>
        public override bool IsPlayable => CurrentState == GameState.Playing;
        
        #endregion
        
        #region Public Properties
        
        public bool IsGameRunning => _isGameRunning;
        public float GameTime => _currentGameTime;
        public RunnerStateManager StateManager => _stateManager;
        public RunnerScoreManager ScoreManager => _scoreManager;
        public PlayerController PlayerController => _playerController;
        
        #endregion
        
        #region Unity Methods
        
        protected override void Awake()
        {
            base.Awake();
            // Don't initialize here - wait for OnInitializeAsync()
        }
        
        private new void Start()
        {
            OnUnityStart();
        }
        
        private void OnUnityStart()
        {
            if (_autoStartGame)
            {
                StartGame();
            }
        }
        
        private void Update()
        {
            // Always process input, even when game is not running (for start input)
            _inputManager?.ProcessInput();
            
            if (_isGameRunning)
            {
                UpdateGameTime();
                UpdateGameSystems();
            }
        }
        
        private new void OnDestroy()
        {
            CleanupGame();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Start the game
        /// </summary>
        public void StartGame()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[EndlessRunnerGame] ❌ Game not initialized!");
                return;
            }
            
            if (_isGameRunning)
            {
                Debug.LogWarning("[EndlessRunnerGame] ⚠️ Game already running!");
                return;
            }
            
            _isGameRunning = true;
            _gameStartTime = Time.time;
            _currentGameTime = 0f;
            
            // Initialize systems
            InitializeSystems();
            
            // Start game state
            _stateManager?.TransitionTo(RunnerGameState.Running);
            
            // Unlock input for new game
            _inputManager?.UnlockInput();
            
            // Publish game started event
            var eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            eventBus?.Publish(new GameStartedEvent(Time.time));
            
            Debug.Log("[EndlessRunnerGame] 🎮 Game started");
        }
        
        /// <summary>
        /// Pause the game
        /// </summary>
        public void PauseGame()
        {
            if (!_isGameRunning) return;
            
            _stateManager?.TransitionTo(RunnerGameState.Paused);
            
            Debug.Log("[EndlessRunnerGame] ⏸️ Game paused");
        }
        
        /// <summary>
        /// Resume the game
        /// </summary>
        public void ResumeGame()
        {
            if (_stateManager?.CurrentState != RunnerGameState.Paused) return;
            
            _stateManager?.TransitionTo(RunnerGameState.Running);
            
            Debug.Log("[EndlessRunnerGame] ▶️ Game resumed");
        }
        
        /// <summary>
        /// End the game
        /// </summary>
        public void EndGame()
        {
            if (!_isGameRunning) return;
            
            _isGameRunning = false;
            _stateManager?.TransitionTo(RunnerGameState.GameOver);
            
            // Save final score
            _scoreManager?.EndGame();
            
            Debug.Log("[EndlessRunnerGame] 🏁 Game ended");
        }
        
        /// <summary>
        /// Restart the game
        /// </summary>
        public void RestartGame()
        {
            ResetGame();
            StartGame();
            
            Debug.Log("[EndlessRunnerGame] 🔄 Game restarted");
        }
        
        /// <summary>
        /// Reset the game state
        /// </summary>
        public void ResetGame()
        {
            _isGameRunning = false;
            _currentGameTime = 0f;
            
            // Reset all systems
            _stateManager?.TransitionTo(RunnerGameState.Ready);
            _scoreManager?.ResetScore();
            _inputManager?.ResetInputState();
            _playerController?.ResetPlayer();
            
            // Unlock input after reset
            _inputManager?.UnlockInput();
            
            Debug.Log("[EndlessRunnerGame] 🔄 Game state reset");
        }
        
        #endregion
        
        #region MiniGameBase Override Methods
        
        /// <summary>
        /// Initialize the mini-game asynchronously
        /// </summary>
        protected override async Task OnInitializeAsync()
        {
            Debug.Log("[EndlessRunnerGame] 🎮 Initializing Endless Runner Game (Async)...");
            
            // Use framework EventBus from ServiceLocator
            var eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            if (eventBus == null)
            {
                Debug.LogError("[EndlessRunnerGame] ❌ No EventBus found in ServiceLocator!");
                return;
            }
            
            Debug.Log("[EndlessRunnerGame] ✅ Using framework EventBus from ServiceLocator");
            
            // Initialize core systems with framework EventBus
            InitializeCoreSystems(eventBus);
            
            // Find game systems
            FindGameSystems(eventBus);
            
            // Subscribe to events
            SubscribeToEvents(eventBus);
            
            _isInitialized = true;
            
            Debug.Log("[EndlessRunnerGame] ✅ Game initialization complete (Async)");
            
            // Add await to make the method truly async
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Start the mini-game
        /// </summary>
        protected override void OnStart()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[EndlessRunnerGame] ❌ Game not initialized!");
                return;
            }
            
            if (_isGameRunning)
            {
                Debug.LogWarning("[EndlessRunnerGame] ⚠️ Game already running!");
                return;
            }
            
            // Don't auto-start the game - wait for user input
            // The game should start in Ready state and wait for first tap
            Debug.Log("[EndlessRunnerGame] 🎯 Game ready - waiting for user input to start");
        }
        
        /// <summary>
        /// Pause the mini-game
        /// </summary>
        protected override void OnPause()
        {
            if (!_isGameRunning) return;
            
            _stateManager?.TransitionTo(RunnerGameState.Paused);
            
            Debug.Log("[EndlessRunnerGame] ⏸️ Game paused");
        }
        
        /// <summary>
        /// Resume the mini-game
        /// </summary>
        protected override void OnResume()
        {
            if (_stateManager?.CurrentState != RunnerGameState.Paused) return;
            
            _stateManager?.TransitionTo(RunnerGameState.Running);
            
            Debug.Log("[EndlessRunnerGame] ▶️ Game resumed");
        }
        
        /// <summary>
        /// End the mini-game
        /// </summary>
        protected override void OnEnd()
        {
            if (!_isGameRunning) return;
            
            _isGameRunning = false;
            _stateManager?.TransitionTo(RunnerGameState.GameOver);
            
            // Save final score
            _scoreManager?.EndGame();
            
            Debug.Log("[EndlessRunnerGame] 🏁 Game ended");
        }
        
        /// <summary>
        /// Clean up resources
        /// </summary>
        protected override void OnCleanup()
        {
            // Clean up all systems
            CleanupGame();
            
            Debug.Log("[EndlessRunnerGame] 🧹 Cleanup completed");
        }
        
        /// <summary>
        /// Get the current score for this game session
        /// </summary>
        public override int GetCurrentScore()
        {
            return _scoreManager?.CurrentScore ?? 0;
        }
        
        #endregion
        
        #region Private Helper Methods
        
        /// <summary>
        /// Set the game state using the base class method
        /// </summary>
        /// <param name="newState">New game state</param>
        private void SetGameState(GameState newState)
        {
            SetState(newState);
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Initialize core systems using framework EventBus
        /// </summary>
        private void InitializeCoreSystems(IEventBus eventBus)
        {
            // Create state manager with framework EventBus
            _stateManager = new RunnerStateManager(eventBus);
            
            // Create score manager with framework EventBus
            _scoreManager = new RunnerScoreManager(eventBus);
            
            // Create input manager with framework EventBus
            _inputManager = new RunnerInputManager(eventBus);
            
            // Create performance manager with framework EventBus
            _performanceManager = new RunnerPerformanceManager(eventBus);
            
            // Create error handler with framework EventBus
            _errorHandler = new RunnerErrorHandler(eventBus);
            
            Debug.Log("[EndlessRunnerGame] ✅ Core systems initialized with framework EventBus");
        }
        
        /// <summary>
        /// Find game systems in scene
        /// </summary>
        private void FindGameSystems(IEventBus eventBus)
        {
            // Find player controller
            _playerController = FindFirstObjectByType<PlayerController>();
            if (_playerController != null)
            {
                _playerController.Initialize(eventBus);
            }
            
            // Find scroll controller
            var scrollController = FindFirstObjectByType<EndlessRunnerScrollController>();
            if (scrollController != null)
            {
                scrollController.Initialize(eventBus);
            }
            
            Debug.Log("[EndlessRunnerGame] ✅ Game systems found");
        }
        
        /// <summary>
        /// Initialize all systems
        /// </summary>
        private void InitializeSystems()
        {
            // Initialize state manager
            _stateManager?.TransitionTo(RunnerGameState.Ready);
            
            // Initialize score manager
            _scoreManager?.ResetScore();
            
            // Initialize input manager
            _inputManager?.ResetInputState();
            
            // Initialize performance manager
            _performanceManager?.StartMonitoring();
            
            // Initialize error handler
            _errorHandler?.SetLoggingEnabled(_enableDebugLogging);
            
            Debug.Log("[EndlessRunnerGame] ✅ All systems initialized");
        }
        
        /// <summary>
        /// Subscribe to game events using framework EventBus
        /// </summary>
        private void SubscribeToEvents(IEventBus eventBus)
        {
            // Subscribe to state changes
            _gameStateSubscription = eventBus.Subscribe<StateChangedEvent<RunnerGameState>>(OnGameStateChanged);
            
            // Subscribe to player events
            _playerDeathSubscription = eventBus.Subscribe<PlayerDeathEvent>(OnPlayerDeath);
            
            // Subscribe to score events
            _scoreUpdateSubscription = eventBus.Subscribe<Events.ScoreChangedEvent>(OnScoreUpdated);
            
            // Subscribe to collectible events
            _collectibleCollectedSubscription = eventBus.Subscribe<CollectibleCollectedEvent>(OnCollectibleCollected);
            
            // Subscribe to obstacle events
            _obstacleCollisionSubscription = eventBus.Subscribe<ObstacleCollisionEvent>(OnObstacleCollision);
            
            Debug.Log("[EndlessRunnerGame] ✅ Event subscriptions created with framework EventBus");
        }
        
        /// <summary>
        /// Update game time
        /// </summary>
        private void UpdateGameTime()
        {
            if (_isGameRunning)
            {
                _currentGameTime = Time.time - _gameStartTime;
            }
        }
        
        /// <summary>
        /// Update game systems
        /// </summary>
        private void UpdateGameSystems()
        {
            // Update performance monitoring
            _performanceManager?.UpdateMonitoring();
            
            // Update error handling
            _errorHandler?.SafeExecute(() => {
                // Safe game update logic
            }, "GameUpdate");
        }
        
        /// <summary>
        /// Cleanup game resources
        /// </summary>
        private void CleanupGame()
        {
            // Unsubscribe from events
            _gameStateSubscription?.Dispose();
            _playerDeathSubscription?.Dispose();
            _scoreUpdateSubscription?.Dispose();
            _collectibleCollectedSubscription?.Dispose();
            _obstacleCollisionSubscription?.Dispose();
            
            Debug.Log("[EndlessRunnerGame] 🧹 Game cleanup complete");
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle game state changes
        /// </summary>
        private void OnGameStateChanged(StateChangedEvent<RunnerGameState> stateEvent)
        {
            Debug.Log($"[EndlessRunnerGame] 🔄 State changed: {stateEvent.OldState} -> {stateEvent.NewState}");
            
            switch (stateEvent.NewState)
            {
                case RunnerGameState.Ready:
                    Debug.Log("[EndlessRunnerGame] 🎯 Game ready to start");
                    break;
                    
                case RunnerGameState.Running:
                    Debug.Log("[EndlessRunnerGame] 🏃 Game running");
                    break;
                    
                case RunnerGameState.Jumping:
                    Debug.Log("[EndlessRunnerGame] 🦘 Player jumping");
                    break;
                    
                case RunnerGameState.Sliding:
                    Debug.Log("[EndlessRunnerGame] 🛷 Player sliding");
                    break;
                    
                case RunnerGameState.Paused:
                    Debug.Log("[EndlessRunnerGame] ⏸️ Game paused");
                    break;
                    
                case RunnerGameState.GameOver:
                    Debug.Log("[EndlessRunnerGame] 💀 Game over");
                    EndGame();
                    break;
            }
        }
        
        /// <summary>
        /// Handle player death
        /// </summary>
        private void OnPlayerDeath(PlayerDeathEvent deathEvent)
        {
            Debug.Log($"[EndlessRunnerGame] 💀 Player died: {deathEvent.DeathCause}");
            
            // End the game
            EndGame();
        }
        
        /// <summary>
        /// Handle score updates
        /// </summary>
        private void OnScoreUpdated(EndlessRunner.Events.ScoreChangedEvent scoreEvent)
        {
            Debug.Log($"[EndlessRunnerGame] 📊 Score updated: {scoreEvent.NewScore} (+{scoreEvent.ScoreChange})");
        }
        
        /// <summary>
        /// Handle collectible collection
        /// </summary>
        private void OnCollectibleCollected(CollectibleCollectedEvent collectionEvent)
        {
            Debug.Log($"[EndlessRunnerGame] 💰 Collectible collected: {collectionEvent.CollectibleType} at {collectionEvent.Position}");
        }
        
        /// <summary>
        /// Handle obstacle collision
        /// </summary>
        private void OnObstacleCollision(ObstacleCollisionEvent collisionEvent)
        {
            Debug.Log($"[EndlessRunnerGame] 💥 Obstacle collision: {collisionEvent.ObstacleType} at {collisionEvent.CollisionPoint}");
            
            // Handle damage based on collision force
            int damageAmount = Mathf.RoundToInt(collisionEvent.CollisionForce);
            _playerController?.TakeDamage(damageAmount);
            
            // Check if player died and lock input
            if (_playerController != null && _playerController.IsDead)
            {
                _inputManager?.LockInput();
                Debug.Log("[EndlessRunnerGame] 🔒 Input locked due to player death");
            }
        }
        
        #endregion
    }
} 