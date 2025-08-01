using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Event published when any mini-game ends (win/lose/quit).
    /// Provides consistent game over notifications across all mini-games.
    /// </summary>
    public class OnGameOverEvent : GameEvent
    {
        #region Properties
        
        /// <summary>
        /// ID of the game that ended
        /// </summary>
        public string GameId { get; private set; }
        
        /// <summary>
        /// Final score achieved in the game
        /// </summary>
        public int FinalScore { get; private set; }
        
        /// <summary>
        /// Reason for game over (e.g., "PlayerDeath", "TimeUp", "Win")
        /// </summary>
        public string GameOverReason { get; private set; }
        
        /// <summary>
        /// Duration of the game session in seconds
        /// </summary>
        public float GameDuration { get; private set; }
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Creates a new OnGameOverEvent
        /// </summary>
        /// <param name="gameId">ID of the game that ended</param>
        /// <param name="finalScore">Final score achieved</param>
        /// <param name="gameOverReason">Reason for game over</param>
        /// <param name="gameDuration">Duration of the game session</param>
        /// <param name="source">Source GameObject that created this event</param>
        public OnGameOverEvent(string gameId, int finalScore = 0, string gameOverReason = "Unknown", float gameDuration = 0f, GameObject source = null) 
            : base(source)
        {
            GameId = gameId;
            FinalScore = finalScore;
            GameOverReason = gameOverReason;
            GameDuration = gameDuration;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Get detailed event information for logging
        /// </summary>
        public override string GetEventDetails()
        {
            return $"{EventType} - Game: {GameId}, Score: {FinalScore}, Reason: {GameOverReason}, Duration: {GameDuration:F2}s, Source: {(Source != null ? Source.name : "None")}";
        }
        
        #endregion
    }
} 