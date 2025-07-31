using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MiniGameFramework.MiniGames.Match3.Data;

namespace MiniGameFramework.MiniGames.Match3.Board
{
    /// <summary>
    /// Generates Match3 boards using constraint-based algorithm to prevent pre-existing matches.
    /// Ensures at least one valid move is always available.
    /// </summary>
    public static class BoardGenerator
    {
        private static readonly TileType[] ValidTileTypes = 
        {
            TileType.Red, TileType.Blue, TileType.Green, 
            TileType.Yellow, TileType.Purple, TileType.Orange
        };
        
        /// <summary>
        /// Generates a new board with no pre-existing matches and at least one valid move.
        /// </summary>
        /// <param name="width">Width of the board.</param>
        /// <param name="height">Height of the board.</param>
        /// <param name="maxAttempts">Maximum attempts to generate a valid board.</param>
        /// <returns>A valid board data with no pre-existing matches.</returns>
        public static BoardData GenerateBoard(int width = BoardData.BOARD_SIZE, int height = BoardData.BOARD_SIZE, int maxAttempts = 100)
        {
            BoardData board;
            int attempts = 0;
            
            do
            {
                board = GenerateBoardInternal(width, height);
                attempts++;
                
                if (attempts >= maxAttempts)
                {
                    Debug.LogWarning($"[BoardGenerator] Could not generate valid board after {maxAttempts} attempts. Using current board.");
                    break;
                }
            }
            while (!IsValidBoard(board));
            
            Debug.Log($"[BoardGenerator] Generated valid board in {attempts} attempts");
            return board;
        }
        
        /// <summary>
        /// Internal method to generate a board using constraint-based algorithm.
        /// </summary>
        private static BoardData GenerateBoardInternal(int width, int height)
        {
            var tiles = new TileData[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var validTypes = GetValidTileTypes(tiles, x, y, width, height);
                    var selectedType = validTypes[Random.Range(0, validTypes.Count)];
                    
                    tiles[x, y] = new TileData(selectedType, new Vector2Int(x, y));
                }
            }
            
            return new BoardData(tiles);
        }
        
        /// <summary>
        /// Gets valid tile types for a position that won't create immediate matches.
        /// </summary>
        /// <param name="tiles">Current board state.</param>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="width">Board width.</param>
        /// <param name="height">Board height.</param>
        /// <returns>List of valid tile types.</returns>
        private static List<TileType> GetValidTileTypes(TileData[,] tiles, int x, int y, int width, int height)
        {
            var validTypes = new List<TileType>(ValidTileTypes);
            var forbiddenTypes = new HashSet<TileType>();
            
            // Check horizontal constraints (left side)
            var leftTile1 = GetTile(tiles, x - 1, y, width, height);
            var leftTile2 = GetTile(tiles, x - 2, y, width, height);
            
            if (leftTile1.IsValid && leftTile2.IsValid && leftTile1.Type == leftTile2.Type)
            {
                forbiddenTypes.Add(leftTile1.Type);
            }
            
            // Check vertical constraints (top side)  
            var topTile1 = GetTile(tiles, x, y - 1, width, height);
            var topTile2 = GetTile(tiles, x, y - 2, width, height);
            
            if (topTile1.IsValid && topTile2.IsValid && topTile1.Type == topTile2.Type)
            {
                forbiddenTypes.Add(topTile1.Type);
            }
            
            // Remove forbidden types
            validTypes.RemoveAll(type => forbiddenTypes.Contains(type));
            
            // Ensure we always have at least one valid type
            if (validTypes.Count == 0)
            {
                validTypes.Add(ValidTileTypes[Random.Range(0, ValidTileTypes.Length)]);
                Debug.LogWarning($"[BoardGenerator] No valid types at position ({x}, {y}), using random type");
            }
            
            return validTypes;
        }
        
        /// <summary>
        /// Safely gets a tile from the board, returning empty tile if out of bounds.
        /// </summary>
        private static TileData GetTile(TileData[,] tiles, int x, int y, int width, int height)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return new TileData(TileType.Empty, new Vector2Int(x, y));
                
            return tiles[x, y];
        }
        
        /// <summary>
        /// Validates that the board has no pre-existing matches and at least one valid move.
        /// </summary>
        /// <param name="board">Board to validate.</param>
        /// <returns>True if board is valid.</returns>
        private static bool IsValidBoard(BoardData board)
        {
            // Check for pre-existing matches
            if (HasMatches(board))
            {
                return false;
            }
            
            // Check for at least one valid move
            if (!HasValidMoves(board))
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Checks if the board has any existing matches.
        /// </summary>
        /// <param name="board">Board to check.</param>
        /// <returns>True if matches exist.</returns>
        private static bool HasMatches(BoardData board)
        {
            // Check horizontal matches
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width - 2; x++)
                {
                    var tile1 = board.GetTile(x, y);
                    var tile2 = board.GetTile(x + 1, y);
                    var tile3 = board.GetTile(x + 2, y);
                    
                    if (tile1.IsValid && tile1.Type == tile2.Type && tile2.Type == tile3.Type)
                    {
                        return true;
                    }
                }
            }
            
            // Check vertical matches
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height - 2; y++)
                {
                    var tile1 = board.GetTile(x, y);
                    var tile2 = board.GetTile(x, y + 1);
                    var tile3 = board.GetTile(x, y + 2);
                    
                    if (tile1.IsValid && tile1.Type == tile2.Type && tile2.Type == tile3.Type)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Checks if the board has at least one valid move.
        /// </summary>
        /// <param name="board">Board to check.</param>
        /// <returns>True if valid moves exist.</returns>
        private static bool HasValidMoves(BoardData board)
        {
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    // Check adjacent positions for valid swaps
                    var positions = new Vector2Int[]
                    {
                        new Vector2Int(x + 1, y), // Right
                        new Vector2Int(x, y + 1), // Up
                        new Vector2Int(x - 1, y), // Left
                        new Vector2Int(x, y - 1)  // Down
                    };
                    
                    foreach (var adjacentPos in positions)
                    {
                        if (board.IsValidPosition(adjacentPos))
                        {
                            // Simulate swap and check for matches
                            var swappedBoard = SimulateSwap(board, new Vector2Int(x, y), adjacentPos);
                            if (HasMatches(swappedBoard))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Simulates a tile swap and returns the resulting board.
        /// </summary>
        /// <param name="board">Original board.</param>
        /// <param name="pos1">First position.</param>
        /// <param name="pos2">Second position.</param>
        /// <returns>Board after simulated swap.</returns>
        private static BoardData SimulateSwap(BoardData board, Vector2Int pos1, Vector2Int pos2)
        {
            var tile1 = board.GetTile(pos1);
            var tile2 = board.GetTile(pos2);
            
            var newBoard = board.SetTile(pos1, tile2.WithPosition(pos1));
            newBoard = newBoard.SetTile(pos2, tile1.WithPosition(pos2));
            
            return newBoard;
        }
    }
}