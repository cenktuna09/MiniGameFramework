using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Architecture;
using Core.Events;
using Core.DI;

namespace MiniGameFramework.Core.Bootstrap
{
    /// <summary>
    /// Static class responsible for loading and initializing mini-games.
    /// Manages scene loading, mini-game initialization, and starting.
    /// </summary>
    public static class MiniGameLoader
    {
        #region Events
        
        /// <summary>
        /// Event fired when a mini-game starts loading
        /// </summary>
        public static event Action<string> OnGameLoadingStarted;
        
        /// <summary>
        /// Event fired when a mini-game finishes loading
        /// </summary>
        public static event Action<string> OnGameLoadingCompleted;
        
        /// <summary>
        /// Event fired when a mini-game fails to load
        /// </summary>
        public static event Action<string, string> OnGameLoadingFailed;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Load a mini-game by its ID (synchronous wrapper)
        /// </summary>
        /// <param name="gameId">ID of the game to load</param>
        /// <param name="loadSceneMode">How to load the scene (Single or Additive)</param>
        public static void LoadGame(string gameId, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            Debug.Log($"[MiniGameLoader] Loading {gameId}...");
            OnGameLoadingStarted?.Invoke(gameId);
            
            try
            {
                // Start coroutine to handle async loading
                var gameObject = new GameObject("MiniGameLoader");
                var loader = gameObject.AddComponent<MiniGameLoaderCoroutine>();
                loader.StartLoadGame(gameId, loadSceneMode);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MiniGameLoader] Failed to start loading {gameId}: {ex.Message}");
                OnGameLoadingFailed?.Invoke(gameId, ex.Message);
            }
        }
        
        /// <summary>
        /// Load a mini-game asynchronously using Task
        /// </summary>
        /// <param name="gameId">ID of the game to load</param>
        /// <param name="loadSceneMode">How to load the scene (Single or Additive)</param>
        /// <returns>Task that completes when the game is loaded</returns>
        public static async Task LoadGameAsync(string gameId, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            Debug.Log($"[MiniGameLoader] Loading {gameId} asynchronously...");
            OnGameLoadingStarted?.Invoke(gameId);
            
            try
            {
                IsLoading = true;
                CurrentlyLoadingGameId = gameId;
                
                // Load scene asynchronously
                var loadOperation = SceneManager.LoadSceneAsync(gameId, loadSceneMode);
                
                if (loadOperation == null)
                {
                    var error = $"Scene '{gameId}' not found in build settings";
                    IsLoading = false;
                    CurrentlyLoadingGameId = null;
                    OnGameLoadingFailed?.Invoke(gameId, error);
                    Debug.LogError($"[MiniGameLoader] {error}");
                    throw new InvalidOperationException(error);
                }
                
                // Wait for scene to load
                while (!loadOperation.isDone)
                {
                    await Task.Yield();
                }
                
                // Initialize the mini-game
                await InitializeMiniGameAsync(gameId);
                
                IsLoading = false;
                CurrentlyLoadingGameId = null;
                OnGameLoadingCompleted?.Invoke(gameId);
                
                Debug.Log($"[MiniGameLoader] {gameId} loaded and initialized successfully");
            }
            catch (Exception ex)
            {
                IsLoading = false;
                CurrentlyLoadingGameId = null;
                OnGameLoadingFailed?.Invoke(gameId, ex.Message);
                Debug.LogError($"[MiniGameLoader] Failed to load {gameId}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Load a mini-game with a loading screen (synchronous wrapper)
        /// </summary>
        /// <param name="gameId">ID of the game to load</param>
        /// <param name="loadingScreenPrefab">Prefab for the loading screen</param>
        /// <param name="loadSceneMode">How to load the scene</param>
        public static void LoadGameWithLoadingScreen(string gameId, GameObject loadingScreenPrefab = null, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            Debug.Log($"[MiniGameLoader] Loading {gameId} with loading screen...");
            OnGameLoadingStarted?.Invoke(gameId);
            
            try
            {
                var gameObject = new GameObject("MiniGameLoader");
                var loader = gameObject.AddComponent<MiniGameLoaderCoroutine>();
                loader.StartLoadGameWithLoadingScreen(gameId, loadingScreenPrefab, loadSceneMode);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MiniGameLoader] Failed to start loading {gameId}: {ex.Message}");
                OnGameLoadingFailed?.Invoke(gameId, ex.Message);
            }
        }
        
        /// <summary>
        /// Load a mini-game with loading screen asynchronously using Task
        /// </summary>
        /// <param name="gameId">ID of the game to load</param>
        /// <param name="loadingScreenPrefab">Prefab for the loading screen</param>
        /// <param name="loadSceneMode">How to load the scene</param>
        /// <returns>Task that completes when the game is loaded</returns>
        public static async Task LoadGameWithLoadingScreenAsync(string gameId, GameObject loadingScreenPrefab = null, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            Debug.Log($"[MiniGameLoader] Loading {gameId} with loading screen asynchronously...");
            OnGameLoadingStarted?.Invoke(gameId);
            
            try
            {
                IsLoading = true;
                CurrentlyLoadingGameId = gameId;
                
                GameObject loadingScreen = null;
                
                // Show loading screen if provided
                if (loadingScreenPrefab != null)
                {
                    loadingScreen = UnityEngine.Object.Instantiate(loadingScreenPrefab);
                    UnityEngine.Object.DontDestroyOnLoad(loadingScreen);
                }
                
                // Load scene asynchronously
                var loadOperation = SceneManager.LoadSceneAsync(gameId, loadSceneMode);
                
                if (loadOperation == null)
                {
                    var error = $"Scene '{gameId}' not found in build settings";
                    IsLoading = false;
                    CurrentlyLoadingGameId = null;
                    OnGameLoadingFailed?.Invoke(gameId, error);
                    Debug.LogError($"[MiniGameLoader] {error}");
                    
                    // Clean up loading screen
                    if (loadingScreen != null)
                    {
                        UnityEngine.Object.Destroy(loadingScreen);
                    }
                    
                    throw new InvalidOperationException(error);
                }
                
                // Update loading progress
                while (!loadOperation.isDone)
                {
                    if (loadingScreen != null)
                    {
                        // Update loading screen progress
                        var progress = loadOperation.progress;
                        // You can add a progress bar component to the loading screen
                        // and update it here
                    }
                    await Task.Yield();
                }
                
                // Initialize the mini-game
                await InitializeMiniGameAsync(gameId);
                
                IsLoading = false;
                CurrentlyLoadingGameId = null;
                OnGameLoadingCompleted?.Invoke(gameId);
                
                Debug.Log($"[MiniGameLoader] {gameId} loaded and initialized successfully");
                
                // Clean up loading screen
                if (loadingScreen != null)
                {
                    UnityEngine.Object.Destroy(loadingScreen);
                }
            }
            catch (Exception ex)
            {
                IsLoading = false;
                CurrentlyLoadingGameId = null;
                OnGameLoadingFailed?.Invoke(gameId, ex.Message);
                Debug.LogError($"[MiniGameLoader] Failed to load {gameId}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Check if a game is currently loading
        /// </summary>
        public static bool IsLoading { get; private set; }
        
        /// <summary>
        /// Get the ID of the currently loading game
        /// </summary>
        public static string CurrentlyLoadingGameId { get; private set; }
        
        #endregion
        
        #region Internal Coroutine Handler
        
        /// <summary>
        /// Internal MonoBehaviour to handle coroutines for async loading
        /// </summary>
        private class MiniGameLoaderCoroutine : MonoBehaviour
        {
            public void StartLoadGame(string gameId, LoadSceneMode loadSceneMode)
            {
                StartCoroutine(LoadGameCoroutine(gameId, loadSceneMode));
            }
            
            public void StartLoadGameWithLoadingScreen(string gameId, GameObject loadingScreenPrefab, LoadSceneMode loadSceneMode)
            {
                StartCoroutine(LoadGameWithLoadingScreenCoroutine(gameId, loadingScreenPrefab, loadSceneMode));
            }
            
            private IEnumerator LoadGameCoroutine(string gameId, LoadSceneMode loadSceneMode)
            {
                IsLoading = true;
                CurrentlyLoadingGameId = gameId;
                
                // Load the scene asynchronously
                var loadOperation = SceneManager.LoadSceneAsync(gameId, loadSceneMode);
                
                if (loadOperation == null)
                {
                    IsLoading = false;
                    CurrentlyLoadingGameId = null;
                    OnGameLoadingFailed?.Invoke(gameId, $"Scene '{gameId}' not found in build settings");
                    Debug.LogError($"[MiniGameLoader] Scene '{gameId}' not found in build settings");
                    Destroy(gameObject);
                    yield break;
                }
                
                // Wait for scene to load
                while (!loadOperation.isDone)
                {
                    yield return null;
                }
                
                // Find and initialize the mini-game
                yield return StartCoroutine(InitializeMiniGame(gameId));
                
                IsLoading = false;
                CurrentlyLoadingGameId = null;
                OnGameLoadingCompleted?.Invoke(gameId);
                
                Debug.Log($"[MiniGameLoader] {gameId} loaded and initialized successfully");
                
                // Clean up this temporary GameObject
                Destroy(gameObject);
            }
            
            private IEnumerator LoadGameWithLoadingScreenCoroutine(string gameId, GameObject loadingScreenPrefab, LoadSceneMode loadSceneMode)
            {
                IsLoading = true;
                CurrentlyLoadingGameId = gameId;
                
                GameObject loadingScreen = null;
                
                // Show loading screen if provided
                if (loadingScreenPrefab != null)
                {
                    loadingScreen = Instantiate(loadingScreenPrefab);
                    DontDestroyOnLoad(loadingScreen);
                }
                
                // Load the scene asynchronously
                var loadOperation = SceneManager.LoadSceneAsync(gameId, loadSceneMode);
                
                if (loadOperation == null)
                {
                    IsLoading = false;
                    CurrentlyLoadingGameId = null;
                    OnGameLoadingFailed?.Invoke(gameId, $"Scene '{gameId}' not found in build settings");
                    Debug.LogError($"[MiniGameLoader] Scene '{gameId}' not found in build settings");
                    
                    // Clean up loading screen
                    if (loadingScreen != null)
                    {
                        Destroy(loadingScreen);
                    }
                    
                    // Clean up this temporary GameObject
                    Destroy(gameObject);
                    yield break;
                }
                
                // Update loading progress
                while (!loadOperation.isDone)
                {
                    if (loadingScreen != null)
                    {
                        // Update loading screen progress
                        var progress = loadOperation.progress;
                        // You can add a progress bar component to the loading screen
                        // and update it here
                    }
                    yield return null;
                }
                
                // Initialize the mini-game
                yield return StartCoroutine(InitializeMiniGame(gameId));
                
                IsLoading = false;
                CurrentlyLoadingGameId = null;
                OnGameLoadingCompleted?.Invoke(gameId);
                
                Debug.Log($"[MiniGameLoader] {gameId} loaded and initialized successfully");
                
                // Clean up loading screen
                if (loadingScreen != null)
                {
                    Destroy(loadingScreen);
                }
                
                // Clean up this temporary GameObject
                Destroy(gameObject);
            }
            
            private IEnumerator InitializeMiniGame(string gameId)
            {
                // Find the mini-game in the loaded scene
                var miniGame = FindFirstObjectByType<MiniGameBase>();
                
                if (miniGame == null)
                {
                    OnGameLoadingFailed?.Invoke(gameId, $"No MiniGameBase found in scene '{gameId}'");
                    Debug.LogError($"[MiniGameLoader] No MiniGameBase found in scene '{gameId}'");
                    yield break;
                }
                
                // Verify the game ID matches
                if (miniGame.GameId != gameId)
                {
                    Debug.LogWarning($"[MiniGameLoader] Game ID mismatch: Expected '{gameId}', found '{miniGame.GameId}'");
                }
                
                // Initialize the mini-game
                var initTask = miniGame.InitializeAsync();
                while (!initTask.IsCompleted)
                {
                    yield return null;
                }
                
                if (initTask.Exception != null)
                {
                    OnGameLoadingFailed?.Invoke(gameId, $"Failed to initialize {gameId}: {initTask.Exception.Message}");
                    Debug.LogError($"[MiniGameLoader] Failed to initialize {gameId}: {initTask.Exception.Message}");
                    yield break;
                }
                
                Debug.Log($"[MiniGameLoader] {gameId} initialized successfully");
            }
        }
        
        #endregion

        #region Private Async Methods
        
        /// <summary>
        /// Initialize a mini-game asynchronously
        /// </summary>
        /// <param name="gameId">ID of the game to initialize</param>
        private static async Task InitializeMiniGameAsync(string gameId)
        {
            // Find the mini-game in the loaded scene
            var miniGame = UnityEngine.Object.FindFirstObjectByType<MiniGameBase>();
            
            if (miniGame == null)
            {
                var error = $"No MiniGameBase found in scene '{gameId}'";
                OnGameLoadingFailed?.Invoke(gameId, error);
                Debug.LogError($"[MiniGameLoader] {error}");
                throw new InvalidOperationException(error);
            }
            
            // Verify the game ID matches
            if (miniGame.GameId != gameId)
            {
                Debug.LogWarning($"[MiniGameLoader] Game ID mismatch: Expected '{gameId}', found '{miniGame.GameId}'");
            }
            
            // Initialize the mini-game
            var initTask = miniGame.InitializeAsync();
            while (!initTask.IsCompleted)
            {
                await Task.Yield();
            }
            
            if (initTask.Exception != null)
            {
                var error = $"Failed to initialize {gameId}: {initTask.Exception.Message}";
                OnGameLoadingFailed?.Invoke(gameId, error);
                Debug.LogError($"[MiniGameLoader] {error}");
                throw new InvalidOperationException(error);
            }
            
            Debug.Log($"[MiniGameLoader] {gameId} initialized successfully");
        }
        
        #endregion
    }
} 