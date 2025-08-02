using UnityEngine;
using EndlessRunner.Collectibles;
using Core.Architecture;
using EndlessRunner.Events;
using Core.DI;

namespace EndlessRunner.Collectibles
{
    /// <summary>
    /// Controls individual collectible behavior and properties.
    /// Extends framework's MonoBehaviour for Unity integration.
    /// </summary>
    public class CollectibleController : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("Collectible Settings")]
        [SerializeField] private CollectibleType _collectibleType = CollectibleType.Coin;
        [SerializeField] private int _pointValue = 10;
        [SerializeField] private bool _isActive = true;
        [SerializeField] private float _rotationSpeed = 90f;
        [SerializeField] private float _bobSpeed = 2f;
        [SerializeField] private float _bobHeight = 0.5f;
        
        #endregion
        
        #region Private Fields
        
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private bool _isInitialized = false;
        private float _bobTime = 0f;
        
        #endregion
        
        #region Properties
        
        public CollectibleType CollectibleType => _collectibleType;
        public int PointValue => _pointValue;
        public bool IsActive => _isActive;
        public Vector3 InitialPosition => _initialPosition;
        public Quaternion InitialRotation => _initialRotation;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeCollectible();
        }
        
        private void Start()
        {
            if (!_isInitialized)
            {
                InitializeCollectible();
            }
        }
        
        private void Update()
        {
            if (!_isActive) return;
            
            // Rotate the collectible
            if (_rotationSpeed > 0f)
            {
                transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
            }
            
            // Bob up and down
            if (_bobSpeed > 0f)
            {
                _bobTime += Time.deltaTime * _bobSpeed;
                float bobOffset = Mathf.Sin(_bobTime) * _bobHeight;
                transform.position = _initialPosition + Vector3.up * bobOffset;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;
            
            // Check if it's the player
            if (other.CompareTag("Player"))
            {
                HandlePlayerCollection(other);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize the collectible
        /// </summary>
        public void InitializeCollectible()
        {
            if (_isInitialized) return;
            
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _isActive = true;
            _isInitialized = true;
            
//            Debug.Log($"[CollectibleController] ✅ Collectible initialized: {_collectibleType} at {_initialPosition}");
        }
        
        /// <summary>
        /// Reset the collectible to its initial state
        /// </summary>
        public void ResetCollectible()
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
            _isActive = true;
            _bobTime = 0f;
            
           // Debug.Log($"[CollectibleController] 🔄 Collectible reset: {_collectibleType}");
        }
        
        /// <summary>
        /// Activate the collectible
        /// </summary>
        public void Activate()
        {
            _isActive = true;
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Deactivate the collectible
        /// </summary>
        public void Deactivate()
        {
            _isActive = false;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Set the collectible type
        /// </summary>
        /// <param name="collectibleType">Type of collectible</param>
        public void SetCollectibleType(CollectibleType collectibleType)
        {
            _collectibleType = collectibleType;
        }
        
        /// <summary>
        /// Set the point value
        /// </summary>
        /// <param name="pointValue">Point value</param>
        public void SetPointValue(int pointValue)
        {
            _pointValue = pointValue;
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
        /// Set the bob parameters
        /// </summary>
        /// <param name="bobSpeed">Bob speed</param>
        /// <param name="bobHeight">Bob height</param>
        public void SetBobParameters(float bobSpeed, float bobHeight)
        {
            _bobSpeed = bobSpeed;
            _bobHeight = bobHeight;
        }
        
        /// <summary>
        /// Set the spawn chance (for factory use)
        /// </summary>
        /// <param name="spawnChance">Spawn chance (0-1)</param>
        public void SetSpawnChance(float spawnChance)
        {
            // This is used by the factory to configure spawn chance
            // The actual spawn chance is handled by the factory, not the controller
            Debug.Log($"[CollectibleController] 🎲 Spawn chance set to: {spawnChance}");
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Handle player collection
        /// </summary>
        /// <param name="playerCollider">Player collider</param>
        private void HandlePlayerCollection(Collider playerCollider)
        {
            Debug.Log($"[CollectibleController] 💰 Player collected {_collectibleType} for {_pointValue} points");
            
            // Deactivate collectible after collection
            Deactivate();
            
            // Publish collection event
            var collectionEvent = new CollectibleCollectedEvent(
                gameObject,
                playerCollider.gameObject,
                transform.position,
                _collectibleType.ToString(),
                _pointValue,
                0 // lane parameter
            );
            
            // Find EventBus and publish
            var eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            eventBus?.Publish(collectionEvent);
            
            // Also publish CollectiblePickedUpEvent for RunnerScoreManager
            var pickupEvent = new CollectiblePickedUpEvent(
                gameObject,
                playerCollider.gameObject,
                transform.position,
                _collectibleType.ToString(),
                _pointValue,
                false // isCombo parameter
            );
            eventBus?.Publish(pickupEvent);
        }
        
        #endregion
    }
} 