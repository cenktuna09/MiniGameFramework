using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Architecture;
using Core.Events;
namespace Core.Common.StateManagement
{
    /// <summary>
    /// Base class for game state management with generic type support
    /// Provides transition rules, state validation, and event-driven state changes
    /// </summary>
    /// <typeparam name="T">Enum type representing game states</typeparam>
    public abstract class BaseGameStateManager<T> where T : Enum
    {
        #region Protected Fields
        
        protected IEventBus _eventBus;
        protected T _currentState;
        protected Dictionary<T, List<T>> _transitionRules;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Current state of the game
        /// </summary>
        public T CurrentState => _currentState;
        
        /// <summary>
        /// Event triggered when state changes
        /// </summary>
        public event Action<T> OnStateChanged;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Initialize the state manager with event bus
        /// </summary>
        /// <param name="eventBus">Event bus for state change notifications</param>
        protected BaseGameStateManager(IEventBus eventBus)
        {
            _eventBus = eventBus;
            Initialize();
        }
        
        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// Initialize state manager with transition rules
        /// </summary>
        protected virtual void Initialize()
        {
            _transitionRules = new Dictionary<T, List<T>>();
            SetupDefaultTransitionRules();
            
            Debug.Log($"[{GetType().Name}] ✅ State manager initialized");
        }
        
        /// <summary>
        /// Setup default transition rules for the game
        /// Must be implemented by derived classes
        /// </summary>
        protected abstract void SetupDefaultTransitionRules();
        
        /// <summary>
        /// Add a transition rule from one state to another
        /// </summary>
        /// <param name="fromState">Source state</param>
        /// <param name="toState">Target state</param>
        protected void AddTransitionRule(T fromState, T toState)
        {
            if (!_transitionRules.ContainsKey(fromState))
            {
                _transitionRules[fromState] = new List<T>();
            }
            
            if (!_transitionRules[fromState].Contains(toState))
            {
                _transitionRules[fromState].Add(toState);
                Debug.Log($"[{GetType().Name}] 🔄 Added transition rule: {fromState} -> {toState}");
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Check if transition to new state is valid
        /// </summary>
        /// <param name="newState">Target state to transition to</param>
        /// <returns>True if transition is valid, false otherwise</returns>
        public virtual bool CanTransitionTo(T newState)
        {
            if (_transitionRules.ContainsKey(_currentState))
            {
                return _transitionRules[_currentState].Contains(newState);
            }
            
            return false;
        }
        
        /// <summary>
        /// Transition to a new state if valid
        /// </summary>
        /// <param name="newState">Target state to transition to</param>
        public virtual void TransitionTo(T newState)
        {
            if (!CanTransitionTo(newState))
            {
                Debug.LogWarning($"[{GetType().Name}] ⚠️ Invalid transition: {_currentState} -> {newState}");
                return;
            }
            
            var oldState = _currentState;
            _currentState = newState;
            
            // Trigger state change event
            OnStateChanged?.Invoke(newState);
            
            // Publish state change event to event bus
            _eventBus?.Publish(new StateChangedEvent<T>(oldState, newState));
            
            Debug.Log($"[{GetType().Name}] 🔄 State transition: {oldState} -> {newState}");
        }
        
        /// <summary>
        /// Get all valid transitions from current state
        /// </summary>
        /// <returns>List of valid target states</returns>
        public List<T> GetValidTransitions()
        {
            if (_transitionRules.ContainsKey(_currentState))
            {
                return new List<T>(_transitionRules[_currentState]);
            }
            
            return new List<T>();
        }
        
        /// <summary>
        /// Check if current state is in a specific state
        /// </summary>
        /// <param name="state">State to check</param>
        /// <returns>True if current state matches</returns>
        public bool IsInState(T state)
        {
            return _currentState.Equals(state);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Event class for state changes
    /// </summary>
    /// <typeparam name="T">Enum type representing game states</typeparam>
    public class StateChangedEvent<T> : GameEvent where T : Enum
    {
        public T OldState { get; }
        public T NewState { get; }
        
        public StateChangedEvent(T oldState, T newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
} 