using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Architecture;
using Core.Events;
using MiniGameFramework.MiniGames.Match3.Board;
using MiniGameFramework.MiniGames.Match3.Data;
using MiniGameFramework.MiniGames.Match3.Config;

namespace MiniGameFramework.MiniGames.Match3.ErrorHandling
{
    /// <summary>
    /// Comprehensive error handling system for Match-3 game.
    /// Provides detailed error reporting, validation, and recovery mechanisms.
    /// </summary>
    public class Match3ErrorHandler
    {
        #region Error Types
        
        /// <summary>
        /// Custom exception for invalid swap operations.
        /// </summary>
        public class InvalidSwapException : Exception
        {
            public Swap Swap { get; }
            public string Reason { get; }
            
            public InvalidSwapException(Swap swap, string reason) : base($"Invalid swap {swap.tileA} ↔ {swap.tileB}: {reason}")
            {
                Swap = swap;
                Reason = reason;
            }
        }
        
        /// <summary>
        /// Custom exception for board state errors.
        /// </summary>
        public class BoardStateException : Exception
        {
            public Vector2Int Position { get; }
            public string Operation { get; }
            
            public BoardStateException(Vector2Int position, string operation, string message) : base($"Board state error at {position} during {operation}: {message}")
            {
                Position = position;
                Operation = operation;
            }
        }
        
        /// <summary>
        /// Custom exception for animation errors.
        /// </summary>
        public class AnimationException : Exception
        {
            public string AnimationType { get; }
            public GameObject Target { get; }
            
            public AnimationException(string animationType, GameObject target, string message) : base($"Animation error for {animationType} on {target?.name}: {message}")
            {
                AnimationType = animationType;
                Target = target;
            }
        }
        
        /// <summary>
        /// Custom exception for configuration errors.
        /// </summary>
        public class ConfigurationException : Exception
        {
            public string Setting { get; }
            public object Value { get; }
            
            public ConfigurationException(string setting, object value, string message) : base($"Configuration error for {setting}={value}: {message}")
            {
                Setting = setting;
                Value = value;
            }
        }
        
        #endregion
        
        #region Private Fields
        
        private readonly IEventBus eventBus;
        private readonly bool enableDetailedReporting;
        private readonly bool enableValidationChecks;
        private readonly List<ErrorLog> errorLogs;
        private readonly int maxErrorLogs;
        
        #endregion
        
        #region Events
        
        public event Action<Exception> OnErrorOccurred;
        public event Action<string> OnWarningOccurred;
        public event Action<string> OnValidationFailed;
        
        #endregion
        
        #region Error Log Structure
        
        [System.Serializable]
        public struct ErrorLog
        {
            public DateTime Timestamp;
            public string ErrorType;
            public string Message;
            public string StackTrace;
            public string Context;
            
            public ErrorLog(string errorType, string message, string stackTrace, string context)
            {
                Timestamp = DateTime.Now;
                ErrorType = errorType;
                Message = message;
                StackTrace = stackTrace;
                Context = context;
            }
        }
        
        #endregion
        
        #region Constructor
        
        public Match3ErrorHandler(IEventBus eventBus, bool enableDetailedReporting = true, bool enableValidationChecks = true, int maxErrorLogs = 100)
        {
            this.eventBus = eventBus;
            this.enableDetailedReporting = enableDetailedReporting;
            this.enableValidationChecks = enableValidationChecks;
            this.maxErrorLogs = maxErrorLogs;
            this.errorLogs = new List<ErrorLog>();
            
            Debug.Log("[Match3ErrorHandler] ✅ Initialized successfully");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Validates a swap operation and throws appropriate exceptions.
        /// </summary>
        /// <param name="swap">The swap to validate</param>
        /// <param name="board">The current board state</param>
        /// <param name="context">Additional context for error reporting</param>
        public void ValidateSwap(Swap swap, BoardData board, string context = "")
        {
            if (!enableValidationChecks) return;
            
            try
            {
                // Check for invalid swap (struct cannot be null, but we can check for invalid positions)
                if (swap.tileA == Vector2Int.zero && swap.tileB == Vector2Int.zero)
                {
                    throw new InvalidSwapException(new Swap(Vector2Int.zero, Vector2Int.zero), "Swap has invalid positions");
                }
                
                // Check for null board
                if (board == null)
                {
                    throw new InvalidSwapException(swap, "Board is null");
                }
                
                // Check if positions are within board bounds
                if (!IsPositionValid(swap.tileA, board) || !IsPositionValid(swap.tileB, board))
                {
                    throw new InvalidSwapException(swap, "Swap positions are outside board bounds");
                }
                
                // Check if tiles are adjacent
                int deltaX = Mathf.Abs(swap.tileA.x - swap.tileB.x);
                int deltaY = Mathf.Abs(swap.tileA.y - swap.tileB.y);
                bool isAdjacent = (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
                
                if (!isAdjacent)
                {
                    throw new InvalidSwapException(swap, "Tiles are not adjacent");
                }
                
                // Check if tiles exist at positions
                var tileA = board.GetTile(swap.tileA);
                var tileB = board.GetTile(swap.tileB);
                
                if (tileA == null)
                {
                    throw new InvalidSwapException(swap, $"No tile found at position {swap.tileA}");
                }
                
                if (tileB == null)
                {
                    throw new InvalidSwapException(swap, $"No tile found at position {swap.tileB}");
                }
                
                Debug.Log($"[Match3ErrorHandler] ✅ Swap validation passed: {swap.tileA} ↔ {swap.tileB}");
            }
            catch (Exception ex)
            {
                LogError(ex, context);
                throw;
            }
        }
        
        /// <summary>
        /// Validates board state and throws appropriate exceptions.
        /// </summary>
        /// <param name="board">The board to validate</param>
        /// <param name="operation">The operation being performed</param>
        public void ValidateBoardState(BoardData board, string operation)
        {
            if (!enableValidationChecks) return;
            
            try
            {
                if (board == null)
                {
                    throw new BoardStateException(Vector2Int.zero, operation, "Board is null");
                }
                
                // Check board dimensions
                if (board.Width <= 0 || board.Height <= 0)
                {
                    throw new BoardStateException(Vector2Int.zero, operation, $"Invalid board dimensions: {board.Width}x{board.Height}");
                }
                
                // Check for null tiles in board
                for (int x = 0; x < board.Width; x++)
                {
                    for (int y = 0; y < board.Height; y++)
                    {
                        var position = new Vector2Int(x, y);
                        var tile = board.GetTile(position);
                        
                        if (tile == null)
                        {
                            throw new BoardStateException(position, operation, "Null tile found in board");
                        }
                        
                        if (tile.Position != position)
                        {
                            throw new BoardStateException(position, operation, $"Tile position mismatch: expected {position}, got {tile.Position}");
                        }
                    }
                }
                
                Debug.Log($"[Match3ErrorHandler] ✅ Board state validation passed for operation: {operation}");
            }
            catch (Exception ex)
            {
                LogError(ex, $"Board validation for operation: {operation}");
                throw;
            }
        }
        
        /// <summary>
        /// Validates animation parameters and throws appropriate exceptions.
        /// </summary>
        /// <param name="animationType">Type of animation</param>
        /// <param name="target">Target GameObject</param>
        /// <param name="duration">Animation duration</param>
        public void ValidateAnimation(string animationType, GameObject target, float duration)
        {
            if (!enableValidationChecks) return;
            
            try
            {
                if (string.IsNullOrEmpty(animationType))
                {
                    throw new AnimationException("Unknown", target, "Animation type is null or empty");
                }
                
                if (target == null)
                {
                    throw new AnimationException(animationType, null, "Target GameObject is null");
                }
                
                if (duration <= 0)
                {
                    throw new AnimationException(animationType, target, $"Invalid duration: {duration}");
                }
                
                if (duration > 10f)
                {
                    throw new AnimationException(animationType, target, $"Duration too long: {duration}");
                }
                
                Debug.Log($"[Match3ErrorHandler] ✅ Animation validation passed: {animationType} on {target.name}");
            }
            catch (Exception ex)
            {
                LogError(ex, $"Animation validation for {animationType}");
                throw;
            }
        }
        
        /// <summary>
        /// Validates configuration settings and throws appropriate exceptions.
        /// </summary>
        /// <param name="config">Configuration to validate</param>
        public void ValidateConfiguration(Match3Config config)
        {
            if (!enableValidationChecks) return;
            
            try
            {
                if (config == null)
                {
                    throw new ConfigurationException("Config", null, "Configuration is null");
                }
                
                // Validate board settings
                if (config.boardWidth < 3 || config.boardHeight < 3)
                {
                    throw new ConfigurationException("BoardSize", $"{config.boardWidth}x{config.boardHeight}", "Board size too small");
                }
                
                if (config.boardWidth > 20 || config.boardHeight > 20)
                {
                    throw new ConfigurationException("BoardSize", $"{config.boardWidth}x{config.boardHeight}", "Board size too large");
                }
                
                // Validate animation settings
                if (config.swapDuration <= 0 || config.gravityDuration <= 0)
                {
                    throw new ConfigurationException("AnimationDuration", $"{config.swapDuration}/{config.gravityDuration}", "Invalid animation duration");
                }
                
                // Validate gameplay settings
                if (config.pointsPerTile <= 0)
                {
                    throw new ConfigurationException("PointsPerTile", config.pointsPerTile, "Points per tile must be positive");
                }
                
                if (config.minMatchLength < 3)
                {
                    throw new ConfigurationException("MinMatchLength", config.minMatchLength, "Minimum match length too low");
                }
                
                Debug.Log("[Match3ErrorHandler] ✅ Configuration validation passed");
            }
            catch (Exception ex)
            {
                LogError(ex, "Configuration validation");
                throw;
            }
        }
        
        /// <summary>
        /// Safely executes an action with error handling and recovery.
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="context">Context for error reporting</param>
        /// <param name="recoveryAction">Recovery action to execute on error</param>
        /// <returns>True if action succeeded, false if error occurred</returns>
        public bool SafeExecute(Action action, string context, Action recoveryAction = null)
        {
            try
            {
                action?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, context);
                
                try
                {
                    recoveryAction?.Invoke();
                    Debug.Log($"[Match3ErrorHandler] 🔄 Recovery action executed for: {context}");
                }
                catch (Exception recoveryEx)
                {
                    LogError(recoveryEx, $"Recovery action for: {context}");
                }
                
                return false;
            }
        }
        
        /// <summary>
        /// Safely executes an action that returns a value with error handling.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="func">Function to execute</param>
        /// <param name="defaultValue">Default value to return on error</param>
        /// <param name="context">Context for error reporting</param>
        /// <returns>Function result or default value on error</returns>
        public T SafeExecute<T>(Func<T> func, T defaultValue, string context)
        {
            try
            {
                return func != null ? func() : defaultValue;
            }
            catch (Exception ex)
            {
                LogError(ex, context);
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Logs an error with detailed information.
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="context">Additional context</param>
        public void LogError(Exception exception, string context = "")
        {
            if (exception == null) return;
            
            var errorLog = new ErrorLog(
                exception.GetType().Name,
                exception.Message,
                enableDetailedReporting ? exception.StackTrace : "",
                context
            );
            
            errorLogs.Add(errorLog);
            
            // Keep only the most recent logs
            if (errorLogs.Count > maxErrorLogs)
            {
                errorLogs.RemoveAt(0);
            }
            
            // Log to Unity console
            Debug.LogError($"[Match3ErrorHandler] ❌ {exception.GetType().Name}: {exception.Message}");
            if (enableDetailedReporting && !string.IsNullOrEmpty(context))
            {
                Debug.LogError($"[Match3ErrorHandler] 📍 Context: {context}");
            }
            
            // Publish error event
            OnErrorOccurred?.Invoke(exception);
            
            // Publish to event bus if available
            eventBus?.Publish(new ErrorOccurredEvent(exception, context, null));
        }
        
        /// <summary>
        /// Logs a warning with context.
        /// </summary>
        /// <param name="message">Warning message</param>
        /// <param name="context">Additional context</param>
        public void LogWarning(string message, string context = "")
        {
            if (string.IsNullOrEmpty(message)) return;
            
            Debug.LogWarning($"[Match3ErrorHandler] ⚠️ {message}");
            if (!string.IsNullOrEmpty(context))
            {
                Debug.LogWarning($"[Match3ErrorHandler] 📍 Context: {context}");
            }
            
            OnWarningOccurred?.Invoke(message);
        }
        
        /// <summary>
        /// Gets the error logs for debugging or reporting.
        /// </summary>
        /// <returns>List of error logs</returns>
        public List<ErrorLog> GetErrorLogs()
        {
            return new List<ErrorLog>(errorLogs);
        }
        
        /// <summary>
        /// Clears all error logs.
        /// </summary>
        public void ClearErrorLogs()
        {
            errorLogs.Clear();
            Debug.Log("[Match3ErrorHandler] 🧹 Error logs cleared");
        }
        
        /// <summary>
        /// Gets error statistics for monitoring.
        /// </summary>
        /// <returns>Dictionary with error type counts</returns>
        public Dictionary<string, int> GetErrorStatistics()
        {
            var stats = new Dictionary<string, int>();
            
            foreach (var log in errorLogs)
            {
                if (stats.ContainsKey(log.ErrorType))
                {
                    stats[log.ErrorType]++;
                }
                else
                {
                    stats[log.ErrorType] = 1;
                }
            }
            
            return stats;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Checks if a position is valid within the board bounds.
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <param name="board">Board to check against</param>
        /// <returns>True if position is valid</returns>
        private bool IsPositionValid(Vector2Int position, BoardData board)
        {
            return position.x >= 0 && position.x < board.Width &&
                   position.y >= 0 && position.y < board.Height;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Event for when an error occurs in the Match-3 system.
    /// </summary>
    public class ErrorOccurredEvent : GameEvent
    {
        public Exception Exception { get; }
        public string Context { get; }
        
        public ErrorOccurredEvent(Exception exception, string context, GameObject source = null) 
            : base(source)
        {
            Exception = exception;
            Context = context;
        }
    }
} 