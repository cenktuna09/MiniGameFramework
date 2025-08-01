using UnityEngine;
using Core.Events;
using Core.Architecture;
using EndlessRunner.Events;

namespace EndlessRunner.Obstacles
{
    /// <summary>
    /// Obstacle controller for 3D Endless Runner
    /// Handles collision detection, damage, and obstacle types
    /// </summary>
    public class ObstacleController : MonoBehaviour
    {
        #region Private Fields
        [Header("Obstacle Settings")]
        [SerializeField] private ObstacleType _obstacleType = ObstacleType.Block;
        [SerializeField] private int _damageAmount = 1;
        [SerializeField] private float _collisionRadius = 1f;
        [SerializeField] private bool _canBeAvoided = true;
        [SerializeField] private bool _canBeDestroyed = false;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _obstacleColor = Color.red;
        [SerializeField] private bool _useAnimation = false;
        [SerializeField] private float _rotationSpeed = 0f;
        
        // Obstacle state
        private bool _isActive = true;
        private bool _hasCollided = false;
        private Vector3 _spawnPosition;
        private int _laneIndex;
        
        // Components
        private Renderer _renderer;
        private Collider _collider;
        private Rigidbody _rigidbody;
        
        // Events
        private IEventBus _eventBus;
        #endregion
        
        #region Public Properties
        public ObstacleType ObstacleType => _obstacleType;
        public int DamageAmount => _damageAmount;
        public bool IsActive => _isActive;
        public bool HasCollided => _hasCollided;
        public int LaneIndex => _laneIndex;
        public Vector3 SpawnPosition => _spawnPosition;
        #endregion
        
        #region Unity Methods
        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
            
            if (_renderer == null)
            {
                Debug.LogError("[ObstacleController] ❌ Renderer component required!");
            }
            
            if (_collider == null)
            {
                Debug.LogError("[ObstacleController] ❌ Collider component required!");
            }
        }
        
        private void Start()
        {
            InitializeObstacle();
        }
        
        private void Update()
        {
            if (!_isActive) return;
            
            UpdateVisuals();
            CheckCollision();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive || _hasCollided) return;
            
            // Check if it's the player
            if (other.CompareTag("Player"))
            {
                HandlePlayerCollision(other.gameObject);
            }
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Initialize obstacle with event bus
        /// </summary>
        public void Initialize(IEventBus eventBus, int laneIndex, Vector3 spawnPosition)
        {
            _eventBus = eventBus;
            _laneIndex = laneIndex;
            _spawnPosition = spawnPosition;
            
            Debug.Log($"[ObstacleController] 🚧 Obstacle initialized: {_obstacleType} at lane {_laneIndex}");
        }
        
        /// <summary>
        /// Set obstacle type and update visuals
        /// </summary>
        public void SetObstacleType(ObstacleType obstacleType)
        {
            _obstacleType = obstacleType;
            UpdateObstacleProperties();
            Debug.Log($"[ObstacleController] 🔄 Obstacle type set to: {_obstacleType}");
        }
        
        /// <summary>
        /// Set damage amount
        /// </summary>
        public void SetDamageAmount(int damage)
        {
            _damageAmount = Mathf.Max(1, damage);
            Debug.Log($"[ObstacleController] 💔 Damage set to: {_damageAmount}");
        }
        
        /// <summary>
        /// Activate obstacle
        /// </summary>
        public void Activate()
        {
            _isActive = true;
            _hasCollided = false;
            gameObject.SetActive(true);
            
            Debug.Log("[ObstacleController] ✅ Obstacle activated");
        }
        
        /// <summary>
        /// Deactivate obstacle
        /// </summary>
        public void Deactivate()
        {
            _isActive = false;
            gameObject.SetActive(false);
            
            Debug.Log("[ObstacleController] ❌ Obstacle deactivated");
        }
        
        /// <summary>
        /// Destroy obstacle (for destructible obstacles)
        /// </summary>
        public void DestroyObstacle()
        {
            if (!_canBeDestroyed)
            {
                Debug.LogWarning("[ObstacleController] ⚠️ This obstacle cannot be destroyed!");
                return;
            }
            
            // Publish obstacle destroyed event
            if (_eventBus != null)
            {
                var destroyedEvent = new ObstacleDestroyedEvent(gameObject, transform.position, _obstacleType, _laneIndex);
                _eventBus.Publish(destroyedEvent);
            }
            
            Deactivate();
            Debug.Log("[ObstacleController] 💥 Obstacle destroyed!");
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Initialize obstacle properties
        /// </summary>
        private void InitializeObstacle()
        {
            UpdateObstacleProperties();
            SetVisuals();
            
            Debug.Log($"[ObstacleController] ✅ Obstacle initialized: {_obstacleType}");
        }
        
        /// <summary>
        /// Update obstacle properties based on type
        /// </summary>
        private void UpdateObstacleProperties()
        {
            switch (_obstacleType)
            {
                case ObstacleType.Block:
                    _damageAmount = 1;
                    _canBeAvoided = true;
                    _canBeDestroyed = false;
                    break;
                    
                case ObstacleType.Spike:
                    _damageAmount = 2;
                    _canBeAvoided = true;
                    _canBeDestroyed = false;
                    break;
                    
                case ObstacleType.Wall:
                    _damageAmount = 3;
                    _canBeAvoided = false;
                    _canBeDestroyed = false;
                    break;
                    
                case ObstacleType.Barrier:
                    _damageAmount = 1;
                    _canBeAvoided = true;
                    _canBeDestroyed = true;
                    break;
                    
                case ObstacleType.Trap:
                    _damageAmount = 2;
                    _canBeAvoided = true;
                    _canBeDestroyed = false;
                    break;
            }
        }
        
        /// <summary>
        /// Set visual properties
        /// </summary>
        private void SetVisuals()
        {
            if (_renderer != null)
            {
                _renderer.material.color = GetObstacleColor();
            }
            
            // Set scale based on obstacle type
            switch (_obstacleType)
            {
                case ObstacleType.Block:
                    transform.localScale = Vector3.one;
                    break;
                    
                case ObstacleType.Spike:
                    transform.localScale = new Vector3(0.5f, 2f, 0.5f);
                    break;
                    
                case ObstacleType.Wall:
                    transform.localScale = new Vector3(2f, 3f, 0.5f);
                    break;
                    
                case ObstacleType.Barrier:
                    transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    break;
                    
                case ObstacleType.Trap:
                    transform.localScale = new Vector3(1f, 0.2f, 1f);
                    break;
            }
        }
        
        /// <summary>
        /// Get color based on obstacle type
        /// </summary>
        private Color GetObstacleColor()
        {
            switch (_obstacleType)
            {
                case ObstacleType.Block:
                    return Color.red;
                    
                case ObstacleType.Spike:
                    return Color.darkRed;
                    
                case ObstacleType.Wall:
                    return Color.maroon;
                    
                case ObstacleType.Barrier:
                    return Color.orange;
                    
                case ObstacleType.Trap:
                    return Color.brown;
                    
                default:
                    return _obstacleColor;
            }
        }
        
        /// <summary>
        /// Update visual effects
        /// </summary>
        private void UpdateVisuals()
        {
            if (!_useAnimation) return;
            
            // Rotate obstacle if rotation speed is set
            if (_rotationSpeed > 0f)
            {
                transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Check for collision with player
        /// </summary>
        private void CheckCollision()
        {
            if (!_isActive || _hasCollided) return;
            
            // Raycast-based collision detection as backup
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, _collisionRadius);
            foreach (var collider in nearbyColliders)
            {
                if (collider.CompareTag("Player"))
                {
                    HandlePlayerCollision(collider.gameObject);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Handle collision with player
        /// </summary>
        private void HandlePlayerCollision(GameObject player)
        {
            if (_hasCollided) return;
            
            _hasCollided = true;
            
            // Get player controller
            var playerController = player.GetComponent<EndlessRunner.Player.PlayerController>();
            if (playerController != null)
            {
                // Check if player can avoid this obstacle
                if (_canBeAvoided && CanPlayerAvoidObstacle(playerController))
                {
                    HandleObstacleAvoided();
                    return;
                }
                
                // Apply damage to player
                playerController.TakeDamage(_damageAmount);
                
                // Publish collision event
                if (_eventBus != null)
                {
                    var collisionEvent = new ObstacleCollisionEvent(gameObject, player, transform.position, _obstacleType, _damageAmount, _laneIndex);
                    _eventBus.Publish(collisionEvent);
                }
                
                Debug.Log($"[ObstacleController] 💥 Player collided with {_obstacleType}! Damage: {_damageAmount}");
            }
        }
        
        /// <summary>
        /// Check if player can avoid this obstacle
        /// </summary>
        private bool CanPlayerAvoidObstacle(EndlessRunner.Player.PlayerController playerController)
        {
            // Check if player is jumping (for ground obstacles)
            if (playerController.IsJumping && _obstacleType != ObstacleType.Wall)
            {
                return true;
            }
            
            // Check if player is sliding (for high obstacles)
            if (playerController.IsSliding && _obstacleType == ObstacleType.Trap)
            {
                return true;
            }
            
            // Check if player is in different lane
            if (playerController.CurrentLane != _laneIndex)
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Handle obstacle being avoided
        /// </summary>
        private void HandleObstacleAvoided()
        {
            // Publish obstacle avoided event
            if (_eventBus != null)
            {
                var avoidedEvent = new ObstacleAvoidedEvent(gameObject, transform.position, _obstacleType, _laneIndex);
                _eventBus.Publish(avoidedEvent);
            }
            
            Debug.Log($"[ObstacleController] ✅ Player avoided {_obstacleType}!");
        }
        #endregion
    }
    
    /// <summary>
    /// Types of obstacles in the game
    /// </summary>
    public enum ObstacleType
    {
        Block,      // Basic block obstacle
        Spike,      // High damage spike
        Wall,       // Unavoidable wall
        Barrier,    // Destructible barrier
        Trap        // Low-profile trap
    }
} 