using System.Collections.Generic;
using UnityEngine;
using Core.Architecture;
using Core.Events;
namespace MiniGameFramework.MiniGames.Match3.Events
{
    /// <summary>
    /// Event fired when gravity starts being applied to a column.
    /// </summary>
    public class GravityStartedEvent : GameEvent
    {
        public int Column { get; }
        public int TilesToMove { get; }
        
        public GravityStartedEvent(int column, int tilesToMove, GameObject source = null) 
            : base(source)
        {
            Column = column;
            TilesToMove = tilesToMove;
        }
    }
    
    /// <summary>
    /// Event fired when gravity completes for a specific column.
    /// </summary>
    public class GravityCompletedEvent : GameEvent
    {
        public int Column { get; }
        public int MovedTiles { get; }
        public float Duration { get; }
        
        public GravityCompletedEvent(int column, int movedTiles, float duration, GameObject source = null) 
            : base(source)
        {
            Column = column;
            MovedTiles = movedTiles;
            Duration = duration;
        }
    }
    
    /// <summary>
    /// Event fired when all gravity operations complete across all columns.
    /// </summary>
    public class AllGravityCompletedEvent : GameEvent
    {
        public int TotalColumns { get; }
        public int TotalMovedTiles { get; }
        public float TotalDuration { get; }
        
        public AllGravityCompletedEvent(int totalColumns, int totalMovedTiles, float totalDuration, GameObject source = null) 
            : base(source)
        {
            TotalColumns = totalColumns;
            TotalMovedTiles = totalMovedTiles;
            TotalDuration = totalDuration;
        }
    }
    
    /// <summary>
    /// Event fired when refill starts for a column.
    /// </summary>
    public class RefillStartedEvent : GameEvent
    {
        public int Column { get; }
        public int TilesToSpawn { get; }
        
        public RefillStartedEvent(int column, int tilesToSpawn, GameObject source = null) 
            : base(source)
        {
            Column = column;
            TilesToSpawn = tilesToSpawn;
        }
    }
    
    /// <summary>
    /// Event fired when refill completes for a specific column.
    /// </summary>
    public class RefillCompletedEvent : GameEvent
    {
        public int Column { get; }
        public int SpawnedTiles { get; }
        public float Duration { get; }
        
        public RefillCompletedEvent(int column, int spawnedTiles, float duration, GameObject source = null) 
            : base(source)
        {
            Column = column;
            SpawnedTiles = spawnedTiles;
            Duration = duration;
        }
    }
    
    /// <summary>
    /// Event fired when all refill operations complete across all columns.
    /// </summary>
    public class AllRefillCompletedEvent : GameEvent
    {
        public int TotalColumns { get; }
        public int TotalSpawnedTiles { get; }
        public float TotalDuration { get; }
        
        public AllRefillCompletedEvent(int totalColumns, int totalSpawnedTiles, float totalDuration, GameObject source = null) 
            : base(source)
        {
            TotalColumns = totalColumns;
            TotalSpawnedTiles = totalSpawnedTiles;
            TotalDuration = totalDuration;
        }
    }
    
    /// <summary>
    /// Event fired when a tile movement starts during gravity.
    /// </summary>
    public class TileMovementStartedEvent : GameEvent
    {
        public int Column { get; }
        public Vector2Int FromPosition { get; }
        public Vector2Int ToPosition { get; }
        public GameObject Tile { get; }
        
        public TileMovementStartedEvent(int column, Vector2Int fromPosition, Vector2Int toPosition, GameObject tile, GameObject source = null) 
            : base(source)
        {
            Column = column;
            FromPosition = fromPosition;
            ToPosition = toPosition;
            Tile = tile;
        }
    }
    
    /// <summary>
    /// Event fired when a tile movement completes during gravity.
    /// </summary>
    public class TileMovementCompletedEvent : GameEvent
    {
        public int Column { get; }
        public Vector2Int FromPosition { get; }
        public Vector2Int ToPosition { get; }
        public GameObject Tile { get; }
        public float Duration { get; }
        
        public TileMovementCompletedEvent(int column, Vector2Int fromPosition, Vector2Int toPosition, GameObject tile, float duration, GameObject source = null) 
            : base(source)
        {
            Column = column;
            FromPosition = fromPosition;
            ToPosition = toPosition;
            Tile = tile;
            Duration = duration;
        }
    }
} 