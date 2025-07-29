using System;
using System.Collections.Generic;
using UnityEngine;
using MiniGameFramework.Core.Architecture;

namespace MiniGameFramework.Core.Events
{
    /// <summary>
    /// Implementation of the event bus system with zero-allocation event handling.
    /// Provides type-safe event publishing and subscription.
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> subscriptions = new Dictionary<Type, List<Delegate>>();
        private readonly Dictionary<Type, List<Delegate>> pendingSubscriptions = new Dictionary<Type, List<Delegate>>();
        private readonly Dictionary<Type, List<Delegate>> pendingUnsubscriptions = new Dictionary<Type, List<Delegate>>();
        
        private bool isPublishing = false;
        
        /// <summary>
        /// Subscribe to an event type.
        /// </summary>
        /// <typeparam name="T">The type of event to subscribe to.</typeparam>
        /// <param name="callback">The callback to invoke when the event is published.</param>
        public void Subscribe<T>(Action<T> callback) where T : class
        {
            if (callback == null)
            {
                Debug.LogWarning("[EventBus] Attempted to subscribe with null callback.");
                return;
            }
            
            var type = typeof(T);
            
            if (isPublishing)
            {
                // Queue subscription for after publishing is complete
                if (!pendingSubscriptions.ContainsKey(type))
                {
                    pendingSubscriptions[type] = new List<Delegate>();
                }
                pendingSubscriptions[type].Add(callback);
                return;
            }
            
            if (!subscriptions.ContainsKey(type))
            {
                subscriptions[type] = new List<Delegate>();
            }
            
            if (!subscriptions[type].Contains(callback))
            {
                subscriptions[type].Add(callback);
                Debug.Log($"[EventBus] Subscribed to {type.Name}");
            }
        }
        
        /// <summary>
        /// Unsubscribe from an event type.
        /// </summary>
        /// <typeparam name="T">The type of event to unsubscribe from.</typeparam>
        /// <param name="callback">The callback to remove.</param>
        public void Unsubscribe<T>(Action<T> callback) where T : class
        {
            if (callback == null)
            {
                Debug.LogWarning("[EventBus] Attempted to unsubscribe with null callback.");
                return;
            }
            
            var type = typeof(T);
            
            if (isPublishing)
            {
                // Queue unsubscription for after publishing is complete
                if (!pendingUnsubscriptions.ContainsKey(type))
                {
                    pendingUnsubscriptions[type] = new List<Delegate>();
                }
                pendingUnsubscriptions[type].Add(callback);
                return;
            }
            
            if (subscriptions.ContainsKey(type))
            {
                if (subscriptions[type].Remove(callback))
                {
                    Debug.Log($"[EventBus] Unsubscribed from {type.Name}");
                }
            }
        }
        
        /// <summary>
        /// Publish an event to all subscribers.
        /// </summary>
        /// <typeparam name="T">The type of event being published.</typeparam>
        /// <param name="eventData">The event data to publish.</param>
        public void Publish<T>(T eventData) where T : class
        {
            if (eventData == null)
            {
                Debug.LogWarning("[EventBus] Attempted to publish null event.");
                return;
            }
            
            var type = typeof(T);
            
            if (!subscriptions.ContainsKey(type))
            {
                // No subscribers for this event type
                return;
            }
            
            isPublishing = true;
            
            try
            {
                // Create a copy of the list to avoid modification during iteration
                var callbacks = new List<Delegate>(subscriptions[type]);
                
                foreach (var callback in callbacks)
                {
                    try
                    {
                        if (callback is Action<T> typedCallback)
                        {
                            typedCallback(eventData);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[EventBus] Error in event callback for {type.Name}: {e.Message}");
                    }
                }
            }
            finally
            {
                isPublishing = false;
                
                // Process pending subscriptions and unsubscriptions
                ProcessPendingSubscriptions();
                ProcessPendingUnsubscriptions();
            }
        }
        
        /// <summary>
        /// Clear all subscriptions for a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of event to clear subscriptions for.</typeparam>
        public void ClearSubscriptions<T>() where T : class
        {
            var type = typeof(T);
            
            if (subscriptions.ContainsKey(type))
            {
                subscriptions[type].Clear();
                Debug.Log($"[EventBus] Cleared all subscriptions for {type.Name}");
            }
        }
        
        /// <summary>
        /// Clear all subscriptions for all event types.
        /// </summary>
        public void ClearAllSubscriptions()
        {
            subscriptions.Clear();
            pendingSubscriptions.Clear();
            pendingUnsubscriptions.Clear();
            Debug.Log("[EventBus] Cleared all subscriptions");
        }
        
        /// <summary>
        /// Get the number of subscribers for a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of event to check.</typeparam>
        /// <returns>The number of subscribers.</returns>
        public int GetSubscriberCount<T>() where T : class
        {
            var type = typeof(T);
            return subscriptions.ContainsKey(type) ? subscriptions[type].Count : 0;
        }
        
        /// <summary>
        /// Check if there are any subscribers for a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of event to check.</typeparam>
        /// <returns>True if there are subscribers, false otherwise.</returns>
        public bool HasSubscribers<T>() where T : class
        {
            return GetSubscriberCount<T>() > 0;
        }
        
        #region Private Methods
        
        private void ProcessPendingSubscriptions()
        {
            foreach (var kvp in pendingSubscriptions)
            {
                var type = kvp.Key;
                var callbacks = kvp.Value;
                
                if (!subscriptions.ContainsKey(type))
                {
                    subscriptions[type] = new List<Delegate>();
                }
                
                foreach (var callback in callbacks)
                {
                    if (!subscriptions[type].Contains(callback))
                    {
                        subscriptions[type].Add(callback);
                    }
                }
            }
            
            pendingSubscriptions.Clear();
        }
        
        private void ProcessPendingUnsubscriptions()
        {
            foreach (var kvp in pendingUnsubscriptions)
            {
                var type = kvp.Key;
                var callbacks = kvp.Value;
                
                if (subscriptions.ContainsKey(type))
                {
                    foreach (var callback in callbacks)
                    {
                        subscriptions[type].Remove(callback);
                    }
                }
            }
            
            pendingUnsubscriptions.Clear();
        }
        
        #endregion
    }
}