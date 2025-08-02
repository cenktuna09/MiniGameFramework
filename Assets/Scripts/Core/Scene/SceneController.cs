using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Core.Architecture;
using Core.DI;

namespace Core.Scene
{
    public class SceneController : MonoBehaviour
    {
        public static SceneController Instance { get; private set; }
        
        [Header("Scene Management")]
        [SerializeField] private bool _useTransitionManager = true;
        [SerializeField] private float _transitionDuration = 0.5f;
        
        // Core dependencies
        private IEventBus _eventBus;
        private TransitionManager _transitionManager;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDependencies();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeDependencies()
        {
            // Get EventBus from ServiceLocator
            _eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            
            // Register self with ServiceLocator
            ServiceLocator.Instance.Register<SceneController>(this);
            
            // Get TransitionManager
            _transitionManager = TransitionManager.Instance;
            
            Debug.Log("[SceneController] Initialized with ServiceLocator");
        }

        public void LoadScene(string sceneName)
        {
            Debug.Log($"[SceneController] Loading scene: {sceneName}");
            
            // Publish scene loading event
            SafePublish(new SceneLoadingEvent { SceneName = sceneName });
            
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            // Play transition out
            if (_useTransitionManager && _transitionManager != null)
            {
                _transitionManager.PlayTransitionOut();
                yield return new WaitForSeconds(_transitionDuration);
            }
            
            // Load scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
                yield return null;
            
            // Play transition in
            if (_useTransitionManager && _transitionManager != null)
            {
                _transitionManager.PlayTransitionIn();
            }
            
            // Publish scene loaded event
            SafePublish(new SceneLoadedEvent { SceneName = sceneName });
            
            Debug.Log($"[SceneController] Scene loaded: {sceneName}");
        }

        public void UnloadCurrentScene()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log($"[SceneController] Unloading scene: {currentScene}");
            
            StartCoroutine(UnloadSceneAsync());
        }

        private IEnumerator UnloadSceneAsync()
        {
            if (_useTransitionManager && _transitionManager != null)
            {
                _transitionManager.PlayTransitionOut();
                yield return new WaitForSeconds(_transitionDuration);
            }
            
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
            
            if (_useTransitionManager && _transitionManager != null)
            {
                _transitionManager.PlayTransitionIn();
            }
        }

        /// <summary>
        /// Safely publishes an event if EventBus is available
        /// </summary>
        private void SafePublish<T>(T eventData) where T : class
        {
            if (_eventBus != null)
            {
                _eventBus.Publish(eventData);
            }
            else
            {
                Debug.LogWarning($"[SceneController] Event not published - EventBus not available: {typeof(T).Name}");
            }
        }
    }

    #region Scene Events

    /// <summary>
    /// Event published when scene loading starts
    /// </summary>
    public class SceneLoadingEvent : IEvent
    {
        public string SceneName { get; set; }
    }

    /// <summary>
    /// Event published when scene loading completes
    /// </summary>
    public class SceneLoadedEvent : IEvent
    {
        public string SceneName { get; set; }
    }

    #endregion
}