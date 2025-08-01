using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Architecture;

namespace MiniGameFramework.Core.StateManagement
{
    /// <summary>
    /// Implementation of game state management with validation and event integration.
    /// Manages global game state transitions and provides validation rules.
    /// </summary>
    public class GameStateManager : IGameStateManager
    {
        private readonly IEventBus eventBus;
        private readonly Dictionary<(GlobalGameState from, GlobalGameState to), List<Func<bool>>> customValidationRules;
        private readonly Dictionary<(GlobalGameState from, GlobalGameState to), bool> defaultTransitionRules;
        private readonly List<GlobalGameState> stateHistory;
        
        private GlobalGameState currentState;
        private GlobalGameState previousState;
        
        /// <summary>Current active game state</summary>
        public GlobalGameState CurrentState => currentState;
        
        /// <summary>Previous game state before the current one</summary>
        public GlobalGameState PreviousState => previousState;

        /// <summary>
        /// Initializes the GameStateManager with event bus dependency.
        /// </summary>
        /// <param name="eventBus">Event bus for state change notifications</param>
        public GameStateManager(IEventBus eventBus)
        {
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            
            customValidationRules = new Dictionary<(GlobalGameState, GlobalGameState), List<Func<bool>>>();
            defaultTransitionRules = new Dictionary<(GlobalGameState, GlobalGameState), bool>();
            stateHistory = new List<GlobalGameState>();
            
            InitializeDefaultTransitionRules();
            Reset();
        }

        /// <summary>
        /// Transition to a new game state with validation.
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        /// <param name="stateData">Optional data to pass with the state transition</param>
        /// <returns>True if transition was successful, false if validation failed</returns>
        public bool TransitionToState(GlobalGameState newState, object stateData = null)
        {
            // Don't transition to the same state
            if (currentState == newState)
            {
                Debug.LogWarning($"[GameStateManager] Already in state {newState}. Ignoring transition request.");
                return false;
            }

            // Validate the transition
            if (!IsValidTransition(currentState, newState))
            {
                var failureReason = $"Invalid transition from {currentState} to {newState}";
                Debug.LogError($"[GameStateManager] {failureReason}");
                
                eventBus.Publish(new StateTransitionFailedEvent(currentState, newState, failureReason, this));
                return false;
            }

            // Perform the state transition
            var oldState = currentState;
            previousState = currentState;
            currentState = newState;
            
            // Add to history (limit to last 20 states)
            stateHistory.Add(newState);
            if (stateHistory.Count > 20)
            {
                stateHistory.RemoveAt(0);
            }

            Debug.Log($"[GameStateManager] State transition: {oldState} -> {newState}");
            
            // Publish state change event
            eventBus.Publish(new GlobalGameStateChangedEvent(oldState, newState, stateData, this));
            
            return true;
        }

        /// <summary>
        /// Check if a transition from current state to target state is valid.
        /// </summary>
        /// <param name="targetState">The state to check transition to</param>
        /// <returns>True if transition is valid, false otherwise</returns>
        public bool CanTransitionTo(GlobalGameState targetState)
        {
            return IsValidTransition(currentState, targetState);
        }

        /// <summary>
        /// Check if a transition from specific state to target state is valid.
        /// </summary>
        /// <param name="fromState">The source state</param>
        /// <param name="toState">The target state</param>
        /// <returns>True if transition is valid, false otherwise</returns>
        public bool IsValidTransition(GlobalGameState fromState, GlobalGameState toState)
        {
            var transitionKey = (fromState, toState);
            
            // Check default rules first
            if (defaultTransitionRules.TryGetValue(transitionKey, out var isAllowed) && !isAllowed)
            {
                return false;
            }
            
            // Check custom validation rules
            if (customValidationRules.TryGetValue(transitionKey, out var validators))
            {
                return validators.All(validator => validator());
            }
            
            // If no specific rule found, check if it's in default allowed transitions
            return defaultTransitionRules.ContainsKey(transitionKey) && defaultTransitionRules[transitionKey];
        }

        /// <summary>
        /// Add a custom validation rule for state transitions.
        /// </summary>
        /// <param name="fromState">Source state for the rule</param>
        /// <param name="toState">Target state for the rule</param>
        /// <param name="validator">Function that returns true if transition should be allowed</param>
        public void AddTransitionRule(GlobalGameState fromState, GlobalGameState toState, Func<bool> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            var transitionKey = (fromState, toState);
            
            if (!customValidationRules.ContainsKey(transitionKey))
            {
                customValidationRules[transitionKey] = new List<Func<bool>>();
            }
            
            customValidationRules[transitionKey].Add(validator);
            Debug.Log($"[GameStateManager] Added custom transition rule: {fromState} -> {toState}");
        }

        /// <summary>
        /// Remove a custom validation rule.
        /// </summary>
        /// <param name="fromState">Source state for the rule to remove</param>
        /// <param name="toState">Target state for the rule to remove</param>
        public void RemoveTransitionRule(GlobalGameState fromState, GlobalGameState toState)
        {
            var transitionKey = (fromState, toState);
            
            if (customValidationRules.ContainsKey(transitionKey))
            {
                customValidationRules.Remove(transitionKey);
                Debug.Log($"[GameStateManager] Removed custom transition rule: {fromState} -> {toState}");
            }
        }

        /// <summary>
        /// Get all valid states that can be transitioned to from the current state.
        /// </summary>
        /// <returns>Collection of valid target states</returns>
        public IEnumerable<GlobalGameState> GetValidTransitions()
        {
            return GetValidTransitions(currentState);
        }

        /// <summary>
        /// Get all valid states that can be transitioned to from a specific state.
        /// </summary>
        /// <param name="fromState">The source state to check transitions from</param>
        /// <returns>Collection of valid target states</returns>
        public IEnumerable<GlobalGameState> GetValidTransitions(GlobalGameState fromState)
        {
            var validStates = new List<GlobalGameState>();
            
            foreach (GlobalGameState state in Enum.GetValues(typeof(GlobalGameState)))
            {
                if (state != fromState && IsValidTransition(fromState, state))
                {
                    validStates.Add(state);
                }
            }
            
            return validStates;
        }

        /// <summary>
        /// Reset the state manager to initial state (Menu).
        /// </summary>
        public void Reset()
        {
            previousState = GlobalGameState.Menu;
            currentState = GlobalGameState.Menu;
            stateHistory.Clear();
            stateHistory.Add(GlobalGameState.Menu);
            customValidationRules.Clear();
            
            Debug.Log("[GameStateManager] Reset to initial state (Menu)");
        }

        /// <summary>
        /// Get the history of state changes.
        /// </summary>
        /// <returns>List of previous states in chronological order</returns>
        public IReadOnlyList<GlobalGameState> GetStateHistory()
        {
            return stateHistory.AsReadOnly();
        }

        /// <summary>
        /// Initialize default state transition rules.
        /// Defines which state transitions are allowed by default.
        /// </summary>
        private void InitializeDefaultTransitionRules()
        {
            // From Menu state
            defaultTransitionRules[(GlobalGameState.Menu, GlobalGameState.Loading)] = true;
            
            // From Loading state
            defaultTransitionRules[(GlobalGameState.Loading, GlobalGameState.Playing)] = true;
            defaultTransitionRules[(GlobalGameState.Loading, GlobalGameState.Menu)] = true;
            
            // From Playing state
            defaultTransitionRules[(GlobalGameState.Playing, GlobalGameState.Paused)] = true;
            defaultTransitionRules[(GlobalGameState.Playing, GlobalGameState.GameOver)] = true;
            defaultTransitionRules[(GlobalGameState.Playing, GlobalGameState.Loading)] = true; // For scene transitions
            defaultTransitionRules[(GlobalGameState.Playing, GlobalGameState.Menu)] = true; // Quit to menu
            
            // From Paused state
            defaultTransitionRules[(GlobalGameState.Paused, GlobalGameState.Playing)] = true;
            defaultTransitionRules[(GlobalGameState.Paused, GlobalGameState.Menu)] = true;
            defaultTransitionRules[(GlobalGameState.Paused, GlobalGameState.GameOver)] = true;
            
            // From GameOver state
            defaultTransitionRules[(GlobalGameState.GameOver, GlobalGameState.Menu)] = true;
            defaultTransitionRules[(GlobalGameState.GameOver, GlobalGameState.Loading)] = true; // Restart game
            defaultTransitionRules[(GlobalGameState.GameOver, GlobalGameState.Playing)] = true; // Quick restart
            
            Debug.Log("[GameStateManager] Default transition rules initialized");
        }
    }
} 