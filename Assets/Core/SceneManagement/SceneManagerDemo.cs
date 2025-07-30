using System.Threading.Tasks;
using UnityEngine;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.Core.DI;

namespace MiniGameFramework.Core.SceneManagement
{
    /// <summary>
    /// Demo script to test scene management features.
    /// Provides UI buttons and methods to test all scene loading functionality.
    /// </summary>
    public class SceneManagerDemo : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private string testSceneName = "SampleScene";
        [SerializeField] private float testDelay = 2f;
        [SerializeField] private bool enableKeyboardControls = true;
        
        [Header("Test Results")]
        [SerializeField] private bool showDebugLogs = true;
        
        private ISceneManager _sceneManager;
        private IEventBus _eventBus;
        private SceneTransitionManager _transitionManager;
        
        private void Start()
        {
            InitializeComponents();
            SubscribeToEvents();
            
            if (showDebugLogs)
            {
                LogTestInstructions();
            }
        }
        
        private void Update()
        {
            if (enableKeyboardControls)
            {
                HandleKeyboardInput();
            }
        }
        
        private void InitializeComponents()
        {
            // Get services from ServiceLocator
            _sceneManager = ServiceLocator.Instance?.Resolve<ISceneManager>();
            _eventBus = ServiceLocator.Instance?.Resolve<IEventBus>();
            _transitionManager = ServiceLocator.Instance?.Resolve<SceneTransitionManager>();
            
            if (_sceneManager == null)
            {
                Debug.LogError("SceneManager not found in ServiceLocator. Make sure SceneManagementBootstrapper is initialized.");
            }
            
            if (_eventBus == null)
            {
                Debug.LogError("EventBus not found in ServiceLocator.");
            }
        }
        
        private void SubscribeToEvents()
        {
            if (_eventBus == null) return;
            
            _eventBus.Subscribe<SceneLoadingStartedEvent>(OnSceneLoadingStarted);
            _eventBus.Subscribe<SceneLoadingCompletedEvent>(OnSceneLoadingCompleted);
            _eventBus.Subscribe<SceneLoadingProgressEvent>(OnSceneLoadingProgress);
            _eventBus.Subscribe<SceneTransitionEvent>(OnSceneTransition);
        }
        
        private void HandleKeyboardInput()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                TestReloadCurrentScene();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.L))
            {
                TestLoadSceneByName();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.T))
            {
                TestTransitionOnly();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.P))
            {
                TestPreloadAndActivate();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.H))
            {
                LogTestInstructions();
            }
        }
        
        private void LogTestInstructions()
        {
            Debug.Log("=== Scene Manager Demo Controls ===\n" +
                     "R - Reload current scene\n" +
                     "L - Load scene by name\n" +
                     "T - Test transition only\n" +
                     "P - Test preload and activate\n" +
                     "H - Show this help\n" +
                     "===================================");
        }
        
        #region Test Methods
        
        [ContextMenu("Test Reload Current Scene")]
        public void TestReloadCurrentScene()
        {
            if (_sceneManager == null)
            {
                LogError("SceneManager not available");
                return;
            }
            
            LogTest("=== Starting Scene Reload Test ===");
            LogTest($"Current scene: {_sceneManager.CurrentScene}");
            LogTest($"Is loading: {_sceneManager.IsLoading}");
            LogTest($"Transition manager available: {_transitionManager != null}");
            if (_transitionManager != null)
            {
                LogTest($"Is transitioning: {_transitionManager.IsTransitioning}");
            }
            
            LogTest("Reloading current scene...");
            
            _ = TestReloadCurrentSceneAsync();
        }
        
        [ContextMenu("Test Load Scene By Name")]
        public void TestLoadSceneByName()
        {
            if (_sceneManager == null)
            {
                LogError("SceneManager not available");
                return;
            }
            
            LogTest($"Loading scene by name: {testSceneName}");
            _ = TestLoadSceneByNameAsync();
        }
        
        [ContextMenu("Test Transition Only")]
        public void TestTransitionOnly()
        {
            if (_transitionManager == null)
            {
                LogError("TransitionManager not available");
                return;
            }
            
            LogTest("Testing transition without scene change...");
            _ = TestTransitionOnlyAsync();
        }
        
        [ContextMenu("Test Preload and Activate")]
        public void TestPreloadAndActivate()
        {
            if (_sceneManager == null)
            {
                LogError("SceneManager not available");
                return;
            }
            
            LogTest($"Testing preload and activate for scene: {testSceneName}");
            _ = TestPreloadAndActivateAsync();
        }
        
        #endregion
        
        #region Async Test Implementations
        
        private async Task TestReloadCurrentSceneAsync()
        {
            try
            {
                LogTest("Starting reload test...");
                await _sceneManager.ReloadCurrentSceneAsync();
                LogTest("Reload test completed successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"Reload test failed: {ex.Message}");
            }
        }
        
        private async Task TestLoadSceneByNameAsync()
        {
            try
            {
                LogTest($"Starting load scene test for: {testSceneName}");
                await _sceneManager.LoadSceneAsync(testSceneName);
                LogTest("Load scene test completed successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"Load scene test failed: {ex.Message}");
            }
        }
        
        private async Task TestTransitionOnlyAsync()
        {
            try
            {
                LogTest("Starting transition test...");
                
                var transitionData = SceneTransitionData.Default;
                transitionData.transitionDuration = 1f;
                
                await _transitionManager.PerformTransitionAsync(transitionData);
                LogTest("Transition test completed successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"Transition test failed: {ex.Message}");
            }
        }
        
        private async Task TestPreloadAndActivateAsync()
        {
            try
            {
                LogTest($"Starting preload test for: {testSceneName}");
                
                // Preload the scene
                await _sceneManager.PreloadSceneAsync(testSceneName);
                LogTest("Scene preloaded successfully");
                
                // Wait a bit to simulate doing other work
                await Task.Delay((int)(testDelay * 1000));
                
                // Activate the preloaded scene
                LogTest("Activating preloaded scene...");
                await _sceneManager.ActivatePreloadedSceneAsync(testSceneName);
                LogTest("Preload and activate test completed successfully");
            }
            catch (System.Exception ex)
            {
                LogError($"Preload and activate test failed: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnSceneLoadingStarted(SceneLoadingStartedEvent eventData)
        {
            LogEvent($"Scene loading started: {eventData.SceneName}");
        }
        
        private void OnSceneLoadingCompleted(SceneLoadingCompletedEvent eventData)
        {
            LogEvent($"Scene loading completed: {eventData.SceneName}");
        }
        
        private void OnSceneLoadingProgress(SceneLoadingProgressEvent eventData)
        {
            LogEvent($"Loading progress: {eventData.Progress:P0} - {eventData.LoadingText}");
        }
        
        private void OnSceneTransition(SceneTransitionEvent eventData)
        {
            LogEvent($"Transition {eventData.State}: {eventData.TransitionType}");
        }
        
        #endregion
        
        #region Logging
        
        private void LogTest(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[SceneManagerDemo] {message}");
            }
        }
        
        private void LogEvent(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[SceneManagerDemo - Event] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[SceneManagerDemo - ERROR] {message}");
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<SceneLoadingStartedEvent>(OnSceneLoadingStarted);
                _eventBus.Unsubscribe<SceneLoadingCompletedEvent>(OnSceneLoadingCompleted);
                _eventBus.Unsubscribe<SceneLoadingProgressEvent>(OnSceneLoadingProgress);
                _eventBus.Unsubscribe<SceneTransitionEvent>(OnSceneTransition);
            }
        }
    }
} 