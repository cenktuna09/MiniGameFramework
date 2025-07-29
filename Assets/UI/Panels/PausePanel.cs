using UnityEngine;
using UnityEngine.UI;
using MiniGameFramework.Core.Events;

namespace MiniGameFramework.UI.Panels
{
    /// <summary>
    /// Pause panel for in-game pause functionality.
    /// Provides options to resume, access settings, return to main menu, or quit.
    /// </summary>
    public class PausePanel : UIPanel
    {
        [Header("Pause UI Elements")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _restartButton;

        [Header("Pause Configuration")]
        [SerializeField] private bool _pauseTimeOnShow = true;
        [SerializeField] private bool _showQuitButton = true;
        [SerializeField] private bool _showRestartButton = true;
        [SerializeField] private bool _confirmBeforeMainMenu = true;
        [SerializeField] private bool _confirmBeforeQuit = true;

        // Event system for communication
        private IEventBus _eventBus;
        private bool _wasPausedBeforeShow = false;

        #region Events

        /// <summary>
        /// Called when resume is requested
        /// </summary>
        public event System.Action OnResumeRequested;

        /// <summary>
        /// Called when restart is requested
        /// </summary>
        public event System.Action OnRestartRequested;

        /// <summary>
        /// Called when main menu return is requested
        /// </summary>
        public event System.Action OnMainMenuRequested;

        /// <summary>
        /// Called when quit is requested
        /// </summary>
        public event System.Action OnQuitRequested;

        #endregion

        #region Unity Lifecycle

        protected override void OnPanelInitialized()
        {
            base.OnPanelInitialized();
            
            // Get EventBus from ServiceLocator
            _eventBus = Core.DI.ServiceLocator.Instance.Get<IEventBus>();
            
            SetupButtonListeners();
            ConfigureUI();
        }

        protected override void OnPanelDestroyed()
        {
            base.OnPanelDestroyed();
            RemoveButtonListeners();
        }

        #endregion

        #region UI Setup

        private void SetupButtonListeners()
        {
            if (_resumeButton != null)
            {
                _resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitClicked);
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(OnRestartClicked);
            }
        }

        private void RemoveButtonListeners()
        {
            if (_resumeButton != null)
            {
                _resumeButton.onClick.RemoveListener(OnResumeClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveListener(OnQuitClicked);
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            }
        }

        private void ConfigureUI()
        {
            // Configure button visibility based on settings
            if (_quitButton != null)
            {
                _quitButton.gameObject.SetActive(_showQuitButton);
            }

            if (_restartButton != null)
            {
                _restartButton.gameObject.SetActive(_showRestartButton);
            }

            // Hide quit button on mobile platforms where it's less relevant
#if UNITY_ANDROID || UNITY_IOS
            if (_quitButton != null)
            {
                _quitButton.gameObject.SetActive(false);
            }
#endif
        }

        #endregion

        #region Button Event Handlers

        private void OnResumeClicked()
        {
            LogIfEnabled("Resume game requested");
            
            OnResumeRequested?.Invoke();
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new GamePauseEvent { IsPaused = false });
            }
            
            // Hide this panel to resume the game
            _ = HideAsync();
        }

        private void OnSettingsClicked()
        {
            LogIfEnabled("Settings requested from pause menu");
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new UINavigationEvent 
                { 
                    PanelId = "SettingsPanel", 
                    Action = UINavigationAction.Push 
                });
            }
        }

        private void OnMainMenuClicked()
        {
            LogIfEnabled("Main menu requested from pause");
            
            if (_confirmBeforeMainMenu)
            {
                ShowMainMenuConfirmation();
            }
            else
            {
                ProcessMainMenuRequest();
            }
        }

        private void OnQuitClicked()
        {
            LogIfEnabled("Quit game requested from pause");
            
            if (_confirmBeforeQuit)
            {
                ShowQuitConfirmation();
            }
            else
            {
                ProcessQuitRequest();
            }
        }

        private void OnRestartClicked()
        {
            LogIfEnabled("Restart game requested from pause");
            
            OnRestartRequested?.Invoke();
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new GameRestartEvent());
            }
        }

        #endregion

        #region Confirmation Dialogs

        private void ShowMainMenuConfirmation()
        {
            if (_eventBus != null)
            {
                var confirmationData = new ConfirmationDialogData
                {
                    Title = "Return to Main Menu",
                    Message = "Are you sure you want to return to the main menu? Your current progress will be lost.",
                    ConfirmText = "Yes",
                    CancelText = "No",
                    OnConfirm = ProcessMainMenuRequest,
                    OnCancel = () => LogIfEnabled("Main menu request cancelled")
                };
                
                _eventBus.Publish(new ShowConfirmationDialogEvent { Data = confirmationData });
            }
            else
            {
                ProcessMainMenuRequest();
            }
        }

        private void ShowQuitConfirmation()
        {
            if (_eventBus != null)
            {
                var confirmationData = new ConfirmationDialogData
                {
                    Title = "Quit Game",
                    Message = "Are you sure you want to quit the game? Your current progress will be lost.",
                    ConfirmText = "Quit",
                    CancelText = "Cancel",
                    OnConfirm = ProcessQuitRequest,
                    OnCancel = () => LogIfEnabled("Quit request cancelled")
                };
                
                _eventBus.Publish(new ShowConfirmationDialogEvent { Data = confirmationData });
            }
            else
            {
                ProcessQuitRequest();
            }
        }

        #endregion

        #region Request Processing

        private void ProcessMainMenuRequest()
        {
            LogIfEnabled("Processing main menu request");
            
            OnMainMenuRequested?.Invoke();
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new GameExitEvent { ExitType = GameExitType.MainMenu });
            }
        }

        private void ProcessQuitRequest()
        {
            LogIfEnabled("Processing quit request");
            
            OnQuitRequested?.Invoke();
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new GameExitEvent { ExitType = GameExitType.QuitApplication });
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Panel Lifecycle Overrides

        protected override void OnPanelShowStarted()
        {
            base.OnPanelShowStarted();
            LogIfEnabled("Pause panel is showing");
            
            // Store current pause state and pause the game if needed
            _wasPausedBeforeShow = Time.timeScale == 0f;
            
            if (_pauseTimeOnShow && !_wasPausedBeforeShow)
            {
                Time.timeScale = 0f;
                
                if (_eventBus != null)
                {
                    _eventBus.Publish(new GamePauseEvent { IsPaused = true });
                }
            }
        }

        protected override void OnPanelShowCompleted()
        {
            base.OnPanelShowCompleted();
            LogIfEnabled("Pause panel is now visible");
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new UIStateEvent { PanelId = "PausePanel", State = UIState.Visible });
            }
        }

        protected override void OnPanelHideStarted()
        {
            base.OnPanelHideStarted();
            LogIfEnabled("Pause panel is hiding");
            
            // Resume time if we paused it and it wasn't paused before we showed
            if (_pauseTimeOnShow && !_wasPausedBeforeShow)
            {
                Time.timeScale = 1f;
                
                if (_eventBus != null)
                {
                    _eventBus.Publish(new GamePauseEvent { IsPaused = false });
                }
            }
        }

        protected override void OnPanelHideCompleted()
        {
            base.OnPanelHideCompleted();
            LogIfEnabled("Pause panel is now hidden");
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new UIStateEvent { PanelId = "PausePanel", State = UIState.Hidden });
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets whether confirmation dialogs should be shown for destructive actions
        /// </summary>
        /// <param name="confirmMainMenu">Show confirmation for main menu</param>
        /// <param name="confirmQuit">Show confirmation for quit</param>
        public void SetConfirmationPreferences(bool confirmMainMenu, bool confirmQuit)
        {
            _confirmBeforeMainMenu = confirmMainMenu;
            _confirmBeforeQuit = confirmQuit;
        }

        /// <summary>
        /// Updates button states based on game context
        /// </summary>
        /// <param name="canRestart">Whether restart is available</param>
        /// <param name="canReturnToMenu">Whether return to menu is available</param>
        public void UpdateButtonStates(bool canRestart = true, bool canReturnToMenu = true)
        {
            if (_restartButton != null)
            {
                _restartButton.interactable = canRestart;
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.interactable = canReturnToMenu;
            }
        }

        #endregion

        #region Private Methods

        private void LogIfEnabled(string message)
        {
            Debug.Log($"[PausePanel] {message}", this);
        }

        #endregion
    }

    #region Event Classes

    /// <summary>
    /// Event for game pause state changes
    /// </summary>
    public class GamePauseEvent
    {
        public bool IsPaused { get; set; }
        public string Source { get; set; } = "PausePanel";
    }

    /// <summary>
    /// Event for game restart requests
    /// </summary>
    public class GameRestartEvent
    {
        public string Source { get; set; } = "PausePanel";
        public System.Collections.Generic.Dictionary<string, object> Parameters { get; set; } = new();
    }

    /// <summary>
    /// Event for game exit requests
    /// </summary>
    public class GameExitEvent
    {
        public GameExitType ExitType { get; set; }
        public string Source { get; set; } = "PausePanel";
    }

    /// <summary>
    /// Types of game exit
    /// </summary>
    public enum GameExitType
    {
        MainMenu,
        QuitApplication,
        Desktop
    }

    /// <summary>
    /// Event for showing confirmation dialogs
    /// </summary>
    public class ShowConfirmationDialogEvent
    {
        public ConfirmationDialogData Data { get; set; }
    }

    /// <summary>
    /// Data for confirmation dialogs
    /// </summary>
    public class ConfirmationDialogData
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string ConfirmText { get; set; } = "OK";
        public string CancelText { get; set; } = "Cancel";
        public System.Action OnConfirm { get; set; }
        public System.Action OnCancel { get; set; }
    }

    #endregion
} 