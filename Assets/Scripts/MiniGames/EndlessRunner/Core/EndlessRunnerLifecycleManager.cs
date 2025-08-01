using System;
using System.Threading.Tasks;
using UnityEngine;
using Core.Architecture;
using Core.DI;
using Core.Events;
using EndlessRunner.StateManagement;
using EndlessRunner.Input;
using EndlessRunner.Scoring;
using EndlessRunner.Player;
using EndlessRunner.Events;

namespace EndlessRunner.Core
{
    /// <summary>
    /// Manages the lifecycle of the Endless Runner game.
    /// Follows framework pattern for easy extension by other mini-games.
    /// </summary>
    public class EndlessRunnerLifecycleManager
    {
        #region Private Fields
        
        private readonly IEventBus _eventBus;
        private readonly RunnerStateManager _stateManager;
        private readonly RunnerScoreManager _scoreManager;
        private readonly RunnerInputManager _inputManager;
        private readonly PlayerController _playerController;
        
        private bool _isGameRunning = false;
        private float _gameStartTime = 0f;
        private float _currentGameTime = 0f;
        
        #endregion
        
        #region Events
        
        public event Action OnGameStarted;
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        public event Action OnGameEnded;
        public event Action OnGameRestarted;
        
        #endregion
        
        #region Constructor
        
        public EndlessRunnerLifecycleManager(
            IEventBus eventBus,
            RunnerStateManager stateManager,
            RunnerScoreManager scoreManager,
            RunnerInputManager inputManager,
            PlayerController playerController)
        {
            _eventBus = eventBus;
            _stateManager = stateManager;
            _scoreManager = scoreManager;
            _inputManager = inputManager;
            _playerController = playerController;
            
            Debug.Log("[EndlessRunnerLifecycleManager] ✅ Lifecycle manager initialized");
        }
        
        #endregion
        
        #region Public Properties
        
        public bool IsGameRunning => _isGameRunning;
        public float GameTime => _currentGameTime;
        public float GameStartTime => _gameStartTime;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Start the game
        /// </summary>
        public void StartGame()
        {
            if (_isGameRunning)
            {
                Debug.LogWarning("[EndlessRunnerLifecycleManager] ⚠️ Game already running!");
                return;
            }
            
            _isGameRunning = true;
            _gameStartTime = Time.time;
            _currentGameTime = 0f;
            
            // Initialize systems
            InitializeSystems();
            
            // Start game state
            _stateManager?.TransitionTo(RunnerGameState.Running);
            
            // Unlock input for new game
            _inputManager?.UnlockInput();
            
            // Publish game started event
            _eventBus?.Publish(new GameStartedEvent(Time.time));
            
            OnGameStarted?.Invoke();
            Debug.Log("[EndlessRunnerLifecycleManager] 🎮 Game started");
        }
        
        /// <summary>
        /// Pause the game
        /// </summary>
        public void PauseGame()
        {
            if (!_isGameRunning) return;
            
            _stateManager?.TransitionTo(RunnerGameState.Paused);
            
            OnGamePaused?.Invoke();
            Debug.Log("[EndlessRunnerLifecycleManager] ⏸️ Game paused");
        }
        
        /// <summary>
        /// Resume the game
        /// </summary>
        public void ResumeGame()
        {
            if (_stateManager?.CurrentState != RunnerGameState.Paused) return;
            
            _stateManager?.TransitionTo(RunnerGameState.Running);
            
            OnGameResumed?.Invoke();
            Debug.Log("[EndlessRunnerLifecycleManager] ▶️ Game resumed");
        }
        
        /// <summary>
        /// End the game
        /// </summary>
        public void EndGame()
        {
            if (!_isGameRunning) return;
            
            _isGameRunning = false;
            _stateManager?.TransitionTo(RunnerGameState.GameOver);
            
            // Save final score
            _scoreManager?.EndGame();
            
            OnGameEnded?.Invoke();
            Debug.Log("[EndlessRunnerLifecycleManager] 🏁 Game ended");
        }
        
        /// <summary>
        /// Restart the game
        /// </summary>
        public void RestartGame()
        {
            ResetGame();
            StartGame();
            
            OnGameRestarted?.Invoke();
            Debug.Log("[EndlessRunnerLifecycleManager] 🔄 Game restarted");
        }
        
        /// <summary>
        /// Reset the game state
        /// </summary>
        public void ResetGame()
        {
            _isGameRunning = false;
            _currentGameTime = 0f;
            
            // Reset all systems
            _stateManager?.TransitionTo(RunnerGameState.Ready);
            _scoreManager?.ResetScore();
            _inputManager?.ResetInputState();
            _playerController?.ResetPlayer();
            
            // Unlock input after reset
            _inputManager?.UnlockInput();
            
            Debug.Log("[EndlessRunnerLifecycleManager] 🔄 Game state reset");
        }
        
        /// <summary>
        /// Update game time and systems
        /// </summary>
        public void Update()
        {
            if (_isGameRunning)
            {
                UpdateGameTime();
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Initialize all systems
        /// </summary>
        private void InitializeSystems()
        {
            // Initialize state manager
            _stateManager?.TransitionTo(RunnerGameState.Ready);
            
            // Initialize score manager
            _scoreManager?.ResetScore();
            
            // Initialize input manager
            _inputManager?.ResetInputState();
            
            Debug.Log("[EndlessRunnerLifecycleManager] ✅ All systems initialized");
        }
        
        /// <summary>
        /// Update game time
        /// </summary>
        private void UpdateGameTime()
        {
            if (_isGameRunning)
            {
                _currentGameTime = Time.time - _gameStartTime;
            }
        }
        
        #endregion
    }
} 