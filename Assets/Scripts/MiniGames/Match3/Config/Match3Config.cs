using UnityEngine;

namespace MiniGameFramework.MiniGames.Match3.Config
{
    /// <summary>
    /// Centralized configuration for Match-3 game settings.
    /// Uses ScriptableObject for easy adjustment in Unity Inspector and runtime modification.
    /// </summary>
    [CreateAssetMenu(fileName = "Match3Config", menuName = "MiniGameFramework/Match3/Config")]
    public class Match3Config : ScriptableObject
    {
        #region Board Settings
        
        [Header("Board Settings")]
        [Tooltip("Width of the game board")]
        public int boardWidth = 8;
        
        [Tooltip("Height of the game board")]
        public int boardHeight = 8;
        
        [Tooltip("Tile size in world units")]
        public float tileSize = 1f;
        
        [Tooltip("Spacing between tiles")]
        public float tileSpacing = 0.1f;
        
        #endregion
        
        #region Animation Settings
        
        [Header("Animation Settings")]
        [Tooltip("Duration of swap animations in seconds")]
        [Range(0.1f, 2f)]
        public float swapDuration = 0.3f;
        
        [Tooltip("Duration of gravity animations in seconds")]
        [Range(0.1f, 2f)]
        public float gravityDuration = 0.4f;
        
        [Tooltip("Duration of refill animations in seconds")]
        [Range(0.1f, 2f)]
        public float refillDuration = 0.5f;
        
        [Tooltip("Duration of match destruction animations in seconds")]
        [Range(0.1f, 2f)]
        public float matchDestructionDuration = 0.2f;
        
        [Tooltip("Duration of invalid move animations in seconds")]
        [Range(0.1f, 2f)]
        public float invalidMoveDuration = 0.3f;
        
        [Tooltip("Easing type for swap animations")]
        public LeanTweenType swapEasing = LeanTweenType.easeInOutQuad;
        
        [Tooltip("Easing type for gravity animations")]
        public LeanTweenType gravityEasing = LeanTweenType.easeInQuad;
        
        [Tooltip("Easing type for refill animations")]
        public LeanTweenType refillEasing = LeanTweenType.easeOutBounce;
        
        #endregion
        
        #region Gameplay Settings
        
        [Header("Gameplay Settings")]
        [Tooltip("Points awarded per tile in a match")]
        [Range(10, 1000)]
        public int pointsPerTile = 100;
        
        [Tooltip("Bonus multiplier for longer matches")]
        [Range(1f, 3f)]
        public float matchLengthMultiplier = 1.5f;
        
        [Tooltip("Minimum tiles required for a match")]
        [Range(3, 5)]
        public int minMatchLength = 3;
        
        [Tooltip("Maximum tiles that can be matched")]
        [Range(5, 10)]
        public int maxMatchLength = 8;
        
        [Tooltip("Delay before showing hints in seconds")]
        [Range(1f, 30f)]
        public float hintDelay = 5f;
        
        [Tooltip("Duration of hint animations in seconds")]
        [Range(0.5f, 3f)]
        public float hintDuration = 1f;
        
        [Tooltip("Number of hints available per game")]
        [Range(0, 10)]
        public int maxHints = 3;
        
        #endregion
        
        #region Performance Settings
        
        [Header("Performance Settings")]
        [Tooltip("Maximum number of tiles to pool")]
        [Range(50, 500)]
        public int maxPooledTiles = 200;
        
        [Tooltip("Enable object pooling for better performance")]
        public bool enableObjectPooling = true;
        
        [Tooltip("Enable position caching for O(1) lookups")]
        public bool enablePositionCache = true;
        
        [Tooltip("Enable event-based gravity completion")]
        public bool enableEventBasedGravity = true;
        
        [Tooltip("Enable memory leak prevention")]
        public bool enableMemoryLeakPrevention = true;
        
        #endregion
        
        #region Visual Settings
        
        [Header("Visual Settings")]
        [Tooltip("Scale factor for invalid move animations")]
        [Range(0.5f, 2f)]
        public float invalidMoveScale = 1.2f;
        
        [Tooltip("Color for invalid move animations")]
        public Color invalidMoveColor = Color.red;
        
        [Tooltip("Scale factor for hint animations")]
        [Range(0.5f, 2f)]
        public float hintScale = 1.1f;
        
        [Tooltip("Color for hint animations")]
        public Color hintColor = Color.yellow;
        
        [Tooltip("Enable particle effects for matches")]
        public bool enableParticleEffects = true;
        
        [Tooltip("Enable screen shake for matches")]
        public bool enableScreenShake = true;
        
        #endregion
        
        #region Audio Settings
        
        [Header("Audio Settings")]
        [Tooltip("Volume for swap sound effects")]
        [Range(0f, 1f)]
        public float swapVolume = 0.7f;
        
        [Tooltip("Volume for match sound effects")]
        [Range(0f, 1f)]
        public float matchVolume = 0.8f;
        
        [Tooltip("Volume for invalid move sound effects")]
        [Range(0f, 1f)]
        public float invalidMoveVolume = 0.5f;
        
        [Tooltip("Volume for hint sound effects")]
        [Range(0f, 1f)]
        public float hintVolume = 0.6f;
        
        #endregion
        
        #region Debug Settings
        
        [Header("Debug Settings")]
        [Tooltip("Enable debug logging")]
        public bool enableDebugLogging = true;
        
        [Tooltip("Enable performance monitoring")]
        public bool enablePerformanceMonitoring = false;
        
        [Tooltip("Enable detailed error reporting")]
        public bool enableDetailedErrorReporting = true;
        
        [Tooltip("Enable validation checks")]
        public bool enableValidationChecks = true;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Validates the configuration settings and logs warnings for invalid values.
        /// </summary>
        public void ValidateSettings()
        {
            if (boardWidth < 3 || boardHeight < 3)
            {
                Debug.LogWarning("[Match3Config] ⚠️ Board size too small. Minimum 3x3 recommended.");
            }
            
            if (boardWidth > 12 || boardHeight > 12)
            {
                Debug.LogWarning("[Match3Config] ⚠️ Board size very large. May impact performance.");
            }
            
            if (swapDuration < 0.1f || gravityDuration < 0.1f)
            {
                Debug.LogWarning("[Match3Config] ⚠️ Animation durations very short. May cause visual glitches.");
            }
            
            if (pointsPerTile < 10)
            {
                Debug.LogWarning("[Match3Config] ⚠️ Points per tile very low. May not be engaging.");
            }
            
            if (minMatchLength < 3)
            {
                Debug.LogWarning("[Match3Config] ⚠️ Minimum match length too low. Standard is 3.");
            }
            
            Debug.Log("[Match3Config] ✅ Configuration validation completed");
        }
        
        /// <summary>
        /// Gets the board dimensions as a Vector2Int.
        /// </summary>
        /// <returns>Board dimensions (width, height)</returns>
        public Vector2Int GetBoardDimensions()
        {
            return new Vector2Int(boardWidth, boardHeight);
        }
        
        /// <summary>
        /// Gets the total board size (width * height).
        /// </summary>
        /// <returns>Total number of tiles on the board</returns>
        public int GetTotalBoardSize()
        {
            return boardWidth * boardHeight;
        }
        
        /// <summary>
        /// Calculates points for a match of given length.
        /// </summary>
        /// <param name="matchLength">Number of tiles in the match</param>
        /// <returns>Points awarded for the match</returns>
        public int CalculateMatchPoints(int matchLength)
        {
            if (matchLength < minMatchLength)
                return 0;
                
            float multiplier = 1f;
            if (matchLength > minMatchLength)
            {
                multiplier = matchLengthMultiplier * (matchLength - minMatchLength + 1);
            }
            
            return Mathf.RoundToInt(pointsPerTile * matchLength * multiplier);
        }
        
        /// <summary>
        /// Checks if a match length is valid according to configuration.
        /// </summary>
        /// <param name="matchLength">Length to validate</param>
        /// <returns>True if the match length is valid</returns>
        public bool IsValidMatchLength(int matchLength)
        {
            return matchLength >= minMatchLength && matchLength <= maxMatchLength;
        }
        
        /// <summary>
        /// Gets the world position for a board position.
        /// </summary>
        /// <param name="boardPos">Board position (x, y)</param>
        /// <returns>World position</returns>
        public Vector3 GetWorldPosition(Vector2Int boardPos)
        {
            float x = boardPos.x * (tileSize + tileSpacing);
            float y = boardPos.y * (tileSize + tileSpacing);
            return new Vector3(x, y, 0);
        }
        
        /// <summary>
        /// Gets the board position from a world position.
        /// </summary>
        /// <param name="worldPos">World position</param>
        /// <returns>Board position (x, y)</returns>
        public Vector2Int GetBoardPosition(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / (tileSize + tileSpacing));
            int y = Mathf.RoundToInt(worldPos.y / (tileSize + tileSpacing));
            return new Vector2Int(x, y);
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void OnValidate()
        {
            // Ensure minimum values
            boardWidth = Mathf.Max(3, boardWidth);
            boardHeight = Mathf.Max(3, boardHeight);
            tileSize = Mathf.Max(0.1f, tileSize);
            tileSpacing = Mathf.Max(0f, tileSpacing);
            
            // Ensure animation durations are reasonable
            swapDuration = Mathf.Max(0.1f, swapDuration);
            gravityDuration = Mathf.Max(0.1f, gravityDuration);
            refillDuration = Mathf.Max(0.1f, refillDuration);
            matchDestructionDuration = Mathf.Max(0.1f, matchDestructionDuration);
            invalidMoveDuration = Mathf.Max(0.1f, invalidMoveDuration);
            
            // Ensure gameplay values are reasonable
            pointsPerTile = Mathf.Max(1, pointsPerTile);
            minMatchLength = Mathf.Max(3, minMatchLength);
            maxMatchLength = Mathf.Max(minMatchLength, maxMatchLength);
            hintDelay = Mathf.Max(0f, hintDelay);
            maxHints = Mathf.Max(0, maxHints);
            
            // Ensure performance values are reasonable
            maxPooledTiles = Mathf.Max(10, maxPooledTiles);
            
            // Ensure visual values are reasonable
            invalidMoveScale = Mathf.Max(0.1f, invalidMoveScale);
            hintScale = Mathf.Max(0.1f, hintScale);
            
            // Ensure audio values are in range
            swapVolume = Mathf.Clamp01(swapVolume);
            matchVolume = Mathf.Clamp01(matchVolume);
            invalidMoveVolume = Mathf.Clamp01(invalidMoveVolume);
            hintVolume = Mathf.Clamp01(hintVolume);
        }
        
        #endregion
    }
} 