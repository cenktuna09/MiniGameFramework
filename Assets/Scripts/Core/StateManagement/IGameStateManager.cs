using System;
using System.Collections.Generic;

namespace MiniGameFramework.Core.StateManagement
{
    /// <summary>
    /// Interface for managing global game state transitions and validation.
    /// Provides a centralized way to control game flow and state changes.
    /// </summary>
    public interface IGameStateManager
    {
        /// <summary>Current active game state</summary>
        GlobalGameState CurrentState { get; }
        
        /// <summary>Previous game state before the current one</summary>
        GlobalGameState PreviousState { get; }
        
        /// <summary>
        /// Transition to a new game state.
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        /// <param name="stateData">Optional data to pass with the state transition</param>
        /// <returns>True if transition was successful, false if validation failed</returns>
        bool TransitionToState(GlobalGameState newState, object stateData = null);
        
        /// <summary>
        /// Check if a transition from current state to target state is valid.
        /// </summary>
        /// <param name="targetState">The state to check transition to</param>
        /// <returns>True if transition is valid, false otherwise</returns>
        bool CanTransitionTo(GlobalGameState targetState);
        
        /// <summary>
        /// Check if a transition from specific state to target state is valid.
        /// </summary>
        /// <param name="fromState">The source state</param>
        /// <param name="toState">The target state</param>
        /// <returns>True if transition is valid, false otherwise</returns>
        bool IsValidTransition(GlobalGameState fromState, GlobalGameState toState);
        
        /// <summary>
        /// Add a custom validation rule for state transitions.
        /// </summary>
        /// <param name="fromState">Source state for the rule</param>
        /// <param name="toState">Target state for the rule</param>
        /// <param name="validator">Function that returns true if transition should be allowed</param>
        void AddTransitionRule(GlobalGameState fromState, GlobalGameState toState, Func<bool> validator);
        
        /// <summary>
        /// Remove a custom validation rule.
        /// </summary>
        /// <param name="fromState">Source state for the rule to remove</param>
        /// <param name="toState">Target state for the rule to remove</param>
        void RemoveTransitionRule(GlobalGameState fromState, GlobalGameState toState);
        
        /// <summary>
        /// Get all valid states that can be transitioned to from the current state.
        /// </summary>
        /// <returns>Collection of valid target states</returns>
        IEnumerable<GlobalGameState> GetValidTransitions();
        
        /// <summary>
        /// Get all valid states that can be transitioned to from a specific state.
        /// </summary>
        /// <param name="fromState">The source state to check transitions from</param>
        /// <returns>Collection of valid target states</returns>
        IEnumerable<GlobalGameState> GetValidTransitions(GlobalGameState fromState);
        
        /// <summary>
        /// Reset the state manager to initial state (Menu).
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Get the history of state changes.
        /// </summary>
        /// <returns>List of previous states in chronological order</returns>
        IReadOnlyList<GlobalGameState> GetStateHistory();
    }
} 