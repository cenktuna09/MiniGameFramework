using UnityEngine;
using Core.Architecture;
using EndlessRunner.World;
using EndlessRunner.Events;
using Core.Events;

namespace EndlessRunner.Testing
{
    /// <summary>
    /// Test script for World Generator integration
    /// Verifies chunk generation, object pooling, and event system
    /// </summary>
    public class WorldGeneratorTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool _enableTesting = true;
        [SerializeField] private bool _logEvents = true;
        [SerializeField] private bool _autoGenerateChunks = true;
        
        // Components
        private WorldGenerator _worldGenerator;
        private IEventBus _eventBus;
        
        // Test state
        private bool _isInitialized = false;
        private float _testTimer = 0f;
        private Vector3 _testPlayerPosition = Vector3.zero;
        
        #region Unity Methods
        private void Start()
        {
            if (!_enableTesting) return;
            
            InitializeTest();
        }
        
        private void Update()
        {
            if (!_isInitialized || !_enableTesting) return;
            
            // Simulate player movement for testing
            SimulatePlayerMovement();
            
            // Auto-generate chunks for testing
            if (_autoGenerateChunks)
            {
                _testTimer += Time.deltaTime;
                if (_testTimer > 5f) // Generate new chunk every 5 seconds
                {
                    _testTimer = 0f;
                    _worldGenerator?.GenerateChunk();
                }
            }
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Initialize the test environment
        /// </summary>
        public void InitializeTest()
        {
            Debug.Log("[WorldGeneratorTester] 🧪 Starting World Generator test...");
            
            // Create event bus
            _eventBus = new EventBus();
            
            // Find or create world generator
            _worldGenerator = FindFirstObjectByType<WorldGenerator>();
            if (_worldGenerator == null)
            {
                Debug.LogError("[WorldGeneratorTester] ❌ No WorldGenerator found in scene!");
                return;
            }
            
            // Initialize world generator
            _worldGenerator.Initialize(_eventBus);
            
            // Subscribe to events for testing
            if (_logEvents)
            {
                SubscribeToTestEvents();
            }
            
            _isInitialized = true;
            
            Debug.Log("[WorldGeneratorTester] ✅ Test environment initialized");
            Debug.Log("[WorldGeneratorTester] 📝 Test Instructions:");
            Debug.Log("  - World will auto-generate chunks every 5 seconds");
            Debug.Log("  - Check console for event logs");
            Debug.Log("  - Monitor object pooling performance");
        }
        
        /// <summary>
        /// Reset the test environment
        /// </summary>
        public void ResetTest()
        {
            if (_worldGenerator != null)
            {
                _worldGenerator.ResetGenerator();
            }
            
            _testTimer = 0f;
            _testPlayerPosition = Vector3.zero;
            
            Debug.Log("[WorldGeneratorTester] 🔄 Test environment reset");
        }
        
        /// <summary>
        /// Manually generate a chunk for testing
        /// </summary>
        public void GenerateTestChunk()
        {
            if (_worldGenerator != null)
            {
                _worldGenerator.GenerateChunk();
                Debug.Log("[WorldGeneratorTester] 🏗️ Manually generated test chunk");
            }
        }
        
        /// <summary>
        /// Set test difficulty
        /// </summary>
        public void SetTestDifficulty(float difficulty)
        {
            if (_worldGenerator != null)
            {
                _worldGenerator.SetDifficulty(difficulty);
                Debug.Log($"[WorldGeneratorTester] 📈 Set test difficulty to: {difficulty}");
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
            
            // Subscribe to world events
            _eventBus.Subscribe<WorldChunkGeneratedEvent>(OnChunkGenerated);
            _eventBus.Subscribe<WorldChunkDespawnedEvent>(OnChunkDespawned);
            _eventBus.Subscribe<WorldDifficultyChangedEvent>(OnDifficultyChanged);
            _eventBus.Subscribe<ObstacleSpawnedEvent>(OnObstacleSpawned);
            _eventBus.Subscribe<CollectibleSpawnedEvent>(OnCollectibleSpawned);
            
            Debug.Log("[WorldGeneratorTester] 📡 Subscribed to world events");
        }
        
        /// <summary>
        /// Simulate player movement for chunk generation testing
        /// </summary>
        private void SimulatePlayerMovement()
        {
            // Move player forward for testing
            _testPlayerPosition += Vector3.forward * 10f * Time.deltaTime;
            
            // Publish simulated player movement event
            if (_eventBus != null)
            {
                var movementEvent = new EndlessRunner.Events.PlayerMovementEvent(_testPlayerPosition, Vector3.forward * 10f * Time.deltaTime, 10f, 0f);
                _eventBus.Publish(movementEvent);
            }
        }
        
        #region Event Handlers
        private void OnChunkGenerated(WorldChunkGeneratedEvent chunkEvent)
        {
            Debug.Log($"[WorldGeneratorTester] 🏗️ Chunk {chunkEvent.ChunkIndex} generated at {chunkEvent.ChunkPosition}");
            Debug.Log($"[WorldGeneratorTester] 📊 Obstacles: {chunkEvent.ObstacleCount}, Collectibles: {chunkEvent.CollectibleCount}");
        }
        
        private void OnChunkDespawned(WorldChunkDespawnedEvent chunkEvent)
        {
            Debug.Log($"[WorldGeneratorTester] 🗑️ Chunk {chunkEvent.ChunkIndex} despawned at {chunkEvent.ChunkPosition}");
            Debug.Log($"[WorldGeneratorTester] 📏 Distance from player: {chunkEvent.DistanceFromPlayer}");
        }
        
        private void OnDifficultyChanged(WorldDifficultyChangedEvent difficultyEvent)
        {
//            Debug.Log($"[WorldGeneratorTester] 📈 Difficulty changed: {difficultyEvent.PreviousDifficulty} → {difficultyEvent.NewDifficulty}");
//            Debug.Log($"[WorldGeneratorTester] 📏 Distance: {difficultyEvent.DistanceTraveled}, Reason: {difficultyEvent.DifficultyReason}");
        }
        
        private void OnObstacleSpawned(ObstacleSpawnedEvent obstacleEvent)
        {
            Debug.Log($"[WorldGeneratorTester] 🚧 Obstacle spawned: {obstacleEvent.ObstacleType} at {obstacleEvent.SpawnPosition}");
            Debug.Log($"[WorldGeneratorTester] 🎯 Lane: {obstacleEvent.LaneIndex}, Speed: {obstacleEvent.ObstacleSpeed}");
        }
        
        private void OnCollectibleSpawned(CollectibleSpawnedEvent collectibleEvent)
        {
            Debug.Log($"[WorldGeneratorTester] 💰 Collectible spawned: {collectibleEvent.CollectibleType} at {collectibleEvent.SpawnPosition}");
            Debug.Log($"[WorldGeneratorTester] 🎯 Lane: {collectibleEvent.LaneIndex}, Value: {collectibleEvent.CollectibleValue}");
        }
        #endregion
        #endregion
    }
} 