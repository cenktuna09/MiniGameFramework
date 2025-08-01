using UnityEngine;
using Core.Architecture;
using Core.Events;
using EndlessRunner.Collectibles;
using EndlessRunner.Events;

namespace EndlessRunner.Testing
{
    /// <summary>
    /// Test script for Collectible System integration
    /// Verifies collectible spawning, collection mechanics, and event system
    /// </summary>
    public class CollectibleSystemTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool _enableTesting = true;
        [SerializeField] private bool _logEvents = true;
        [SerializeField] private bool _autoSpawnCollectibles = true;
        [SerializeField] private float _spawnInterval = 2f;
        
        [Header("Test Collectibles")]
        [SerializeField] private CollectibleType _testCollectibleType = CollectibleType.Coin;
        [SerializeField] private Vector3 _testSpawnPosition = new Vector3(0f, 2f, 10f);
        [SerializeField] private int _testLaneIndex = 1;
        
        // Components
        private CollectibleManager _collectibleManager;
        private IEventBus _eventBus;
        
        // Test state
        private bool _isInitialized = false;
        private float _testTimer = 0f;
        private int _collectiblesSpawned = 0;
        
        #region Unity Methods
        private void Start()
        {
            if (!_enableTesting) return;
            
            InitializeTest();
        }
        
        private void Update()
        {
            if (!_isInitialized || !_enableTesting) return;
            
            // Auto-spawn collectibles for testing
            if (_autoSpawnCollectibles)
            {
                _testTimer += Time.deltaTime;
                if (_testTimer > _spawnInterval)
                {
                    _testTimer = 0f;
                    SpawnTestCollectible();
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
            Debug.Log("[CollectibleSystemTester] 🧪 Starting Collectible System test...");
            
            // Create event bus
            _eventBus = new EventBus();
            
            // Find or create collectible manager
            _collectibleManager = FindObjectOfType<CollectibleManager>();
            if (_collectibleManager == null)
            {
                Debug.LogError("[CollectibleSystemTester] ❌ No CollectibleManager found in scene!");
                return;
            }
            
            // Initialize collectible manager
            _collectibleManager.Initialize(_eventBus);
            
            // Subscribe to events for testing
            if (_logEvents)
            {
                SubscribeToTestEvents();
            }
            
            _isInitialized = true;
            
            Debug.Log("[CollectibleSystemTester] ✅ Test environment initialized");
            Debug.Log("[CollectibleSystemTester] 📝 Test Instructions:");
            Debug.Log("  - Collectibles will auto-spawn every 2 seconds");
            Debug.Log("  - Check console for event logs");
            Debug.Log("  - Test collection mechanics with player");
        }
        
        /// <summary>
        /// Reset the test environment
        /// </summary>
        public void ResetTest()
        {
            if (_collectibleManager != null)
            {
                _collectibleManager.ResetManager();
            }
            
            _testTimer = 0f;
            _collectiblesSpawned = 0;
            
            Debug.Log("[CollectibleSystemTester] 🔄 Test environment reset");
        }
        
        /// <summary>
        /// Manually spawn a test collectible
        /// </summary>
        public void SpawnTestCollectible()
        {
            if (_collectibleManager != null)
            {
                var collectible = _collectibleManager.SpawnCollectible(_testSpawnPosition, _testLaneIndex);
                if (collectible != null)
                {
                    _collectiblesSpawned++;
                    Debug.Log($"[CollectibleSystemTester] 💰 Manually spawned test collectible #{_collectiblesSpawned}");
                }
            }
        }
        
        /// <summary>
        /// Set test difficulty
        /// </summary>
        public void SetTestDifficulty(float difficulty)
        {
            if (_collectibleManager != null)
            {
                _collectibleManager.SetDifficulty(difficulty);
                Debug.Log($"[CollectibleSystemTester] 📈 Set test difficulty to: {difficulty}");
            }
        }
        
        /// <summary>
        /// Test collectible collection with player
        /// </summary>
        public void TestCollectibleCollection()
        {
            // Find player
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Find nearest collectible
                var collectibles = FindObjectsOfType<CollectibleController>();
                if (collectibles.Length > 0)
                {
                    var nearestCollectible = collectibles[0];
                    float minDistance = Vector3.Distance(player.transform.position, nearestCollectible.transform.position);
                    
                    foreach (var collectible in collectibles)
                    {
                        float distance = Vector3.Distance(player.transform.position, collectible.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestCollectible = collectible;
                        }
                    }
                    
                    // Move player to collectible for collection test
                    Vector3 collectionPosition = nearestCollectible.transform.position;
                    player.transform.position = collectionPosition;
                    
                    Debug.Log($"[CollectibleSystemTester] 💰 Testing collection with {nearestCollectible.CollectibleType} at {collectionPosition}");
                }
            }
        }
        
        /// <summary>
        /// Force collect all collectibles in scene
        /// </summary>
        public void ForceCollectAll()
        {
            var collectibles = FindObjectsOfType<CollectibleController>();
            int collectedCount = 0;
            
            foreach (var collectible in collectibles)
            {
                if (collectible.IsActive && !collectible.HasBeenCollected)
                {
                    collectible.ForceCollect();
                    collectedCount++;
                }
            }
            
            Debug.Log($"[CollectibleSystemTester] 💰 Force collected {collectedCount} collectibles!");
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Subscribe to events for testing and logging
        /// </summary>
        private void SubscribeToTestEvents()
        {
            if (_eventBus == null) return;
            
            // Subscribe to collectible events
            _eventBus.Subscribe<CollectibleSpawnedEvent>(OnCollectibleSpawned);
            _eventBus.Subscribe<CollectibleCollectedEvent>(OnCollectibleCollected);
            
            Debug.Log("[CollectibleSystemTester] 📡 Subscribed to collectible events");
        }
        
        #region Event Handlers
        private void OnCollectibleSpawned(CollectibleSpawnedEvent spawnEvent)
        {
            Debug.Log($"[CollectibleSystemTester] 💰 Collectible spawned: {spawnEvent.CollectibleType} at {spawnEvent.SpawnPosition}");
            Debug.Log($"[CollectibleSystemTester] 🎯 Lane: {spawnEvent.LaneIndex}, Value: {spawnEvent.CollectibleValue}");
        }
        
        private void OnCollectibleCollected(CollectibleCollectedEvent collectionEvent)
        {
            Debug.Log($"[CollectibleSystemTester] 💰 Collectible collected: {collectionEvent.CollectibleType} at {collectionEvent.Position}");
            Debug.Log($"[CollectibleSystemTester] 💎 Points: {collectionEvent.PointValue}, Lane: {collectionEvent.Lane}");
        }
        #endregion
        #endregion
    }
} 