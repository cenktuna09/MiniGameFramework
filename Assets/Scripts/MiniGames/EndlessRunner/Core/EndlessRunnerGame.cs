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
using EndlessRunner.Obstacles;
using EndlessRunner.Collectibles;

namespace EndlessRunner.Core
{
    /// <summary>
    /// Main game controller for 3D Endless Runner
    /// Manages all game systems and coordinates gameplay
    /// </summary>
    public class EndlessRunnerGame : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("Game Systems")]
        [SerializeField] private bool _autoStartGame = true;
        [SerializeField] private bool _enableDebugLogging = true;
        
        #endregion
        
        #region Private Fields
        
        // Core systems
        private IEventBus _eventBus;
        private RunnerStateManager _stateManager;
        private RunnerScoreManager _scoreManager;
        private RunnerInputManager _inputManager;
        private RunnerPerformanceManager _performanceManager;
        private RunnerErrorHandler _errorHandler;
        
        // Game systems
        private PlayerController _playerController;
        private WorldGenerator _worldGenerator;
        private ObstacleManager _obstacleManager;
        private CollectibleManager _collectibleManager;
        
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
        
        #region Public Properties
        
        public bool IsGameRunning => _isGameRunning;
        public float GameTime => _currentGameTime;
        public RunnerStateManager StateManager => _stateManager;
        public RunnerScoreManager ScoreManager => _scoreManager;
        public PlayerController PlayerController => _playerController;
        public WorldGenerator WorldGenerator => _worldGenerator;
        
        #endregion
        
        #region Unity Methods
        
        private void Awake()
        {
            InitializeGame();
        }
        
        private void Start()
        {
            if (_autoStartGame)
            {
                StartGame();
            }
        }
        
        private void Update()
        {
            if (_isGameRunning)
            {
                UpdateGameTime();
                UpdateGameSystems();
            }
        }
        
        private void OnDestroy()
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
            
            // Publish game started event
            _eventBus?.Publish(new GameStartedEvent(Time.time));
            
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
            _worldGenerator?.ResetGenerator();
            _obstacleManager?.ResetManager();
            _collectibleManager?.ResetManager();
            
            Debug.Log("[EndlessRunnerGame] 🔄 Game state reset");
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Initialize the game
        /// </summary>
        private void InitializeGame()
        {
            Debug.Log("[EndlessRunnerGame] 🎮 Initializing Endless Runner Game...");
            
            // Create event bus
            _eventBus = new EventBus();
            
            // Initialize core systems
            InitializeCoreSystems();
            
            // Find game systems
            FindGameSystems();
            
            // Subscribe to events
            SubscribeToEvents();
            
            _isInitialized = true;
            
            Debug.Log("[EndlessRunnerGame] ✅ Game initialization complete");
        }
        
        /// <summary>
        /// Initialize core systems
        /// </summary>
        private void InitializeCoreSystems()
        {
            // Create state manager
            _stateManager = new RunnerStateManager(_eventBus);
            
            // Create score manager
            _scoreManager = new RunnerScoreManager(_eventBus);
            
            // Create input manager
            _inputManager = new RunnerInputManager(_eventBus);
            
            // Create performance manager
            _performanceManager = new RunnerPerformanceManager(_eventBus);
            
            // Create error handler
            _errorHandler = new RunnerErrorHandler(_eventBus);
            
            Debug.Log("[EndlessRunnerGame] ✅ Core systems initialized");
        }
        
        /// <summary>
        /// Find game systems in scene
        /// </summary>
        private void FindGameSystems()
        {
            // Find player controller
            _playerController = FindFirstObjectByType<PlayerController>();
            if (_playerController != null)
            {
                _playerController.Initialize(_eventBus);
            }
            
            // Find world generator
            _worldGenerator = FindFirstObjectByType<WorldGenerator>();
            if (_worldGenerator != null)
            {
                _worldGenerator.Initialize(_eventBus);
            }
            
            // Find obstacle manager
            _obstacleManager = FindFirstObjectByType<ObstacleManager>();
            if (_obstacleManager != null)
            {
                _obstacleManager.Initialize(_eventBus);
            }
            
            // Find collectible manager
            _collectibleManager = FindFirstObjectByType<CollectibleManager>();
            if (_collectibleManager != null)
            {
                _collectibleManager.Initialize(_eventBus);
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
        /// Subscribe to game events
        /// </summary>
        private void SubscribeToEvents()
        {
            // Subscribe to state changes
            _gameStateSubscription = _eventBus.Subscribe<StateChangedEvent<RunnerGameState>>(OnGameStateChanged);
            
            // Subscribe to player events
            _playerDeathSubscription = _eventBus.Subscribe<PlayerDeathEvent>(OnPlayerDeath);
            
            // Subscribe to score events
            _scoreUpdateSubscription = _eventBus.Subscribe<EndlessRunner.Events.ScoreChangedEvent>(OnScoreUpdated);
            
            // Subscribe to collectible events
            _collectibleCollectedSubscription = _eventBus.Subscribe<CollectibleCollectedEvent>(OnCollectibleCollected);
            
            // Subscribe to obstacle events
            _obstacleCollisionSubscription = _eventBus.Subscribe<ObstacleCollisionEvent>(OnObstacleCollision);
            
            Debug.Log("[EndlessRunnerGame] ✅ Event subscriptions created");
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
            // Update input
            _inputManager?.ProcessInput();
            
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
        }
        
        #endregion
    }
} 