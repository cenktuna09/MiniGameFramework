using UnityEngine;

namespace MiniGameFramework.MiniGames.Match3.Pooling
{
    /// <summary>
    /// Component that identifies a pooled tile and tracks its pool origin.
    /// </summary>
    public class PooledTile : MonoBehaviour
    {
        /// <summary>
        /// The pool that owns this tile.
        /// </summary>
        public TilePool Pool { get; set; }
        
        /// <summary>
        /// Returns this tile to its origin pool.
        /// </summary>
        public void ReturnToPool()
        {
            if (Pool != null)
            {
                Pool.ReturnTile(gameObject);
            }
            else
            {
                Debug.LogWarning("[PooledTile] Attempted to return tile with no assigned pool");
            }
        }
    }
}