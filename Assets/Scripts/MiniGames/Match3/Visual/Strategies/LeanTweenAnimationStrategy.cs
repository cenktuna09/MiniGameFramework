using System;
using System.Collections.Generic;
using UnityEngine;
using MiniGameFramework.MiniGames.Match3.Data;

namespace MiniGameFramework.MiniGames.Match3.Visual.Strategies
{
    /// <summary>
    /// LeanTween-based animation strategy for optimal performance.
    /// Uses LeanTween for smooth, efficient animations with minimal GC allocations.
    /// </summary>
    public class LeanTweenAnimationStrategy : AnimationStrategyBase
    {
        public override string StrategyName => "LeanTween";
        
        private readonly Dictionary<GameObject, List<LTDescr>> activeAnimations;
        private readonly Dictionary<GameObject, Vector3> originalPositions;
        
        public LeanTweenAnimationStrategy()
        {
            activeAnimations = new Dictionary<GameObject, List<LTDescr>>();
            originalPositions = new Dictionary<GameObject, Vector3>();
        }
        
        public override void Initialize()
        {
            base.Initialize();
            activeAnimations.Clear();
            originalPositions.Clear();
            
            Debug.Log("[LeanTweenAnimationStrategy] 🚀 LeanTween strategy initialized");
        }
        
        public override void Cleanup()
        {
            StopAllAnimations();
            base.Cleanup();
        }
        
        public override void AnimateSwap(GameObject tileA, GameObject tileB, Vector3 targetPosA, Vector3 targetPosB, float duration, Action onComplete = null)
        {
            if (!IsActive || tileA == null || tileB == null) return;
            
            TrackAnimation(duration);
            
            // Store original positions
            StoreOriginalPosition(tileA);
            StoreOriginalPosition(tileB);
            
            // Bring tiles to front for animation
            SetSortingOrder(tileA, 1);
            SetSortingOrder(tileB, 1);
            
            var completedAnimations = 0;
            var totalAnimations = 2;
            
            Action checkCompletion = () =>
            {
                completedAnimations++;
                if (completedAnimations >= totalAnimations)
                {
                    SetSortingOrder(tileA, 0);
                    SetSortingOrder(tileB, 0);
                    TrackCompletion();
                    onComplete?.Invoke();
                }
            };
            
            // Animate tile A
            var tweenA = LeanTween.move(tileA, targetPosA, duration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(checkCompletion);
            
            // Animate tile B
            var tweenB = LeanTween.move(tileB, targetPosB, duration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(checkCompletion);
            
            // Track animations
            TrackAnimationForObject(tileA, tweenA);
            TrackAnimationForObject(tileB, tweenB);
            
            Debug.Log($"[LeanTweenAnimationStrategy] 🎬 Swap animation started: {tileA.name} ↔ {tileB.name}");
        }
        
        public override void AnimateGravity(GameObject tile, Vector3 targetPos, float duration, Action onComplete = null)
        {
            if (!IsActive || tile == null) return;
            
            TrackAnimation(duration);
            
            var tween = LeanTween.move(tile, targetPos, duration)
                .setEase(LeanTweenType.easeInQuad)
                .setOnComplete(() =>
                {
                    TrackCompletion();
                    onComplete?.Invoke();
                });
            
            TrackAnimationForObject(tile, tween);
            
            Debug.Log($"[LeanTweenAnimationStrategy] 🌍 Gravity animation: {tile.name} → {targetPos}");
        }
        
        public override void AnimateSpawn(GameObject tile, Vector3 startPos, Vector3 targetPos, float duration, Action onComplete = null)
        {
            if (!IsActive || tile == null) return;
            
            TrackAnimation(duration);
            
            // Set initial position
            tile.transform.position = startPos;
            
            var tween = LeanTween.move(tile, targetPos, duration)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() =>
                {
                    TrackCompletion();
                    onComplete?.Invoke();
                });
            
            TrackAnimationForObject(tile, tween);
            
            Debug.Log($"[LeanTweenAnimationStrategy] 🎬 Spawn animation: {tile.name} {startPos} → {targetPos}");
        }
        
        public override void AnimateExplosion(GameObject tile, float duration, Action onComplete = null)
        {
            if (!IsActive || tile == null) return;
            
            TrackAnimation(duration);
            
            // Scale and fade out animation
            var originalScale = tile.transform.localScale;
            var originalAlpha = GetSpriteAlpha(tile);
            
            var tween = LeanTween.scale(tile, Vector3.zero, duration)
                .setEase(LeanTweenType.easeInBack)
                .setOnComplete(() =>
                {
                    // Restore original scale and alpha
                    tile.transform.localScale = originalScale;
                    SetSpriteAlpha(tile, originalAlpha);
                    TrackCompletion();
                    onComplete?.Invoke();
                });
            
            // Fade out
            LeanTween.alpha(tile, 0f, duration)
                .setEase(LeanTweenType.easeInQuad);
            
            TrackAnimationForObject(tile, tween);
            
            Debug.Log($"[LeanTweenAnimationStrategy] 💥 Explosion animation: {tile.name}");
        }
        
        public override void AnimateInvalidMove(GameObject tileA, GameObject tileB, Vector3 originalPosA, Vector3 originalPosB, float duration, Action onComplete = null)
        {
            if (!IsActive || tileA == null || tileB == null) return;
            
            TrackAnimation(duration);
            
            // Store original positions
            StoreOriginalPosition(tileA);
            StoreOriginalPosition(tileB);
            
            // Bring tiles to front
            SetSortingOrder(tileA, 1);
            SetSortingOrder(tileB, 1);
            
            var completedAnimations = 0;
            var totalAnimations = 4; // Move to swap positions, then back
            
            Action checkCompletion = () =>
            {
                completedAnimations++;
                if (completedAnimations >= totalAnimations)
                {
                    SetSortingOrder(tileA, 0);
                    SetSortingOrder(tileB, 0);
                    TrackCompletion();
                    onComplete?.Invoke();
                }
            };
            
            // Move to swap positions (faster)
            var swapDuration = duration * 0.4f;
            var returnDuration = duration * 0.6f;
            
            // Move to swap positions
            var tweenA1 = LeanTween.move(tileA, originalPosB, swapDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(checkCompletion);
            
            var tweenB1 = LeanTween.move(tileB, originalPosA, swapDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(checkCompletion);
            
            // Return to original positions
            var tweenA2 = LeanTween.move(tileA, originalPosA, returnDuration)
                .setDelay(swapDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(checkCompletion);
            
            var tweenB2 = LeanTween.move(tileB, originalPosB, returnDuration)
                .setDelay(swapDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(checkCompletion);
            
            // Track animations
            TrackAnimationForObject(tileA, tweenA1);
            TrackAnimationForObject(tileA, tweenA2);
            TrackAnimationForObject(tileB, tweenB1);
            TrackAnimationForObject(tileB, tweenB2);
            
            Debug.Log($"[LeanTweenAnimationStrategy] ❌ Invalid move animation: {tileA.name} ↔ {tileB.name}");
        }
        
        public override void StopAnimations(GameObject tile)
        {
            if (tile == null || !activeAnimations.ContainsKey(tile)) return;
            
            var animations = activeAnimations[tile];
            foreach (var animation in animations)
            {
                if (animation != null)
                {
                    LeanTween.cancel(animation.id);
                }
            }
            
            activeAnimations.Remove(tile);
            Debug.Log($"[LeanTweenAnimationStrategy] ⏹️ Stopped animations for: {tile.name}");
        }
        
        public override void StopAllAnimations()
        {
            foreach (var kvp in activeAnimations)
            {
                foreach (var animation in kvp.Value)
                {
                    if (animation != null)
                    {
                        LeanTween.cancel(animation.id);
                    }
                }
            }
            
            activeAnimations.Clear();
            Debug.Log("[LeanTweenAnimationStrategy] ⏹️ Stopped all animations");
        }
        
        #region Private Methods
        
        private void TrackAnimationForObject(GameObject obj, LTDescr tween)
        {
            if (!activeAnimations.ContainsKey(obj))
            {
                activeAnimations[obj] = new List<LTDescr>();
            }
            
            activeAnimations[obj].Add(tween);
        }
        
        private void StoreOriginalPosition(GameObject obj)
        {
            if (!originalPositions.ContainsKey(obj))
            {
                originalPositions[obj] = obj.transform.position;
            }
        }
        
        private void SetSortingOrder(GameObject obj, int order)
        {
            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = order;
            }
        }
        
        private float GetSpriteAlpha(GameObject obj)
        {
            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
            return spriteRenderer != null ? spriteRenderer.color.a : 1f;
        }
        
        private void SetSpriteAlpha(GameObject obj, float alpha)
        {
            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                var color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }
        }
        
        #endregion
    }
} 