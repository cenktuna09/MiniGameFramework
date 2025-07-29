using System;

namespace MiniGameFramework.Core.Architecture
{
    /// <summary>
    /// Defines the contract for the event bus system.
    /// Enables loose coupling between systems through event-driven communication.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Subscribe to an event type.
        /// </summary>
        /// <typeparam name="T">The type of event to subscribe to.</typeparam>
        /// <param name="callback">The callback to invoke when the event is published.</param>
        void Subscribe<T>(Action<T> callback) where T : class;
        
        /// <summary>
        /// Unsubscribe from an event type.
        /// </summary>
        /// <typeparam name="T">The type of event to unsubscribe from.</typeparam>
        /// <param name="callback">The callback to remove.</param>
        void Unsubscribe<T>(Action<T> callback) where T : class;
        
        /// <summary>
        /// Publish an event to all subscribers.
        /// </summary>
        /// <typeparam name="T">The type of event being published.</typeparam>
        /// <param name="eventData">The event data to publish.</param>
        void Publish<T>(T eventData) where T : class;
        
        /// <summary>
        /// Clear all subscriptions for a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of event to clear subscriptions for.</typeparam>
        void ClearSubscriptions<T>() where T : class;
        
        /// <summary>
        /// Clear all subscriptions for all event types.
        /// </summary>
        void ClearAllSubscriptions();
    }
    
    /// <summary>
    /// Base class for all events in the framework.
    /// Provides common functionality and type safety.
    /// </summary>
    public abstract class GameEvent
    {
        /// <summary>
        /// Timestamp when the event was created.
        /// </summary>
        public float Timestamp { get; }
        
        /// <summary>
        /// Source object that published this event (optional).
        /// </summary>
        public object Source { get; }
        
        protected GameEvent(object source = null)
        {
            Timestamp = UnityEngine.Time.time;
            Source = source;
        }
    }
    
    /// <summary>
    /// Event fired when a mini-game state changes.
    /// </summary>
    public class GameStateChangedEvent : GameEvent
    {
        public string GameId { get; }
        public GameState OldState { get; }
        public GameState NewState { get; }
        
        public GameStateChangedEvent(string gameId, GameState oldState, GameState newState, object source = null) 
            : base(source)
        {
            GameId = gameId;
            OldState = oldState;
            NewState = newState;
        }
    }
    
    /// <summary>
    /// Event fired when a scene loading operation starts.
    /// </summary>
    public class SceneLoadingStartedEvent : GameEvent
    {
        public string SceneName { get; }
        
        public SceneLoadingStartedEvent(string sceneName, object source = null) 
            : base(source)
        {
            SceneName = sceneName;
        }
    }
    
    /// <summary>
    /// Event fired when a scene loading operation completes.
    /// </summary>
    public class SceneLoadingCompletedEvent : GameEvent
    {
        public string SceneName { get; }
        
        public SceneLoadingCompletedEvent(string sceneName, object source = null) 
            : base(source)
        {
            SceneName = sceneName;
        }
    }
    
    /// <summary>
    /// Event fired when the player's score changes.
    /// </summary>
    public class ScoreChangedEvent : GameEvent
    {
        public string GameId { get; }
        public int OldScore { get; }
        public int NewScore { get; }
        public int ScoreDelta { get; }
        
        public ScoreChangedEvent(string gameId, int oldScore, int newScore, object source = null) 
            : base(source)
        {
            GameId = gameId;
            OldScore = oldScore;
            NewScore = newScore;
            ScoreDelta = newScore - oldScore;
        }
    }
}