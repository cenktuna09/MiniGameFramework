using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MiniGameFramework.MiniGames.Match3.Data;

namespace MiniGameFramework.MiniGames.Match3.Logic
{
    /// <summary>
    /// Detects matches on the Match3 board.
    /// Handles both horizontal and vertical matches of 3 or more tiles.
    /// </summary>
    public static class MatchDetector
    {
        /// <summary>
        /// Represents a match found on the board.
        /// </summary>
        public readonly struct Match
        {
            public Vector2Int[] Positions { get; }
            public TileType TileType { get; }
            public bool IsHorizontal { get; }
            public int Length => Positions.Length;
            
            public Match(Vector2Int[] positions, TileType tileType, bool isHorizontal)
            {
                Positions = positions;
                TileType = tileType;
                IsHorizontal = isHorizontal;
            }
        }
        
        /// <summary>
        /// Finds all matches on the board.
        /// </summary>
        /// <param name="board">Board to check for matches.</param>
        /// <returns>List of all matches found.</returns>
        public static List<Match> FindMatches(BoardData board)
        {
            var matches = new List<Match>();
            
            // Find horizontal matches
            matches.AddRange(FindHorizontalMatches(board));
            
            // Find vertical matches
            matches.AddRange(FindVerticalMatches(board));
            
            // Remove duplicate positions (tiles that are part of multiple matches)
            return MergeOverlappingMatches(matches);
        }
        
        /// <summary>
        /// Finds all horizontal matches on the board.
        /// </summary>
        /// <param name="board">Board to check.</param>
        /// <returns>List of horizontal matches.</returns>
        private static List<Match> FindHorizontalMatches(BoardData board)
        {
            var matches = new List<Match>();
            
            for (int y = 0; y < board.Height; y++)
            {
                var currentMatch = new List<Vector2Int>();
                TileType currentType = TileType.Empty;
                
                for (int x = 0; x < board.Width; x++)
                {
                    var tile = board.GetTile(x, y);
                    
                    if (tile.IsValid && tile.Type == currentType)
                    {
                        // Continue current match
                        currentMatch.Add(new Vector2Int(x, y));
                    }
                    else
                    {
                        // Check if we have a valid match to add
                        if (currentMatch.Count >= 3)
                        {
                            matches.Add(new Match(currentMatch.ToArray(), currentType, true));
                        }
                        
                        // Start new potential match
                        currentMatch.Clear();
                        if (tile.IsValid)
                        {
                            currentMatch.Add(new Vector2Int(x, y));
                            currentType = tile.Type;
                        }
                        else
                        {
                            currentType = TileType.Empty;
                        }
                    }
                }
                
                // Check final match in row
                if (currentMatch.Count >= 3)
                {
                    matches.Add(new Match(currentMatch.ToArray(), currentType, true));
                }
            }
            
            return matches;
        }
        
        /// <summary>
        /// Finds all vertical matches on the board.
        /// </summary>
        /// <param name="board">Board to check.</param>
        /// <returns>List of vertical matches.</returns>
        private static List<Match> FindVerticalMatches(BoardData board)
        {
            var matches = new List<Match>();
            
            for (int x = 0; x < board.Width; x++)
            {
                var currentMatch = new List<Vector2Int>();
                TileType currentType = TileType.Empty;
                
                for (int y = 0; y < board.Height; y++)
                {
                    var tile = board.GetTile(x, y);
                    
                    if (tile.IsValid && tile.Type == currentType)
                    {
                        // Continue current match
                        currentMatch.Add(new Vector2Int(x, y));
                    }
                    else
                    {
                        // Check if we have a valid match to add
                        if (currentMatch.Count >= 3)
                        {
                            matches.Add(new Match(currentMatch.ToArray(), currentType, false));
                        }
                        
                        // Start new potential match
                        currentMatch.Clear();
                        if (tile.IsValid)
                        {
                            currentMatch.Add(new Vector2Int(x, y));
                            currentType = tile.Type;
                        }
                        else
                        {
                            currentType = TileType.Empty;
                        }
                    }
                }
                
                // Check final match in column
                if (currentMatch.Count >= 3)
                {
                    matches.Add(new Match(currentMatch.ToArray(), currentType, false));
                }
            }
            
            return matches;
        }
        
        /// <summary>
        /// Merges overlapping matches to avoid counting the same tile multiple times.
        /// </summary>
        /// <param name="matches">List of matches to merge.</param>
        /// <returns>List of merged matches.</returns>
        private static List<Match> MergeOverlappingMatches(List<Match> matches)
        {
            if (matches.Count <= 1)
                return matches;
            
            var mergedMatches = new List<Match>();
            var processedPositions = new HashSet<Vector2Int>();
            
            foreach (var match in matches)
            {
                var newPositions = new List<Vector2Int>();
                
                // Only include positions that haven't been processed yet
                foreach (var position in match.Positions)
                {
                    if (!processedPositions.Contains(position))
                    {
                        newPositions.Add(position);
                        processedPositions.Add(position);
                    }
                }
                
                // Only add match if it still has valid positions
                if (newPositions.Count >= 3)
                {
                    mergedMatches.Add(new Match(newPositions.ToArray(), match.TileType, match.IsHorizontal));
                }
            }
            
            return mergedMatches;
        }
        
        /// <summary>
        /// Checks if a specific swap would create any matches.
        /// </summary>
        /// <param name="board">Current board state.</param>
        /// <param name="pos1">First swap position.</param>
        /// <param name="pos2">Second swap position.</param>
        /// <returns>True if swap would create matches.</returns>
        public static bool WouldSwapCreateMatches(BoardData board, Vector2Int pos1, Vector2Int pos2)
        {
            // Simulate the swap
            var tile1 = board.GetTile(pos1);
            var tile2 = board.GetTile(pos2);
            
            var swappedBoard = board.SetTile(pos1, tile2.WithPosition(pos1));
            swappedBoard = swappedBoard.SetTile(pos2, tile1.WithPosition(pos2));
            
            // Check for matches around the swapped positions
            var matches = FindMatchesAroundPositions(swappedBoard, new[] { pos1, pos2 });
            
            return matches.Count > 0;
        }
        
        /// <summary>
        /// Finds matches around specific positions (optimization for checking only relevant areas).
        /// </summary>
        /// <param name="board">Board to check.</param>
        /// <param name="positions">Positions to check around.</param>
        /// <returns>List of matches found around the positions.</returns>
        public static List<Match> FindMatchesAroundPositions(BoardData board, Vector2Int[] positions)
        {
            var matches = new List<Match>();
            var checkedRows = new HashSet<int>();
            var checkedColumns = new HashSet<int>();
            
            foreach (var position in positions)
            {
                // Check horizontal matches in this row
                if (!checkedRows.Contains(position.y))
                {
                    checkedRows.Add(position.y);
                    matches.AddRange(FindHorizontalMatchesInRow(board, position.y));
                }
                
                // Check vertical matches in this column
                if (!checkedColumns.Contains(position.x))
                {
                    checkedColumns.Add(position.x);
                    matches.AddRange(FindVerticalMatchesInColumn(board, position.x));
                }
            }
            
            return MergeOverlappingMatches(matches);
        }
        
        /// <summary>
        /// Finds horizontal matches in a specific row.
        /// </summary>
        private static List<Match> FindHorizontalMatchesInRow(BoardData board, int row)
        {
            var matches = new List<Match>();
            var currentMatch = new List<Vector2Int>();
            TileType currentType = TileType.Empty;
            
            for (int x = 0; x < board.Width; x++)
            {
                var tile = board.GetTile(x, row);
                
                if (tile.IsValid && tile.Type == currentType)
                {
                    currentMatch.Add(new Vector2Int(x, row));
                }
                else
                {
                    if (currentMatch.Count >= 3)
                    {
                        matches.Add(new Match(currentMatch.ToArray(), currentType, true));
                    }
                    
                    currentMatch.Clear();
                    if (tile.IsValid)
                    {
                        currentMatch.Add(new Vector2Int(x, row));
                        currentType = tile.Type;
                    }
                    else
                    {
                        currentType = TileType.Empty;
                    }
                }
            }
            
            if (currentMatch.Count >= 3)
            {
                matches.Add(new Match(currentMatch.ToArray(), currentType, true));
            }
            
            return matches;
        }
        
        /// <summary>
        /// Finds vertical matches in a specific column.
        /// </summary>
        private static List<Match> FindVerticalMatchesInColumn(BoardData board, int column)
        {
            var matches = new List<Match>();
            var currentMatch = new List<Vector2Int>();
            TileType currentType = TileType.Empty;
            
            for (int y = 0; y < board.Height; y++)
            {
                var tile = board.GetTile(column, y);
                
                if (tile.IsValid && tile.Type == currentType)
                {
                    currentMatch.Add(new Vector2Int(column, y));
                }
                else
                {
                    if (currentMatch.Count >= 3)
                    {
                        matches.Add(new Match(currentMatch.ToArray(), currentType, false));
                    }
                    
                    currentMatch.Clear();
                    if (tile.IsValid)
                    {
                        currentMatch.Add(new Vector2Int(column, y));
                        currentType = tile.Type;
                    }
                    else
                    {
                        currentType = TileType.Empty;
                    }
                }
            }
            
            if (currentMatch.Count >= 3)
            {
                matches.Add(new Match(currentMatch.ToArray(), currentType, false));
            }
            
            return matches;
        }
    }
}