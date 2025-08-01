using System;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Base class for all game events in the system.
    /// Provides common functionality and structure for event-driven communication.
    /// </summary>
    public abstract class GameEvent
    {
        #region Properties
        /// <summary>
        /// Unique identifier for this event instance
        /// </summary>
        public Guid EventId { get; private set; }
        
        /// <summary>
        /// Timestamp when the event was created
        /// </summary>
        public float Timestamp { get; private set; }
        
        /// <summary>
        /// Frame number when the event was created
        /// </summary>
        public int FrameNumber { get; private set; }
        
        /// <summary>
        /// Source object that created this event (optional)
        /// </summary>
        public GameObject Source { get; private set; }
        
        /// <summary>
        /// Event type name for debugging and logging
        /// </summary>
        public string EventType => GetType().Name;
        #endregion
        
        #region Constructor
        protected GameEvent(GameObject source = null)
        {
            EventId = Guid.NewGuid();
            Timestamp = Time.time;
            FrameNumber = Time.frameCount;
            Source = source;
            
            Debug.Log($"[GameEvent] Created {EventType} at frame {FrameNumber}");
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Get a string representation of the event for debugging
        /// </summary>
        public override string ToString()
        {
            return $"{EventType} [ID: {EventId}, Frame: {FrameNumber}, Time: {Timestamp:F2}s]";
        }
        
        /// <summary>
        /// Get detailed event information for logging
        /// </summary>
        public virtual string GetEventDetails()
        {
            return $"{EventType} - ID: {EventId}, Frame: {FrameNumber}, Time: {Timestamp:F2}s, Source: {(Source != null ? Source.name : "None")}";
        }
        
        /// <summary>
        /// Check if this event is still valid (not too old)
        /// </summary>
        public virtual bool IsValid(float maxAgeSeconds = 5f)
        {
            return (Time.time - Timestamp) <= maxAgeSeconds;
        }
        #endregion
        
        #region Static Methods
        /// <summary>
        /// Create a simple event with basic information
        /// </summary>
        public static T Create<T>(GameObject source = null) where T : GameEvent, new()
        {
            var gameEvent = new T();
            // Note: This approach requires the derived class to have a parameterless constructor
            // For now, we'll use the protected constructor approach above
            return gameEvent;
        }
        #endregion
    }
    
    /// <summary>
    /// Interface for events that can be cancelled
    /// </summary>
    public interface ICancellableEvent
    {
        bool IsCancelled { get; set; }
        string CancellationReason { get; set; }
    }
    
    /// <summary>
    /// Interface for events that have a priority level
    /// </summary>
    public interface IPrioritizedEvent
    {
        int Priority { get; set; }
    }
    
    /// <summary>
    /// Interface for events that can be delayed
    /// </summary>
    public interface IDelayedEvent
    {
        float DelaySeconds { get; set; }
        float ExecutionTime { get; set; }
    }
} 