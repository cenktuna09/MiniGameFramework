using System;
using UnityEngine;

namespace MiniGameFramework.Core.GameManagement
{
    /// <summary>
    /// Implementation of game session tracking with score and timing management.
    /// Tracks individual game sessions and provides statistics.
    /// </summary>
    public class GameSession : IGameSession
    {
        private int currentScore;
        private int bestScore;
        private bool isActive;
        private GameResult? result;

        /// <summary>Unique session identifier</summary>
        public string SessionId { get; private set; }
        
        /// <summary>Game type identifier</summary>
        public string GameId { get; private set; }
        
        /// <summary>When the session started</summary>
        public DateTime StartTime { get; private set; }
        
        /// <summary>When the session ended (null if still active)</summary>
        public DateTime? EndTime { get; private set; }
        
        /// <summary>Current session score</summary>
        public int Score => currentScore;
        
        /// <summary>Best score achieved in this session</summary>
        public int BestScore => bestScore;
        
        /// <summary>Total time elapsed in session</summary>
        public TimeSpan ElapsedTime => 
            EndTime.HasValue ? EndTime.Value - StartTime : DateTime.Now - StartTime;
        
        /// <summary>Is session currently active</summary>
        public bool IsActive => isActive;
        
        /// <summary>Session result (null if still active)</summary>
        public GameResult? Result => result;

        /// <summary>
        /// Initialize a new game session.
        /// </summary>
        /// <param name="gameId">Game type identifier</param>
        /// <param name="initialBestScore">Previous best score for this game type</param>
        public GameSession(string gameId, int initialBestScore = 0)
        {
            SessionId = Guid.NewGuid().ToString();
            GameId = gameId ?? throw new ArgumentNullException(nameof(gameId));
            StartTime = DateTime.Now;
            currentScore = 0;
            bestScore = initialBestScore;
            isActive = true;
            result = null;
            EndTime = null;
            
            Debug.Log($"[GameSession] Started new session: {SessionId} for game: {GameId}");
        }

        /// <summary>
        /// Update the current score.
        /// </summary>
        /// <param name="newScore">New score value</param>
        public void UpdateScore(int newScore)
        {
            if (!isActive)
            {
                Debug.LogWarning($"[GameSession] Cannot update score on inactive session: {SessionId}");
                return;
            }

            int oldScore = currentScore;
            currentScore = Math.Max(0, newScore); // Ensure score doesn't go negative
            
            // Update best score if necessary
            if (currentScore > bestScore)
            {
                bestScore = currentScore;
                Debug.Log($"[GameSession] New best score achieved: {bestScore}");
            }
            
            Debug.Log($"[GameSession] Score updated: {oldScore} -> {currentScore}");
        }

        /// <summary>
        /// Add points to the current score.
        /// </summary>
        /// <param name="points">Points to add</param>
        public void AddScore(int points)
        {
            UpdateScore(currentScore + points);
        }

        /// <summary>
        /// End the session with a result.
        /// </summary>
        /// <param name="gameResult">Final game result</param>
        public void EndSession(GameResult gameResult)
        {
            if (!isActive)
            {
                Debug.LogWarning($"[GameSession] Session already ended: {SessionId}");
                return;
            }

            isActive = false;
            result = gameResult;
            EndTime = DateTime.Now;
            
            Debug.Log($"[GameSession] Session ended: {SessionId}, Result: {gameResult}, Final Score: {currentScore}, Duration: {ElapsedTime:mm\\:ss}");
        }

        /// <summary>
        /// Get session statistics.
        /// </summary>
        /// <returns>Session stats data</returns>
        public GameSessionStats GetStats()
        {
            return new GameSessionStats
            {
                SessionId = SessionId,
                GameId = GameId,
                StartTime = StartTime,
                EndTime = EndTime,
                Duration = ElapsedTime,
                FinalScore = currentScore,
                BestScore = bestScore,
                Result = result,
                IsCompleted = !isActive
            };
        }
    }
} 