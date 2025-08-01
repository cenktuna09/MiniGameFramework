using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Common.InputManagement
{
    /// <summary>
    /// Base class for input management with command pattern
    /// Provides input locking, command queue, and event-driven input handling
    /// </summary>
    public abstract class BaseInputManager
    {
        #region Protected Fields
        
        protected IEventBus _eventBus;
        protected bool _isInputLocked = false;
        protected List<BaseInputCommand> _commandQueue;
        
        #endregion
        
        #region Public Properties
        
        /// <summary>
        /// Whether input is currently locked
        /// </summary>
        public bool IsInputLocked => _isInputLocked;
        
        /// <summary>
        /// Number of commands in the queue
        /// </summary>
        public int CommandQueueCount => _commandQueue?.Count ?? 0;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Initialize input manager with event bus
        /// </summary>
        /// <param name="eventBus">Event bus for input notifications</param>
        protected BaseInputManager(IEventBus eventBus)
        {
            _eventBus = eventBus;
            Initialize();
        }
        
        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// Initialize input manager with command queue
        /// </summary>
        protected virtual void Initialize()
        {
            _commandQueue = new List<BaseInputCommand>();
            
            Debug.Log($"[{GetType().Name}] ✅ Input manager initialized");
        }
        
        /// <summary>
        /// Process input and create commands
        /// Must be implemented by derived classes
        /// </summary>
        public abstract void ProcessInput();
        
        /// <summary>
        /// Handle input command
        /// Must be implemented by derived classes
        /// </summary>
        /// <param name="command">Input command to handle</param>
        protected abstract void HandleInputCommand(BaseInputCommand command);
        
        /// <summary>
        /// Add command to queue
        /// </summary>
        /// <param name="command">Command to add</param>
        protected void AddCommand(BaseInputCommand command)
        {
            if (command != null)
            {
                _commandQueue.Add(command);
                Debug.Log($"[{GetType().Name}] 📝 Added command: {command.GetType().Name}");
            }
        }
        
        /// <summary>
        /// Process all commands in queue
        /// </summary>
        protected void ProcessCommandQueue()
        {
            if (_isInputLocked || _commandQueue.Count == 0)
                return;
            
            var commandsToProcess = new List<BaseInputCommand>(_commandQueue);
            _commandQueue.Clear();
            
            foreach (var command in commandsToProcess)
            {
                HandleInputCommand(command);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Lock input processing
        /// </summary>
        public virtual void LockInput()
        {
            _isInputLocked = true;
            Debug.Log($"[{GetType().Name}] 🔒 Input locked");
        }
        
        /// <summary>
        /// Unlock input processing
        /// </summary>
        public virtual void UnlockInput()
        {
            _isInputLocked = false;
            Debug.Log($"[{GetType().Name}] 🔓 Input unlocked");
        }
        
        /// <summary>
        /// Clear all commands in queue
        /// </summary>
        public void ClearCommandQueue()
        {
            _commandQueue?.Clear();
            Debug.Log($"[{GetType().Name}] 🗑️ Command queue cleared");
        }
        
        /// <summary>
        /// Get current command queue
        /// </summary>
        /// <returns>List of commands in queue</returns>
        public List<BaseInputCommand> GetCommandQueue()
        {
            return new List<BaseInputCommand>(_commandQueue);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Base class for input commands
    /// </summary>
    public abstract class BaseInputCommand
    {
        /// <summary>
        /// Timestamp when command was created
        /// </summary>
        public float Timestamp { get; }
        
        /// <summary>
        /// Command priority (higher = more important)
        /// </summary>
        public int Priority { get; set; }
        
        protected BaseInputCommand()
        {
            Timestamp = Time.time;
            Priority = 0;
        }
        
        /// <summary>
        /// Execute the command
        /// </summary>
        public abstract void Execute();
    }
    
    /// <summary>
    /// Event for input commands
    /// </summary>
    public class InputCommandEvent : GameEvent
    {
        public BaseInputCommand Command { get; }
        
        public InputCommandEvent(BaseInputCommand command)
        {
            Command = command;
        }
    }
} 