using System;
using UnityEngine;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.Core.Input;
using MiniGameFramework.MiniGames.Match3.Data;

namespace MiniGameFramework.MiniGames.Match3.Input
{
    /// <summary>
    /// Handles input processing for Match3 game including swipe detection and tile selection.
    /// Uses EventBus for decoupled communication.
    /// </summary>
    public class Match3InputHandler : MonoBehaviour
    {
        [Header("Input Configuration")]
        [SerializeField] private float minSwipeDistance = 50f;
        [SerializeField] private float maxSwipeTime = 0.5f;
        [SerializeField] private Camera gameCamera;
        [SerializeField] private LayerMask tileLayerMask = -1;
        
        private IEventBus eventBus;
        private Vector2 swipeStartPosition;
        private float swipeStartTime;
        private bool isSwipeStarted = false;
        private Vector2Int? selectedTilePosition;
        
        /// <summary>
        /// Currently selected tile position (if any).
        /// </summary>
        public Vector2Int? SelectedTilePosition => selectedTilePosition;
        
        /// <summary>
        /// Whether a tile is currently selected.
        /// </summary>
        public bool HasSelectedTile => selectedTilePosition.HasValue;
        
        private void Awake()
        {
            if (gameCamera == null)
                gameCamera = Camera.main;
        }
        
        /// <summary>
        /// Initializes the input handler with the event bus.
        /// </summary>
        /// <param name="eventBus">Event bus for communication.</param>
        public void Initialize(IEventBus eventBus)
        {
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            
            // Subscribe to input events
            eventBus.Subscribe<TileClickInputEvent>(OnTileClick);
            eventBus.Subscribe<SwipeInputEvent>(OnSwipeInput);
            
            Debug.Log("[Match3InputHandler] Initialized and subscribed to input events");
        }
        
        /// <summary>
        /// Cleans up event subscriptions.
        /// </summary>
        public void Cleanup()
        {
            if (eventBus != null)
            {
                eventBus.Unsubscribe<TileClickInputEvent>(OnTileClick);
                eventBus.Unsubscribe<SwipeInputEvent>(OnSwipeInput);
            }
            
            selectedTilePosition = null;
        }
        
        /// <summary>
        /// Handles tile click/tap input.
        /// </summary>
        private void OnTileClick(TileClickInputEvent clickEvent)
        {
            var boardPosition = WorldPositionToBoardPosition(clickEvent.WorldPosition);
            
            if (!IsValidBoardPosition(boardPosition))
                return;
            
            if (HasSelectedTile)
            {
                // If clicking the same tile, deselect it
                if (selectedTilePosition == boardPosition)
                {
                    DeselectTile();
                    return;
                }
                
                // Check if clicked tile is adjacent to selected tile
                if (IsAdjacent(selectedTilePosition.Value, boardPosition))
                {
                    // Perform swap
                    PerformTileSwap(selectedTilePosition.Value, boardPosition);
                    DeselectTile();
                }
                else
                {
                    // Select new tile
                    SelectTile(boardPosition);
                }
            }
            else
            {
                // Select tile
                SelectTile(boardPosition);
            }
        }
        
        /// <summary>
        /// Handles swipe input for tile swapping.
        /// </summary>
        private void OnSwipeInput(SwipeInputEvent swipeEvent)
        {
            if (swipeEvent.Distance < minSwipeDistance)
                return;
            
            var startBoardPos = WorldPositionToBoardPosition(swipeEvent.StartPosition);
            var swipeDirection = GetSwipeDirection(swipeEvent.Direction);
            var endBoardPos = startBoardPos + swipeDirection;
            
            if (!IsValidBoardPosition(startBoardPos) || !IsValidBoardPosition(endBoardPos))
                return;
            
            // Perform swap based on swipe
            PerformTileSwap(startBoardPos, endBoardPos);
        }
        
        /// <summary>
        /// Selects a tile at the specified board position.
        /// </summary>
        /// <param name="boardPosition">Board position to select.</param>
        private void SelectTile(Vector2Int boardPosition)
        {
            selectedTilePosition = boardPosition;
            
            // Publish tile selection event
            eventBus?.Publish(new TileSelectedEvent(boardPosition, TileType.Empty)); // TileType will be filled by board manager
            
            Debug.Log($"[Match3InputHandler] Selected tile at {boardPosition}");
        }
        
        /// <summary>
        /// Deselects the currently selected tile.
        /// </summary>
        private void DeselectTile()
        {
            selectedTilePosition = null;
            Debug.Log("[Match3InputHandler] Deselected tile");
        }
        
        /// <summary>
        /// Performs a tile swap between two positions.
        /// </summary>
        /// <param name="fromPosition">Source position.</param>
        /// <param name="toPosition">Target position.</param>
        private void PerformTileSwap(Vector2Int fromPosition, Vector2Int toPosition)
        {
            // Publish tile swap event
            eventBus?.Publish(new TilesSwappedEvent(fromPosition, toPosition, true));
            
            Debug.Log($"[Match3InputHandler] Performed swap: {fromPosition} -> {toPosition}");
        }
        
        /// <summary>
        /// Converts world position to board position.
        /// </summary>
        /// <param name="worldPosition">World position.</param>
        /// <returns>Board position.</returns>
        private Vector2Int WorldPositionToBoardPosition(Vector2 worldPosition)
        {
            // This will need to be adjusted based on your board layout
            // For now, assuming 1 unit = 1 tile and board starts at (0,0)
            var x = Mathf.FloorToInt(worldPosition.x);
            var y = Mathf.FloorToInt(worldPosition.y);
            
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// Converts board position to world position.
        /// </summary>
        /// <param name="boardPosition">Board position.</param>
        /// <returns>World position.</returns>
        public Vector3 BoardPositionToWorldPosition(Vector2Int boardPosition)
        {
            // This will need to be adjusted based on your board layout
            return new Vector3(boardPosition.x, boardPosition.y, 0);
        }
        
        /// <summary>
        /// Checks if a board position is valid.
        /// </summary>
        /// <param name="position">Position to check.</param>
        /// <returns>True if position is valid.</returns>
        private bool IsValidBoardPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < BoardData.BOARD_SIZE &&
                   position.y >= 0 && position.y < BoardData.BOARD_SIZE;
        }
        
        /// <summary>
        /// Checks if two board positions are adjacent.
        /// </summary>
        /// <param name="pos1">First position.</param>
        /// <param name="pos2">Second position.</param>
        /// <returns>True if positions are adjacent.</returns>
        private bool IsAdjacent(Vector2Int pos1, Vector2Int pos2)
        {
            var diff = pos1 - pos2;
            var distance = Mathf.Abs(diff.x) + Mathf.Abs(diff.y);
            
            return distance == 1; // Manhattan distance of 1 means adjacent
        }
        
        /// <summary>
        /// Converts swipe direction to board direction.
        /// </summary>
        /// <param name="swipeDirection">Normalized swipe direction.</param>
        /// <returns>Board direction vector.</returns>
        private Vector2Int GetSwipeDirection(Vector2 swipeDirection)
        {
            var absX = Mathf.Abs(swipeDirection.x);
            var absY = Mathf.Abs(swipeDirection.y);
            
            if (absX > absY)
            {
                // Horizontal swipe
                return swipeDirection.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                // Vertical swipe
                return swipeDirection.y > 0 ? Vector2Int.up : Vector2Int.down;
            }
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
    }
}