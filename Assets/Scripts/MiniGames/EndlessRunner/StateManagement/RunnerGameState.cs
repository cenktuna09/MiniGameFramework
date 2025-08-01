namespace EndlessRunner.StateManagement
{
    /// <summary>
    /// Game states for 3D Endless Runner
    /// Defines all possible states and their transitions
    /// </summary>
    public enum RunnerGameState
    {
        /// <summary>
        /// Game is ready to start
        /// </summary>
        Ready,
        
        /// <summary>
        /// Player is running (main gameplay state)
        /// </summary>
        Running,
        
        /// <summary>
        /// Player is jumping
        /// </summary>
        Jumping,
        
        /// <summary>
        /// Player is sliding
        /// </summary>
        Sliding,
        
        /// <summary>
        /// Game is paused
        /// </summary>
        Paused,
        
        /// <summary>
        /// Game is over
        /// </summary>
        GameOver
    }
} 