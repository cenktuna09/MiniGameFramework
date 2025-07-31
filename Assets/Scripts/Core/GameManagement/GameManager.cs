using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.Core.DI;
using MiniGameFramework.Core.SaveSystem;
using MiniGameFramework.Core.StateManagement;

namespace MiniGameFramework.Core.GameManagement
{
    /// <summary>
    /// Central game manager implementation responsible for mini-game lifecycle and flow control.
    /// Coordinates between different systems and manages game sessions.
    /// </summary>
    public class GameManager : MonoBehaviour, IGameManager
    {
        [Header("Game Manager Configuration")]
        [SerializeField] private bool autoStartTimer = true;
        [SerializeField] private float autoSaveInterval = 30f;

        // Dependencies
        private IEventBus eventBus;
        private ISaveSystem saveSystem;
        private IGameStateManager globalStateManager;

        // Current state
        private IMiniGame currentGame;
        private IGameSession currentSession;
        private GameManagerState state;
        private TimerSystem sessionTimer;
        private Dictionary<string, int> bestScores;

        // Mini-game registry
        private readonly Dictionary<string, Type> registeredGames = new Dictionary<string, Type>();

        /// <summary>Current active mini-game instance</summary>
        public IMiniGame CurrentGame => currentGame;
        
        /// <summary>Current game session data</summary>
        public IGameSession CurrentSession => currentSession;
        
        /// <summary>Is any game currently active</summary>
        public bool IsGameActive => currentGame != null && currentSession?.IsActive == true;
        
        /// <summary>Global game manager state</summary>
        public GameManagerState State => state;

        /// <summary>
        /// Event fired when game manager state changes.
        /// </summary>
        public event Action<GameManagerState> OnStateChanged;
        
        /// <summary>
        /// Event fired when a mini-game is loaded.
        /// </summary>
        public event Action<IMiniGame> OnGameLoaded;
        
        /// <summary>
        /// Event fired when a mini-game is unloaded.
        /// </summary>
        public event Action<string> OnGameUnloaded;

        #region Unity Lifecycle

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            // Wait for all services to be ready
            if (ServiceLocator.Instance.IsRegistered<IEventBus>())
            {
                OnServicesReady();
            }
            else
            {
                StartCoroutine(WaitForServices());
            }
        }

        private void Update()
        {
            // Update session timer
            sessionTimer?.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            state = GameManagerState.Idle;
            bestScores = new Dictionary<string, int>();
            sessionTimer = new TimerSystem(TimerMode.Stopwatch);
            
            Debug.Log("[GameManager] Initialized successfully");
        }

        private System.Collections.IEnumerator WaitForServices()
        {
            while (!ServiceLocator.Instance.IsRegistered<IEventBus>())
            {
                yield return null;
            }
            OnServicesReady();
        }

        private void OnServicesReady()
        {
            // Resolve dependencies
            eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            saveSystem = ServiceLocator.Instance.Resolve<ISaveSystem>();
            globalStateManager = ServiceLocator.Instance.Resolve<IGameStateManager>();

            // Load saved data
            LoadBestScores();

            // Subscribe to global state changes
            if (eventBus != null)
            {
                eventBus.Subscribe<GlobalGameStateChangedEvent>(OnGlobalStateChanged);
            }

            Debug.Log("[GameManager] Services resolved and ready");
        }

        #endregion

        #region Game Lifecycle Management

        /// <summary>
        /// Load and start a mini-game by ID.
        /// </summary>
        /// <param name="gameId">Unique identifier of the mini-game</param>
        /// <returns>Task that completes when game is loaded and ready</returns>
        public async Task<bool> LoadGameAsync(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                Debug.LogError("[GameManager] Cannot load game with null or empty ID");
                return false;
            }

            if (state != GameManagerState.Idle)
            {
                Debug.LogWarning($"[GameManager] Cannot load game in current state: {state}");
                return false;
            }

            ChangeState(GameManagerState.Loading);

            try
            {
                // Unload current game if any
                if (currentGame != null)
                {
                    await UnloadCurrentGameAsync();
                }

                // Create new game instance
                currentGame = await CreateGameInstanceAsync(gameId);
                if (currentGame == null)
                {
                    ChangeState(GameManagerState.Idle);
                    return false;
                }

                // Initialize the game
                await currentGame.InitializeAsync();

                // Create new session
                int bestScore = GetBestScore(gameId);
                currentSession = new GameSession(gameId, bestScore);

                // Setup session timer
                if (autoStartTimer)
                {
                    sessionTimer.Reset();
                    sessionTimer.Start();
                }

                ChangeState(GameManagerState.Ready);
                OnGameLoaded?.Invoke(currentGame);

                Debug.Log($"[GameManager] Successfully loaded game: {gameId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameManager] Failed to load game {gameId}: {ex.Message}");
                ChangeState(GameManagerState.Idle);
                return false;
            }
        }

        /// <summary>
        /// Unload the current mini-game and clean up resources.
        /// </summary>
        /// <returns>Task that completes when game is unloaded</returns>
        public async Task<bool> UnloadCurrentGameAsync()
        {
            if (currentGame == null)
            {
                Debug.LogWarning("[GameManager] No game to unload");
                return true;
            }

            ChangeState(GameManagerState.Unloading);

            try
            {
                string gameId = currentGame.GameId;

                // End current session if active
                if (currentSession?.IsActive == true)
                {
                    currentSession.EndSession(GameResult.Quit);
                }

                // Cleanup game
                currentGame.Cleanup();
                
                // Destroy game object if it's a MonoBehaviour
                if (currentGame is MonoBehaviour gameComponent)
                {
                    Destroy(gameComponent.gameObject);
                }

                currentGame = null;
                currentSession = null;

                // Stop session timer
                sessionTimer?.Stop();

                ChangeState(GameManagerState.Idle);
                OnGameUnloaded?.Invoke(gameId);

                Debug.Log($"[GameManager] Successfully unloaded game: {gameId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameManager] Failed to unload game: {ex.Message}");
                ChangeState(GameManagerState.Idle);
                return false;
            }
        }

        /// <summary>
        /// Start the currently loaded mini-game.
        /// </summary>
        /// <returns>True if game started successfully</returns>
        public bool StartCurrentGame()
        {
            if (currentGame == null || state != GameManagerState.Ready)
            {
                Debug.LogWarning($"[GameManager] Cannot start game in current state: {state}");
                return false;
            }

            try
            {
                currentGame.Start();
                ChangeState(GameManagerState.Playing);
                
                // Update global state
                globalStateManager?.TransitionToState(GlobalGameState.Playing, $"Started {currentGame.GameId}");

                Debug.Log($"[GameManager] Started game: {currentGame.GameId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameManager] Failed to start game: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Pause the current mini-game.
        /// </summary>
        /// <returns>True if game paused successfully</returns>
        public bool PauseCurrentGame()
        {
            if (currentGame == null || state != GameManagerState.Playing)
            {
                Debug.LogWarning($"[GameManager] Cannot pause game in current state: {state}");
                return false;
            }

            try
            {
                currentGame.Pause();
                sessionTimer?.Pause();
                ChangeState(GameManagerState.Paused);

                // Update global state
                globalStateManager?.TransitionToState(GlobalGameState.Paused, $"Paused {currentGame.GameId}");

                Debug.Log($"[GameManager] Paused game: {currentGame.GameId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameManager] Failed to pause game: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Resume the current mini-game.
        /// </summary>
        /// <returns>True if game resumed successfully</returns>
        public bool ResumeCurrentGame()
        {
            if (currentGame == null || state != GameManagerState.Paused)
            {
                Debug.LogWarning($"[GameManager] Cannot resume game in current state: {state}");
                return false;
            }

            try
            {
                currentGame.Resume();
                sessionTimer?.Resume();
                ChangeState(GameManagerState.Playing);

                // Update global state
                globalStateManager?.TransitionToState(GlobalGameState.Playing, $"Resumed {currentGame.GameId}");

                Debug.Log($"[GameManager] Resumed game: {currentGame.GameId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameManager] Failed to resume game: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// End the current mini-game and save results.
        /// </summary>
        /// <param name="gameResult">Final game result data</param>
        /// <returns>True if game ended successfully</returns>
        public bool EndCurrentGame(GameResult gameResult)
        {
            if (currentGame == null || !IsGameActive)
            {
                Debug.LogWarning($"[GameManager] Cannot end game in current state: {state}");
                return false;
            }

            try
            {
                // End game
                currentGame.End();
                
                // End session
                currentSession?.EndSession(gameResult);
                
                // Stop session timer
                sessionTimer?.Stop();

                // Save best score if improved
                if (currentSession != null && currentSession.Score > GetBestScore(currentSession.GameId))
                {
                    SetBestScore(currentSession.GameId, currentSession.Score);
                    SaveBestScores();
                }

                ChangeState(GameManagerState.GameOver);

                // Update global state
                globalStateManager?.TransitionToState(GlobalGameState.GameOver, 
                    $"Ended {currentGame.GameId} with {gameResult}");

                Debug.Log($"[GameManager] Ended game: {currentGame.GameId}, Result: {gameResult}, Score: {currentSession?.Score}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameManager] Failed to end game: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Score and Session Management

        /// <summary>
        /// Get the current score for the active game.
        /// </summary>
        /// <returns>Current score, or 0 if no game active</returns>
        public int GetCurrentScore()
        {
            return currentSession?.Score ?? 0;
        }

        /// <summary>
        /// Get the best score for a specific game type.
        /// </summary>
        /// <param name="gameId">Game identifier</param>
        /// <returns>Best score for the game type</returns>
        public int GetBestScore(string gameId)
        {
            return bestScores.TryGetValue(gameId, out int score) ? score : 0;
        }

        /// <summary>
        /// Get elapsed time for current game session.
        /// </summary>
        /// <returns>Time elapsed since game start</returns>
        public TimeSpan GetSessionTime()
        {
            return currentSession?.ElapsedTime ?? TimeSpan.Zero;
        }

        private void SetBestScore(string gameId, int score)
        {
            bestScores[gameId] = score;
            Debug.Log($"[GameManager] New best score for {gameId}: {score}");
        }

        #endregion

        #region State Management

        private void ChangeState(GameManagerState newState)
        {
            if (state == newState) return;

            GameManagerState oldState = state;
            state = newState;
            
            Debug.Log($"[GameManager] State changed: {oldState} -> {newState}");
            OnStateChanged?.Invoke(newState);
        }

        private void OnGlobalStateChanged(GlobalGameStateChangedEvent stateEvent)
        {
            // React to global state changes if needed
            Debug.Log($"[GameManager] Global state changed: {stateEvent.PreviousState} -> {stateEvent.CurrentState}");
        }

        #endregion

        #region Game Registration and Creation

        /// <summary>
        /// Register a mini-game type for loading.
        /// </summary>
        /// <typeparam name="T">Mini-game type</typeparam>
        /// <param name="gameId">Unique game identifier</param>
        public void RegisterGame<T>(string gameId) where T : IMiniGame
        {
            if (string.IsNullOrEmpty(gameId))
            {
                Debug.LogError("[GameManager] Cannot register game with null or empty ID");
                return;
            }

            registeredGames[gameId] = typeof(T);
            Debug.Log($"[GameManager] Registered game: {gameId} -> {typeof(T).Name}");
        }

        private async Task<IMiniGame> CreateGameInstanceAsync(string gameId)
        {
            if (!registeredGames.TryGetValue(gameId, out Type gameType))
            {
                Debug.LogError($"[GameManager] Game type not registered: {gameId}");
                return null;
            }

            try
            {
                // For now, create via Activator. In the future, this could use DI container
                IMiniGame gameInstance = (IMiniGame)Activator.CreateInstance(gameType);
                Debug.Log($"[GameManager] Created game instance: {gameId}");
                return gameInstance;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameManager] Failed to create game instance {gameId}: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Save/Load

        private void LoadBestScores()
        {
            try
            {
                bestScores.Clear();
                
                // Load from save system
                if (saveSystem != null)
                {
                    var savedScores = saveSystem.Load<Dictionary<string, int>>("BestScores");
                    if (savedScores != null)
                    {
                        bestScores = savedScores;
                        Debug.Log($"[GameManager] Loaded {bestScores.Count} best scores");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameManager] Failed to load best scores: {ex.Message}");
            }
        }

        private void SaveBestScores()
        {
            try
            {
                saveSystem?.Save("BestScores", bestScores);
                Debug.Log($"[GameManager] Saved {bestScores.Count} best scores");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameManager] Failed to save best scores: {ex.Message}");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Reset all game data and return to initial state.
        /// </summary>
        public void Reset()
        {
            // Unload current game
            if (currentGame != null)
            {
                _ = UnloadCurrentGameAsync();
            }

            // Reset state
            ChangeState(GameManagerState.Idle);
            
            Debug.Log("[GameManager] Reset to initial state");
        }

        /// <summary>
        /// Get all registered game IDs.
        /// </summary>
        /// <returns>Array of registered game identifiers</returns>
        public string[] GetRegisteredGameIds()
        {
            var gameIds = new string[registeredGames.Count];
            registeredGames.Keys.CopyTo(gameIds, 0);
            return gameIds;
        }

        #endregion

        #region Cleanup

        private void Cleanup()
        {
            // Unload current game
            if (currentGame != null)
            {
                currentGame.Cleanup();
            }

            // Save data
            SaveBestScores();

            // Cleanup timer
            sessionTimer?.Stop();

            Debug.Log("[GameManager] Cleanup completed");
        }

        #endregion
    }
} 