using System;
using System.Threading.Tasks;
using UnityEngine;
using Core.Architecture;
using Core.DI;
using Core.Events;
using EndlessRunner.StateManagement;
using EndlessRunner.Input;
using EndlessRunner.Scoring;
using EndlessRunner.Performance;
using EndlessRunner.ErrorHandling;
using EndlessRunner.Player;
using EndlessRunner.World;

namespace EndlessRunner.Core
{
    /// <summary>
    /// Handles initialization of all Endless Runner systems.
    /// Follows framework pattern for easy extension by other mini-games.
    /// </summary>
    public class EndlessRunnerSystemInitializer
    {
        #region Private Fields
        
        private readonly IEventBus _eventBus;
        private readonly bool _enableDebugLogging;
        
        // Core systems
        private RunnerStateManager _stateManager;
        private RunnerScoreManager _scoreManager;
        private RunnerInputManager _inputManager;
        private RunnerPerformanceManager _performanceManager;
        private RunnerErrorHandler _errorHandler;
        
        // Game systems
        private PlayerController _playerController;
        private EndlessRunnerScrollController _scrollController;
        
        #endregion
        
        #region Events
        
        public event Action<RunnerStateManager> OnStateManagerInitialized;
        public event Action<RunnerScoreManager> OnScoreManagerInitialized;
        public event Action<RunnerInputManager> OnInputManagerInitialized;
        public event Action<PlayerController> OnPlayerControllerInitialized;
        public event Action<EndlessRunnerScrollController> OnScrollControllerInitialized;
        public event Action OnAllSystemsInitialized;
        
        #endregion
        
        #region Constructor
        
        public EndlessRunnerSystemInitializer(IEventBus eventBus, bool enableDebugLogging = true)
        {
            _eventBus = eventBus;
            _enableDebugLogging = enableDebugLogging;
            
            Debug.Log("[EndlessRunnerSystemInitializer] ✅ System initializer created");
        }
        
        #endregion
        
        #region Public Properties
        
        public RunnerStateManager StateManager => _stateManager;
        public RunnerScoreManager ScoreManager => _scoreManager;
        public RunnerInputManager InputManager => _inputManager;
        public RunnerPerformanceManager PerformanceManager => _performanceManager;
        public RunnerErrorHandler ErrorHandler => _errorHandler;
        public PlayerController PlayerController => _playerController;
        public EndlessRunnerScrollController ScrollController => _scrollController;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Initialize all core systems
        /// </summary>
        public async Task InitializeCoreSystemsAsync()
        {
            Debug.Log("[EndlessRunnerSystemInitializer] 🎮 Initializing core systems...");
            
            try
            {
                // Initialize state manager
                await InitializeStateManagerAsync();
                
                // Initialize score manager
                await InitializeScoreManagerAsync();
                
                // Initialize input manager
                await InitializeInputManagerAsync();
                
                // Initialize performance manager
                await InitializePerformanceManagerAsync();
                
                // Initialize error handler
                await InitializeErrorHandlerAsync();
                
                Debug.Log("[EndlessRunnerSystemInitializer] ✅ Core systems initialized successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EndlessRunnerSystemInitializer] ❌ Failed to initialize core systems: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Find and initialize game systems in scene
        /// </summary>
        public async Task InitializeGameSystemsAsync()
        {
            Debug.Log("[EndlessRunnerSystemInitializer] 🎮 Finding and initializing game systems...");
            
            try
            {
                // Find and initialize player controller
                await InitializePlayerControllerAsync();
                
                // Find and initialize scroll controller
                await InitializeScrollControllerAsync();
                
                Debug.Log("[EndlessRunnerSystemInitializer] ✅ Game systems initialized successfully");
                OnAllSystemsInitialized?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[EndlessRunnerSystemInitializer] ❌ Failed to initialize game systems: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Initialize all systems
        /// </summary>
        public async Task InitializeAllSystemsAsync()
        {
            await InitializeCoreSystemsAsync();
            await InitializeGameSystemsAsync();
        }
        
        /// <summary>
        /// Get initialization status
        /// </summary>
        public bool AreAllSystemsInitialized()
        {
            return _stateManager != null &&
                   _scoreManager != null &&
                   _inputManager != null &&
                   _performanceManager != null &&
                   _errorHandler != null &&
                   _playerController != null &&
                   _scrollController != null;
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Initialize state manager
        /// </summary>
        private async Task InitializeStateManagerAsync()
        {
            _stateManager = new RunnerStateManager(_eventBus);
            OnStateManagerInitialized?.Invoke(_stateManager);
            
            await Task.CompletedTask;
            Debug.Log("[EndlessRunnerSystemInitializer] ✅ State manager initialized");
        }
        
        /// <summary>
        /// Initialize score manager
        /// </summary>
        private async Task InitializeScoreManagerAsync()
        {
            _scoreManager = new RunnerScoreManager(_eventBus);
            OnScoreManagerInitialized?.Invoke(_scoreManager);
            
            await Task.CompletedTask;
            Debug.Log("[EndlessRunnerSystemInitializer] ✅ Score manager initialized");
        }
        
        /// <summary>
        /// Initialize input manager
        /// </summary>
        private async Task InitializeInputManagerAsync()
        {
            _inputManager = new RunnerInputManager(_eventBus);
            OnInputManagerInitialized?.Invoke(_inputManager);
            
            await Task.CompletedTask;
            Debug.Log("[EndlessRunnerSystemInitializer] ✅ Input manager initialized");
        }
        
        /// <summary>
        /// Initialize performance manager
        /// </summary>
        private async Task InitializePerformanceManagerAsync()
        {
            _performanceManager = new RunnerPerformanceManager(_eventBus);
            
            await Task.CompletedTask;
            Debug.Log("[EndlessRunnerSystemInitializer] ✅ Performance manager initialized");
        }
        
        /// <summary>
        /// Initialize error handler
        /// </summary>
        private async Task InitializeErrorHandlerAsync()
        {
            _errorHandler = new RunnerErrorHandler(_eventBus);
            _errorHandler.SetLoggingEnabled(_enableDebugLogging);
            
            await Task.CompletedTask;
            Debug.Log("[EndlessRunnerSystemInitializer] ✅ Error handler initialized");
        }
        
        /// <summary>
        /// Find and initialize player controller
        /// </summary>
        private async Task InitializePlayerControllerAsync()
        {
            _playerController = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            if (_playerController != null)
            {
                _playerController.Initialize(_eventBus);
                OnPlayerControllerInitialized?.Invoke(_playerController);
                Debug.Log("[EndlessRunnerSystemInitializer] ✅ Player controller found and initialized");
            }
            else
            {
                Debug.LogWarning("[EndlessRunnerSystemInitializer] ⚠️ PlayerController not found in scene");
            }
            
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Find and initialize scroll controller
        /// </summary>
        private async Task InitializeScrollControllerAsync()
        {
            _scrollController = UnityEngine.Object.FindFirstObjectByType<EndlessRunnerScrollController>();
            if (_scrollController != null)
            {
                _scrollController.Initialize(_eventBus);
                OnScrollControllerInitialized?.Invoke(_scrollController);
                Debug.Log("[EndlessRunnerSystemInitializer] ✅ Scroll controller found and initialized");
            }
            else
            {
                Debug.LogWarning("[EndlessRunnerSystemInitializer] ⚠️ EndlessRunnerScrollController not found in scene");
            }
            
            await Task.CompletedTask;
        }
        
        #endregion
    }
} 