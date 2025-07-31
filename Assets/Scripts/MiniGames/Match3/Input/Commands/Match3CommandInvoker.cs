using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MiniGameFramework.Core.Architecture;

namespace MiniGameFramework.MiniGames.Match3.Input.Commands
{
    /// <summary>
    /// Manages command execution, history, and undo functionality.
    /// Implements the Command pattern invoker for Match3 input system.
    /// </summary>
    public class Match3CommandInvoker
    {
        #region Private Fields
        
        private readonly Stack<IMatch3Command> commandHistory;
        private readonly Stack<IMatch3Command> undoHistory;
        private readonly IEventBus eventBus;
        private readonly int maxHistorySize;
        private bool isExecuting = false;
        
        #endregion
        
        #region Events
        
        public event Action<IMatch3Command> OnCommandExecuted;
        public event Action<IMatch3Command> OnCommandUndone;
        public event Action<string> OnCommandError;
        
        #endregion
        
        #region Constructor
        
        public Match3CommandInvoker(IEventBus eventBus, int maxHistorySize = 50)
        {
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            this.maxHistorySize = maxHistorySize;
            commandHistory = new Stack<IMatch3Command>();
            undoHistory = new Stack<IMatch3Command>();
            
            Debug.Log($"[Match3CommandInvoker] ✅ Initialized with max history size: {maxHistorySize}");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Executes a command and adds it to history.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>True if the command was executed successfully.</returns>
        public bool ExecuteCommand(IMatch3Command command)
        {
            if (command == null)
            {
                OnCommandError?.Invoke("Cannot execute null command");
                return false;
            }
            
            if (isExecuting)
            {
                OnCommandError?.Invoke("Cannot execute command while another is being processed");
                return false;
            }
            
            try
            {
                isExecuting = true;
                
                Debug.Log($"[Match3CommandInvoker] 🚀 Executing command: {command.GetDebugInfo()}");
                
                bool success = command.Execute();
                
                if (success)
                {
                    AddToHistory(command);
                    OnCommandExecuted?.Invoke(command);
                    
                    Debug.Log($"[Match3CommandInvoker] ✅ Command executed successfully: {command.Type}");
                }
                else
                {
                    Debug.LogWarning($"[Match3CommandInvoker] ⚠️ Command execution failed: {command.Type}");
                }
                
                return success;
            }
            catch (Exception e)
            {
                OnCommandError?.Invoke($"Command execution error: {e.Message}");
                Debug.LogError($"[Match3CommandInvoker] ❌ Command execution error: {e.Message}");
                return false;
            }
            finally
            {
                isExecuting = false;
            }
        }
        
        /// <summary>
        /// Undoes the last executed command.
        /// </summary>
        /// <returns>True if a command was undone successfully.</returns>
        public bool UndoLastCommand()
        {
            if (commandHistory.Count == 0)
            {
                Debug.Log("[Match3CommandInvoker] 📭 No commands to undo");
                return false;
            }
            
            try
            {
                var command = commandHistory.Pop();
                Debug.Log($"[Match3CommandInvoker] ↩️ Undoing command: {command.GetDebugInfo()}");
                
                command.Undo();
                undoHistory.Push(command);
                OnCommandUndone?.Invoke(command);
                
                Debug.Log($"[Match3CommandInvoker] ✅ Command undone successfully: {command.Type}");
                return true;
            }
            catch (Exception e)
            {
                OnCommandError?.Invoke($"Command undo error: {e.Message}");
                Debug.LogError($"[Match3CommandInvoker] ❌ Command undo error: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Redoes the last undone command.
        /// </summary>
        /// <returns>True if a command was redone successfully.</returns>
        public bool RedoLastCommand()
        {
            if (undoHistory.Count == 0)
            {
                Debug.Log("[Match3CommandInvoker] 📭 No commands to redo");
                return false;
            }
            
            try
            {
                var command = undoHistory.Pop();
                Debug.Log($"[Match3CommandInvoker] 🔄 Redoing command: {command.GetDebugInfo()}");
                
                bool success = command.Execute();
                
                if (success)
                {
                    commandHistory.Push(command);
                    OnCommandExecuted?.Invoke(command);
                    Debug.Log($"[Match3CommandInvoker] ✅ Command redone successfully: {command.Type}");
                }
                else
                {
                    Debug.LogWarning($"[Match3CommandInvoker] ⚠️ Command redo failed: {command.Type}");
                }
                
                return success;
            }
            catch (Exception e)
            {
                OnCommandError?.Invoke($"Command redo error: {e.Message}");
                Debug.LogError($"[Match3CommandInvoker] ❌ Command redo error: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Clears all command history.
        /// </summary>
        public void ClearHistory()
        {
            commandHistory.Clear();
            undoHistory.Clear();
            Debug.Log("[Match3CommandInvoker] 🧹 Command history cleared");
        }
        
        /// <summary>
        /// Gets the number of commands in history.
        /// </summary>
        /// <returns>Tuple of (command history count, undo history count).</returns>
        public (int commandCount, int undoCount) GetHistoryCounts()
        {
            return (commandHistory.Count, undoHistory.Count);
        }
        
        /// <summary>
        /// Gets debug information about the command invoker.
        /// </summary>
        /// <returns>Debug information string.</returns>
        public string GetDebugInfo()
        {
            var (commandCount, undoCount) = GetHistoryCounts();
            var totalCommands = commandCount + undoCount;
            
            return $"[Match3CommandInvoker] 📊 History: {commandCount} commands, {undoCount} undone, Total: {totalCommands}, Max: {maxHistorySize}";
        }
        
        /// <summary>
        /// Gets the last executed command.
        /// </summary>
        /// <returns>The last command or null if none.</returns>
        public IMatch3Command GetLastCommand()
        {
            return commandHistory.Count > 0 ? commandHistory.Peek() : null;
        }
        
        /// <summary>
        /// Gets all commands in history for debugging.
        /// </summary>
        /// <returns>Array of command debug info.</returns>
        public string[] GetCommandHistoryDebug()
        {
            return commandHistory.Select(cmd => cmd.GetDebugInfo()).ToArray();
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Adds a command to history, maintaining the maximum size.
        /// </summary>
        /// <param name="command">The command to add.</param>
        private void AddToHistory(IMatch3Command command)
        {
            commandHistory.Push(command);
            
            // Maintain maximum history size
            if (commandHistory.Count > maxHistorySize)
            {
                var oldestCommand = commandHistory.ToArray().Last();
                commandHistory.Clear();
                
                // Rebuild stack without the oldest command
                var commands = commandHistory.ToArray().Reverse().Skip(1).Reverse();
                foreach (var cmd in commands)
                {
                    commandHistory.Push(cmd);
                }
                
                Debug.Log($"[Match3CommandInvoker] 🗑️ Removed oldest command to maintain history size: {oldestCommand.Type}");
            }
            
            // Clear undo history when new command is executed
            undoHistory.Clear();
        }
        
        #endregion
    }
} 