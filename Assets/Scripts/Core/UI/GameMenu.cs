using UnityEngine;
using UnityEngine.UI;
using Core.Scene;
using UnityEngine.SceneManagement;
using Core.Architecture;
using Core.DI;
using TMPro; // Add TextMeshPro using statement
using Core.Common.ScoringManagement; // Add scoring management using statement
using MiniGameFramework.Core.Bootstrap; // Add MiniGameLoader using statement

namespace Core.UI
{
    public class GameMenu : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button backButton;
        
        [Header("Pause Menu Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button nextGameButton;
        
        [Header("Game Over Panel Buttons")]
        [SerializeField] private Button gameOverRestartButton;
        [SerializeField] private Button gameOverMainMenuButton;
        [SerializeField] private Button gameOverNextGameButton;

        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI scoreText; // Score text component
        [SerializeField] private string scoreFormat = "Score: {0}"; // Score display format

        [Header("Menu Configuration")]
        [SerializeField] private bool _showPauseButton = true;
        [SerializeField] private bool _showBackButton = true;
        [SerializeField] private bool _enableEscapeKey = true;

        // Core dependencies
        private IEventBus _eventBus;
        private SceneController _sceneController;
        private bool isPaused = false;
        private int _currentScore = 0; // Current score value

        void Awake()
        {
            InitializeDependencies();
        }

        void Start()
        {
            SetupButtons();
            SetupPauseMenu();
            SetupGameOverPanel();
            SetupScoreDisplay();
            
            // Diğer sahnelerde GameMenu'nun doğru çalışması için sahneye özel ayarlar
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                pauseButton.gameObject.SetActive(false);
                backButton.gameObject.SetActive(false);
            }
        }

        private void InitializeDependencies()
        {
            // Get EventBus from ServiceLocator
            _eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            
            // Get SceneController from ServiceLocator
            _sceneController = ServiceLocator.Instance.Resolve<SceneController>();
            
            // Register self with ServiceLocator
            ServiceLocator.Instance.Register<GameMenu>(this);
            
            Debug.Log("[GameMenu] Initialized with ServiceLocator");
        }

        private void SetupButtons()
        {
            Debug.Log("[GameMenu] 🔧 Setting up buttons...");
            
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(TogglePause);
                pauseButton.gameObject.SetActive(_showPauseButton);
                Debug.Log($"[GameMenu] ✅ Pause button setup: {_showPauseButton}");
            }
            else
            {
                Debug.LogWarning("[GameMenu] ⚠️ Pause button is null!");
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(GoToMainMenu);
                backButton.gameObject.SetActive(_showBackButton);
                Debug.Log($"[GameMenu] ✅ Back button setup: {_showBackButton}");
            }
            else
            {
                Debug.LogWarning("[GameMenu] ⚠️ Back button is null!");
            }
            
            // Check EventSystem
            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                Debug.LogError("[GameMenu] ❌ No EventSystem found in scene!");
            }
            else
            {
                Debug.Log("[GameMenu] ✅ EventSystem found");
            }
        }

        private void SetupPauseMenu()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(ResumeGame);
                
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);
                
            if (nextGameButton != null)
                nextGameButton.onClick.AddListener(GoToNextGame);
                
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }
        
        private void SetupGameOverPanel()
        {
            if (gameOverRestartButton != null)
                gameOverRestartButton.onClick.AddListener(RestartGame);
                
            if (gameOverMainMenuButton != null)
                gameOverMainMenuButton.onClick.AddListener(GoToMainMenu);
                
            if (gameOverNextGameButton != null)
                gameOverNextGameButton.onClick.AddListener(GoToNextGame);
                
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        private void SetupScoreDisplay()
        {
            // Get EventBus from ServiceLocator and subscribe to score change events
            var eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            if (eventBus != null)
            {
                eventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
                Debug.Log("[GameMenu] ✅ Subscribed to ScoreChangedEvent via ServiceLocator");
            }
            else
            {
                Debug.LogWarning("[GameMenu] ⚠️ EventBus not found in ServiceLocator");
            }
            
            // Initialize score display
            UpdateScoreDisplay();
        }

        /// <summary>
        /// Handle score change events from the scoring system
        /// </summary>
        /// <param name="scoreEvent">Score change event data</param>
        private void OnScoreChanged(ScoreChangedEvent scoreEvent)
        {
            _currentScore = scoreEvent.NewScore;
            UpdateScoreDisplay();
            
            Debug.Log($"[GameMenu] 📊 Score updated: {_currentScore} (+{scoreEvent.ScoreDelta})");
        }

        /// <summary>
        /// Update the score text display
        /// </summary>
        private void UpdateScoreDisplay()
        {
            if (scoreText != null)
            {
                scoreText.text = string.Format(scoreFormat, _currentScore);
            }
        }

        /// <summary>
        /// Set the current score manually (for testing or direct updates)
        /// </summary>
        /// <param name="score">New score value</param>
        public void SetScore(int score)
        {
            _currentScore = score;
            UpdateScoreDisplay();
        }

        /// <summary>
        /// Get the current score value
        /// </summary>
        /// <returns>Current score</returns>
        public int GetCurrentScore()
        {
            return _currentScore;
        }

        void Update()
        {
            if (_enableEscapeKey && Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        private void TogglePause()
        {
            isPaused = !isPaused;
            
            if (pausePanel != null)
            {
                pausePanel.SetActive(isPaused);
            }
            
            Time.timeScale = isPaused ? 0 : 1; // Oyun zamanını duraklat/yeniden başlat
            
            // Publish pause event
            SafePublish(new GamePauseEvent { IsPaused = isPaused });
            
            Debug.Log($"[GameMenu] Game {(isPaused ? "paused" : "resumed")}");
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1; // Zamanı normale döndür
            
            // Publish menu navigation event
            SafePublish(new MenuNavigationEvent { TargetScene = "MainMenu" });
            
            if (_sceneController != null)
            {
                _sceneController.LoadScene("MainMenu");
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

        // Pause panelindeki butonlar için public metotlar
        public void ResumeGame()
        {
            TogglePause();
        }

        public void RestartGame()
        {
            Time.timeScale = 1;
            
            string currentScene = SceneManager.GetActiveScene().name;
            
            // Publish restart event
            SafePublish(new GameRestartEvent { SceneName = currentScene });
            
            if (_sceneController != null)
            {
                _sceneController.LoadScene(currentScene);
            }
            else
            {
                SceneManager.LoadScene(currentScene);
            }
        }

        public void GoToNextGame()
        {
            Time.timeScale = 1;
            string nextGameId = (SceneManager.GetActiveScene().name == "Match3") ? "EndlessRunner" : "Match3";
            
            // Publish next game event
            SafePublish(new GameNavigationEvent { NextScene = nextGameId });
            
            // Use MiniGameLoader to properly initialize the next game
            Debug.Log($"[GameMenu] 🎮 Loading next game via MiniGameLoader: {nextGameId}");
            MiniGameLoader.LoadGame(nextGameId);
        }
        
        /// <summary>
        /// Show game over panel
        /// </summary>
        public void ShowGameOverPanel()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                Debug.Log("[GameMenu] 💀 Game over panel shown");
            }
            else
            {
                Debug.LogWarning("[GameMenu] ⚠️ Game over panel is null!");
            }
        }
        
        /// <summary>
        /// Hide game over panel
        /// </summary>
        public void HideGameOverPanel()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
                Debug.Log("[GameMenu] ✅ Game over panel hidden");
            }
        }

        /// <summary>
        /// Safely publishes an event if EventBus is available
        /// </summary>
        private void SafePublish<T>(T eventData) where T : class
        {
            if (_eventBus != null)
            {
                _eventBus.Publish(eventData);
            }
            else
            {
                Debug.LogWarning($"[GameMenu] Event not published - EventBus not available: {typeof(T).Name}");
            }
        }

        void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            var eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            if (eventBus != null)
            {
                eventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
                Debug.Log("[GameMenu] 🗑️ Unsubscribed from ScoreChangedEvent via ServiceLocator");
            }
        }
    }

    #region Game Menu Events

    /// <summary>
    /// Event published when game pause state changes
    /// </summary>
    public class GamePauseEvent : IEvent
    {
        public bool IsPaused { get; set; }
    }

    /// <summary>
    /// Event published when navigating to menu
    /// </summary>
    public class MenuNavigationEvent : IEvent
    {
        public string TargetScene { get; set; }
    }

    /// <summary>
    /// Event published when restarting game
    /// </summary>
    public class GameRestartEvent : IEvent
    {
        public string SceneName { get; set; }
    }

    /// <summary>
    /// Event published when navigating to next game
    /// </summary>
    public class GameNavigationEvent : IEvent
    {
        public string NextScene { get; set; }
    }

    #endregion
}