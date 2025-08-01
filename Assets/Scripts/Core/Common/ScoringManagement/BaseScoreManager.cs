using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Architecture;
using Core.Events;
namespace Core.Common.ScoringManagement
{
    /// <summary>
    /// Base class for score management across all mini-games
    /// Provides common scoring functionality, high score management, and event publishing
    /// </summary>
    public abstract class BaseScoreManager
    {
        #region Protected Fields
        
        protected IEventBus _eventBus;
        protected int _currentScore = 0;
        protected int _highScore = 0;
        protected int _scoreMultiplier = 1;
        protected List<int> _scoreHistory;
        protected int _maxHistoryCount = 10;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Current score
        /// </summary>
        public int CurrentScore => _currentScore;
        
        /// <summary>
        /// High score
        /// </summary>
        public int HighScore => _highScore;
        
        /// <summary>
        /// Current score multiplier
        /// </summary>
        public int ScoreMultiplier => _scoreMultiplier;
        
        /// <summary>
        /// Number of scores in history
        /// </summary>
        public int ScoreHistoryCount => _scoreHistory?.Count ?? 0;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Event triggered when score changes
        /// </summary>
        public event Action<int, int> OnScoreChanged;
        
        /// <summary>
        /// Event triggered when high score is achieved
        /// </summary>
        public event Action<int> OnHighScoreAchieved;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Initialize score manager with event bus
        /// </summary>
        /// <param name="eventBus">Event bus for score notifications</param>
        protected BaseScoreManager(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _scoreHistory = new List<int>();
            Initialize();
        }
        
        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// Initialize score manager
        /// </summary>
        protected virtual void Initialize()
        {
            LoadHighScore();
            
            Debug.Log($"[{GetType().Name}] ✅ Score manager initialized");
        }
        
        /// <summary>
        /// Load high score from persistent storage
        /// Must be implemented by derived classes
        /// </summary>
        protected abstract void LoadHighScore();
        
        /// <summary>
        /// Save high score to persistent storage
        /// Must be implemented by derived classes
        /// </summary>
        protected abstract void SaveHighScore();
        
        /// <summary>
        /// Calculate score based on game-specific logic
        /// Must be implemented by derived classes
        /// </summary>
        /// <param name="basePoints">Base points to add</param>
        /// <param name="multiplier">Score multiplier</param>
        /// <returns>Calculated score</returns>
        protected abstract int CalculateScore(int basePoints, int multiplier = 1);
        
        /// <summary>
        /// Validate score value
        /// </summary>
        /// <param name="score">Score to validate</param>
        /// <returns>True if score is valid</returns>
        protected virtual bool ValidateScore(int score)
        {
            return score >= 0;
        }
        
        /// <summary>
        /// Add score to history
        /// </summary>
        /// <param name="score">Score to add</param>
        protected virtual void AddToHistory(int score)
        {
            if (_scoreHistory.Count >= _maxHistoryCount)
            {
                _scoreHistory.RemoveAt(0);
            }
            
            _scoreHistory.Add(score);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Add points to current score
        /// </summary>
        /// <param name="points">Points to add</param>
        public virtual void AddScore(int points)
        {
            if (!ValidateScore(points))
            {
                Debug.LogWarning($"[{GetType().Name}] ⚠️ Invalid points: {points}");
                return;
            }
            
            var calculatedPoints = CalculateScore(points, _scoreMultiplier);
            var oldScore = _currentScore;
            _currentScore += calculatedPoints;
            
            // Publish score changed event
            OnScoreChanged?.Invoke(_currentScore, calculatedPoints);
            _eventBus?.Publish(new ScoreChangedEvent(_currentScore, calculatedPoints));
            
            Debug.Log($"[{GetType().Name}] 📊 Score updated: {_currentScore} (+{calculatedPoints})");
        }
        
        /// <summary>
        /// Set current score
        /// </summary>
        /// <param name="score">New score value</param>
        public virtual void SetScore(int score)
        {
            if (!ValidateScore(score))
            {
                Debug.LogWarning($"[{GetType().Name}] ⚠️ Invalid score: {score}");
                return;
            }
            
            var oldScore = _currentScore;
            _currentScore = score;
            
            // Publish score changed event
            OnScoreChanged?.Invoke(_currentScore, _currentScore - oldScore);
            _eventBus?.Publish(new ScoreChangedEvent(_currentScore, _currentScore - oldScore));
            
            Debug.Log($"[{GetType().Name}] 📊 Score set to: {_currentScore}");
        }
        
        /// <summary>
        /// Reset current score to zero
        /// </summary>
        public virtual void ResetScore()
        {
            var oldScore = _currentScore;
            _currentScore = 0;
            
            // Publish score changed event
            OnScoreChanged?.Invoke(_currentScore, -oldScore);
            _eventBus?.Publish(new ScoreChangedEvent(_currentScore, -oldScore));
            
            Debug.Log($"[{GetType().Name}] 🔄 Score reset to: {_currentScore}");
        }
        
        /// <summary>
        /// Set score multiplier
        /// </summary>
        /// <param name="multiplier">New multiplier value</param>
        public virtual void SetScoreMultiplier(int multiplier)
        {
            if (multiplier < 1)
            {
                Debug.LogWarning($"[{GetType().Name}] ⚠️ Invalid multiplier: {multiplier}");
                return;
            }
            
            _scoreMultiplier = multiplier;
            Debug.Log($"[{GetType().Name}] 📈 Score multiplier set to: {_scoreMultiplier}x");
        }
        
        /// <summary>
        /// Check if current score is a new high score
        /// </summary>
        /// <returns>True if current score is higher than high score</returns>
        public virtual bool IsNewHighScore()
        {
            return _currentScore > _highScore;
        }
        
        /// <summary>
        /// Update high score if current score is higher
        /// </summary>
        public virtual void UpdateHighScore()
        {
            if (IsNewHighScore())
            {
                _highScore = _currentScore;
                SaveHighScore();
                
                // Publish high score event
                OnHighScoreAchieved?.Invoke(_highScore);
                _eventBus?.Publish(new HighScoreEvent(_highScore));
                
                Debug.Log($"[{GetType().Name}] 🏆 New high score: {_highScore}");
            }
        }
        
        /// <summary>
        /// End game and finalize score
        /// </summary>
        public virtual void EndGame()
        {
            // Add current score to history
            AddToHistory(_currentScore);
            
            // Update high score
            UpdateHighScore();
            
            Debug.Log($"[{GetType().Name}] 🏁 Game ended with score: {_currentScore}");
        }
        
        /// <summary>
        /// Get score history
        /// </summary>
        /// <returns>List of recent scores</returns>
        public List<int> GetScoreHistory()
        {
            return new List<int>(_scoreHistory);
        }
        
        /// <summary>
        /// Clear score history
        /// </summary>
        public virtual void ClearScoreHistory()
        {
            _scoreHistory.Clear();
            Debug.Log($"[{GetType().Name}] 🗑️ Score history cleared");
        }
        
        /// <summary>
        /// Get score statistics
        /// </summary>
        /// <returns>Score statistics string</returns>
        public virtual string GetScoreStats()
        {
            return $"Current: {_currentScore}, High: {_highScore}, Multiplier: {_scoreMultiplier}x, History: {_scoreHistory.Count}";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Event for score changes
    /// </summary>
    public class ScoreChangedEvent : GameEvent
    {
        public int NewScore { get; }
        public int ScoreDelta { get; }
        
        public ScoreChangedEvent(int newScore, int scoreDelta)
        {
            NewScore = newScore;
            ScoreDelta = scoreDelta;
        }
    }
    
    /// <summary>
    /// Event for high score achievements
    /// </summary>
    public class HighScoreEvent : GameEvent
    {
        public int HighScore { get; }
        
        public HighScoreEvent(int highScore)
        {
            HighScore = highScore;
        }
    }
} 