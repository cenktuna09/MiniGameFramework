using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.MiniGames.Match3.Board;
using MiniGameFramework.MiniGames.Match3.Data;

namespace MiniGameFramework.MiniGames.Match3.Logic
{
    /// <summary>
    /// Manages core Match-3 game logic including swap detection, match processing, and board state management.
    /// Follows Single Responsibility Principle by focusing solely on game logic operations.
    /// </summary>
    public class Match3GameLogicManager
    {
        #region Private Fields
        
        private readonly IEventBus eventBus;
        private BoardData currentBoard;
        private List<Swap> possibleSwaps;
        private bool isProcessingMatches;
        
        #endregion
        
        #region Events
        
        public event Action<List<Swap>> OnPossibleSwapsUpdated;
        public event Action<List<MatchDetector.Match>> OnMatchesFound;
        public event Action<Swap> OnValidSwapExecuted;
        public event Action<Swap> OnInvalidSwapAttempted;
        
        #endregion
        
        #region Constructor
        
        public Match3GameLogicManager(IEventBus eventBus)
        {
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            this.possibleSwaps = new List<Swap>();
            this.isProcessingMatches = false;
            
            Debug.Log("[Match3GameLogicManager] ✅ Initialized successfully");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initializes the game logic manager with a new board.
        /// </summary>
        /// <param name="board">The initial board data</param>
        public void InitializeBoard(BoardData board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));
                
            currentBoard = board;
            DetectPossibleSwaps();
            
            Debug.Log($"[Match3GameLogicManager] 🎲 Board initialized: {board.Width}x{board.Height}");
        }
        
        /// <summary>
        /// Updates the current board state and recalculates possible swaps.
        /// </summary>
        /// <param name="newBoard">The updated board data</param>
        public void UpdateBoard(BoardData newBoard)
        {
            if (newBoard == null)
                throw new ArgumentNullException(nameof(newBoard));
                
            currentBoard = newBoard;
            DetectPossibleSwaps();
            
            Debug.Log($"[Match3GameLogicManager] 📊 Board updated, possible swaps: {possibleSwaps.Count}");
        }
        
        /// <summary>
        /// Validates if a swap would create a match and executes it if valid.
        /// </summary>
        /// <param name="swap">The swap to validate and execute</param>
        /// <returns>True if the swap was valid and executed, false otherwise</returns>
        public bool ValidateAndExecuteSwap(Swap swap)
        {
            if (swap.tileA == Vector2Int.zero && swap.tileB == Vector2Int.zero)
                throw new ArgumentException("Invalid swap: both tiles are at zero position");
                
            if (isProcessingMatches)
            {
                Debug.LogWarning("[Match3GameLogicManager] ⚠️ Cannot execute swap while processing matches");
                return false;
            }
            
            // Validate swap
            if (!IsValidSwap(swap))
            {
                OnInvalidSwapAttempted?.Invoke(swap);
                Debug.Log($"[Match3GameLogicManager] ❌ Invalid swap attempted: {swap.tileA} ↔ {swap.tileB}");
                return false;
            }
            
            // Execute swap
            ExecuteSwap(swap);
            OnValidSwapExecuted?.Invoke(swap);
            
            Debug.Log($"[Match3GameLogicManager] ✅ Valid swap executed: {swap.tileA} ↔ {swap.tileB}");
            return true;
        }
        
        /// <summary>
        /// Processes all matches on the current board.
        /// </summary>
        /// <returns>List of matches found and processed</returns>
        public List<MatchDetector.Match> ProcessMatches()
        {
            if (isProcessingMatches)
            {
                Debug.LogWarning("[Match3GameLogicManager] ⚠️ Already processing matches");
                return new List<MatchDetector.Match>();
            }
            
            isProcessingMatches = true;
            
            try
            {
                var matches = MatchDetector.FindMatches(currentBoard);
                
                if (matches != null && matches.Count > 0)
                {
                    Debug.Log($"[Match3GameLogicManager] 🎯 Found {matches.Count} matches");
                    OnMatchesFound?.Invoke(matches);
                    
                    // Publish match events
                    foreach (var match in matches)
                    {
                        eventBus?.Publish(new MatchFoundEvent(match.Positions, match.TileType));
                    }
                }
                else
                {
                    Debug.Log("[Match3GameLogicManager] 📭 No matches found");
                }
                
                return matches;
            }
            finally
            {
                isProcessingMatches = false;
            }
        }
        
        /// <summary>
        /// Gets the current list of possible swaps.
        /// </summary>
        /// <returns>List of valid swaps that would create matches</returns>
        public List<Swap> GetPossibleSwaps()
        {
            return new List<Swap>(possibleSwaps);
        }
        
        /// <summary>
        /// Checks if the current board has any possible moves.
        /// </summary>
        /// <returns>True if there are possible moves, false if the game is stuck</returns>
        public bool HasPossibleMoves()
        {
            return possibleSwaps.Count > 0;
        }
        
        /// <summary>
        /// Gets a random hint from the possible swaps.
        /// </summary>
        /// <returns>A random valid swap for hinting</returns>
        public Swap GetRandomHint()
        {
            if (possibleSwaps.Count == 0)
                return new Swap(Vector2Int.zero, Vector2Int.zero);
                
            var randomIndex = UnityEngine.Random.Range(0, possibleSwaps.Count);
            return possibleSwaps[randomIndex];
        }
        
        /// <summary>
        /// Gets the current board state.
        /// </summary>
        /// <returns>Current board data</returns>
        public BoardData GetCurrentBoard()
        {
            return currentBoard;
        }
        
        /// <summary>
        /// Checks if the game logic is currently processing matches.
        /// </summary>
        /// <returns>True if processing matches, false otherwise</returns>
        public bool IsProcessingMatches()
        {
            return isProcessingMatches;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Detects all possible swaps that would create matches.
        /// </summary>
        private void DetectPossibleSwaps()
        {
            possibleSwaps.Clear();
            
            for (int x = 0; x < currentBoard.Width; x++)
            {
                for (int y = 0; y < currentBoard.Height; y++)
                {
                    // Check horizontal swaps
                    if (x < currentBoard.Width - 1)
                    {
                        var swap = new Swap(new Vector2Int(x, y), new Vector2Int(x + 1, y));
                        if (WouldSwapCreateMatch(swap))
                        {
                            possibleSwaps.Add(swap);
                        }
                    }
                    
                    // Check vertical swaps
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
            
            OnPossibleSwapsUpdated?.Invoke(possibleSwaps);
            Debug.Log($"[Match3GameLogicManager] 🔍 Detected {possibleSwaps.Count} possible swaps");
        }
        
        /// <summary>
        /// Validates if a swap is valid (adjacent and would create a match).
        /// </summary>
        /// <param name="swap">The swap to validate</param>
        /// <returns>True if the swap is valid</returns>
        private bool IsValidSwap(Swap swap)
        {
            // Check if tiles are adjacent
            int deltaX = Mathf.Abs(swap.tileA.x - swap.tileB.x);
            int deltaY = Mathf.Abs(swap.tileA.y - swap.tileB.y);
            bool isAdjacent = (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
            
            if (!isAdjacent)
            {
                Debug.Log($"[Match3GameLogicManager] ❌ Swap not adjacent: {swap.tileA} ↔ {swap.tileB}");
                return false;
            }
            
            // Check if this swap would create a match
            bool wouldCreateMatch = WouldSwapCreateMatch(swap);
            
            Debug.Log($"[Match3GameLogicManager] 🔍 Validating swap: {swap.tileA} ↔ {swap.tileB}, WouldCreateMatch: {wouldCreateMatch}");
            
            return wouldCreateMatch;
        }
        
        /// <summary>
        /// Checks if a swap would create a match by simulating the swap.
        /// </summary>
        /// <param name="swap">The swap to test</param>
        /// <returns>True if the swap would create a match</returns>
        private bool WouldSwapCreateMatch(Swap swap)
        {
            try
            {
                // Simulate the swap
                var tileA = currentBoard.GetTile(swap.tileA);
                var tileB = currentBoard.GetTile(swap.tileB);
                
                if (tileA == null || tileB == null)
                {
                    Debug.LogWarning($"[Match3GameLogicManager] ⚠️ Null tiles in swap: {swap.tileA} ↔ {swap.tileB}");
                    return false;
                }
                
                var simulatedBoard = currentBoard
                    .SetTile(swap.tileA, tileB.WithPosition(swap.tileA))
                    .SetTile(swap.tileB, tileA.WithPosition(swap.tileB));
                
                var matches = MatchDetector.FindMatches(simulatedBoard);
                bool wouldCreateMatch = matches != null && matches.Count > 0;
                
                Debug.Log($"[Match3GameLogicManager] 🔍 Testing swap {swap.tileA} ↔ {swap.tileB}: " +
                         $"TileA={tileA.Type}, TileB={tileB.Type}, " +
                         $"WouldCreateMatch={wouldCreateMatch}, MatchesFound={matches.Count}");
                
                return wouldCreateMatch;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Match3GameLogicManager] ❌ Error testing swap {swap.tileA} ↔ {swap.tileB}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Executes a valid swap on the current board.
        /// </summary>
        /// <param name="swap">The swap to execute</param>
        private void ExecuteSwap(Swap swap)
        {
            try
            {
                var tileA = currentBoard.GetTile(swap.tileA);
                var tileB = currentBoard.GetTile(swap.tileB);
                
                if (tileA == null || tileB == null)
                {
                    throw new InvalidOperationException($"Cannot execute swap with null tiles: {swap.tileA} ↔ {swap.tileB}");
                }
                
                // Execute the swap
                currentBoard = currentBoard
                    .SetTile(swap.tileA, tileB.WithPosition(swap.tileA))
                    .SetTile(swap.tileB, tileA.WithPosition(swap.tileB));
                
                Debug.Log($"[Match3GameLogicManager] ✅ Swap executed: {swap.tileA} ↔ {swap.tileB}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Match3GameLogicManager] ❌ Error executing swap {swap.tileA} ↔ {swap.tileB}: {ex.Message}");
                throw;
            }
        }
        
        #endregion
    }
} 