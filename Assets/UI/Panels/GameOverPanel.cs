using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MiniGameFramework.Core.Architecture;

namespace MiniGameFramework.UI.Panels
{
    /// <summary>
    /// Game over panel that displays final score, achievements, and options to restart or return to menu.
    /// Supports different game over scenarios (win, lose, timeout, etc.)
    /// </summary>
    public class GameOverPanel : UIPanel
    {
        [Header("Game Over UI Elements")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private TextMeshProUGUI _newHighScoreText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _shareButton;
        [SerializeField] private Button _leaderboardButton;

        [Header("Visual Elements")]
        [SerializeField] private GameObject _newHighScoreContainer;
        [SerializeField] private GameObject _achievementContainer;
        [SerializeField] private Image _backgroundTint;
        [SerializeField] private ParticleSystem _celebrationParticles;

        [Header("Game Over Configuration")]
        [SerializeField] private Color _winColor = Color.green;
        [SerializeField] private Color _loseColor = Color.red;
        [SerializeField] private Color _neutralColor = Color.blue;
        [SerializeField] private bool _showShareButton = true;
        [SerializeField] private bool _showLeaderboardButton = true;
        [SerializeField] private float _scoreCountUpDuration = 2f;

        [Header("Audio")]
        [SerializeField] private AudioClip _winSound;
        [SerializeField] private AudioClip _loseSound;
        [SerializeField] private AudioClip _newHighScoreSound;

        // Event system and dependencies
        private IEventBus _eventBus;
        private Core.SaveSystem.ISaveSystem _saveSystem;
        
        // State tracking
        private GameOverData _currentGameOverData;
        private bool _isNewHighScore = false;
        private int _displayedScore = 0;
        
        // Animation tweens
        private int _scoreCountTweenId = -1;

        #region Events

        /// <summary>
        /// Called when restart is requested
        /// </summary>
        public event System.Action OnRestartRequested;

        /// <summary>
        /// Called when main menu return is requested
        /// </summary>
        public event System.Action OnMainMenuRequested;

        /// <summary>
        /// Called when share is requested
        /// </summary>
        public event System.Action<GameOverData> OnShareRequested;

        /// <summary>
        /// Called when leaderboard is requested
        /// </summary>
        public event System.Action OnLeaderboardRequested;

        #endregion

        #region Unity Lifecycle

        protected override void OnPanelInitialized()
        {
            base.OnPanelInitialized();
            
            // Get dependencies from ServiceLocator
            _eventBus = Core.DI.ServiceLocator.Instance.Resolve<IEventBus>();
            _saveSystem = Core.DI.ServiceLocator.Instance.Resolve<Core.SaveSystem.ISaveSystem>();
            
            SetupButtonListeners();
            ConfigureUI();
        }

        protected override void OnPanelDestroyed()
        {
            base.OnPanelDestroyed();
            
            StopScoreAnimation();
            RemoveButtonListeners();
        }

        #endregion

        #region UI Setup

        private void SetupButtonListeners()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }

            if (_shareButton != null)
            {
                _shareButton.onClick.AddListener(OnShareClicked);
            }

            if (_leaderboardButton != null)
            {
                _leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
            }
        }

        private void RemoveButtonListeners()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            }

            if (_shareButton != null)
            {
                _shareButton.onClick.RemoveListener(OnShareClicked);
            }

            if (_leaderboardButton != null)
            {
                _leaderboardButton.onClick.RemoveListener(OnLeaderboardClicked);
            }
        }

        private void ConfigureUI()
        {
            // Configure button visibility
            if (_shareButton != null)
            {
                _shareButton.gameObject.SetActive(_showShareButton);
            }

            if (_leaderboardButton != null)
            {
                _leaderboardButton.gameObject.SetActive(_showLeaderboardButton);
            }

            // Hide optional elements initially
            if (_newHighScoreContainer != null)
            {
                _newHighScoreContainer.SetActive(false);
            }

            if (_achievementContainer != null)
            {
                _achievementContainer.SetActive(false);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Shows the game over panel with the specified data
        /// </summary>
        /// <param name="gameOverData">Data about the game over state</param>
        public async void ShowGameOver(GameOverData gameOverData)
        {
            _currentGameOverData = gameOverData;
            
            // Setup the UI with the game over data
            SetupGameOverUI(gameOverData);
            
            // Check for high score
            CheckAndUpdateHighScore(gameOverData);
            
            // Show the panel
            await ShowAsync();
            
            // Start score count-up animation
            StartScoreCountUp();
            
            // Play appropriate audio
            PlayGameOverAudio(gameOverData.Result);
            
            // Show particle effects if appropriate
            if (gameOverData.Result == GameResult.Victory || _isNewHighScore)
            {
                ShowCelebrationEffects();
            }
            
            LogIfEnabled($"Game over shown: {gameOverData.Result}, Score: {gameOverData.Score}");
        }

        /// <summary>
        /// Updates the displayed score and statistics
        /// </summary>
        /// <param name="score">Current score</param>
        /// <param name="statistics">Additional game statistics</param>
        public void UpdateScore(int score, System.Collections.Generic.Dictionary<string, object> statistics = null)
        {
            _currentGameOverData.Score = score;
            _currentGameOverData.Statistics = statistics ?? new System.Collections.Generic.Dictionary<string, object>();
            
            UpdateScoreDisplay(score);
        }

        #endregion

        #region Game Over Setup

        private void SetupGameOverUI(GameOverData data)
        {
            // Set title based on result
            if (_titleText != null)
            {
                _titleText.text = GetGameOverTitle(data.Result);
                _titleText.color = GetGameOverColor(data.Result);
            }

            // Set background tint
            if (_backgroundTint != null)
            {
                _backgroundTint.color = GetGameOverColor(data.Result);
            }

            // Set message
            if (_messageText != null)
            {
                _messageText.text = data.Message ?? GetDefaultMessage(data.Result);
            }

            // Initialize score display
            _displayedScore = 0;
            UpdateScoreDisplay(0);
        }

        private string GetGameOverTitle(GameResult result)
        {
            return result switch
            {
                GameResult.Victory => "Victory!",
                GameResult.Defeat => "Game Over",
                GameResult.Timeout => "Time's Up!",
                GameResult.Draw => "Draw",
                _ => "Game Over"
            };
        }

        private Color GetGameOverColor(GameResult result)
        {
            return result switch
            {
                GameResult.Victory => _winColor,
                GameResult.Defeat => _loseColor,
                GameResult.Timeout => _neutralColor,
                GameResult.Draw => _neutralColor,
                _ => _neutralColor
            };
        }

        private string GetDefaultMessage(GameResult result)
        {
            return result switch
            {
                GameResult.Victory => "Congratulations! You won!",
                GameResult.Defeat => "Better luck next time!",
                GameResult.Timeout => "You ran out of time!",
                GameResult.Draw => "It's a tie!",
                _ => "Thanks for playing!"
            };
        }

        #endregion

        #region High Score Management

        private async void CheckAndUpdateHighScore(GameOverData data)
        {
            if (string.IsNullOrEmpty(data.GameType) || _saveSystem == null)
                return;

            string highScoreKey = $"HighScore_{data.GameType}";
            int currentHighScore = await _saveSystem.LoadAsync<int>(highScoreKey, 0);
            
            _isNewHighScore = data.Score > currentHighScore;
            
            if (_isNewHighScore)
            {
                await _saveSystem.SaveAsync<int>(highScoreKey, data.Score);
                currentHighScore = data.Score;
                
                if (_newHighScoreContainer != null)
                {
                    _newHighScoreContainer.SetActive(true);
                }
                
                if (_newHighScoreText != null)
                {
                    _newHighScoreText.text = "New High Score!";
                }
                
                LogIfEnabled($"New high score achieved: {data.Score}");
            }
            
            if (_highScoreText != null)
            {
                _highScoreText.text = $"High Score: {currentHighScore:N0}";
            }
        }

        #endregion

        #region Animation Methods

        private void StartScoreCountUp()
        {
            if (_scoreText == null || _currentGameOverData == null)
                return;

            StopScoreAnimation();

            // Animate score count-up with LeanTween
            _scoreCountTweenId = LeanTween.value(gameObject, 0, _currentGameOverData.Score, _scoreCountUpDuration)
                .setEase(LeanTweenType.easeOutCubic)
                .setOnUpdate((float value) => {
                    _displayedScore = Mathf.RoundToInt(value);
                    UpdateScoreDisplay(_displayedScore);
                })
                .setOnComplete(() => {
                    _scoreCountTweenId = -1;
                    _displayedScore = _currentGameOverData.Score;
                    UpdateScoreDisplay(_displayedScore);
                })
                .id;
        }

        private void UpdateScoreDisplay(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {score:N0}";
            }
        }

        private void StopScoreAnimation()
        {
            if (_scoreCountTweenId != -1)
            {
                LeanTween.cancel(_scoreCountTweenId);
                _scoreCountTweenId = -1;
            }
        }

        private void ShowCelebrationEffects()
        {
            if (_celebrationParticles != null)
            {
                _celebrationParticles.Play();
            }
            
            // Could add additional celebration effects here
            // like screen flash, UI element bounce animations, etc.
        }

        #endregion

        #region Audio

        private void PlayGameOverAudio(GameResult result)
        {
            AudioClip clipToPlay = null;
            
            if (_isNewHighScore && _newHighScoreSound != null)
            {
                clipToPlay = _newHighScoreSound;
            }
            else
            {
                clipToPlay = result switch
                {
                    GameResult.Victory => _winSound,
                    GameResult.Defeat => _loseSound,
                    _ => null
                };
            }
            
            if (clipToPlay != null)
            {
                // Play audio through AudioSource or audio system
                // Implementation depends on your audio architecture
                LogIfEnabled($"Playing game over audio: {clipToPlay.name}");
            }
        }

        #endregion

        #region Button Event Handlers

        private void OnRestartClicked()
        {
            LogIfEnabled("Restart requested from game over");
            
            OnRestartRequested?.Invoke();
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new GameRestartEvent { Source = "GameOverPanel" });
            }
        }

        private void OnMainMenuClicked()
        {
            LogIfEnabled("Main menu requested from game over");
            
            OnMainMenuRequested?.Invoke();
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new GameExitEvent { ExitType = GameExitType.MainMenu, Source = "GameOverPanel" });
            }
        }

        private void OnShareClicked()
        {
            LogIfEnabled("Share requested from game over");
            
            OnShareRequested?.Invoke(_currentGameOverData);
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new ShareScoreEvent { GameOverData = _currentGameOverData });
            }
        }

        private void OnLeaderboardClicked()
        {
            LogIfEnabled("Leaderboard requested from game over");
            
            OnLeaderboardRequested?.Invoke();
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new ShowLeaderboardEvent { GameType = _currentGameOverData?.GameType });
            }
        }

        #endregion

        #region Panel Lifecycle Overrides

        protected override void OnPanelShowStarted()
        {
            base.OnPanelShowStarted();
            LogIfEnabled("Game over panel is showing");
        }

        protected override void OnPanelShowCompleted()
        {
            base.OnPanelShowCompleted();
            LogIfEnabled("Game over panel is now visible");
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new UIStateEvent { PanelId = "GameOverPanel", State = UIState.Visible });
            }
        }

        protected override void OnPanelHideStarted()
        {
            base.OnPanelHideStarted();
            LogIfEnabled("Game over panel is hiding");
            
            StopScoreAnimation();
        }

        protected override void OnPanelHideCompleted()
        {
            base.OnPanelHideCompleted();
            LogIfEnabled("Game over panel is now hidden");
            
            // Reset state
            _currentGameOverData = null;
            _isNewHighScore = false;
            _displayedScore = 0;
            
            if (_eventBus != null)
            {
                _eventBus.Publish(new UIStateEvent { PanelId = "GameOverPanel", State = UIState.Hidden });
            }
        }

        #endregion

        #region Private Methods

        private void LogIfEnabled(string message)
        {
            Debug.Log($"[GameOverPanel] {message}", this);
        }

        #endregion
    }

    #region Data Classes

    /// <summary>
    /// Data structure for game over information
    /// </summary>
    [System.Serializable]
    public class GameOverData
    {
        public GameResult Result { get; set; }
        public int Score { get; set; }
        public string GameType { get; set; }
        public string Message { get; set; }
        public float PlayTime { get; set; }
        public System.Collections.Generic.Dictionary<string, object> Statistics { get; set; } = new();
    }

    /// <summary>
    /// Game result types
    /// </summary>
    public enum GameResult
    {
        Victory,
        Defeat,
        Timeout,
        Draw,
        Quit
    }

    #endregion

    #region Event Classes

    /// <summary>
    /// Event for sharing scores
    /// </summary>
    public class ShareScoreEvent
    {
        public GameOverData GameOverData { get; set; }
        public string Platform { get; set; } = "Default";
    }

    /// <summary>
    /// Event for showing leaderboards
    /// </summary>
    public class ShowLeaderboardEvent
    {
        public string GameType { get; set; }
        public string Category { get; set; } = "HighScore";
    }

    #endregion
} 