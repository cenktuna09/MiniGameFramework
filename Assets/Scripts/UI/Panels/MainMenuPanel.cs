using UnityEngine;
using UnityEngine.UI;
using Core.Architecture;

namespace MiniGameFramework.UI.Panels
{
    /// <summary>
    /// Main menu panel that provides navigation to different mini-games and settings.
    /// Extends UIPanel for consistent animation and lifecycle management.
    /// </summary>
    public class MainMenuPanel : UIPanel
    {
        [Header("Main Menu UI Elements")]
        [SerializeField] private Button _playMatch3Button;
        [SerializeField] private Button _playEndlessRunnerButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _achievementsButton;
        [SerializeField] private Button _exitButton;
        
        [Header("Menu Configuration")]
        [SerializeField] private bool _showExitButtonOnMobile = false;

        // Event system for communication
        private IEventBus _eventBus;

        #region Unity Lifecycle

        protected override void OnPanelInitialized()
        {
            base.OnPanelInitialized();
            
            // Get EventBus from ServiceLocator
            _eventBus = Core.DI.ServiceLocator.Instance.Resolve<IEventBus>();
            
            SetupButtonListeners();
            ConfigurePlatformSpecificUI();
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
            if (_playMatch3Button != null)
            {
                _playMatch3Button.onClick.AddListener(OnPlayMatch3Clicked);
            }

            if (_playEndlessRunnerButton != null)
            {
                _playEndlessRunnerButton.onClick.AddListener(OnPlayEndlessRunnerClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (_achievementsButton != null)
            {
                _achievementsButton.onClick.AddListener(OnAchievementsClicked);
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.AddListener(OnExitClicked);
            }
        }

        private void RemoveButtonListeners()
        {
            if (_playMatch3Button != null)
            {
                _playMatch3Button.onClick.RemoveListener(OnPlayMatch3Clicked);
            }

            if (_playEndlessRunnerButton != null)
            {
                _playEndlessRunnerButton.onClick.RemoveListener(OnPlayEndlessRunnerClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            }

            if (_achievementsButton != null)
            {
                _achievementsButton.onClick.RemoveListener(OnAchievementsClicked);
            }

            if (_exitButton != null)
            {
                _exitButton.onClick.RemoveListener(OnExitClicked);
            }
        }

        private void ConfigurePlatformSpecificUI()
        {
            // Hide exit button on mobile platforms if configured
            if (_exitButton != null && !_showExitButtonOnMobile)
            {
#if UNITY_ANDROID || UNITY_IOS
                _exitButton.gameObject.SetActive(false);
#endif
            }
        }

        #endregion

        #region Button Event Handlers

        private void OnPlayMatch3Clicked()
        {
            LogIfEnabled("Match3 game selected");
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new GameSelectionEvent { GameType = "Match3" });
            }
            
            // Could also directly load the scene using SceneManager
            // Core.SceneManagement.SceneManagerImpl.Instance.LoadSceneAsync("Match3Game");
        }

        private void OnPlayEndlessRunnerClicked()
        {
            LogIfEnabled("Endless Runner game selected");
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new GameSelectionEvent { GameType = "EndlessRunner" });
            }
        }

        private void OnSettingsClicked()
        {
            LogIfEnabled("Settings panel requested");
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new UINavigationEvent { PanelId = "SettingsPanel", Action = UINavigationAction.Push });
            }
        }

        private void OnAchievementsClicked()
        {
            LogIfEnabled("Achievements panel requested");
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new UINavigationEvent { PanelId = "AchievementsPanel", Action = UINavigationAction.Push });
            }
        }

        private void OnExitClicked()
        {
            LogIfEnabled("Exit game requested");
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new ApplicationEvent { Action = ApplicationAction.Quit });
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
            LogIfEnabled("Main menu is showing");
            
            // Could trigger background music, analytics events, etc.
            if (_eventBus != null)
            {
                _eventBus.Publish(new UIStateEvent { PanelId = "MainMenu", State = UIState.Showing });
            }
        }

        protected override void OnPanelShowCompleted()
        {
            base.OnPanelShowCompleted();
            LogIfEnabled("Main menu is now visible");
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new UIStateEvent { PanelId = "MainMenu", State = UIState.Visible });
            }
        }

        protected override void OnPanelHideStarted()
        {
            base.OnPanelHideStarted();
            LogIfEnabled("Main menu is hiding");
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new UIStateEvent { PanelId = "MainMenu", State = UIState.Hiding });
            }
        }

        protected override void OnPanelHideCompleted()
        {
            base.OnPanelHideCompleted();
            LogIfEnabled("Main menu is now hidden");
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new UIStateEvent { PanelId = "MainMenu", State = UIState.Hidden });
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Enables or disables the play buttons
        /// </summary>
        /// <param name="enabled">Whether the play buttons should be enabled</param>
        public void SetPlayButtonsEnabled(bool enabled)
        {
            if (_playMatch3Button != null)
                _playMatch3Button.interactable = enabled;
                
            if (_playEndlessRunnerButton != null)
                _playEndlessRunnerButton.interactable = enabled;
        }

        /// <summary>
        /// Updates the UI state based on available games or features
        /// </summary>
        /// <param name="match3Available">Whether Match3 game is available</param>
        /// <param name="endlessRunnerAvailable">Whether Endless Runner game is available</param>
        public void UpdateGameAvailability(bool match3Available, bool endlessRunnerAvailable)
        {
            if (_playMatch3Button != null)
            {
                _playMatch3Button.gameObject.SetActive(match3Available);
            }

            if (_playEndlessRunnerButton != null)
            {
                _playEndlessRunnerButton.gameObject.SetActive(endlessRunnerAvailable);
            }
        }

        #endregion

        #region Private Methods

        private void LogIfEnabled(string message)
        {
            Debug.Log($"[MainMenuPanel] {message}", this);
        }

        #endregion
    }

    #region Event Classes

    /// <summary>
    /// Event for game selection from main menu
    /// </summary>
    public class GameSelectionEvent
    {
        public string GameType { get; set; }
        public System.Collections.Generic.Dictionary<string, object> Parameters { get; set; } = new();
    }

    /// <summary>
    /// Event for UI navigation requests
    /// </summary>
    public class UINavigationEvent
    {
        public string PanelId { get; set; }
        public UINavigationAction Action { get; set; }
        public System.Collections.Generic.Dictionary<string, object> Parameters { get; set; } = new();
    }

    /// <summary>
    /// Navigation actions for UI events
    /// </summary>
    public enum UINavigationAction
    {
        Push,
        Pop,
        Replace,
        Show,
        Hide
    }

    /// <summary>
    /// Event for UI state changes
    /// </summary>
    public class UIStateEvent
    {
        public string PanelId { get; set; }
        public UIState State { get; set; }
        public System.Collections.Generic.Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// UI panel states
    /// </summary>
    public enum UIState
    {
        Hidden,
        Showing,
        Visible,
        Hiding
    }

    /// <summary>
    /// Event for application-level actions
    /// </summary>
    public class ApplicationEvent
    {
        public ApplicationAction Action { get; set; }
        public System.Collections.Generic.Dictionary<string, object> Parameters { get; set; } = new();
    }

    /// <summary>
    /// Application-level actions
    /// </summary>
    public enum ApplicationAction
    {
        Quit,
        Pause,
        Resume,
        Minimize,
        FocusLost,
        FocusGained
    }

    #endregion
} 