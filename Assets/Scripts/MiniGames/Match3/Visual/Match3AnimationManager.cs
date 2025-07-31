using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.MiniGames.Match3.Events;
using MiniGameFramework.MiniGames.Match3.Data;
using MiniGameFramework.MiniGames.Match3.Logic;

namespace MiniGameFramework.MiniGames.Match3.Visual
{
    /// <summary>
    /// Centralized animation manager for Match3 game.
    /// Handles all animations with proper event integration and completion tracking.
    /// </summary>
    public class Match3AnimationManager
    {
        private readonly IEventBus eventBus;
        private readonly float tileSize;
        private readonly float swapDuration;
        private readonly float gravityDuration;
        private readonly float matchAnimationDuration;
        
        // Animation tracking
        private readonly Dictionary<int, List<GameObject>> activeAnimations = new Dictionary<int, List<GameObject>>();
        private readonly Dictionary<GameObject, Action> animationCallbacks = new Dictionary<GameObject, Action>();
        
        public Match3AnimationManager(IEventBus eventBus, float tileSize, float swapDuration, float gravityDuration, float matchAnimationDuration)
        {
            this.eventBus = eventBus;
            this.tileSize = tileSize;
            this.swapDuration = swapDuration;
            this.gravityDuration = gravityDuration;
            this.matchAnimationDuration = matchAnimationDuration;
        }
        
        /// <summary>
        /// Animates a tile swap with LeanTween and event integration.
        /// </summary>
        /// <param name="swap">The swap to animate.</param>
        /// <param name="visualTiles">The visual tiles array.</param>
        /// <returns>Task that completes when animation finishes.</returns>
        public async Task AnimateSwap(Swap swap, GameObject[,] visualTiles)
        {
            var visualA = visualTiles[swap.tileA.x, swap.tileA.y];
            var visualB = visualTiles[swap.tileB.x, swap.tileB.y];
            
            if (visualA == null || visualB == null)
            {
                Debug.LogError("[Match3AnimationManager] ❌ One or both visual tiles are null!");
                return;
            }
            
            Debug.Log($"[Match3AnimationManager] 🎬 Starting swap animation: {swap.tileA} ↔ {swap.tileB}");
            
            // Calculate target positions
            var posA = new Vector3(swap.tileB.x * tileSize, swap.tileB.y * tileSize, 0);
            var posB = new Vector3(swap.tileA.x * tileSize, swap.tileA.y * tileSize, 0);
            
            // Bring animated tiles to front
            var spriteRendererA = visualA.GetComponent<SpriteRenderer>();
            var spriteRendererB = visualB.GetComponent<SpriteRenderer>();
            
            if (spriteRendererA != null) spriteRendererA.sortingOrder = 1;
            if (spriteRendererB != null) spriteRendererB.sortingOrder = 1;
            
            // Track animations
            var animations = new List<GameObject> { visualA, visualB };
            var completedAnimations = 0;
            var totalAnimations = 2;
            
            // Start LeanTween animations
            LeanTween.move(visualA, posA, swapDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() => {
                    completedAnimations++;
                    if (spriteRendererA != null) spriteRendererA.sortingOrder = 0;
                    
                    if (completedAnimations >= totalAnimations)
                    {
                        Debug.Log("[Match3AnimationManager] ✅ Swap animation completed");
                    }
                });
            
            LeanTween.move(visualB, posB, swapDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() => {
                    completedAnimations++;
                    if (spriteRendererB != null) spriteRendererB.sortingOrder = 0;
                    
                    if (completedAnimations >= totalAnimations)
                    {
                        Debug.Log("[Match3AnimationManager] ✅ Swap animation completed");
                    }
                });
            
            // Wait for animations to complete
            await Task.Delay((int)(swapDuration * 1000) + 100); // Extra 100ms buffer
        }
        
        /// <summary>
        /// Animates gravity for a single column with event integration.
        /// </summary>
        /// <param name="column">The column to animate.</param>
        /// <param name="tilesToMove">List of tiles to move with their positions.</param>
        /// <param name="visualTiles">The visual tiles array.</param>
        /// <returns>Task that completes when gravity animation finishes.</returns>
        public async Task AnimateGravity(int column, List<(int fromY, int toY, TileData tile, GameObject visual)> tilesToMove, GameObject[,] visualTiles)
        {
            if (tilesToMove.Count == 0)
            {
                Debug.Log($"[Match3AnimationManager] No tiles to move in column {column}");
                return;
            }
            
            Debug.Log($"[Match3AnimationManager] 🎬 Starting gravity animation for column {column}: {tilesToMove.Count} tiles");
            
            // Publish gravity started event
            eventBus?.Publish(new GravityStartedEvent(column, tilesToMove.Count, this));
            
            var startTime = Time.time;
            var completedAnimations = 0;
            var totalAnimations = tilesToMove.Count;
            
            // Track animations for this column
            activeAnimations[column] = new List<GameObject>();
            
            foreach (var (fromY, toY, tile, visual) in tilesToMove)
            {
                if (visual == null) continue;
                
                // Publish tile movement started event
                eventBus?.Publish(new TileMovementStartedEvent(column, new Vector2Int(column, fromY), new Vector2Int(column, toY), visual, this));
                
                var targetPos = new Vector3(column * tileSize, toY * tileSize, 0);
                
                // Add to active animations
                activeAnimations[column].Add(visual);
                
                // Start LeanTween animation
                var tween = LeanTween.move(visual, targetPos, gravityDuration);
                tween.setEase(LeanTweenType.easeInQuad);
                tween.setOnComplete(() => {
                    completedAnimations++;
                    
                    // Publish tile movement completed event
                    var duration = Time.time - startTime;
                    eventBus?.Publish(new TileMovementCompletedEvent(column, new Vector2Int(column, fromY), new Vector2Int(column, toY), visual, duration, this));
                    
                    // Remove from active animations
                    activeAnimations[column].Remove(visual);
                    
                    Debug.Log($"[Match3AnimationManager] ✅ Gravity animation {completedAnimations}/{totalAnimations} completed for column {column}");
                    
                    // Check if all animations are done
                    if (completedAnimations >= totalAnimations)
                    {
                        var totalDuration = Time.time - startTime;
                        eventBus?.Publish(new GravityCompletedEvent(column, totalAnimations, totalDuration, this));
                        Debug.Log($"[Match3AnimationManager] 🎯 All {totalAnimations} gravity animations completed for column {column}!");
                    }
                });
            }
            
            // Wait for all animations to complete
            await Task.Delay((int)(gravityDuration * 1000) + 200); // Extra 200ms buffer
        }
        
        /// <summary>
        /// Animates refill for a single column with event integration.
        /// </summary>
        /// <param name="column">The column to refill.</param>
        /// <param name="tilesToSpawn">List of tiles to spawn with their positions.</param>
        /// <param name="visualTiles">The visual tiles array.</param>
        /// <returns>Task that completes when refill animation finishes.</returns>
        public async Task AnimateRefill(int column, List<(int y, TileData tile)> tilesToSpawn, GameObject[,] visualTiles)
        {
            if (tilesToSpawn.Count == 0)
            {
                Debug.Log($"[Match3AnimationManager] No tiles to spawn in column {column}");
                return;
            }
            
            Debug.Log($"[Match3AnimationManager] 🎬 Starting refill animation for column {column}: {tilesToSpawn.Count} tiles");
            
            // Publish refill started event
            eventBus?.Publish(new RefillStartedEvent(column, tilesToSpawn.Count, this));
            
            var startTime = Time.time;
            var completedSpawns = 0;
            var totalSpawns = tilesToSpawn.Count;
            
            // Small delay to ensure gravity is completely finished
            await Task.Delay(100);
            
            foreach (var (y, tileData) in tilesToSpawn)
            {
                // Create visual tile (this should be done by the game logic)
                // For now, we'll assume the tile is already created
                var tileObject = visualTiles[column, y];
                if (tileObject == null) continue;
                
                // Calculate spawn position (above the board)
                var spawnPos = new Vector3(column * tileSize, 8 * tileSize, 0); // Assuming board height is 8
                var targetPos = new Vector3(column * tileSize, y * tileSize, 0);
                
                // Position tile above board and animate falling
                tileObject.transform.position = spawnPos;
                
                // Start LeanTween falling animation
                var tween = LeanTween.move(tileObject, targetPos, gravityDuration);
                tween.setEase(LeanTweenType.easeInQuad);
                tween.setOnComplete(() => {
                    completedSpawns++;
                    Debug.Log($"[Match3AnimationManager] ✅ Refill animation {completedSpawns}/{totalSpawns} completed for column {column}");
                    
                    // Check if all spawns are done
                    if (completedSpawns >= totalSpawns)
                    {
                        var totalDuration = Time.time - startTime;
                        eventBus?.Publish(new RefillCompletedEvent(column, totalSpawns, totalDuration, this));
                        Debug.Log($"[Match3AnimationManager] 🎯 All {totalSpawns} refill animations completed for column {column}!");
                    }
                });
            }
            
            // Wait for all spawn animations to complete
            await Task.Delay((int)(gravityDuration * 1000) + 200); // Extra 200ms buffer
        }
        
        /// <summary>
        /// Animates matched tiles before removal.
        /// </summary>
        /// <param name="matches">List of matches to animate.</param>
        /// <param name="visualTiles">The visual tiles array.</param>
        /// <returns>Task that completes when match animations finish.</returns>
        public async Task AnimateMatches(List<MatchDetector.Match> matches, GameObject[,] visualTiles)
        {
            if (matches == null || matches.Count == 0)
            {
                Debug.Log("[Match3AnimationManager] No matches to animate");
                return;
            }
            
            Debug.Log($"[Match3AnimationManager] 🎬 Starting match animations for {matches.Count} matches");
            
            var animatedTiles = new List<GameObject>();
            
            foreach (var match in matches)
            {
                foreach (var position in match.Positions)
                {
                    var visualTile = visualTiles[position.x, position.y];
                    if (visualTile != null)
                    {
                        animatedTiles.Add(visualTile);
                        
                        // Play match animation (pulse effect)
                        var originalScale = visualTile.transform.localScale;
                        LeanTween.scale(visualTile, originalScale * 1.2f, matchAnimationDuration * 0.5f)
                            .setEase(LeanTweenType.easeInOutQuad)
                            .setOnComplete(() => {
                                LeanTween.scale(visualTile, originalScale, matchAnimationDuration * 0.5f)
                                    .setEase(LeanTweenType.easeInOutQuad);
                            });
                    }
                }
            }
            
            // Wait for match animations to complete
            await Task.Delay((int)(matchAnimationDuration * 1000));
            
            Debug.Log($"[Match3AnimationManager] ✅ Match animations completed for {animatedTiles.Count} tiles");
        }
        
        /// <summary>
        /// Shows invalid move animation.
        /// </summary>
        /// <param name="tileA">First tile.</param>
        /// <param name="tileB">Second tile.</param>
        /// <returns>Task that completes when animation finishes.</returns>
        public async Task ShowInvalidMoveAnimation(GameObject tileA, GameObject tileB)
        {
            if (tileA == null || tileB == null) return;
            
            Debug.Log("[Match3AnimationManager] ❌ Showing invalid move animation");
            
            var posA = tileA.transform.position;
            var posB = tileB.transform.position;
            
            // Bring tiles to front for animation
            var spriteRendererA = tileA.GetComponent<SpriteRenderer>();
            var spriteRendererB = tileB.GetComponent<SpriteRenderer>();
            
            if (spriteRendererA != null) spriteRendererA.sortingOrder = 1;
            if (spriteRendererB != null) spriteRendererB.sortingOrder = 1;
            
            // Move to swap positions (faster than valid moves)
            LeanTween.move(tileA, posB, 0.2f);
            LeanTween.move(tileB, posA, 0.2f)
                .setOnComplete(() => {
                    // Return to original positions
                    LeanTween.move(tileA, posA, 0.2f)
                        .setOnComplete(() => {
                            if (spriteRendererA != null) spriteRendererA.sortingOrder = 0;
                            if (spriteRendererB != null) spriteRendererB.sortingOrder = 0;
                        });
                    LeanTween.move(tileB, posB, 0.2f);
                });
            
            // Wait for animation to complete
            await Task.Delay(600); // 0.2f + 0.2f + 0.2f = 0.6s total
        }
        
        /// <summary>
        /// Checks if any animations are currently active.
        /// </summary>
        /// <returns>True if animations are active, false otherwise.</returns>
        public bool HasActiveAnimations()
        {
            foreach (var animations in activeAnimations.Values)
            {
                if (animations.Count > 0) return true;
            }
            return false;
        }
        
        /// <summary>
        /// Gets the number of active animations for a specific column.
        /// </summary>
        /// <param name="column">The column to check.</param>
        /// <returns>The number of active animations.</returns>
        public int GetActiveAnimationCount(int column)
        {
            return activeAnimations.TryGetValue(column, out var animations) ? animations.Count : 0;
        }
        
        /// <summary>
        /// Stops all active animations.
        /// </summary>
        public void StopAllAnimations()
        {
            foreach (var animations in activeAnimations.Values)
            {
                foreach (var tile in animations)
                {
                    if (tile != null)
                    {
                        LeanTween.cancel(tile);
                    }
                }
                animations.Clear();
            }
            
            Debug.Log("[Match3AnimationManager] 🛑 All animations stopped");
        }
        
        /// <summary>
        /// Stops animations for a specific column.
        /// </summary>
        /// <param name="column">The column to stop animations for.</param>
        public void StopAnimationsForColumn(int column)
        {
            if (activeAnimations.TryGetValue(column, out var animations))
            {
                foreach (var tile in animations)
                {
                    if (tile != null)
                    {
                        LeanTween.cancel(tile);
                    }
                }
                animations.Clear();
                
                Debug.Log($"[Match3AnimationManager] 🛑 Animations stopped for column {column}");
            }
        }
    }
} 