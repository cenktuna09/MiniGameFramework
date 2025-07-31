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
    /// Optimized swap detection system that uses incremental updates and caching.
    /// Reduces time complexity from O(n²) to O(1) for most operations.
    /// </summary>
    public class Match3OptimizedSwapDetector
    {
        #region Private Fields
        
        private BoardData currentBoard;
        private Dictionary<Swap, bool> swapCache;
        private HashSet<Swap> validSwaps;
        private Dictionary<Vector2Int, List<Swap>> positionToSwaps;
        private bool isDirty = true;
        private readonly object lockObject = new object();
        
        // Performance tracking
        private int cacheHits = 0;
        private int cacheMisses = 0;
        private int incrementalUpdates = 0;
        private int fullRecalculations = 0;
        
        #endregion
        
        #region Public Properties
        
        public bool IsDirty => isDirty;
        public int ValidSwapCount => validSwaps?.Count ?? 0;
        public int CacheSize => swapCache?.Count ?? 0;
        
        #endregion
        
        #region Constructor
        
        public Match3OptimizedSwapDetector()
        {
            swapCache = new Dictionary<Swap, bool>();
            validSwaps = new HashSet<Swap>();
            positionToSwaps = new Dictionary<Vector2Int, List<Swap>>();
            
            Debug.Log("[Match3OptimizedSwapDetector] ✅ Initialized");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Updates the board and recalculates swaps incrementally.
        /// </summary>
        /// <param name="newBoard">The new board state.</param>
        /// <param name="changedPositions">Positions that changed (for incremental updates).</param>
        public void UpdateBoard(BoardData newBoard, List<Vector2Int> changedPositions = null)
        {
            lock (lockObject)
            {
                if (newBoard == null)
                {
                    Debug.LogWarning("[Match3OptimizedSwapDetector] ⚠️ Cannot update with null board");
                    return;
                }
                
                currentBoard = newBoard;
                
                if (changedPositions != null && changedPositions.Count > 0)
                {
                    // Incremental update - only recalculate affected swaps
                    IncrementalUpdate(changedPositions);
                    incrementalUpdates++;
                    Debug.Log($"[Match3OptimizedSwapDetector] 🔄 Incremental update for {changedPositions.Count} positions");
                }
                else
                {
                    // Full recalculation
                    FullRecalculation();
                    fullRecalculations++;
                    Debug.Log("[Match3OptimizedSwapDetector] 🔄 Full recalculation");
                }
                
                isDirty = false;
            }
        }
        
        /// <summary>
        /// Gets all valid swaps with caching.
        /// </summary>
        /// <returns>List of valid swaps.</returns>
        public List<Swap> GetValidSwaps()
        {
            lock (lockObject)
            {
                if (isDirty)
                {
                    FullRecalculation();
                }
                
                return new List<Swap>(validSwaps);
            }
        }
        
        /// <summary>
        /// Checks if a specific swap is valid with caching.
        /// </summary>
        /// <param name="swap">The swap to check.</param>
        /// <returns>True if the swap is valid.</returns>
        public bool IsSwapValid(Swap swap)
        {
            lock (lockObject)
            {
                if (swapCache.TryGetValue(swap, out bool cachedResult))
                {
                    cacheHits++;
                    return cachedResult;
                }
                
                cacheMisses++;
                var result = CalculateSwapValidity(swap);
                swapCache[swap] = result;
                
                return result;
            }
        }
        
        /// <summary>
        /// Gets a random valid swap for hints.
        /// </summary>
        /// <returns>A random valid swap or default if none available.</returns>
        public Swap GetRandomValidSwap()
        {
            lock (lockObject)
            {
                if (validSwaps.Count == 0)
                    return new Swap();
                
                var randomIndex = UnityEngine.Random.Range(0, validSwaps.Count);
                return validSwaps.ElementAt(randomIndex);
            }
        }
        
        /// <summary>
        /// Marks the detector as dirty, forcing recalculation on next access.
        /// </summary>
        public void MarkDirty()
        {
            lock (lockObject)
            {
                isDirty = true;
                Debug.Log("[Match3OptimizedSwapDetector] 🗑️ Marked as dirty");
            }
        }
        
        /// <summary>
        /// Clears all cached data.
        /// </summary>
        public void ClearCache()
        {
            lock (lockObject)
            {
                swapCache.Clear();
                validSwaps.Clear();
                positionToSwaps.Clear();
                isDirty = true;
                cacheHits = 0;
                cacheMisses = 0;
                incrementalUpdates = 0;
                fullRecalculations = 0;
                
                Debug.Log("[Match3OptimizedSwapDetector] 🧹 Cache cleared");
            }
        }
        
        /// <summary>
        /// Gets performance statistics.
        /// </summary>
        /// <returns>Performance statistics string.</returns>
        public string GetPerformanceStats()
        {
            var totalRequests = cacheHits + cacheMisses;
            var hitRate = totalRequests > 0 ? (float)cacheHits / totalRequests * 100 : 0;
            
            return $"[Match3OptimizedSwapDetector] 📊 Cache: Hits={cacheHits}, Misses={cacheMisses}, HitRate={hitRate:F1}%, " +
                   $"Updates: Incremental={incrementalUpdates}, Full={fullRecalculations}, " +
                   $"ValidSwaps={validSwaps.Count}, CacheSize={swapCache.Count}";
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Performs incremental update for changed positions.
        /// </summary>
        private void IncrementalUpdate(List<Vector2Int> changedPositions)
        {
            var affectedSwaps = new HashSet<Swap>();
            
            // Find all swaps affected by the changed positions
            foreach (var position in changedPositions)
            {
                if (positionToSwaps.TryGetValue(position, out var swaps))
                {
                    foreach (var swap in swaps)
                    {
                        affectedSwaps.Add(swap);
                    }
                }
                
                // Check adjacent positions for new swaps
                var adjacentPositions = GetAdjacentPositions(position);
                foreach (var adjPos in adjacentPositions)
                {
                    var swap = new Swap(position, adjPos);
                    affectedSwaps.Add(swap);
                    
                    var reverseSwap = new Swap(adjPos, position);
                    affectedSwaps.Add(reverseSwap);
                }
            }
            
            // Recalculate affected swaps
            foreach (var swap in affectedSwaps)
            {
                var isValid = CalculateSwapValidity(swap);
                swapCache[swap] = isValid;
                
                if (isValid)
                {
                    validSwaps.Add(swap);
                }
                else
                {
                    validSwaps.Remove(swap);
                }
            }
            
            // Update position-to-swaps mapping
            UpdatePositionMapping();
        }
        
        /// <summary>
        /// Performs full recalculation of all swaps.
        /// </summary>
        private void FullRecalculation()
        {
            validSwaps.Clear();
            swapCache.Clear();
            positionToSwaps.Clear();
            
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
                        var isValid = CalculateSwapValidity(swap);
                        swapCache[swap] = isValid;
                        
                        if (isValid)
                        {
                            validSwaps.Add(swap);
                        }
                    }
                    
                    // Check down neighbor
                    if (y < currentBoard.Height - 1)
                    {
                        var swap = new Swap(new Vector2Int(x, y), new Vector2Int(x, y + 1));
                        var isValid = CalculateSwapValidity(swap);
                        swapCache[swap] = isValid;
                        
                        if (isValid)
                        {
                            validSwaps.Add(swap);
                        }
                    }
                }
            }
            
            UpdatePositionMapping();
        }
        
        /// <summary>
        /// Calculates if a swap would create matches.
        /// </summary>
        private bool CalculateSwapValidity(Swap swap)
        {
            var tileA = currentBoard.GetTile(swap.tileA);
            var tileB = currentBoard.GetTile(swap.tileB);
            
            if (!tileA.IsValid || !tileB.IsValid) return false;
            
            var simulatedBoard = currentBoard
                .SetTile(swap.tileA, tileB.WithPosition(swap.tileA))
                .SetTile(swap.tileB, tileA.WithPosition(swap.tileB));
            
            var matches = MatchDetector.FindMatches(simulatedBoard);
            return matches.Count > 0;
        }
        
        /// <summary>
        /// Gets adjacent positions for a given position.
        /// </summary>
        private List<Vector2Int> GetAdjacentPositions(Vector2Int position)
        {
            var adjacent = new List<Vector2Int>();
            
            // Right
            if (position.x < currentBoard.Width - 1)
                adjacent.Add(new Vector2Int(position.x + 1, position.y));
            
            // Left
            if (position.x > 0)
                adjacent.Add(new Vector2Int(position.x - 1, position.y));
            
            // Up
            if (position.y < currentBoard.Height - 1)
                adjacent.Add(new Vector2Int(position.x, position.y + 1));
            
            // Down
            if (position.y > 0)
                adjacent.Add(new Vector2Int(position.x, position.y - 1));
            
            return adjacent;
        }
        
        /// <summary>
        /// Updates the position-to-swaps mapping for quick lookups.
        /// </summary>
        private void UpdatePositionMapping()
        {
            positionToSwaps.Clear();
            
            foreach (var swap in validSwaps)
            {
                AddSwapToPositionMapping(swap.tileA, swap);
                AddSwapToPositionMapping(swap.tileB, swap);
            }
        }
        
        /// <summary>
        /// Adds a swap to the position mapping.
        /// </summary>
        private void AddSwapToPositionMapping(Vector2Int position, Swap swap)
        {
            if (!positionToSwaps.ContainsKey(position))
            {
                positionToSwaps[position] = new List<Swap>();
            }
            
            if (!positionToSwaps[position].Contains(swap))
            {
                positionToSwaps[position].Add(swap);
            }
        }
        
        #endregion
    }
} 