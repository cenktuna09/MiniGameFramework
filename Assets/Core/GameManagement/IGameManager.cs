using System;
using System.Threading.Tasks;
using MiniGameFramework.Core.Architecture;

namespace MiniGameFramework.Core.GameManagement
{
    /// <summary>
    /// Central game manager interface responsible for mini-game lifecycle and flow control.
    /// Manages session tracking, scoring, and coordination between different mini-games.
    /// </summary>
    public interface IGameManager
    {
        /// <summary>Current active mini-game instance</summary>
        IMiniGame CurrentGame { get; }
        
        /// <summary>Current game session data</summary>
        IGameSession CurrentSession { get; }
        
        /// <summary>Is any game currently active</summary>
        bool IsGameActive { get; }
        
        /// <summary>Global game manager state</summary>
        GameManagerState State { get; }
        
        /// <summary>
        /// Event fired when game manager state changes.
        /// </summary>
        event Action<GameManagerState> OnStateChanged;
        
        /// <summary>
        /// Event fired when a mini-game is loaded.
        /// </summary>
        event Action<IMiniGame> OnGameLoaded;
        
        /// <summary>
        /// Event fired when a mini-game is unloaded.
        /// </summary>
        event Action<string> OnGameUnloaded;
        
        /// <summary>
        /// Load and start a mini-game by ID.
        /// </summary>
        /// <param name="gameId">Unique identifier of the mini-game</param>
        /// <returns>Task that completes when game is loaded and ready</returns>
        Task<bool> LoadGameAsync(string gameId);
        
        /// <summary>
        /// Unload the current mini-game and clean up resources.
        /// </summary>
        /// <returns>Task that completes when game is unloaded</returns>
        Task<bool> UnloadCurrentGameAsync();
        
        /// <summary>
        /// Start the currently loaded mini-game.
        /// </summary>
        /// <returns>True if game started successfully</returns>
        bool StartCurrentGame();
        
        /// <summary>
        /// Pause the current mini-game.
        /// </summary>
        /// <returns>True if game paused successfully</returns>
        bool PauseCurrentGame();
        
        /// <summary>
        /// Resume the current mini-game.
        /// </summary>
        /// <returns>True if game resumed successfully</returns>
        bool ResumeCurrentGame();
        
        /// <summary>
        /// End the current mini-game and save results.
        /// </summary>
        /// <param name="gameResult">Final game result data</param>
        /// <returns>True if game ended successfully</returns>
        bool EndCurrentGame(GameResult gameResult);
        
        /// <summary>
        /// Get the current score for the active game.
        /// </summary>
        /// <returns>Current score, or 0 if no game active</returns>
        int GetCurrentScore();
        
        /// <summary>
        /// Get the best score for a specific game type.
        /// </summary>
        /// <param name="gameId">Game identifier</param>
        /// <returns>Best score for the game type</returns>
        int GetBestScore(string gameId);
        
        /// <summary>
        /// Get elapsed time for current game session.
        /// </summary>
        /// <returns>Time elapsed since game start</returns>
        TimeSpan GetSessionTime();
        
        /// <summary>
        /// Reset all game data and return to initial state.
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Register a mini-game type for loading.
        /// </summary>
        /// <typeparam name="T">Mini-game type</typeparam>
        /// <param name="gameId">Unique game identifier</param>
        void RegisterGame<T>(string gameId) where T : IMiniGame;
    }
    
    /// <summary>
    /// Represents the current state of the game manager.
    /// </summary>
    public enum GameManagerState
    {
        /// <summary>Game manager is idle, no game loaded</summary>
        Idle,
        
        /// <summary>Loading a mini-game</summary>
        Loading,
        
        /// <summary>Game is loaded and ready to start</summary>
        Ready,
        
        /// <summary>Game is actively running</summary>
        Playing,
        
        /// <summary>Game is paused</summary>
        Paused,
        
        /// <summary>Game has ended, showing results</summary>
        GameOver,
        
        /// <summary>Unloading current game</summary>
        Unloading
    }
    
    /// <summary>
    /// Represents the final result of a completed game.
    /// </summary>
    public enum GameResult
    {
        /// <summary>Player completed successfully</summary>
        Victory,
        
        /// <summary>Player failed to complete</summary>
        Defeat,
        
        /// <summary>Player quit the game early</summary>
        Quit,
        
        /// <summary>Game ended due to time limit</summary>
        TimeOut
    }
} 