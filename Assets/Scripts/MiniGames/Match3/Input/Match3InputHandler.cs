using System.Collections;
using UnityEngine;
using Core.Architecture;
using MiniGameFramework.MiniGames.Match3.Data;
using MiniGameFramework.MiniGames.Match3.Utils;
using System.Collections.Generic;
using System.Linq; // Added for .Any()

namespace MiniGameFramework.MiniGames.Match3.Input
{
    /// <summary>
    /// Handles all input logic for Match3 game.
    /// Extracted from Match3Game to follow separation of concerns.
    /// </summary>
    public class Match3InputHandler
    {
        private readonly IEventBus eventBus;
        private readonly Match3FoundationManager foundationManager;
        
        // Input state
        private GameObject selectedTile = null;
        private bool isDragging = false;
        private bool inputLocked = false;
        
        // Configuration
        private readonly float tileSize;
        private readonly float swapDuration;
        
        // Game state for validation
        private List<Swap> possibleSwaps = new List<Swap>();
        
        public Match3InputHandler(
            IEventBus eventBus, 
            Match3FoundationManager foundationManager,
            float tileSize, 
            float swapDuration)
        {
            this.eventBus = eventBus;
            this.foundationManager = foundationManager;
            this.tileSize = tileSize;
            this.swapDuration = swapDuration;
            
            Debug.Log("[Match3InputHandler] ✅ Input handler initialized");
        }
        
        /// <summary>
        /// Updates the possible swaps list for validation.
        /// </summary>
        /// <param name="swaps">The list of valid swaps.</param>
        public void UpdatePossibleSwaps(List<Swap> swaps)
        {
            possibleSwaps = swaps ?? new List<Swap>();
        }
        
        /// <summary>
        /// Processes input for the current frame.
        /// </summary>
        /// <param name="isProcessingMatches">Whether matches are being processed.</param>
        /// <param name="isSwapping">Whether a swap is in progress.</param>
        /// <returns>Input result containing any detected actions.</returns>
        public InputResult ProcessInput(bool isProcessingMatches, bool isSwapping)
        {
            if (inputLocked || isProcessingMatches || isSwapping)
            {
                return InputResult.None();
            }
            
            var result = new InputResult();
            
            // Mouse Down - Select Tile
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                var worldPos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                var hit = Physics2D.Raycast(worldPos, Vector2.zero);
                
                if (hit.collider?.CompareTag("Tile") == true)
                {
                    SelectTile(hit.collider.gameObject);
                    isDragging = true;
                    result.TileSelected = true;
                    result.SelectedTile = hit.collider.gameObject;
                    
                    Debug.Log($"[Match3InputHandler] 🎯 Tile selected: {hit.collider.gameObject.name}");
                }
            }
            
            // Mouse Up - End Selection
            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                DeselectTile();
                result.TileDeselected = true;
            }
            
            // Drag Detection - Attempt Swap
            if (isDragging && selectedTile != null)
            {
                var worldPos = Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                var hit = Physics2D.Raycast(worldPos, Vector2.zero);
                
                if (hit.collider?.CompareTag("Tile") == true && hit.collider.gameObject != selectedTile)
                {
                    var targetTile = hit.collider.gameObject;
                    var swap = CreateSwapFromTiles(selectedTile, targetTile);
                    
                    Debug.Log($"[Match3InputHandler] 🎯 Attempting swap: {swap.tileA} ↔ {swap.tileB}");
                    
                    if (IsValidSwap(swap))
                    {
                        result.SwapDetected = true;
                        result.DetectedSwap = swap;
                        result.IsValidSwap = true;
                        
                        Debug.Log($"[Match3InputHandler] ✅ Valid swap detected!");
                    }
                    else
                    {
                        result.SwapDetected = true;
                        result.DetectedSwap = swap;
                        result.IsValidSwap = false;
                        result.InvalidSwapTiles = (selectedTile, targetTile);
                        
                        Debug.Log($"[Match3InputHandler] ❌ Invalid swap detected!");
                    }
                    
                    // Reset drag state
                    isDragging = false;
                    DeselectTile();
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Creates a swap from two tile GameObjects.
        /// </summary>
        /// <param name="tileA">First tile.</param>
        /// <param name="tileB">Second tile.</param>
        /// <returns>The swap.</returns>
        private Swap CreateSwapFromTiles(GameObject tileA, GameObject tileB)
        {
            var posA = GetTileBoardPosition(tileA);
            var posB = GetTileBoardPosition(tileB);
            
            Debug.Log($"[Match3InputHandler] 📍 Tile A position: {posA}");
            Debug.Log($"[Match3InputHandler] 📍 Tile B position: {posB}");
            
            return new Swap(posA, posB);
        }
        
        /// <summary>
        /// Gets the board position of a tile using foundation manager.
        /// </summary>
        /// <param name="tile">The tile GameObject.</param>
        /// <returns>The board position.</returns>
        private Vector2Int GetTileBoardPosition(GameObject tile)
        {
            if (foundationManager != null)
            {
                var position = foundationManager.GetTilePosition(tile);
                if (position != Vector2Int.zero)
                {
                    Debug.Log($"[Match3InputHandler] 📍 Cache hit for {tile?.name}: {position}");
                    return position;
                }
                else
                {
                    Debug.LogWarning($"[Match3InputHandler] ⚠️ Position cache miss for {tile?.name}, foundation manager returned zero");
                }
            }
            else
            {
                Debug.LogWarning($"[Match3InputHandler] ⚠️ Foundation manager is null for {tile?.name}");
            }
            
            // Fallback: Search in visual tiles array
            Debug.LogWarning($"[Match3InputHandler] ⚠️ Position cache miss for {tile?.name}, performing fallback search...");
            
            // This is a temporary fallback - in a real implementation, we'd have access to visualTiles
            // For now, we'll try to extract position from the tile's transform
            if (tile != null)
            {
                var worldPos = tile.transform.position;
                var boardPos = new Vector2Int(
                    Mathf.RoundToInt(worldPos.x / tileSize),
                    Mathf.RoundToInt(worldPos.y / tileSize)
                );
                
                Debug.Log($"[Match3InputHandler] 📍 Fallback position for {tile.name}: " +
                         $"WorldPos={worldPos}, TileSize={tileSize}, BoardPos={boardPos}");
                return boardPos;
            }
            
            Debug.LogError($"[Match3InputHandler] ❌ Tile is null, returning zero position");
            return Vector2Int.zero;
        }
        
        /// <summary>
        /// Checks if a swap is valid (adjacent tiles that would create matches).
        /// </summary>
        /// <param name="swap">The swap to validate.</param>
        /// <returns>True if the swap is valid.</returns>
        private bool IsValidSwap(Swap swap)
        {
            // Check if tiles are adjacent
            int deltaX = Mathf.Abs(swap.tileA.x - swap.tileB.x);
            int deltaY = Mathf.Abs(swap.tileA.y - swap.tileB.y);
            bool isAdjacent = (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
            
            if (!isAdjacent)
            {
                return false;
            }
            
            // Check if this swap is in the possible swaps list
            bool isValidSwap = possibleSwaps.Any(s => 
                (s.tileA == swap.tileA && s.tileB == swap.tileB) ||
                (s.tileA == swap.tileB && s.tileB == swap.tileA)
            );
            
            return isValidSwap;
        }
        
        /// <summary>
        /// Selects a tile with visual feedback.
        /// </summary>
        /// <param name="tile">The tile to select.</param>
        private void SelectTile(GameObject tile)
        {
            selectedTile = tile;
            var spriteRenderer = tile.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.yellow; // Highlight selected tile
            }
        }
        
        /// <summary>
        /// Deselects current tile and removes visual feedback.
        /// </summary>
        private void DeselectTile()
        {
            if (selectedTile != null)
            {
                var spriteRenderer = selectedTile.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white; // Reset color
                }
                selectedTile = null;
            }
        }
        
        /// <summary>
        /// Locks input processing.
        /// </summary>
        public void LockInput()
        {
            inputLocked = true;
            Debug.Log("[Match3InputHandler] 🔒 Input locked");
        }
        
        /// <summary>
        /// Unlocks input processing.
        /// </summary>
        public void UnlockInput()
        {
            inputLocked = false;
            Debug.Log("[Match3InputHandler] 🔓 Input unlocked");
        }
        
        /// <summary>
        /// Forces deselection of current tile.
        /// </summary>
        public void ForceDeselect()
        {
            DeselectTile();
            isDragging = false;
        }
        
        /// <summary>
        /// Gets the current input state summary.
        /// </summary>
        /// <returns>Input state summary.</returns>
        public string GetInputStateSummary()
        {
            return $"[Match3InputHandler] Input State:\n" +
                   $"  - Selected Tile: {(selectedTile != null ? selectedTile.name : "None")}\n" +
                   $"  - Is Dragging: {isDragging}\n" +
                   $"  - Input Locked: {inputLocked}";
        }
    }
    
    /// <summary>
    /// Result of input processing.
    /// </summary>
    public class InputResult
    {
        public bool TileSelected { get; set; }
        public bool TileDeselected { get; set; }
        public bool SwapDetected { get; set; }
        public bool IsValidSwap { get; set; }
        public Swap DetectedSwap { get; set; }
        public (GameObject tileA, GameObject tileB)? InvalidSwapTiles { get; set; }
        public GameObject SelectedTile { get; set; }
        
        public static InputResult None()
        {
            return new InputResult();
        }
    }
}