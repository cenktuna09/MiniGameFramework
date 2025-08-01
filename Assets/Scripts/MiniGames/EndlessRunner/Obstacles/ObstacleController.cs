using UnityEngine;
using EndlessRunner.Events;
using Core.Events;
using Core.DI;
using Core.Architecture;

namespace EndlessRunner.Obstacles
{
    /// <summary>
    /// Controls individual obstacle behavior and properties.
    /// Extends framework's MonoBehaviour for Unity integration.
    /// </summary>
    public class ObstacleController : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("Obstacle Settings")]
        [SerializeField] private ObstacleType _obstacleType = ObstacleType.Block;
        [SerializeField] private float _damageAmount = 1f;
        [SerializeField] private bool _isActive = true;
        [SerializeField] private float _rotationSpeed = 0f;
        
        #endregion
        
        #region Private Fields
        
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private bool _isInitialized = false;
        
        #endregion
        
        #region Properties
        
        public ObstacleType ObstacleType => _obstacleType;
        public float DamageAmount => _damageAmount;
        public bool IsActive => _isActive;
        public Vector3 InitialPosition => _initialPosition;
        public Quaternion InitialRotation => _initialRotation;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeObstacle();
        }
        
        private void Start()
        {
            if (!_isInitialized)
            {
                InitializeObstacle();
            }
        }
        
        private void Update()
        {
            if (_isActive && _rotationSpeed > 0f)
            {
                transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;
            
            // Check if it's the player
            if (other.CompareTag("Player"))
            {
                HandlePlayerCollision(other);
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!_isActive) return;
            
            // Check if it's the player
            if (collision.gameObject.CompareTag("Player"))
            {
                HandlePlayerCollision(collision.collider);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize the obstacle
        /// </summary>
        public void InitializeObstacle()
        {
            if (_isInitialized) return;
            
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _isActive = true;
            _isInitialized = true;
            
            Debug.Log($"[ObstacleController] ✅ Obstacle initialized: {_obstacleType} at {_initialPosition}");
        }
        
        /// <summary>
        /// Reset the obstacle to its initial state
        /// </summary>
        public void ResetObstacle()
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
            _isActive = true;
            
            Debug.Log($"[ObstacleController] 🔄 Obstacle reset: {_obstacleType}");
        }
        
        /// <summary>
        /// Activate the obstacle
        /// </summary>
        public void Activate()
        {
            _isActive = true;
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Deactivate the obstacle
        /// </summary>
        public void Deactivate()
        {
            _isActive = false;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Set the obstacle type
        /// </summary>
        /// <param name="obstacleType">Type of obstacle</param>
        public void SetObstacleType(ObstacleType obstacleType)
        {
            _obstacleType = obstacleType;
        }
        
        /// <summary>
        /// Set the damage amount
        /// </summary>
        /// <param name="damageAmount">Damage amount</param>
        public void SetDamageAmount(float damageAmount)
        {
            _damageAmount = damageAmount;
        }
        
        /// <summary>
        /// Set the rotation speed
        /// </summary>
        /// <param name="rotationSpeed">Rotation speed in degrees per second</param>
        public void SetRotationSpeed(float rotationSpeed)
        {
            _rotationSpeed = rotationSpeed;
        }
        
        /// <summary>
        /// Set the spawn chance (for factory use)
        /// </summary>
        /// <param name="spawnChance">Spawn chance (0-1)</param>
        public void SetSpawnChance(float spawnChance)
        {
            // This is used by the factory to configure spawn chance
            // The actual spawn chance is handled by the factory, not the controller
            Debug.Log($"[ObstacleController] 🎲 Spawn chance set to: {spawnChance}");
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Handle player collision
        /// </summary>
        /// <param name="playerCollider">Player collider</param>
        private void HandlePlayerCollision(Collider playerCollider)
        {
            Debug.Log($"[ObstacleController] 💥 Player collision with {_obstacleType} obstacle");
            
            // Deactivate obstacle after collision
            Deactivate();
            
            // Publish collision event
            var collisionEvent = new ObstacleCollisionEvent(
                gameObject,
                playerCollider.gameObject,
                transform.position,
                _obstacleType.ToString(),
                _damageAmount
            );
            
            // Find EventBus and publish
            var eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            eventBus?.Publish(collisionEvent);
        }
        
        #endregion
    }
} 