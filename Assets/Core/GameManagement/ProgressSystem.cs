using System;
using UnityEngine;

namespace MiniGameFramework.Core.GameManagement
{
    /// <summary>
    /// Reusable progress tracking system for goals, achievements, and progress bars.
    /// Provides event-driven progress updates with validation.
    /// </summary>
    public class ProgressSystem : IProgressSystem
    {
        private float currentProgress;
        private int currentStep;
        private int totalSteps;
        private bool isComplete;

        /// <summary>Current progress value (0.0 to 1.0)</summary>
        public float Progress => currentProgress;
        
        /// <summary>Current step in progression</summary>
        public int CurrentStep => currentStep;
        
        /// <summary>Total steps in progression</summary>
        public int TotalSteps => totalSteps;
        
        /// <summary>Is progression complete</summary>
        public bool IsComplete => isComplete;

        /// <summary>
        /// Event fired when progress changes.
        /// </summary>
        public event Action<float> OnProgressChanged;
        
        /// <summary>
        /// Event fired when progression completes.
        /// </summary>
        public event Action OnProgressComplete;

        /// <summary>
        /// Initialize progress system with step-based progression.
        /// </summary>
        /// <param name="totalSteps">Total number of steps</param>
        public ProgressSystem(int totalSteps = 100)
        {
            this.totalSteps = Math.Max(1, totalSteps);
            Reset();
            Debug.Log($"[ProgressSystem] Initialized with {totalSteps} total steps");
        }

        /// <summary>
        /// Set progress directly (0.0 to 1.0).
        /// </summary>
        /// <param name="progress">Progress value</param>
        public void SetProgress(float progress)
        {
            float newProgress = Mathf.Clamp01(progress);
            
            if (Math.Abs(newProgress - currentProgress) < 0.001f) return; // Avoid unnecessary updates
            
            currentProgress = newProgress;
            currentStep = Mathf.RoundToInt(currentProgress * totalSteps);
            
            // Check for completion
            bool wasComplete = isComplete;
            isComplete = currentProgress >= 1.0f;
            
            Debug.Log($"[ProgressSystem] Progress updated: {currentProgress:P2} ({currentStep}/{totalSteps})");
            
            // Fire events
            OnProgressChanged?.Invoke(currentProgress);
            
            if (isComplete && !wasComplete)
            {
                Debug.Log("[ProgressSystem] Progress completed!");
                OnProgressComplete?.Invoke();
            }
        }

        /// <summary>
        /// Advance progress by one step.
        /// </summary>
        public void AdvanceStep()
        {
            SetStep(currentStep + 1);
        }

        /// <summary>
        /// Set current step directly.
        /// </summary>
        /// <param name="step">Step number</param>
        public void SetStep(int step)
        {
            int newStep = Mathf.Clamp(step, 0, totalSteps);
            float newProgress = (float)newStep / totalSteps;
            SetProgress(newProgress);
        }

        /// <summary>
        /// Reset progress to initial state.
        /// </summary>
        public void Reset()
        {
            currentProgress = 0f;
            currentStep = 0;
            isComplete = false;
            Debug.Log("[ProgressSystem] Reset to initial state");
        }

        /// <summary>
        /// Set total steps and recalculate progress.
        /// </summary>
        /// <param name="newTotalSteps">New total step count</param>
        public void SetTotalSteps(int newTotalSteps)
        {
            int oldTotalSteps = totalSteps;
            totalSteps = Math.Max(1, newTotalSteps);
            
            // Recalculate progress to maintain relative position
            if (oldTotalSteps != totalSteps)
            {
                float progressRatio = (float)currentStep / oldTotalSteps;
                SetProgress(progressRatio);
                Debug.Log($"[ProgressSystem] Total steps changed: {oldTotalSteps} -> {totalSteps}");
            }
        }
    }
} 