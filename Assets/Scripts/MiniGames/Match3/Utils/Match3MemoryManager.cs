using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.MiniGames.Match3.Events;

namespace MiniGameFramework.MiniGames.Match3.Utils
{
    /// <summary>
    /// Memory management system for Match3 game to prevent memory leaks.
    /// Handles coroutine cleanup, event unsubscription, and resource disposal.
    /// </summary>
    public class Match3MemoryManager
    {
        private readonly IEventBus eventBus;
        private readonly List<Coroutine> activeCoroutines = new List<Coroutine>();
        private readonly List<IDisposable> eventSubscriptions = new List<IDisposable>();
        private readonly List<GameObject> trackedObjects = new List<GameObject>();
        
        // Event subscriptions for memory management
        private IDisposable gravityCompletedSubscription;
        private IDisposable refillCompletedSubscription;
        private IDisposable tileMovementSubscription;
        
        public Match3MemoryManager(IEventBus eventBus)
        {
            this.eventBus = eventBus;
            SubscribeToEvents();
        }
        
        /// <summary>
        /// Subscribes to events for memory management tracking.
        /// </summary>
        private void SubscribeToEvents()
        {
            gravityCompletedSubscription = eventBus.Subscribe<GravityCompletedEvent>(OnGravityCompleted);
            refillCompletedSubscription = eventBus.Subscribe<RefillCompletedEvent>(OnRefillCompleted);
            tileMovementSubscription = eventBus.Subscribe<TileMovementCompletedEvent>(OnTileMovementCompleted);
            
            eventSubscriptions.Add(gravityCompletedSubscription);
            eventSubscriptions.Add(refillCompletedSubscription);
            eventSubscriptions.Add(tileMovementSubscription);
        }
        
        /// <summary>
        /// Tracks a coroutine for cleanup.
        /// </summary>
        /// <param name="coroutine">The coroutine to track.</param>
        public void TrackCoroutine(Coroutine coroutine)
        {
            if (coroutine != null)
            {
                activeCoroutines.Add(coroutine);
                Debug.Log($"[Match3MemoryManager] Tracking coroutine: {coroutine.GetHashCode()}");
            }
        }
        
        /// <summary>
        /// Tracks an event subscription for cleanup.
        /// </summary>
        /// <param name="subscription">The subscription to track.</param>
        public void TrackSubscription(IDisposable subscription)
        {
            if (subscription != null)
            {
                eventSubscriptions.Add(subscription);
                Debug.Log($"[Match3MemoryManager] Tracking subscription: {subscription.GetType().Name}");
            }
        }
        
        /// <summary>
        /// Tracks a GameObject for cleanup.
        /// </summary>
        /// <param name="gameObject">The GameObject to track.</param>
        public void TrackGameObject(GameObject gameObject)
        {
            if (gameObject != null)
            {
                trackedObjects.Add(gameObject);
                Debug.Log($"[Match3MemoryManager] Tracking GameObject: {gameObject.name}");
            }
        }
        
        /// <summary>
        /// Stops and removes a tracked coroutine.
        /// </summary>
        /// <param name="coroutine">The coroutine to stop.</param>
        /// <param name="monoBehaviour">The MonoBehaviour that started the coroutine.</param>
        public void StopCoroutine(Coroutine coroutine, MonoBehaviour monoBehaviour)
        {
            if (coroutine != null && monoBehaviour != null)
            {
                monoBehaviour.StopCoroutine(coroutine);
                activeCoroutines.Remove(coroutine);
                Debug.Log($"[Match3MemoryManager] Stopped coroutine: {coroutine.GetHashCode()}");
            }
        }
        
        /// <summary>
        /// Disposes and removes a tracked subscription.
        /// </summary>
        /// <param name="subscription">The subscription to dispose.</param>
        public void DisposeSubscription(IDisposable subscription)
        {
            if (subscription != null)
            {
                subscription.Dispose();
                eventSubscriptions.Remove(subscription);
                Debug.Log($"[Match3MemoryManager] Disposed subscription: {subscription.GetType().Name}");
            }
        }
        
        /// <summary>
        /// Destroys a tracked GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to destroy.</param>
        public void DestroyGameObject(GameObject gameObject)
        {
            if (gameObject != null)
            {
                UnityEngine.Object.Destroy(gameObject);
                trackedObjects.Remove(gameObject);
                Debug.Log($"[Match3MemoryManager] Destroyed GameObject: {gameObject.name}");
            }
        }
        
        /// <summary>
        /// Stops all tracked coroutines.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour that started the coroutines.</param>
        public void StopAllCoroutines(MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null) return;
            
            foreach (var coroutine in activeCoroutines)
            {
                if (coroutine != null)
                {
                    monoBehaviour.StopCoroutine(coroutine);
                    Debug.Log($"[Match3MemoryManager] Stopped coroutine: {coroutine.GetHashCode()}");
                }
            }
            
            activeCoroutines.Clear();
            Debug.Log("[Match3MemoryManager] All tracked coroutines stopped");
        }
        
        /// <summary>
        /// Disposes all tracked subscriptions.
        /// </summary>
        public void DisposeAllSubscriptions()
        {
            foreach (var subscription in eventSubscriptions)
            {
                if (subscription != null)
                {
                    subscription.Dispose();
                    Debug.Log($"[Match3MemoryManager] Disposed subscription: {subscription.GetType().Name}");
                }
            }
            
            eventSubscriptions.Clear();
            Debug.Log("[Match3MemoryManager] All tracked subscriptions disposed");
        }
        
        /// <summary>
        /// Destroys all tracked GameObjects.
        /// </summary>
        public void DestroyAllTrackedObjects()
        {
            foreach (var gameObject in trackedObjects)
            {
                if (gameObject != null)
                {
                    UnityEngine.Object.Destroy(gameObject);
                    Debug.Log($"[Match3MemoryManager] Destroyed GameObject: {gameObject.name}");
                }
            }
            
            trackedObjects.Clear();
            Debug.Log("[Match3MemoryManager] All tracked GameObjects destroyed");
        }
        
        /// <summary>
        /// Performs complete cleanup of all tracked resources.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour that started the coroutines.</param>
        public void CleanupAll(MonoBehaviour monoBehaviour)
        {
            Debug.Log("[Match3MemoryManager] 🧹 Starting complete cleanup...");
            
            StopAllCoroutines(monoBehaviour);
            DisposeAllSubscriptions();
            DestroyAllTrackedObjects();
            
            Debug.Log("[Match3MemoryManager] ✅ Complete cleanup finished");
        }
        
        /// <summary>
        /// Gets the number of tracked coroutines.
        /// </summary>
        /// <returns>The number of active coroutines.</returns>
        public int GetActiveCoroutineCount()
        {
            return activeCoroutines.Count;
        }
        
        /// <summary>
        /// Gets the number of tracked subscriptions.
        /// </summary>
        /// <returns>The number of active subscriptions.</returns>
        public int GetActiveSubscriptionCount()
        {
            return eventSubscriptions.Count;
        }
        
        /// <summary>
        /// Gets the number of tracked GameObjects.
        /// </summary>
        /// <returns>The number of tracked GameObjects.</returns>
        public int GetTrackedObjectCount()
        {
            return trackedObjects.Count;
        }
        
        /// <summary>
        /// Logs memory usage statistics.
        /// </summary>
        public void LogMemoryStats()
        {
            Debug.Log($"[Match3MemoryManager] 📊 Memory Stats:");
            Debug.Log($"  - Active Coroutines: {GetActiveCoroutineCount()}");
            Debug.Log($"  - Active Subscriptions: {GetActiveSubscriptionCount()}");
            Debug.Log($"  - Tracked Objects: {GetTrackedObjectCount()}");
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Handles gravity completed events for memory management.
        /// </summary>
        /// <param name="gravityEvent">The gravity completed event.</param>
        private void OnGravityCompleted(GravityCompletedEvent gravityEvent)
        {
            Debug.Log($"[Match3MemoryManager] Gravity completed for column {gravityEvent.Column}");
            // Could add specific cleanup logic here if needed
        }
        
        /// <summary>
        /// Handles refill completed events for memory management.
        /// </summary>
        /// <param name="refillEvent">The refill completed event.</param>
        private void OnRefillCompleted(RefillCompletedEvent refillEvent)
        {
            Debug.Log($"[Match3MemoryManager] Refill completed for column {refillEvent.Column}");
            // Could add specific cleanup logic here if needed
        }
        
        /// <summary>
        /// Handles tile movement completed events for memory management.
        /// </summary>
        /// <param name="movementEvent">The tile movement completed event.</param>
        private void OnTileMovementCompleted(TileMovementCompletedEvent movementEvent)
        {
            Debug.Log($"[Match3MemoryManager] Tile movement completed: {movementEvent.FromPosition} -> {movementEvent.ToPosition}");
            // Could add specific cleanup logic here if needed
        }
        
        #endregion
        
        /// <summary>
        /// Creates a safe coroutine wrapper that automatically tracks the coroutine.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour to start the coroutine on.</param>
        /// <param name="coroutine">The coroutine to start.</param>
        /// <returns>The started coroutine.</returns>
        public Coroutine StartTrackedCoroutine(MonoBehaviour monoBehaviour, IEnumerator coroutine)
        {
            if (monoBehaviour == null || coroutine == null) return null;
            
            var startedCoroutine = monoBehaviour.StartCoroutine(coroutine);
            TrackCoroutine(startedCoroutine);
            return startedCoroutine;
        }
        
        /// <summary>
        /// Creates a safe event subscription that automatically tracks the subscription.
        /// </summary>
        /// <typeparam name="T">The event type.</typeparam>
        /// <param name="callback">The callback to subscribe.</param>
        /// <returns>The subscription.</returns>
        public IDisposable SubscribeTracked<T>(Action<T> callback) where T : class
        {
            if (callback == null) return null;
            
            var subscription = eventBus.Subscribe(callback);
            TrackSubscription(subscription);
            return subscription;
        }
    }
} 