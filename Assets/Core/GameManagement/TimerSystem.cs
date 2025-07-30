using System;
using UnityEngine;

namespace MiniGameFramework.Core.GameManagement
{
    /// <summary>
    /// Reusable timer system supporting both countdown and stopwatch modes.
    /// Provides frame-independent timing with pause/resume functionality.
    /// </summary>
    public class TimerSystem : ITimerSystem
    {
        private float duration;
        private float elapsedTime;
        private bool isRunning;
        private bool isPaused;
        private bool isComplete;
        private TimerMode mode;

        /// <summary>Current time remaining (for countdown timers)</summary>
        public float TimeRemaining => mode == TimerMode.Countdown ? Mathf.Max(0, duration - elapsedTime) : 0f;
        
        /// <summary>Time elapsed (for stopwatch timers)</summary>
        public float TimeElapsed => elapsedTime;
        
        /// <summary>Initial duration (for countdown timers)</summary>
        public float Duration => duration;
        
        /// <summary>Is timer currently running</summary>
        public bool IsRunning => isRunning && !isPaused;
        
        /// <summary>Is timer paused</summary>
        public bool IsPaused => isPaused;
        
        /// <summary>Has timer completed</summary>
        public bool IsComplete => isComplete;
        
        /// <summary>Timer mode (countdown or stopwatch)</summary>
        public TimerMode Mode => mode;

        /// <summary>
        /// Event fired when timer completes.
        /// </summary>
        public event Action OnTimerComplete;
        
        /// <summary>
        /// Event fired each frame while timer is running.
        /// </summary>
        public event Action<float> OnTimerTick;

        /// <summary>
        /// Initialize timer system.
        /// </summary>
        /// <param name="mode">Timer mode (countdown or stopwatch)</param>
        /// <param name="duration">Duration for countdown mode (ignored for stopwatch)</param>
        public TimerSystem(TimerMode mode = TimerMode.Stopwatch, float duration = 0f)
        {
            this.mode = mode;
            this.duration = Math.Max(0f, duration);
            Reset();
            Debug.Log($"[TimerSystem] Initialized in {mode} mode with duration: {duration:F2}s");
        }

        /// <summary>
        /// Update timer - should be called every frame.
        /// </summary>
        /// <param name="deltaTime">Time since last frame</param>
        public void Update(float deltaTime)
        {
            if (!isRunning || isPaused || isComplete) return;

            elapsedTime += deltaTime;

            // Check for completion in countdown mode
            if (mode == TimerMode.Countdown && elapsedTime >= duration)
            {
                elapsedTime = duration;
                CompleteTimer();
            }

            // Fire tick event
            OnTimerTick?.Invoke(mode == TimerMode.Countdown ? TimeRemaining : TimeElapsed);
        }

        /// <summary>
        /// Start the timer.
        /// </summary>
        public void Start()
        {
            if (isComplete)
            {
                Debug.LogWarning("[TimerSystem] Cannot start completed timer. Use Reset() first.");
                return;
            }

            isRunning = true;
            isPaused = false;
            Debug.Log($"[TimerSystem] Timer started in {mode} mode");
        }

        /// <summary>
        /// Pause the timer.
        /// </summary>
        public void Pause()
        {
            if (!isRunning)
            {
                Debug.LogWarning("[TimerSystem] Cannot pause timer that is not running.");
                return;
            }

            isPaused = true;
            Debug.Log("[TimerSystem] Timer paused");
        }

        /// <summary>
        /// Resume the timer.
        /// </summary>
        public void Resume()
        {
            if (!isRunning || !isPaused)
            {
                Debug.LogWarning("[TimerSystem] Cannot resume timer that is not paused.");
                return;
            }

            isPaused = false;
            Debug.Log("[TimerSystem] Timer resumed");
        }

        /// <summary>
        /// Stop and reset the timer.
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            isPaused = false;
            Reset();
            Debug.Log("[TimerSystem] Timer stopped and reset");
        }

        /// <summary>
        /// Reset timer to initial state.
        /// </summary>
        public void Reset()
        {
            elapsedTime = 0f;
            isRunning = false;
            isPaused = false;
            isComplete = false;
            Debug.Log("[TimerSystem] Timer reset to initial state");
        }

        /// <summary>
        /// Set timer duration (for countdown mode).
        /// </summary>
        /// <param name="newDuration">Duration in seconds</param>
        public void SetDuration(float newDuration)
        {
            duration = Math.Max(0f, newDuration);
            
            // If timer is already complete and we set a new duration, reset completion state
            if (isComplete && duration > elapsedTime)
            {
                isComplete = false;
            }
            
            Debug.Log($"[TimerSystem] Duration set to: {duration:F2}s");
        }

        /// <summary>
        /// Set timer mode and optionally reset.
        /// </summary>
        /// <param name="newMode">New timer mode</param>
        /// <param name="resetTimer">Whether to reset timer state</param>
        public void SetMode(TimerMode newMode, bool resetTimer = true)
        {
            mode = newMode;
            
            if (resetTimer)
            {
                Reset();
            }
            
            Debug.Log($"[TimerSystem] Mode changed to: {mode}");
        }

        /// <summary>
        /// Get formatted time string.
        /// </summary>
        /// <param name="includeMilliseconds">Include milliseconds in format</param>
        /// <returns>Formatted time string</returns>
        public string GetFormattedTime(bool includeMilliseconds = false)
        {
            float timeToDisplay = mode == TimerMode.Countdown ? TimeRemaining : TimeElapsed;
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeToDisplay);
            
            if (includeMilliseconds)
            {
                return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}";
            }
            else
            {
                return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
        }

        /// <summary>
        /// Complete the timer and fire completion event.
        /// </summary>
        private void CompleteTimer()
        {
            if (isComplete) return;

            isComplete = true;
            isRunning = false;
            isPaused = false;
            
            Debug.Log($"[TimerSystem] Timer completed! Final time: {GetFormattedTime()}");
            OnTimerComplete?.Invoke();
        }
    }
} 