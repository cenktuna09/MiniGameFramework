using UnityEngine;
using Core.Events;

namespace EndlessRunner.Events
{
    #region Game State Events
    public class GameStartedEvent : GameEvent
    {
        public float StartTime { get; private set; }
        
        public GameStartedEvent(float startTime)
        {
            StartTime = startTime;
        }
    }
    
    public class GameEndedEvent : GameEvent
    {
        public float EndTime { get; private set; }
        public float GameDuration { get; private set; }
        public int FinalScore { get; private set; }
        public string EndReason { get; private set; }
        
        public GameEndedEvent(float endTime, float gameDuration, int finalScore, string endReason)
        {
            EndTime = endTime;
            GameDuration = gameDuration;
            FinalScore = finalScore;
            EndReason = endReason;
        }
    }
    
    public class GamePausedEvent : GameEvent
    {
        public float PauseTime { get; private set; }
        
        public GamePausedEvent(float pauseTime)
        {
            PauseTime = pauseTime;
        }
    }
    
    public class GameResumedEvent : GameEvent
    {
        public float ResumeTime { get; private set; }
        
        public GameResumedEvent(float resumeTime)
        {
            ResumeTime = resumeTime;
        }
    }
    #endregion
    
    #region Scoring Events
    public class ScoreChangedEvent : GameEvent
    {
        public int NewScore { get; private set; }
        public int ScoreChange { get; private set; }
        public int ScoreMultiplier { get; private set; }
        
        public ScoreChangedEvent(int newScore, int scoreChange, int scoreMultiplier)
        {
            NewScore = newScore;
            ScoreChange = scoreChange;
            ScoreMultiplier = scoreMultiplier;
        }
    }
    
    public class HighScoreAchievedEvent : GameEvent
    {
        public int NewHighScore { get; private set; }
        public int PreviousHighScore { get; private set; }
        
        public HighScoreAchievedEvent(int newHighScore, int previousHighScore)
        {
            NewHighScore = newHighScore;
            PreviousHighScore = previousHighScore;
        }
    }
    
    public class ScoreMultiplierChangedEvent : GameEvent
    {
        public int NewMultiplier { get; private set; }
        public int PreviousMultiplier { get; private set; }
        
        public ScoreMultiplierChangedEvent(int newMultiplier, int previousMultiplier)
        {
            NewMultiplier = newMultiplier;
            PreviousMultiplier = previousMultiplier;
        }
    }
    #endregion
    
    #region Performance Events
    public class PerformanceWarningEvent : GameEvent
    {
        public string WarningType { get; private set; }
        public float CurrentValue { get; private set; }
        public float ThresholdValue { get; private set; }
        
        public PerformanceWarningEvent(string warningType, float currentValue, float thresholdValue)
        {
            WarningType = warningType;
            CurrentValue = currentValue;
            ThresholdValue = thresholdValue;
        }
    }
    
    public class PerformanceAlertEvent : GameEvent
    {
        public string AlertType { get; private set; }
        public float CurrentValue { get; private set; }
        public float CriticalValue { get; private set; }
        
        public PerformanceAlertEvent(string alertType, float currentValue, float criticalValue)
        {
            AlertType = alertType;
            CurrentValue = currentValue;
            CriticalValue = criticalValue;
        }
    }
    #endregion
    
    #region Error Events
    public class GameErrorEvent : GameEvent
    {
        public string ErrorMessage { get; private set; }
        public string ErrorType { get; private set; }
        public System.Exception Exception { get; private set; }
        
        public GameErrorEvent(string errorMessage, string errorType, System.Exception exception = null)
        {
            ErrorMessage = errorMessage;
            ErrorType = errorType;
            Exception = exception;
        }
    }
    
    public class GameWarningEvent : GameEvent
    {
        public string WarningMessage { get; private set; }
        public string WarningType { get; private set; }
        
        public GameWarningEvent(string warningMessage, string warningType)
        {
            WarningMessage = warningMessage;
            WarningType = warningType;
        }
    }
    #endregion
} 