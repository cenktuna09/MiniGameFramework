using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.Architecture
{
    /// <summary>
    /// Base class for all mini-games in the framework.
    /// Provides common functionality and enforces the IMiniGame contract.
    /// </summary>
    public abstract class MiniGameBase : MonoBehaviour, IMiniGame
    {
        [Header("Mini Game Configuration")]
        [SerializeField] protected string gameId;
        [SerializeField] protected string displayName;
        
        /// <summary>
        /// Current state of the mini-game.
        /// </summary>
        protected GameState currentState = GameState.Uninitialized;
        
        /// <summary>
        /// Event fired when the game state changes.
        /// </summary>
        public event Action<GameState> OnStateChanged;
        
        #region IMiniGame Implementation
        
        public string GameId => gameId;
        public string DisplayName => displayName;
        public GameState CurrentState => currentState;
        public abstract bool IsPlayable { get; }
        
        public virtual async Task InitializeAsync()
        {
            if (currentState != GameState.Uninitialized)
            {
                Debug.LogWarning($"[{GameId}] InitializeAsync called when not in Uninitialized state: {currentState}");
                return;
            }
            
            SetState(GameState.Initializing);
            
            try
            {
                await OnInitializeAsync();
                SetState(GameState.Ready);
            }
            catch (Exception e)
            {
                Debug.LogError($"[{GameId}] Failed to initialize: {e.Message}");
                SetState(GameState.Uninitialized);
                throw;
            }
        }
        
        public virtual void Start()
        {
            if (currentState != GameState.Ready)
            {
                Debug.LogWarning($"[{GameId}] Start called when not in Ready state: {currentState}");
                return;
            }
            
            SetState(GameState.Playing);
            OnStart();
        }
        
        public virtual void Pause()
        {
            if (currentState != GameState.Playing)
            {
                Debug.LogWarning($"[{GameId}] Pause called when not in Playing state: {currentState}");
                return;
            }
            
            SetState(GameState.Paused);
            OnPause();
        }
        
        public virtual void Resume()
        {
            if (currentState != GameState.Paused)
            {
                Debug.LogWarning($"[{GameId}] Resume called when not in Paused state: {currentState}");
                return;
            }
            
            SetState(GameState.Playing);
            OnResume();
        }
        
        public virtual void End()
        {
            if (currentState != GameState.Playing && currentState != GameState.Paused)
            {
                Debug.LogWarning($"[{GameId}] End called when not in Playing or Paused state: {currentState}");
                return;
            }
            
            SetState(GameState.GameOver);
            OnEnd();
        }
        
        public virtual void Cleanup()
        {
            if (currentState == GameState.CleaningUp)
            {
                Debug.LogWarning($"[{GameId}] Cleanup called when already cleaning up");
                return;
            }
            
            SetState(GameState.CleaningUp);
            OnCleanup();
        }
        
        public abstract int GetCurrentScore();
        
        #endregion
        
        #region Protected Methods for Subclasses
        
        /// <summary>
        /// Called during initialization. Override to set up game-specific systems.
        /// </summary>
        protected virtual Task OnInitializeAsync()
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Called when the game starts. Override to begin gameplay.
        /// </summary>
        protected virtual void OnStart()
        {
        }
        
        /// <summary>
        /// Called when the game is paused. Override to pause game-specific systems.
        /// </summary>
        protected virtual void OnPause()
        {
        }
        
        /// <summary>
        /// Called when the game resumes. Override to resume game-specific systems.
        /// </summary>
        protected virtual void OnResume()
        {
        }
        
        /// <summary>
        /// Called when the game ends. Override to handle game over logic.
        /// </summary>
        protected virtual void OnEnd()
        {
        }
        
        /// <summary>
        /// Called during cleanup. Override to dispose of game-specific resources.
        /// </summary>
        protected virtual void OnCleanup()
        {
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Sets the current state and fires the OnStateChanged event.
        /// </summary>
        public void SetState(GameState newState)
        {
            if (currentState == newState)
                return;
                
            var oldState = currentState;
            currentState = newState;
            
            Debug.Log($"[{GameId}] State changed: {oldState} -> {newState}");
            OnStateChanged?.Invoke(newState);
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        protected virtual void Awake()
        {
            // Validate configuration
            if (string.IsNullOrEmpty(gameId))
            {
                Debug.LogError($"[{GetType().Name}] GameId is not set!");
            }
            
            if (string.IsNullOrEmpty(displayName))
            {
                Debug.LogError($"[{GetType().Name}] DisplayName is not set!");
            }
        }
        
        protected virtual void OnDestroy()
        {
            // Ensure cleanup is called
            if (currentState != GameState.Uninitialized && currentState != GameState.CleaningUp)
            {
                Cleanup();
            }
        }
        
        #endregion
    }
}