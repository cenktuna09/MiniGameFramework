using System;

namespace MiniGameFramework.Core.GameManagement
{
    /// <summary>
    /// Interface for tracking individual game session data and progress.
    /// Manages score, timing, and session-specific metrics.
    /// </summary>
    public interface IGameSession
    {
        /// <summary>Unique session identifier</summary>
        string SessionId { get; }
        
        /// <summary>Game type identifier</summary>
        string GameId { get; }
        
        /// <summary>When the session started</summary>
        DateTime StartTime { get; }
        
        /// <summary>When the session ended (null if still active)</summary>
        DateTime? EndTime { get; }
        
        /// <summary>Current session score</summary>
        int Score { get; }
        
        /// <summary>Best score achieved in this session</summary>
        int BestScore { get; }
        
        /// <summary>Total time elapsed in session</summary>
        TimeSpan ElapsedTime { get; }
        
        /// <summary>Is session currently active</summary>
        bool IsActive { get; }
        
        /// <summary>Session result (null if still active)</summary>
        GameResult? Result { get; }
        
        /// <summary>
        /// Update the current score.
        /// </summary>
        /// <param name="newScore">New score value</param>
        void UpdateScore(int newScore);
        
        /// <summary>
        /// Add points to the current score.
        /// </summary>
        /// <param name="points">Points to add</param>
        void AddScore(int points);
        
        /// <summary>
        /// End the session with a result.
        /// </summary>
        /// <param name="result">Final game result</param>
        void EndSession(GameResult result);
        
        /// <summary>
        /// Get session statistics.
        /// </summary>
        /// <returns>Session stats data</returns>
        GameSessionStats GetStats();
    }
    
    /// <summary>
    /// Progress tracking interface for reusable progress systems.
    /// </summary>
    public interface IProgressSystem
    {
        /// <summary>Current progress value (0.0 to 1.0)</summary>
        float Progress { get; }
        
        /// <summary>Current step in progression</summary>
        int CurrentStep { get; }
        
        /// <summary>Total steps in progression</summary>
        int TotalSteps { get; }
        
        /// <summary>Is progression complete</summary>
        bool IsComplete { get; }
        
        /// <summary>
        /// Event fired when progress changes.
        /// </summary>
        event Action<float> OnProgressChanged;
        
        /// <summary>
        /// Event fired when progression completes.
        /// </summary>
        event Action OnProgressComplete;
        
        /// <summary>
        /// Set progress directly (0.0 to 1.0).
        /// </summary>
        /// <param name="progress">Progress value</param>
        void SetProgress(float progress);
        
        /// <summary>
        /// Advance progress by one step.
        /// </summary>
        void AdvanceStep();
        
        /// <summary>
        /// Set current step directly.
        /// </summary>
        /// <param name="step">Step number</param>
        void SetStep(int step);
        
        /// <summary>
        /// Reset progress to initial state.
        /// </summary>
        void Reset();
    }
    
    /// <summary>
    /// Timer system interface for reusable timing functionality.
    /// </summary>
    public interface ITimerSystem
    {
        /// <summary>Current time remaining (for countdown timers)</summary>
        float TimeRemaining { get; }
        
        /// <summary>Time elapsed (for stopwatch timers)</summary>
        float TimeElapsed { get; }
        
        /// <summary>Initial duration (for countdown timers)</summary>
        float Duration { get; }
        
        /// <summary>Is timer currently running</summary>
        bool IsRunning { get; }
        
        /// <summary>Is timer paused</summary>
        bool IsPaused { get; }
        
        /// <summary>Has timer completed</summary>
        bool IsComplete { get; }
        
        /// <summary>Timer mode (countdown or stopwatch)</summary>
        TimerMode Mode { get; }
        
        /// <summary>
        /// Event fired when timer completes.
        /// </summary>
        event Action OnTimerComplete;
        
        /// <summary>
        /// Event fired each frame while timer is running.
        /// </summary>
        event Action<float> OnTimerTick;
        
        /// <summary>
        /// Start the timer.
        /// </summary>
        void Start();
        
        /// <summary>
        /// Pause the timer.
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Resume the timer.
        /// </summary>
        void Resume();
        
        /// <summary>
        /// Stop and reset the timer.
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Reset timer to initial state.
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Set timer duration (for countdown mode).
        /// </summary>
        /// <param name="duration">Duration in seconds</param>
        void SetDuration(float duration);
    }
    
    /// <summary>
    /// Game session statistics data.
    /// </summary>
    public struct GameSessionStats
    {
        public string SessionId;
        public string GameId;
        public DateTime StartTime;
        public DateTime? EndTime;
        public TimeSpan Duration;
        public int FinalScore;
        public int BestScore;
        public GameResult? Result;
        public bool IsCompleted;
    }
    
    /// <summary>
    /// Timer operation mode.
    /// </summary>
    public enum TimerMode
    {
        /// <summary>Count down from duration to zero</summary>
        Countdown,
        
        /// <summary>Count up from zero</summary>
        Stopwatch
    }
} 