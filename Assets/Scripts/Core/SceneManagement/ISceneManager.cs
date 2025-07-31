using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MiniGameFramework.Core.SceneManagement
{
    /// <summary>
    /// Defines the contract for scene management operations.
    /// Provides async scene loading with progress tracking and transition support.
    /// </summary>
    public interface ISceneManager
    {
        /// <summary>
        /// Gets the currently active scene name.
        /// </summary>
        string CurrentScene { get; }
        
        /// <summary>
        /// Indicates if a scene loading operation is currently in progress.
        /// </summary>
        bool IsLoading { get; }
        
        /// <summary>
        /// Current loading progress (0.0 to 1.0).
        /// </summary>
        float LoadingProgress { get; }
        
        /// <summary>
        /// Event triggered when loading progress changes.
        /// </summary>
        event Action<float> ProgressChanged;
        
        /// <summary>
        /// Event triggered when scene loading starts.
        /// </summary>
        event Action<string> LoadingStarted;
        
        /// <summary>
        /// Event triggered when scene loading completes.
        /// </summary>
        event Action<string> LoadingCompleted;
        
        /// <summary>
        /// Load a scene asynchronously by name.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load</param>
        /// <param name="fadeTransition">Whether to use fade transition</param>
        /// <returns>Task that completes when the scene is loaded</returns>
        Task LoadSceneAsync(string sceneName, bool fadeTransition = true);
        
        /// <summary>
        /// Load a scene asynchronously by build index.
        /// </summary>
        /// <param name="sceneIndex">Build index of the scene to load</param>
        /// <param name="fadeTransition">Whether to use fade transition</param>
        /// <returns>Task that completes when the scene is loaded</returns>
        Task LoadSceneAsync(int sceneIndex, bool fadeTransition = true);
        
        /// <summary>
        /// Reload the current scene.
        /// </summary>
        /// <param name="fadeTransition">Whether to use fade transition</param>
        /// <returns>Task that completes when the scene is reloaded</returns>
        Task ReloadCurrentSceneAsync(bool fadeTransition = true);
        
        /// <summary>
        /// Preload a scene asynchronously without activating it.
        /// </summary>
        /// <param name="sceneName">Name of the scene to preload</param>
        /// <returns>Task that completes when the scene is preloaded</returns>
        Task PreloadSceneAsync(string sceneName);
        
        /// <summary>
        /// Activate a preloaded scene.
        /// </summary>
        /// <param name="sceneName">Name of the preloaded scene to activate</param>
        /// <param name="fadeTransition">Whether to use fade transition</param>
        /// <returns>Task that completes when the scene is activated</returns>
        Task ActivatePreloadedSceneAsync(string sceneName, bool fadeTransition = true);
    }
} 