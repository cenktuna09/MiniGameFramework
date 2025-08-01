using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Core.Architecture;
using MiniGameFramework.UI.Panels;
using Core.DI;
namespace MiniGameFramework.UI
{
    /// <summary>
    /// Central UI Manager that handles navigation, event coordination, and panel management.
    /// Serves as the main interface between the game logic and UI systems.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Manager Configuration")]
        [SerializeField] private UIPanelStackManager _panelStackManager;
        [SerializeField] private Canvas _mainCanvas;
        [SerializeField] private bool _logUIEvents = false;
        [SerializeField] private bool _initializeOnAwake = true;

        [Header("Panel References")]
        [SerializeField] private MainMenuPanel _mainMenuPanel;
        [SerializeField] private LoadingPanel _loadingPanel;
        [SerializeField] private PausePanel _pausePanel;
        [SerializeField] private GameOverPanel _gameOverPanel;

        // Core dependencies
        private IEventBus _eventBus;
        private bool _isInitialized = false;

        // Event subscriptions for cleanup
        private readonly List<IDisposable> _eventSubscriptions = new();

        #region Properties

        /// <summary>
        /// Whether the UI Manager is initialized and ready
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// The main panel stack manager
        /// </summary>
        public UIPanelStackManager PanelStackManager => _panelStackManager;

        /// <summary>
        /// Currently active panel
        /// </summary>
        public UIPanel CurrentPanel => _panelStackManager?.CurrentPanel;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_initializeOnAwake)
            {
                Initialize();
            }
        }

        private void Start()
        {
            // Show main menu by default if no other panel is active
            if (_isInitialized && (_panelStackManager.CurrentPanel == null))
            {
                ShowMainMenu();
            }
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the UI Manager and sets up event subscriptions
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
                return;

            // Get EventBus from ServiceLocator
            _eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            
            if (_eventBus == null)
            {
                Debug.LogError("EventBus not found in ServiceLocator! UI Manager requires EventBus to function.", this);
                return;
            }

            // Ensure we have a panel stack manager
            if (_panelStackManager == null)
            {
                _panelStackManager = GetComponentInChildren<UIPanelStackManager>();
                
                if (_panelStackManager == null)
                {
                    Debug.LogError("UIPanelStackManager not found! Creating one automatically.", this);
                    var stackManagerGO = new GameObject("PanelStackManager");
                    stackManagerGO.transform.SetParent(transform);
                    _panelStackManager = stackManagerGO.AddComponent<UIPanelStackManager>();
                }
            }

            SetupEventSubscriptions();
            RegisterPanels();
            
            _isInitialized = true;
            LogIfEnabled("UI Manager initialized successfully");
        }

        private void SetupEventSubscriptions()
        {
            // Subscribe to UI navigation events
            var uiNavSub = _eventBus.Subscribe<UINavigationEvent>(OnUINavigationEvent);
            _eventSubscriptions.Add(uiNavSub);

            // Subscribe to game state events
            var gameSelectionSub = _eventBus.Subscribe<GameSelectionEvent>(OnGameSelectionEvent);
            _eventSubscriptions.Add(gameSelectionSub);

            var gameExitSub = _eventBus.Subscribe<GameExitEvent>(OnGameExitEvent);
            _eventSubscriptions.Add(gameExitSub);

            var gameRestartSub = _eventBus.Subscribe<GameRestartEvent>(OnGameRestartEvent);
            _eventSubscriptions.Add(gameRestartSub);

            // Subscribe to application events
            var appEventSub = _eventBus.Subscribe<ApplicationEvent>(OnApplicationEvent);
            _eventSubscriptions.Add(appEventSub);

            // Subscribe to confirmation dialog events
            var confirmationSub = _eventBus.Subscribe<ShowConfirmationDialogEvent>(OnShowConfirmationDialog);
            _eventSubscriptions.Add(confirmationSub);

            LogIfEnabled("UI event subscriptions set up");
        }

        private void RegisterPanels()
        {
            // Register all known panels with the stack manager
            if (_mainMenuPanel != null)
                _panelStackManager.RegisterPanel("MainMenu", _mainMenuPanel);

            if (_loadingPanel != null)
                _panelStackManager.RegisterPanel("Loading", _loadingPanel);

            if (_pausePanel != null)
                _panelStackManager.RegisterPanel("Pause", _pausePanel);

            if (_gameOverPanel != null)
                _panelStackManager.RegisterPanel("GameOver", _gameOverPanel);

            LogIfEnabled($"Registered {_panelStackManager.RegisteredPanels.Count} panels");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Shows the main menu panel
        /// </summary>
        public async Task ShowMainMenuAsync()
        {
            await _panelStackManager.ClearStackAsync("MainMenu");
        }

        /// <summary>
        /// Shows the main menu panel (fire and forget)
        /// </summary>
        public void ShowMainMenu()
        {
            _panelStackManager.ClearStack("MainMenu");
        }

        /// <summary>
        /// Shows the loading panel with optional progress tracking
        /// </summary>
        /// <param name="showCancelButton">Whether to show cancel button</param>
        /// <param name="loadingMessage">Initial loading message</param>
        public async Task ShowLoadingAsync(bool showCancelButton = false, string loadingMessage = null)
        {
            await _panelStackManager.ShowPanelAsync("Loading");
            
            if (_loadingPanel != null)
            {
                _loadingPanel.StartLoading(showCancelButton);
                
                if (!string.IsNullOrEmpty(loadingMessage))
                {
                    _loadingPanel.SetLoadingMessage(loadingMessage);
                }
            }
        }

        /// <summary>
        /// Updates loading progress
        /// </summary>
        /// <param name="progress">Progress value 0-1</param>
        /// <param name="message">Optional progress message</param>
        public void UpdateLoadingProgress(float progress, string message = null)
        {
            if (_loadingPanel != null)
            {
                _loadingPanel.SetProgress(progress);
                
                if (!string.IsNullOrEmpty(message))
                {
                    _loadingPanel.SetLoadingMessage(message);
                }
            }
        }

        /// <summary>
        /// Completes and hides the loading panel
        /// </summary>
        public async Task CompleteLoadingAsync()
        {
            if (_loadingPanel != null)
            {
                _loadingPanel.CompleteLoading();
                
                // Wait a moment for the completion animation
                await Task.Delay(500);
                
                await _panelStackManager.HidePanelAsync("Loading");
            }
        }

        /// <summary>
        /// Shows the pause panel
        /// </summary>
        public async Task ShowPauseMenuAsync()
        {
            await _panelStackManager.PushPanelAsync("Pause", false); // Don't hide current panel
        }

        /// <summary>
        /// Shows the game over panel with results
        /// </summary>
        /// <param name="gameOverData">Game over data to display</param>
        public async Task ShowGameOverAsync(GameOverData gameOverData)
        {
            // Clear the stack first, then show game over
            await _panelStackManager.ClearStackAsync();
            
            if (_gameOverPanel != null)
            {
                _gameOverPanel.ShowGameOver(gameOverData);
            }
        }

        /// <summary>
        /// Navigates back in the panel stack
        /// </summary>
        public async Task<UIPanelStackManager.NavigationResult> GoBackAsync()
        {
            return await _panelStackManager.PopPanelAsync();
        }

        /// <summary>
        /// Gets a specific panel by ID
        /// </summary>
        /// <typeparam name="T">Panel type</typeparam>
        /// <param name="panelId">Panel identifier</param>
        /// <returns>Panel instance or null</returns>
        public T GetPanel<T>(string panelId) where T : UIPanel
        {
            return _panelStackManager.GetPanel(panelId) as T;
        }

        /// <summary>
        /// Checks if a specific panel is currently visible
        /// </summary>
        /// <param name="panelId">Panel identifier</param>
        /// <returns>True if panel is visible</returns>
        public bool IsPanelVisible(string panelId)
        {
            var panel = _panelStackManager.GetPanel(panelId);
            return panel != null && panel.IsVisible;
        }

        #endregion

        #region Event Handlers

        private async void OnUINavigationEvent(UINavigationEvent navigationEvent)
        {
            LogIfEnabled($"UI Navigation: {navigationEvent.Action} - {navigationEvent.PanelId}");

            switch (navigationEvent.Action)
            {
                case UINavigationAction.Push:
                    await _panelStackManager.PushPanelAsync(navigationEvent.PanelId);
                    break;
                    
                case UINavigationAction.Pop:
                    await _panelStackManager.PopPanelAsync();
                    break;
                    
                case UINavigationAction.Replace:
                    await _panelStackManager.ReplacePanelAsync(navigationEvent.PanelId);
                    break;
                    
                case UINavigationAction.Show:
                    await _panelStackManager.ShowPanelAsync(navigationEvent.PanelId);
                    break;
                    
                case UINavigationAction.Hide:
                    await _panelStackManager.HidePanelAsync(navigationEvent.PanelId);
                    break;
            }
        }

        private void OnGameSelectionEvent(GameSelectionEvent gameSelection)
        {
            LogIfEnabled($"Game selected: {gameSelection.GameType}");
            
            // This would typically trigger scene loading or game initialization
            // For now, we'll just show a loading screen
            _ = ShowLoadingAsync(false, $"Loading {gameSelection.GameType}...");
        }

        private async void OnGameExitEvent(GameExitEvent gameExit)
        {
            LogIfEnabled($"Game exit requested: {gameExit.ExitType}");

            switch (gameExit.ExitType)
            {
                case GameExitType.MainMenu:
                    await ShowMainMenuAsync();
                    break;
                    
                case GameExitType.QuitApplication:
                    // Could show confirmation dialog here
                    Application.Quit();
                    break;
            }
        }

        private void OnGameRestartEvent(GameRestartEvent gameRestart)
        {
            LogIfEnabled("Game restart requested");
            
            // Hide current UI and show loading
            _ = ShowLoadingAsync(false, "Restarting game...");
        }

        private void OnApplicationEvent(ApplicationEvent appEvent)
        {
            LogIfEnabled($"Application event: {appEvent.Action}");

            switch (appEvent.Action)
            {
                case ApplicationAction.Quit:
                    Application.Quit();
                    break;
                    
                case ApplicationAction.Pause:
                    // Could show pause menu automatically
                    break;
            }
        }

        private void OnShowConfirmationDialog(ShowConfirmationDialogEvent confirmationEvent)
        {
            LogIfEnabled($"Confirmation dialog requested: {confirmationEvent.Data.Title}");
            
            // This would typically show a confirmation dialog
            // For now, we'll just log and auto-confirm or cancel based on some logic
            // In a real implementation, you'd show a proper dialog UI
            
            // Simple implementation: if we're in the editor, auto-confirm for testing
#if UNITY_EDITOR
            confirmationEvent.Data.OnConfirm?.Invoke();
#else
            // In build, you might want to show a proper UI dialog
            confirmationEvent.Data.OnCancel?.Invoke();
#endif
        }

        #endregion

        #region Cleanup

        private void Cleanup()
        {
            // Unsubscribe from all events
            foreach (var subscription in _eventSubscriptions)
            {
                subscription?.Dispose();
            }
            _eventSubscriptions.Clear();

            _isInitialized = false;
            LogIfEnabled("UI Manager cleaned up");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Sets the active state of the entire UI system
        /// </summary>
        /// <param name="active">Whether UI should be active</param>
        public void SetUIActive(bool active)
        {
            if (_mainCanvas != null)
            {
                _mainCanvas.gameObject.SetActive(active);
            }
            else
            {
                gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// Gets UI navigation statistics for debugging
        /// </summary>
        /// <returns>Dictionary with UI stats</returns>
        public Dictionary<string, object> GetUIStats()
        {
            return new Dictionary<string, object>
            {
                ["StackCount"] = _panelStackManager?.StackCount ?? 0,
                ["CurrentPanel"] = _panelStackManager?.CurrentPanel?.name ?? "None",
                ["RegisteredPanels"] = _panelStackManager?.RegisteredPanels.Count ?? 0,
                ["IsNavigating"] = _panelStackManager?.IsNavigating ?? false,
                ["CanGoBack"] = _panelStackManager?.CanGoBack ?? false
            };
        }

        private void LogIfEnabled(string message)
        {
            if (_logUIEvents)
            {
                Debug.Log($"[UIManager] {message}", this);
            }
        }

        #endregion

        #region Static Access (Optional - for convenience)

        private static UIManager _instance;

        /// <summary>
        /// Singleton instance for easy access (optional pattern)
        /// </summary>
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<UIManager>();
                }
                return _instance;
            }
        }

        private void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Debug.LogWarning("Multiple UIManager instances found! Using the first one.", this);
            }
        }

        private void OnDisable()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion
    }
} 