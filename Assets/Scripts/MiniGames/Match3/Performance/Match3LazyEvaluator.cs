using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MiniGameFramework.MiniGames.Match3.Board;
using MiniGameFramework.MiniGames.Match3.Data;
using MiniGameFramework.MiniGames.Match3.Logic;

namespace MiniGameFramework.MiniGames.Match3.Performance
{
    /// <summary>
    /// Implements lazy evaluation for expensive Match3 computations.
    /// Caches results and only recalculates when board state changes.
    /// </summary>
    public class Match3LazyEvaluator
    {
        #region Private Fields
        
        private BoardData? lastEvaluatedBoard;
        private List<Swap> cachedPossibleSwaps;
        private Dictionary<Swap, bool> cachedSwapResults;
        private bool isDirty = true;
        private readonly object lockObject = new object();
        
        #endregion
        
        #region Public Properties
        
        public bool IsDirty => isDirty;
        public int CacheHitCount { get; private set; }
        public int CacheMissCount { get; private set; }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Marks the evaluator as dirty, forcing recalculation on next access.
        /// </summary>
        public void MarkDirty()
        {
            lock (lockObject)
            {
                isDirty = true;
                Debug.Log("[Match3LazyEvaluator] 🗑️ Cache marked as dirty");
            }
        }
        
        /// <summary>
        /// Gets possible swaps with lazy evaluation.
        /// </summary>
        /// <param name="currentBoard">The current board state.</param>
        /// <returns>Cached or newly calculated possible swaps.</returns>
        public List<Swap> GetPossibleSwaps(BoardData currentBoard)
        {
            lock (lockObject)
            {
                if (!isDirty && lastEvaluatedBoard.HasValue && BoardDataEquals(lastEvaluatedBoard.Value, currentBoard))
                {
                    CacheHitCount++;
                    Debug.Log($"[Match3LazyEvaluator] ✅ Cache hit! Returning {cachedPossibleSwaps?.Count ?? 0} cached swaps");
                    return cachedPossibleSwaps ?? new List<Swap>();
                }
                
                CacheMissCount++;
                Debug.Log("[Match3LazyEvaluator] 🔄 Cache miss - recalculating possible swaps");
                
                var swaps = CalculatePossibleSwaps(currentBoard);
                CacheResult(currentBoard, swaps);
                
                return swaps;
            }
        }
        
        /// <summary>
        /// Checks if a swap would create matches with lazy evaluation.
        /// </summary>
        /// <param name="swap">The swap to test.</param>
        /// <param name="currentBoard">The current board state.</param>
        /// <returns>True if the swap would create matches.</returns>
        public bool WouldSwapCreateMatch(Swap swap, BoardData currentBoard)
        {
            lock (lockObject)
            {
                if (!isDirty && lastEvaluatedBoard.HasValue && BoardDataEquals(lastEvaluatedBoard.Value, currentBoard))
                {
                    if (cachedSwapResults.TryGetValue(swap, out bool cachedResult))
                    {
                        CacheHitCount++;
                        return cachedResult;
                    }
                }
                
                CacheMissCount++;
                var result = CalculateSwapResult(swap, currentBoard);
                
                if (!isDirty && lastEvaluatedBoard.HasValue && BoardDataEquals(lastEvaluatedBoard.Value, currentBoard))
                {
                    cachedSwapResults[swap] = result;
                }
                
                return result;
            }
        }
        
        /// <summary>
        /// Clears all cached data.
        /// </summary>
        public void ClearCache()
        {
            lock (lockObject)
            {
                cachedPossibleSwaps?.Clear();
                cachedSwapResults?.Clear();
                lastEvaluatedBoard = null;
                isDirty = true;
                CacheHitCount = 0;
                CacheMissCount = 0;
                
                Debug.Log("[Match3LazyEvaluator] 🧹 Cache cleared");
            }
        }
        
        /// <summary>
        /// Gets cache statistics for debugging.
        /// </summary>
        /// <returns>Cache statistics string.</returns>
        public string GetCacheStats()
        {
            var totalRequests = CacheHitCount + CacheMissCount;
            var hitRate = totalRequests > 0 ? (float)CacheHitCount / totalRequests * 100 : 0;
            
            return $"[Match3LazyEvaluator] 📊 Cache Stats: Hits={CacheHitCount}, Misses={CacheMissCount}, HitRate={hitRate:F1}%, CachedSwaps={cachedPossibleSwaps?.Count ?? 0}, CachedResults={cachedSwapResults?.Count ?? 0}";
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Calculates all possible swaps for the given board.
        /// </summary>
        private List<Swap> CalculatePossibleSwaps(BoardData board)
        {
            var swaps = new List<Swap>();
            
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var currentTile = board.GetTile(x, y);
                    if (!currentTile.IsValid) continue;
                    
                    // Check right neighbor
                    if (x < board.Width - 1)
                    {
                        var swap = new Swap(new Vector2Int(x, y), new Vector2Int(x + 1, y));
                        if (CalculateSwapResult(swap, board))
                        {
                            swaps.Add(swap);
                        }
                    }
                    
                    // Check down neighbor
                    if (y < board.Height - 1)
                    {
                        var swap = new Swap(new Vector2Int(x, y), new Vector2Int(x, y + 1));
                        if (CalculateSwapResult(swap, board))
                        {
                            swaps.Add(swap);
                        }
                    }
                }
            }
            
            Debug.Log($"[Match3LazyEvaluator] 🔍 Calculated {swaps.Count} possible swaps");
            return swaps;
        }
        
        /// <summary>
        /// Calculates if a swap would create matches.
        /// </summary>
        private bool CalculateSwapResult(Swap swap, BoardData board)
        {
            var tileA = board.GetTile(swap.tileA);
            var tileB = board.GetTile(swap.tileB);
            
            if (!tileA.IsValid || !tileB.IsValid) return false;
            
            var simulatedBoard = board
                .SetTile(swap.tileA, tileB.WithPosition(swap.tileA))
                .SetTile(swap.tileB, tileA.WithPosition(swap.tileB));
            
            var matches = MatchDetector.FindMatches(simulatedBoard);
            return matches.Count > 0;
        }
        
        /// <summary>
        /// Caches the calculation results.
        /// </summary>
        private void CacheResult(BoardData board, List<Swap> swaps)
        {
            lastEvaluatedBoard = board;
            cachedPossibleSwaps = new List<Swap>(swaps);
            cachedSwapResults = new Dictionary<Swap, bool>();
            isDirty = false;
            
            Debug.Log($"[Match3LazyEvaluator] 💾 Cached {swaps.Count} possible swaps");
        }
        
        /// <summary>
        /// Compares two board data objects for equality.
        /// </summary>
        private bool BoardDataEquals(BoardData board1, BoardData board2)
        {
            if (board1.Width != board2.Width || board1.Height != board2.Height)
                return false;
            
            for (int x = 0; x < board1.Width; x++)
            {
                for (int y = 0; y < board1.Height; y++)
                {
                    var tile1 = board1.GetTile(x, y);
                    var tile2 = board2.GetTile(x, y);
                    
                    if (tile1.Type != tile2.Type || tile1.IsValid != tile2.IsValid)
                        return false;
                }
            }
            
            return true;
        }
        
        #endregion
    }
} 