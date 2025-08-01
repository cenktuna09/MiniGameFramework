using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Core.Events;
using Core.Architecture;
using EndlessRunner.Events;
using EndlessRunner.StateManagement;
using Core.DI;
using Core.Common.StateManagement;

namespace EndlessRunner.World
{
    /// <summary>
    /// Adapted ScrollController for EndlessRunner framework
    /// Handles platform recycling system with event-driven architecture
    /// </summary>
    public class EndlessRunnerScrollController : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("Speed Settings")]
        [SerializeField] private float _minSpeed = 8.0f;
        [SerializeField] private float _maxSpeed = 18.0f;
        [SerializeField] private float _speedIncreaseRate = 0.5f;
        
        [Header("Platform Settings")]
        [SerializeField] private EndlessRunnerPlatformController[] _platforms;
        [SerializeField] private EndlessRunnerPlatformController _lastPlatform;
        [SerializeField] private float _recycleDistance = -15.0f;
        
        [Header("Obstacle Settings")]
        [SerializeField] private GameObject[] _obstaclePrefabs;
        
        [Header("Collectible Settings")]
        [SerializeField] private GameObject[] _collectiblePrefabs;
        
        [Header("Power-up Settings")]
        [SerializeField] private int _powerUpSpawnInterval = 5;
        
        [Header("Visual Effects")]
        [SerializeField] private Renderer _waterRenderer;
        [SerializeField] private float _waterSpeedFactor = 1.0f;
        
        #endregion
        
        #region Private Fields
        
        private IEventBus _eventBus;
        private RunnerGameState _currentGameState = RunnerGameState.Ready;
        private float _currentSpeed;
        private float _holdUpSpeed;
        private float _defaultWaterSpeed;
        private int _powerUpCounter = 0;
        
        // Event subscriptions
        private System.IDisposable _stateChangedSubscription;
        private System.IDisposable _playerMovementSubscription;
        
        #endregion
        
        #region Public Properties
        
        public float CurrentSpeed => _currentSpeed;
        public float HoldUpSpeed => _holdUpSpeed;
        public float MinSpeed => _minSpeed;
        public float MaxSpeed => _maxSpeed;
        public EndlessRunnerPlatformController[] Platforms => _platforms;
        public EndlessRunnerPlatformController LastPlatform => _lastPlatform;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Start()
        {
            // Get EventBus from ServiceLocator if not provided
            if (_eventBus == null)
            {
                _eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
                if (_eventBus == null)
                {
                    Debug.LogWarning("[EndlessRunnerScrollController] ⚠️ No EventBus found, creating new one");
                    _eventBus = new EventBus();
                }
            }
            
            InitializeScrollController();
        }
        
        private void Update()
        {
            
            if (_currentGameState == RunnerGameState.Running)
            {
                UpdatePlatformMovement();
                UpdateSpeed();
                UpdateWaterEffect();
            }
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize the scroll controller with event bus
        /// </summary>
        /// <param name="eventBus">Event bus for communication</param>
        public void Initialize(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new System.ArgumentNullException(nameof(eventBus));
            
            InitializePlatforms();
            SubscribeToEvents();
            InitializeWaterEffect();
            
            _currentSpeed = 0.0f;
            _holdUpSpeed = _minSpeed;
            
            Debug.Log("[EndlessRunnerScrollController] ✅ Initialized with event-driven architecture");
        }
        
        /// <summary>
        /// Start the scrolling system
        /// </summary>
        public void StartScrolling()
        {
            _currentSpeed = _minSpeed;
            _holdUpSpeed = _minSpeed;
            
            // Publish scrolling started event
            _eventBus?.Publish(new ScrollingStartedEvent(_currentSpeed));
            
            Debug.Log($"[EndlessRunnerScrollController] 🚀 Scrolling started at speed: {_currentSpeed}");
        }
        
        /// <summary>
        /// Stop the scrolling system
        /// </summary>
        public void StopScrolling()
        {
            _currentSpeed = 0.0f;
            
            // Publish scrolling stopped event
            _eventBus?.Publish(new ScrollingStoppedEvent());
            
            Debug.Log("[EndlessRunnerScrollController] 🛑 Scrolling stopped");
        }
        
        /// <summary>
        /// Reset the scroll controller to initial state
        /// </summary>
        public void ResetScrollController()
        {
            _currentSpeed = 0.0f;
            _holdUpSpeed = _minSpeed;
            _powerUpCounter = 0;
            
            // Reset all platforms
            foreach (var platform in _platforms)
            {
                if (platform != null)
                {
                    platform.ResetPlatform();
                }
            }
            
            // Publish reset event
            _eventBus?.Publish(new ScrollControllerResetEvent());
            
            Debug.Log("[EndlessRunnerScrollController] 🔄 Reset to initial state");
        }
        
        /// <summary>
        /// Handle crash event
        /// </summary>
        public void OnCrash()
        {
            StopScrolling();
            ResetWaterEffect();
            
            // Publish crash event
            _eventBus?.Publish(new ScrollControllerCrashedEvent());
            
            Debug.Log("[EndlessRunnerScrollController] 💥 Crash handled");
        }
        
        #endregion
        
        #region Private Methods - Initialization
        
        /// <summary>
        /// Initialize the scroll controller
        /// </summary>
        private void InitializeScrollController()
        {
            // Validate required components
            if (_platforms == null || _platforms.Length == 0)
            {
                Debug.LogError("[EndlessRunnerScrollController] ❌ No platforms assigned!");
                return;
            }
            
            if (_lastPlatform == null)
            {
                Debug.LogError("[EndlessRunnerScrollController] ❌ Last platform not assigned!");
                return;
            }
            
            // Get EventBus from ServiceLocator if not provided
            if (_eventBus == null)
            {
                _eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
                if (_eventBus == null)
                {
                    Debug.LogWarning("[EndlessRunnerScrollController] ⚠️ No EventBus found, creating new one");
                    _eventBus = new EventBus();
                }
            }
            
            // Initialize platforms
            foreach (var platform in _platforms)
            {
                if (platform != null)
                {
                    platform.Initialize(_eventBus);
                    platform.CreateContent();
                }
            }
            
            Debug.Log($"[EndlessRunnerScrollController] 🏗️ Initialized with {_platforms.Length} platforms");
        }
        
        /// <summary>
        /// Initialize platforms
        /// </summary>
        private void InitializePlatforms()
        {
            if (_platforms == null || _platforms.Length == 0)
            {
                Debug.LogError("[EndlessRunnerScrollController] ❌ No platforms assigned!");
                return;
            }
            
            foreach (var platform in _platforms)
            {
                if (platform != null)
                {
                    platform.Initialize(_eventBus);
                    platform.CreateContent();
                }
            }
            
            Debug.Log($"[EndlessRunnerScrollController] 🏗️ Platforms initialized: {_platforms.Length} platforms");
        }
        
        /// <summary>
        /// Initialize water effect
        /// </summary>
        private void InitializeWaterEffect()
        {
            if (_waterRenderer != null)
            {
                _defaultWaterSpeed = _waterRenderer.material.GetFloat("_flowSpeed");
                Debug.Log($"[EndlessRunnerScrollController] 💧 Water effect initialized");
            }
        }
        
        /// <summary>
        /// Subscribe to relevant events
        /// </summary>
        private void SubscribeToEvents()
        {
            _stateChangedSubscription = _eventBus.Subscribe<StateChangedEvent<RunnerGameState>>(OnGameStateChanged);
            _playerMovementSubscription = _eventBus.Subscribe<PlayerMovementEvent>(OnPlayerMovement);
        }
        
        #endregion
        
        #region Private Methods - Update Logic
        
        /// <summary>
        /// Update platform movement and recycling
        /// </summary>
        private void UpdatePlatformMovement()
        {
            if (_lastPlatform == null) return;
            
            foreach (var platform in _platforms)
            {
                if (platform == null) continue;
                
                // Move platform toward player (negative Z direction)
                // Platformlar karaktere doğru gelmeli
                Vector3 movement = Vector3.back * (Time.deltaTime * _currentSpeed);
                platform.transform.position += movement;
                
                // Check if platform needs recycling
                if (platform.GetEndPoint().z <= _recycleDistance)
                {
                    RecyclePlatform(platform);
                }
            }
        }
        
        /// <summary>
        /// Update speed progression
        /// </summary>
        private void UpdateSpeed()
        {
            // Increase hold up speed over time
            if (_holdUpSpeed <= _maxSpeed)
            {
                _holdUpSpeed += Time.deltaTime * _speedIncreaseRate;
            }
            
            // Set current speed
            _currentSpeed = _holdUpSpeed;
        
            // Publish speed change event
            _eventBus?.Publish(new SpeedChangedEvent(_currentSpeed, _holdUpSpeed));
        }
        
        /// <summary>
        /// Update water visual effect
        /// </summary>
        private void UpdateWaterEffect()
        {
            if (_waterRenderer != null)
            {
                _waterRenderer.material.SetFloat("_flowSpeed", _waterSpeedFactor * _currentSpeed);
            }
        }
        
        /// <summary>
        /// Recycle a platform to the front
        /// </summary>
        /// <param name="platform">Platform to recycle</param>
        private void RecyclePlatform(EndlessRunnerPlatformController platform)
        {
            // Move platform to front of last platform
            platform.transform.position = _lastPlatform.GetEndPoint();
            
            // Clear old content and create new content
            platform.ClearContent();
            platform.CreateContent();
            
            // Update last platform reference
            _lastPlatform = platform;
            
            // Publish platform recycled event
            _eventBus?.Publish(new PlatformRecycledEvent(platform.transform.position, platform.gameObject));
            
        }
        
        /// <summary>
        /// Reset water effect to default
        /// </summary>
        private void ResetWaterEffect()
        {
            if (_waterRenderer != null)
            {
                _waterRenderer.material.SetFloat("_flowSpeed", _defaultWaterSpeed);
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle game state changes
        /// </summary>
        /// <param name="stateEvent">State change event</param>
        private void OnGameStateChanged(StateChangedEvent<RunnerGameState> stateEvent)
        {
            _currentGameState = stateEvent.NewState;
            
            switch (stateEvent.NewState)
            {
                case RunnerGameState.Ready:
                    StopScrolling();
                    break;
                    
                case RunnerGameState.Running:
                    StartScrolling();
                    break;
                    
                case RunnerGameState.Paused:
                    StopScrolling();
                    break;
                    
                case RunnerGameState.GameOver:
                    StopScrolling();
                    break;
            }
            
            Debug.Log($"[EndlessRunnerScrollController] 🔄 State changed to: {stateEvent.NewState}");
        }
        
        /// <summary>
        /// Handle player movement events
        /// </summary>
        /// <param name="movementEvent">Player movement event</param>
        private void OnPlayerMovement(PlayerMovementEvent movementEvent)
        {
            // Optional: Adjust speed based on player movement
            // This can be used for dynamic speed adjustments
        }
        
        #endregion
        
        #region Cleanup
        
        /// <summary>
        /// Clean up resources and subscriptions
        /// </summary>
        private void Cleanup()
        {
            _stateChangedSubscription?.Dispose();
            _playerMovementSubscription?.Dispose();
            
            ResetWaterEffect();
            
            Debug.Log("[EndlessRunnerScrollController] 🧹 Cleanup completed");
        }
        
        #endregion
    }
    
    #region Scroll Controller Events
    
    /// <summary>
    /// Event published when scrolling starts
    /// </summary>
    public class ScrollingStartedEvent : GameEvent
    {
        public float Speed { get; }
        
        public ScrollingStartedEvent(float speed)
        {
            Speed = speed;
        }
    }
    
    /// <summary>
    /// Event published when scrolling stops
    /// </summary>
    public class ScrollingStoppedEvent : GameEvent
    {
        public ScrollingStoppedEvent() { }
    }
    
    /// <summary>
    /// Event published when scroll controller is reset
    /// </summary>
    public class ScrollControllerResetEvent : GameEvent
    {
        public ScrollControllerResetEvent() { }
    }
    
    /// <summary>
    /// Event published when scroll controller crashes
    /// </summary>
    public class ScrollControllerCrashedEvent : GameEvent
    {
        public ScrollControllerCrashedEvent() { }
    }
    
    /// <summary>
    /// Event published when a platform is recycled
    /// </summary>
    public class PlatformRecycledEvent : GameEvent
    {
        public Vector3 Position { get; }
        public GameObject Platform { get; }
        
        public PlatformRecycledEvent(Vector3 position, GameObject platform)
        {
            Position = position;
            Platform = platform;
        }
    }
    
    /// <summary>
    /// Event published when speed changes
    /// </summary>
    public class SpeedChangedEvent : GameEvent
    {
        public float CurrentSpeed { get; }
        public float HoldUpSpeed { get; }
        
        public SpeedChangedEvent(float currentSpeed, float holdUpSpeed)
        {
            CurrentSpeed = currentSpeed;
            HoldUpSpeed = holdUpSpeed;
        }
    }
    
    #endregion
} 