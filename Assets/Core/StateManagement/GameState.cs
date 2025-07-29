using MiniGameFramework.Core.Architecture;

namespace MiniGameFramework.Core.StateManagement
{
    /// <summary>
    /// Defines the possible global application states.
    /// This is different from mini-game specific states (see GameState in IMiniGame).
    /// </summary>
    public enum GlobalGameState
    {
        /// <summary>Main menu state - showing game selection and options</summary>
        Menu,
        
        /// <summary>Loading state - transitioning between scenes or initializing game</summary>
        Loading,
        
        /// <summary>Playing state - active gameplay</summary>
        Playing,
        
        /// <summary>Paused state - game is paused but can be resumed</summary>
        Paused,
        
        /// <summary>Game over state - game has ended, showing results</summary>
        GameOver
    }

    /// <summary>
    /// Event fired when the global game state changes.
    /// This is different from mini-game specific state changes.
    /// </summary>
    public class GlobalGameStateChangedEvent : GameEvent
    {
        /// <summary>Previous state before the change</summary>
        public GlobalGameState PreviousState { get; }
        
        /// <summary>New current state after the change</summary>
        public GlobalGameState CurrentState { get; }
        
        /// <summary>Optional data associated with the state change</summary>
        public object StateData { get; }

        public GlobalGameStateChangedEvent(GlobalGameState previousState, GlobalGameState currentState, object stateData = null, object source = null) 
            : base(source)
        {
            PreviousState = previousState;
            CurrentState = currentState;
            StateData = stateData;
        }
    }

    /// <summary>
    /// Event fired when a state transition is requested but validation fails.
    /// </summary>
    public class StateTransitionFailedEvent : GameEvent
    {
        /// <summary>Current state that was active during failed transition</summary>
        public GlobalGameState CurrentState { get; }
        
        /// <summary>Requested state that failed to transition to</summary>
        public GlobalGameState RequestedState { get; }
        
        /// <summary>Reason for the transition failure</summary>
        public string FailureReason { get; }

        public StateTransitionFailedEvent(GlobalGameState currentState, GlobalGameState requestedState, string failureReason, object source = null) 
            : base(source)
        {
            CurrentState = currentState;
            RequestedState = requestedState;
            FailureReason = failureReason;
        }
    }
} 