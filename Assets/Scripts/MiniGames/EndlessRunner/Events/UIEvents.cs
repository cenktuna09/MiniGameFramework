using UnityEngine;
using Core.Events;

namespace EndlessRunner.Events
{
    #region UI State Events
    public class UIPanelOpenedEvent : GameEvent
    {
        public string PanelName { get; private set; }
        public string PreviousPanel { get; private set; }
        public bool IsModal { get; private set; }
        public float OpenTime { get; private set; }
        
        public UIPanelOpenedEvent(string panelName, string previousPanel, bool isModal, float openTime)
        {
            PanelName = panelName;
            PreviousPanel = previousPanel;
            IsModal = isModal;
            OpenTime = openTime;
        }
    }
    
    public class UIPanelClosedEvent : GameEvent
    {
        public string PanelName { get; private set; }
        public string NextPanel { get; private set; }
        public float CloseTime { get; private set; }
        public string CloseReason { get; private set; }
        
        public UIPanelClosedEvent(string panelName, string nextPanel, float closeTime, string closeReason)
        {
            PanelName = panelName;
            NextPanel = nextPanel;
            CloseTime = closeTime;
            CloseReason = closeReason;
        }
    }
    
    public class UITransitionStartedEvent : GameEvent
    {
        public string FromPanel { get; private set; }
        public string ToPanel { get; private set; }
        public string TransitionType { get; private set; }
        public float TransitionDuration { get; private set; }
        
        public UITransitionStartedEvent(string fromPanel, string toPanel, string transitionType, float transitionDuration)
        {
            FromPanel = fromPanel;
            ToPanel = toPanel;
            TransitionType = transitionType;
            TransitionDuration = transitionDuration;
        }
    }
    
    public class UITransitionCompletedEvent : GameEvent
    {
        public string FromPanel { get; private set; }
        public string ToPanel { get; private set; }
        public string TransitionType { get; private set; }
        public float ActualDuration { get; private set; }
        
        public UITransitionCompletedEvent(string fromPanel, string toPanel, string transitionType, float actualDuration)
        {
            FromPanel = fromPanel;
            ToPanel = toPanel;
            TransitionType = transitionType;
            ActualDuration = actualDuration;
        }
    }
    #endregion
    
    #region Button Events
    public class UIButtonClickedEvent : GameEvent
    {
        public string ButtonName { get; private set; }
        public string ButtonAction { get; private set; }
        public Vector2 ClickPosition { get; private set; }
        public float ClickTime { get; private set; }
        
        public UIButtonClickedEvent(string buttonName, string buttonAction, Vector2 clickPosition, float clickTime)
        {
            ButtonName = buttonName;
            ButtonAction = buttonAction;
            ClickPosition = clickPosition;
            ClickTime = clickTime;
        }
    }
    
    public class UIButtonHoveredEvent : GameEvent
    {
        public string ButtonName { get; private set; }
        public bool IsHovered { get; private set; }
        public Vector2 HoverPosition { get; private set; }
        
        public UIButtonHoveredEvent(string buttonName, bool isHovered, Vector2 hoverPosition)
        {
            ButtonName = buttonName;
            IsHovered = isHovered;
            HoverPosition = hoverPosition;
        }
    }
    
    public class UIButtonPressedEvent : GameEvent
    {
        public string ButtonName { get; private set; }
        public string ButtonAction { get; private set; }
        public Vector2 PressPosition { get; private set; }
        public float PressDuration { get; private set; }
        
        public UIButtonPressedEvent(string buttonName, string buttonAction, Vector2 pressPosition, float pressDuration)
        {
            ButtonName = buttonName;
            ButtonAction = buttonAction;
            PressPosition = pressPosition;
            PressDuration = pressDuration;
        }
    }
    #endregion
    
    #region Display Update Events
    public class UIScoreUpdatedEvent : GameEvent
    {
        public int NewScore { get; private set; }
        public int PreviousScore { get; private set; }
        public int ScoreChange { get; private set; }
        public string DisplayFormat { get; private set; }
        
        public UIScoreUpdatedEvent(int newScore, int previousScore, int scoreChange, string displayFormat)
        {
            NewScore = newScore;
            PreviousScore = previousScore;
            ScoreChange = scoreChange;
            DisplayFormat = displayFormat;
        }
    }
    
    public class UIHighScoreUpdatedEvent : GameEvent
    {
        public int NewHighScore { get; private set; }
        public int PreviousHighScore { get; private set; }
        public bool IsNewRecord { get; private set; }
        public string DisplayFormat { get; private set; }
        
        public UIHighScoreUpdatedEvent(int newHighScore, int previousHighScore, bool isNewRecord, string displayFormat)
        {
            NewHighScore = newHighScore;
            PreviousHighScore = previousHighScore;
            IsNewRecord = isNewRecord;
            DisplayFormat = displayFormat;
        }
    }
    
    public class UIDistanceUpdatedEvent : GameEvent
    {
        public float NewDistance { get; private set; }
        public float PreviousDistance { get; private set; }
        public float DistanceChange { get; private set; }
        public string DistanceUnit { get; private set; }
        
        public UIDistanceUpdatedEvent(float newDistance, float previousDistance, float distanceChange, string distanceUnit)
        {
            NewDistance = newDistance;
            PreviousDistance = previousDistance;
            DistanceChange = distanceChange;
            DistanceUnit = distanceUnit;
        }
    }
    
    public class UISpeedUpdatedEvent : GameEvent
    {
        public float NewSpeed { get; private set; }
        public float PreviousSpeed { get; private set; }
        public float SpeedMultiplier { get; private set; }
        public string SpeedUnit { get; private set; }
        
        public UISpeedUpdatedEvent(float newSpeed, float previousSpeed, float speedMultiplier, string speedUnit)
        {
            NewSpeed = newSpeed;
            PreviousSpeed = previousSpeed;
            SpeedMultiplier = speedMultiplier;
            SpeedUnit = speedUnit;
        }
    }
    #endregion
    
    #region Game UI Events
    public class UIGameStartedEvent : GameEvent
    {
        public float StartTime { get; private set; }
        public string GameMode { get; private set; }
        public int InitialScore { get; private set; }
        
        public UIGameStartedEvent(float startTime, string gameMode, int initialScore)
        {
            StartTime = startTime;
            GameMode = gameMode;
            InitialScore = initialScore;
        }
    }
    
    public class UIGamePausedEvent : GameEvent
    {
        public float PauseTime { get; private set; }
        public string PauseReason { get; private set; }
        public bool ShowPauseMenu { get; private set; }
        
        public UIGamePausedEvent(float pauseTime, string pauseReason, bool showPauseMenu)
        {
            PauseTime = pauseTime;
            PauseReason = pauseReason;
            ShowPauseMenu = showPauseMenu;
        }
    }
    
    public class UIGameResumedEvent : GameEvent
    {
        public float ResumeTime { get; private set; }
        public float PauseDuration { get; private set; }
        public string ResumeReason { get; private set; }
        
        public UIGameResumedEvent(float resumeTime, float pauseDuration, string resumeReason)
        {
            ResumeTime = resumeTime;
            PauseDuration = pauseDuration;
            ResumeReason = resumeReason;
        }
    }
    
    public class UIGameOverEvent : GameEvent
    {
        public float EndTime { get; private set; }
        public int FinalScore { get; private set; }
        public float FinalDistance { get; private set; }
        public string EndReason { get; private set; }
        public bool IsNewHighScore { get; private set; }
        
        public UIGameOverEvent(float endTime, int finalScore, float finalDistance, string endReason, bool isNewHighScore)
        {
            EndTime = endTime;
            FinalScore = finalScore;
            FinalDistance = finalDistance;
            EndReason = endReason;
            IsNewHighScore = isNewHighScore;
        }
    }
    #endregion
    
    #region Notification Events
    public class UINotificationShownEvent : GameEvent
    {
        public string NotificationType { get; private set; }
        public string Message { get; private set; }
        public float Duration { get; private set; }
        public Vector2 Position { get; private set; }
        
        public UINotificationShownEvent(string notificationType, string message, float duration, Vector2 position)
        {
            NotificationType = notificationType;
            Message = message;
            Duration = duration;
            Position = position;
        }
    }
    
    public class UINotificationHiddenEvent : GameEvent
    {
        public string NotificationType { get; private set; }
        public string Message { get; private set; }
        public float DisplayTime { get; private set; }
        public string HideReason { get; private set; }
        
        public UINotificationHiddenEvent(string notificationType, string message, float displayTime, string hideReason)
        {
            NotificationType = notificationType;
            Message = message;
            DisplayTime = displayTime;
            HideReason = hideReason;
        }
    }
    
    public class UITutorialStepEvent : GameEvent
    {
        public int StepNumber { get; private set; }
        public string StepTitle { get; private set; }
        public string StepDescription { get; private set; }
        public bool IsCompleted { get; private set; }
        
        public UITutorialStepEvent(int stepNumber, string stepTitle, string stepDescription, bool isCompleted)
        {
            StepNumber = stepNumber;
            StepTitle = stepTitle;
            StepDescription = stepDescription;
            IsCompleted = isCompleted;
        }
    }
    #endregion
    
    #region Settings Events
    public class UISettingsChangedEvent : GameEvent
    {
        public string SettingName { get; private set; }
        public object NewValue { get; private set; }
        public object PreviousValue { get; private set; }
        public string SettingCategory { get; private set; }
        
        public UISettingsChangedEvent(string settingName, object newValue, object previousValue, string settingCategory)
        {
            SettingName = settingName;
            NewValue = newValue;
            PreviousValue = previousValue;
            SettingCategory = settingCategory;
        }
    }
    
    public class UIVolumeChangedEvent : GameEvent
    {
        public string AudioType { get; private set; }
        public float NewVolume { get; private set; }
        public float PreviousVolume { get; private set; }
        public bool IsMuted { get; private set; }
        
        public UIVolumeChangedEvent(string audioType, float newVolume, float previousVolume, bool isMuted)
        {
            AudioType = audioType;
            NewVolume = newVolume;
            PreviousVolume = previousVolume;
            IsMuted = isMuted;
        }
    }
    #endregion
} 