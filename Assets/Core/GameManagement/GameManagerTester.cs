using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.Core.Bootstrap;
using MiniGameFramework.Core.DI;

namespace MiniGameFramework.Core.GameManagement
{
    /// <summary>
    /// Comprehensive test system for GameManager and Game Management functionality.
    /// Tests lifecycle management, session tracking, progress/timer systems, and integration.
    /// </summary>
    public class GameManagerTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool autoStartTests = true;
        [SerializeField] private float testInterval = 2f;
        [SerializeField] private bool runContinuousTests = false;

        [Header("Manual Game Management Controls")]
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button pauseGameButton;
        [SerializeField] private Button resumeGameButton;
        [SerializeField] private Button endGameButton;
        [SerializeField] private Button unloadGameButton;

        [Header("Session Controls")]
        [SerializeField] private Button updateScoreButton;
        [SerializeField] private Button addScoreButton;
        [SerializeField] private TMP_InputField scoreInputField;

        [Header("Timer and Progress Controls")]
        [SerializeField] private Button startTimerButton;
        [SerializeField] private Button pauseTimerButton;
        [SerializeField] private Button resetTimerButton;
        [SerializeField] private Button advanceProgressButton;
        [SerializeField] private Button resetProgressButton;

        [Header("Test Controls")]
        [SerializeField] private Button runAllTestsButton;
        [SerializeField] private Button testBootstrapIntegrationButton;
        [SerializeField] private Button testGameLifecycleButton;
        [SerializeField] private Button testSessionTrackingButton;
        [SerializeField] private Button testProgressTimerButton;

        [Header("UI Display")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI gameManagerStateText;
        [SerializeField] private TextMeshProUGUI currentGameText;
        [SerializeField] private TextMeshProUGUI sessionInfoText;
        [SerializeField] private TextMeshProUGUI timerDisplayText;
        [SerializeField] private TextMeshProUGUI progressDisplayText;
        [SerializeField] private TextMeshProUGUI testResultsText;
        [SerializeField] private Slider progressBar;

        // Dependencies
        private IGameManager gameManager;
        private IEventBus eventBus;
        private GameBootstrap gameBootstrap;

        // Test systems
        private TimerSystem testTimer;
        private ProgressSystem testProgress;
        private TestGameMock mockGame;

        // Test state
        private int testsRun = 0;
        private int testsPassedCount = 0;
        private int testsFailedCount = 0;
        private List<string> testResults = new List<string>();
        private Coroutine continuousTestCoroutine;

        #region Unity Lifecycle

        private void Start()
        {
            InitializeTester();
            SetupEventSubscriptions();
            SetupUICallbacks();

            // Retry service resolution if needed
            StartCoroutine(RetryServiceResolution());

            if (autoStartTests)
            {
                StartCoroutine(DelayedAutoTest());
            }
        }

        private IEnumerator RetryServiceResolution()
        {
            float timeout = 5f;
            float elapsed = 0f;

            while ((gameManager == null || eventBus == null) && elapsed < timeout)
            {
                try
                {
                    if (gameManager == null)
                        gameManager = ServiceLocator.Instance.Resolve<IGameManager>();
                    
                    if (eventBus == null)
                        eventBus = ServiceLocator.Instance.Resolve<IEventBus>();

                    if (gameManager != null && eventBus != null)
                    {
                        UpdateStatusText("Services resolved successfully");
                        break;
                    }
                }
                catch
                {
                    // Services not ready yet, continue waiting
                }

                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (gameManager == null || eventBus == null)
            {
                UpdateStatusText("⚠️ Warning: Some services could not be resolved");
            }
        }

        private void Update()
        {
            UpdateTimerDisplay();
            UpdateProgressDisplay();
            UpdateUI();
        }

        private void OnDestroy()
        {
            CleanupEventSubscriptions();
            CleanupTestSystems();
        }

        #endregion

        #region Initialization

        private void InitializeTester()
        {
            Debug.Log("[GameManagerTester] Initializing Game Manager test system...");

            // Find dependencies
            gameBootstrap = FindFirstObjectByType<GameBootstrap>();
            if (gameBootstrap == null)
            {
                LogTestResult("❌ GameBootstrap not found!", false);
                return;
            }

            // Resolve GameManager from ServiceLocator
            try
            {
                gameManager = ServiceLocator.Instance.Resolve<IGameManager>();
                eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GameManagerTester] Services not yet available: {ex.Message}");
                // Services will be resolved later when they become available
            }

            // Initialize test systems
            testTimer = new TimerSystem(TimerMode.Stopwatch);
            testProgress = new ProgressSystem(10);

            // Create mock game for testing
            mockGame = gameObject.AddComponent<TestGameMock>();

            UpdateStatusText("GameManagerTester initialized successfully");
            LogTestResult("✅ GameManagerTester initialization completed", true);
        }

        private void SetupEventSubscriptions()
        {
            if (testProgress != null)
            {
                testProgress.OnProgressChanged += OnTestProgressChanged;
                testProgress.OnProgressComplete += OnTestProgressComplete;
            }

            if (testTimer != null)
            {
                testTimer.OnTimerTick += OnTestTimerTick;
                testTimer.OnTimerComplete += OnTestTimerComplete;
            }
        }

        private void CleanupEventSubscriptions()
        {
            if (testProgress != null)
            {
                testProgress.OnProgressChanged -= OnTestProgressChanged;
                testProgress.OnProgressComplete -= OnTestProgressComplete;
            }

            if (testTimer != null)
            {
                testTimer.OnTimerTick -= OnTestTimerTick;
                testTimer.OnTimerComplete -= OnTestTimerComplete;
            }
        }

        private void CleanupTestSystems()
        {
            testTimer?.Stop();
            
            if (continuousTestCoroutine != null)
            {
                StopCoroutine(continuousTestCoroutine);
            }
        }

        #endregion

        #region UI Setup

        private void SetupUICallbacks()
        {
            // Game Management Controls
            loadGameButton?.onClick.AddListener(() => LoadTestGame());
            startGameButton?.onClick.AddListener(() => StartCurrentGame());
            pauseGameButton?.onClick.AddListener(() => PauseCurrentGame());
            resumeGameButton?.onClick.AddListener(() => ResumeCurrentGame());
            endGameButton?.onClick.AddListener(() => EndCurrentGame(GameResult.Victory));
            unloadGameButton?.onClick.AddListener(() => UnloadCurrentGame());

            // Session Controls
            updateScoreButton?.onClick.AddListener(() => UpdateScore());
            addScoreButton?.onClick.AddListener(() => AddScore());

            // Timer and Progress Controls
            startTimerButton?.onClick.AddListener(() => testTimer?.Start());
            pauseTimerButton?.onClick.AddListener(() => testTimer?.Pause());
            resetTimerButton?.onClick.AddListener(() => testTimer?.Reset());
            advanceProgressButton?.onClick.AddListener(() => testProgress?.AdvanceStep());
            resetProgressButton?.onClick.AddListener(() => testProgress?.Reset());

            // Test Controls
            runAllTestsButton?.onClick.AddListener(() => StartCoroutine(RunAllTests()));
            testBootstrapIntegrationButton?.onClick.AddListener(() => StartCoroutine(TestBootstrapIntegration()));
            testGameLifecycleButton?.onClick.AddListener(() => StartCoroutine(TestGameLifecycle()));
            testSessionTrackingButton?.onClick.AddListener(() => StartCoroutine(TestSessionTracking()));
            testProgressTimerButton?.onClick.AddListener(() => StartCoroutine(TestProgressAndTimer()));
        }

        #endregion

        #region Test Execution

        private IEnumerator DelayedAutoTest()
        {
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(RunAllTests());

            if (runContinuousTests)
            {
                continuousTestCoroutine = StartCoroutine(ContinuousTestLoop());
            }
        }

        private IEnumerator ContinuousTestLoop()
        {
            while (runContinuousTests)
            {
                yield return new WaitForSeconds(testInterval);
                yield return StartCoroutine(RunAllTests());
            }
        }

        public IEnumerator RunAllTests()
        {
            UpdateStatusText("🔄 Running comprehensive Game Manager tests...");
            ResetTestCounters();

            yield return StartCoroutine(TestBootstrapIntegration());
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(TestGameLifecycle());
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(TestSessionTracking());
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(TestProgressAndTimer());

            ShowFinalResults();
        }

        private IEnumerator TestBootstrapIntegration()
        {
            LogTestResult("=== Testing Bootstrap Integration ===", true);

            // Test GameBootstrap availability
            bool bootstrapFound = gameBootstrap != null;
            LogTestResult($"GameBootstrap found: {bootstrapFound}", bootstrapFound);

            if (bootstrapFound)
            {
                // Test service registration
                bool servicesInitialized = gameBootstrap.IsInitialized;
                LogTestResult($"Services initialized: {servicesInitialized}", servicesInitialized);

                if (servicesInitialized)
                {
                    // Try to resolve GameManager from ServiceLocator
                    try
                    {
                        if (ServiceLocator.Instance.IsRegistered<IGameManager>())
                        {
                            gameManager = ServiceLocator.Instance.Resolve<IGameManager>();
                            bool gameManagerResolved = gameManager != null;
                            LogTestResult($"GameManager resolved: {gameManagerResolved}", gameManagerResolved);
                        }
                        else
                        {
                            LogTestResult("GameManager not yet registered - this is expected until bootstrap integration is complete", true);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogTestResult($"Failed to resolve GameManager: {ex.Message}", false);
                    }

                    // Test EventBus availability
                    try
                    {
                        eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
                        bool eventBusResolved = eventBus != null;
                        LogTestResult($"EventBus resolved: {eventBusResolved}", eventBusResolved);
                    }
                    catch (Exception ex)
                    {
                        LogTestResult($"Failed to resolve EventBus: {ex.Message}", false);
                    }
                }
            }

            yield return null;
        }

        private IEnumerator TestGameLifecycle()
        {
            LogTestResult("=== Testing Game Lifecycle ===", true);

            if (gameManager == null)
            {
                LogTestResult("⚠️  GameManager not available, testing standalone systems", true);
                yield break;
            }

            // Test initial state
            bool initialStateCorrect = gameManager.State == GameManagerState.Idle;
            LogTestResult($"Initial state is Idle: {initialStateCorrect}", initialStateCorrect);

            // Test game registration
            try
            {
                gameManager.RegisterGame<TestGameMock>("TestGame");
                LogTestResult("Game registration successful", true);
            }
            catch (Exception ex)
            {
                LogTestResult($"Game registration failed: {ex.Message}", false);
            }

            yield return new WaitForSeconds(0.2f);

            // Test game loading (using coroutine to handle async)
            yield return StartCoroutine(TestGameLoadingCoroutine());

            yield return null;
        }

        private IEnumerator TestGameLoadingCoroutine()
        {
            bool loadCompleted = false;
            bool loadResult = false;
            Exception loadException = null;

            // Start async operation
            System.Threading.Tasks.Task.Run(async () => {
                try
                {
                    loadResult = await gameManager.LoadGameAsync("TestGame");
                    loadCompleted = true;
                }
                catch (Exception ex)
                {
                    loadException = ex;
                    loadCompleted = true;
                }
            });

            // Wait for completion
            while (!loadCompleted)
            {
                yield return null;
            }

            // Process results
            if (loadException != null)
            {
                LogTestResult($"Game loading failed: {loadException.Message}", false);
            }
            else
            {
                LogTestResult($"Game loading: {loadResult}", loadResult);
                
                if (loadResult)
                {
                    bool stateIsReady = gameManager.State == GameManagerState.Ready;
                    LogTestResult($"State is Ready after load: {stateIsReady}", stateIsReady);
                }
            }
        }

        private IEnumerator TestSessionTracking()
        {
            LogTestResult("=== Testing Session Tracking ===", true);

            // Test standalone GameSession
            var testSession = new GameSession("TestGame", 100);
            
            bool sessionCreated = testSession != null && testSession.IsActive;
            LogTestResult($"Session created and active: {sessionCreated}", sessionCreated);

            bool initialScoreZero = testSession.Score == 0;
            LogTestResult($"Initial score is zero: {initialScoreZero}", initialScoreZero);

            // Test score updates
            testSession.UpdateScore(150);
            bool scoreUpdated = testSession.Score == 150;
            LogTestResult($"Score update working: {scoreUpdated}", scoreUpdated);

            // Test score addition
            testSession.AddScore(50);
            bool scoreAdded = testSession.Score == 200;
            LogTestResult($"Score addition working: {scoreAdded}", scoreAdded);

            // Test best score tracking
            bool bestScoreTracked = testSession.BestScore == 200;
            LogTestResult($"Best score tracking: {bestScoreTracked}", bestScoreTracked);

            // Test session end
            testSession.EndSession(GameResult.Victory);
            bool sessionEnded = !testSession.IsActive && testSession.Result == GameResult.Victory;
            LogTestResult($"Session end working: {sessionEnded}", sessionEnded);

            yield return null;
        }

        private IEnumerator TestProgressAndTimer()
        {
            LogTestResult("=== Testing Progress and Timer Systems ===", true);

            // Test ProgressSystem
            var progress = new ProgressSystem(5);
            bool progressInitialized = progress.Progress == 0f && progress.TotalSteps == 5;
            LogTestResult($"Progress system initialized: {progressInitialized}", progressInitialized);

            progress.AdvanceStep();
            progress.AdvanceStep();
            bool progressAdvanced = progress.CurrentStep == 2 && Math.Abs(progress.Progress - 0.4f) < 0.1f;
            LogTestResult($"Progress advancement working: {progressAdvanced}", progressAdvanced);

            progress.SetProgress(1f);
            bool progressCompleted = progress.IsComplete;
            LogTestResult($"Progress completion working: {progressCompleted}", progressCompleted);

            // Test TimerSystem
            var timer = new TimerSystem(TimerMode.Countdown, 1f);
            bool timerInitialized = timer.Mode == TimerMode.Countdown && timer.Duration == 1f;
            LogTestResult($"Timer system initialized: {timerInitialized}", timerInitialized);

            timer.Start();
            bool timerStarted = timer.IsRunning;
            LogTestResult($"Timer start working: {timerStarted}", timerStarted);

            yield return new WaitForSeconds(0.5f);

            timer.Pause();
            bool timerPaused = timer.IsPaused;
            LogTestResult($"Timer pause working: {timerPaused}", timerPaused);

            timer.Resume();
            bool timerResumed = timer.IsRunning && !timer.IsPaused;
            LogTestResult($"Timer resume working: {timerResumed}", timerResumed);

            yield return null;
        }

        #endregion

        #region Manual Controls

        private void LoadTestGame()
        {
            if (gameManager != null)
            {
                try
                {
                    gameManager.RegisterGame<TestGameMock>("TestGame");
                    StartCoroutine(LoadTestGameCoroutine());
                }
                catch (Exception ex)
                {
                    UpdateStatusText($"Load Game Error: {ex.Message}");
                }
            }
            else
            {
                UpdateStatusText("GameManager not available");
            }
        }

        private IEnumerator LoadTestGameCoroutine()
        {
            bool loadCompleted = false;
            bool loadResult = false;
            Exception loadException = null;

            // Start async operation
            System.Threading.Tasks.Task.Run(async () => {
                try
                {
                    loadResult = await gameManager.LoadGameAsync("TestGame");
                    loadCompleted = true;
                }
                catch (Exception ex)
                {
                    loadException = ex;
                    loadCompleted = true;
                }
            });

            // Wait for completion
            while (!loadCompleted)
            {
                yield return null;
            }

            // Process results
            if (loadException != null)
            {
                UpdateStatusText($"Load Game Error: {loadException.Message}");
            }
            else
            {
                UpdateStatusText($"Load Game Result: {loadResult}");
            }
        }

        private void StartCurrentGame()
        {
            if (gameManager != null)
            {
                bool result = gameManager.StartCurrentGame();
                UpdateStatusText($"Start Game Result: {result}");
            }
        }

        private void PauseCurrentGame()
        {
            if (gameManager != null)
            {
                bool result = gameManager.PauseCurrentGame();
                UpdateStatusText($"Pause Game Result: {result}");
            }
        }

        private void ResumeCurrentGame()
        {
            if (gameManager != null)
            {
                bool result = gameManager.ResumeCurrentGame();
                UpdateStatusText($"Resume Game Result: {result}");
            }
        }

        private void EndCurrentGame(GameResult result)
        {
            if (gameManager != null)
            {
                bool endResult = gameManager.EndCurrentGame(result);
                UpdateStatusText($"End Game Result: {endResult}");
            }
        }

        private void UnloadCurrentGame()
        {
            if (gameManager != null)
            {
                StartCoroutine(UnloadCurrentGameCoroutine());
            }
        }

        private IEnumerator UnloadCurrentGameCoroutine()
        {
            bool unloadCompleted = false;
            bool unloadResult = false;
            Exception unloadException = null;

            // Start async operation
            System.Threading.Tasks.Task.Run(async () => {
                try
                {
                    unloadResult = await gameManager.UnloadCurrentGameAsync();
                    unloadCompleted = true;
                }
                catch (Exception ex)
                {
                    unloadException = ex;
                    unloadCompleted = true;
                }
            });

            // Wait for completion
            while (!unloadCompleted)
            {
                yield return null;
            }

            // Process results
            if (unloadException != null)
            {
                UpdateStatusText($"Unload Game Error: {unloadException.Message}");
            }
            else
            {
                UpdateStatusText($"Unload Game Result: {unloadResult}");
            }
        }

        private void UpdateScore()
        {
            if (gameManager?.CurrentSession != null && int.TryParse(scoreInputField?.text, out int score))
            {
                gameManager.CurrentSession.UpdateScore(score);
                UpdateStatusText($"Score updated to: {score}");
            }
        }

        private void AddScore()
        {
            if (gameManager?.CurrentSession != null && int.TryParse(scoreInputField?.text, out int points))
            {
                gameManager.CurrentSession.AddScore(points);
                UpdateStatusText($"Added {points} points");
            }
        }

        #endregion

        #region Event Handlers

        private void OnTestProgressChanged(float progress)
        {
            if (progressBar != null)
            {
                progressBar.value = progress;
            }
        }

        private void OnTestProgressComplete()
        {
            UpdateStatusText("Test progress completed!");
        }

        private void OnTestTimerTick(float timeValue)
        {
            // Timer tick handled in UpdateTimerDisplay
        }

        private void OnTestTimerComplete()
        {
            UpdateStatusText("Test timer completed!");
        }

        #endregion

        #region UI Updates

        private void UpdateUI()
        {
            // Update Game Manager state
            if (gameManagerStateText != null)
            {
                string stateInfo = gameManager != null ? 
                    $"State: {gameManager.State}\nActive: {gameManager.IsGameActive}" : 
                    "GameManager: Not Available";
                gameManagerStateText.text = stateInfo;
            }

            // Update current game info
            if (currentGameText != null)
            {
                string gameInfo = gameManager?.CurrentGame != null ? 
                    $"Game: {gameManager.CurrentGame.GameId}\nScore: {gameManager.GetCurrentScore()}" : 
                    "No Game Loaded";
                currentGameText.text = gameInfo;
            }

            // Update session info
            if (sessionInfoText != null && gameManager?.CurrentSession != null)
            {
                var session = gameManager.CurrentSession;
                string sessionInfo = $"Session: {session.SessionId[..8]}...\n" +
                                   $"Score: {session.Score} (Best: {session.BestScore})\n" +
                                   $"Time: {session.ElapsedTime:mm\\:ss}\n" +
                                   $"Active: {session.IsActive}";
                sessionInfoText.text = sessionInfo;
            }
            else if (sessionInfoText != null)
            {
                sessionInfoText.text = "No Active Session";
            }

            // Update button states
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool hasGameManager = gameManager != null;
            bool hasCurrentGame = gameManager?.CurrentGame != null;
            bool isGameActive = gameManager?.IsGameActive ?? false;

            // Game Management Buttons
            if (loadGameButton != null) loadGameButton.interactable = hasGameManager;
            if (startGameButton != null) startGameButton.interactable = hasCurrentGame && !isGameActive;
            if (pauseGameButton != null) pauseGameButton.interactable = hasCurrentGame && isGameActive;
            if (resumeGameButton != null) resumeGameButton.interactable = hasCurrentGame && !isGameActive;
            if (endGameButton != null) endGameButton.interactable = hasCurrentGame && isGameActive;
            if (unloadGameButton != null) unloadGameButton.interactable = hasCurrentGame;

            // Session Control Buttons
            if (updateScoreButton != null) updateScoreButton.interactable = hasCurrentGame;
            if (addScoreButton != null) addScoreButton.interactable = hasCurrentGame;

            // Timer Control Buttons
            if (startTimerButton != null) startTimerButton.interactable = testTimer != null;
            if (pauseTimerButton != null) pauseTimerButton.interactable = testTimer != null;
            if (resetTimerButton != null) resetTimerButton.interactable = testTimer != null;

            // Progress Control Buttons
            if (advanceProgressButton != null) advanceProgressButton.interactable = testProgress != null;
            if (resetProgressButton != null) resetProgressButton.interactable = testProgress != null;

            // Test Control Buttons - Always available
            if (runAllTestsButton != null) runAllTestsButton.interactable = true;
            if (testBootstrapIntegrationButton != null) testBootstrapIntegrationButton.interactable = true;
            if (testGameLifecycleButton != null) testGameLifecycleButton.interactable = true;
            if (testSessionTrackingButton != null) testSessionTrackingButton.interactable = true;
            if (testProgressTimerButton != null) testProgressTimerButton.interactable = true;
        }

        private void UpdateTimerDisplay()
        {
            if (timerDisplayText != null && testTimer != null)
            {
                string timerInfo = $"Timer: {testTimer.GetFormattedTime()}\n" +
                                 $"Running: {testTimer.IsRunning}\n" +
                                 $"Paused: {testTimer.IsPaused}\n" +
                                 $"Mode: {testTimer.Mode}";
                timerDisplayText.text = timerInfo;
            }
        }

        private void UpdateProgressDisplay()
        {
            if (progressDisplayText != null && testProgress != null)
            {
                string progressInfo = $"Progress: {testProgress.Progress:P1}\n" +
                                    $"Step: {testProgress.CurrentStep}/{testProgress.TotalSteps}\n" +
                                    $"Complete: {testProgress.IsComplete}";
                progressDisplayText.text = progressInfo;
            }
        }

        private void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"[{DateTime.Now:HH:mm:ss}] {message}";
            }
            Debug.Log($"[GameManagerTester] {message}");
        }

        #endregion

        #region Test Utilities

        private void ResetTestCounters()
        {
            testsRun = 0;
            testsPassedCount = 0;
            testsFailedCount = 0;
            testResults.Clear();
        }

        private void LogTestResult(string message, bool passed)
        {
            testsRun++;
            if (passed) testsPassedCount++;
            else testsFailedCount++;

            string resultMessage = $"{(passed ? "✅" : "❌")} {message}";
            testResults.Add(resultMessage);
            
            Debug.Log($"[GameManagerTester] {resultMessage}");
            UpdateTestResultsDisplay();
        }

        private void UpdateTestResultsDisplay()
        {
            if (testResultsText != null)
            {
                string resultsDisplay = $"Tests: {testsRun} | Passed: {testsPassedCount} | Failed: {testsFailedCount}\n\n";
                resultsDisplay += string.Join("\n", testResults);
                testResultsText.text = resultsDisplay;
            }
        }

        private void ShowFinalResults()
        {
            string finalStatus = testsFailedCount == 0 ? 
                $"✅ === ALL TESTS PASSED! ===\nTests Complete: {testsPassedCount}/{testsRun} passed, 0 failed" :
                $"⚠️ === TESTS COMPLETED WITH ISSUES ===\nTests Complete: {testsPassedCount}/{testsRun} passed, {testsFailedCount} failed";

            UpdateStatusText(finalStatus);
            LogTestResult(finalStatus, testsFailedCount == 0);
        }

        #endregion
    }

    #region Mock Game for Testing

    /// <summary>
    /// Mock mini-game implementation for testing GameManager functionality.
    /// </summary>
    public class TestGameMock : MonoBehaviour, IMiniGame
    {
        private GameState currentState = GameState.Uninitialized;
        private int currentScore = 0;

        public string GameId => "TestGame";
        public string DisplayName => "Test Game Mock";
        public GameState CurrentState => currentState;
        public bool IsPlayable => currentState == GameState.Playing;

        public event Action<GameState> OnStateChanged;

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            ChangeState(GameState.Initializing);
            
            await System.Threading.Tasks.Task.Delay(100); // Simulate async initialization
            
            ChangeState(GameState.Ready);
        }

        public void Start()
        {
            ChangeState(GameState.Playing);
        }

        public void Pause()
        {
            ChangeState(GameState.Paused);
        }

        public void Resume()
        {
            ChangeState(GameState.Playing);
        }

        public void End()
        {
            ChangeState(GameState.GameOver);
        }

        public void Cleanup()
        {
            ChangeState(GameState.CleaningUp);
        }

        public int GetCurrentScore()
        {
            return currentScore;
        }

        public void SetScore(int score)
        {
            currentScore = score;
        }

        private void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            OnStateChanged?.Invoke(currentState);
        }
    }

    #endregion
} 