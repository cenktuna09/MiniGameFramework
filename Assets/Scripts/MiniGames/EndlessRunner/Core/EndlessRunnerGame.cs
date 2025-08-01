using System;
using System.Threading.Tasks;
using UnityEngine;
using Core.Architecture;
using Core.DI;
using Core.Events;
using Core.Common.StateManagement;
using EndlessRunner.StateManagement;
using EndlessRunner.Input;
using EndlessRunner.Scoring;
using EndlessRunner.Performance;
using EndlessRunner.ErrorHandling;
using EndlessRunner.Events;
using EndlessRunner.Config;
using EndlessRunner.Player;
using EndlessRunner.World;
using EndlessRunner.Factories;
using EndlessRunner.Obstacles;
using EndlessRunner.Collectibles;

namespace EndlessRunner.Core
{
    /// <summary>
    /// Main game controller for 3D Endless Runner
    /// Thin facade coordinating specialized managers
    /// Extends MiniGameBase for framework compliance
    /// </summary>
    public class EndlessRunnerGame : MiniGameBase
    {
        #region Serialized Fields
        
        [Header("Game Configuration")]
        [SerializeField] private bool _autoStartGame = false;
        [SerializeField] private bool _enableDebugLogging = true;
        
        [Header("Factory Settings")]
        [SerializeField] private GameObject[] _obstaclePrefabs;
        [SerializeField] private GameObject[] _collectiblePrefabs;
        [SerializeField] private GameObject _platformPrefab;
        
        #endregion
        
        #region Private Fields
        
        // Framework services
        private IEventBus _eventBus;
        
        // Core systems - using framework EventBus
        private RunnerStateManager _stateManager;
        private RunnerScoreManager _scoreManager;
        private RunnerInputManager _inputManager;
        private RunnerPerformanceManager _performanceManager;
        private RunnerErrorHandler _errorHandler;
        
        // Game systems
        private PlayerController _playerController;
        private EndlessRunnerScrollController _scrollController;
        
        // Factory system
        private EndlessRunnerFactoryManager _factoryManager;
        
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
        public EndlessRunnerFactoryManager FactoryManager => _factoryManager;
        
        #endregion
        
        #region Unity Methods
        
        protected override void Awake()
        {
            Debug.Log("[EndlessRunnerGame] 🔧 Awake called");
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
            // Always process input, even when game is not running (for first tap to start)
            _inputManager?.ProcessInput();
            
            if (_isGameRunning)
            {
                UpdateGameTime();
                UpdateGameSystems();
            }
        }
        
        private new void OnDestroy()
        {
            OnUnityDestroy();
        }
        
        private void OnUnityDestroy()
        {
            CleanupGame();
        }
        
        #endregion
        
        #region Public Game Control Methods
        
        /// <summary>
        /// Start the game
        /// </summary>
        public void StartGame()
        {
            if (_isGameRunning)
            {
                Debug.LogWarning("[EndlessRunnerGame] ⚠️ Game already running!");
                return;
            }
            
            _isGameRunning = true;
            _gameStartTime = Time.time;
            _currentGameTime = 0f;
            
            // Initialize systems if not already done
            if (!_isInitialized)
            {
                Debug.LogWarning("[EndlessRunnerGame] ⚠️ Game not initialized, initializing now");
                InitializeCoreSystems(_eventBus);
            }
            
            // Start game state
            _stateManager?.TransitionTo(RunnerGameState.Running);
            
            // Unlock input for new game
            _inputManager?.UnlockInput();
            
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
            
            // Unlock input after reset
            _inputManager?.UnlockInput();
            
            Debug.Log("[EndlessRunnerGame] 🔄 Game state reset");
        }
        
        #endregion
        
        #region MiniGameBase Implementation
        
        protected override async Task OnInitializeAsync()
        {
            Debug.Log("[EndlessRunnerGame] 🚀 OnInitializeAsync called");
            try
            {
                Debug.Log("[EndlessRunnerGame] 🚀 Starting initialization...");
                
                // Get EventBus from ServiceLocator
                _eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
                if (_eventBus == null)
                {
                    Debug.LogError("[EndlessRunnerGame] ❌ No EventBus found in ServiceLocator!");
                    return;
                }
                
                // Initialize core systems
                InitializeCoreSystems(_eventBus);
                
                // Initialize Factory Manager
                InitializeFactoryManager();
                
                // Find and initialize game systems
                FindGameSystems(_eventBus);
                
                // Initialize all systems
                InitializeSystems();
                
                // Subscribe to events
                SubscribeToEvents(_eventBus);
                
                _isInitialized = true;
                
                Debug.Log("[EndlessRunnerGame] ✅ Initialization completed successfully");
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EndlessRunnerGame] ❌ Initialization failed: {ex.Message}");
                throw;
            }
        }
        
        protected override void OnStart()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[EndlessRunnerGame] ❌ Game not initialized!");
                return;
            }
            
            StartGame();
        }
        
        protected override void OnPause()
        {
            PauseGame();
        }
        
        protected override void OnResume()
        {
            ResumeGame();
        }
        
        protected override void OnEnd()
        {
            EndGame();
        }
        
        protected override void OnCleanup()
        {
            CleanupGame();
        }
        
        public override int GetCurrentScore()
        {
            return _scoreManager?.CurrentScore ?? 0;
        }
        
        #endregion
        
        #region Private Methods - Initialization
        
        /// <summary>
        /// Initialize core systems using framework services
        /// </summary>
        private void InitializeCoreSystems(IEventBus eventBus)
        {
            // Initialize state manager
            _stateManager = new RunnerStateManager(eventBus);
            RegisterSceneService(_stateManager);
            
            // Initialize score manager
            _scoreManager = new RunnerScoreManager(eventBus);
            RegisterSceneService(_scoreManager);
            
            // Initialize input manager
            _inputManager = new RunnerInputManager(eventBus);
            RegisterSceneService(_inputManager);
            
            // Initialize performance manager
            _performanceManager = new RunnerPerformanceManager(eventBus);
            RegisterSceneService(_performanceManager);
            
            // Initialize error handler
            _errorHandler = new RunnerErrorHandler(eventBus);
            RegisterSceneService(_errorHandler);
            
            Debug.Log("[EndlessRunnerGame] ✅ Core systems initialized");
        }
        
        /// <summary>
        /// Initialize Factory Manager
        /// </summary>
        private void InitializeFactoryManager()
        {
            // Create factory manager
            _factoryManager = new EndlessRunnerFactoryManager(
                obstacleParent: transform.Find("Obstacles"),
                collectibleParent: transform.Find("Collectibles"),
                platformParent: transform.Find("Platforms")
            );
            
            // Register obstacle factories
            if (_obstaclePrefabs != null && _obstaclePrefabs.Length > 0)
            {
                foreach (var prefab in _obstaclePrefabs)
                {
                    if (prefab != null)
                    {
                        _factoryManager.RegisterObstacleFactory(ObstacleType.Block, prefab, 1f, 0.7f);
                    }
                }
            }
            
            // Register collectible factories
            if (_collectiblePrefabs != null && _collectiblePrefabs.Length > 0)
            {
                foreach (var prefab in _collectiblePrefabs)
                {
                    if (prefab != null)
                    {
                        _factoryManager.RegisterCollectibleFactory(CollectibleType.Coin, prefab, 10, 0.8f, 90f);
                    }
                }
            }
            
            // Register platform factory
            if (_platformPrefab != null)
            {
                _factoryManager.RegisterPlatformFactory(_platformPrefab, 50f, 10f, 5, 10);
            }
            
            // Register with ServiceLocator
            RegisterSceneService(_factoryManager);
            
            Debug.Log("[EndlessRunnerGame] ✅ Factory Manager initialized");
        }
        
        /// <summary>
        /// Find and initialize game systems
        /// </summary>
        private void FindGameSystems(IEventBus eventBus)
        {
            // Find player controller
            _playerController = FindFirstObjectByType<PlayerController>();
            if (_playerController == null)
            {
                Debug.LogError("[EndlessRunnerGame] ❌ PlayerController not found!");
            }
            
            // Find scroll controller
            _scrollController = FindFirstObjectByType<EndlessRunnerScrollController>();
            if (_scrollController == null)
            {
                Debug.LogError("[EndlessRunnerGame] ❌ EndlessRunnerScrollController not found!");
            }
            
            Debug.Log("[EndlessRunnerGame] ✅ Game systems found");
        }
        
        /// <summary>
        /// Initialize all systems
        /// </summary>
        private void InitializeSystems()
        {
            // Initialize scroll controller
            if (_scrollController != null)
            {
                _scrollController.Initialize(_eventBus);
            }
            
            // Initialize player controller
            if (_playerController != null)
            {
                _playerController.Initialize(_eventBus);
            }
            
            Debug.Log("[EndlessRunnerGame] ✅ All systems initialized");
        }
        
        #endregion
        
        #region Private Methods - Event Handling
        
        /// <summary>
        /// Subscribe to relevant events
        /// </summary>
        private void SubscribeToEvents(IEventBus eventBus)
        {
            _gameStateSubscription = eventBus.Subscribe<StateChangedEvent<RunnerGameState>>(OnGameStateChanged);
            _playerDeathSubscription = eventBus.Subscribe<PlayerDeathEvent>(OnPlayerDeath);
            _scoreUpdateSubscription = eventBus.Subscribe<EndlessRunner.Events.ScoreChangedEvent>(OnScoreUpdated);
            _collectibleCollectedSubscription = eventBus.Subscribe<CollectibleCollectedEvent>(OnCollectibleCollected);
            _obstacleCollisionSubscription = eventBus.Subscribe<ObstacleCollisionEvent>(OnObstacleCollision);
            
            Debug.Log("[EndlessRunnerGame] ✅ Event subscriptions completed");
        }
        
        #endregion
        
        #region Private Methods - Update Logic
        
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
            // Update performance manager
            _performanceManager?.UpdateMonitoring();
            
            // Update scroll controller
            if (_scrollController != null)
            {
                // Scroll controller updates itself in Update()
            }
        }
        
        #endregion
        
        #region Private Methods - Cleanup
        
        /// <summary>
        /// Clean up game resources
        /// </summary>
        private void CleanupGame()
        {
            // Dispose event subscriptions
            _gameStateSubscription?.Dispose();
            _playerDeathSubscription?.Dispose();
            _scoreUpdateSubscription?.Dispose();
            _collectibleCollectedSubscription?.Dispose();
            _obstacleCollisionSubscription?.Dispose();
            
            // Clear scene services
            ServiceLocator.Instance.ClearSceneServices();
            
            Debug.Log("[EndlessRunnerGame] 🧹 Cleanup completed");
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle game state changes
        /// </summary>
        private void OnGameStateChanged(StateChangedEvent<RunnerGameState> stateEvent)
        {
            switch (stateEvent.NewState)
            {
                case RunnerGameState.Ready:
                    _isGameRunning = false;
                    break;
                    
                case RunnerGameState.Running:
                    _isGameRunning = true;
                    break;
                    
                case RunnerGameState.Paused:
                    _isGameRunning = false;
                    break;
                    
                case RunnerGameState.GameOver:
                    _isGameRunning = false;
                    break;
            }
            
            if (_enableDebugLogging)
            {
                Debug.Log($"[EndlessRunnerGame] 🔄 State changed to: {stateEvent.NewState}");
            }
        }
        
        /// <summary>
        /// Handle player death
        /// </summary>
        private void OnPlayerDeath(PlayerDeathEvent deathEvent)
        {
            EndGame();
            
            if (_enableDebugLogging)
            {
                Debug.Log($"[EndlessRunnerGame] 💀 Player died: {deathEvent.DeathCause}");
            }
        }
        
        /// <summary>
        /// Handle score updates
        /// </summary>
        private void OnScoreUpdated(EndlessRunner.Events.ScoreChangedEvent scoreEvent)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[EndlessRunnerGame] 📊 Score updated: {scoreEvent.NewScore}");
            }
        }
        
        /// <summary>
        /// Handle collectible collection
        /// </summary>
        private void OnCollectibleCollected(CollectibleCollectedEvent collectionEvent)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[EndlessRunnerGame] 🪙 Collectible collected: {collectionEvent.CollectibleType}");
            }
        }
        
        /// <summary>
        /// Handle obstacle collision
        /// </summary>
        private void OnObstacleCollision(ObstacleCollisionEvent collisionEvent)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[EndlessRunnerGame] 💥 Obstacle collision: {collisionEvent.ObstacleType}");
            }
        }
        
        #endregion
    }
} 