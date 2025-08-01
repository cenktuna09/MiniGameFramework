using UnityEngine;
using System.Collections.Generic;
using Core.Events;
using Core.Architecture;
using EndlessRunner.Events;

namespace EndlessRunner.Collectibles
{
    /// <summary>
    /// Collectible manager for 3D Endless Runner
    /// Handles collectible spawning, type selection, and management
    /// </summary>
    public class CollectibleManager : MonoBehaviour
    {
        #region Private Fields
        [Header("Spawn Settings")]
        [SerializeField] private float _spawnRate = 0.5f;
        [SerializeField] private float _difficultyMultiplier = 0.05f;
        [SerializeField] private int _maxCollectiblesPerChunk = 8;
        [SerializeField] private float _minCollectibleDistance = 5f;
        
        [Header("Collectible Types")]
        [SerializeField] private CollectibleType[] _availableCollectibleTypes = { CollectibleType.Coin, CollectibleType.Gem, CollectibleType.Health };
        [SerializeField] private float[] _collectibleTypeWeights = { 0.7f, 0.2f, 0.1f };
        
        [Header("Difficulty Scaling")]
        [SerializeField] private float _difficultyIncreaseRate = 0.1f;
        [SerializeField] private float _maxDifficulty = 10f;
        [SerializeField] private float _crystalSpawnThreshold = 0.7f;
        [SerializeField] private float _starSpawnThreshold = 0.9f;
        
        // Collectible management
        private List<CollectibleController> _activeCollectibles = new List<CollectibleController>();
        private Queue<CollectibleController> _collectiblePool = new Queue<CollectibleController>();
        private int _collectiblePoolSize = 25;
        
        // Spawn state
        private float _currentDifficulty = 1f;
        private Vector3 _lastCollectiblePosition = Vector3.zero;
        private int _collectiblesSpawnedThisChunk = 0;
        
        // Events
        private IEventBus _eventBus;
        private System.IDisposable _chunkGeneratedSubscription;
        private System.IDisposable _difficultyChangedSubscription;
        #endregion
        
        #region Public Properties
        public float SpawnRate => _spawnRate;
        public float CurrentDifficulty => _currentDifficulty;
        public int ActiveCollectibleCount => _activeCollectibles.Count;
        public int PooledCollectibleCount => _collectiblePool.Count;
        #endregion
        
        #region Unity Methods
        private void Start()
        {
            InitializeCollectiblePool();
            Debug.Log("[CollectibleManager] ✅ Collectible manager initialized");
        }
        
        private void Update()
        {
            UpdateDifficulty();
            CleanupDistantCollectibles();
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
        /// Initialize collectible manager with event bus
        /// </summary>
        public void Initialize(IEventBus eventBus)
        {
            _eventBus = eventBus;
            SubscribeToEvents();
            Debug.Log("[CollectibleManager] 💰 Collectible manager connected to event system");
        }
        
        /// <summary>
        /// Spawn collectible at specified position
        /// </summary>
        public CollectibleController SpawnCollectible(Vector3 position, int laneIndex)
        {
            // Check spawn conditions
            if (!ShouldSpawnCollectible(position))
            {
                return null;
            }
            
            // Get collectible from pool or create new
            CollectibleController collectible = GetCollectibleFromPool();
            if (collectible == null)
            {
                collectible = CreateNewCollectible();
            }
            
            // Configure collectible
            ConfigureCollectible(collectible, position, laneIndex);
            
            // Add to active collectibles
            _activeCollectibles.Add(collectible);
            _collectiblesSpawnedThisChunk++;
            
            // Update state
            _lastCollectiblePosition = position;
            
            // Publish collectible spawned event
            if (_eventBus != null)
            {
                var spawnEvent = new CollectibleSpawnedEvent(collectible.gameObject, position, collectible.CollectibleType.ToString(), collectible.PointValue, laneIndex);
                _eventBus.Publish(spawnEvent);
            }
            
            Debug.Log($"[CollectibleManager] 💰 Spawned {collectible.CollectibleType} at {position}");
            return collectible;
        }
        
        /// <summary>
        /// Set difficulty level
        /// </summary>
        public void SetDifficulty(float difficulty)
        {
            _currentDifficulty = Mathf.Clamp(difficulty, 1f, _maxDifficulty);
            Debug.Log($"[CollectibleManager] 📈 Difficulty set to: {_currentDifficulty}");
        }
        
        /// <summary>
        /// Reset collectible manager
        /// </summary>
        public void ResetManager()
        {
            // Return all active collectibles to pool
            foreach (var collectible in _activeCollectibles)
            {
                if (collectible != null)
                {
                    ReturnCollectibleToPool(collectible);
                }
            }
            _activeCollectibles.Clear();
            
            // Reset state
            _currentDifficulty = 1f;
            _lastCollectiblePosition = Vector3.zero;
            _collectiblesSpawnedThisChunk = 0;
            
            Debug.Log("[CollectibleManager] 🔄 Collectible manager reset");
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Initialize collectible pool
        /// </summary>
        private void InitializeCollectiblePool()
        {
            for (int i = 0; i < _collectiblePoolSize; i++)
            {
                var collectible = CreateNewCollectible();
                ReturnCollectibleToPool(collectible);
            }
            
            Debug.Log($"[CollectibleManager] 🏊 Collectible pool initialized with {_collectiblePoolSize} collectibles");
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
            
            Debug.Log("[CollectibleManager] 📡 Subscribed to world events");
        }
        
        /// <summary>
        /// Handle chunk generation
        /// </summary>
        private void OnChunkGenerated(WorldChunkGeneratedEvent chunkEvent)
        {
            _collectiblesSpawnedThisChunk = 0;
            Debug.Log($"[CollectibleManager] 🏗️ New chunk generated: {chunkEvent.ChunkIndex}");
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
        /// Check if collectible should be spawned
        /// </summary>
        private bool ShouldSpawnCollectible(Vector3 position)
        {
            // Check spawn rate
            if (Random.value > _spawnRate * _currentDifficulty)
            {
                return false;
            }
            
            // Check distance from last collectible
            if (Vector3.Distance(position, _lastCollectiblePosition) < _minCollectibleDistance)
            {
                return false;
            }
            
            // Check max collectibles per chunk
            if (_collectiblesSpawnedThisChunk >= _maxCollectiblesPerChunk)
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get collectible from pool
        /// </summary>
        private CollectibleController GetCollectibleFromPool()
        {
            if (_collectiblePool.Count > 0)
            {
                var collectible = _collectiblePool.Dequeue();
                collectible.Activate();
                return collectible;
            }
            
            return null;
        }
        
        /// <summary>
        /// Create new collectible
        /// </summary>
        private CollectibleController CreateNewCollectible()
        {
            GameObject collectibleObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            collectibleObj.name = "Collectible";
            
            var collectible = collectibleObj.AddComponent<CollectibleController>();
            
            // Add required components
            if (collectibleObj.GetComponent<Rigidbody>() == null)
            {
                collectibleObj.AddComponent<Rigidbody>();
            }
            
            if (collectibleObj.GetComponent<Collider>() == null)
            {
                collectibleObj.AddComponent<SphereCollider>();
            }
            
            return collectible;
        }
        
        /// <summary>
        /// Configure collectible with type and position
        /// </summary>
        private void ConfigureCollectible(CollectibleController collectible, Vector3 position, int laneIndex)
        {
            // Set position
            collectible.transform.position = position;
            
            // Select collectible type based on difficulty
            CollectibleType selectedType = SelectCollectibleType();
            collectible.SetCollectibleType(selectedType);
            
            // Initialize with event bus
            collectible.Initialize(_eventBus, laneIndex, position);
            
            // Set tag for collection detection
            collectible.gameObject.tag = "Collectible";
        }
        
        /// <summary>
        /// Select collectible type based on difficulty and weights
        /// </summary>
        private CollectibleType SelectCollectibleType()
        {
            float randomValue = Random.value;
            
            // High difficulty: Add Crystal and Star types
            if (_currentDifficulty > _starSpawnThreshold * _maxDifficulty)
            {
                if (randomValue < 0.05f)
                {
                    return CollectibleType.Star;
                }
            }
            
            if (_currentDifficulty > _crystalSpawnThreshold * _maxDifficulty)
            {
                if (randomValue < 0.1f)
                {
                    return CollectibleType.Crystal;
                }
            }
            
            // Normal collectible type selection based on weights
            float cumulativeWeight = 0f;
            for (int i = 0; i < _availableCollectibleTypes.Length; i++)
            {
                cumulativeWeight += _collectibleTypeWeights[i];
                if (randomValue <= cumulativeWeight)
                {
                    return _availableCollectibleTypes[i];
                }
            }
            
            // Default to first available type
            return _availableCollectibleTypes[0];
        }
        
        /// <summary>
        /// Return collectible to pool
        /// </summary>
        private void ReturnCollectibleToPool(CollectibleController collectible)
        {
            if (collectible != null)
            {
                collectible.Deactivate();
                _collectiblePool.Enqueue(collectible);
            }
        }
        
        /// <summary>
        /// Cleanup collectibles that are too far behind
        /// </summary>
        private void CleanupDistantCollectibles()
        {
            for (int i = _activeCollectibles.Count - 1; i >= 0; i--)
            {
                var collectible = _activeCollectibles[i];
                if (collectible == null)
                {
                    _activeCollectibles.RemoveAt(i);
                    continue;
                }
                
                // Check if collectible is too far behind (assuming player moves forward)
                float distanceFromOrigin = collectible.transform.position.z;
                if (distanceFromOrigin < -50f) // 50 units behind
                {
                    ReturnCollectibleToPool(collectible);
                    _activeCollectibles.RemoveAt(i);
                    
                    Debug.Log($"[CollectibleManager] 🗑️ Cleaned up distant collectible at {collectible.transform.position}");
                }
            }
        }
        #endregion
    }
} 