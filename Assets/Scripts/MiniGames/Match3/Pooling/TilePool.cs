using System.Collections.Generic;
using UnityEngine;
using MiniGameFramework.MiniGames.Match3.Data;
using MiniGameFramework.MiniGames.Match3.Visual;

namespace MiniGameFramework.MiniGames.Match3.Pooling
{
    /// <summary>
    /// Object pool for tile GameObjects in Match3.
    /// Prevents GC allocations by reusing tile instances.
    /// </summary>
    public class TilePool : MonoBehaviour
    {
        [Header("Pool Configuration")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private int initialPoolSize = 50;
        [SerializeField] private Transform poolParent;
        
        private readonly Stack<GameObject> availableTiles = new Stack<GameObject>();
        private readonly HashSet<GameObject> activeTiles = new HashSet<GameObject>();
        
        /// <summary>
        /// Initializes the tile pool with the specified size.
        /// </summary>
        public void Initialize()
        {
            if (tilePrefab == null)
            {
                Debug.LogError("[TilePool] Tile prefab is not assigned!");
                return;
            }
            
            // Create pool parent if not assigned
            if (poolParent == null)
            {
                var poolObject = new GameObject("TilePool");
                poolObject.transform.SetParent(transform);
                poolParent = poolObject.transform;
            }
            
            // Pre-populate the pool
            for (int i = 0; i < initialPoolSize; i++)
            {
                var tile = CreateTileInstance();
                tile.SetActive(false);
                availableTiles.Push(tile);
            }
            
            Debug.Log($"[TilePool] Initialized with {initialPoolSize} tiles");
        }
        
        /// <summary>
        /// Gets a tile from the pool or creates a new one if needed.
        /// </summary>
        /// <returns>A tile GameObject ready for use.</returns>
        public GameObject GetTile()
        {
            GameObject tile;
            
            if (availableTiles.Count > 0)
            {
                tile = availableTiles.Pop();
            }
            else
            {
                tile = CreateTileInstance();
                Debug.Log("[TilePool] Pool exhausted, creating new tile instance");
            }
            
            tile.SetActive(true);
            activeTiles.Add(tile);
            
            return tile;
        }
        
        /// <summary>
        /// Returns a tile to the pool for reuse.
        /// </summary>
        /// <param name="tile">The tile to return to the pool.</param>
        public void ReturnTile(GameObject tile)
        {
            if (tile == null) return;
            
            var pooledTile = tile.GetComponent<PooledTile>();
            if (pooledTile == null || pooledTile.Pool != this)
            {
                Debug.LogWarning("[TilePool] Attempted to return tile that doesn't belong to this pool");
                return;
            }
            
            if (!activeTiles.Remove(tile))
            {
                Debug.LogWarning("[TilePool] Attempted to return tile that wasn't active");
                return;
            }
            
            // Reset tile state
            var tileVisual = tile.GetComponent<TileVisual>();
            if (tileVisual != null)
            {
                tileVisual.ResetTile();
            }
            
            tile.SetActive(false);
            availableTiles.Push(tile);
        }
        
        /// <summary>
        /// Returns all active tiles to the pool.
        /// </summary>
        public void ReturnAllTiles()
        {
            var tilesToReturn = new List<GameObject>(activeTiles);
            foreach (var tile in tilesToReturn)
            {
                ReturnTile(tile);
            }
        }
        
        /// <summary>
        /// Gets the number of active tiles.
        /// </summary>
        public int ActiveTileCount => activeTiles.Count;
        
        /// <summary>
        /// Gets the number of available tiles in the pool.
        /// </summary>
        public int AvailableTileCount => availableTiles.Count;
        
        /// <summary>
        /// Creates a new tile instance with required components.
        /// </summary>
        private GameObject CreateTileInstance()
        {
            var tile = Instantiate(tilePrefab, poolParent);
            
            // Ensure required components exist
            var pooledTile = tile.GetComponent<PooledTile>();
            if (pooledTile == null)
            {
                pooledTile = tile.AddComponent<PooledTile>();
            }
            pooledTile.Pool = this;
            
            var tileVisual = tile.GetComponent<TileVisual>();
            if (tileVisual == null)
            {
                tileVisual = tile.AddComponent<TileVisual>();
            }
            
            return tile;
        }
        
        private void OnDestroy()
        {
            // Clean up all tiles when pool is destroyed
            ReturnAllTiles();
        }
    }
}