using UnityEngine;
using Core.Events;
using Core.Architecture;
using EndlessRunner.Events;

namespace EndlessRunner.Collectibles
{
    /// <summary>
    /// Collectible controller for 3D Endless Runner
    /// Handles collection mechanics, scoring, and collectible types
    /// </summary>
    public class CollectibleController : MonoBehaviour
    {
        #region Private Fields
        [Header("Collectible Settings")]
        [SerializeField] private CollectibleType _collectibleType = CollectibleType.Coin;
        [SerializeField] private int _pointValue = 100;
        [SerializeField] private float _collectionRadius = 1f;
        [SerializeField] private bool _autoCollect = true;
        [SerializeField] private bool _canBeCollected = true;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _collectibleColor = Color.yellow;
        [SerializeField] private bool _useAnimation = true;
        [SerializeField] private float _rotationSpeed = 90f;
        [SerializeField] private float _bobSpeed = 2f;
        [SerializeField] private float _bobHeight = 0.5f;
        
        // Collectible state
        private bool _isActive = true;
        private bool _hasBeenCollected = false;
        private Vector3 _spawnPosition;
        private int _laneIndex;
        private float _bobTimer = 0f;
        
        // Components
        private Renderer _renderer;
        private Collider _collider;
        private Rigidbody _rigidbody;
        
        // Events
        private IEventBus _eventBus;
        #endregion
        
        #region Public Properties
        public CollectibleType CollectibleType => _collectibleType;
        public int PointValue => _pointValue;
        public bool IsActive => _isActive;
        public bool HasBeenCollected => _hasBeenCollected;
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
                Debug.LogError("[CollectibleController] ❌ Renderer component required!");
            }
            
            if (_collider == null)
            {
                Debug.LogError("[CollectibleController] ❌ Collider component required!");
            }
        }
        
        private void Start()
        {
            InitializeCollectible();
        }
        
        private void Update()
        {
            if (!_isActive) return;
            
            UpdateVisuals();
            CheckCollection();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive || _hasBeenCollected) return;
            
            // Check if it's the player
            if (other.CompareTag("Player"))
            {
                HandlePlayerCollection(other.gameObject);
            }
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Initialize collectible with event bus
        /// </summary>
        public void Initialize(IEventBus eventBus, int laneIndex, Vector3 spawnPosition)
        {
            _eventBus = eventBus;
            _laneIndex = laneIndex;
            _spawnPosition = spawnPosition;
            
            Debug.Log($"[CollectibleController] 💰 Collectible initialized: {_collectibleType} at lane {_laneIndex}");
        }
        
        /// <summary>
        /// Set collectible type and update visuals
        /// </summary>
        public void SetCollectibleType(CollectibleType collectibleType)
        {
            _collectibleType = collectibleType;
            UpdateCollectibleProperties();
            Debug.Log($"[CollectibleController] 🔄 Collectible type set to: {_collectibleType}");
        }
        
        /// <summary>
        /// Set point value
        /// </summary>
        public void SetPointValue(int points)
        {
            _pointValue = Mathf.Max(1, points);
            Debug.Log($"[CollectibleController] 💎 Points set to: {_pointValue}");
        }
        
        /// <summary>
        /// Activate collectible
        /// </summary>
        public void Activate()
        {
            _isActive = true;
            _hasBeenCollected = false;
            gameObject.SetActive(true);
            
            Debug.Log("[CollectibleController] ✅ Collectible activated");
        }
        
        /// <summary>
        /// Deactivate collectible
        /// </summary>
        public void Deactivate()
        {
            _isActive = false;
            gameObject.SetActive(false);
            
            Debug.Log("[CollectibleController] ❌ Collectible deactivated");
        }
        
        /// <summary>
        /// Force collect this collectible
        /// </summary>
        public void ForceCollect()
        {
            if (_hasBeenCollected) return;
            
            _hasBeenCollected = true;
            
            // Publish collection event
            if (_eventBus != null)
            {
                var collectionEvent = new CollectibleCollectedEvent(gameObject, null, transform.position, _collectibleType.ToString(), _pointValue, _laneIndex);
                _eventBus.Publish(collectionEvent);
            }
            
            Deactivate();
            Debug.Log($"[CollectibleController] 💰 Force collected {_collectibleType}!");
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Initialize collectible properties
        /// </summary>
        private void InitializeCollectible()
        {
            UpdateCollectibleProperties();
            SetVisuals();
            
            Debug.Log($"[CollectibleController] ✅ Collectible initialized: {_collectibleType}");
        }
        
        /// <summary>
        /// Update collectible properties based on type
        /// </summary>
        private void UpdateCollectibleProperties()
        {
            switch (_collectibleType)
            {
                case CollectibleType.Coin:
                    _pointValue = 100;
                    _autoCollect = true;
                    _canBeCollected = true;
                    break;
                    
                case CollectibleType.Gem:
                    _pointValue = 500;
                    _autoCollect = true;
                    _canBeCollected = true;
                    break;
                    
                case CollectibleType.Crystal:
                    _pointValue = 1000;
                    _autoCollect = true;
                    _canBeCollected = true;
                    break;
                    
                case CollectibleType.Star:
                    _pointValue = 2000;
                    _autoCollect = true;
                    _canBeCollected = true;
                    break;
                    
                case CollectibleType.Health:
                    _pointValue = 0; // Health doesn't give points
                    _autoCollect = true;
                    _canBeCollected = true;
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
                _renderer.material.color = GetCollectibleColor();
            }
            
            // Set scale based on collectible type
            switch (_collectibleType)
            {
                case CollectibleType.Coin:
                    transform.localScale = Vector3.one;
                    break;
                    
                case CollectibleType.Gem:
                    transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);
                    break;
                    
                case CollectibleType.Crystal:
                    transform.localScale = new Vector3(1.2f, 1.5f, 1.2f);
                    break;
                    
                case CollectibleType.Star:
                    transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    break;
                    
                case CollectibleType.Health:
                    transform.localScale = new Vector3(1f, 1f, 1f);
                    break;
            }
        }
        
        /// <summary>
        /// Get color based on collectible type
        /// </summary>
        private Color GetCollectibleColor()
        {
            switch (_collectibleType)
            {
                case CollectibleType.Coin:
                    return Color.yellow;
                    
                case CollectibleType.Gem:
                    return Color.cyan;
                    
                case CollectibleType.Crystal:
                    return Color.magenta;
                    
                case CollectibleType.Star:
                    return Color.white;
                    
                case CollectibleType.Health:
                    return Color.green;
                    
                default:
                    return _collectibleColor;
            }
        }
        
        /// <summary>
        /// Update visual effects
        /// </summary>
        private void UpdateVisuals()
        {
            if (!_useAnimation) return;
            
            // Rotate collectible
            if (_rotationSpeed > 0f)
            {
                transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
            }
            
            // Bob up and down
            _bobTimer += Time.deltaTime * _bobSpeed;
            float bobOffset = Mathf.Sin(_bobTimer) * _bobHeight;
            
            Vector3 newPosition = _spawnPosition;
            newPosition.y += bobOffset;
            transform.position = newPosition;
        }
        
        /// <summary>
        /// Check for collection with player
        /// </summary>
        private void CheckCollection()
        {
            if (!_isActive || _hasBeenCollected) return;
            
            // Raycast-based collection detection as backup
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, _collectionRadius);
            foreach (var collider in nearbyColliders)
            {
                if (collider.CompareTag("Player"))
                {
                    HandlePlayerCollection(collider.gameObject);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Handle collection by player
        /// </summary>
        private void HandlePlayerCollection(GameObject player)
        {
            if (_hasBeenCollected || !_canBeCollected) return;
            
            _hasBeenCollected = true;
            
            // Get player controller
            var playerController = player.GetComponent<EndlessRunner.Player.PlayerController>();
            if (playerController != null)
            {
                // Handle different collectible types
                switch (_collectibleType)
                {
                    case CollectibleType.Health:
                        // Heal player
                        playerController.Heal(1);
                        break;
                        
                    default:
                        // Add points (handled by score manager via events)
                        break;
                }
                
                // Publish collection event
                if (_eventBus != null)
                {
                    var collectionEvent = new CollectibleCollectedEvent(gameObject, player, transform.position, _collectibleType.ToString(), _pointValue, _laneIndex);
                    _eventBus.Publish(collectionEvent);
                }
                
                Debug.Log($"[CollectibleController] 💰 Player collected {_collectibleType}! Points: {_pointValue}");
            }
            
            // Deactivate collectible
            Deactivate();
        }
        #endregion
    }
    
    /// <summary>
    /// Types of collectibles in the game
    /// </summary>
    public enum CollectibleType
    {
        Coin,       // Basic coin (100 points)
        Gem,        // Valuable gem (500 points)
        Crystal,    // Rare crystal (1000 points)
        Star,       // Special star (2000 points)
        Health      // Health pickup (heals player)
    }
} 