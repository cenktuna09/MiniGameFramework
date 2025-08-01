using UnityEngine;
using Core.Architecture;
using EndlessRunner.Obstacles;

namespace EndlessRunner.Testing
{
    /// <summary>
    /// Test script for Obstacle System integration
    /// Verifies obstacle spawning, collision detection, and event system
    /// </summary>
    public class ObstacleSystemTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool _enableTesting = true;
        [SerializeField] private bool _logEvents = true;
        [SerializeField] private bool _autoSpawnObstacles = true;
        [SerializeField] private float _spawnInterval = 3f;
        
        [Header("Test Obstacles")]
        [SerializeField] private ObstacleType _testObstacleType = ObstacleType.Block;
        [SerializeField] private Vector3 _testSpawnPosition = new Vector3(0f, 1f, 10f);
        [SerializeField] private int _testLaneIndex = 1;
        
        // Components
        private ObstacleManager _obstacleManager;
        private IEventBus _eventBus;
        
        // Test state
        private bool _isInitialized = false;
        private float _testTimer = 0f;
        private int _obstaclesSpawned = 0;
        
        #region Unity Methods
        private void Start()
        {
            if (!_enableTesting) return;
            
            InitializeTest();
        }
        
        private void Update()
        {
            if (!_isInitialized || !_enableTesting) return;
            
            // Auto-spawn obstacles for testing
            if (_autoSpawnObstacles)
            {
                _testTimer += Time.deltaTime;
                if (_testTimer > _spawnInterval)
                {
                    _testTimer = 0f;
                    SpawnTestObstacle();
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
            Debug.Log("[ObstacleSystemTester] 🧪 Starting Obstacle System test...");
            
            // Create event bus
            _eventBus = new Core.Events.EventBus();
            
            // Find or create obstacle manager
            _obstacleManager = FindObjectOfType<ObstacleManager>();
            if (_obstacleManager == null)
            {
                Debug.LogError("[ObstacleSystemTester] ❌ No ObstacleManager found in scene!");
                return;
            }
            
            // Initialize obstacle manager
            _obstacleManager.Initialize(_eventBus);
            
            // Subscribe to events for testing
            if (_logEvents)
            {
                SubscribeToTestEvents();
            }
            
            _isInitialized = true;
            
            Debug.Log("[ObstacleSystemTester] ✅ Test environment initialized");
            Debug.Log("[ObstacleSystemTester] 📝 Test Instructions:");
            Debug.Log("  - Obstacles will auto-spawn every 3 seconds");
            Debug.Log("  - Check console for event logs");
            Debug.Log("  - Test collision detection with player");
        }
        
        /// <summary>
        /// Reset the test environment
        /// </summary>
        public void ResetTest()
        {
            if (_obstacleManager != null)
            {
                _obstacleManager.ResetManager();
            }
            
            _testTimer = 0f;
            _obstaclesSpawned = 0;
            
            Debug.Log("[ObstacleSystemTester] 🔄 Test environment reset");
        }
        
        /// <summary>
        /// Manually spawn a test obstacle
        /// </summary>
        public void SpawnTestObstacle()
        {
            if (_obstacleManager != null)
            {
                var obstacle = _obstacleManager.SpawnObstacle(_testSpawnPosition, _testLaneIndex);
                if (obstacle != null)
                {
                    _obstaclesSpawned++;
                    Debug.Log($"[ObstacleSystemTester] 🚧 Manually spawned test obstacle #{_obstaclesSpawned}");
                }
            }
        }
        
        /// <summary>
        /// Set test difficulty
        /// </summary>
        public void SetTestDifficulty(float difficulty)
        {
            if (_obstacleManager != null)
            {
                _obstacleManager.SetDifficulty(difficulty);
                Debug.Log($"[ObstacleSystemTester] 📈 Set test difficulty to: {difficulty}");
            }
        }
        
        /// <summary>
        /// Test obstacle collision with player
        /// </summary>
        public void TestObstacleCollision()
        {
            // Find player
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Find nearest obstacle
                var obstacles = FindObjectsOfType<ObstacleController>();
                if (obstacles.Length > 0)
                {
                    var nearestObstacle = obstacles[0];
                    float minDistance = Vector3.Distance(player.transform.position, nearestObstacle.transform.position);
                    
                    foreach (var obstacle in obstacles)
                    {
                        float distance = Vector3.Distance(player.transform.position, obstacle.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestObstacle = obstacle;
                        }
                    }
                    
                    // Move player to obstacle for collision test
                    Vector3 collisionPosition = nearestObstacle.transform.position;
                    player.transform.position = collisionPosition;
                    
                    Debug.Log($"[ObstacleSystemTester] 💥 Testing collision with {nearestObstacle.ObstacleType} at {collisionPosition}");
                }
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
            
            // Subscribe to obstacle events
            _eventBus.Subscribe<ObstacleSpawnedEvent>(OnObstacleSpawned);
            _eventBus.Subscribe<ObstacleCollisionEvent>(OnObstacleCollision);
            _eventBus.Subscribe<ObstacleAvoidedEvent>(OnObstacleAvoided);
            _eventBus.Subscribe<ObstacleDestroyedEvent>(OnObstacleDestroyed);
            
            Debug.Log("[ObstacleSystemTester] 📡 Subscribed to obstacle events");
        }
        
        #region Event Handlers
        private void OnObstacleSpawned(ObstacleSpawnedEvent spawnEvent)
        {
            Debug.Log($"[ObstacleSystemTester] 🚧 Obstacle spawned: {spawnEvent.ObstacleType} at {spawnEvent.Position}");
            Debug.Log($"[ObstacleSystemTester] 🎯 Lane: {spawnEvent.Lane}, Speed: {spawnEvent.Speed}");
        }
        
        private void OnObstacleCollision(ObstacleCollisionEvent collisionEvent)
        {
            Debug.Log($"[ObstacleSystemTester] 💥 Obstacle collision: {collisionEvent.ObstacleType} with player");
            Debug.Log($"[ObstacleSystemTester] 💔 Damage: {collisionEvent.DamageAmount}, Lane: {collisionEvent.Lane}");
        }
        
        private void OnObstacleAvoided(ObstacleAvoidedEvent avoidedEvent)
        {
            Debug.Log($"[ObstacleSystemTester] ✅ Obstacle avoided: {avoidedEvent.ObstacleType} at {avoidedEvent.Position}");
            Debug.Log($"[ObstacleSystemTester] 🎯 Lane: {avoidedEvent.Lane}");
        }
        
        private void OnObstacleDestroyed(ObstacleDestroyedEvent destroyedEvent)
        {
            Debug.Log($"[ObstacleSystemTester] 💥 Obstacle destroyed: {destroyedEvent.ObstacleType} at {destroyedEvent.Position}");
            Debug.Log($"[ObstacleSystemTester] 🎯 Lane: {destroyedEvent.Lane}");
        }
        #endregion
        #endregion
    }
} 