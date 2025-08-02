using UnityEngine;
using Core.Architecture;
using Core.DI;

namespace Core.Scene
{
    public class TransitionManager : MonoBehaviour
    {
        public static TransitionManager Instance { get; private set; }
        
        [Header("Transition Settings")]
        [SerializeField] private Animator transitionAnimator;
        [SerializeField] private bool _useServiceLocator = true;
        
        // Core dependencies
        private IEventBus _eventBus;

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
            if (_useServiceLocator)
            {
                // Get EventBus from ServiceLocator
                _eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
                
                // Register self with ServiceLocator
                ServiceLocator.Instance.Register<TransitionManager>(this);
                
                Debug.Log("[TransitionManager] Initialized with ServiceLocator");
            }
        }

        public void PlayTransitionOut()
        {
            if (transitionAnimator != null)
            {
                transitionAnimator.SetTrigger("FadeOut");
            }
            
            // Publish transition event
            SafePublish(new TransitionEvent { IsTransitioningOut = true });
            
            Debug.Log("[TransitionManager] Playing transition out");
        }

        public void PlayTransitionIn()
        {
            if (transitionAnimator != null)
            {
                transitionAnimator.SetTrigger("FadeIn");
            }
            
            // Publish transition event
            SafePublish(new TransitionEvent { IsTransitioningOut = false });
            
            Debug.Log("[TransitionManager] Playing transition in");
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
                Debug.LogWarning($"[TransitionManager] Event not published - EventBus not available: {typeof(T).Name}");
            }
        }
    }

    #region Transition Events

    /// <summary>
    /// Event published when transition starts/ends
    /// </summary>
    public class TransitionEvent : IEvent
    {
        public bool IsTransitioningOut { get; set; }
    }

    #endregion
}