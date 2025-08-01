using UnityEngine;
using Core.Common.PerformanceManagement;
using Core.Events;
using EndlessRunner.Events;

namespace EndlessRunner.Performance
{
    /// <summary>
    /// Performance manager for Endless Runner game
    /// Monitors FPS, memory usage, and game-specific performance metrics
    /// </summary>
    public class RunnerPerformanceManager : BasePerformanceManager
    {
        #region Private Fields
        private float _lastFrameTime = 0f;
        private float _averageFrameTime = 0f;
        private int _frameCount = 0;
        private float _memoryUsageMB = 0f;
        private float _cpuUsagePercent = 0f;
        #endregion
        
        #region Constructor
        public RunnerPerformanceManager(IEventBus eventBus) : base(eventBus)
        {
            Debug.Log("[RunnerPerformanceManager] ✅ Runner performance manager initialized");
        }
        #endregion
        
        #region Protected Methods
        /// <summary>
        /// Initialize performance monitoring
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            
            // Set runner-specific thresholds
            SetFPSThreshold(30f); // Minimum 30 FPS for runner
            SetMemoryThreshold(200f); // 200MB memory limit
            SetCPUThreshold(80f); // 80% CPU usage limit
            
            Debug.Log("[RunnerPerformanceManager] 🎯 Performance thresholds set for runner");
        }
        
        /// <summary>
        /// Update performance metrics
        /// </summary>
        protected override void UpdatePerformanceMetrics()
        {
            // Update frame time
            float currentFrameTime = Time.unscaledDeltaTime;
            _averageFrameTime = Mathf.Lerp(_averageFrameTime, currentFrameTime, 0.1f);
            _lastFrameTime = currentFrameTime;
            _frameCount++;
            
            // Update FPS
            float currentFPS = 1f / _averageFrameTime;
            SetCurrentFPS(currentFPS);
            
            // Update memory usage
            _memoryUsageMB = System.GC.GetTotalMemory(false) / (1024f * 1024f);
            SetCurrentMemoryUsage(_memoryUsageMB);
            
            // Update CPU usage (simplified)
            _cpuUsagePercent = Mathf.Clamp01(Time.deltaTime / (1f / 60f)) * 100f;
            SetCurrentCPUUsage(_cpuUsagePercent);
            
            // Check for performance warnings
            CheckPerformanceWarnings();
        }
        
        /// <summary>
        /// Check for performance warnings and publish events
        /// </summary>
        protected override void CheckPerformanceWarnings()
        {
            float currentFPS = GetCurrentFPS();
            float currentMemory = GetCurrentMemoryUsage();
            float currentCPU = GetCurrentCPUUsage();
            
            // FPS warning
            if (currentFPS < GetFPSThreshold())
            {
                var warningEvent = new PerformanceWarningEvent("LowFPS", currentFPS, GetFPSThreshold());
                _eventBus.Publish(warningEvent);
                
                Debug.LogWarning($"[RunnerPerformanceManager] ⚠️ Low FPS detected: {currentFPS:F1} < {GetFPSThreshold():F1}");
            }
            
            // Memory warning
            if (currentMemory > GetMemoryThreshold())
            {
                var warningEvent = new PerformanceWarningEvent("HighMemory", currentMemory, GetMemoryThreshold());
                _eventBus.Publish(warningEvent);
                
                Debug.LogWarning($"[RunnerPerformanceManager] ⚠️ High memory usage: {currentMemory:F1}MB > {GetMemoryThreshold():F1}MB");
            }
            
            // CPU warning
            if (currentCPU > GetCPUThreshold())
            {
                var warningEvent = new PerformanceWarningEvent("HighCPU", currentCPU, GetCPUThreshold());
                _eventBus.Publish(warningEvent);
                
                Debug.LogWarning($"[RunnerPerformanceManager] ⚠️ High CPU usage: {currentCPU:F1}% > {GetCPUThreshold():F1}%");
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
                   $"FPS: {GetCurrentFPS():F1} (Target: {GetFPSThreshold():F1})\n" +
                   $"Memory: {GetCurrentMemoryUsage():F1}MB (Limit: {GetMemoryThreshold():F1}MB)\n" +
                   $"CPU: {GetCurrentCPUUsage():F1}% (Limit: {GetCPUThreshold():F1}%)\n" +
                   $"Frame Time: {_averageFrameTime * 1000f:F1}ms\n" +
                   $"Frame Count: {_frameCount}";
        }
        
        /// <summary>
        /// Get runner-specific performance metrics
        /// </summary>
        public RunnerPerformanceMetrics GetRunnerMetrics()
        {
            return new RunnerPerformanceMetrics
            {
                CurrentFPS = GetCurrentFPS(),
                AverageFrameTime = _averageFrameTime,
                MemoryUsageMB = GetCurrentMemoryUsage(),
                CPUUsagePercent = GetCurrentCPUUsage(),
                FrameCount = _frameCount,
                IsPerformanceGood = IsPerformanceGood()
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