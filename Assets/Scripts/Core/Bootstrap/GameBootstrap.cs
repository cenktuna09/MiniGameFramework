using UnityEngine;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.Core.Events;
using MiniGameFramework.Core.SaveSystem;
using MiniGameFramework.Core.StateManagement;
using MiniGameFramework.Core.DI;
using MiniGameFramework.Core.GameManagement;

namespace MiniGameFramework.Core.Bootstrap
{
    /// <summary>
    /// Bootstrap system that initializes all core services when the game starts.
    /// Ensures proper dependency injection and service registration.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Bootstrap Configuration")]
        [SerializeField] private bool _initializeOnAwake = true;
        [SerializeField] private bool _dontDestroyOnLoad = true;
        [SerializeField] private bool _logBootstrapProcess = true;

        [Header("Service References")]
        // UIManager will auto-initialize itself after services are ready

        // Service instances
        private IEventBus _eventBus;
        private ISaveSystem _saveSystem;
        private IGameStateManager _gameStateManager;
        private IGameManager _gameManager;
        private bool _isInitialized = false;

        #region Unity Lifecycle

        private void Awake()
        {
            if (_initializeOnAwake)
            {
                InitializeServices();
            }

            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            // Services are ready, UIManager will auto-initialize itself
            LogIfEnabled("Bootstrap complete, UIManager should auto-initialize");
        }

        #endregion

        #region Service Initialization

        /// <summary>
        /// Initializes all core services and registers them with ServiceLocator
        /// </summary>
        public void InitializeServices()
        {
            if (_isInitialized)
            {
                LogIfEnabled("Services already initialized, skipping...");
                return;
            }

            LogIfEnabled("Starting service initialization...");

            try
            {
                // Initialize EventBus
                InitializeEventBus();

                // Initialize SaveSystem
                InitializeSaveSystem();

                // Initialize GameStateManager (depends on EventBus)
                InitializeGameStateManager();

                // Initialize GameManager (depends on EventBus, SaveSystem, GameStateManager)
                InitializeGameManager();

                // Register services with ServiceLocator
                RegisterServices();

                _isInitialized = true;
                LogIfEnabled("All services initialized successfully!");

                // Publish bootstrap complete event
                _eventBus?.Publish(new BootstrapCompleteEvent());
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameBootstrap] Failed to initialize services: {ex.Message}", this);
            }
        }

        private void InitializeEventBus()
        {
            _eventBus = new EventBus();
            LogIfEnabled("EventBus initialized");
        }

        private void InitializeSaveSystem()
        {
            _saveSystem = new PlayerPrefsSaveSystem();
            LogIfEnabled("SaveSystem initialized");
        }

        private void InitializeGameStateManager()
        {
            _gameStateManager = new GameStateManager(_eventBus);
            LogIfEnabled("GameStateManager initialized");
        }

        private void InitializeGameManager()
        {
            // Create GameManager GameObject
            var gameManagerObject = new GameObject("GameManager");
            gameManagerObject.transform.SetParent(this.transform);
            
            // Add GameManager component directly
            _gameManager = gameManagerObject.AddComponent<GameManager>();
            LogIfEnabled("GameManager initialized");
        }

        private void RegisterServices()
        {
            var serviceLocator = ServiceLocator.Instance;

            // Register EventBus
            serviceLocator.Register<IEventBus>(_eventBus);
            LogIfEnabled("EventBus registered with ServiceLocator");

            // Register SaveSystem
            serviceLocator.Register<ISaveSystem>(_saveSystem);
            LogIfEnabled("SaveSystem registered with ServiceLocator");

            // Register GameStateManager
            serviceLocator.Register<IGameStateManager>(_gameStateManager);
            LogIfEnabled("GameStateManager registered with ServiceLocator");

            // Register GameManager (if available)
            if (_gameManager != null)
            {
                serviceLocator.Register<IGameManager>(_gameManager);
                LogIfEnabled("GameManager registered with ServiceLocator");
            }

            // Register InputManager (find it in scene)
            var inputManager = FindFirstObjectByType<MiniGameFramework.Core.Input.InputManager>();
            if (inputManager != null)
            {
                serviceLocator.Register<MiniGameFramework.Core.Input.IInputManager>(inputManager);
                LogIfEnabled("InputManager registered with ServiceLocator");
            }
            else
            {
                LogIfEnabled("InputManager not found in scene - input may not work for mini-games");
            }

            LogIfEnabled("All services registered successfully");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Manually trigger service initialization
        /// </summary>
        public void Initialize()
        {
            InitializeServices();
        }

        /// <summary>
        /// Check if bootstrap process is complete
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Get the EventBus instance
        /// </summary>
        public IEventBus EventBus => _eventBus;

        /// <summary>
        /// Get the SaveSystem instance
        /// </summary>
        public ISaveSystem SaveSystem => _saveSystem;

        /// <summary>
        /// Get the GameStateManager instance
        /// </summary>
        public IGameStateManager GameStateManager => _gameStateManager;

        /// <summary>
        /// Get the GameManager instance (if available)
        /// </summary>
        public IGameManager GameManager => _gameManager;

        /// <summary>
        /// Check if GameManager is available
        /// </summary>
        public bool IsGameManagerAvailable => _gameManager != null;

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (_isInitialized)
            {
                CleanupServices();
            }
        }

        private void CleanupServices()
        {
            LogIfEnabled("Cleaning up services...");

            // Clear ServiceLocator
            ServiceLocator.Instance.Clear();

            // Clear EventBus subscriptions
            _eventBus?.ClearAllSubscriptions();

            // Cleanup GameManager
            if (_gameManager != null)
            {
                var gameManagerComponent = _gameManager as MonoBehaviour;
                if (gameManagerComponent != null)
                {
                    DestroyImmediate(gameManagerComponent.gameObject);
                }
                _gameManager = null;
            }

            _isInitialized = false;
            LogIfEnabled("Services cleanup complete");
        }

        #endregion

        #region Utility

        private void LogIfEnabled(string message)
        {
            if (_logBootstrapProcess)
            {
                Debug.Log($"[GameBootstrap] {message}", this);
            }
        }

        #endregion
    }

    #region Bootstrap Events

    /// <summary>
    /// Event published when bootstrap process is complete
    /// </summary>
    public class BootstrapCompleteEvent
    {
        public float BootstrapTime { get; }
        public string Version { get; }

        public BootstrapCompleteEvent()
        {
            BootstrapTime = Time.time;
            Version = Application.version;
        }
    }

    #endregion
} 