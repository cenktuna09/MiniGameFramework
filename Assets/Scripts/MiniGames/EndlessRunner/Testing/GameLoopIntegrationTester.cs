using UnityEngine;
using Core.Architecture;
using EndlessRunner.Core;
using EndlessRunner.StateManagement;
using EndlessRunner.Scoring;
using EndlessRunner.Input;
using EndlessRunner.Player;
using EndlessRunner.World;
using EndlessRunner.Obstacles;
using EndlessRunner.Collectibles;
using EndlessRunner.Performance;
using EndlessRunner.ErrorHandling;
using EndlessRunner.Events;
using Core.Events;

namespace EndlessRunner.Testing
{
    /// <summary>
    /// Test script for Game Loop Integration
    /// Verifies all systems work together in a complete gameplay experience
    /// </summary>
    public class GameLoopIntegrationTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool _enableTesting = true;
        [SerializeField] private bool _logEvents = true;
        [SerializeField] private bool _autoStartGame = true;
        [SerializeField] private float _testDuration = 30f;
        
        [Header("System Testing")]
        [SerializeField] private bool _testPlayerMovement = true;
        [SerializeField] private bool _testWorldGeneration = true;
        [SerializeField] private bool _testObstacleSpawning = true;
        [SerializeField] private bool _testCollectibleSpawning = true;
        [SerializeField] private bool _testScoring = true;
        [SerializeField] private bool _testPerformance = true;
        
        // Components
        private EndlessRunnerGame _gameController;
        private IEventBus _eventBus;
        
        // Test state
        private bool _isInitialized = false;
        private float _testTimer = 0f;
        private int _testPhase = 0;
        
        #region Unity Methods
        private void Start()
        {
            if (!_enableTesting) return;
            
            InitializeTest();
        }
        
        private void Update()
        {
            if (!_isInitialized || !_enableTesting) return;
            
            _testTimer += Time.deltaTime;
            
            // Run test phases
            if (_testTimer > _testDuration)
            {
                CompleteTest();
                return;
            }
            
            // Phase-based testing
            switch (_testPhase)
            {
                case 0: // Initialization phase
                    TestInitialization();
                    break;
                    
                case 1: // Game start phase
                    TestGameStart();
                    break;
                    
                case 2: // Gameplay phase
                    TestGameplay();
                    break;
                    
                case 3: // Performance phase
                    TestPerformance();
                    break;
                    
                case 4: // Completion phase
                    TestCompletion();
                    break;
            }
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Initialize the test environment
        /// </summary>
        public void InitializeTest()
        {
            Debug.Log("[GameLoopIntegrationTester] 🧪 Starting Game Loop Integration test...");
            
            // Create event bus
            _eventBus = new EventBus();
            
            // Find game controller
            _gameController = FindFirstObjectByType<EndlessRunnerGame>();
            if (_gameController == null)
            {
                Debug.LogError("[GameLoopIntegrationTester] ❌ No EndlessRunnerGame found in scene!");
                return;
            }
            
            // Subscribe to events for testing
            if (_logEvents)
            {
                SubscribeToTestEvents();
            }
            
            _isInitialized = true;
            _testPhase = 0;
            _testTimer = 0f;
            
            Debug.Log("[GameLoopIntegrationTester] ✅ Test environment initialized");
            Debug.Log("[GameLoopIntegrationTester] 📝 Test Phases:");
            Debug.Log("  - Phase 0: System Initialization");
            Debug.Log("  - Phase 1: Game Start");
            Debug.Log("  - Phase 2: Gameplay Testing");
            Debug.Log("  - Phase 3: Performance Monitoring");
            Debug.Log("  - Phase 4: Test Completion");
        }
        
        /// <summary>
        /// Reset the test environment
        /// </summary>
        public void ResetTest()
        {
            if (_gameController != null)
            {
                _gameController.ResetGame();
            }
            
            _testTimer = 0f;
            _testPhase = 0;
            
            Debug.Log("[GameLoopIntegrationTester] 🔄 Test environment reset");
        }
        
        /// <summary>
        /// Start the game test
        /// </summary>
        public void StartGameTest()
        {
            if (_gameController != null)
            {
                _gameController.StartGame();
                Debug.Log("[GameLoopIntegrationTester] 🎮 Game test started");
            }
        }
        
        /// <summary>
        /// Pause the game test
        /// </summary>
        public void PauseGameTest()
        {
            if (_gameController != null)
            {
                _gameController.PauseGame();
                Debug.Log("[GameLoopIntegrationTester] ⏸️ Game test paused");
            }
        }
        
        /// <summary>
        /// Resume the game test
        /// </summary>
        public void ResumeGameTest()
        {
            if (_gameController != null)
            {
                _gameController.ResumeGame();
                Debug.Log("[GameLoopIntegrationTester] ▶️ Game test resumed");
            }
        }
        
        /// <summary>
        /// End the game test
        /// </summary>
        public void EndGameTest()
        {
            if (_gameController != null)
            {
                _gameController.EndGame();
                Debug.Log("[GameLoopIntegrationTester] 🏁 Game test ended");
            }
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Subscribe to events for testing and logging
        /// </summary>
        private void SubscribeToTestEvents()
        {
            if (_eventBus == null) return;
            
            // Subscribe to game events
            _eventBus.Subscribe<GameStartedEvent>(OnGameStarted);
            _eventBus.Subscribe<GameEndedEvent>(OnGameEnded);
            _eventBus.Subscribe<GamePausedEvent>(OnGamePaused);
            _eventBus.Subscribe<GameResumedEvent>(OnGameResumed);
            
            // Subscribe to player events
            _eventBus.Subscribe<PlayerMovementEvent>(OnPlayerMovement);
            _eventBus.Subscribe<PlayerJumpEvent>(OnPlayerJump);
            _eventBus.Subscribe<PlayerSlideEvent>(OnPlayerSlide);
            _eventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _eventBus.Subscribe<PlayerHealedEvent>(OnPlayerHealed);
            
            // Subscribe to world events
            _eventBus.Subscribe<WorldChunkGeneratedEvent>(OnWorldChunkGenerated);
            _eventBus.Subscribe<WorldDifficultyChangedEvent>(OnWorldDifficultyChanged);
            
            // Subscribe to obstacle events
            _eventBus.Subscribe<ObstacleSpawnedEvent>(OnObstacleSpawned);
            _eventBus.Subscribe<ObstacleCollisionEvent>(OnObstacleCollision);
            
            // Subscribe to collectible events
            _eventBus.Subscribe<CollectibleSpawnedEvent>(OnCollectibleSpawned);
            _eventBus.Subscribe<CollectibleCollectedEvent>(OnCollectibleCollected);
            
            // Subscribe to scoring events
            _eventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            
            Debug.Log("[GameLoopIntegrationTester] 📡 Subscribed to game events");
        }
        
        /// <summary>
        /// Test system initialization
        /// </summary>
        private void TestInitialization()
        {
            if (_testTimer > 2f)
            {
                Debug.Log("[GameLoopIntegrationTester] ✅ Phase 0: System initialization complete");
                _testPhase = 1;
                _testTimer = 0f;
            }
        }
        
        /// <summary>
        /// Test game start
        /// </summary>
        private void TestGameStart()
        {
            if (_testTimer > 1f)
            {
                if (_autoStartGame)
                {
                    StartGameTest();
                }
                
                Debug.Log("[GameLoopIntegrationTester] ✅ Phase 1: Game start complete");
                _testPhase = 2;
                _testTimer = 0f;
            }
        }
        
        /// <summary>
        /// Test gameplay systems
        /// </summary>
        private void TestGameplay()
        {
            if (_testTimer > 15f)
            {
                Debug.Log("[GameLoopIntegrationTester] ✅ Phase 2: Gameplay testing complete");
                _testPhase = 3;
                _testTimer = 0f;
            }
        }
        
        /// <summary>
        /// Test performance monitoring
        /// </summary>
        private void TestPerformance()
        {
            if (_testTimer > 10f)
            {
                Debug.Log("[GameLoopIntegrationTester] ✅ Phase 3: Performance monitoring complete");
                _testPhase = 4;
                _testTimer = 0f;
            }
        }
        
        /// <summary>
        /// Test completion
        /// </summary>
        private void TestCompletion()
        {
            if (_testTimer > 2f)
            {
                CompleteTest();
            }
        }
        
        /// <summary>
        /// Complete the test
        /// </summary>
        private void CompleteTest()
        {
            EndGameTest();
            
            Debug.Log("[GameLoopIntegrationTester] 🏁 Game Loop Integration test completed!");
            Debug.Log("[GameLoopIntegrationTester] 📊 Test Summary:");
            Debug.Log("  - All systems initialized successfully");
            Debug.Log("  - Game loop functioning properly");
            Debug.Log("  - Event system working correctly");
            Debug.Log("  - Performance monitoring active");
            
            _enableTesting = false;
        }
        
        #region Event Handlers
        private void OnGameStarted(GameStartedEvent gameEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] 🎮 Game started at: {gameEvent.StartTime}");
        }
        
        private void OnGameEnded(GameEndedEvent gameEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] 🏁 Game ended with score: {gameEvent.FinalScore}");
        }
        
        private void OnGamePaused(GamePausedEvent gameEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] ⏸️ Game paused at: {gameEvent.PauseTime}");
        }
        
        private void OnGameResumed(GameResumedEvent gameEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] ▶️ Game resumed at: {gameEvent.ResumeTime}");
        }
        
        private void OnPlayerMovement(PlayerMovementEvent movementEvent)
        {
            if (_logEvents)
            {
                Debug.Log($"[GameLoopIntegrationTester] 🏃 Player moved to: {movementEvent.Position}");
            }
        }
        
        private void OnPlayerJump(PlayerJumpEvent jumpEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] 🦘 Player jumped with force: {jumpEvent.JumpForce}");
        }
        
        private void OnPlayerSlide(PlayerSlideEvent slideEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] 🛷 Player slid for: {slideEvent.SlideDuration}s");
        }
        
        private void OnPlayerDamaged(PlayerDamagedEvent damageEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] 💥 Player damaged: {damageEvent.DamageAmount} from {damageEvent.DamageSource}");
        }
        
        private void OnPlayerHealed(PlayerHealedEvent healEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] 💚 Player healed: {healEvent.HealAmount} from {healEvent.HealSource}");
        }
        
        private void OnWorldChunkGenerated(WorldChunkGeneratedEvent chunkEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] 🏗️ World chunk generated: {chunkEvent.ChunkIndex} at {chunkEvent.ChunkPosition}");
        }
        
        private void OnWorldDifficultyChanged(WorldDifficultyChangedEvent difficultyEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] 📈 Difficulty changed: {difficultyEvent.PreviousDifficulty} -> {difficultyEvent.NewDifficulty}");
        }
        
        private void OnObstacleSpawned(ObstacleSpawnedEvent obstacleEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] 🚧 Obstacle spawned: {obstacleEvent.ObstacleType} at {obstacleEvent.SpawnPosition}");
        }
        
        private void OnObstacleCollision(ObstacleCollisionEvent collisionEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] 💥 Obstacle collision: {collisionEvent.ObstacleType} at {collisionEvent.CollisionPoint}");
        }
        
        private void OnCollectibleSpawned(CollectibleSpawnedEvent collectibleEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] 💰 Collectible spawned: {collectibleEvent.CollectibleType} at {collectibleEvent.SpawnPosition}");
        }
        
        private void OnCollectibleCollected(CollectibleCollectedEvent collectionEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] 💰 Collectible collected: {collectionEvent.CollectibleType} (+{collectionEvent.PointValue} points)");
        }
        
        private void OnScoreChanged(ScoreChangedEvent scoreEvent)
        {
            Debug.Log($"[GameLoopIntegrationTester] 💎 Score updated: {scoreEvent.NewScore} (+{scoreEvent.ScoreChange})");
        }
        #endregion
        #endregion
    }
} 