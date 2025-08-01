using UnityEngine;
using Core.Architecture;
using MiniGameFramework.MiniGames.Match3.Utils;
using System.Collections.Generic;

namespace MiniGameFramework.MiniGames.Match3.Input
{
    /// <summary>
    /// Input manager that integrates input handling with foundation systems.
    /// Provides a clean interface for Match3Game to use the input system.
    /// </summary>
    public class Match3InputManager
    {
        private readonly IEventBus eventBus;
        private readonly Match3FoundationManager foundationManager;
        private readonly Match3InputHandler inputHandler;
        
        // Configuration
        private readonly float tileSize;
        private readonly float swapDuration;
        
        public Match3InputManager(
            IEventBus eventBus,
            Match3FoundationManager foundationManager,
            float tileSize,
            float swapDuration)
        {
            this.eventBus = eventBus;
            this.foundationManager = foundationManager;
            this.tileSize = tileSize;
            this.swapDuration = swapDuration;
            
            // Initialize input handler
            inputHandler = new Match3InputHandler(eventBus, foundationManager, tileSize, swapDuration);
            
            Debug.Log("[Match3InputManager] ✅ Input manager initialized");
        }
        
        /// <summary>
        /// Processes input for the current frame.
        /// </summary>
        /// <param name="isProcessingMatches">Whether matches are being processed.</param>
        /// <param name="isSwapping">Whether a swap is in progress.</param>
        /// <returns>Input result containing any detected actions.</returns>
        public InputResult ProcessInput(bool isProcessingMatches, bool isSwapping)
        {
            return inputHandler.ProcessInput(isProcessingMatches, isSwapping);
        }
        
        /// <summary>
        /// Locks input processing.
        /// </summary>
        public void LockInput()
        {
            inputHandler.LockInput();
        }
        
        /// <summary>
        /// Unlocks input processing.
        /// </summary>
        public void UnlockInput()
        {
            inputHandler.UnlockInput();
        }
        
        /// <summary>
        /// Forces deselection of current tile.
        /// </summary>
        public void ForceDeselect()
        {
            inputHandler.ForceDeselect();
        }
        
        /// <summary>
        /// Updates the possible swaps list for validation.
        /// </summary>
        /// <param name="swaps">The list of valid swaps.</param>
        public void UpdatePossibleSwaps(List<Swap> swaps)
        {
            inputHandler.UpdatePossibleSwaps(swaps);
        }
        
        /// <summary>
        /// Gets the current input state summary.
        /// </summary>
        /// <returns>Input state summary.</returns>
        public string GetInputStateSummary()
        {
            return inputHandler.GetInputStateSummary();
        }
        
        /// <summary>
        /// Gets a comprehensive status summary including foundation integration.
        /// </summary>
        /// <returns>Status summary string.</returns>
        public string GetStatusSummary()
        {
            return $"[Match3InputManager] Status Summary:\n" +
                   $"  - Input Handler: {inputHandler.GetInputStateSummary()}\n" +
                   $"  - Foundation Manager: {foundationManager.GetStatusSummary()}";
        }
    }
} 