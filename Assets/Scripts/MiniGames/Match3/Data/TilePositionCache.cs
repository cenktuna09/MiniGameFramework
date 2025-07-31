using System.Collections.Generic;
using UnityEngine;

namespace MiniGameFramework.MiniGames.Match3.Data
{
    /// <summary>
    /// Optimized position cache system for tile lookups.
    /// Reduces position lookup complexity from O(n²) to O(1).
    /// </summary>
    public class TilePositionCache
    {
        private readonly Dictionary<GameObject, Vector2Int> cache = new Dictionary<GameObject, Vector2Int>();
        private readonly Dictionary<Vector2Int, GameObject> reverseCache = new Dictionary<Vector2Int, GameObject>();
        
        /// <summary>
        /// Gets the board position of a tile with O(1) complexity.
        /// </summary>
        /// <param name="tile">The tile GameObject to find position for.</param>
        /// <returns>The board position of the tile, or Vector2Int.zero if not found.</returns>
        public Vector2Int GetPosition(GameObject tile)
        {
            if (tile == null) return Vector2Int.zero;
            
            if (cache.TryGetValue(tile, out var position))
            {
                return position;
            }
            
            // Fallback to search if not in cache
            var pos = SearchTilePosition(tile);
            if (pos != Vector2Int.zero)
            {
                cache[tile] = pos;
                reverseCache[pos] = tile;
            }
            
            return pos;
        }
        
        /// <summary>
        /// Gets the tile at a specific position with O(1) complexity.
        /// </summary>
        /// <param name="position">The board position to check.</param>
        /// <returns>The tile GameObject at the position, or null if not found.</returns>
        public GameObject GetTileAtPosition(Vector2Int position)
        {
            reverseCache.TryGetValue(position, out var tile);
            return tile;
        }
        
        /// <summary>
        /// Updates the position of a tile in the cache.
        /// </summary>
        /// <param name="tile">The tile GameObject.</param>
        /// <param name="newPosition">The new position.</param>
        public void UpdatePosition(GameObject tile, Vector2Int newPosition)
        {
            if (tile == null) return;
            
            // Remove old position from reverse cache
            if (cache.TryGetValue(tile, out var oldPosition))
            {
                reverseCache.Remove(oldPosition);
            }
            
            // Update position cache
            cache[tile] = newPosition;
            reverseCache[newPosition] = tile;
            
            Debug.Log($"[TilePositionCache] Updated position: {tile.name} -> {newPosition}");
        }
        
        /// <summary>
        /// Removes a tile from the cache.
        /// </summary>
        /// <param name="tile">The tile GameObject to remove.</param>
        public void RemoveTile(GameObject tile)
        {
            if (tile == null) return;
            
            if (cache.TryGetValue(tile, out var position))
            {
                cache.Remove(tile);
                reverseCache.Remove(position);
                Debug.Log($"[TilePositionCache] Removed tile: {tile.name} at {position}");
            }
        }
        
        /// <summary>
        /// Clears all cached data.
        /// </summary>
        public void Clear()
        {
            cache.Clear();
            reverseCache.Clear();
            Debug.Log("[TilePositionCache] Cache cleared");
        }
        
        /// <summary>
        /// Gets the number of cached tiles.
        /// </summary>
        /// <returns>The number of tiles in the cache.</returns>
        public int GetCacheSize()
        {
            return cache.Count;
        }
        
        /// <summary>
        /// Checks if a tile is cached.
        /// </summary>
        /// <param name="tile">The tile GameObject to check.</param>
        /// <returns>True if the tile is cached, false otherwise.</returns>
        public bool IsCached(GameObject tile)
        {
            return tile != null && cache.ContainsKey(tile);
        }
        
        /// <summary>
        /// Fallback method to search for tile position in visual tiles array.
        /// This should only be called when cache miss occurs.
        /// </summary>
        /// <param name="tile">The tile GameObject to find.</param>
        /// <returns>The position of the tile, or Vector2Int.zero if not found.</returns>
        private Vector2Int SearchTilePosition(GameObject tile)
        {
            // This method should be called with the visual tiles array
            // For now, return zero - the actual implementation will be provided by Match3Game
            Debug.LogWarning($"[TilePositionCache] Cache miss for tile: {tile?.name}, performing search...");
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// Initializes the cache with the current board state.
        /// </summary>
        /// <param name="visualTiles">The visual tiles array from the game.</param>
        public void InitializeCache(GameObject[,] visualTiles)
        {
            Clear();
            
            if (visualTiles == null) return;
            
            int width = visualTiles.GetLength(0);
            int height = visualTiles.GetLength(1);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var tile = visualTiles[x, y];
                    if (tile != null)
                    {
                        var position = new Vector2Int(x, y);
                        cache[tile] = position;
                        reverseCache[position] = tile;
                    }
                }
            }
            
            Debug.Log($"[TilePositionCache] Cache initialized with {cache.Count} tiles");
        }
    }
} 