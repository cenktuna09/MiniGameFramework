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
        

        
        // Game state
        private bool _isInitialized = false;
        private bool _isGameRunning = false;
        private float _gameStartTime = 0f;
        private float _currentGameTime = 0f;
        private float _lastScoreTime = 0f; // Track last time score was added
        private float _scoreInterval = 1f; // Add score every second
        private int _survivalScore = 10; // Points per second of survival
        private Vector3 _lastPlayerPosition = Vector3.zero; // Track last player position
        private float _movementUpdateInterval = 0.1f; // Update movement every 100ms
        private float _lastMovementUpdateTime = 0f; // Track last movement update
        
        // Event subscriptions
        private System.IDisposable _gameStateSubscription;
        private System.IDisposable _playerDeathSubscription;
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
            Debug.Log("[EndlessRunnerGame] 🔧 Awake called - calling base.Awake()");
            base.Awake();
            Debug.Log("[EndlessRunnerGame] ✅ Awake completed - starting initialization");
            
            // Start initialization immediately
            _ = InitializeAsync();
        }
        
        private new void Start()
        {
            Debug.Log("[EndlessRunnerGame] 🚀 Start called - checking if ready to start");
            
            // Only call base.Start() if we're in Ready state (after initialization)
            if (CurrentState == GameState.Ready)
            {
                Debug.Log("[EndlessRunnerGame] ✅ Ready state detected - calling base.Start()");
                base.Start();
            }
            else
            {
                Debug.LogWarning($"[EndlessRunnerGame] ⚠️ Not in Ready state: {CurrentState} - waiting for initialization");
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
            Debug.Log("[EndlessRunnerGame] 🚀 StartGame called - checking current state");
            
            if (_isGameRunning)
            {
                Debug.LogWarning("[EndlessRunnerGame] ⚠️ Game already running!");
                return;
            }
            
            Debug.Log("[EndlessRunnerGame] ✅ Game not running, proceeding with start");
            
            // Ensure systems are initialized
            if (!_isInitialized)
            {
                Debug.LogWarning("[EndlessRunnerGame] ⚠️ Game not initialized, initializing now");
                
                // Get EventBus from ServiceLocator if not available
                if (_eventBus == null)
                {
                    _eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
                    if (_eventBus == null)
                    {
                        Debug.LogError("[EndlessRunnerGame] ❌ No EventBus found in ServiceLocator!");
                        return;
                    }
                }
                
                InitializeCoreSystems(_eventBus);
                FindGameSystems(_eventBus);
                InitializeSystems();
                SubscribeToEvents(_eventBus);
                _isInitialized = true;
            }
            
            // Ensure input manager is available and unlocked
            if (_inputManager == null)
            {
                Debug.LogError("[EndlessRunnerGame] ❌ InputManager not found!");
                return;
            }
            
            Debug.Log("[EndlessRunnerGame] ✅ All systems ready, starting game");
            
            _isGameRunning = true;
            _gameStartTime = Time.time;
            _currentGameTime = 0f;
            _lastScoreTime = Time.time; // Reset score timer
            _lastPlayerPosition = _playerController != null ? _playerController.transform.position : Vector3.zero;
            _lastMovementUpdateTime = Time.time;
            
            // Start game state
            _stateManager?.TransitionTo(RunnerGameState.Running);
            
            // Unlock input for new game
            _inputManager.UnlockInput();
            
            // Publish game started event
            _eventBus?.Publish(new GameStartedEvent(Time.time));
            
            Debug.Log("[EndlessRunnerGame] 🎮 Game started successfully");
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
            _lastScoreTime = 0f; // Reset score timer
            _lastPlayerPosition = Vector3.zero;
            _lastMovementUpdateTime = 0f;
            
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
                Debug.Log("[EndlessRunnerGame] ✅ EventBus resolved from ServiceLocator");
                
                // Initialize core systems
                Debug.Log("[EndlessRunnerGame] 🔧 Initializing core systems...");
                InitializeCoreSystems(_eventBus);
            
                // Find and initialize game systems
                Debug.Log("[EndlessRunnerGame] 🔍 Finding game systems...");
                FindGameSystems(_eventBus);
                
                // Unlock input after initialization
                Debug.Log("[EndlessRunnerGame] 🔓 Unlocking input after initialization...");
                _inputManager?.UnlockInput();
                
                // Initialize all systems
                Debug.Log("[EndlessRunnerGame] ⚙️ Initializing all systems...");
                InitializeSystems();
                
                // Subscribe to events
                Debug.Log("[EndlessRunnerGame] 📡 Subscribing to events...");
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
            Debug.Log("[EndlessRunnerGame] 🎮 OnStart called - checking initialization and game state");
            
            if (!_isInitialized)
            {
                Debug.LogError("[EndlessRunnerGame] ❌ Game not initialized!");
                return;
            }
            
            Debug.Log($"[EndlessRunnerGame] ✅ Game is initialized, current game running state: {_isGameRunning}");
            
            // Auto-start the game when framework calls OnStart
            if (!_isGameRunning)
            {
                Debug.Log("[EndlessRunnerGame] 🎮 Framework OnStart called - starting game automatically");
                StartGame();
            }
            else
            {
                Debug.Log("[EndlessRunnerGame] ⚠️ Game is already running, skipping auto-start");
            }
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
            // Note: RunnerScoreManager already publishes Core.Common.ScoringManagement.ScoreChangedEvent
            // So no need to subscribe to EndlessRunner.Events.ScoreChangedEvent
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
                
                // Add survival score every second
                if (Time.time - _lastScoreTime >= _scoreInterval)
                {
                    _scoreManager?.AddScore(_survivalScore);
                    _lastScoreTime = Time.time;
                    
                    if (_enableDebugLogging)
                    {
                        Debug.Log($"[EndlessRunnerGame] ⏰ Survival score: +{_survivalScore} (Total time: {_currentGameTime:F1}s)");
                    }
                }
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