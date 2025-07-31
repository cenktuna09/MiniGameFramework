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
        
        // # Game Flow Control
        private bool isSwapping = false;
        private bool isProcessingMatches = false;
        private bool inputLocked = false;
        
        // # Possible Swaps System
        private List<Swap> possibleSwaps = new List<Swap>();
        private Coroutine hintCoroutine;
        
        // # Input System
        private GameObject selectedTile = null;
        private bool isDragging = false;
        

        
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
                
                // # 2. Initialize Components  
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
                    Debug.Log("[Match3Game] 🚀 Auto-starting game...");
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
            Debug.Log("[Match3Game] 🎮 Game Started!");
            inputLocked = false;
            StartHintTimer();
        }
        
        protected override void OnPause()
        {
            Debug.Log("[Match3Game] ⏸️ Game Paused");
            inputLocked = true;
            StopHintTimer();
        }
        
        protected override void OnResume()
        {
            Debug.Log("[Match3Game] ▶️ Game Resumed");
            inputLocked = false;
            StartHintTimer();
        }
        
        protected override void OnEnd()
        {
            Debug.Log($"[Match3Game] 🏁 Game Ended! Final Score: {currentScore}");
            inputLocked = true;
            StopHintTimer();
            
            // Publish game over event
            eventBus?.Publish(new GameStateChangedEvent(gameId, GameState.Playing, GameState.GameOver, this));
        }
        
        protected override void OnCleanup()
        {
            Debug.Log("[Match3Game] Cleaning up resources");
            
            // Return all tiles to pool
            if (tilePool != null)
            {
                tilePool.ReturnAllTiles();
            }
            
            selectedTile = null;
            isDragging = false;
            inputLocked = true;
            currentScore = 0;
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
            if (currentState == GameState.Playing && !inputLocked)
            {
                HandleInput();
            }
        }

        /// <summary>
        /// Clean input handling with LeanTween animations for tile selection and swapping
        /// </summary>
        private void HandleInput()
        {
            if (isProcessingMatches || isSwapping) return;
            
            // # Mouse Down - Select Tile
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                var worldPos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                var hit = Physics2D.Raycast(worldPos, Vector2.zero);
                
                if (hit.collider?.CompareTag("Tile") == true)
                {
                    SelectTile(hit.collider.gameObject);
                    isDragging = true;
                    RestartHintTimer(); // Reset hint timer on player activity
                }
            }
            
            // # Mouse Up - End Selection
            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                DeselectTile();
            }
            
            // # Drag Detection - Attempt Swap with LeanTween
            if (isDragging && selectedTile != null)
            {
                var worldPos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                var hit = Physics2D.Raycast(worldPos, Vector2.zero);
                
                if (hit.collider?.CompareTag("Tile") == true && hit.collider.gameObject != selectedTile)
                {
                    var targetTile = hit.collider.gameObject;
                    var swap = new Swap(GetTileBoardPosition(selectedTile), GetTileBoardPosition(targetTile));
                    
                    // Check if this is a valid swap
                    if (IsValidSwap(swap))
                    {
                        StartCoroutine(ProcessSwapWithLeanTween(swap));
                        DeselectTile();
                        isDragging = false;
                    }
                    else
                    {
                        // Invalid move - show error animation
                        ShowInvalidMoveAnimation(selectedTile, targetTile);
                        DeselectTile();
                        isDragging = false;
                    }
                }
            }
        }

        #region Input Handling Helpers
        
        /// <summary>
        /// Selects a tile with visual feedback
        /// </summary>
        private void SelectTile(GameObject tile)
        {
            selectedTile = tile;
            var spriteRenderer = tile.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.yellow; // Highlight selected tile
            }
        }
        
        /// <summary>
        /// Deselects current tile and removes visual feedback
        /// </summary>
        private void DeselectTile()
        {
            if (selectedTile != null)
            {
                var spriteRenderer = selectedTile.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white; // Reset color
                }
                selectedTile = null;
            }
        }
        
        /// <summary>
        /// Gets the board position of a visual tile
        /// </summary>
        private Vector2Int GetTileBoardPosition(GameObject tile)
        {
            for (int x = 0; x < currentBoard.Width; x++)
            {
                for (int y = 0; y < currentBoard.Height; y++)
                {
                    if (visualTiles[x, y] == tile)
                        return new Vector2Int(x, y);
                }
            }
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// Checks if a swap is valid (adjacent tiles that would create matches)
        /// </summary>
        private bool IsValidSwap(Swap swap)
        {
            // Check if tiles are adjacent
            int deltaX = Mathf.Abs(swap.tileA.x - swap.tileB.x);
            int deltaY = Mathf.Abs(swap.tileA.y - swap.tileB.y);
            bool isAdjacent = (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
            
            if (!isAdjacent) return false;
            
            // Check if swap exists in possible swaps list
            return possibleSwaps.Contains(swap) || 
                   possibleSwaps.Contains(new Swap(swap.tileB, swap.tileA));
        }
        
        #endregion
        
        #region Core Game Systems
        
        /// <summary>
        /// Detects all possible swaps that would create matches
        /// </summary>
        private void DetectPossibleSwaps()
        {
            possibleSwaps.Clear();
            
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
                        }
                    }
                    
                    // Check down neighbor  
                    if (y < currentBoard.Height - 1)
                    {
                        var swap = new Swap(new Vector2Int(x, y), new Vector2Int(x, y + 1));
                        if (WouldSwapCreateMatch(swap))
                        {
                            possibleSwaps.Add(swap);
                        }
                    }
                }
            }
            
            Debug.Log($"[Match3Game] 🔍 Found {possibleSwaps.Count} possible swaps");
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
            
            return MatchDetector.FindMatches(simulatedBoard).Count > 0;
        }
        
        /// <summary>
        /// Generates a constraint-based board that prevents initial matches
        /// </summary>
        private void GenerateConstraintBasedBoard()
        {
            Debug.Log("[Match3Game] 🎲 Generating constraint-based board...");
            
            // Generate board ensuring no initial matches
            currentBoard = BoardGenerator.GenerateBoard();
            
            // Create visual representation
            CreateVisualBoard();
            
            Debug.Log("[Match3Game] ✅ Constraint-based board generated successfully");
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
        /// Clean gravity and refill system
        /// </summary>
        private IEnumerator ApplyGravityAndRefill()
        {
            Debug.Log("[Match3Game] 🌍 Starting gravity and refill process...");
            
            // # Apply gravity (tiles fall down)
            Debug.Log("[Match3Game] ⬇️ Applying gravity to all columns...");
            for (int x = 0; x < currentBoard.Width; x++)
            {
                yield return StartCoroutine(ApplyGravityToColumn(x));
            }
            
            // # Refill empty spaces
            Debug.Log("[Match3Game] 🔄 Refilling empty spaces...");
            for (int x = 0; x < currentBoard.Width; x++)
            {
                yield return StartCoroutine(RefillColumn(x));
            }
            
            Debug.Log("[Match3Game] ✅ Gravity and refill process completed");
        }
        
        /// <summary>
        /// Applies gravity to a single column with smooth animation
        /// </summary>
        private IEnumerator ApplyGravityToColumn(int column)
        {
            Debug.Log($"[Match3Game] 🌍 Applying gravity to column {column}");
            
            var tilesToMove = new List<(int fromY, int toY, TileData tile, GameObject visual)>();
            
            // # AŞAĞIDAN YUKARI TARAMA - Daha kapsamlı algoritma
            // Tüm boş pozisyonları bul ve her biri için üstündeki tüm tile'ları aşağı çek
            var emptyPositions = new List<int>();
            
            for (int y = currentBoard.Height - 1; y >= 0; y--)
            {
                var tile = currentBoard.GetTile(column, y);
                Debug.Log($"[Match3Game] Column {column}, Y {y}: {tile.Type} (Valid: {tile.IsValid})");
                
                if (!tile.IsValid) // Boş pozisyon bulduk
                {
                    emptyPositions.Add(y);
                }
            }
            
            Debug.Log($"[Match3Game] Column {column}: Found {emptyPositions.Count} empty positions");
            
            // Her boş pozisyon için üstündeki tüm tile'ları aşağı çek
            foreach (int emptyY in emptyPositions)
            {
                Debug.Log($"[Match3Game] Processing empty position at Y={emptyY}");
                
                // Bu boş pozisyonun üstündeki tüm tile'ları bul
                var tilesAbove = new List<(int fromY, TileData tile, GameObject visual)>();
                
                for (int aboveY = emptyY + 1; aboveY < currentBoard.Height; aboveY++)
                {
                    var tileAbove = currentBoard.GetTile(column, aboveY);
                    if (tileAbove.IsValid)
                    {
                        tilesAbove.Add((aboveY, tileAbove, visualTiles[column, aboveY]));
                        Debug.Log($"[Match3Game] Found tile above: ({column},{aboveY}) -> ({column},{emptyY})");
                    }
                }
                
                // Bu tile'ları aşağı çek (en yakın boş pozisyondan başlayarak)
                for (int i = 0; i < tilesAbove.Count; i++)
                {
                    var (fromY, tile, visual) = tilesAbove[i];
                    var targetY = emptyY + i; // Her tile bir pozisyon aşağı
                    
                    if (targetY < currentBoard.Height)
                    {
                        Debug.Log($"[Match3Game] Moving tile down: ({column},{fromY}) -> ({column},{targetY})");
                        tilesToMove.Add((fromY, targetY, tile, visual));
                    }
                }
            }
            
            // # YENİ: Aşağıdan yukarı doğru cascade effect
            // Eğer aşağıda boşluk varsa, yukarıdaki tüm tile'ları aşağı çek
            for (int y = 0; y < currentBoard.Height - 1; y++)
            {
                var currentTile = currentBoard.GetTile(column, y);
                var nextTile = currentBoard.GetTile(column, y + 1);
                
                // Eğer şu anki pozisyon boş ve üstünde tile varsa
                if (!currentTile.IsValid && nextTile.IsValid)
                {
                    Debug.Log($"[Match3Game] Cascade effect: ({column},{y+1}) -> ({column},{y})");
                    tilesToMove.Add((y + 1, y, nextTile, visualTiles[column, y + 1]));
                }
            }
            
            // # Duplicate tile hareketlerini temizle
            var uniqueMoves = new List<(int fromY, int toY, TileData tile, GameObject visual)>();
            var processedTiles = new HashSet<GameObject>();
            
            foreach (var move in tilesToMove)
            {
                if (!processedTiles.Contains(move.visual))
                {
                    uniqueMoves.Add(move);
                    processedTiles.Add(move.visual);
                }
            }
            
            tilesToMove = uniqueMoves;
            
            Debug.Log($"[Match3Game] Column {column}: {tilesToMove.Count} tiles need to move");
            
            // # Move tiles with animation
            foreach (var (fromY, toY, tile, visual) in tilesToMove)
            {
                Debug.Log($"[Match3Game] Moving tile from ({column},{fromY}) to ({column},{toY})");
                
                // Update board data
                currentBoard = currentBoard.SetTile(column, fromY, new TileData(TileType.Empty, new Vector2Int(column, fromY)));
                currentBoard = currentBoard.SetTile(column, toY, tile.WithPosition(new Vector2Int(column, toY)));
                
                // Update visual position
                visualTiles[column, fromY] = null;
                visualTiles[column, toY] = visual;
                
                // Animate visual tile with LeanTween
                if (visual != null)
                {
                    var targetPos = new Vector3(column * tileSize, toY * tileSize, 0);
                    Debug.Log($"[Match3Game] 🎬 Animating tile with LeanTween: ({column},{fromY}) -> ({column},{toY})");
                    
                    // Use LeanTween for gravity animation with easing
                    var tween = LeanTween.move(visual, targetPos, gravityDuration);
                    tween.setEase(LeanTweenType.easeInQuad);
                    tween.setOnComplete(() => {
                        Debug.Log($"[Match3Game] ✅ Gravity animation completed: ({column},{fromY}) -> ({column},{toY})");
                    });
                }
            }
            
            // # Wait for LeanTween animations to complete
            if (tilesToMove.Count > 0)
            {
                Debug.Log($"[Match3Game] Waiting for {tilesToMove.Count} LeanTween gravity animations...");
                
                // Wait for gravity duration plus a small buffer
                yield return new WaitForSeconds(gravityDuration + 0.1f);
                
                Debug.Log($"[Match3Game] ✅ All LeanTween gravity animations completed");
            }
            
            Debug.Log($"[Match3Game] ✅ Gravity applied to column {column}");
        }
        
        /// <summary>
        /// Refills empty spaces with new random tiles
        /// </summary>
        private IEnumerator RefillColumn(int column)
        {
            Debug.Log($"[Match3Game] 🔄 Refilling column {column}...");
            int refillCount = 0;
            
            for (int y = currentBoard.Height - 1; y >= 0; y--)
            {
                var tile = currentBoard.GetTile(column, y);
                Debug.Log($"[Match3Game] Column {column}, Y {y}: {tile.Type} (Valid: {tile.IsValid})");
                
                if (!tile.IsValid)
                {
                    // Generate constraint-based tile (avoiding immediate matches)
                    var newTileType = GenerateConstraintTile(column, y);
                    var newTile = new TileData(newTileType, new Vector2Int(column, y));
                    
                    Debug.Log($"[Match3Game] Creating new tile at ({column},{y}): {newTileType}");
                    
                    currentBoard = currentBoard.SetTile(column, y, newTile);
                    CreateVisualTile(newTile, column, y);
                    
                    refillCount++;
                    yield return new WaitForSeconds(0.05f); // Small delay for effect
                }
            }
            
            Debug.Log($"[Match3Game] ✅ Column {column} refilled with {refillCount} new tiles");
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