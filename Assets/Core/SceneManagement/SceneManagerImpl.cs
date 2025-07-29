using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MiniGameFramework.Core.Architecture;

namespace MiniGameFramework.Core.SceneManagement
{
    /// <summary>
    /// Unity 6 implementation of scene management with async/await support.
    /// Provides progress tracking, transitions, and event-driven notifications.
    /// </summary>
    public class SceneManagerImpl : ISceneManager
    {
        private readonly IEventBus _eventBus;
        private readonly Dictionary<string, AsyncOperation> _preloadedScenes;
        
        private AsyncOperation _currentLoadOperation;
        private string _currentScene;
        private bool _isLoading;
        private float _loadingProgress;
        private SceneTransitionManager _transitionManager;
        
        /// <inheritdoc />
        public string CurrentScene => _currentScene ?? UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        /// <inheritdoc />
        public bool IsLoading => _isLoading;
        
        /// <inheritdoc />
        public float LoadingProgress => _loadingProgress;
        
        /// <inheritdoc />
        public event Action<float> ProgressChanged;
        
        /// <inheritdoc />
        public event Action<string> LoadingStarted;
        
        /// <inheritdoc />
        public event Action<string> LoadingCompleted;
        
        public SceneManagerImpl(IEventBus eventBus, SceneTransitionManager transitionManager = null)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _preloadedScenes = new Dictionary<string, AsyncOperation>();
            _currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            _transitionManager = transitionManager;
            
            // Subscribe to Unity's scene management events
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        /// <inheritdoc />
        public async Task LoadSceneAsync(string sceneName, bool fadeTransition = true)
        {
            if (string.IsNullOrEmpty(sceneName))
                throw new ArgumentException("Scene name cannot be null or empty", nameof(sceneName));
                
            if (_isLoading)
            {
                Debug.LogWarning($"Scene loading already in progress. Ignoring request to load {sceneName}");
                return;
            }
            
            await LoadSceneInternalAsync(sceneName, LoadSceneMode.Single, fadeTransition);
        }
        
        /// <inheritdoc />
        public async Task LoadSceneAsync(int sceneIndex, bool fadeTransition = true)
        {
            if (sceneIndex < 0 || sceneIndex >= UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
                throw new ArgumentOutOfRangeException(nameof(sceneIndex), "Scene index is out of range");
                
            if (_isLoading)
            {
                Debug.LogWarning($"Scene loading already in progress. Ignoring request to load scene index {sceneIndex}");
                return;
            }
            
            var sceneName = System.IO.Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(sceneIndex));
            await LoadSceneInternalAsync(sceneName, LoadSceneMode.Single, fadeTransition, sceneIndex);
        }
        
        /// <inheritdoc />
        public async Task ReloadCurrentSceneAsync(bool fadeTransition = true)
        {
            await LoadSceneAsync(CurrentScene, fadeTransition);
        }
        
        /// <inheritdoc />
        public async Task PreloadSceneAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                throw new ArgumentException("Scene name cannot be null or empty", nameof(sceneName));
                
            if (_preloadedScenes.ContainsKey(sceneName))
            {
                Debug.LogWarning($"Scene {sceneName} is already preloaded");
                return;
            }
            
            Debug.Log($"Preloading scene: {sceneName}");
            
            var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;
            
            _preloadedScenes[sceneName] = asyncOperation;
            
            // Wait until the scene is loaded but not activated (progress reaches 0.9)
            while (asyncOperation.progress < 0.9f)
            {
                await Task.Yield();
            }
            
            Debug.Log($"Scene {sceneName} preloaded successfully");
        }
        
        /// <inheritdoc />
        public async Task ActivatePreloadedSceneAsync(string sceneName, bool fadeTransition = true)
        {
            if (!_preloadedScenes.TryGetValue(sceneName, out var asyncOperation))
                throw new InvalidOperationException($"Scene {sceneName} is not preloaded. Call PreloadSceneAsync first.");
                
            if (_isLoading)
            {
                Debug.LogWarning($"Scene loading already in progress. Ignoring request to activate {sceneName}");
                return;
            }
            
            _isLoading = true;
            _currentLoadOperation = asyncOperation;
            
            try
            {
                // Start transition
                if (fadeTransition)
                {
                    await StartTransitionAsync();
                }
                
                // Fire events
                OnLoadingStarted(sceneName);
                
                // Activate the preloaded scene
                asyncOperation.allowSceneActivation = true;
                
                // Wait for activation to complete
                while (!asyncOperation.isDone)
                {
                    UpdateProgress(asyncOperation.progress);
                    await Task.Yield();
                }
                
                // Set the activated scene as active
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
                if (scene.isLoaded)
                {
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
                }
                
                // Clean up
                _preloadedScenes.Remove(sceneName);
                _currentScene = sceneName;
                
                // End transition
                if (fadeTransition)
                {
                    await EndTransitionAsync();
                }
                
                OnLoadingCompleted(sceneName);
            }
            finally
            {
                _isLoading = false;
                _currentLoadOperation = null;
                UpdateProgress(0f);
            }
        }
        
        private async Task LoadSceneInternalAsync(string sceneName, LoadSceneMode loadMode, bool fadeTransition, int sceneIndex = -1)
        {
            _isLoading = true;
            
            try
            {
                // Start transition
                if (fadeTransition)
                {
                    await StartTransitionAsync();
                }
                
                // Fire events
                OnLoadingStarted(sceneName);
                
                // Start loading
                _currentLoadOperation = sceneIndex >= 0 
                    ? UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex, loadMode)
                    : UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, loadMode);
                
                // Track progress with minimum loading time for better UX
                var startTime = UnityEngine.Time.unscaledTime;
                var minLoadingTime = 1.0f; // Minimum 1 second loading for demo
                
                while (!_currentLoadOperation.isDone || (UnityEngine.Time.unscaledTime - startTime) < minLoadingTime)
                {
                    UpdateProgress(_currentLoadOperation.progress);
                    await Task.Yield();
                }
                
                _currentScene = sceneName;
                
                // End transition
                if (fadeTransition)
                {
                    await EndTransitionAsync();
                }
                
                OnLoadingCompleted(sceneName);
            }
            finally
            {
                _isLoading = false;
                _currentLoadOperation = null;
                UpdateProgress(0f);
            }
        }
        
        private async Task StartTransitionAsync()
        {
            if (_transitionManager != null)
            {
                await _transitionManager.FadeOutAsync();
            }
        }
        
        private async Task EndTransitionAsync()
        {
            if (_transitionManager != null)
            {
                await _transitionManager.FadeInAsync();
            }
        }
        
        private void UpdateProgress(float progress)
        {
            _loadingProgress = progress;
            ProgressChanged?.Invoke(progress);
            
            // Publish detailed progress event
            _eventBus.Publish(new SceneLoadingProgressEvent(_currentScene ?? "Unknown", progress, null, this));
        }
        
        private void OnLoadingStarted(string sceneName)
        {
            Debug.Log($"Scene loading started: {sceneName}");
            LoadingStarted?.Invoke(sceneName);
            _eventBus.Publish(new SceneLoadingStartedEvent(sceneName, this));
        }
        
        private void OnLoadingCompleted(string sceneName)
        {
            Debug.Log($"Scene loading completed: {sceneName}");
            LoadingCompleted?.Invoke(sceneName);
            _eventBus.Publish(new SceneLoadingCompletedEvent(sceneName, this));
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
            {
                _currentScene = scene.name;
            }
        }
        
        ~SceneManagerImpl()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
} 