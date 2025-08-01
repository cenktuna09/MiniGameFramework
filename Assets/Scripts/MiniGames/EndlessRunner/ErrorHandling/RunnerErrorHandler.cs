using System;
using UnityEngine;
using Core.Architecture;
using Core.Common.ErrorHandling;
using Core.Common.ConfigManagement;
using Core.Events;
using EndlessRunner.Events;
using EndlessRunner.Config;
using EndlessRunner.Player;
using EndlessRunner.World;

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
            
            // Set runner-specific error thresholds
            SetMaxErrorCount(10);
            
            Debug.Log("[RunnerErrorHandler] 🛡️ Error handling configured for runner");
        }
        #endregion
        
        #region Abstract Method Implementations
        
        /// <summary>
        /// Execute action safely with error handling
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="operationName">Name of the operation for logging</param>
        public override void SafeExecute(Action action, string operationName)
        {
            try
            {
                action?.Invoke();
                LogInfo($"Successfully executed: {operationName}");
            }
            catch (Exception ex)
            {
                LogError($"Error during {operationName}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Validate animation parameters
        /// </summary>
        /// <param name="animationName">Name of the animation</param>
        /// <param name="target">Target GameObject</param>
        /// <param name="duration">Animation duration</param>
        public override void ValidateAnimation(string animationName, GameObject target, float duration)
        {
            if (string.IsNullOrEmpty(animationName))
            {
                LogError("Animation name is null or empty");
                return;
            }
            
            if (target == null)
            {
                LogError("Animation target is null");
                return;
            }
            
            if (duration <= 0f)
            {
                LogWarning($"Animation duration is invalid: {duration}");
                return;
            }
            
            LogInfo($"Animation validation passed: {animationName} on {target.name} for {duration}s");
        }
        
        /// <summary>
        /// Validate configuration settings
        /// </summary>
        /// <param name="config">Configuration to validate</param>
        public override void ValidateConfiguration(BaseGameConfig config)
        {
            if (config == null)
            {
                LogError("Configuration is null");
                return;
            }
            
            // Validate as RunnerConfig if possible
            if (config is RunnerConfig runnerConfig)
            {
                ValidateRunnerConfig(runnerConfig);
            }
            else
            {
                LogWarning("Configuration is not a RunnerConfig");
            }
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
                LogError("RunnerConfig is null");
                return false;
            }
            
            // Validate player settings
            if (config.PlayerSpeed <= 0f)
            {
                LogWarning("Player speed is zero or negative");
                return false;
            }
            
            if (config.JumpForce <= 0f)
            {
                LogWarning("Jump force is zero or negative");
                return false;
            }
            
            if (config.SlideDuration <= 0f)
            {
                LogWarning("Slide duration is zero or negative");
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
                LogError("PlayerController is null");
                return false;
            }
            
            // Check if player has required components
            var rigidbody = playerController.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                LogError("PlayerController missing Rigidbody component");
                return false;
            }
            
            var collider = playerController.GetComponent<Collider>();
            if (collider == null)
            {
                LogError("PlayerController missing Collider component");
                return false;
            }
            
            Debug.Log("[RunnerErrorHandler] ✅ Player controller validated successfully");
            return true;
        }

        
        /// <summary>
        /// Get runner-specific error stats
        /// </summary>
        public override string GetErrorStats()
        {
            return $"Runner Error Stats:\n" +
                   $"Errors: {CurrentErrorCount} (Max: {MaxErrorCount})\n" +
                   $"History Size: {GetErrorHistory().Count}\n" +
                   $"Logging Enabled: {EnableLogging}\n" +
                   $"Validation Enabled: {EnableValidation}";
        }
        #endregion
        
        #region Safe Execution Methods
        /// <summary>
        /// Safely execute player movement
        /// </summary>
        public bool SafeExecutePlayerMovement(System.Action movementAction)
        {
            try
            {
                SafeExecute(movementAction, "PlayerMovement");
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Safely execute world generation
        /// </summary>
        public bool SafeExecuteWorldGeneration(System.Action generationAction)
        {
            try
            {
                SafeExecute(generationAction, "WorldGeneration");
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Safely execute scoring update
        /// </summary>
        public bool SafeExecuteScoringUpdate(System.Action scoringAction)
        {
            try
            {
                SafeExecute(scoringAction, "ScoringUpdate");
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Safely execute input processing
        /// </summary>
        public bool SafeExecuteInputProcessing(System.Action inputAction)
        {
            try
            {
                SafeExecute(inputAction, "InputProcessing");
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
} 