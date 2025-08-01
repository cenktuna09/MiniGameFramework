using UnityEngine;
using Core.Architecture;
using Core.Events;
using EndlessRunner.Player;
using EndlessRunner.Input;
using EndlessRunner.Events;

namespace EndlessRunner.Testing
{
    /// <summary>
    /// Test script for Player Controller integration
    /// Verifies input system, event system, and player movement
    /// </summary>
    public class PlayerControllerTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool _enableTesting = true;
        [SerializeField] private bool _logEvents = true;
        
        // Components
        private PlayerController _playerController;
        private RunnerInputManager _inputManager;
        private IEventBus _eventBus;
        
        // Test state
        private bool _isInitialized = false;
        
        #region Unity Methods
        private void Start()
        {
            if (!_enableTesting) return;
            
            InitializeTest();
        }
        
        private void Update()
        {
            if (!_isInitialized || !_enableTesting) return;
            
            // Process input
            _inputManager?.ProcessInput();
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Initialize the test environment
        /// </summary>
        public void InitializeTest()
        {
            Debug.Log("[PlayerControllerTester] 🧪 Starting Player Controller test...");
            
            // Create event bus
            _eventBus = new EventBus();
            
            // Find or create player controller
            _playerController = FindObjectOfType<PlayerController>();
            if (_playerController == null)
            {
                Debug.LogError("[PlayerControllerTester] ❌ No PlayerController found in scene!");
                return;
            }
            
            // Initialize player controller
            _playerController.Initialize(_eventBus);
            
            // Create input manager
            _inputManager = new RunnerInputManager(_eventBus);
            
            // Subscribe to events for testing
            if (_logEvents)
            {
                SubscribeToTestEvents();
            }
            
            _isInitialized = true;
            
            Debug.Log("[PlayerControllerTester] ✅ Test environment initialized");
            Debug.Log("[PlayerControllerTester] 📝 Test Instructions:");
            Debug.Log("  - Click and drag horizontally to move laterally");
            Debug.Log("  - Swipe up/down to jump");
            Debug.Log("  - Swipe down with horizontal movement to slide");
        }
        
        /// <summary>
        /// Reset the test environment
        /// </summary>
        public void ResetTest()
        {
            if (_playerController != null)
            {
                _playerController.ResetPlayer();
            }
            
            if (_inputManager != null)
            {
                _inputManager.ResetInputState();
            }
            
            Debug.Log("[PlayerControllerTester] 🔄 Test environment reset");
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Subscribe to events for testing and logging
        /// </summary>
        private void SubscribeToTestEvents()
        {
            if (_eventBus == null) return;
            
            // Subscribe to player events
            _eventBus.Subscribe<PlayerMovementEvent>(OnPlayerMovement);
            _eventBus.Subscribe<PlayerJumpEvent>(OnPlayerJump);
            _eventBus.Subscribe<PlayerSlideEvent>(OnPlayerSlide);
            _eventBus.Subscribe<PlayerLateralMovementEvent>(OnPlayerLateralMovement);
            _eventBus.Subscribe<PlayerGroundedEvent>(OnPlayerGrounded);
            _eventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _eventBus.Subscribe<PlayerHealedEvent>(OnPlayerHealed);
            
            // Subscribe to game events
            _eventBus.Subscribe<GameEndedEvent>(OnGameEnded);
            
            Debug.Log("[PlayerControllerTester] 📡 Subscribed to test events");
        }
        
        #region Event Handlers
        private void OnPlayerMovement(PlayerMovementEvent movementEvent)
        {
//            Debug.Log($"[PlayerControllerTester] 🏃 Player moved: {movementEvent.Position}, Speed: {movementEvent.Speed}");
        }
        
        private void OnPlayerJump(PlayerJumpEvent jumpEvent)
        {
            Debug.Log($"[PlayerControllerTester] 🦘 Player jumped at: {jumpEvent.JumpPosition}, Force: {jumpEvent.JumpForce}");
        }
        
        private void OnPlayerSlide(PlayerSlideEvent slideEvent)
        {
            string action = slideEvent.IsSliding ? "started" : "stopped";
            Debug.Log($"[PlayerControllerTester] 🛷 Player {action} sliding at: {slideEvent.SlidePosition}");
        }
        
        private void OnPlayerLateralMovement(PlayerLateralMovementEvent lateralEvent)
        {
            Debug.Log($"[PlayerControllerTester] ↔️ Player lateral movement: {lateralEvent.LateralDirection}");
        }
        
        private void OnPlayerGrounded(PlayerGroundedEvent groundedEvent)
        {
            Debug.Log($"[PlayerControllerTester] 🌍 Player grounded: {groundedEvent.IsGrounded}");
        }
        
        private void OnPlayerDamaged(PlayerDamagedEvent damageEvent)
        {
            Debug.Log($"[PlayerControllerTester] 💔 Player damaged: {damageEvent.DamageAmount} by {damageEvent.DamageSource}");
        }
        
        private void OnPlayerHealed(PlayerHealedEvent healEvent)
        {
            Debug.Log($"[PlayerControllerTester] 💚 Player healed: {healEvent.HealAmount} from {healEvent.HealSource}");
        }
        
        private void OnGameEnded(GameEndedEvent gameEndedEvent)
        {
            Debug.Log($"[PlayerControllerTester] 🎮 Game ended: {gameEndedEvent.EndReason}");
        }
        #endregion
        #endregion
    }
} 