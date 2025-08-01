using UnityEngine;
using System.Collections;
using Core.Events;
using Core.Architecture;
using EndlessRunner.Events;
using Core.DI;

namespace EndlessRunner.World
{
    /// <summary>
    /// Adapted PlatformController for EndlessRunner framework
    /// Handles individual platform management with event-driven architecture
    /// </summary>
    public class EndlessRunnerPlatformController : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("Platform Settings")]
        [SerializeField] private Transform _endPoint;
        [SerializeField] private Transform[] _obstacleSpawnPoints;
        [SerializeField] private Transform[] _collectibleSpawnPoints;
        [SerializeField] private GameObject _obstacleHolder;
        [SerializeField] private GameObject _collectibleHolder;
        [SerializeField] private bool _skipFirstLoop = false;
        
        [Header("Obstacle Settings")]
        [SerializeField] private GameObject[] _obstaclePrefabs;
        [SerializeField] private float _obstacleSpawnChance = 0.3f;
        
        [Header("Collectible Settings")]
        [SerializeField] private GameObject[] _collectiblePrefabs;
        [SerializeField] private float _collectibleSpawnChance = 0.5f;
        
        #endregion
        
        #region Private Fields
        
        private IEventBus _eventBus;
        private bool _isInitialized = false;
        private Vector3 _originalPosition;
        
        #endregion
        
        #region Public Properties
        
        public Vector3 EndPointPosition => _endPoint ? _endPoint.position : Vector3.zero;
        public Transform[] ObstacleSpawnPoints => _obstacleSpawnPoints;
        public Transform[] CollectibleSpawnPoints => _collectibleSpawnPoints;
        public bool IsInitialized => _isInitialized;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Start()
        {
            _originalPosition = transform.position;
            InitializePlatform();
        }
        
        private void OnDestroy()
        {
            CleanupPlatform();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize the platform with event bus
        /// </summary>
        /// <param name="eventBus">Event bus for communication</param>
        public void Initialize(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _isInitialized = true;
            
            // Publish platform initialized event
            _eventBus?.Publish(new PlatformInitializedEvent(transform.position, gameObject));
            
            Debug.Log($"[EndlessRunnerPlatformController] ✅ Platform initialized at {transform.position}");
        }
        
        /// <summary>
        /// Get the end point position
        /// </summary>
        /// <returns>End point position</returns>
        public Vector3 GetEndPoint()
        {
            return EndPointPosition;
        }
        
        /// <summary>
        /// Create obstacles and collectibles on this platform
        /// </summary>
        public void CreateContent()
        {
            if (_skipFirstLoop)
            {
                _skipFirstLoop = false;
                return;
            }
            
            CreateObstacles();
            CreateCollectibles();
            
            // Publish content created event
            _eventBus?.Publish(new PlatformContentCreatedEvent(transform.position, gameObject));
        }
        
        /// <summary>
        /// Clear all content from the platform
        /// </summary>
        public void ClearContent()
        {
            ClearObstacles();
            ClearCollectibles();
            
            // Publish content cleared event
            _eventBus?.Publish(new PlatformContentClearedEvent(transform.position, gameObject));
        }
        
        /// <summary>
        /// Reset platform to initial state
        /// </summary>
        public void ResetPlatform()
        {
            transform.position = _originalPosition;
            ClearContent();
            _skipFirstLoop = false;
            
            // Publish platform reset event
            _eventBus?.Publish(new PlatformResetEvent(transform.position, gameObject));
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Initialize the platform
        /// </summary>
        private void InitializePlatform()
        {
            // Validate required components
            if (_endPoint == null)
            {
                Debug.LogError("[EndlessRunnerPlatformController] ❌ End point is required!");
                return;
            }
            
            if (_obstacleHolder == null)
            {
                Debug.LogWarning("[EndlessRunnerPlatformController] ⚠️ Obstacle holder not assigned");
            }
            
            if (_collectibleHolder == null)
            {
                Debug.LogWarning("[EndlessRunnerPlatformController] ⚠️ Collectible holder not assigned");
            }
            
            Debug.Log($"[EndlessRunnerPlatformController] 🏗️ Platform initialized at {transform.position}");
        }
        
        /// <summary>
        /// Create obstacles on the platform
        /// </summary>
        private void CreateObstacles()
        {
            if (_obstaclePrefabs == null || _obstaclePrefabs.Length == 0)
            {
                Debug.LogWarning("[EndlessRunnerPlatformController] ⚠️ No obstacle prefabs assigned");
                return;
            }
            
            if (_obstacleSpawnPoints == null || _obstacleSpawnPoints.Length == 0)
            {
                Debug.LogWarning("[EndlessRunnerPlatformController] ⚠️ No obstacle spawn points assigned");
                return;
            }
            
            foreach (var spawnPoint in _obstacleSpawnPoints)
            {
                if (spawnPoint == null) continue;
                
                if (Random.Range(0f, 1f) < _obstacleSpawnChance)
                {
                    GameObject obstaclePrefab = _obstaclePrefabs[Random.Range(0, _obstaclePrefabs.Length)];
                    GameObject obstacle = Instantiate(obstaclePrefab, spawnPoint.position, spawnPoint.rotation);
                    
                    // Always parent to obstacle holder (platform child)
                    if (_obstacleHolder != null)
                    {
                        obstacle.transform.SetParent(_obstacleHolder.transform);
                    }
                    else
                    {
                        // Fallback: parent to platform itself
                        obstacle.transform.SetParent(transform);
                    }
                    
                    // Publish obstacle spawned event
                    _eventBus?.Publish(new ObstacleSpawnedEvent(obstacle, spawnPoint.position, "Block", 0f, 0));
                    
                }
            }
        }
        
        /// <summary>
        /// Create collectibles on the platform
        /// </summary>
        private void CreateCollectibles()
        {
            if (_collectiblePrefabs == null || _collectiblePrefabs.Length == 0)
            {
                Debug.LogWarning("[EndlessRunnerPlatformController] ⚠️ No collectible prefabs assigned");
                return;
            }
            
            if (_collectibleSpawnPoints == null || _collectibleSpawnPoints.Length == 0)
            {
                Debug.LogWarning("[EndlessRunnerPlatformController] ⚠️ No collectible spawn points assigned");
                return;
            }
            
            foreach (var spawnPoint in _collectibleSpawnPoints)
            {
                if (spawnPoint == null) continue;
                
                if (Random.Range(0f, 1f) < _collectibleSpawnChance)
                {
                    GameObject collectiblePrefab = _collectiblePrefabs[Random.Range(0, _collectiblePrefabs.Length)];
                    GameObject collectible = Instantiate(collectiblePrefab, spawnPoint.position, spawnPoint.rotation);
                    
                    // Always parent to collectible holder (platform child)
                    if (_collectibleHolder != null)
                    {
                        collectible.transform.SetParent(_collectibleHolder.transform);
                    }
                    else
                    {
                        // Fallback: parent to platform itself
                        collectible.transform.SetParent(transform);
                    }
                    
                    // Publish collectible spawned event
                    _eventBus?.Publish(new CollectibleSpawnedEvent(collectible, spawnPoint.position, "Coin", 10, 0));
                    
                }
            }
        }
        
        /// <summary>
        /// Clear all obstacles from the platform
        /// </summary>
        private void ClearObstacles()
        {
            if (_obstacleHolder != null)
            {
                for (int i = _obstacleHolder.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(_obstacleHolder.transform.GetChild(i).gameObject);
                }
            }
        }
        
        /// <summary>
        /// Clear all collectibles from the platform
        /// </summary>
        private void ClearCollectibles()
        {
            if (_collectibleHolder != null)
            {
                for (int i = _collectibleHolder.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(_collectibleHolder.transform.GetChild(i).gameObject);
                }
            }
        }
        
        /// <summary>
        /// Cleanup platform resources
        /// </summary>
        private void CleanupPlatform()
        {
            ClearContent();
            
            // Publish platform destroyed event
            _eventBus?.Publish(new PlatformDestroyedEvent(transform.position, gameObject));
        }
        
        #endregion
        
        #region Gizmos
        
        private void OnDrawGizmos()
        {
            // Platform center
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 1.0f);
            
            // End point
            Gizmos.color = Color.red;
            if (_endPoint)
            {
                Gizmos.DrawWireSphere(_endPoint.position, 1.0f);
            }
            
            // Obstacle spawn points
            Gizmos.color = Color.blue;
            if (_obstacleSpawnPoints != null)
            {
                for (int i = 0; i < _obstacleSpawnPoints.Length; i++)
                {
                    if (_obstacleSpawnPoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(_obstacleSpawnPoints[i].position, 0.5f);
                    }
                }
            }
            
            // Collectible spawn points
            Gizmos.color = Color.yellow;
            if (_collectibleSpawnPoints != null)
            {
                for (int i = 0; i < _collectibleSpawnPoints.Length; i++)
                {
                    if (_collectibleSpawnPoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(_collectibleSpawnPoints[i].position, 0.3f);
                    }
                }
            }
        }
        
        #endregion
    }
    
    #region Platform Events
    
    /// <summary>
    /// Event published when a platform is initialized
    /// </summary>
    public class PlatformInitializedEvent : GameEvent
    {
        public Vector3 Position { get; }
        public GameObject Platform { get; }
        
        public PlatformInitializedEvent(Vector3 position, GameObject platform)
        {
            Position = position;
            Platform = platform;
        }
    }
    
    /// <summary>
    /// Event published when platform content is created
    /// </summary>
    public class PlatformContentCreatedEvent : GameEvent
    {
        public Vector3 Position { get; }
        public GameObject Platform { get; }
        
        public PlatformContentCreatedEvent(Vector3 position, GameObject platform)
        {
            Position = position;
            Platform = platform;
        }
    }
    
    /// <summary>
    /// Event published when platform content is cleared
    /// </summary>
    public class PlatformContentClearedEvent : GameEvent
    {
        public Vector3 Position { get; }
        public GameObject Platform { get; }
        
        public PlatformContentClearedEvent(Vector3 position, GameObject platform)
        {
            Position = position;
            Platform = platform;
        }
    }
    
    /// <summary>
    /// Event published when a platform is reset
    /// </summary>
    public class PlatformResetEvent : GameEvent
    {
        public Vector3 Position { get; }
        public GameObject Platform { get; }
        
        public PlatformResetEvent(Vector3 position, GameObject platform)
        {
            Position = position;
            Platform = platform;
        }
    }
    
    /// <summary>
    /// Event published when a platform is destroyed
    /// </summary>
    public class PlatformDestroyedEvent : GameEvent
    {
        public Vector3 Position { get; }
        public GameObject Platform { get; }
        
        public PlatformDestroyedEvent(Vector3 position, GameObject platform)
        {
            Position = position;
            Platform = platform;
        }
    }
    
    #endregion
} 