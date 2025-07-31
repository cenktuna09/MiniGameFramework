using UnityEngine;
using MiniGameFramework.MiniGames.Match3.Data;

namespace MiniGameFramework.MiniGames.Match3.Visual
{
    /// <summary>
    /// Visual representation of a Match3 tile.
    /// Handles sprite assignment, animations, and visual feedback.
    /// </summary>
    public class TileVisual : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        
        [Header("Tile Sprites")]
        [SerializeField] private Sprite[] tileSprites = new Sprite[7]; // Index 0 = Empty, 1-6 = Colors
        
        [Header("Animation")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private TileData currentTileData;
        private Vector3 targetPosition;
        private Vector3 startPosition;
        private bool isMoving = false;
        private float totalDistance;
        
        /// <summary>
        /// Current tile data this visual represents.
        /// </summary>
        public TileData TileData => currentTileData;
        
        /// <summary>
        /// Whether this tile is currently moving.
        /// </summary>
        public bool IsMoving => isMoving;
        
        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (animator == null)
                animator = GetComponent<Animator>();
            
            // Validate components
            if (spriteRenderer == null)
            {
                Debug.LogError($"[TileVisual] No SpriteRenderer found on {gameObject.name}!");
            }
            
            if (animator != null && animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning($"[TileVisual] Animator found but no Controller assigned on {gameObject.name}. Animations may not work.");
            }
        }
        
        /// <summary>
        /// Sets the tile data and updates the visual representation.
        /// </summary>
        /// <param name="tileData">The tile data to display.</param>
        public void SetTileData(TileData tileData)
        {
            currentTileData = tileData;
            UpdateVisual();
        }
        
        /// <summary>
        /// Updates the tile type and sprite.
        /// </summary>
        /// <param name="tileType">The new tile type.</param>
        public void SetTileType(TileType tileType)
        {
            currentTileData = currentTileData.WithType(tileType);
            UpdateVisual();
        }
        
        /// <summary>
        /// Moves the tile to a target world position with animation.
        /// </summary>
        /// <param name="worldPosition">Target world position.</param>
        public void MoveTo(Vector3 worldPosition)
        {
            if (Vector3.Distance(transform.position, worldPosition) < 0.01f)
            {
                // Already at target position
                transform.position = worldPosition;
                isMoving = false;
                currentTileData = currentTileData.WithMoving(false);
                return;
            }
            
            startPosition = transform.position;
            targetPosition = worldPosition;
            totalDistance = Vector3.Distance(startPosition, targetPosition);
            isMoving = true;
            currentTileData = currentTileData.WithMoving(true);
            
            Debug.Log($"[TileVisual] MoveTo: {gameObject.name} {startPosition} -> {targetPosition} (distance: {totalDistance:F2})");
        }
        
        /// <summary>
        /// Sets the tile position immediately without animation.
        /// </summary>
        /// <param name="worldPosition">World position to set.</param>
        public void SetPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
            startPosition = worldPosition;
            targetPosition = worldPosition;
            totalDistance = 0f;
            isMoving = false;
            currentTileData = currentTileData.WithMoving(false);
        }
        
        /// <summary>
        /// Triggers the match animation for this tile.
        /// </summary>
        public void PlayMatchAnimation()
        {
            currentTileData = currentTileData.WithMatched(true);
            
            if (animator != null)
            {
                animator.SetTrigger("Match");
            }
        }
        
        /// <summary>
        /// Triggers the explosion/destroy animation.
        /// </summary>
        public void PlayDestroyAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger("Destroy");
            }
        }
        
        /// <summary>
        /// Resets the tile to its default state.
        /// </summary>
        public void ResetTile()
        {
            currentTileData = new TileData(TileType.Empty, Vector2Int.zero);
            isMoving = false;
            startPosition = transform.position;
            targetPosition = transform.position;
            totalDistance = 0f;
            
            if (spriteRenderer != null)
            {
                var color = spriteRenderer.color;
                color.a = 1.0f;
                spriteRenderer.color = color;
            }
            
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
            
            UpdateVisual();
        }
        
        private void Update()
        {
            if (isMoving)
            {
                HandleMovement();
            }
        }
        
        /// <summary>
        /// Handles smooth movement animation to target position.
        /// </summary>
        private void HandleMovement()
        {
            var currentDistance = Vector3.Distance(transform.position, targetPosition);
            
            if (currentDistance < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
                currentTileData = currentTileData.WithMoving(false);
                Debug.Log($"[TileVisual] ✅ Movement completed: {gameObject.name} -> {targetPosition}");
                return;
            }
            
            // Calculate progress: 0 (at start) to 1 (at target)
            var progress = Mathf.Clamp01(1.0f - (currentDistance / totalDistance));
            var curveValue = moveCurve.Evaluate(progress);
            var step = moveSpeed * Time.deltaTime * curveValue;
            
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            
            // Debug every few frames to avoid spam
            if (Time.frameCount % 60 == 0) // Reduced frequency
            {
                Debug.Log($"[TileVisual] 🔄 Moving {gameObject.name}: {transform.position} -> {targetPosition} (progress: {progress:F2}, distance: {currentDistance:F2})");
            }
        }
        
        /// <summary>
        /// Updates the visual representation based on current tile data.
        /// </summary>
        private void UpdateVisual()
        {
            if (spriteRenderer == null) return;
            
            var tileTypeIndex = (int)currentTileData.Type;
            
            if (tileTypeIndex >= 0 && tileTypeIndex < tileSprites.Length && tileSprites[tileTypeIndex] != null)
            {
                spriteRenderer.sprite = tileSprites[tileTypeIndex];
                spriteRenderer.enabled = currentTileData.Type != TileType.Empty;
            }
            else
            {
                spriteRenderer.enabled = false;
            }
        }
        
        /// <summary>
        /// Called when the tile animation completes.
        /// This method is called from Animation Events.
        /// </summary>
        public void OnAnimationComplete()
        {
            // Animation complete callback
        }
    }
}