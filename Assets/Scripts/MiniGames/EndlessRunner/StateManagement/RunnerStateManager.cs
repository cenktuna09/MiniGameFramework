using UnityEngine;
using Core.Common.StateManagement;
using Core.Architecture;

namespace EndlessRunner.StateManagement
{
    /// <summary>
    /// State manager for 3D Endless Runner
    /// Extends BaseGameStateManager with runner-specific transition rules
    /// </summary>
    public class RunnerStateManager : BaseGameStateManager<RunnerGameState>
    {
        #region Constructor
        
        /// <summary>
        /// Initialize runner state manager
        /// </summary>
        /// <param name="eventBus">Event bus for state notifications</param>
        public RunnerStateManager(IEventBus eventBus) : base(eventBus)
        {
            Debug.Log("[RunnerStateManager] ✅ Runner state manager initialized");
        }
        
        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// Setup default transition rules for runner game
        /// </summary>
        protected override void SetupDefaultTransitionRules()
        {
            // Ready -> Running (game starts)
            AddTransitionRule(RunnerGameState.Ready, RunnerGameState.Running);
            
            // Running -> Jumping (player jumps)
            AddTransitionRule(RunnerGameState.Running, RunnerGameState.Jumping);
            
            // Running -> Sliding (player slides)
            AddTransitionRule(RunnerGameState.Running, RunnerGameState.Sliding);
            
            // Running -> Paused (game paused)
            AddTransitionRule(RunnerGameState.Running, RunnerGameState.Paused);
            
            // Running -> GameOver (player dies)
            AddTransitionRule(RunnerGameState.Running, RunnerGameState.GameOver);
            
            // Jumping -> Running (landing)
            AddTransitionRule(RunnerGameState.Jumping, RunnerGameState.Running);
            
            // Jumping -> GameOver (death while jumping)
            AddTransitionRule(RunnerGameState.Jumping, RunnerGameState.GameOver);
            
            // Sliding -> Running (slide ends)
            AddTransitionRule(RunnerGameState.Sliding, RunnerGameState.Running);
            
            // Sliding -> GameOver (death while sliding)
            AddTransitionRule(RunnerGameState.Sliding, RunnerGameState.GameOver);
            
            // Paused -> Running (game resumed)
            AddTransitionRule(RunnerGameState.Paused, RunnerGameState.Running);
            
            // Paused -> GameOver (game over while paused)
            AddTransitionRule(RunnerGameState.Paused, RunnerGameState.GameOver);
            
            Debug.Log("[RunnerStateManager] 🔄 Runner transition rules configured");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Check if transition to new state is valid with runner-specific logic
        /// </summary>
        /// <param name="newState">Target state to transition to</param>
        /// <returns>True if transition is valid</returns>
        public override bool CanTransitionTo(RunnerGameState newState)
        {
            Debug.Log($"[RunnerStateManager] 🔄 Checking transition from {CurrentState} to {newState}");
            
            // Runner-specific validation logic
            if (newState == RunnerGameState.Jumping && CurrentState != RunnerGameState.Running)
            {
                Debug.LogWarning($"[RunnerStateManager] ⚠️ Cannot jump from state: {CurrentState}");
                return false;
            }
            
            if (newState == RunnerGameState.Sliding && CurrentState != RunnerGameState.Running)
            {
                Debug.LogWarning($"[RunnerStateManager] ⚠️ Cannot slide from state: {CurrentState}");
                return false;
            }
            
            if (newState == RunnerGameState.Running && CurrentState == RunnerGameState.GameOver)
            {
                Debug.LogWarning("[RunnerStateManager] ⚠️ Cannot return to running from game over");
                return false;
            }
            
            // Use base validation
            bool canTransition = base.CanTransitionTo(newState);
            Debug.Log($"[RunnerStateManager] ✅ Can transition to {newState}: {canTransition}");
            return canTransition;
        }
        
        /// <summary>
        /// Check if player can jump
        /// </summary>
        /// <returns>True if player can jump</returns>
        public bool CanJump()
        {
            return CanTransitionTo(RunnerGameState.Jumping);
        }
        
        /// <summary>
        /// Check if player can slide
        /// </summary>
        /// <returns>True if player can slide</returns>
        public bool CanSlide()
        {
            return CanTransitionTo(RunnerGameState.Sliding);
        }
        
        /// <summary>
        /// Check if game can be paused
        /// </summary>
        /// <returns>True if game can be paused</returns>
        public bool CanPause()
        {
            return CanTransitionTo(RunnerGameState.Paused);
        }
        
        /// <summary>
        /// Check if game can be resumed
        /// </summary>
        /// <returns>True if game can be resumed</returns>
        public bool CanResume()
        {
            return CanTransitionTo(RunnerGameState.Running);
        }
        
        /// <summary>
        /// Check if game can end
        /// </summary>
        /// <returns>True if game can end</returns>
        public bool CanEndGame()
        {
            return CanTransitionTo(RunnerGameState.GameOver);
        }
        
        /// <summary>
        /// Get current state as string for debugging
        /// </summary>
        /// <returns>Current state string</returns>
        public string GetCurrentStateString()
        {
            return $"Current State: {CurrentState}";
        }
        
        #endregion
    }
} 