using System;
using UnityEngine;
using MiniGameFramework.MiniGames.Match3.Data;

namespace MiniGameFramework.MiniGames.Match3.Visual.Strategies
{
    /// <summary>
    /// Strategy interface for Match3 animations.
    /// Allows different animation implementations to be swapped at runtime.
    /// </summary>
    public interface IAnimationStrategy
    {
        /// <summary>
        /// Gets the strategy name for identification.
        /// </summary>
        string StrategyName { get; }
        
        /// <summary>
        /// Gets whether this strategy is currently active.
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Initializes the animation strategy.
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Cleans up the animation strategy.
        /// </summary>
        void Cleanup();
        
        /// <summary>
        /// Animates a tile swap.
        /// </summary>
        /// <param name="tileA">First tile to animate.</param>
        /// <param name="tileB">Second tile to animate.</param>
        /// <param name="targetPosA">Target position for tile A.</param>
        /// <param name="targetPosB">Target position for tile B.</param>
        /// <param name="duration">Animation duration.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        void AnimateSwap(GameObject tileA, GameObject tileB, Vector3 targetPosA, Vector3 targetPosB, float duration, Action onComplete = null);
        
        /// <summary>
        /// Animates tile falling (gravity).
        /// </summary>
        /// <param name="tile">Tile to animate.</param>
        /// <param name="targetPos">Target position.</param>
        /// <param name="duration">Animation duration.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        void AnimateGravity(GameObject tile, Vector3 targetPos, float duration, Action onComplete = null);
        
        /// <summary>
        /// Animates tile spawning (refill).
        /// </summary>
        /// <param name="tile">Tile to animate.</param>
        /// <param name="startPos">Starting position.</param>
        /// <param name="targetPos">Target position.</param>
        /// <param name="duration">Animation duration.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        void AnimateSpawn(GameObject tile, Vector3 startPos, Vector3 targetPos, float duration, Action onComplete = null);
        
        /// <summary>
        /// Animates tile explosion (match).
        /// </summary>
        /// <param name="tile">Tile to animate.</param>
        /// <param name="duration">Animation duration.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        void AnimateExplosion(GameObject tile, float duration, Action onComplete = null);
        
        /// <summary>
        /// Animates invalid move feedback.
        /// </summary>
        /// <param name="tileA">First tile.</param>
        /// <param name="tileB">Second tile.</param>
        /// <param name="originalPosA">Original position of tile A.</param>
        /// <param name="originalPosB">Original position of tile B.</param>
        /// <param name="duration">Animation duration.</param>
        /// <param name="onComplete">Callback when animation completes.</param>
        void AnimateInvalidMove(GameObject tileA, GameObject tileB, Vector3 originalPosA, Vector3 originalPosB, float duration, Action onComplete = null);
        
        /// <summary>
        /// Stops all animations for a specific tile.
        /// </summary>
        /// <param name="tile">Tile to stop animations for.</param>
        void StopAnimations(GameObject tile);
        
        /// <summary>
        /// Stops all animations.
        /// </summary>
        void StopAllAnimations();
        
        /// <summary>
        /// Gets performance statistics for this strategy.
        /// </summary>
        /// <returns>Performance statistics string.</returns>
        string GetPerformanceStats();
    }
    
    /// <summary>
    /// Base class for animation strategies with common functionality.
    /// </summary>
    public abstract class AnimationStrategyBase : IAnimationStrategy
    {
        public abstract string StrategyName { get; }
        public bool IsActive { get; protected set; }
        
        protected int animationCount = 0;
        protected int completedAnimations = 0;
        protected float totalAnimationTime = 0f;
        
        public virtual void Initialize()
        {
            IsActive = true;
            animationCount = 0;
            completedAnimations = 0;
            totalAnimationTime = 0f;
            
            Debug.Log($"[{StrategyName}] ✅ Initialized");
        }
        
        public virtual void Cleanup()
        {
            IsActive = false;
            StopAllAnimations();
            
            Debug.Log($"[{StrategyName}] 🧹 Cleaned up");
        }
        
        public abstract void AnimateSwap(GameObject tileA, GameObject tileB, Vector3 targetPosA, Vector3 targetPosB, float duration, Action onComplete = null);
        public abstract void AnimateGravity(GameObject tile, Vector3 targetPos, float duration, Action onComplete = null);
        public abstract void AnimateSpawn(GameObject tile, Vector3 startPos, Vector3 targetPos, float duration, Action onComplete = null);
        public abstract void AnimateExplosion(GameObject tile, float duration, Action onComplete = null);
        public abstract void AnimateInvalidMove(GameObject tileA, GameObject tileB, Vector3 originalPosA, Vector3 originalPosB, float duration, Action onComplete = null);
        public abstract void StopAnimations(GameObject tile);
        public abstract void StopAllAnimations();
        
        public virtual string GetPerformanceStats()
        {
            var avgTime = animationCount > 0 ? totalAnimationTime / animationCount : 0f;
            var completionRate = animationCount > 0 ? (float)completedAnimations / animationCount * 100 : 0f;
            
            return $"[{StrategyName}] 📊 Animations: {animationCount}, Completed: {completedAnimations}, AvgTime: {avgTime:F3}s, CompletionRate: {completionRate:F1}%";
        }
        
        protected void TrackAnimation(float duration)
        {
            animationCount++;
            totalAnimationTime += duration;
        }
        
        protected void TrackCompletion()
        {
            completedAnimations++;
        }
    }
} 