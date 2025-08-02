using System;
using System.Threading.Tasks;

namespace Core.Architecture
{
    /// <summary>
    /// Defines the contract for all mini-games in the framework.
    /// Ensures consistent lifecycle management and integration with the core framework.
    /// </summary>
    public interface IMiniGame
    {
        /// <summary>
        /// Unique identifier for this mini-game type.
        /// Used for scene loading, save data, and event routing.
        /// </summary>
        string GameId { get; }
        
        /// <summary>
        /// Display name for UI and menus.
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// Current state of the mini-game.
        /// </summary>
        GameState CurrentState { get; }
        
        /// <summary>
        /// Event fired when the game state changes.
        /// </summary>
        event Action<GameState> OnStateChanged;
        
        /// <summary>
        /// Initialize the mini-game. Called before any gameplay begins.
        /// Set up systems, load data, prepare UI.
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Start the mini-game. Called after initialization.
        /// Begin gameplay, start timers, enable input.
        /// </summary>
        void Start();
        
        /// <summary>
        /// Pause the mini-game. Called when player pauses or switches focus.
        /// Pause timers, disable input, save state if needed.
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Resume the mini-game. Called when player resumes from pause.
        /// Resume timers, enable input, restore state if needed.
        /// </summary>
        void Resume();
        
        /// <summary>
        /// End the mini-game. Called when gameplay ends (win/lose/quit).
        /// Clean up gameplay systems, save final state.
        /// </summary>
        void End();
        
        /// <summary>
        /// Clean up resources. Called when switching away from this game.
        /// Dispose of systems, clear references, free memory.
        /// </summary>
        void Cleanup();
        
        /// <summary>
        /// Get the current score for this game session.
        /// Used for high score tracking and UI display.
        /// </summary>
        int GetCurrentScore();
        
        /// <summary>
        /// Check if the game is currently playable.
        /// Used for UI state management and input handling.
        /// </summary>
        bool IsPlayable { get; }
    }
    
    /// <summary>
    /// Represents the different states a mini-game can be in.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Game is not yet initialized.
        /// </summary>
        Uninitialized,
        
        /// <summary>
        /// Game is initializing (loading assets, setting up systems).
        /// </summary>
        Initializing,
        
        /// <summary>
        /// Game is ready to start (initialization complete).
        /// </summary>
        Ready,
        
        /// <summary>
        /// Game is actively being played.
        /// </summary>
        Playing,
        
        /// <summary>
        /// Game is paused.
        /// </summary>
        Paused,
        
        /// <summary>
        /// Game has ended (win/lose condition met).
        /// </summary>
        GameOver,
        
        /// <summary>
        /// Game is being cleaned up.
        /// </summary>
        CleaningUp
    }
}