using System.Threading.Tasks;
using UnityEngine;

namespace MiniGameFramework.Core.SceneManagement
{
    /// <summary>
    /// Defines the contract for scene transition implementations.
    /// Provides async transition effects for scene changes.
    /// </summary>
    public interface ISceneTransition
    {
        /// <summary>
        /// Gets the type of this transition.
        /// </summary>
        TransitionType TransitionType { get; }
        
        /// <summary>
        /// Gets the duration of the transition in seconds.
        /// </summary>
        float Duration { get; }
        
        /// <summary>
        /// Indicates if the transition is currently active.
        /// </summary>
        bool IsTransitioning { get; }
        
        /// <summary>
        /// Start the fade-out transition (hiding the current scene).
        /// </summary>
        /// <param name="duration">Duration override (uses default if null)</param>
        /// <returns>Task that completes when fade-out is finished</returns>
        Task FadeOutAsync(float? duration = null);
        
        /// <summary>
        /// Start the fade-in transition (revealing the new scene).
        /// </summary>
        /// <param name="duration">Duration override (uses default if null)</param>
        /// <returns>Task that completes when fade-in is finished</returns>
        Task FadeInAsync(float? duration = null);
        
        /// <summary>
        /// Perform a complete transition cycle (fade-out, then fade-in).
        /// </summary>
        /// <param name="duration">Duration override for each phase (uses default if null)</param>
        /// <returns>Task that completes when the full transition is finished</returns>
        Task TransitionAsync(float? duration = null);
        
        /// <summary>
        /// Initialize the transition with custom settings.
        /// </summary>
        /// <param name="transitionData">Configuration data for the transition</param>
        void Initialize(SceneTransitionData transitionData);
        
        /// <summary>
        /// Reset the transition to its initial state.
        /// </summary>
        void Reset();
    }
} 