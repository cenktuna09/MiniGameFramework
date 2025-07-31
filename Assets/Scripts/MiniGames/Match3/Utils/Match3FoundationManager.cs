using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.MiniGames.Match3.Data;
using MiniGameFramework.MiniGames.Match3.Events;
using MiniGameFramework.MiniGames.Match3.Visual;
using MiniGameFramework.MiniGames.Match3.Logic;

namespace MiniGameFramework.MiniGames.Match3.Utils
{
    /// <summary>
    /// Foundation manager that integrates all Week 1-2 foundation systems.
    /// Provides a clean interface for Match3Game to use the new systems.
    /// </summary>
    public class Match3FoundationManager
    {
        private readonly IEventBus eventBus;
        private readonly TilePositionCache positionCache;
        private readonly Match3AnimationManager animationManager;
        private readonly Match3MemoryManager memoryManager;
        
        // Event tracking for gravity completion
        private readonly Dictionary<int, bool> gravityCompletionStatus = new Dictionary<int, bool>();
        private readonly Dictionary<int, bool> refillCompletionStatus = new Dictionary<int, bool>();
        
        // Configuration
        private readonly float tileSize;
        private readonly float swapDuration;
        private readonly float gravityDuration;
        private readonly float matchAnimationDuration;
        
        public Match3FoundationManager(
            IEventBus eventBus, 
            float tileSize, 
            float swapDuration, 
            float gravityDuration, 
            float matchAnimationDuration)
        {
            this.eventBus = eventBus;
            this.tileSize = tileSize;
            this.swapDuration = swapDuration;
            this.gravityDuration = gravityDuration;
            this.matchAnimationDuration = matchAnimationDuration;
            
            // Initialize foundation systems
            positionCache = new TilePositionCache();
            animationManager = new Match3AnimationManager(eventBus, tileSize, swapDuration, gravityDuration, matchAnimationDuration);
            memoryManager = new Match3MemoryManager(eventBus);
            
            // Subscribe to completion events
            SubscribeToCompletionEvents();
            
            Debug.Log("[Match3FoundationManager] ✅ Foundation systems initialized");
        }
        
        /// <summary>
        /// Subscribes to completion events for tracking.
        /// </summary>
        private void SubscribeToCompletionEvents()
        {
            memoryManager.SubscribeTracked<GravityCompletedEvent>(OnGravityCompleted);
            memoryManager.SubscribeTracked<RefillCompletedEvent>(OnRefillCompleted);
            memoryManager.SubscribeTracked<AllGravityCompletedEvent>(OnAllGravityCompleted);
            memoryManager.SubscribeTracked<AllRefillCompletedEvent>(OnAllRefillCompleted);
        }
        
        #region Position Cache Integration
        
        /// <summary>
        /// Gets the board position of a tile with O(1) complexity.
        /// </summary>
        /// <param name="tile">The tile GameObject.</param>
        /// <returns>The board position.</returns>
        public Vector2Int GetTilePosition(GameObject tile)
        {
            return positionCache.GetPosition(tile);
        }
        
        /// <summary>
        /// Updates the position of a tile in the cache.
        /// </summary>
        /// <param name="tile">The tile GameObject.</param>
        /// <param name="newPosition">The new position.</param>
        public void UpdateTilePosition(GameObject tile, Vector2Int newPosition)
        {
            positionCache.UpdatePosition(tile, newPosition);
        }
        
        /// <summary>
        /// Removes a tile from the position cache.
        /// </summary>
        /// <param name="tile">The tile GameObject.</param>
        public void RemoveTileFromCache(GameObject tile)
        {
            positionCache.RemoveTile(tile);
        }
        
        /// <summary>
        /// Initializes the position cache with the current board state.
        /// </summary>
        /// <param name="visualTiles">The visual tiles array.</param>
        public void InitializePositionCache(GameObject[,] visualTiles)
        {
            positionCache.InitializeCache(visualTiles);
        }
        
        /// <summary>
        /// Gets the number of cached tiles.
        /// </summary>
        /// <returns>The cache size.</returns>
        public int GetPositionCacheSize()
        {
            return positionCache.GetCacheSize();
        }
        
        #endregion
        
        #region Animation Manager Integration
        
        /// <summary>
        /// Animates a tile swap.
        /// </summary>
        /// <param name="swap">The swap to animate.</param>
        /// <param name="visualTiles">The visual tiles array.</param>
        public async Task AnimateSwap(Swap swap, GameObject[,] visualTiles)
        {
            if (animationManager == null)
            {
                Debug.LogError("[Match3FoundationManager] Animation manager is null");
                return;
            }
            await animationManager.AnimateSwap(swap, visualTiles);
        }
        
        /// <summary>
        /// Animates gravity for a column.
        /// </summary>
        /// <param name="column">The column to animate.</param>
        /// <param name="tilesToMove">The tiles to move.</param>
        /// <param name="visualTiles">The visual tiles array.</param>
        public async Task AnimateGravity(int column, List<(int fromY, int toY, TileData tile, GameObject visual)> tilesToMove, GameObject[,] visualTiles)
        {
            if (animationManager == null)
            {
                Debug.LogError("[Match3FoundationManager] Animation manager is null");
                return;
            }
            
            // Reset completion status
            gravityCompletionStatus[column] = false;
            
            await animationManager.AnimateGravity(column, tilesToMove, visualTiles);
        }
        
        /// <summary>
        /// Animates refill for a column.
        /// </summary>
        /// <param name="column">The column to refill.</param>
        /// <param name="tilesToSpawn">The tiles to spawn.</param>
        /// <param name="visualTiles">The visual tiles array.</param>
        public async Task AnimateRefill(int column, List<(int y, TileData tile)> tilesToSpawn, GameObject[,] visualTiles)
        {
            if (animationManager == null)
            {
                Debug.LogError("[Match3FoundationManager] Animation manager is null");
                return;
            }
            
            // Reset completion status
            refillCompletionStatus[column] = false;
            
            await animationManager.AnimateRefill(column, tilesToSpawn, visualTiles);
        }
        
        /// <summary>
        /// Animates matched tiles.
        /// </summary>
        /// <param name="matches">The matches to animate.</param>
        /// <param name="visualTiles">The visual tiles array.</param>
        public async Task AnimateMatches(List<MatchDetector.Match> matches, GameObject[,] visualTiles)
        {
            if (animationManager == null)
            {
                Debug.LogError("[Match3FoundationManager] Animation manager is null");
                return;
            }
            await animationManager.AnimateMatches(matches, visualTiles);
        }
        
        /// <summary>
        /// Shows invalid move animation.
        /// </summary>
        /// <param name="tileA">First tile.</param>
        /// <param name="tileB">Second tile.</param>
        public async Task ShowInvalidMoveAnimation(GameObject tileA, GameObject tileB)
        {
            if (animationManager == null)
            {
                Debug.LogError("[Match3FoundationManager] Animation manager is null");
                return;
            }
            await animationManager.ShowInvalidMoveAnimation(tileA, tileB);
        }
        
        /// <summary>
        /// Checks if any animations are active.
        /// </summary>
        /// <returns>True if animations are active.</returns>
        public bool HasActiveAnimations()
        {
            return animationManager.HasActiveAnimations();
        }
        
        /// <summary>
        /// Stops all animations.
        /// </summary>
        public void StopAllAnimations()
        {
            animationManager.StopAllAnimations();
        }
        
        #endregion
        
        #region Memory Manager Integration
        
        /// <summary>
        /// Tracks a coroutine for cleanup.
        /// </summary>
        /// <param name="coroutine">The coroutine to track.</param>
        public void TrackCoroutine(Coroutine coroutine)
        {
            memoryManager.TrackCoroutine(coroutine);
        }
        
        /// <summary>
        /// Stops a tracked coroutine.
        /// </summary>
        /// <param name="coroutine">The coroutine to stop.</param>
        /// <param name="monoBehaviour">The MonoBehaviour.</param>
        public void StopCoroutine(Coroutine coroutine, MonoBehaviour monoBehaviour)
        {
            memoryManager.StopCoroutine(coroutine, monoBehaviour);
        }
        
        /// <summary>
        /// Stops all tracked coroutines.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour.</param>
        public void StopAllCoroutines(MonoBehaviour monoBehaviour)
        {
            memoryManager.StopAllCoroutines(monoBehaviour);
        }
        
        /// <summary>
        /// Performs complete cleanup.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour.</param>
        public void CleanupAll(MonoBehaviour monoBehaviour)
        {
            memoryManager.CleanupAll(monoBehaviour);
        }
        
        /// <summary>
        /// Logs memory statistics.
        /// </summary>
        public void LogMemoryStats()
        {
            memoryManager.LogMemoryStats();
        }
        
        #endregion
        
        #region Event-Based Completion Tracking
        
        /// <summary>
        /// Checks if gravity has completed for a specific column.
        /// </summary>
        /// <param name="column">The column to check.</param>
        /// <returns>True if gravity has completed.</returns>
        public bool IsGravityCompleted(int column)
        {
            return gravityCompletionStatus.TryGetValue(column, out var completed) && completed;
        }
        
        /// <summary>
        /// Checks if refill has completed for a specific column.
        /// </summary>
        /// <param name="column">The column to check.</param>
        /// <returns>True if refill has completed.</returns>
        public bool IsRefillCompleted(int column)
        {
            return refillCompletionStatus.TryGetValue(column, out var completed) && completed;
        }
        
        /// <summary>
        /// Waits for gravity to complete for all columns.
        /// </summary>
        /// <param name="columns">The columns to wait for.</param>
        /// <returns>Task that completes when all gravity operations finish.</returns>
        public async Task WaitForGravityCompletion(int[] columns)
        {
            var startTime = Time.time;
            var timeout = 10f; // 10 second timeout
            
            while (Time.time - startTime < timeout)
            {
                bool allCompleted = true;
                foreach (var column in columns)
                {
                    if (!IsGravityCompleted(column))
                    {
                        allCompleted = false;
                        break;
                    }
                }
                
                if (allCompleted)
                {
                    Debug.Log("[Match3FoundationManager] ✅ All gravity operations completed");
                    return;
                }
                
                await Task.Delay(50); // Check every 50ms
            }
            
            Debug.LogWarning("[Match3FoundationManager] ⚠️ Gravity completion timeout");
        }
        
        /// <summary>
        /// Waits for refill to complete for all columns.
        /// </summary>
        /// <param name="columns">The columns to wait for.</param>
        /// <returns>Task that completes when all refill operations finish.</returns>
        public async Task WaitForRefillCompletion(int[] columns)
        {
            var startTime = Time.time;
            var timeout = 10f; // 10 second timeout
            
            while (Time.time - startTime < timeout)
            {
                bool allCompleted = true;
                foreach (var column in columns)
                {
                    if (!IsRefillCompleted(column))
                    {
                        allCompleted = false;
                        break;
                    }
                }
                
                if (allCompleted)
                {
                    Debug.Log("[Match3FoundationManager] ✅ All refill operations completed");
                    return;
                }
                
                await Task.Delay(50); // Check every 50ms
            }
            
            Debug.LogWarning("[Match3FoundationManager] ⚠️ Refill completion timeout");
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnGravityCompleted(GravityCompletedEvent gravityEvent)
        {
            gravityCompletionStatus[gravityEvent.Column] = true;
            Debug.Log($"[Match3FoundationManager] Gravity completed for column {gravityEvent.Column}");
        }
        
        private void OnRefillCompleted(RefillCompletedEvent refillEvent)
        {
            refillCompletionStatus[refillEvent.Column] = true;
            Debug.Log($"[Match3FoundationManager] Refill completed for column {refillEvent.Column}");
        }
        
        private void OnAllGravityCompleted(AllGravityCompletedEvent allGravityEvent)
        {
            Debug.Log($"[Match3FoundationManager] All gravity completed: {allGravityEvent.TotalMovedTiles} tiles in {allGravityEvent.TotalDuration:F2}s");
        }
        
        private void OnAllRefillCompleted(AllRefillCompletedEvent allRefillEvent)
        {
            Debug.Log($"[Match3FoundationManager] All refill completed: {allRefillEvent.TotalSpawnedTiles} tiles in {allRefillEvent.TotalDuration:F2}s");
        }
        
        #endregion
        
        /// <summary>
        /// Gets a summary of all foundation systems status.
        /// </summary>
        /// <returns>A status summary string.</returns>
        public string GetStatusSummary()
        {
            return $"[Match3FoundationManager] Status Summary:\n" +
                   $"  - Position Cache: {GetPositionCacheSize()} tiles\n" +
                   $"  - Active Animations: {animationManager.HasActiveAnimations()}\n" +
                   $"  - Memory Stats: {memoryManager.GetActiveCoroutineCount()} coroutines, {memoryManager.GetActiveSubscriptionCount()} subscriptions";
        }
    }
} 