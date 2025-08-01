using UnityEngine;
using Core.Architecture;
using Core.Common.PerformanceManagement;
using Core.Events;

namespace EndlessRunner.Performance
{
    /// <summary>
    /// Performance manager for Endless Runner game
    /// Monitors FPS, memory usage, and game-specific performance metrics
    /// </summary>
    public class RunnerPerformanceManager : BasePerformanceManager
    {
        #region Private Fields
        private float _memoryUsageMB = 0f;
        private float _cpuUsagePercent = 0f;
        #endregion
        
        #region Constructor
        public RunnerPerformanceManager(IEventBus eventBus) : base(eventBus)
        {
            Debug.Log("[RunnerPerformanceManager] ✅ Runner performance manager initialized");
        }
        #endregion
        
        #region Abstract Method Implementations
        
        /// <summary>
        /// Initialize performance monitoring
        /// </summary>
        public override void Initialize()
        {
            // Set runner-specific thresholds
            SetFrameTimeThreshold(1f / 30f); // Minimum 30 FPS for runner
            SetMemoryThreshold(200f); // 200MB memory limit
            
            Debug.Log("[RunnerPerformanceManager] 🎯 Performance thresholds set for runner");
        }
        
        /// <summary>
        /// Start performance monitoring
        /// </summary>
        public override void StartMonitoring()
        {
            _isMonitoring = true;
            ResetCounters();
            Debug.Log("[RunnerPerformanceManager] 📊 Performance monitoring started");
        }
        
        /// <summary>
        /// Stop performance monitoring
        /// </summary>
        public override void StopMonitoring()
        {
            _isMonitoring = false;
            Debug.Log("[RunnerPerformanceManager] 📊 Performance monitoring stopped");
        }
        
        /// <summary>
        /// Update performance monitoring
        /// </summary>
        public override void UpdateMonitoring()
        {
            if (!_isMonitoring) return;
            
            // Update frame time using base class method
            UpdateFrameTime();
            
            // Update memory usage
            _memoryUsageMB = CurrentMemoryUsage;
            
            // Update CPU usage (simplified)
            _cpuUsagePercent = Mathf.Clamp01(Time.deltaTime / (1f / 60f)) * 100f;
            
            // Check for performance issues using base class method
            CheckPerformance();
        }
        
        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// Check for performance warnings and publish events
        /// </summary>
        protected override void CheckPerformance()
        {
            // Call base implementation first
            base.CheckPerformance();
            
            // Add runner-specific performance checks
            float currentFPS = CurrentFPS;
            float currentMemory = CurrentMemoryUsage;
            
            // FPS warning
            if (currentFPS < 30f)
            {
                var warningEvent = new PerformanceWarningEvent($"Low FPS: {currentFPS:F1}");
                _eventBus?.Publish(warningEvent);
                
                Debug.LogWarning($"[RunnerPerformanceManager] ⚠️ Low FPS detected: {currentFPS:F1}");
            }
            
            // Memory warning
            if (currentMemory > 200f)
            {
                var warningEvent = new PerformanceWarningEvent($"High Memory: {currentMemory:F1}MB");
                _eventBus?.Publish(warningEvent);
                
                Debug.LogWarning($"[RunnerPerformanceManager] ⚠️ High memory usage: {currentMemory:F1}MB");
            }
        }
        
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Get detailed performance stats for runner
        /// </summary>
        public override string GetPerformanceStats()
        {
            return $"Runner Performance Stats:\n" +
                   $"FPS: {CurrentFPS:F1} (Target: 30.0)\n" +
                   $"Memory: {CurrentMemoryUsage:F1}MB (Limit: 200.0MB)\n" +
                   $"CPU: {_cpuUsagePercent:F1}%\n" +
                   $"Frame Time: {_lastFrameTime * 1000f:F1}ms\n" +
                   $"Frame Count: {_frameCount}";
        }
        
        /// <summary>
        /// Get runner-specific performance metrics
        /// </summary>
        public RunnerPerformanceMetrics GetRunnerMetrics()
        {
            return new RunnerPerformanceMetrics
            {
                CurrentFPS = CurrentFPS,
                AverageFrameTime = _averageFrameTime,
                MemoryUsageMB = CurrentMemoryUsage,
                CPUUsagePercent = _cpuUsagePercent,
                FrameCount = _frameCount,
                IsPerformanceGood = CurrentFPS >= 30f && CurrentMemoryUsage <= 200f
            };
        }
        #endregion
    }
    
    /// <summary>
    /// Performance metrics specific to the runner game
    /// </summary>
    public struct RunnerPerformanceMetrics
    {
        public float CurrentFPS;
        public float AverageFrameTime;
        public float MemoryUsageMB;
        public float CPUUsagePercent;
        public int FrameCount;
        public bool IsPerformanceGood;
    }
} 