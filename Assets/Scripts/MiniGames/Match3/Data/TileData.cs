using UnityEngine;

namespace MiniGameFramework.MiniGames.Match3.Data
{
    /// <summary>
    /// Enum defining the 6 basic tile types for Match3.
    /// </summary>
    public enum TileType
    {
        Empty = 0,
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4,
        Purple = 5,
        Orange = 6
    }
    
    /// <summary>
    /// Lightweight readonly struct representing a tile on the board.
    /// </summary>
    public readonly struct TileData : System.IEquatable<TileData>
    {
        public TileType Type { get; }
        public Vector2Int Position { get; }
        public bool IsMoving { get; }
        public bool IsMatched { get; }
        
        public TileData(TileType type, Vector2Int position, bool isMoving = false, bool isMatched = false)
        {
            Type = type;
            Position = position;
            IsMoving = isMoving;
            IsMatched = isMatched;
        }
        
        public TileData WithType(TileType newType) => new TileData(newType, Position, IsMoving, IsMatched);
        public TileData WithPosition(Vector2Int newPosition) => new TileData(Type, newPosition, IsMoving, IsMatched);
        public TileData WithMoving(bool moving) => new TileData(Type, Position, moving, IsMatched);
        public TileData WithMatched(bool matched) => new TileData(Type, Position, IsMoving, matched);
        
        public bool IsEmpty => Type == TileType.Empty;
        public bool IsValid => Type != TileType.Empty;
        
        // IEquatable implementation
        public bool Equals(TileData other)
        {
            return Type == other.Type && Position.Equals(other.Position) && IsMoving == other.IsMoving && IsMatched == other.IsMatched;
        }
        
        public override bool Equals(object obj)
        {
            return obj is TileData other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return System.HashCode.Combine(Type, Position, IsMoving, IsMatched);
        }
        
        public static bool operator ==(TileData left, TileData right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(TileData left, TileData right)
        {
            return !left.Equals(right);
        }
    }
    
    /// <summary>
    /// Readonly struct representing the entire game board state.
    /// </summary>
    public readonly struct BoardData : System.IEquatable<BoardData>
    {
        public const int BOARD_SIZE = 6;
        
        public TileData[,] Tiles { get; }
        public int Width { get; }
        public int Height { get; }
        
        public BoardData(int width = BOARD_SIZE, int height = BOARD_SIZE)
        {
            Width = width;
            Height = height;
            Tiles = new TileData[width, height];
        }
        
        public BoardData(TileData[,] tiles)
        {
            Tiles = tiles;
            Width = tiles.GetLength(0);
            Height = tiles.GetLength(1);
        }
        
        public TileData GetTile(int x, int y)
        {
            if (IsValidPosition(x, y))
                return Tiles[x, y];
            return new TileData(TileType.Empty, new Vector2Int(x, y));
        }
        
        public TileData GetTile(Vector2Int position) => GetTile(position.x, position.y);
        
        public bool IsValidPosition(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
        public bool IsValidPosition(Vector2Int position) => IsValidPosition(position.x, position.y);
        
        public BoardData SetTile(int x, int y, TileData tile)
        {
            if (!IsValidPosition(x, y)) return this;
            
            var newTiles = (TileData[,])Tiles.Clone();
            newTiles[x, y] = tile;
            return new BoardData(newTiles);
        }
        
        public BoardData SetTile(Vector2Int position, TileData tile) => SetTile(position.x, position.y, tile);
        
        // IEquatable implementation
        public bool Equals(BoardData other)
        {
            if (Width != other.Width || Height != other.Height)
                return false;
            
            if (Tiles == null && other.Tiles == null)
                return true;
            
            if (Tiles == null || other.Tiles == null)
                return false;
            
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (!Tiles[x, y].Equals(other.Tiles[x, y]))
                        return false;
                }
            }
            
            return true;
        }
        
        public override bool Equals(object obj)
        {
            return obj is BoardData other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            var hash = System.HashCode.Combine(Width, Height);
            
            if (Tiles != null)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        hash = System.HashCode.Combine(hash, Tiles[x, y]);
                    }
                }
            }
            
            return hash;
        }
        
        public static bool operator ==(BoardData left, BoardData right)
        {
            return left.Equals(right);
        }
        
        public static bool operator !=(BoardData left, BoardData right)
        {
            return !left.Equals(right);
        }
    }
}