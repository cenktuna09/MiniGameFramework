using UnityEngine;
using Core.Common.ErrorHandling;
using Core.Events;
using EndlessRunner.Events;

namespace EndlessRunner.ErrorHandling
{
    /// <summary>
    /// Error handler for Endless Runner game
    /// Provides safe execution patterns and error logging
    /// </summary>
    public class RunnerErrorHandler : BaseErrorHandler
    {
        #region Constructor
        public RunnerErrorHandler(IEventBus eventBus) : base(eventBus)
        {
            Debug.Log("[RunnerErrorHandler] ✅ Runner error handler initialized");
        }
        #endregion
        
        #region Protected Methods
        /// <summary>
        /// Initialize error handling
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            
            // Set runner-specific error thresholds
            SetMaxErrorCount(10);
            SetMaxWarningCount(20);
            SetErrorHistorySize(50);
            
            Debug.Log("[RunnerErrorHandler] 🛡️ Error handling configured for runner");
        }
        
        /// <summary>
        /// Handle game-specific errors
        /// </summary>
        protected override void HandleError(string errorMessage, string errorType, System.Exception exception = null)
        {
            base.HandleError(errorMessage, errorType, exception);
            
            // Publish error event
            var errorEvent = new GameErrorEvent(errorMessage, errorType, exception);
            _eventBus.Publish(errorEvent);
            
            Debug.LogError($"[RunnerErrorHandler] ❌ Game Error: {errorType} - {errorMessage}");
        }
        
        /// <summary>
        /// Handle game-specific warnings
        /// </summary>
        protected override void HandleWarning(string warningMessage, string warningType)
        {
            base.HandleWarning(warningMessage, warningType);
            
            // Publish warning event
            var warningEvent = new GameWarningEvent(warningMessage, warningType);
            _eventBus.Publish(warningEvent);
            
            Debug.LogWarning($"[RunnerErrorHandler] ⚠️ Game Warning: {warningType} - {warningMessage}");
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Validate runner-specific configurations
        /// </summary>
        public bool ValidateRunnerConfig(RunnerConfig config)
        {
            if (config == null)
            {
                HandleError("RunnerConfig is null", "ConfigurationError");
                return false;
            }
            
            // Validate player settings
            if (config.PlayerSpeed <= 0f)
            {
                HandleWarning("Player speed is zero or negative", "ConfigurationWarning");
                return false;
            }
            
            if (config.JumpForce <= 0f)
            {
                HandleWarning("Jump force is zero or negative", "ConfigurationWarning");
                return false;
            }
            
            if (config.SlideDuration <= 0f)
            {
                HandleWarning("Slide duration is zero or negative", "ConfigurationWarning");
                return false;
            }
            
            Debug.Log("[RunnerErrorHandler] ✅ Runner configuration validated successfully");
            return true;
        }
        
        /// <summary>
        /// Validate player controller
        /// </summary>
        public bool ValidatePlayerController(PlayerController playerController)
        {
            if (playerController == null)
            {
                HandleError("PlayerController is null", "ComponentError");
                return false;
            }
            
            // Check if player has required components
            var rigidbody = playerController.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                HandleError("PlayerController missing Rigidbody component", "ComponentError");
                return false;
            }
            
            var collider = playerController.GetComponent<Collider>();
            if (collider == null)
            {
                HandleError("PlayerController missing Collider component", "ComponentError");
                return false;
            }
            
            Debug.Log("[RunnerErrorHandler] ✅ Player controller validated successfully");
            return true;
        }
        
        /// <summary>
        /// Validate world generator
        /// </summary>
        public bool ValidateWorldGenerator(WorldGenerator worldGenerator)
        {
            if (worldGenerator == null)
            {
                HandleError("WorldGenerator is null", "ComponentError");
                return false;
            }
            
            Debug.Log("[RunnerErrorHandler] ✅ World generator validated successfully");
            return true;
        }
        
        /// <summary>
        /// Get runner-specific error stats
        /// </summary>
        public override string GetErrorStats()
        {
            return $"Runner Error Stats:\n" +
                   $"Errors: {GetErrorCount()} (Max: {GetMaxErrorCount()})\n" +
                   $"Warnings: {GetWarningCount()} (Max: {GetMaxWarningCount()})\n" +
                   $"History Size: {GetErrorHistoryCount()} (Max: {GetErrorHistorySize()})\n" +
                   $"Last Error: {GetLastError()}\n" +
                   $"Last Warning: {GetLastWarning()}";
        }
        #endregion
        
        #region Safe Execution Methods
        /// <summary>
        /// Safely execute player movement
        /// </summary>
        public bool SafeExecutePlayerMovement(System.Action movementAction)
        {
            return SafeExecute(movementAction, "PlayerMovement");
        }
        
        /// <summary>
        /// Safely execute world generation
        /// </summary>
        public bool SafeExecuteWorldGeneration(System.Action generationAction)
        {
            return SafeExecute(generationAction, "WorldGeneration");
        }
        
        /// <summary>
        /// Safely execute scoring update
        /// </summary>
        public bool SafeExecuteScoringUpdate(System.Action scoringAction)
        {
            return SafeExecute(scoringAction, "ScoringUpdate");
        }
        
        /// <summary>
        /// Safely execute input processing
        /// </summary>
        public bool SafeExecuteInputProcessing(System.Action inputAction)
        {
            return SafeExecute(inputAction, "InputProcessing");
        }
        #endregion
    }
} 