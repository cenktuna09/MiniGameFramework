using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MiniGameFramework.Core.Architecture;

namespace MiniGameFramework.Core.SceneManagement
{
    /// <summary>
    /// Manages scene transitions and coordinates different transition types.
    /// Provides a unified interface for scene transition effects.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        [Header("Default Transition Settings")]
        [SerializeField] private SceneTransitionData defaultTransitionData = SceneTransitionData.Default;
        
        [Header("Transition Components")]
        [SerializeField] private FadeTransition fadeTransition;
        
        private IEventBus _eventBus;
        private Dictionary<TransitionType, ISceneTransition> _transitions;
        private ISceneTransition _currentTransition;
        
        /// <summary>
        /// Gets the currently active transition, if any.
        /// </summary>
        public ISceneTransition CurrentTransition => _currentTransition;
        
        /// <summary>
        /// Indicates if any transition is currently active.
        /// </summary>
        public bool IsTransitioning => _currentTransition?.IsTransitioning ?? false;
        
        private void Awake()
        {
            _transitions = new Dictionary<TransitionType, ISceneTransition>();
            InitializeTransitions();
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            // Ensure all transitions start in reset state
            foreach (var transition in _transitions.Values)
            {
                transition.Reset();
            }
        }
        
        /// <summary>
        /// Initialize the transition manager with dependency injection.
        /// </summary>
        /// <param name="eventBus">Event bus for publishing transition events</param>
        public void Initialize(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            
            // Initialize fade transition with event bus
            if (fadeTransition != null)
            {
                fadeTransition.Initialize(eventBus);
            }
            
            // Re-initialize all transitions with EventBus
            foreach (var transition in _transitions.Values)
            {
                if (transition is FadeTransition fade)
                {
                    fade.Initialize(eventBus);
                }
            }
        }
        
        /// <summary>
        /// Perform a scene transition with the specified configuration.
        /// </summary>
        /// <param name="transitionData">Configuration for the transition</param>
        /// <returns>Task that completes when the transition is finished</returns>
        public async Task PerformTransitionAsync(SceneTransitionData transitionData)
        {
            var transition = GetTransition(transitionData.transitionType);
            if (transition == null)
            {
                Debug.LogWarning($"Transition type {transitionData.transitionType} not available, using default fade");
                transition = GetTransition(TransitionType.Fade);
            }
            
            if (transition == null)
            {
                Debug.LogError("No transitions available!");
                return;
            }
            
            _currentTransition = transition;
            transition.Initialize(transitionData);
            
            PublishTransitionEvent(TransitionState.Started, transitionData.transitionType);
            
            try
            {
                await transition.TransitionAsync();
            }
            finally
            {
                PublishTransitionEvent(TransitionState.Completed, transitionData.transitionType);
                _currentTransition = null;
            }
        }
        
        /// <summary>
        /// Perform a fade-out transition only.
        /// </summary>
        /// <param name="duration">Duration override (uses default if null)</param>
        /// <returns>Task that completes when fade-out is finished</returns>
        public async Task FadeOutAsync(float? duration = null)
        {
            var transition = GetTransition(TransitionType.Fade);
            if (transition != null)
            {
                _currentTransition = transition;
                try
                {
                    await transition.FadeOutAsync(duration);
                }
                finally
                {
                    _currentTransition = null;
                }
            }
        }
        
        /// <summary>
        /// Perform a fade-in transition only.
        /// </summary>
        /// <param name="duration">Duration override (uses default if null)</param>
        /// <returns>Task that completes when fade-in is finished</returns>
        public async Task FadeInAsync(float? duration = null)
        {
            var transition = GetTransition(TransitionType.Fade);
            if (transition != null)
            {
                _currentTransition = transition;
                try
                {
                    await transition.FadeInAsync(duration);
                }
                finally
                {
                    _currentTransition = null;
                }
            }
        }
        
        /// <summary>
        /// Get a transition implementation by type.
        /// </summary>
        /// <param name="transitionType">Type of transition to get</param>
        /// <returns>Transition implementation or null if not found</returns>
        public ISceneTransition GetTransition(TransitionType transitionType)
        {
            _transitions.TryGetValue(transitionType, out var transition);
            return transition;
        }
        
        /// <summary>
        /// Register a custom transition implementation.
        /// </summary>
        /// <param name="transitionType">Type of transition</param>
        /// <param name="transition">Transition implementation</param>
        public void RegisterTransition(TransitionType transitionType, ISceneTransition transition)
        {
            if (transition == null)
                throw new ArgumentNullException(nameof(transition));
                
            _transitions[transitionType] = transition;
        }
        
        /// <summary>
        /// Reset all transitions to their initial state.
        /// </summary>
        public void ResetAllTransitions()
        {
            foreach (var transition in _transitions.Values)
            {
                transition.Reset();
            }
            
            _currentTransition = null;
        }
        
        private void InitializeTransitions()
        {
            // Setup fade transition
            if (fadeTransition == null)
            {
                var fadeGO = new GameObject("FadeTransition");
                fadeGO.transform.SetParent(transform);
                fadeTransition = fadeGO.AddComponent<FadeTransition>();
            }
            
            if (fadeTransition != null)
            {
                _transitions[TransitionType.Fade] = fadeTransition;
                
                // Initialize with EventBus if available
                if (_eventBus != null)
                {
                    fadeTransition.Initialize(_eventBus);
                }
            }
            
            // Initialize default transition data for all transitions
            foreach (var transition in _transitions.Values)
            {
                transition.Initialize(defaultTransitionData);
            }
        }
        
        private void PublishTransitionEvent(TransitionState state, TransitionType transitionType)
        {
            _eventBus?.Publish(new SceneTransitionEvent(state, "", transitionType, this));
        }
        
        private void OnDestroy()
        {
            ResetAllTransitions();
        }
    }
} 