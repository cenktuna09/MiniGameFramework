using UnityEngine;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.MiniGames.Match3.Data;

namespace MiniGameFramework.MiniGames.Match3
{
    /// <summary>
    /// Event fired when a tile is selected in Match3.
    /// </summary>
    public class TileSelectedEvent : GameEvent
    {
        public Vector2Int BoardPosition { get; }
        public TileType TileType { get; }
        
        public TileSelectedEvent(Vector2Int boardPosition, TileType tileType)
        {
            BoardPosition = boardPosition;
            TileType = tileType;
        }
    }
    
    /// <summary>
    /// Event fired when tiles are swapped in Match3.
    /// </summary>
    public class TilesSwappedEvent : GameEvent
    {
        public Vector2Int FromPosition { get; }
        public Vector2Int ToPosition { get; }
        public bool WasValidSwap { get; }
        
        public TilesSwappedEvent(Vector2Int fromPosition, Vector2Int toPosition, bool wasValidSwap)
        {
            FromPosition = fromPosition;
            ToPosition = toPosition;
            WasValidSwap = wasValidSwap;
        }
    }
    
    /// <summary>
    /// Event fired when matches are found on the board.
    /// </summary>
    public class MatchFoundEvent : GameEvent
    {
        public Vector2Int[] MatchedPositions { get; }
        public TileType MatchedTileType { get; }
        public int MatchLength { get; }
        
        public MatchFoundEvent(Vector2Int[] matchedPositions, TileType matchedTileType)
        {
            MatchedPositions = matchedPositions;
            MatchedTileType = matchedTileType;
            MatchLength = matchedPositions.Length;
        }
    }
    
    /// <summary>
    /// Event fired when the board needs to be refilled after matches.
    /// </summary>
    public class BoardRefillEvent : GameEvent
    {
        public Vector2Int[] EmptyPositions { get; }
        
        public BoardRefillEvent(Vector2Int[] emptyPositions)
        {
            EmptyPositions = emptyPositions;
        }
    }
}