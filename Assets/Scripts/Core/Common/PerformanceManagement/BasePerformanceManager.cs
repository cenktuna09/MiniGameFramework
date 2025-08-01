using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Architecture;
using Core.Events;
namespace Core.Common.PerformanceManagement
{
    /// <summary>
    /// Base class for performance monitoring and optimization
    /// Provides FPS monitoring, memory tracking, and performance alerts
    /// </summary>
    public abstract class BasePerformanceManager
    {
        #region Protected Fields
        
        protected IEventBus _eventBus;
        protected bool _isMonitoring = false;
        protected float _frameTimeThreshold = 16.67f; // 60 FPS
        protected float _memoryThreshold = 100f; // 100MB
        protected float _lastFrameTime;
        protected float _averageFrameTime;
        protected int _frameCount;
        protected float _totalFrameTime;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Whether performance monitoring is active
        /// </summary>
        public bool IsMonitoring => _isMonitoring;
        
        /// <summary>
        /// Current FPS
        /// </summary>
        public float CurrentFPS => _lastFrameTime > 0 ? 1f / _lastFrameTime : 0f;
        
        /// <summary>
        /// Average FPS
        /// </summary>
        public float AverageFPS => _averageFrameTime > 0 ? 1f / _averageFrameTime : 0f;
        
        /// <summary>
        /// Current memory usage in MB
        /// </summary>
        public float CurrentMemoryUsage => GC.GetTotalMemory(false) / (1024f * 1024f);
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Event triggered when performance warning occurs
        /// </summary>
        public event Action<string> OnPerformanceWarning;
        
        /// <summary>
        /// Event triggered when performance alert occurs
        /// </summary>
        public event Action<string> OnPerformanceAlert;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Initialize performance manager with event bus
        /// </summary>
        /// <param name="eventBus">Event bus for performance notifications</param>
        protected BasePerformanceManager(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
        
        #endregion
        
        #region Abstract Methods
        
        /// <summary>
        /// Initialize performance monitoring
        /// Must be implemented by derived classes
        /// </summary>
        public abstract void Initialize();
        
        /// <summary>
        /// Start performance monitoring
        /// Must be implemented by derived classes
        /// </summary>
        public abstract void StartMonitoring();
        
        /// <summary>
        /// Stop performance monitoring
        /// Must be implemented by derived classes
        /// </summary>
        public abstract void StopMonitoring();
        
        /// <summary>
        /// Update performance monitoring
        /// Must be implemented by derived classes
        /// </summary>
        public abstract void UpdateMonitoring();
        
        /// <summary>
        /// Get performance statistics as string
        /// Must be implemented by derived classes
        /// </summary>
        /// <returns>Performance statistics string</returns>
        public abstract string GetPerformanceStats();
        
        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// Update frame time calculations
        /// </summary>
        protected virtual void UpdateFrameTime()
        {
            _lastFrameTime = Time.deltaTime;
            _totalFrameTime += _lastFrameTime;
            _frameCount++;
            
            if (_frameCount > 0)
            {
                _averageFrameTime = _totalFrameTime / _frameCount;
            }
        }
        
        /// <summary>
        /// Check for performance issues
        /// </summary>
        protected virtual void CheckPerformance()
        {
            // Check FPS
            if (_lastFrameTime > _frameTimeThreshold)
            {
                var warning = $"Low FPS detected: {CurrentFPS:F1} FPS";
                OnPerformanceWarning?.Invoke(warning);
                _eventBus?.Publish(new PerformanceWarningEvent(warning));
                
                Debug.LogWarning($"[{GetType().Name}] ⚠️ {warning}");
            }
            
            // Check memory usage
            var memoryUsage = CurrentMemoryUsage;
            if (memoryUsage > _memoryThreshold)
            {
                var alert = $"High memory usage: {memoryUsage:F1} MB";
                OnPerformanceAlert?.Invoke(alert);
                _eventBus?.Publish(new PerformanceAlertEvent(alert));
                
                Debug.LogError($"[{GetType().Name}] 🚨 {alert}");
            }
        }
        
        /// <summary>
        /// Reset performance counters
        /// </summary>
        protected virtual void ResetCounters()
        {
            _frameCount = 0;
            _totalFrameTime = 0f;
            _averageFrameTime = 0f;
            _lastFrameTime = 0f;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Set frame time threshold for performance warnings
        /// </summary>
        /// <param name="threshold">Frame time threshold in seconds</param>
        public void SetFrameTimeThreshold(float threshold)
        {
            _frameTimeThreshold = threshold;
            Debug.Log($"[{GetType().Name}] 📊 Frame time threshold set to: {threshold:F3}s");
        }
        
        /// <summary>
        /// Set memory threshold for performance alerts
        /// </summary>
        /// <param name="threshold">Memory threshold in MB</param>
        public void SetMemoryThreshold(float threshold)
        {
            _memoryThreshold = threshold;
            Debug.Log($"[{GetType().Name}] 📊 Memory threshold set to: {threshold:F1} MB");
        }
        
        /// <summary>
        /// Get detailed performance information
        /// </summary>
        /// <returns>Detailed performance info</returns>
        public virtual PerformanceInfo GetPerformanceInfo()
        {
            return new PerformanceInfo
            {
                CurrentFPS = CurrentFPS,
                AverageFPS = AverageFPS,
                CurrentMemoryUsage = CurrentMemoryUsage,
                FrameCount = _frameCount,
                IsMonitoring = _isMonitoring
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// Performance information structure
    /// </summary>
    public struct PerformanceInfo
    {
        public float CurrentFPS;
        public float AverageFPS;
        public float CurrentMemoryUsage;
        public int FrameCount;
        public bool IsMonitoring;
    }
    
    /// <summary>
    /// Event for performance warnings
    /// </summary>
    public class PerformanceWarningEvent : GameEvent
    {
        public string Warning { get; }
        
        public PerformanceWarningEvent(string warning)
        {
            Warning = warning;
        }
    }
    
    /// <summary>
    /// Event for performance alerts
    /// </summary>
    public class PerformanceAlertEvent : GameEvent
    {
        public string Alert { get; }
        
        public PerformanceAlertEvent(string alert)
        {
            Alert = alert;
        }
    }
} 