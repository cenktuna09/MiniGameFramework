using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.Core.DI;
using MiniGameFramework.MiniGames.Match3.Data;
using MiniGameFramework.MiniGames.Match3.Board;
using MiniGameFramework.MiniGames.Match3.Pooling;
using MiniGameFramework.MiniGames.Match3.Logic;
using MiniGameFramework.MiniGames.Match3.Visual;
using MiniGameFramework.MiniGames.Match3.Utils;
using MiniGameFramework.MiniGames.Match3.Input;
using MiniGameFramework.MiniGames.Match3.Config;
using MiniGameFramework.MiniGames.Match3.ErrorHandling;

namespace MiniGameFramework.MiniGames.Match3
{
    /// <summary>
    /// Represents a possible swap between two tiles
    /// </summary>
    [System.Serializable]
    public struct Swap
    {
        public Vector2Int tileA;
        public Vector2Int tileB;
        
        public Swap(Vector2Int a, Vector2Int b)
        {
            tileA = a;
            tileB = b;
        }
        
        public bool IsValid => tileA != tileB;
    }
    
    /// <summary>
    /// Clean Match3 game implementation with constraint-based generation,
    /// possible swaps detection, cascade effects, and smooth animations.
    /// </summary>
    public class Match3Game : MiniGameBase
    {
        [Header("Match3 Configuration")]
        [SerializeField] private Transform boardParent;
        [SerializeField] private TilePool tilePool;
        [SerializeField] private float tileSize = 1.0f;
        [SerializeField] private int pointsPerTile = 100;
        
        [Header("Animation Settings")]
        [SerializeField] private float swapDuration = 0.3f;
        [SerializeField] private float gravityDuration = 0.4f;
        [SerializeField] private float matchAnimationDuration = 0.5f;
        
        [Header("Gameplay Settings")]
        [SerializeField] private float hintDelay = 5f; // Hint after 5 seconds of inactivity
        
        // # Core Game State
        private BoardData currentBoard;
        private GameObject[,] visualTiles;
        private IEventBus eventBus;
        private int currentScore = 0;
        
        // # Foundation Systems (Week 1-2)
        private Match3FoundationManager foundationManager;
        
        // Input system
        private Match3InputManager inputManager;
        
        // # Week 3-4: Game Logic and Configuration
        private Match3GameLogicManager gameLogicManager;
        private Match3Config gameConfig;
        private Match3ErrorHandler errorHandler;
        
        // Game state tracking
        private bool isSwapping = false;
        private bool isProcessingMatches = false;
        
        // Game state tracking
        private List<Swap> possibleSwaps = new List<Swap>();
        private Coroutine hintCoroutine;
        

        
        public override bool IsPlayable => currentState == GameState.Ready || currentState == GameState.Playing;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Set default values if not assigned
            if (string.IsNullOrEmpty(gameId))
                gameId = "Match3";
            
            if (string.IsNullOrEmpty(displayName))
                displayName = "Match 3 Game";
                
            Debug.Log("[Match3Game] Awake completed");
        }
        
        /// <summary>
        /// Unity Start - manually initialize the game
        /// </summary>
        private new async void Start()
        {
            Debug.Log("[Match3Game] Start called - beginning initialization");
            
            // Wait a frame to ensure all services are ready
            await Task.Yield();
            
            // Initialize the game
            await InitializeAsync();
        }
        
        protected override async Task OnInitializeAsync()
        {
            Debug.Log("[Match3Game] 🎮 Initializing Clean Match3 System...");
            try
            {
                // # 1. Resolve Dependencies
                eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
                if (eventBus == null)
                {
                    throw new InvalidOperationException("EventBus is required for Match3Game");
                }
                
                // # 2. Initialize Foundation Systems (Week 1-2)
                InitializeFoundationSystems();
                
                // # 3. Initialize Components  
                await InitializeComponents();
                
                // # 3. Generate Constraint-Based Board
                GenerateConstraintBasedBoard();
                
                // # 4. Calculate Initial Possible Swaps
                DetectPossibleSwaps();
                
                Debug.Log($"[Match3Game] ✅ Initialization complete! Found {possibleSwaps.Count} possible swaps");
                
                // # 5. Auto-start game (FIXED)
                await Task.Yield();
                Debug.Log($"[Match3Game] Current state after init: {currentState}");
                if (currentState == GameState.Ready)
                {
                    Debug.Log("[Match3Game] �� Auto-starting game...");
                    SetState(GameState.Playing);
                    OnStart();
                    Debug.Log($"[Match3Game] State after auto-start: {currentState}");
                }
                else
                {
                    Debug.LogWarning($"[Match3Game] Cannot auto-start in state: {currentState}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Match3Game] ❌ Initialization failed: {e.Message}");
                throw;
            }
        }
        
        protected override void OnStart()
        {
            Debug.Log("[Match3Game] 🚀 Starting Match3 game...");
            
            // Initialize game state
            currentScore = 0;
            
            // Unlock input
            inputManager?.UnlockInput();
            
            // Start hint timer
            StartHintTimer();
            
            Debug.Log("[Match3Game] ✅ Game started successfully");
        }
        
        protected override void OnPause()
        {
            Debug.Log("[Match3Game] ⏸️ Pausing Match3 game...");
            
            // Lock input
            inputManager?.LockInput();
            
            // Stop hint timer
            StopHintTimer();
            
            Debug.Log("[Match3Game] ✅ Game paused");
        }
        
        protected override void OnResume()
        {
            Debug.Log("[Match3Game] ▶️ Resuming Match3 game...");
            
            // Unlock input
            inputManager?.UnlockInput();
            
            // Restart hint timer
            RestartHintTimer();
            
            Debug.Log("[Match3Game] ✅ Game resumed");
        }
        
        protected override void OnEnd()
        {
            Debug.Log("[Match3Game] 🏁 Ending Match3 game...");
            
            // Lock input
            inputManager?.LockInput();
            
            // Stop hint timer
            StopHintTimer();
            
            // Force deselect any selected tile
            inputManager?.ForceDeselect();
            
            Debug.Log("[Match3Game] ✅ Game ended");
        }
        
        protected override void OnCleanup()
        {
            Debug.Log("[Match3Game] Cleaning up resources");
            
            // Cleanup foundation systems
            if (foundationManager != null)
            {
                foundationManager.CleanupAll(this);
                foundationManager.LogMemoryStats();
            }
            
            // Stop hint timer
            StopHintTimer();
            
            // Clear visual board
            ClearVisualBoard();
            
            Debug.Log("[Match3Game] ✅ Cleanup completed");
        }
        
        public override int GetCurrentScore()
        {
            return currentScore;
        }

        /// <summary>
        /// Unity Update - handles input and game flow
        /// </summary>
        private void Update()
        {
            if (currentState != GameState.Playing) return;
            
            // Handle input
            HandleInput();
        }

        /// <summary>
        /// Handles input using the new input manager system.
        /// </summary>
        private void HandleInput()
        {
            if (inputManager == null) return;
            
            var inputResult = inputManager.ProcessInput(isProcessingMatches, isSwapping);
            
            // Handle tile selection
            if (inputResult.TileSelected)
            {
                RestartHintTimer(); // Reset hint timer on player activity
            }
            
            // Handle valid swap
            if (inputResult.SwapDetected && inputResult.IsValidSwap)
            {
                StartCoroutine(ProcessSwapWithLeanTween(inputResult.DetectedSwap));
            }
            
            // Handle invalid swap
            if (inputResult.SwapDetected && !inputResult.IsValidSwap && inputResult.InvalidSwapTiles.HasValue)
            {
                var invalidTiles = inputResult.InvalidSwapTiles.Value;
                ShowInvalidMoveAnimation(invalidTiles.tileA, invalidTiles.tileB);
            }
        }

        #region Core Game Systems
        
        /// <summary>
        /// Detects all possible swaps that would create matches
        /// </summary>
        private void DetectPossibleSwaps()
        {
            // Use game logic manager for swap detection
            if (gameLogicManager != null)
            {
                gameLogicManager.UpdateBoard(currentBoard);
                possibleSwaps = gameLogicManager.GetPossibleSwaps();
                inputManager?.UpdatePossibleSwaps(possibleSwaps);
                
                Debug.Log($"[Match3Game] 🔍 Found {possibleSwaps.Count} possible swaps (via GameLogicManager)");
            }
            else
            {
                // Fallback to original implementation
                possibleSwaps.Clear();
                
                Debug.Log("[Match3Game] 🔍 Starting possible swaps detection (fallback)...");
                
                for (int x = 0; x < currentBoard.Width; x++)
                {
                    for (int y = 0; y < currentBoard.Height; y++)
                    {
                        var currentTile = currentBoard.GetTile(x, y);
                        if (!currentTile.IsValid) continue;
                        
                        // Check right neighbor
                        if (x < currentBoard.Width - 1)
                        {
                            var swap = new Swap(new Vector2Int(x, y), new Vector2Int(x + 1, y));
                            if (WouldSwapCreateMatch(swap))
                            {
                                possibleSwaps.Add(swap);
                                Debug.Log($"[Match3Game] ✅ Found valid swap: {swap.tileA} ↔ {swap.tileB}");
                            }
                        }
                        
                        // Check down neighbor  
                        if (y < currentBoard.Height - 1)
                        {
                            var swap = new Swap(new Vector2Int(x, y), new Vector2Int(x, y + 1));
                            if (WouldSwapCreateMatch(swap))
                            {
                                possibleSwaps.Add(swap);
                                Debug.Log($"[Match3Game] ✅ Found valid swap: {swap.tileA} ↔ {swap.tileB}");
                            }
                        }
                    }
                }
                
                Debug.Log($"[Match3Game] 🔍 Found {possibleSwaps.Count} possible swaps (fallback)");
                inputManager?.UpdatePossibleSwaps(possibleSwaps);
            }
        }
        
        /// <summary>
        /// Checks if a swap would create any matches
        /// </summary>
        private bool WouldSwapCreateMatch(Swap swap)
        {
            // Simulate the swap
            var tileA = currentBoard.GetTile(swap.tileA);
            var tileB = currentBoard.GetTile(swap.tileB);
            
            var simulatedBoard = currentBoard
                .SetTile(swap.tileA, tileB.WithPosition(swap.tileA))
                .SetTile(swap.tileB, tileA.WithPosition(swap.tileB));
            
            var matches = MatchDetector.FindMatches(simulatedBoard);
            bool wouldCreateMatch = matches.Count > 0;
            
            Debug.Log($"[Match3Game] 🔍 Testing swap {swap.tileA} ↔ {swap.tileB}: " +
                     $"TileA={tileA.Type}, TileB={tileB.Type}, " +
                     $"WouldCreateMatch={wouldCreateMatch}, MatchesFound={matches.Count}");
            
            return wouldCreateMatch;
        }
        
        /// <summary>
        /// Generates a constraint-based board that prevents initial matches
        /// </summary>
        private void GenerateConstraintBasedBoard()
        {
            Debug.Log("[Match3Game] 🎲 Generating constraint-based board...");
            
            // Use error handler for safe execution
            errorHandler?.SafeExecute(() =>
            {
                // Generate board ensuring no initial matches
                currentBoard = BoardGenerator.GenerateBoard();
                
                // Create visual representation
                CreateVisualBoard();
                
                // Initialize position cache with visual tiles
                foundationManager?.InitializePositionCache(visualTiles);
                
                // Initialize game logic manager with new board
                gameLogicManager?.InitializeBoard(currentBoard);
                
                Debug.Log("[Match3Game] ✅ Constraint-based board generated successfully");
            }, "Board generation");
        }
        
        #endregion
        
        #region Hint System
        
        /// <summary>
        /// Starts the hint timer
        /// </summary>
        private void StartHintTimer()
        {
            StopHintTimer();
            if (hintDelay > 0)
            {
                hintCoroutine = StartCoroutine(HintTimerCoroutine());
            }
        }
        
        /// <summary>
        /// Stops the hint timer
        /// </summary>
        private void StopHintTimer()
        {
            if (hintCoroutine != null)
            {
                StopCoroutine(hintCoroutine);
                hintCoroutine = null;
            }
        }
        
        /// <summary>
        /// Restarts the hint timer (called on player activity)
        /// </summary>
        private void RestartHintTimer()
        {
            StartHintTimer();
        }
        
        /// <summary>
        /// Hint timer coroutine
        /// </summary>
        private IEnumerator HintTimerCoroutine()
        {
            yield return new WaitForSeconds(hintDelay);
            
            if (possibleSwaps.Count > 0 && currentState == GameState.Playing)
            {
                ShowRandomHint();
            }
        }
        
        /// <summary>
        /// Shows a random hint to the player
        /// </summary>
        private void ShowRandomHint()
        {
            if (possibleSwaps.Count == 0) return;
            
            var randomSwap = possibleSwaps[UnityEngine.Random.Range(0, possibleSwaps.Count)];
            StartCoroutine(HighlightSwap(randomSwap));
            
            Debug.Log($"[Match3Game] 💡 Showing hint: {randomSwap.tileA} ↔ {randomSwap.tileB}");
        }
        
        /// <summary>
        /// Highlights a swap with visual effects
        /// </summary>
        private IEnumerator HighlightSwap(Swap swap)
        {
            var tileA = visualTiles[swap.tileA.x, swap.tileA.y];
            var tileB = visualTiles[swap.tileB.x, swap.tileB.y];
            
            if (tileA == null || tileB == null) yield break;
            
            var spriteA = tileA.GetComponent<SpriteRenderer>();
            var spriteB = tileB.GetComponent<SpriteRenderer>();
            
            // Highlight with green color for 2 seconds
            var originalColorA = spriteA.color;
            var originalColorB = spriteB.color;
            
            spriteA.color = Color.green;
            spriteB.color = Color.green;
            
            yield return new WaitForSeconds(1.5f);
            
            spriteA.color = originalColorA;
            spriteB.color = originalColorB;
            
            // Restart hint timer for next hint
            StartHintTimer();
        }
        
        #endregion
        
        #region Clean Cascade System
        
        /// <summary>
        /// Processes a tile swap with LeanTween animations and cascade effects
        /// </summary>
        private IEnumerator ProcessSwapWithLeanTween(Swap swap)
        {
            isSwapping = true;
            StopHintTimer();
            
            Debug.Log($"[Match3Game] 🔄 Processing swap with LeanTween: {swap.tileA} ↔ {swap.tileB}");
            
            // # 1. Animate tile swap with LeanTween
            yield return StartCoroutine(AnimateSwapWithLeanTween(swap));
            
            // # 2. Update board data
            SwapTilesInBoard(swap);
            
            // # 3. Handle cascade matches
            yield return StartCoroutine(HandleCascadeMatches());
            
            // # 4. Recalculate possible swaps
            DetectPossibleSwaps();
            
            // # 5. Check game over
            if (possibleSwaps.Count == 0)
            {
                Debug.Log("[Match3Game] ⚠️ No more possible swaps - Game Over!");
                End();
            }
            else
            {
                StartHintTimer(); // Restart hint timer
            }
            
            isSwapping = false;
            Debug.Log("[Match3Game] ✅ Swap processing completed");
        }
        
        /// <summary>
        /// Processes a tile swap with smooth animation and cascade effects (legacy)
        /// </summary>
        private IEnumerator ProcessSwap(Swap swap)
        {
            isSwapping = true;
            StopHintTimer();
            
            Debug.Log($"[Match3Game] 🔄 Processing swap: {swap.tileA} ↔ {swap.tileB}");
            
            // # 1. Animate tile swap with LeanTween
            yield return StartCoroutine(AnimateSwap(swap));
            
            // # 2. Update board data
            SwapTilesInBoard(swap);
            
            // # 3. Handle cascade matches
            yield return StartCoroutine(HandleCascadeMatches());
            
            // # 4. Recalculate possible swaps
            DetectPossibleSwaps();
            
            // # 5. Check game over
            if (possibleSwaps.Count == 0)
            {
                Debug.Log("[Match3Game] ⚠️ No more possible swaps - Game Over!");
                End();
            }
            else
            {
                StartHintTimer(); // Restart hint timer
            }
            
            isSwapping = false;
            Debug.Log("[Match3Game] ✅ Swap processing completed");
        }
        
        /// <summary>
        /// Smoothly animates tile swap with LeanTween for better performance
        /// </summary>
        private IEnumerator AnimateSwapWithLeanTween(Swap swap)
        {
            Debug.Log($"[Match3Game] 🎬 Starting LeanTween swap animation: {swap.tileA} ↔ {swap.tileB}");
            
            var visualA = visualTiles[swap.tileA.x, swap.tileA.y];
            var visualB = visualTiles[swap.tileB.x, swap.tileB.y];
            
            if (visualA == null || visualB == null)
            {
                Debug.LogError("[Match3Game] ❌ One or both visual tiles are null!");
                yield break;
            }
            
            // Store references for completion callback
            var tileACopy = visualA;
            var tileBCopy = visualB;
            
            // Calculate target positions
            var posA = new Vector3(swap.tileB.x * tileSize, swap.tileB.y * tileSize, 0);
            var posB = new Vector3(swap.tileA.x * tileSize, swap.tileA.y * tileSize, 0);
            
            Debug.Log($"[Match3Game] 🎯 Moving A to {posA}, B to {posB}");
            
            // Bring animated tile to front
            var spriteRendererA = visualA.GetComponent<SpriteRenderer>();
            if (spriteRendererA != null)
            {
                spriteRendererA.sortingOrder = 1;
            }
            
            // Start LeanTween animations
            LeanTween.move(visualA, posA, swapDuration)
                .setOnComplete(() => {
                    Debug.Log("[Match3Game] ✅ Tile A animation completed");
                    if (spriteRendererA != null)
                    {
                        spriteRendererA.sortingOrder = 0;
                    }
                });
            
            LeanTween.move(visualB, posB, swapDuration);
            
            // Wait for animations to complete
            yield return new WaitForSeconds(swapDuration);
            
            // Swap visual references AFTER animation
            visualTiles[swap.tileA.x, swap.tileA.y] = visualB;
            visualTiles[swap.tileB.x, swap.tileB.y] = visualA;
            
            // Update position cache
            if (visualA != null)
            {
                foundationManager?.UpdateTilePosition(visualA, swap.tileB);
            }
            if (visualB != null)
            {
                foundationManager?.UpdateTilePosition(visualB, swap.tileA);
            }
            
            Debug.Log("[Match3Game] ✅ LeanTween swap animation completed");
        }
        
        /// <summary>
        /// Shows invalid move animation with LeanTween
        /// </summary>
        private void ShowInvalidMoveAnimation(GameObject tileA, GameObject tileB)
        {
            Debug.Log("[Match3Game] ❌ Showing invalid move animation");
            
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
            
            // TODO: Play error sound
            // SoundManager.instance.PlaySound("Error");
        }
        
        /// <summary>
        /// Smoothly animates tile swap with proper reference handling (legacy)
        /// </summary>
        private IEnumerator AnimateSwap(Swap swap)
        {
            Debug.Log($"[Match3Game] 🎬 Starting swap animation: {swap.tileA} ↔ {swap.tileB}");
            
            var visualA = visualTiles[swap.tileA.x, swap.tileA.y];
            var visualB = visualTiles[swap.tileB.x, swap.tileB.y];
            
            if (visualA == null || visualB == null)
            {
                Debug.LogError("[Match3Game] ❌ One or both visual tiles are null!");
                yield break;
            }
            
            var tileVisualA = visualA.GetComponent<TileVisual>();
            var tileVisualB = visualB.GetComponent<TileVisual>();
            
            // Calculate target positions
            var posA = new Vector3(swap.tileB.x * tileSize, swap.tileB.y * tileSize, 0);
            var posB = new Vector3(swap.tileA.x * tileSize, swap.tileA.y * tileSize, 0);
            
            Debug.Log($"[Match3Game] 🎯 Moving A to {posA}, B to {posB}");
            
            // Start animations
            tileVisualA?.MoveTo(posA);
            tileVisualB?.MoveTo(posB);
            
            // Wait for animations with timeout
            float timeout = 3f;
            float elapsed = 0f;
            
            Debug.Log($"[Match3Game] 🕐 Waiting for animations... (timeout: {timeout}s)");
            
            while (elapsed < timeout)
            {
                bool aMoving = tileVisualA?.IsMoving == true;
                bool bMoving = tileVisualB?.IsMoving == true;
                
                Debug.Log($"[Match3Game] 🔄 Animation status - A: {aMoving}, B: {bMoving}, Elapsed: {elapsed:F2}s");
                
                if (!aMoving && !bMoving)
                {
                    Debug.Log("[Match3Game] ✅ Both animations completed");
                    break;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (elapsed >= timeout)
            {
                Debug.LogWarning("[Match3Game] ⚠️ Animation timeout - forcing completion");
                // Force completion
                if (tileVisualA != null) tileVisualA.transform.position = posA;
                if (tileVisualB != null) tileVisualB.transform.position = posB;
            }
            
            // Swap visual references AFTER animation
            visualTiles[swap.tileA.x, swap.tileA.y] = visualB;
            visualTiles[swap.tileB.x, swap.tileB.y] = visualA;
            
            Debug.Log($"[Match3Game] ✅ Swap animation completed in {elapsed:F2}s");
        }
        
        /// <summary>
        /// Swaps tiles in board data
        /// </summary>
        private void SwapTilesInBoard(Swap swap)
        {
            var tileA = currentBoard.GetTile(swap.tileA);
            var tileB = currentBoard.GetTile(swap.tileB);
            
            currentBoard = currentBoard
                .SetTile(swap.tileA, tileB.WithPosition(swap.tileA))
                .SetTile(swap.tileB, tileA.WithPosition(swap.tileB));
        }
        
        /// <summary>
        /// Handles cascade matches with recursive processing
        /// </summary>
        private IEnumerator HandleCascadeMatches()
        {
            Debug.Log("[Match3Game] 🎯 Starting cascade match processing...");
            isProcessingMatches = true;
            int cascadeCount = 0;
            
            while (true)
            {
                var matches = MatchDetector.FindMatches(currentBoard);
                Debug.Log($"[Match3Game] 🔍 Checking for matches... Found: {matches.Count}");
                
                if (matches.Count == 0) 
                {
                    Debug.Log("[Match3Game] ✅ No more matches found - cascade complete");
                    break;
                }
                
                cascadeCount++;
                Debug.Log($"[Match3Game] 💥 Cascade {cascadeCount}: Found {matches.Count} matches");
                
                // # Calculate score with cascade bonus
                int matchScore = CalculateMatchScore(matches, cascadeCount);
                currentScore += matchScore;
                Debug.Log($"[Match3Game] 💰 Score: +{matchScore} (Total: {currentScore})");
                
                // # Animate matched tiles
                Debug.Log("[Match3Game] 🎬 Animating matched tiles...");
                yield return StartCoroutine(AnimateMatches(matches));
                
                // # Explode tiles (remove from board and pool)
                Debug.Log("[Match3Game] 💣 Exploding tiles...");
                ExplodeTiles(matches);
                
                // # Apply gravity and refill
                Debug.Log("[Match3Game] 🌍 Applying gravity and refilling...");
                yield return StartCoroutine(ApplyGravityAndRefill());
                
                Debug.Log($"[Match3Game] 🎯 Cascade {cascadeCount} complete! Score: +{matchScore}");
            }
            
            isProcessingMatches = false;
            Debug.Log($"[Match3Game] ✅ All cascades completed! Total cascades: {cascadeCount}");
        }
        
        /// <summary>
        /// Calculates score for matches with cascade bonuses
        /// </summary>
        private int CalculateMatchScore(List<MatchDetector.Match> matches, int cascadeMultiplier)
        {
            int baseScore = 0;
            foreach (var match in matches)
            {
                baseScore += match.Length * pointsPerTile;
            }
            
            // Apply cascade bonus (2x, 3x, 4x, etc.)
            return baseScore * cascadeMultiplier;
        }
        
        /// <summary>
        /// Explodes (removes) matched tiles from board and returns to pool
        /// </summary>
        private void ExplodeTiles(List<MatchDetector.Match> matches)
        {
            var explodedTiles = new List<GameObject>();
            
            foreach (var match in matches)
            {
                foreach (var position in match.Positions)
                {
                    // # Update board data
                    var emptyTile = new TileData(TileType.Empty, position);
                    currentBoard = currentBoard.SetTile(position, emptyTile);
                    
                    // # Return visual tile to pool
                    var visualTile = visualTiles[position.x, position.y];
                    if (visualTile != null)
                    {
                        explodedTiles.Add(visualTile);
                        tilePool.ReturnTile(visualTile);
                        visualTiles[position.x, position.y] = null;
                        
                        // TODO: Play VFX explosion effect here
                    }
                    
                    // Publish match event
                    eventBus?.Publish(new MatchFoundEvent(match.Positions, match.TileType));
                }
            }
            
            Debug.Log($"[Match3Game] 💣 Exploded {explodedTiles.Count} tiles");
        }
        
        #endregion
        
        /// <summary>
        /// Manual start button for testing (Inspector button)
        /// </summary>
        [ContextMenu("Start Match3 Game")]
        public void StartGame()
        {
            Debug.Log($"[Match3Game] StartGame called, current state: {currentState}");
            
            if (currentState == GameState.Ready)
            {
                Debug.Log("[Match3Game] Transitioning from Ready to Playing...");
                SetState(GameState.Playing);
                OnStart();
                Debug.Log($"[Match3Game] State after transition: {currentState}");
            }
            else
            {
                Debug.LogWarning($"[Match3Game] Cannot start in current state: {currentState}");
            }
        }
        
        /// <summary>
        /// Show random hint (for testing)
        /// </summary>
        [ContextMenu("Show Hint")]
        public void ShowHint()
        {
            if (possibleSwaps.Count > 0)
            {
                ShowRandomHint();
            }
            else
            {
                Debug.LogWarning("[Match3Game] No possible swaps to hint!");
            }
        }
        
        /// <summary>
        /// Force recalculate possible swaps (for debugging)  
        /// </summary>
        [ContextMenu("Recalculate Possible Swaps")]
        public void RecalculatePossibleSwaps()
        {
            DetectPossibleSwaps();
            Debug.Log($"[Match3Game] 🔍 Recalculated: {possibleSwaps.Count} possible swaps");
        }
        
        [ContextMenu("Test Gravity")]
        public void TestGravity()
        {
            Debug.Log("[Match3Game] 🧪 Testing gravity...");
            StartCoroutine(ApplyGravityAndRefill());
        }
        
        [ContextMenu("Debug Board State")]
        public void DebugBoardState()
        {
            Debug.Log("[Match3Game] 📊 Current board state:");
            for (int y = currentBoard.Height - 1; y >= 0; y--)
            {
                var row = "";
                for (int x = 0; x < currentBoard.Width; x++)
                {
                    var tile = currentBoard.GetTile(x, y);
                    row += tile.IsValid ? tile.Type.ToString()[0] : ".";
                }
                Debug.Log($"[Match3Game] Row {y}: {row}");
            }
        }
        
        [ContextMenu("Create Test Scenario")]
        public void CreateTestScenario()
        {
            Debug.Log("[Match3Game] 🧪 Creating test scenario with empty spaces...");
            
            // Test senaryosu: Bazı tile'ları boş yap
            for (int x = 0; x < currentBoard.Width; x++)
            {
                for (int y = 0; y < currentBoard.Height; y++)
                {
                    if ((x == 2 && y == 3) || (x == 3 && y == 4) || (x == 4 && y == 5))
                    {
                        // Bu pozisyonları boş yap
                        currentBoard = currentBoard.SetTile(x, y, new TileData(TileType.Empty, new Vector2Int(x, y)));
                        
                        // Visual tile'ı da kaldır
                        if (visualTiles[x, y] != null)
                        {
                            tilePool.ReturnTile(visualTiles[x, y]);
                            visualTiles[x, y] = null;
                        }
                    }
                }
            }
            
            Debug.Log("[Match3Game] ✅ Test scenario created - some tiles removed");
            DebugBoardState();
        }
        
        [ContextMenu("Create Bottom Match Test")]
        public void CreateBottomMatchTest()
        {
            Debug.Log("[Match3Game] 🧪 Creating bottom match test scenario...");
            
            // Alt kısımda üçlü match oluştur (soldan sağa, yukarıdan aşağıya)
            var positionsToExplode = new List<Vector2Int>
            {
                new Vector2Int(0, 0), // Sol alt köşe
                new Vector2Int(1, 0), // Orta alt
                new Vector2Int(2, 0), // Sağ alt
                new Vector2Int(0, 1), // Sol orta
                new Vector2Int(1, 1), // Orta orta
                new Vector2Int(2, 1), // Sağ orta
                new Vector2Int(0, 2), // Sol üst
                new Vector2Int(1, 2), // Orta üst
                new Vector2Int(2, 2)  // Sağ üst
            };
            
            Debug.Log($"[Match3Game] 💥 Exploding {positionsToExplode.Count} tiles in bottom match pattern...");
            
            foreach (var pos in positionsToExplode)
            {
                // Board data'yı güncelle
                currentBoard = currentBoard.SetTile(pos.x, pos.y, new TileData(TileType.Empty, pos));
                
                // Visual tile'ı kaldır
                if (visualTiles[pos.x, pos.y] != null)
                {
                    tilePool.ReturnTile(visualTiles[pos.x, pos.y]);
                    visualTiles[pos.x, pos.y] = null;
                }
                
                Debug.Log($"[Match3Game] 💥 Exploded tile at ({pos.x},{pos.y})");
            }
            
            Debug.Log("[Match3Game] ✅ Bottom match test scenario created");
            DebugBoardState();
        }
        
        [ContextMenu("Create Cascade Test")]
        public void CreateCascadeTest()
        {
            Debug.Log("[Match3Game] 🧪 Creating cascade test scenario...");
            
            // Daha büyük bir alan explode et (cascade effect'i görmek için)
            var positionsToExplode = new List<Vector2Int>
            {
                // Column 0: Alt kısımda büyük boşluk
                new Vector2Int(0, 0), // En alt
                new Vector2Int(0, 1), // Alt orta
                new Vector2Int(0, 2), // Üst orta
                // Column 1: Orta kısımda boşluk
                new Vector2Int(1, 1), // Orta
                new Vector2Int(1, 2), // Üst
                // Column 2: Üst kısımda boşluk
                new Vector2Int(2, 3), // Üst
                new Vector2Int(2, 4), // En üst
            };
            
            Debug.Log($"[Match3Game] 💥 Exploding {positionsToExplode.Count} tiles for cascade test...");
            
            foreach (var pos in positionsToExplode)
            {
                // Board data'yı güncelle
                currentBoard = currentBoard.SetTile(pos.x, pos.y, new TileData(TileType.Empty, pos));
                
                // Visual tile'ı kaldır
                if (visualTiles[pos.x, pos.y] != null)
                {
                    tilePool.ReturnTile(visualTiles[pos.x, pos.y]);
                    visualTiles[pos.x, pos.y] = null;
                }
                
                Debug.Log($"[Match3Game] 💥 Exploded tile at ({pos.x},{pos.y})");
            }
            
            Debug.Log("[Match3Game] ✅ Cascade test scenario created");
            DebugBoardState();
        }
        
        [ContextMenu("Test Gravity and Show Result")]
        public void TestGravityAndShowResult()
        {
            Debug.Log("[Match3Game] 🧪 Testing gravity and showing result...");
            StartCoroutine(TestGravityCoroutine());
        }
        
        private IEnumerator TestGravityCoroutine()
        {
            Debug.Log("[Match3Game] 📊 BEFORE GRAVITY:");
            DebugBoardState();
            
            yield return StartCoroutine(ApplyGravityAndRefill());
            
            Debug.Log("[Match3Game] 📊 AFTER GRAVITY:");
            DebugBoardState();
        }
        
        [ContextMenu("Debug Visual Tiles")]
        public void DebugVisualTiles()
        {
            Debug.Log("[Match3Game] 🔍 Visual Tiles State:");
            for (int y = currentBoard.Height - 1; y >= 0; y--)
            {
                var row = "";
                for (int x = 0; x < currentBoard.Width; x++)
                {
                    var visual = visualTiles[x, y];
                    var boardTile = currentBoard.GetTile(x, y);
                    row += $"{(visual != null ? "V" : "X")}{(boardTile.IsValid ? "B" : "E")} ";
                }
                Debug.Log($"[Match3Game] Row {y}: {row}");
            }
        }

        #region Private Methods
        
        /// <summary>
        /// Initializes foundation systems (Week 1-2) and new components (Week 3-4).
        /// </summary>
        private void InitializeFoundationSystems()
        {
            Debug.Log("[Match3Game] 🔧 Initializing foundation systems...");
            foundationManager = new Match3FoundationManager(
                eventBus,
                tileSize,
                swapDuration,
                gravityDuration,
                matchAnimationDuration
            );
            
            // Initialize input manager
            inputManager = new Match3InputManager(
                eventBus,
                foundationManager,
                tileSize,
                swapDuration
            );
            
            // Initialize game logic manager (Week 3-4)
            gameLogicManager = new Match3GameLogicManager(eventBus);
            
            // Initialize configuration (Week 3-4)
            gameConfig = ScriptableObject.CreateInstance<Match3Config>();
            gameConfig.boardWidth = 8;
            gameConfig.boardHeight = 8;
            gameConfig.tileSize = tileSize;
            gameConfig.swapDuration = swapDuration;
            gameConfig.gravityDuration = gravityDuration;
            gameConfig.pointsPerTile = pointsPerTile;
            gameConfig.hintDelay = hintDelay;
            gameConfig.ValidateSettings();
            
            // Initialize error handler (Week 3-4)
            errorHandler = new Match3ErrorHandler(eventBus, true, true, 100);
            
            Debug.Log("[Match3Game] ✅ Foundation systems and new components initialized");
        }
        
        /// <summary>
        /// Initializes all required components
        /// </summary>
        private async Task InitializeComponents()
        {
            Debug.Log("[Match3Game] 🔧 Initializing components...");
            
            // # Find and initialize tile pool
            if (tilePool == null)
            {
                tilePool = GetComponentInChildren<TilePool>();
                if (tilePool == null)
                {
                    throw new InvalidOperationException("TilePool is required");
                }
            }
            
            tilePool.Initialize();
            
            // # Setup board parent
            if (boardParent == null)
            {
                var boardObject = new GameObject("Board");
                boardObject.transform.SetParent(transform);
                boardParent = boardObject.transform;
            }
            
            // # Initialize visual tiles array
            visualTiles = new GameObject[BoardData.BOARD_SIZE, BoardData.BOARD_SIZE];
            
            Debug.Log("[Match3Game] ✅ Components initialized successfully");
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Creates clean visual board representation
        /// </summary>
        private void CreateVisualBoard()
        {
            ClearVisualBoard();
            
            for (int x = 0; x < currentBoard.Width; x++)
            {
                for (int y = 0; y < currentBoard.Height; y++)
                {
                    var tileData = currentBoard.GetTile(x, y);
                    CreateVisualTile(tileData, x, y);
                }
            }
        }
        
        /// <summary>
        /// Creates a single visual tile at the specified position
        /// </summary>
        private void CreateVisualTile(TileData tileData, int x, int y)
        {
            var tileObject = tilePool.GetTile();
            var tileVisual = tileObject.GetComponent<TileVisual>();
            
            if (tileVisual != null)
            {
                tileVisual.SetTileData(tileData);
                var worldPos = new Vector3(x * tileSize, y * tileSize, 0);
                tileVisual.SetPosition(worldPos);
            }
            
            tileObject.transform.SetParent(boardParent);
            visualTiles[x, y] = tileObject;
        }
        
        /// <summary>
        /// Clears all visual tiles and returns them to pool
        /// </summary>
        private void ClearVisualBoard()
        {
            if (visualTiles == null) return;
            
            for (int x = 0; x < visualTiles.GetLength(0); x++)
            {
                for (int y = 0; y < visualTiles.GetLength(1); y++)
                {
                    if (visualTiles[x, y] != null)
                    {
                        tilePool.ReturnTile(visualTiles[x, y]);
                        visualTiles[x, y] = null;
                    }
                }
            }
        }
        /// <summary>
        /// Animates matched tiles before removal
        /// </summary>
        private IEnumerator AnimateMatches(List<MatchDetector.Match> matches)
        {
            foreach (var match in matches)
            {
                foreach (var position in match.Positions)
                {
                    var visualTile = visualTiles[position.x, position.y];
                    if (visualTile != null)
                    {
                        var tileVisual = visualTile.GetComponent<TileVisual>();
                        tileVisual?.PlayMatchAnimation();
                    }
                }
            }
            
            yield return new WaitForSeconds(matchAnimationDuration);
        }
        
        /// <summary>
        /// Optimized gravity and refill system with proper timing and delayed refill
        /// </summary>
        private IEnumerator ApplyGravityAndRefill()
        {
            Debug.Log("[Match3Game] 🌍 Starting optimized gravity and refill process...");
            
            // # PHASE 1: Apply gravity to all columns simultaneously
            Debug.Log("[Match3Game] ⬇️ Applying gravity to all columns in parallel...");
            var gravityCoroutines = new List<Coroutine>();
            
            for (int x = 0; x < currentBoard.Width; x++)
            {
                gravityCoroutines.Add(StartCoroutine(ApplyGravityToColumn(x)));
            }
            
            // Wait for ALL gravity animations to complete before refill
            Debug.Log("[Match3Game] ⏳ Waiting for gravity animations to complete...");
            yield return new WaitForSeconds(gravityDuration + 0.3f); // Extra buffer for safety
            
            // # PHASE 2: Verify all tiles have fallen before refill
            Debug.Log("[Match3Game] 🔍 Verifying gravity completion...");
            yield return StartCoroutine(VerifyGravityCompletion());
            
            // # PHASE 3: Refill all columns simultaneously (DELAYED)
            Debug.Log("[Match3Game] 🔄 Starting DELAYED refill for all columns...");
            var refillCoroutines = new List<Coroutine>();
            
            for (int x = 0; x < currentBoard.Width; x++)
            {
                refillCoroutines.Add(StartCoroutine(RefillColumn(x)));
            }
            
            // Wait for all refill animations to complete
            yield return new WaitForSeconds(gravityDuration + 0.2f);
            
            Debug.Log("[Match3Game] ✅ Optimized gravity and refill process completed");
        }
        
        /// <summary>
        /// Verifies that all gravity animations have completed before refill
        /// </summary>
        private IEnumerator VerifyGravityCompletion()
        {
            Debug.Log("[Match3Game] 🔍 Verifying all tiles have fallen to correct positions...");
            
            // Check if any tiles are still moving
            bool anyMoving = true;
            float timeout = 2f;
            float elapsed = 0f;
            
            while (anyMoving && elapsed < timeout)
            {
                anyMoving = false;
                
                // Check all visual tiles for movement
                for (int x = 0; x < currentBoard.Width; x++)
                {
                    for (int y = 0; y < currentBoard.Height; y++)
                    {
                        var visual = visualTiles[x, y];
                        if (visual != null)
                        {
                            var tileVisual = visual.GetComponent<TileVisual>();
                            if (tileVisual?.IsMoving == true)
                            {
                                anyMoving = true;
                                Debug.Log($"[Match3Game] 🔄 Tile at ({x},{y}) is still moving...");
                                break;
                            }
                        }
                    }
                    if (anyMoving) break;
                }
                
                if (!anyMoving)
                {
                    Debug.Log("[Match3Game] ✅ All gravity animations verified complete!");
                    break;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (elapsed >= timeout)
            {
                Debug.LogWarning("[Match3Game] ⚠️ Gravity verification timeout - proceeding with refill");
            }
            
            // Extra safety delay
            yield return new WaitForSeconds(0.1f);
        }
        
        /// <summary>
        /// Applies gravity to a single column with LeanTween batch processing
        /// </summary>
        /// <summary>
        /// Applies gravity to a single column, making tiles fall to the lowest available position
        /// Implements proper cascade effect where tiles fall to the bottom-most empty space
        /// </summary>
        private IEnumerator ApplyGravityToColumn(int column)
        {
            Debug.Log($"[Match3Game] 🌍 Applying gravity to column {column}");
            
            // # PHASE 1: Calculate final positions for all tiles using proper cascade algorithm
            var tilesToMove = new List<(int fromY, int toY, TileData tile, GameObject visual)>();
            
            // # Step 1: Find all tiles in this column (from bottom to top)
            var tilesInColumn = new List<(int y, TileData tile, GameObject visual)>();
            for (int y = 0; y < currentBoard.Height; y++)
            {
                var tile = currentBoard.GetTile(column, y);
                if (tile.IsValid)
                {
                    tilesInColumn.Add((y, tile, visualTiles[column, y]));
                }
            }
            
            Debug.Log($"[Match3Game] Column {column}: Found {tilesInColumn.Count} tiles");
            
            // # Step 2: Calculate final positions using bottom-up approach
            // Start from the bottom and place tiles in the lowest available positions
            var finalPositions = new List<(int fromY, int toY, TileData tile, GameObject visual)>();
            int nextEmptyPosition = 0; // Track the next available position from bottom
            
            // Sort tiles by their current Y position (bottom to top)
            var sortedTiles = tilesInColumn.OrderBy(t => t.y).ToList();
            
            foreach (var (currentY, tile, visual) in sortedTiles)
            {
                // Find the lowest available position for this tile
                int targetY = nextEmptyPosition;
                
                // Move to the next available position
                nextEmptyPosition++;
                
                // Only add movement if the tile actually needs to move
                if (currentY != targetY)
                {
                    Debug.Log($"[Match3Game] CASCADE: ({column},{currentY}) -> ({column},{targetY}) - Tile falls {currentY - targetY} positions");
                    finalPositions.Add((currentY, targetY, tile, visual));
                }
            }
            
            tilesToMove = finalPositions;
            
            Debug.Log($"[Match3Game] Column {column}: {tilesToMove.Count} tiles need to move");
            
            // # PHASE 2: Execute all animations with proper completion tracking
            if (tilesToMove.Count > 0)
            {
                var completedAnimations = 0;
                var totalAnimations = tilesToMove.Count;
                var animationsCompleted = false;
                
                Debug.Log($"[Match3Game] 🎬 Starting {totalAnimations} gravity animations for column {column}");
                
                // Start all animations simultaneously
                foreach (var (fromY, toY, tile, visual) in tilesToMove)
                {
                    Debug.Log($"[Match3Game] 🎬 Starting gravity animation: ({column},{fromY}) -> ({column},{toY})");
                    
                    // Update board data immediately
                    currentBoard = currentBoard.SetTile(column, fromY, new TileData(TileType.Empty, new Vector2Int(column, fromY)));
                    currentBoard = currentBoard.SetTile(column, toY, tile.WithPosition(new Vector2Int(column, toY)));
                    
                    // Update visual references
                    visualTiles[column, fromY] = null;
                    visualTiles[column, toY] = visual;
                    
                    // Update position cache
                    if (visual != null)
                    {
                        foundationManager?.UpdateTilePosition(visual, new Vector2Int(column, toY));
                    }
                    
                    // Start LeanTween animation with completion tracking
                    if (visual != null)
                    {
                        var targetPos = new Vector3(column * tileSize, toY * tileSize, 0);
                        
                        var tween = LeanTween.move(visual, targetPos, gravityDuration);
                        tween.setEase(LeanTweenType.easeInQuad);
                        tween.setOnComplete(() => {
                            completedAnimations++;
                            Debug.Log($"[Match3Game] ✅ Gravity animation {completedAnimations}/{totalAnimations} completed for column {column}");
                            
                            // Check if all animations are done
                            if (completedAnimations >= totalAnimations && !animationsCompleted)
                            {
                                animationsCompleted = true;
                                Debug.Log($"[Match3Game] 🎯 All {totalAnimations} gravity animations completed for column {column}!");
                            }
                        });
                    }
                }
                
                // Wait for all animations to complete with extra safety buffer
                yield return new WaitForSeconds(gravityDuration + 0.2f);
                
                // Double-check completion
                if (!animationsCompleted)
                {
                    Debug.LogWarning($"[Match3Game] ⚠️ Gravity animations may not have completed for column {column}");
                }
                
                Debug.Log($"[Match3Game] ✅ Batch gravity animations completed for column {column}");
            }
            
            Debug.Log($"[Match3Game] ✅ Gravity applied to column {column}");
        }
        
        /// <summary>
        /// Refills empty spaces with new tiles using LeanTween spawn animations
        /// </summary>
        private IEnumerator RefillColumn(int column)
        {
            Debug.Log($"[Match3Game] 🔄 Refilling column {column} with LeanTween animations...");
            
            var tilesToSpawn = new List<(int y, TileData tile)>();
            
            // # PHASE 1: Calculate all tiles that need to be spawned
            for (int y = currentBoard.Height - 1; y >= 0; y--)
            {
                var tile = currentBoard.GetTile(column, y);
                
                if (!tile.IsValid)
                {
                    var newTileType = GenerateConstraintTile(column, y);
                    var newTile = new TileData(newTileType, new Vector2Int(column, y));
                    tilesToSpawn.Add((y, newTile));
                }
            }
            
            Debug.Log($"[Match3Game] Column {column}: Need to spawn {tilesToSpawn.Count} new tiles");
            
            // # PHASE 2: Batch spawn all tiles with LeanTween (DELAYED)
            if (tilesToSpawn.Count > 0)
            {
                var completedSpawns = 0;
                var totalSpawns = tilesToSpawn.Count;
                var spawnsCompleted = false;
                
                Debug.Log($"[Match3Game] 🎬 Starting DELAYED spawn of {totalSpawns} tiles for column {column}");
                
                // Small delay to ensure gravity is completely finished
                yield return new WaitForSeconds(0.1f);
                
                foreach (var (y, tileData) in tilesToSpawn)
                {
                    // Update board data
                    currentBoard = currentBoard.SetTile(column, y, tileData);
                    
                    // Create visual tile
                    var tileObject = tilePool.GetTile();
                    var tileVisual = tileObject.GetComponent<TileVisual>();
                    
                    if (tileVisual != null)
                    {
                        tileVisual.SetTileData(tileData);
                        
                        // Calculate spawn position (above the board)
                        var spawnPos = new Vector3(column * tileSize, currentBoard.Height * tileSize, 0);
                        var targetPos = new Vector3(column * tileSize, y * tileSize, 0);
                        
                        // Position tile above board and animate falling
                        tileObject.transform.position = spawnPos;
                        tileObject.transform.SetParent(boardParent);
                        visualTiles[column, y] = tileObject;
                        
                        // Update position cache
                        foundationManager?.UpdateTilePosition(tileObject, new Vector2Int(column, y));
                        
                        Debug.Log($"[Match3Game] 🎬 DELAYED spawning tile at ({column},{y}): {tileData.Type}");
                        
                        // Start LeanTween falling animation
                        var tween = LeanTween.move(tileObject, targetPos, gravityDuration);
                        tween.setEase(LeanTweenType.easeInQuad);
                        tween.setOnComplete(() => {
                            completedSpawns++;
                            Debug.Log($"[Match3Game] ✅ DELAYED spawn animation {completedSpawns}/{totalSpawns} completed for column {column}");
                            
                            // Check if all spawns are done
                            if (completedSpawns >= totalSpawns && !spawnsCompleted)
                            {
                                spawnsCompleted = true;
                                Debug.Log($"[Match3Game] 🎯 All {totalSpawns} DELAYED spawn animations completed for column {column}!");
                            }
                        });
                    }
                }
                
                // Wait for all spawn animations to complete
                yield return new WaitForSeconds(gravityDuration + 0.2f);
                
                // Double-check completion
                if (!spawnsCompleted)
                {
                    Debug.LogWarning($"[Match3Game] ⚠️ DELAYED spawn animations may not have completed for column {column}");
                }
                
                Debug.Log($"[Match3Game] ✅ DELAYED batch spawn animations completed for column {column}");
            }
            
            Debug.Log($"[Match3Game] ✅ Column {column} refill completed");
        }
        
        /// <summary>
        /// Generates a tile type that avoids creating immediate matches
        /// </summary>
        private TileType GenerateConstraintTile(int x, int y)
        {
            var availableTypes = new List<TileType>() 
            { 
                TileType.Red, TileType.Blue, TileType.Green, 
                TileType.Yellow, TileType.Purple, TileType.Orange 
            };
            
            // Remove types that would create immediate matches
            var leftTile1 = (x > 0) ? currentBoard.GetTile(x - 1, y) : new TileData();
            var leftTile2 = (x > 1) ? currentBoard.GetTile(x - 2, y) : new TileData();
            var topTile1 = (y > 0) ? currentBoard.GetTile(x, y - 1) : new TileData();
            var topTile2 = (y > 1) ? currentBoard.GetTile(x, y - 2) : new TileData();
            
            // Remove horizontal match possibility
            if (leftTile1.IsValid && leftTile2.IsValid && leftTile1.Type == leftTile2.Type)
            {
                availableTypes.Remove(leftTile1.Type);
            }
            
            // Remove vertical match possibility  
            if (topTile1.IsValid && topTile2.IsValid && topTile1.Type == topTile2.Type)
            {
                availableTypes.Remove(topTile1.Type);
            }
            
            // Return random available type
            return availableTypes.Count > 0 
                ? availableTypes[UnityEngine.Random.Range(0, availableTypes.Count)]
                : TileType.Red; // Fallback
        }
        
        #endregion
    }
}