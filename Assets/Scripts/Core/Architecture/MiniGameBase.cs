using System;
using System.Threading.Tasks;
using UnityEngine;
using Core.Events;
using Core.DI;

namespace Core.Architecture
{
    /// <summary>
    /// Base class for all mini-games in the framework.
    /// Provides common functionality and enforces the IMiniGame contract.
    /// Supports scene-scoped services and lifecycle management.
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
                // Clear any existing scene services before initialization
                ServiceLocator.Instance.ClearSceneServices();
                
                await OnInitializeAsync();
                SetState(GameState.Ready);
                
                Debug.Log($"[{GameId}] Mini-game initialized successfully");
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
            
            SetState(GameState.CleaningUp);
            OnEnd();
            SetState(GameState.Uninitialized);
        }
        
        public virtual void Cleanup()
        {
            if (currentState == GameState.CleaningUp)
            {
                Debug.LogWarning($"[{GameId}] Cleanup already in progress");
                return;
            }
            
            SetState(GameState.CleaningUp);
            OnCleanup();
            
            // Clear scene-scoped services
            ServiceLocator.Instance.ClearSceneServices();
            
            SetState(GameState.Uninitialized);
            Debug.Log($"[{GameId}] Mini-game cleanup completed");
        }
        
        public abstract int GetCurrentScore();
        
        #endregion
        
        #region Protected Virtual Methods
        
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
            // Publish game over event using framework EventBus
            var eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            if (eventBus != null)
            {
                var gameOverEvent = new OnGameOverEvent(GameId, GetCurrentScore(), "GameOver", Time.time);
                eventBus.Publish(gameOverEvent);
                Debug.Log($"[{GameId}] Game over event published");
            }
        }
        
        /// <summary>
        /// Called during cleanup. Override to dispose of game-specific resources.
        /// </summary>
        protected virtual void OnCleanup()
        {
        }
        
        #endregion
        
        #region Service Management
        
        /// <summary>
        /// Register a scene-scoped service for this mini-game.
        /// </summary>
        /// <typeparam name="T">The type of service to register.</typeparam>
        /// <param name="service">The service instance.</param>
        protected void RegisterSceneService<T>(T service) where T : class
        {
            ServiceLocator.Instance.RegisterScene(service);
            Debug.Log($"[{GameId}] Registered scene service: {typeof(T).Name}");
        }
        
        /// <summary>
        /// Register a scene-scoped factory for this mini-game.
        /// </summary>
        /// <typeparam name="T">The type of service to register.</typeparam>
        /// <param name="factory">The factory function.</param>
        protected void RegisterSceneFactory<T>(Func<T> factory) where T : class
        {
            ServiceLocator.Instance.RegisterScene(factory);
            Debug.Log($"[{GameId}] Registered scene factory: {typeof(T).Name}");
        }
        
        /// <summary>
        /// Resolve a service (scene-scoped first, then global).
        /// </summary>
        /// <typeparam name="T">The type of service to resolve.</typeparam>
        /// <returns>The service instance, or null if not found.</returns>
        protected T ResolveService<T>() where T : class
        {
            return ServiceLocator.Instance.Resolve<T>();
        }
        
        /// <summary>
        /// Resolve a service with fallback creation if not found.
        /// </summary>
        /// <typeparam name="T">The type of service to resolve.</typeparam>
        /// <param name="fallbackFactory">Factory to create service if not found.</param>
        /// <returns>The service instance.</returns>
        protected T ResolveOrCreateService<T>(Func<T> fallbackFactory) where T : class
        {
            return ServiceLocator.Instance.ResolveOrCreate(fallbackFactory);
        }
        
        /// <summary>
        /// Check if a service is registered.
        /// </summary>
        /// <typeparam name="T">The type of service to check.</typeparam>
        /// <returns>True if the service is registered, false otherwise.</returns>
        protected bool IsServiceRegistered<T>() where T : class
        {
            return ServiceLocator.Instance.IsRegistered<T>();
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