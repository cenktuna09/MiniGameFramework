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
    /// Main game coordinator for 3D Endless Runner
    /// Integrates all systems and manages the complete game loop
    /// </summary>
    public class EndlessRunnerGame : MonoBehaviour
    {
        #region Private Fields
        [Header("Game Systems")]
        [SerializeField] private RunnerConfig _runnerConfig;
        [SerializeField] private bool _autoStartGame = true;
        [SerializeField] private bool _enableDebugLogging = true;
        
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
            
            _gameStartTime = Time.time;
            _currentGameTime = 0f;
            _isGameRunning = true;
            
            // Initialize all systems
            InitializeSystems();
            
            // Set initial game state
            _stateManager?.TransitionTo(RunnerGameState.Running);
            
            // Start world generation
            _worldGenerator?.Initialize(_eventBus);
            
            // Start obstacle system
            _obstacleManager?.Initialize(_eventBus);
            
            // Start collectible system
            _collectibleManager?.Initialize(_eventBus);
            
            // Initialize player
            _playerController?.Initialize(_eventBus);
            
            // Initialize input system (already initialized in constructor)
            
            // Initialize score system (already initialized in constructor)
            
            // Initialize performance monitoring
            _performanceManager?.Initialize();
            
            // Initialize error handling (already initialized in constructor)
            
            // Validate configuration
            _errorHandler?.ValidateConfiguration((BaseGameConfig)_runnerConfig);
            
            Debug.Log("[EndlessRunnerGame] 🎮 Game started successfully!");
            Debug.Log("[EndlessRunnerGame] 📊 Systems initialized:");
            Debug.Log($"  - State Manager: {(_stateManager != null ? "✅" : "❌")}");
            Debug.Log($"  - Score Manager: {(_scoreManager != null ? "✅" : "❌")}");
            Debug.Log($"  - Input Manager: {(_inputManager != null ? "✅" : "❌")}");
            Debug.Log($"  - Player Controller: {(_playerController != null ? "✅" : "❌")}");
            Debug.Log($"  - World Generator: {(_worldGenerator != null ? "✅" : "❌")}");
            Debug.Log($"  - Obstacle Manager: {(_obstacleManager != null ? "✅" : "❌")}");
            Debug.Log($"  - Collectible Manager: {(_collectibleManager != null ? "✅" : "❌")}");
            Debug.Log($"  - Performance Manager: {(_performanceManager != null ? "✅" : "❌")}");
            Debug.Log($"  - Error Handler: {(_errorHandler != null ? "✅" : "❌")}");
        }
        
        /// <summary>
        /// Pause the game
        /// </summary>
        public void PauseGame()
        {
            if (!_isGameRunning) return;
            
            _isGameRunning = false;
            _stateManager?.TransitionTo(RunnerGameState.Paused);
            
            Debug.Log("[EndlessRunnerGame] ⏸️ Game paused");
        }
        
        /// <summary>
        /// Resume the game
        /// </summary>
        public void ResumeGame()
        {
            if (_isGameRunning) return;
            
            _isGameRunning = true;
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
            _stateManager?.SetState(RunnerGameState.GameOver);
            
            // Save final score
            _scoreManager?.SaveHighScore();
            
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
            _stateManager?.ResetManager();
            _scoreManager?.ResetManager();
            _inputManager?.ResetManager();
            _playerController?.ResetPlayer();
            _worldGenerator?.ResetGenerator();
            _obstacleManager?.ResetManager();
            _collectibleManager?.ResetManager();
            _performanceManager?.ResetManager();
            
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
            _scoreManager = new RunnerScoreManager();
            
            // Create input manager
            _inputManager = new RunnerInputManager();
            
            // Create performance manager
            _performanceManager = new RunnerPerformanceManager();
            
            // Create error handler
            _errorHandler = new RunnerErrorHandler();
            
            Debug.Log("[EndlessRunnerGame] ✅ Core systems initialized");
        }
        
        /// <summary>
        /// Find game systems in scene
        /// </summary>
        private void FindGameSystems()
        {
            // Find player controller
            _playerController = FindObjectOfType<PlayerController>();
            if (_playerController == null)
            {
                Debug.LogWarning("[EndlessRunnerGame] ⚠️ No PlayerController found in scene!");
            }
            
            // Find world generator
            _worldGenerator = FindObjectOfType<WorldGenerator>();
            if (_worldGenerator == null)
            {
                Debug.LogWarning("[EndlessRunnerGame] ⚠️ No WorldGenerator found in scene!");
            }
            
            // Find obstacle manager
            _obstacleManager = FindObjectOfType<ObstacleManager>();
            if (_obstacleManager == null)
            {
                Debug.LogWarning("[EndlessRunnerGame] ⚠️ No ObstacleManager found in scene!");
            }
            
            // Find collectible manager
            _collectibleManager = FindObjectOfType<CollectibleManager>();
            if (_collectibleManager == null)
            {
                Debug.LogWarning("[EndlessRunnerGame] ⚠️ No CollectibleManager found in scene!");
            }
            
            Debug.Log("[EndlessRunnerGame] ✅ Game systems found");
        }
        
        /// <summary>
        /// Initialize all systems
        /// </summary>
        private void InitializeSystems()
        {
            // Initialize state manager
            _stateManager?.Initialize(_eventBus);
            
            // Initialize score manager
            _scoreManager?.Initialize(_eventBus);
            
            // Initialize input manager
            _inputManager?.Initialize(_eventBus);
            
            // Initialize performance manager
            _performanceManager?.Initialize(_eventBus);
            
            // Initialize error handler
            _errorHandler?.Initialize(_eventBus);
            
            Debug.Log("[EndlessRunnerGame] ✅ All systems initialized");
        }
        
        /// <summary>
        /// Subscribe to events
        /// </summary>
        private void SubscribeToEvents()
        {
            if (_eventBus == null) return;
            
            // Subscribe to game state changes
            _gameStateSubscription = _eventBus.Subscribe<StateChangedEvent<RunnerGameState>>(OnGameStateChanged);
            
            // Subscribe to player death
            _playerDeathSubscription = _eventBus.Subscribe<PlayerDeathEvent>(OnPlayerDeath);
            
            // Subscribe to score updates
            _scoreUpdateSubscription = _eventBus.Subscribe<ScoreChangedEvent>(OnScoreUpdated);
            
            // Subscribe to collectible collection
            _collectibleCollectedSubscription = _eventBus.Subscribe<CollectibleCollectedEvent>(OnCollectibleCollected);
            
            // Subscribe to obstacle collision
            _obstacleCollisionSubscription = _eventBus.Subscribe<ObstacleCollisionEvent>(OnObstacleCollision);
            
            Debug.Log("[EndlessRunnerGame] 📡 Subscribed to game events");
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
            _performanceManager?.UpdatePerformance();
            
            // Update error handling
            _errorHandler?.UpdateErrorHandling();
        }
        
        /// <summary>
        /// Cleanup game
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
        
        #region Event Handlers
        /// <summary>
        /// Handle game state changes
        /// </summary>
        private void OnGameStateChanged(StateChangedEvent<RunnerGameState> stateEvent)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[EndlessRunnerGame] 🎯 Game state changed: {stateEvent.PreviousState} -> {stateEvent.NewState}");
            }
            
            // Handle specific state changes
            switch (stateEvent.NewState)
            {
                case RunnerGameState.GameOver:
                    EndGame();
                    break;
                    
                case RunnerGameState.Paused:
                    PauseGame();
                    break;
                    
                case RunnerGameState.Playing:
                    if (!_isGameRunning)
                    {
                        ResumeGame();
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Handle player death
        /// </summary>
        private void OnPlayerDeath(PlayerDeathEvent deathEvent)
        {
            Debug.Log($"[EndlessRunnerGame] 💀 Player died! Final score: {_scoreManager?.CurrentScore ?? 0}");
            
            // End the game
            EndGame();
        }
        
        /// <summary>
        /// Handle score updates
        /// </summary>
        private void OnScoreUpdated(ScoreChangedEvent scoreEvent)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[EndlessRunnerGame] 💎 Score updated: {scoreEvent.NewScore} (+{scoreEvent.ScoreChange})");
            }
        }
        
        /// <summary>
        /// Handle collectible collection
        /// </summary>
        private void OnCollectibleCollected(CollectibleCollectedEvent collectionEvent)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[EndlessRunnerGame] 💰 Collectible collected: {collectionEvent.CollectibleType} (+{collectionEvent.PointValue} points)");
            }
        }
        
        /// <summary>
        /// Handle obstacle collision
        /// </summary>
        private void OnObstacleCollision(ObstacleCollisionEvent collisionEvent)
        {
            Debug.Log($"[EndlessRunnerGame] 💥 Obstacle collision: {collisionEvent.ObstacleType} (-{collisionEvent.DamageAmount} health)");
        }
        #endregion
        #endregion
    }
} 