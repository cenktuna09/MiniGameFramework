using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Common.ErrorHandling
{
    /// <summary>
    /// Base class for error handling and validation
    /// Provides safe execution patterns and validation methods
    /// </summary>
    public abstract class BaseErrorHandler
    {
        #region Protected Fields
        
        protected IEventBus _eventBus;
        protected bool _enableLogging = true;
        protected bool _enableValidation = true;
        protected int _maxErrorCount = 10;
        protected int _currentErrorCount = 0;
        protected List<string> _errorHistory;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Whether error logging is enabled
        /// </summary>
        public bool EnableLogging => _enableLogging;
        
        /// <summary>
        /// Whether validation is enabled
        /// </summary>
        public bool EnableValidation => _enableValidation;
        
        /// <summary>
        /// Current error count
        /// </summary>
        public int CurrentErrorCount => _currentErrorCount;
        
        /// <summary>
        /// Maximum allowed error count
        /// </summary>
        public int MaxErrorCount => _maxErrorCount;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Initialize error handler with event bus
        /// </summary>
        /// <param name="eventBus">Event bus for error notifications</param>
        protected BaseErrorHandler(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _errorHistory = new List<string>();
        }
        
        #endregion
        
        #region Abstract Methods
        
        /// <summary>
        /// Execute action safely with error handling
        /// Must be implemented by derived classes
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="operationName">Name of the operation for logging</param>
        public abstract void SafeExecute(Action action, string operationName);
        
        /// <summary>
        /// Validate animation parameters
        /// Must be implemented by derived classes
        /// </summary>
        /// <param name="animationName">Name of the animation</param>
        /// <param name="target">Target GameObject</param>
        /// <param name="duration">Animation duration</param>
        public abstract void ValidateAnimation(string animationName, GameObject target, float duration);
        
        /// <summary>
        /// Validate configuration settings
        /// Must be implemented by derived classes
        /// </summary>
        /// <param name="config">Configuration to validate</param>
        public abstract void ValidateConfiguration(BaseGameConfig config);
        
        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// Log error message
        /// </summary>
        /// <param name="message">Error message</param>
        protected virtual void LogError(string message)
        {
            if (!_enableLogging) return;
            
            _currentErrorCount++;
            _errorHistory.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            
            // Keep only recent errors
            if (_errorHistory.Count > _maxErrorCount)
            {
                _errorHistory.RemoveAt(0);
            }
            
            Debug.LogError($"[{GetType().Name}] ❌ {message}");
            
            // Publish error event
            _eventBus?.Publish(new ErrorOccurredEvent(message));
        }
        
        /// <summary>
        /// Log warning message
        /// </summary>
        /// <param name="message">Warning message</param>
        protected virtual void LogWarning(string message)
        {
            if (!_enableLogging) return;
            
            Debug.LogWarning($"[{GetType().Name}] ⚠️ {message}");
            
            // Publish warning event
            _eventBus?.Publish(new WarningOccurredEvent(message));
        }
        
        /// <summary>
        /// Log info message
        /// </summary>
        /// <param name="message">Info message</param>
        protected virtual void LogInfo(string message)
        {
            if (!_enableLogging) return;
            
            Debug.Log($"[{GetType().Name}] ℹ️ {message}");
        }
        
        /// <summary>
        /// Check if error count exceeds maximum
        /// </summary>
        /// <returns>True if error count is too high</returns>
        protected virtual bool IsErrorCountExceeded()
        {
            return _currentErrorCount >= _maxErrorCount;
        }
        
        /// <summary>
        /// Reset error count
        /// </summary>
        protected virtual void ResetErrorCount()
        {
            _currentErrorCount = 0;
            _errorHistory.Clear();
            LogInfo("Error count reset");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Set error logging enabled/disabled
        /// </summary>
        /// <param name="enabled">Whether logging is enabled</param>
        public void SetLoggingEnabled(bool enabled)
        {
            _enableLogging = enabled;
            LogInfo($"Logging {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Set validation enabled/disabled
        /// </summary>
        /// <param name="enabled">Whether validation is enabled</param>
        public void SetValidationEnabled(bool enabled)
        {
            _enableValidation = enabled;
            LogInfo($"Validation {(enabled ? "enabled" : "disabled")}");
        }
        
        /// <summary>
        /// Set maximum error count
        /// </summary>
        /// <param name="maxCount">Maximum error count</param>
        public void SetMaxErrorCount(int maxCount)
        {
            _maxErrorCount = maxCount;
            LogInfo($"Max error count set to: {maxCount}");
        }
        
        /// <summary>
        /// Get error history
        /// </summary>
        /// <returns>List of recent errors</returns>
        public List<string> GetErrorHistory()
        {
            return new List<string>(_errorHistory);
        }
        
        /// <summary>
        /// Clear error history
        /// </summary>
        public void ClearErrorHistory()
        {
            _errorHistory.Clear();
            LogInfo("Error history cleared");
        }
        
        /// <summary>
        /// Get error statistics
        /// </summary>
        /// <returns>Error statistics string</returns>
        public virtual string GetErrorStats()
        {
            return $"Errors: {_currentErrorCount}/{_maxErrorCount}, History: {_errorHistory.Count}";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Event for error occurrences
    /// </summary>
    public class ErrorOccurredEvent : GameEvent
    {
        public string ErrorMessage { get; }
        
        public ErrorOccurredEvent(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
    
    /// <summary>
    /// Event for warning occurrences
    /// </summary>
    public class WarningOccurredEvent : GameEvent
    {
        public string WarningMessage { get; }
        
        public WarningOccurredEvent(string warningMessage)
        {
            WarningMessage = warningMessage;
        }
    }
} 