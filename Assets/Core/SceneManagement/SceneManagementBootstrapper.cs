using UnityEngine;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.Core.Events;
using MiniGameFramework.Core.DI;

namespace MiniGameFramework.Core.SceneManagement
{
    /// <summary>
    /// Bootstrapper for scene management system.
    /// Initializes and coordinates all scene management components with dependency injection.
    /// </summary>
    public class SceneManagementBootstrapper : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private bool createDefaultComponents = true;
        
        [Header("Component References")]
        [SerializeField] private SceneTransitionManager transitionManager;
        [SerializeField] private LoadingScreen loadingScreen;
        
        private IEventBus _eventBus;
        private ISceneManager _sceneManager;
        private bool _isInitialized;
        
        /// <summary>
        /// Gets the scene manager instance.
        /// </summary>
        public ISceneManager SceneManager => _sceneManager;
        
        /// <summary>
        /// Gets the transition manager instance.
        /// </summary>
        public SceneTransitionManager TransitionManager => transitionManager;
        
        /// <summary>
        /// Gets the loading screen instance.
        /// </summary>
        public LoadingScreen LoadingScreen => loadingScreen;
        
        /// <summary>
        /// Indicates if the scene management system is initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        private void Awake()
        {
            if (initializeOnAwake)
            {
                Initialize();
            }
        }
        
        /// <summary>
        /// Initialize the scene management system.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("Scene management system is already initialized");
                return;
            }
            
            Debug.Log("Initializing Scene Management System...");
            
            try
            {
                InitializeEventBus();
                CreateDefaultComponents();
                InitializeComponents();
                RegisterServices();
                
                _isInitialized = true;
                Debug.Log("Scene Management System initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize scene management system: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Manual setup method for when not using automatic initialization.
        /// </summary>
        /// <param name="eventBus">Event bus instance</param>
        /// <param name="transitionManager">Transition manager instance</param>
        /// <param name="loadingScreen">Loading screen instance</param>
        public void Setup(IEventBus eventBus, SceneTransitionManager transitionManager = null, LoadingScreen loadingScreen = null)
        {
            _eventBus = eventBus;
            this.transitionManager = transitionManager;
            this.loadingScreen = loadingScreen;
            
            InitializeComponents();
            RegisterServices();
            
            _isInitialized = true;
        }
        
        private void InitializeEventBus()
        {
            // Try to get event bus from service locator
            _eventBus = ServiceLocator.Instance?.Resolve<IEventBus>();
            
            // Create default if not found
            if (_eventBus == null)
            {
                _eventBus = new EventBus();
                
                // Register with service locator if available
                ServiceLocator.Instance?.Register<IEventBus>(_eventBus);
                
                Debug.Log("Created default EventBus");
            }
        }
        
        private void CreateDefaultComponents()
        {
            if (!createDefaultComponents) return;
            
            // Create transition manager if not assigned
            if (transitionManager == null)
            {
                var transitionGO = new GameObject("SceneTransitionManager");
                transitionGO.transform.SetParent(transform);
                transitionManager = transitionGO.AddComponent<SceneTransitionManager>();
                Debug.Log("Created default SceneTransitionManager");
            }
            
            // Create loading screen if not assigned
            if (loadingScreen == null)
            {
                var loadingGO = new GameObject("LoadingScreen");
                loadingGO.transform.SetParent(transform);
                loadingScreen = loadingGO.AddComponent<LoadingScreen>();
                Debug.Log("Created default LoadingScreen");
            }
        }
        
        private void InitializeComponents()
        {
            // Initialize transition manager
            if (transitionManager != null)
            {
                transitionManager.Initialize(_eventBus);
            }
            
            // Initialize loading screen
            if (loadingScreen != null)
            {
                loadingScreen.Initialize(_eventBus);
            }
            
            // Create scene manager
            _sceneManager = new SceneManagerImpl(_eventBus, transitionManager);
        }
        
        private void RegisterServices()
        {
            var serviceLocator = ServiceLocator.Instance;
            if (serviceLocator == null)
            {
                Debug.LogWarning("ServiceLocator not available. Scene management services will not be registered globally.");
                return;
            }
            
            // Register services
            serviceLocator.Register<ISceneManager>(_sceneManager);
            
            if (transitionManager != null)
            {
                serviceLocator.Register<SceneTransitionManager>(transitionManager);
            }
            
            if (loadingScreen != null)
            {
                serviceLocator.Register<LoadingScreen>(loadingScreen);
            }
            
            Debug.Log("Scene management services registered with ServiceLocator");
        }
        
        private void OnDestroy()
        {
            if (_isInitialized && ServiceLocator.Instance != null)
            {
                // Unregister services
                ServiceLocator.Instance.Unregister<ISceneManager>();
                ServiceLocator.Instance.Unregister<SceneTransitionManager>();
                ServiceLocator.Instance.Unregister<LoadingScreen>();
            }
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Test Scene Loading")]
        private void TestSceneLoading()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("Scene management system not initialized");
                return;
            }
            
            // Test loading the current scene (reload)
            TestReloadCurrentScene();
        }
        
        private async void TestReloadCurrentScene()
        {
            try
            {
                Debug.Log("Testing scene reload...");
                await _sceneManager.ReloadCurrentSceneAsync();
                Debug.Log("Scene reload test completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Scene reload test failed: {ex.Message}");
            }
        }
        #endif
    }
} 