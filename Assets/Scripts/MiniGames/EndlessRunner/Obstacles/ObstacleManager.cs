using UnityEngine;
using System.Collections.Generic;
using Core.Events;
using Core.Architecture;
using EndlessRunner.Events;

namespace EndlessRunner.Obstacles
{
    /// <summary>
    /// Obstacle manager for 3D Endless Runner
    /// Handles obstacle spawning, type selection, and management
    /// </summary>
    public class ObstacleManager : MonoBehaviour
    {
        #region Private Fields
        [Header("Spawn Settings")]
        [SerializeField] private float _spawnRate = 0.3f;
        [SerializeField] private float _difficultyMultiplier = 0.1f;
        [SerializeField] private int _maxObstaclesPerChunk = 5;
        [SerializeField] private float _minObstacleDistance = 10f;
        
        [Header("Obstacle Types")]
        [SerializeField] private ObstacleType[] _availableObstacleTypes = { ObstacleType.Block, ObstacleType.Spike, ObstacleType.Trap };
        [SerializeField] private float[] _obstacleTypeWeights = { 0.5f, 0.3f, 0.2f };
        
        [Header("Difficulty Scaling")]
        [SerializeField] private float _difficultyIncreaseRate = 0.1f;
        [SerializeField] private float _maxDifficulty = 10f;
        [SerializeField] private float _wallSpawnThreshold = 0.8f;
        [SerializeField] private float _barrierSpawnThreshold = 0.6f;
        
        // Obstacle management
        private List<ObstacleController> _activeObstacles = new List<ObstacleController>();
        private Queue<ObstacleController> _obstaclePool = new Queue<ObstacleController>();
        private int _obstaclePoolSize = 20;
        
        // Spawn state
        private float _currentDifficulty = 1f;
        private Vector3 _lastObstaclePosition = Vector3.zero;
        private int _obstaclesSpawnedThisChunk = 0;
        
        // Events
        private IEventBus _eventBus;
        private System.IDisposable _chunkGeneratedSubscription;
        private System.IDisposable _difficultyChangedSubscription;
        #endregion
        
        #region Public Properties
        public float SpawnRate => _spawnRate;
        public float CurrentDifficulty => _currentDifficulty;
        public int ActiveObstacleCount => _activeObstacles.Count;
        public int PooledObstacleCount => _obstaclePool.Count;
        #endregion
        
        #region Unity Methods
        private void Start()
        {
            InitializeObstaclePool();
            Debug.Log("[ObstacleManager] ✅ Obstacle manager initialized");
        }
        
        private void Update()
        {
            UpdateDifficulty();
            CleanupDistantObstacles();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            _chunkGeneratedSubscription?.Dispose();
            _difficultyChangedSubscription?.Dispose();
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Initialize obstacle manager with event bus
        /// </summary>
        public void Initialize(IEventBus eventBus)
        {
            _eventBus = eventBus;
            SubscribeToEvents();
            Debug.Log("[ObstacleManager] 🚧 Obstacle manager connected to event system");
        }
        
        /// <summary>
        /// Spawn obstacle at specified position
        /// </summary>
        public ObstacleController SpawnObstacle(Vector3 position, int laneIndex)
        {
            // Check spawn conditions
            if (!ShouldSpawnObstacle(position))
            {
                return null;
            }
            
            // Get obstacle from pool or create new
            ObstacleController obstacle = GetObstacleFromPool();
            if (obstacle == null)
            {
                obstacle = CreateNewObstacle();
            }
            
            // Configure obstacle
            ConfigureObstacle(obstacle, position, laneIndex);
            
            // Add to active obstacles
            _activeObstacles.Add(obstacle);
            _obstaclesSpawnedThisChunk++;
            
            // Update state
            _lastObstaclePosition = position;
            
            // Publish obstacle spawned event
            if (_eventBus != null)
            {
                var spawnEvent = new ObstacleSpawnedEvent(obstacle.gameObject, position, obstacle.ObstacleType.ToString(), 0f, laneIndex);
                _eventBus.Publish(spawnEvent);
            }
            
            Debug.Log($"[ObstacleManager] 🚧 Spawned {obstacle.ObstacleType} at {position}");
            return obstacle;
        }
        
        /// <summary>
        /// Set difficulty level
        /// </summary>
        public void SetDifficulty(float difficulty)
        {
            _currentDifficulty = Mathf.Clamp(difficulty, 1f, _maxDifficulty);
            Debug.Log($"[ObstacleManager] 📈 Difficulty set to: {_currentDifficulty}");
        }
        
        /// <summary>
        /// Reset obstacle manager
        /// </summary>
        public void ResetManager()
        {
            // Return all active obstacles to pool
            foreach (var obstacle in _activeObstacles)
            {
                if (obstacle != null)
                {
                    ReturnObstacleToPool(obstacle);
                }
            }
            _activeObstacles.Clear();
            
            // Reset state
            _currentDifficulty = 1f;
            _lastObstaclePosition = Vector3.zero;
            _obstaclesSpawnedThisChunk = 0;
            
            Debug.Log("[ObstacleManager] 🔄 Obstacle manager reset");
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Initialize obstacle pool
        /// </summary>
        private void InitializeObstaclePool()
        {
            for (int i = 0; i < _obstaclePoolSize; i++)
            {
                var obstacle = CreateNewObstacle();
                ReturnObstacleToPool(obstacle);
            }
            
            Debug.Log($"[ObstacleManager] 🏊 Obstacle pool initialized with {_obstaclePoolSize} obstacles");
        }
        
        /// <summary>
        /// Subscribe to events
        /// </summary>
        private void SubscribeToEvents()
        {
            if (_eventBus == null) return;
            
            // Subscribe to chunk generation
            _chunkGeneratedSubscription = _eventBus.Subscribe<WorldChunkGeneratedEvent>(OnChunkGenerated);
            
            // Subscribe to difficulty changes
            _difficultyChangedSubscription = _eventBus.Subscribe<WorldDifficultyChangedEvent>(OnDifficultyChanged);
            
            Debug.Log("[ObstacleManager] 📡 Subscribed to world events");
        }
        
        /// <summary>
        /// Handle chunk generation
        /// </summary>
        private void OnChunkGenerated(WorldChunkGeneratedEvent chunkEvent)
        {
            _obstaclesSpawnedThisChunk = 0;
            Debug.Log($"[ObstacleManager] 🏗️ New chunk generated: {chunkEvent.ChunkIndex}");
        }
        
        /// <summary>
        /// Handle difficulty changes
        /// </summary>
        private void OnDifficultyChanged(WorldDifficultyChangedEvent difficultyEvent)
        {
            SetDifficulty(difficultyEvent.NewDifficulty);
        }
        
        /// <summary>
        /// Update difficulty based on time
        /// </summary>
        private void UpdateDifficulty()
        {
            float timeBasedDifficulty = 1f + (Time.time * _difficultyIncreaseRate);
            SetDifficulty(timeBasedDifficulty);
        }
        
        /// <summary>
        /// Check if obstacle should be spawned
        /// </summary>
        private bool ShouldSpawnObstacle(Vector3 position)
        {
            // Check spawn rate
            if (Random.value > _spawnRate * _currentDifficulty)
            {
                return false;
            }
            
            // Check distance from last obstacle
            if (Vector3.Distance(position, _lastObstaclePosition) < _minObstacleDistance)
            {
                return false;
            }
            
            // Check max obstacles per chunk
            if (_obstaclesSpawnedThisChunk >= _maxObstaclesPerChunk)
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get obstacle from pool
        /// </summary>
        private ObstacleController GetObstacleFromPool()
        {
            if (_obstaclePool.Count > 0)
            {
                var obstacle = _obstaclePool.Dequeue();
                obstacle.Activate();
                return obstacle;
            }
            
            return null;
        }
        
        /// <summary>
        /// Create new obstacle
        /// </summary>
        private ObstacleController CreateNewObstacle()
        {
            GameObject obstacleObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacleObj.name = "Obstacle";
            
            var obstacle = obstacleObj.AddComponent<ObstacleController>();
            
            // Add required components
            if (obstacleObj.GetComponent<Rigidbody>() == null)
            {
                obstacleObj.AddComponent<Rigidbody>();
            }
            
            if (obstacleObj.GetComponent<Collider>() == null)
            {
                obstacleObj.AddComponent<BoxCollider>();
            }
            
            return obstacle;
        }
        
        /// <summary>
        /// Configure obstacle with type and position
        /// </summary>
        private void ConfigureObstacle(ObstacleController obstacle, Vector3 position, int laneIndex)
        {
            // Set position
            obstacle.transform.position = position;
            
            // Select obstacle type based on difficulty
            ObstacleType selectedType = SelectObstacleType();
            obstacle.SetObstacleType(selectedType);
            
            // Initialize with event bus
            obstacle.Initialize(_eventBus, laneIndex, position);
            
            // Set tag for collision detection
            obstacle.gameObject.tag = "Obstacle";
        }
        
        /// <summary>
        /// Select obstacle type based on difficulty and weights
        /// </summary>
        private ObstacleType SelectObstacleType()
        {
            float randomValue = Random.value;
            
            // High difficulty: Add Wall and Barrier types
            if (_currentDifficulty > _wallSpawnThreshold * _maxDifficulty)
            {
                if (randomValue < 0.1f)
                {
                    return ObstacleType.Wall;
                }
            }
            
            if (_currentDifficulty > _barrierSpawnThreshold * _maxDifficulty)
            {
                if (randomValue < 0.2f)
                {
                    return ObstacleType.Barrier;
                }
            }
            
            // Normal obstacle type selection based on weights
            float cumulativeWeight = 0f;
            for (int i = 0; i < _availableObstacleTypes.Length; i++)
            {
                cumulativeWeight += _obstacleTypeWeights[i];
                if (randomValue <= cumulativeWeight)
                {
                    return _availableObstacleTypes[i];
                }
            }
            
            // Default to first available type
            return _availableObstacleTypes[0];
        }
        
        /// <summary>
        /// Return obstacle to pool
        /// </summary>
        private void ReturnObstacleToPool(ObstacleController obstacle)
        {
            if (obstacle != null)
            {
                obstacle.Deactivate();
                _obstaclePool.Enqueue(obstacle);
            }
        }
        
        /// <summary>
        /// Cleanup obstacles that are too far behind
        /// </summary>
        private void CleanupDistantObstacles()
        {
            for (int i = _activeObstacles.Count - 1; i >= 0; i--)
            {
                var obstacle = _activeObstacles[i];
                if (obstacle == null)
                {
                    _activeObstacles.RemoveAt(i);
                    continue;
                }
                
                // Check if obstacle is too far behind (assuming player moves forward)
                float distanceFromOrigin = obstacle.transform.position.z;
                if (distanceFromOrigin < -50f) // 50 units behind
                {
                    ReturnObstacleToPool(obstacle);
                    _activeObstacles.RemoveAt(i);
                    
                    Debug.Log($"[ObstacleManager] 🗑️ Cleaned up distant obstacle at {obstacle.transform.position}");
                }
            }
        }
        #endregion
    }
} 