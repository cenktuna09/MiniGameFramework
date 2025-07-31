using System;
using UnityEngine;
using MiniGameFramework.MiniGames.Match3.Data;

namespace MiniGameFramework.MiniGames.Match3.Input.Commands
{
    /// <summary>
    /// Base interface for all Match3 commands.
    /// Implements the Command pattern for input handling.
    /// </summary>
    public interface IMatch3Command
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>True if the command was executed successfully.</returns>
        bool Execute();
        
        /// <summary>
        /// Undoes the command (if possible).
        /// </summary>
        void Undo();
        
        /// <summary>
        /// Gets the command type for identification.
        /// </summary>
        CommandType Type { get; }
        
        /// <summary>
        /// Gets the timestamp when the command was created.
        /// </summary>
        float Timestamp { get; }
        
        /// <summary>
        /// Gets a string representation of the command for debugging.
        /// </summary>
        string GetDebugInfo();
    }
    
    /// <summary>
    /// Types of Match3 commands.
    /// </summary>
    public enum CommandType
    {
        TileSelect,
        TileDeselect,
        SwapTiles,
        InvalidSwap,
        HintRequest,
        GamePause,
        GameResume
    }
    
    /// <summary>
    /// Base class for all Match3 commands with common functionality.
    /// </summary>
    public abstract class Match3Command : IMatch3Command
    {
        public CommandType Type { get; protected set; }
        public float Timestamp { get; protected set; }
        
        protected Match3Command(CommandType type)
        {
            Type = type;
            Timestamp = Time.time;
        }
        
        public abstract bool Execute();
        public abstract void Undo();
        
        /// <summary>
        /// Gets a string representation of the command for debugging.
        /// </summary>
        public virtual string GetDebugInfo()
        {
            return $"[{Type}] at {Timestamp:F2}s";
        }
    }
    
    /// <summary>
    /// Command for selecting a tile.
    /// </summary>
    public class TileSelectCommand : Match3Command
    {
        private readonly GameObject tile;
        private readonly Vector2Int boardPosition;
        private bool wasExecuted = false;
        
        public TileSelectCommand(GameObject tile, Vector2Int boardPosition) 
            : base(CommandType.TileSelect)
        {
            this.tile = tile;
            this.boardPosition = boardPosition;
        }
        
        public override bool Execute()
        {
            if (wasExecuted) return false;
            
            Debug.Log($"[TileSelectCommand] 🎯 Selecting tile at {boardPosition}");
            wasExecuted = true;
            return true;
        }
        
        public override void Undo()
        {
            if (!wasExecuted) return;
            
            Debug.Log($"[TileSelectCommand] ↩️ Deselecting tile at {boardPosition}");
            wasExecuted = false;
        }
        
        public override string GetDebugInfo()
        {
            return $"[{Type}] Tile at {boardPosition} - Executed: {wasExecuted}";
        }
    }
    
    /// <summary>
    /// Command for deselecting a tile.
    /// </summary>
    public class TileDeselectCommand : Match3Command
    {
        private readonly GameObject tile;
        private readonly Vector2Int boardPosition;
        private bool wasExecuted = false;
        
        public TileDeselectCommand(GameObject tile, Vector2Int boardPosition) 
            : base(CommandType.TileDeselect)
        {
            this.tile = tile;
            this.boardPosition = boardPosition;
        }
        
        public override bool Execute()
        {
            if (wasExecuted) return false;
            
            Debug.Log($"[TileDeselectCommand] ❌ Deselecting tile at {boardPosition}");
            wasExecuted = true;
            return true;
        }
        
        public override void Undo()
        {
            if (!wasExecuted) return;
            
            Debug.Log($"[TileDeselectCommand] ↩️ Reselecting tile at {boardPosition}");
            wasExecuted = false;
        }
        
        public override string GetDebugInfo()
        {
            return $"[{Type}] Tile at {boardPosition} - Executed: {wasExecuted}";
        }
    }
    
    /// <summary>
    /// Command for swapping tiles.
    /// </summary>
    public class SwapTilesCommand : Match3Command
    {
        private readonly Swap swap;
        private readonly bool isValid;
        private bool wasExecuted = false;
        
        public SwapTilesCommand(Swap swap, bool isValid) 
            : base(CommandType.SwapTiles)
        {
            this.swap = swap;
            this.isValid = isValid;
        }
        
        public override bool Execute()
        {
            if (wasExecuted) return false;
            
            var status = isValid ? "✅ Valid" : "❌ Invalid";
            Debug.Log($"[SwapTilesCommand] {status} swap: {swap.tileA} ↔ {swap.tileB}");
            wasExecuted = true;
            return isValid;
        }
        
        public override void Undo()
        {
            if (!wasExecuted) return;
            
            Debug.Log($"[SwapTilesCommand] ↩️ Undoing swap: {swap.tileA} ↔ {swap.tileB}");
            wasExecuted = false;
        }
        
        public override string GetDebugInfo()
        {
            return $"[{Type}] {swap.tileA} ↔ {swap.tileB} - Valid: {isValid}, Executed: {wasExecuted}";
        }
    }
    
    /// <summary>
    /// Command for invalid swap attempts.
    /// </summary>
    public class InvalidSwapCommand : Match3Command
    {
        private readonly Swap swap;
        private readonly string reason;
        private bool wasExecuted = false;
        
        public InvalidSwapCommand(Swap swap, string reason) 
            : base(CommandType.InvalidSwap)
        {
            this.swap = swap;
            this.reason = reason;
        }
        
        public override bool Execute()
        {
            if (wasExecuted) return false;
            
            Debug.Log($"[InvalidSwapCommand] ❌ Invalid swap: {swap.tileA} ↔ {swap.tileB} - Reason: {reason}");
            wasExecuted = true;
            return true;
        }
        
        public override void Undo()
        {
            if (!wasExecuted) return;
            
            Debug.Log($"[InvalidSwapCommand] ↩️ Undoing invalid swap: {swap.tileA} ↔ {swap.tileB}");
            wasExecuted = false;
        }
        
        public override string GetDebugInfo()
        {
            return $"[{Type}] {swap.tileA} ↔ {swap.tileB} - Reason: {reason}, Executed: {wasExecuted}";
        }
    }
    
    /// <summary>
    /// Command for requesting a hint.
    /// </summary>
    public class HintRequestCommand : Match3Command
    {
        private bool wasExecuted = false;
        
        public HintRequestCommand() : base(CommandType.HintRequest)
        {
        }
        
        public override bool Execute()
        {
            if (wasExecuted) return false;
            
            Debug.Log("[HintRequestCommand] 💡 Requesting hint");
            wasExecuted = true;
            return true;
        }
        
        public override void Undo()
        {
            if (!wasExecuted) return;
            
            Debug.Log("[HintRequestCommand] ↩️ Undoing hint request");
            wasExecuted = false;
        }
        
        public override string GetDebugInfo()
        {
            return $"[{Type}] Hint request - Executed: {wasExecuted}";
        }
    }
} 