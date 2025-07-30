using UnityEngine;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.Core.DI;
using MiniGameFramework.Core.Events;

namespace MiniGameFramework.Core.Input
{
    /// <summary>
    /// Handles initialization of the Input System during game bootstrap.
    /// Registers InputManager with the service locator.
    /// </summary>
    public class InputSystemBootstrapper : MonoBehaviour
    {
        [Header("Input System Configuration")]
        [SerializeField] private InputManager inputManagerPrefab;
        [SerializeField] private bool initializeOnAwake = true;

        private InputManager inputManagerInstance;

        private void Awake()
        {
            if (initializeOnAwake)
            {
                InitializeInputSystem();
            }
        }

        /// <summary>
        /// Initialize the input system and register it with ServiceLocator.
        /// </summary>
        public void InitializeInputSystem()
        {
            if (inputManagerInstance != null)
            {
                Debug.LogWarning("InputSystemBootstrapper: Input system already initialized!");
                return;
            }

            // Ensure EventBus is available before initializing InputManager
            EnsureEventBusExists();

            // Create InputManager instance
            if (inputManagerPrefab != null)
            {
                var inputManagerObject = Instantiate(inputManagerPrefab.gameObject);
                inputManagerInstance = inputManagerObject.GetComponent<InputManager>();
                DontDestroyOnLoad(inputManagerObject);
            }
            else
            {
                // Fallback: create InputManager on a new GameObject
                var inputManagerObject = new GameObject("InputManager");
                inputManagerInstance = inputManagerObject.AddComponent<InputManager>();
                DontDestroyOnLoad(inputManagerObject);
                
                Debug.LogWarning("InputSystemBootstrapper: No InputManager prefab assigned, created default instance.");
            }

            // Register with ServiceLocator
            ServiceLocator.Instance.Register<IInputManager>(inputManagerInstance);

            // Initialize the input manager (now EventBus should be available)
            inputManagerInstance.Initialize(InputContext.Menu);

            Debug.Log("InputSystemBootstrapper: Input system initialized and registered with ServiceLocator.");
        }

        private void EnsureEventBusExists()
        {
            var eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            if (eventBus == null)
            {
                var eventBusInstance = new EventBus();
                ServiceLocator.Instance.Register<IEventBus>(eventBusInstance);
                Debug.Log("InputSystemBootstrapper: Created and registered EventBus service.");
            }
        }

        /// <summary>
        /// Shutdown the input system.
        /// </summary>
        public void ShutdownInputSystem()
        {
            if (inputManagerInstance != null)
            {
                ServiceLocator.Instance.Unregister<IInputManager>();
                inputManagerInstance.Dispose();
                
                if (inputManagerInstance.gameObject != null)
                {
                    Destroy(inputManagerInstance.gameObject);
                }
                
                inputManagerInstance = null;
                Debug.Log("InputSystemBootstrapper: Input system shutdown.");
            }
        }

        private void OnDestroy()
        {
            ShutdownInputSystem();
        }

        #region Editor Support

#if UNITY_EDITOR
        [Header("Editor Testing")]
        [SerializeField] private bool enableEditorTesting = false;

        private void OnValidate()
        {
            if (enableEditorTesting && Application.isPlaying && inputManagerInstance == null)
            {
                InitializeInputSystem();
            }
        }
#endif

        #endregion
    }
}