using UnityEngine;
using Core.Events;
using Core.Architecture;
using EndlessRunner.Events;

namespace EndlessRunner.Player
{
    /// <summary>
    /// Player controller for 3D Endless Runner
    /// Handles movement, jumping, sliding, and physics
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        #region Private Fields
        [Header("Movement Settings")]
        [SerializeField] private float _forwardSpeed = 10f;
        [SerializeField] private float _lateralSpeed = 5f;
        [SerializeField] private float _jumpForce = 8f;
        [SerializeField] private float _slideDuration = 1f;
        [SerializeField] private int _maxHealth = 3;
        
        [Header("Lane Settings")]
        [SerializeField] private float _laneWidth = 3f;
        [SerializeField] private float _laneChangeSpeed = 5f;
        
        // Movement state
        private bool _isGrounded = true;
        private bool _isSliding = false;
        private bool _isJumping = false;
        private float _slideTimer = 0f;
        private int _currentHealth;
        private int _currentLane = 1; // 0=left, 1=center, 2=right
        private float _targetLaneX = 0f;
        private float _currentLaneX = 0f;
        
        // Components
        private Rigidbody _rigidbody;
        private Collider _collider;
        private Animator _animator;
        
        // Events
        private IEventBus _eventBus;
        private System.IDisposable _inputSubscription;
        #endregion
        
        #region Public Properties
        public bool IsGrounded => _isGrounded;
        public bool IsSliding => _isSliding;
        public bool IsJumping => _isJumping;
        public int CurrentHealth => _currentHealth;
        public int CurrentLane => _currentLane;
        public float ForwardSpeed => _forwardSpeed;
        public float LateralSpeed => _lateralSpeed;
        #endregion
        
        #region Unity Methods
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _animator = GetComponent<Animator>();
            
            if (_rigidbody == null)
            {
                Debug.LogError("[PlayerController] ❌ Rigidbody component required!");
            }
            
            if (_collider == null)
            {
                Debug.LogError("[PlayerController] ❌ Collider component required!");
            }
        }
        
        private void Start()
        {
            _currentHealth = _maxHealth;
            _targetLaneX = _currentLaneX = 0f; // Center lane
            Debug.Log("[PlayerController] ✅ Player controller initialized");
        }
        
        private void Update()
        {
            UpdateSlideTimer();
            UpdateLaneMovement();
            CheckGrounded();
        }
        
        private void FixedUpdate()
        {
            ApplyForwardMovement();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            _inputSubscription?.Dispose();
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Initialize player controller with event bus
        /// </summary>
        public void Initialize(IEventBus eventBus)
        {
            _eventBus = eventBus;
            SubscribeToEvents();
            Debug.Log("[PlayerController] 🎮 Player controller connected to event system");
        }
        
        /// <summary>
        /// Move player laterally (left/right)
        /// </summary>
        public void MoveLaterally(float direction)
        {
            if (_isSliding) return;
            
            // Calculate target lane based on direction
            int targetLane = Mathf.Clamp(_currentLane + Mathf.RoundToInt(direction), 0, 2);
            if (targetLane != _currentLane)
            {
                _currentLane = targetLane;
                _targetLaneX = (_currentLane - 1) * _laneWidth; // -1 for left, 0 for center, 1 for right
                
                Debug.Log($"[PlayerController] 🛣️ Moving to lane: {_currentLane} (X: {_targetLaneX})");
            }
        }
        
        /// <summary>
        /// Make player jump
        /// </summary>
        public void Jump()
        {
            if (!_isGrounded || _isSliding) 
            {
                Debug.Log("[PlayerController] ⚠️ Cannot jump - not grounded or sliding");
                return;
            }
            
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _isJumping = true;
            _isGrounded = false;
            
            // Publish jump event
            if (_eventBus != null)
            {
                var jumpEvent = new PlayerJumpEvent(transform.position, _jumpForce, 0f, false);
                _eventBus.Publish(jumpEvent);
            }
            
            Debug.Log("[PlayerController] 🦘 Player jumped!");
        }
        
        /// <summary>
        /// Make player slide
        /// </summary>
        public void Slide()
        {
            if (_isSliding || _isJumping) 
            {
                Debug.Log("[PlayerController] ⚠️ Cannot slide - already sliding or jumping");
                return;
            }
            
            _isSliding = true;
            _slideTimer = _slideDuration;
            
            // Adjust collider for sliding
            if (_collider is CapsuleCollider capsuleCollider)
            {
                capsuleCollider.height *= 0.5f;
                capsuleCollider.center = new Vector3(0, -0.25f, 0);
            }
            
            // Publish slide event
            if (_eventBus != null)
            {
                var slideEvent = new PlayerSlideEvent(transform.position, _slideDuration, 0f, true);
                _eventBus.Publish(slideEvent);
            }
            
            Debug.Log("[PlayerController] 🛷 Player started sliding!");
        }
        
        /// <summary>
        /// Take damage
        /// </summary>
        public void TakeDamage(int damage)
        {
            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            
            // Publish damage event
            if (_eventBus != null)
            {
                var damageEvent = new PlayerDamagedEvent(damage, "Obstacle", transform.position, false);
                _eventBus.Publish(damageEvent);
            }
            
            Debug.Log($"[PlayerController] 💔 Player took {damage} damage! Health: {_currentHealth}");
            
            if (_currentHealth <= 0)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Heal player
        /// </summary>
        public void Heal(int healAmount)
        {
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + healAmount);
            
            // Publish heal event
            if (_eventBus != null)
            {
                var healEvent = new PlayerHealedEvent(healAmount, "Collectible", false);
                _eventBus.Publish(healEvent);
            }
            
            Debug.Log($"[PlayerController] 💚 Player healed {healAmount}! Health: {_currentHealth}");
        }
        
        /// <summary>
        /// Reset player state
        /// </summary>
        public void ResetPlayer()
        {
            _currentHealth = _maxHealth;
            _isSliding = false;
            _isJumping = false;
            _slideTimer = 0f;
            _currentLane = 1;
            _targetLaneX = _currentLaneX = 0f;
            
            // Reset collider
            if (_collider is CapsuleCollider capsuleCollider)
            {
                capsuleCollider.height = 2f;
                capsuleCollider.center = Vector3.zero;
            }
            
            // Reset position
            transform.position = new Vector3(0f, transform.position.y, transform.position.z);
            
            Debug.Log("[PlayerController] 🔄 Player state reset");
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Subscribe to input events
        /// </summary>
        private void SubscribeToEvents()
        {
            if (_eventBus == null) return;
            
            // Subscribe to lateral movement events
            _inputSubscription = _eventBus.Subscribe<PlayerLateralMovementEvent>(OnLateralMovement);
            
            // Subscribe to jump events
            _eventBus.Subscribe<PlayerJumpEvent>(OnJumpInput);
            
            // Subscribe to slide events
            _eventBus.Subscribe<PlayerSlideEvent>(OnSlideInput);
            
            Debug.Log("[PlayerController] 📡 Subscribed to input events");
        }
        
        /// <summary>
        /// Handle lateral movement input
        /// </summary>
        private void OnLateralMovement(PlayerLateralMovementEvent movementEvent)
        {
            MoveLaterally(movementEvent.LateralDirection);
        }
        
        /// <summary>
        /// Handle jump input
        /// </summary>
        private void OnJumpInput(PlayerJumpEvent jumpEvent)
        {
            Jump();
        }
        
        /// <summary>
        /// Handle slide input
        /// </summary>
        private void OnSlideInput(PlayerSlideEvent slideEvent)
        {
            if (slideEvent.IsSliding)
            {
                Slide();
            }
        }
        
        /// <summary>
        /// Apply forward movement
        /// </summary>
        private void ApplyForwardMovement()
        {
            Vector3 forwardMovement = Vector3.forward * _forwardSpeed * Time.deltaTime;
            transform.Translate(forwardMovement);
            
            // Publish movement event
            if (_eventBus != null)
            {
                var movementEvent = new PlayerMovementEvent(transform.position, forwardMovement, _forwardSpeed, 0f);
                _eventBus.Publish(movementEvent);
            }
        }
        
        /// <summary>
        /// Update lane movement (smooth lane transitions)
        /// </summary>
        private void UpdateLaneMovement()
        {
            if (Mathf.Abs(_currentLaneX - _targetLaneX) > 0.01f)
            {
                _currentLaneX = Mathf.Lerp(_currentLaneX, _targetLaneX, _laneChangeSpeed * Time.deltaTime);
                
                Vector3 newPosition = transform.position;
                newPosition.x = _currentLaneX;
                transform.position = newPosition;
            }
        }
        
        /// <summary>
        /// Update slide timer
        /// </summary>
        private void UpdateSlideTimer()
        {
            if (_isSliding)
            {
                _slideTimer -= Time.deltaTime;
                if (_slideTimer <= 0f)
                {
                    EndSlide();
                }
            }
        }
        
        /// <summary>
        /// End sliding state
        /// </summary>
        private void EndSlide()
        {
            _isSliding = false;
            _slideTimer = 0f;
            
            // Reset collider
            if (_collider is CapsuleCollider capsuleCollider)
            {
                capsuleCollider.height = 2f;
                capsuleCollider.center = Vector3.zero;
            }
            
            // Publish slide end event
            if (_eventBus != null)
            {
                var slideEvent = new PlayerSlideEvent(transform.position, 0f, 0f, false);
                _eventBus.Publish(slideEvent);
            }
            
            Debug.Log("[PlayerController] 🛷 Player stopped sliding!");
        }
        
        /// <summary>
        /// Check if player is grounded
        /// </summary>
        private void CheckGrounded()
        {
            bool wasGrounded = _isGrounded;
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
            
            if (_isGrounded && !wasGrounded)
            {
                _isJumping = false;
                
                // Publish grounded event
                if (_eventBus != null)
                {
                    var groundedEvent = new PlayerGroundedEvent(true, transform.position, 0f);
                    _eventBus.Publish(groundedEvent);
                }
            }
        }
        
        /// <summary>
        /// Handle player death
        /// </summary>
        private void Die()
        {
            Debug.Log("[PlayerController] 💀 Player died!");
            
            // Publish death event
            if (_eventBus != null)
            {
                var gameEndedEvent = new GameEndedEvent(Time.time, 0f, 0, "PlayerDeath");
                _eventBus.Publish(gameEndedEvent);
            }
        }
        #endregion
    }
} 