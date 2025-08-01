using UnityEngine;
using System.Collections.Generic;
using Core.Events;
using Core.Architecture;
using EndlessRunner.Events;

namespace EndlessRunner.World
{
    /// <summary>
    /// World generator for 3D Endless Runner
    /// Handles procedural world generation and chunk management
    /// </summary>
    public class WorldGenerator : MonoBehaviour
    {
        #region Private Fields
        [Header("Generation Settings")]
        [SerializeField] private float _chunkLength = 50f;
        [SerializeField] private int _maxChunks = 5;
        [SerializeField] private float _obstacleSpawnRate = 0.3f;
        [SerializeField] private float _collectibleSpawnRate = 0.5f;
        [SerializeField] private int _laneCount = 3;
        [SerializeField] private float _laneWidth = 3f;
        [SerializeField] private float _despawnDistance = 100f;
        
        [Header("Difficulty Settings")]
        [SerializeField] private float _difficultyIncreaseRate = 0.1f;
        [SerializeField] private float _maxDifficulty = 10f;
        [SerializeField] private float _speedIncreaseRate = 0.5f;
        [SerializeField] private float _maxSpeed = 30f;
        
        [Header("Object Pooling")]
        [SerializeField] private int _groundPoolSize = 10;
        [SerializeField] private int _obstaclePoolSize = 20;
        [SerializeField] private int _collectiblePoolSize = 15;
        
        // Generation state
        private float _currentDifficulty = 1f;
        private float _currentSpeed = 10f;
        private int _chunkIndex = 0;
        private Vector3 _lastChunkPosition = Vector3.zero;
        private Vector3 _playerPosition = Vector3.zero;
        
        // Chunk management
        private Queue<WorldChunk> _activeChunks = new Queue<WorldChunk>();
        private List<GameObject> _activeObjects = new List<GameObject>();
        
        // Object pools
        private ObjectPool _groundPool;
        private ObjectPool _obstaclePool;
        private ObjectPool _collectiblePool;
        
        // Obstacle management (will be integrated later)
        // private ObstacleManager _obstacleManager;
        
        // Events
        private IEventBus _eventBus;
        private System.IDisposable _playerMovementSubscription;
        #endregion
        
        #region Public Properties
        public float ChunkLength => _chunkLength;
        public int MaxChunks => _maxChunks;
        public float ObstacleSpawnRate => _obstacleSpawnRate;
        public float CollectibleSpawnRate => _collectibleSpawnRate;
        public int LaneCount => _laneCount;
        public float LaneWidth => _laneWidth;
        public float CurrentDifficulty => _currentDifficulty;
        public float CurrentSpeed => _currentSpeed;
        public int ActiveChunkCount => _activeChunks.Count;
        #endregion
        
        #region Unity Methods
        private void Start()
        {
            InitializeObjectPools();
            Debug.Log("[WorldGenerator] ✅ World generator initialized");
        }
        
        private void Update()
        {
            UpdateDifficulty();
            CheckChunkGeneration();
            CleanupDistantObjects();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            _playerMovementSubscription?.Dispose();
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Initialize world generator with event bus
        /// </summary>
        public void Initialize(IEventBus eventBus)
        {
            _eventBus = eventBus;
            SubscribeToEvents();
            GenerateInitialChunks();
            Debug.Log("[WorldGenerator] 🌍 World generator connected to event system");
        }
        
        /// <summary>
        /// Generate a new world chunk
        /// </summary>
        public void GenerateChunk()
        {
            Vector3 chunkPosition = _lastChunkPosition + Vector3.forward * _chunkLength;
            
            // Create chunk data
            var chunk = new WorldChunk
            {
                Index = _chunkIndex,
                Position = chunkPosition,
                Length = _chunkLength,
                Difficulty = _currentDifficulty,
                Speed = _currentSpeed
            };
            
            // Generate ground
            GameObject ground = GenerateGround(chunkPosition);
            chunk.GroundObject = ground;
            
            // Generate obstacles
            List<GameObject> obstacles = GenerateObstacles(chunkPosition);
            chunk.Obstacles = obstacles;
            
            // Generate collectibles
            List<GameObject> collectibles = GenerateCollectibles(chunkPosition);
            chunk.Collectibles = collectibles;
            
            // Add to active chunks
            _activeChunks.Enqueue(chunk);
            _activeObjects.Add(ground);
            _activeObjects.AddRange(obstacles);
            _activeObjects.AddRange(collectibles);
            
            // Update state
            _lastChunkPosition = chunkPosition;
            _chunkIndex++;
            
            // Publish chunk generated event
            if (_eventBus != null)
            {
                var chunkEvent = new WorldChunkGeneratedEvent(_chunkIndex, chunkPosition, _chunkLength, obstacles.Count, collectibles.Count);
                _eventBus.Publish(chunkEvent);
            }
            
            Debug.Log($"[WorldGenerator] 🏗️ Generated chunk {_chunkIndex} at {chunkPosition}");
        }
        
        /// <summary>
        /// Set difficulty level
        /// </summary>
        public void SetDifficulty(float difficulty)
        {
            float previousDifficulty = _currentDifficulty;
            _currentDifficulty = Mathf.Clamp(difficulty, 1f, _maxDifficulty);
            
            // Publish difficulty change event
            if (_eventBus != null)
            {
                var difficultyEvent = new WorldDifficultyChangedEvent(_currentDifficulty, previousDifficulty, 0f, "Manual");
                _eventBus.Publish(difficultyEvent);
            }
            
            Debug.Log($"[WorldGenerator] 📈 Difficulty set to: {_currentDifficulty}");
        }
        
        /// <summary>
        /// Set world speed
        /// </summary>
        public void SetSpeed(float speed)
        {
            _currentSpeed = Mathf.Clamp(speed, 5f, _maxSpeed);
            Debug.Log($"[WorldGenerator] ⚡ Speed set to: {_currentSpeed}");
        }
        
        /// <summary>
        /// Reset world generator
        /// </summary>
        public void ResetGenerator()
        {
            // Clear all active objects
            foreach (var obj in _activeObjects)
            {
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
            _activeObjects.Clear();
            _activeChunks.Clear();
            
            _chunkIndex = 0;
            _lastChunkPosition = Vector3.zero;
            _currentDifficulty = 1f;
            _currentSpeed = 10f;
            
            Debug.Log("[WorldGenerator] 🔄 World generator reset");
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Initialize object pools
        /// </summary>
        private void InitializeObjectPools()
        {
            _groundPool = new ObjectPool("GroundPool", CreateGroundPrefab, _groundPoolSize);
            _obstaclePool = new ObjectPool("ObstaclePool", CreateObstaclePrefab, _obstaclePoolSize);
            _collectiblePool = new ObjectPool("CollectiblePool", CreateCollectiblePrefab, _collectiblePoolSize);
            
            Debug.Log("[WorldGenerator] 🏊 Object pools initialized");
        }
        
        /// <summary>
        /// Subscribe to events
        /// </summary>
        private void SubscribeToEvents()
        {
            if (_eventBus == null) return;
            
            // Subscribe to player movement for chunk generation
            _playerMovementSubscription = _eventBus.Subscribe<PlayerMovementEvent>(OnPlayerMovement);
            
            Debug.Log("[WorldGenerator] 📡 Subscribed to player movement events");
        }
        
        /// <summary>
        /// Handle player movement for chunk generation
        /// </summary>
        private void OnPlayerMovement(PlayerMovementEvent movementEvent)
        {
            _playerPosition = movementEvent.Position;
        }
        
        /// <summary>
        /// Generate initial chunks
        /// </summary>
        private void GenerateInitialChunks()
        {
            for (int i = 0; i < _maxChunks; i++)
            {
                GenerateChunk();
            }
        }
        
        /// <summary>
        /// Update difficulty based on time/distance
        /// </summary>
        private void UpdateDifficulty()
        {
            // Increase difficulty over time
            float timeBasedDifficulty = 1f + (Time.time * _difficultyIncreaseRate);
            SetDifficulty(timeBasedDifficulty);
            
            // Increase speed with difficulty
            float newSpeed = 10f + (_currentDifficulty * _speedIncreaseRate);
            SetSpeed(newSpeed);
        }
        
        /// <summary>
        /// Check if new chunk should be generated
        /// </summary>
        private void CheckChunkGeneration()
        {
            if (_activeChunks.Count < _maxChunks)
            {
                float distanceToLastChunk = Vector3.Distance(_playerPosition, _lastChunkPosition);
                if (distanceToLastChunk > _chunkLength * 0.8f)
                {
                    GenerateChunk();
                }
            }
        }
        
        /// <summary>
        /// Cleanup objects that are too far behind the player
        /// </summary>
        private void CleanupDistantObjects()
        {
            if (_activeChunks.Count == 0) return;
            
            var oldestChunk = _activeChunks.Peek();
            float distanceToOldestChunk = Vector3.Distance(_playerPosition, oldestChunk.Position);
            
            if (distanceToOldestChunk > _despawnDistance)
            {
                DespawnChunk(oldestChunk);
                _activeChunks.Dequeue();
            }
        }
        
        /// <summary>
        /// Despawn a chunk and return objects to pools
        /// </summary>
        private void DespawnChunk(WorldChunk chunk)
        {
            // Return ground to pool
            if (chunk.GroundObject != null)
            {
                _groundPool.ReturnObject(chunk.GroundObject);
                _activeObjects.Remove(chunk.GroundObject);
            }
            
            // Return obstacles to pool
            foreach (var obstacle in chunk.Obstacles)
            {
                if (obstacle != null)
                {
                    _obstaclePool.ReturnObject(obstacle);
                    _activeObjects.Remove(obstacle);
                }
            }
            
            // Return collectibles to pool
            foreach (var collectible in chunk.Collectibles)
            {
                if (collectible != null)
                {
                    _collectiblePool.ReturnObject(collectible);
                    _activeObjects.Remove(collectible);
                }
            }
            
            // Publish chunk despawned event
            if (_eventBus != null)
            {
                float distanceFromPlayer = Vector3.Distance(_playerPosition, chunk.Position);
                var despawnEvent = new WorldChunkDespawnedEvent(chunk.Index, chunk.Position, distanceFromPlayer);
                _eventBus.Publish(despawnEvent);
            }
            
            Debug.Log($"[WorldGenerator] 🗑️ Despawned chunk {chunk.Index}");
        }
        
        /// <summary>
        /// Generate ground for chunk
        /// </summary>
        private GameObject GenerateGround(Vector3 chunkPosition)
        {
            GameObject ground = _groundPool.GetObject();
            ground.transform.position = chunkPosition;
            ground.transform.localScale = new Vector3(_laneWidth * _laneCount, 1f, _chunkLength);
            ground.name = $"Ground_Chunk_{_chunkIndex}";
            
            Debug.Log($"[WorldGenerator] 🌱 Generated ground for chunk {_chunkIndex}");
            return ground;
        }
        
        /// <summary>
        /// Generate obstacles for chunk
        /// </summary>
        private List<GameObject> GenerateObstacles(Vector3 chunkPosition)
        {
            List<GameObject> obstacles = new List<GameObject>();
            
            for (int lane = 0; lane < _laneCount; lane++)
            {
                if (Random.value < _obstacleSpawnRate * _currentDifficulty)
                {
                    Vector3 obstaclePosition = chunkPosition + Vector3.right * (lane - 1) * _laneWidth;
                    obstaclePosition.y = 1f; // Above ground
                    
                    GameObject obstacle = _obstaclePool.GetObject();
                    obstacle.transform.position = obstaclePosition;
                    obstacle.name = $"Obstacle_Chunk_{_chunkIndex}_Lane_{lane}";
                    
                    obstacles.Add(obstacle);
                    
                    // Publish obstacle spawned event
                    if (_eventBus != null)
                    {
                        var obstacleEvent = new ObstacleSpawnedEvent(obstacle, obstaclePosition, "Cube", _currentSpeed, lane);
                        _eventBus.Publish(obstacleEvent);
                    }
                }
            }
            
            Debug.Log($"[WorldGenerator] 🚧 Generated {obstacles.Count} obstacles for chunk {_chunkIndex}");
            return obstacles;
        }
        
        /// <summary>
        /// Generate collectibles for chunk
        /// </summary>
        private List<GameObject> GenerateCollectibles(Vector3 chunkPosition)
        {
            List<GameObject> collectibles = new List<GameObject>();
            
            for (int lane = 0; lane < _laneCount; lane++)
            {
                if (Random.value < _collectibleSpawnRate)
                {
                    Vector3 collectiblePosition = chunkPosition + Vector3.right * (lane - 1) * _laneWidth;
                    collectiblePosition.y = 2f; // Above ground
                    
                    GameObject collectible = _collectiblePool.GetObject();
                    collectible.transform.position = collectiblePosition;
                    collectible.name = $"Collectible_Chunk_{_chunkIndex}_Lane_{lane}";
                    
                    collectibles.Add(collectible);
                    
                    // Publish collectible spawned event
                    if (_eventBus != null)
                    {
                        var collectibleEvent = new CollectibleSpawnedEvent(collectible, collectiblePosition, "Coin", 100, lane);
                        _eventBus.Publish(collectibleEvent);
                    }
                }
            }
            
            Debug.Log($"[WorldGenerator] 💰 Generated {collectibles.Count} collectibles for chunk {_chunkIndex}");
            return collectibles;
        }
        
        #region Object Pool Creation Methods
        private GameObject CreateGroundPrefab()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.green;
            }
            return ground;
        }
        
        private GameObject CreateObstaclePrefab()
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Renderer renderer = obstacle.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }
            return obstacle;
        }
        
        private GameObject CreateCollectiblePrefab()
        {
            GameObject collectible = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Renderer renderer = collectible.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;
            }
            return collectible;
        }
        #endregion
        #endregion
    }
    
    /// <summary>
    /// Represents a world chunk with its objects
    /// </summary>
    public class WorldChunk
    {
        public int Index { get; set; }
        public Vector3 Position { get; set; }
        public float Length { get; set; }
        public float Difficulty { get; set; }
        public float Speed { get; set; }
        public GameObject GroundObject { get; set; }
        public List<GameObject> Obstacles { get; set; } = new List<GameObject>();
        public List<GameObject> Collectibles { get; set; } = new List<GameObject>();
    }
    
    /// <summary>
    /// Simple object pool for performance optimization
    /// </summary>
    public class ObjectPool
    {
        private string _poolName;
        private System.Func<GameObject> _createFunction;
        private Queue<GameObject> _pool = new Queue<GameObject>();
        private int _maxSize;
        
        public ObjectPool(string poolName, System.Func<GameObject> createFunction, int maxSize)
        {
            _poolName = poolName;
            _createFunction = createFunction;
            _maxSize = maxSize;
            
            // Pre-populate pool
            for (int i = 0; i < maxSize / 2; i++)
            {
                var obj = _createFunction();
                obj.SetActive(false);
                _pool.Enqueue(obj);
            }
        }
        
        public GameObject GetObject()
        {
            if (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                return _createFunction();
            }
        }
        
        public void ReturnObject(GameObject obj)
        {
            if (_pool.Count < _maxSize)
            {
                obj.SetActive(false);
                _pool.Enqueue(obj);
            }
            else
            {
                Object.Destroy(obj);
            }
        }
    }
} 