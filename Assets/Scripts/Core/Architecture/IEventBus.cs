using System;

namespace Core.Architecture
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
        /// <returns>IDisposable that can be used to unsubscribe.</returns>
        IDisposable Subscribe<T>(Action<T> callback) where T : class;
        
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
}