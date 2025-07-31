using System;
using System.Collections.Generic;
using UnityEngine;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.MiniGames.Match3.Board;
using MiniGameFramework.MiniGames.Match3.Data;
using MiniGameFramework.MiniGames.Match3.Input.Commands;
using MiniGameFramework.MiniGames.Match3.Visual.Strategies;

namespace MiniGameFramework.MiniGames.Match3.Performance
{
    /// <summary>
    /// Central performance manager that coordinates all optimization systems.
    /// Provides monitoring, statistics, and performance tuning capabilities.
    /// </summary>
    public class Match3PerformanceManager
    {
        #region Private Fields
        
        private readonly IEventBus eventBus;
        private readonly Match3LazyEvaluator lazyEvaluator;
        private readonly Match3OptimizedSwapDetector swapDetector;
        private readonly Match3CommandInvoker commandInvoker;
        private readonly IAnimationStrategy animationStrategy;
        
        // Performance monitoring
        private readonly Dictionary<string, float> performanceMetrics;
        private readonly List<string> performanceLog;
        private bool isMonitoring = false;
        private float lastPerformanceCheck = 0f;
        private const float PERFORMANCE_CHECK_INTERVAL = 5f; // Check every 5 seconds
        
        #endregion
        
        #region Events
        
        public event Action<string> OnPerformanceWarning;
        public event Action<string> OnPerformanceAlert;
        public event Action<string> OnPerformanceStats;
        
        #endregion
        
        #region Constructor
        
        public Match3PerformanceManager(IEventBus eventBus)
        {
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            
            // Initialize optimization systems
            lazyEvaluator = new Match3LazyEvaluator();
            swapDetector = new Match3OptimizedSwapDetector();
            commandInvoker = new Match3CommandInvoker(eventBus, 100);
            animationStrategy = new LeanTweenAnimationStrategy();
            
            // Initialize performance tracking
            performanceMetrics = new Dictionary<string, float>();
            performanceLog = new List<string>();
            
            // Subscribe to events
            commandInvoker.OnCommandExecuted += OnCommandExecuted;
            commandInvoker.OnCommandError += OnCommandError;
            
            Debug.Log("[Match3PerformanceManager] ✅ Performance manager initialized");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initializes all performance systems.
        /// </summary>
        public void Initialize()
        {
            lazyEvaluator.MarkDirty();
            swapDetector.MarkDirty();
            animationStrategy.Initialize();
            
            Debug.Log("[Match3PerformanceManager] 🚀 All performance systems initialized");
        }
        
        /// <summary>
        /// Updates the board state across all optimization systems.
        /// </summary>
        /// <param name="board">The current board state.</param>
        /// <param name="changedPositions">Positions that changed (for incremental updates).</param>
        public void UpdateBoardState(BoardData board, List<Vector2Int> changedPositions = null)
        {
            var startTime = Time.realtimeSinceStartup;
            
            // Update lazy evaluator
            lazyEvaluator.MarkDirty();
            
            // Update swap detector
            swapDetector.UpdateBoard(board, changedPositions);
            
            var updateTime = Time.realtimeSinceStartup - startTime;
            TrackMetric("BoardUpdateTime", updateTime);
            
            Debug.Log($"[Match3PerformanceManager] 🔄 Board state updated in {updateTime:F3}s");
        }
        
        /// <summary>
        /// Gets possible swaps using optimized detection.
        /// </summary>
        /// <param name="board">The current board state.</param>
        /// <returns>List of possible swaps.</returns>
        public List<Swap> GetPossibleSwaps(BoardData board)
        {
            var startTime = Time.realtimeSinceStartup;
            
            var swaps = swapDetector.GetValidSwaps();
            
            var detectionTime = Time.realtimeSinceStartup - startTime;
            TrackMetric("SwapDetectionTime", detectionTime);
            
            Debug.Log($"[Match3PerformanceManager] 🔍 Found {swaps.Count} possible swaps in {detectionTime:F3}s");
            return swaps;
        }
        
        /// <summary>
        /// Checks if a swap is valid using optimized detection.
        /// </summary>
        /// <param name="swap">The swap to check.</param>
        /// <returns>True if the swap is valid.</returns>
        public bool IsSwapValid(Swap swap)
        {
            var startTime = Time.realtimeSinceStartup;
            
            var isValid = swapDetector.IsSwapValid(swap);
            
            var checkTime = Time.realtimeSinceStartup - startTime;
            TrackMetric("SwapValidationTime", checkTime);
            
            return isValid;
        }
        
        /// <summary>
        /// Executes a command using the command pattern.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>True if the command was executed successfully.</returns>
        public bool ExecuteCommand(IMatch3Command command)
        {
            var startTime = Time.realtimeSinceStartup;
            
            var success = commandInvoker.ExecuteCommand(command);
            
            var executionTime = Time.realtimeSinceStartup - startTime;
            TrackMetric("CommandExecutionTime", executionTime);
            
            return success;
        }
        
        /// <summary>
        /// Animates a swap using the strategy pattern.
        /// </summary>
        /// <param name="tileA">First tile.</param>
        /// <param name="tileB">Second tile.</param>
        /// <param name="targetPosA">Target position for tile A.</param>
        /// <param name="targetPosB">Target position for tile B.</param>
        /// <param name="duration">Animation duration.</param>
        /// <param name="onComplete">Completion callback.</param>
        public void AnimateSwap(GameObject tileA, GameObject tileB, Vector3 targetPosA, Vector3 targetPosB, float duration, Action onComplete = null)
        {
            var startTime = Time.realtimeSinceStartup;
            
            animationStrategy.AnimateSwap(tileA, tileB, targetPosA, targetPosB, duration, () =>
            {
                var animationTime = Time.realtimeSinceStartup - startTime;
                TrackMetric("SwapAnimationTime", animationTime);
                onComplete?.Invoke();
            });
        }
        
        /// <summary>
        /// Starts performance monitoring.
        /// </summary>
        public void StartMonitoring()
        {
            isMonitoring = true;
            lastPerformanceCheck = Time.time;
            Debug.Log("[Match3PerformanceManager] 📊 Performance monitoring started");
        }
        
        /// <summary>
        /// Stops performance monitoring.
        /// </summary>
        public void StopMonitoring()
        {
            isMonitoring = false;
            Debug.Log("[Match3PerformanceManager] 📊 Performance monitoring stopped");
        }
        
        /// <summary>
        /// Updates performance monitoring (call from Update).
        /// </summary>
        public void UpdateMonitoring()
        {
            if (!isMonitoring) return;
            
            if (Time.time - lastPerformanceCheck >= PERFORMANCE_CHECK_INTERVAL)
            {
                CheckPerformance();
                lastPerformanceCheck = Time.time;
            }
        }
        
        /// <summary>
        /// Gets comprehensive performance statistics.
        /// </summary>
        /// <returns>Performance statistics string.</returns>
        public string GetPerformanceStats()
        {
            var stats = new List<string>
            {
                "[Match3PerformanceManager] 📊 PERFORMANCE STATISTICS:",
                $"Lazy Evaluator: {lazyEvaluator.GetCacheStats()}",
                $"Swap Detector: {swapDetector.GetPerformanceStats()}",
                $"Animation Strategy: {animationStrategy.GetPerformanceStats()}",
                $"Command Invoker: {commandInvoker.GetDebugInfo()}"
            };
            
            // Add custom metrics
            foreach (var metric in performanceMetrics)
            {
                stats.Add($"Metric '{metric.Key}': {metric.Value:F3}s");
            }
            
            return string.Join("\n", stats);
        }
        
        /// <summary>
        /// Cleans up all performance systems.
        /// </summary>
        public void Cleanup()
        {
            lazyEvaluator.ClearCache();
            swapDetector.ClearCache();
            commandInvoker.ClearHistory();
            animationStrategy.Cleanup();
            
            Debug.Log("[Match3PerformanceManager] 🧹 All performance systems cleaned up");
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Tracks a performance metric.
        /// </summary>
        private void TrackMetric(string metricName, float value)
        {
            if (performanceMetrics.ContainsKey(metricName))
            {
                // Use exponential moving average for stability
                performanceMetrics[metricName] = performanceMetrics[metricName] * 0.9f + value * 0.1f;
            }
            else
            {
                performanceMetrics[metricName] = value;
            }
        }
        
        /// <summary>
        /// Checks performance and issues warnings if needed.
        /// </summary>
        private void CheckPerformance()
        {
            var warnings = new List<string>();
            
            // Check board update time
            if (performanceMetrics.TryGetValue("BoardUpdateTime", out float boardUpdateTime))
            {
                if (boardUpdateTime > 0.016f) // More than 16ms (60 FPS target)
                {
                    warnings.Add($"Board update time ({boardUpdateTime:F3}s) exceeds 16ms target");
                }
            }
            
            // Check swap detection time
            if (performanceMetrics.TryGetValue("SwapDetectionTime", out float swapDetectionTime))
            {
                if (swapDetectionTime > 0.008f) // More than 8ms
                {
                    warnings.Add($"Swap detection time ({swapDetectionTime:F3}s) exceeds 8ms target");
                }
            }
            
            // Check animation time
            if (performanceMetrics.TryGetValue("SwapAnimationTime", out float animationTime))
            {
                if (animationTime > 0.5f) // More than 500ms
                {
                    warnings.Add($"Animation time ({animationTime:F3}s) exceeds 500ms target");
                }
            }
            
            // Issue warnings
            foreach (var warning in warnings)
            {
                OnPerformanceWarning?.Invoke(warning);
                Debug.LogWarning($"[Match3PerformanceManager] ⚠️ {warning}");
            }
            
            // Log performance stats
            if (warnings.Count == 0)
            {
                OnPerformanceStats?.Invoke(GetPerformanceStats());
            }
        }
        
        /// <summary>
        /// Handles command execution events.
        /// </summary>
        private void OnCommandExecuted(IMatch3Command command)
        {
            Debug.Log($"[Match3PerformanceManager] ✅ Command executed: {command.Type}");
        }
        
        /// <summary>
        /// Handles command error events.
        /// </summary>
        private void OnCommandError(string error)
        {
            OnPerformanceAlert?.Invoke($"Command error: {error}");
            Debug.LogError($"[Match3PerformanceManager] ❌ {error}");
        }
        
        #endregion
    }
} 