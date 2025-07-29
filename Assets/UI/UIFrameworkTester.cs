using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.Core.DI;
using MiniGameFramework.Core.Bootstrap;
using MiniGameFramework.UI.Panels;

namespace MiniGameFramework.UI
{
    /// <summary>
    /// Comprehensive tester for the UI Framework system.
    /// Tests all UI panels, animations, and integration with GameBootstrap.
    /// </summary>
    public class UIFrameworkTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool _autoStartTests = true;
        [SerializeField] private float _testInterval = 2f;
        [SerializeField] private bool _logDetailedResults = true;

        [Header("UI Panels to Test")]
        [SerializeField] private MainMenuPanel _mainMenuPanel;
        [SerializeField] private LoadingPanel _loadingPanel;
        [SerializeField] private PausePanel _pausePanel;
        [SerializeField] private GameOverPanel _gameOverPanel;

        [Header("Test Controls")]
        [SerializeField] private Button _testMainMenuButton;
        [SerializeField] private Button _testLoadingButton;
        [SerializeField] private Button _testPauseButton;
        [SerializeField] private Button _testGameOverButton;
        [SerializeField] private Button _testAllPanelsButton;
        [SerializeField] private Button _testAnimationsButton;
        [SerializeField] private TextMeshProUGUI _statusText;

        // Dependencies
        private IEventBus _eventBus;
        private GameBootstrap _gameBootstrap;
        
        // Test state
        private bool _testsRunning = false;
        private int _passedTests = 0;
        private int _failedTests = 0;
        private int _totalTests = 0;

        #region Unity Lifecycle

        private void Start()
        {
            InitializeTester();
            
            if (_autoStartTests)
            {
                StartCoroutine(DelayedAutoTest());
            }
        }

        #endregion

        #region Initialization

        private void InitializeTester()
        {
            // Find GameBootstrap
            _gameBootstrap = FindFirstObjectByType<GameBootstrap>();
            if (_gameBootstrap == null)
            {
                LogError("GameBootstrap not found! Please add GameBootstrap to the scene.");
                return;
            }

            // Wait for GameBootstrap to initialize
            if (_gameBootstrap.IsInitialized)
            {
                OnBootstrapReady();
            }
            else
            {
                StartCoroutine(WaitForBootstrap());
            }

            SetupTestButtons();
            UpdateStatusText("UI Framework Tester Ready");
        }

        private IEnumerator WaitForBootstrap()
        {
            while (!_gameBootstrap.IsInitialized)
            {
                yield return null;
            }
            OnBootstrapReady();
        }

        private void OnBootstrapReady()
        {
            // Get dependencies from ServiceLocator
            _eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            
            LogSuccess("GameBootstrap initialized successfully!");
            LogSuccess($"EventBus available: {_eventBus != null}");
            
            // Find UI panels if not assigned
            FindUIPanels();
        }

        private void FindUIPanels()
        {
            if (_mainMenuPanel == null)
                _mainMenuPanel = FindFirstObjectByType<MainMenuPanel>();
            
            if (_loadingPanel == null)
                _loadingPanel = FindFirstObjectByType<LoadingPanel>();
            
            if (_pausePanel == null)
                _pausePanel = FindFirstObjectByType<PausePanel>();
            
            if (_gameOverPanel == null)
                _gameOverPanel = FindFirstObjectByType<GameOverPanel>();

            LogInfo($"Found panels - MainMenu: {_mainMenuPanel != null}, Loading: {_loadingPanel != null}, Pause: {_pausePanel != null}, GameOver: {_gameOverPanel != null}");
        }

        private void SetupTestButtons()
        {
            if (_testMainMenuButton != null)
                _testMainMenuButton.onClick.AddListener(() => StartCoroutine(TestMainMenuPanel()));
            
            if (_testLoadingButton != null)
                _testLoadingButton.onClick.AddListener(() => StartCoroutine(TestLoadingPanel()));
            
            if (_testPauseButton != null)
                _testPauseButton.onClick.AddListener(() => StartCoroutine(TestPausePanel()));
            
            if (_testGameOverButton != null)
                _testGameOverButton.onClick.AddListener(() => StartCoroutine(TestGameOverPanel()));
            
            if (_testAllPanelsButton != null)
                _testAllPanelsButton.onClick.AddListener(() => StartCoroutine(TestAllPanels()));
            
            if (_testAnimationsButton != null)
                _testAnimationsButton.onClick.AddListener(() => StartCoroutine(TestAnimations()));
        }

        #endregion

        #region Auto Testing

        private IEnumerator DelayedAutoTest()
        {
            yield return new WaitForSeconds(1f); // Wait for everything to initialize
            
            if (_gameBootstrap != null && _gameBootstrap.IsInitialized)
            {
                yield return StartCoroutine(RunAllTests());
            }
        }

        private IEnumerator RunAllTests()
        {
            if (_testsRunning)
            {
                LogWarning("Tests already running!");
                yield break;
            }

            _testsRunning = true;
            _passedTests = 0;
            _failedTests = 0;
            _totalTests = 0;

            LogInfo("=== Starting UI Framework Tests ===");
            UpdateStatusText("Running Tests...");

            // Test GameBootstrap
            yield return StartCoroutine(TestGameBootstrap());
            yield return new WaitForSeconds(_testInterval);

            // Test each panel
            LogInfo("Starting MainMenuPanel test");
            yield return StartCoroutine(TestMainMenuPanel());
            yield return new WaitForSeconds(_testInterval);

            LogInfo("Starting LoadingPanel test");
            yield return StartCoroutine(TestLoadingPanel());
            yield return new WaitForSeconds(_testInterval);

            LogInfo("Starting PausePanel test");
            yield return StartCoroutine(TestPausePanel());
            LogInfo("PausePanel test completed, moving to next test");
            yield return new WaitForSeconds(_testInterval);

            LogInfo("Starting GameOverPanel test");
            yield return StartCoroutine(TestGameOverPanel());
            yield return new WaitForSeconds(_testInterval);

            LogInfo("Starting Animation tests");
            yield return StartCoroutine(TestAnimations());

            // Show results
            ShowTestResults();
            _testsRunning = false;
        }

        #endregion

        #region Individual Tests

        private IEnumerator TestGameBootstrap()
        {
            LogInfo("Testing GameBootstrap...");
            
            BeginTest("GameBootstrap Initialization");
            if (_gameBootstrap != null && _gameBootstrap.IsInitialized)
            {
                PassTest("GameBootstrap is initialized");
            }
            else
            {
                FailTest("GameBootstrap is not initialized");
            }

            BeginTest("EventBus Availability");
            if (_eventBus != null)
            {
                PassTest("EventBus is available");
            }
            else
            {
                FailTest("EventBus is not available");
            }

            yield return null;
        }

        private IEnumerator TestMainMenuPanel()
        {
            LogInfo("Testing MainMenuPanel...");
            
            if (_mainMenuPanel == null)
            {
                FailTest("MainMenuPanel not found");
                yield break;
            }

            yield return StartCoroutine(TestPanelLifecycle(_mainMenuPanel, "MainMenuPanel"));
        }

        private IEnumerator TestLoadingPanel()
        {
            LogInfo("Testing LoadingPanel...");
            
            if (_loadingPanel == null)
            {
                FailTest("LoadingPanel not found");
                yield break;
            }

            yield return StartCoroutine(TestPanelLifecycle(_loadingPanel, "LoadingPanel"));

            // Test loading-specific functionality
            BeginTest("LoadingPanel Progress");
            yield return StartCoroutine(ExecuteAsyncTask(_loadingPanel.ShowAsync()));
            _loadingPanel.StartLoading();
            _loadingPanel.SetProgress(0.5f);
            yield return new WaitForSeconds(0.5f);
            _loadingPanel.SetProgress(1f);
            yield return new WaitForSeconds(0.5f);
            _loadingPanel.CompleteLoading();
            yield return StartCoroutine(ExecuteAsyncTask(_loadingPanel.HideAsync()));
            PassTest("LoadingPanel progress test completed");
        }

        private IEnumerator TestPausePanel()
        {
            LogInfo("Testing PausePanel...");
            
            if (_pausePanel == null)
            {
                FailTest("PausePanel not found");
                yield break;
            }

            yield return StartCoroutine(TestPanelLifecycle(_pausePanel, "PausePanel"));
        }

        private IEnumerator TestGameOverPanel()
        {
            LogInfo("Testing GameOverPanel...");
            
            if (_gameOverPanel == null)
            {
                FailTest("GameOverPanel not found");
                yield break;
            }

            yield return StartCoroutine(TestPanelLifecycle(_gameOverPanel, "GameOverPanel"));

            // Test game over specific functionality
            BeginTest("GameOverPanel Score Display");
            var gameOverData = new GameOverData
            {
                Result = GameResult.Victory,
                Score = 12345,
                GameType = "TestGame",
                Message = "Test completed successfully!"
            };
            _gameOverPanel.ShowGameOver(gameOverData);
            yield return new WaitForSeconds(3f); // Wait for score count-up animation
            yield return StartCoroutine(ExecuteAsyncTask(_gameOverPanel.HideAsync()));
            PassTest("GameOverPanel score display test completed");
        }

        private IEnumerator TestPanelLifecycle(UIPanel panel, string panelName)
        {
            LogInfo($"Starting lifecycle test for {panelName}");
            
            // Test Show Animation
            BeginTest($"{panelName} Show Animation");
            LogInfo($"Calling ShowAsync() for {panelName}");
            yield return StartCoroutine(ExecuteAsyncTask(panel.ShowAsync()));
            
            LogInfo($"ShowAsync() completed for {panelName}, checking visibility...");
            if (panel.IsVisible)
            {
                PassTest($"{panelName} shown successfully");
            }
            else
            {
                FailTest($"{panelName} failed to show - IsVisible: {panel.IsVisible}, CurrentState: {panel.CurrentState}");
            }

            LogInfo($"Waiting 1 second before hide test for {panelName}");
            yield return new WaitForSeconds(1f);

            // Test Hide Animation  
            BeginTest($"{panelName} Hide Animation");
            LogInfo($"Calling HideAsync() for {panelName}");
            yield return StartCoroutine(ExecuteAsyncTask(panel.HideAsync()));
            
            LogInfo($"HideAsync() completed for {panelName}, checking visibility...");
            if (!panel.IsVisible)
            {
                PassTest($"{panelName} hidden successfully");
            }
            else
            {
                FailTest($"{panelName} failed to hide - IsVisible: {panel.IsVisible}, CurrentState: {panel.CurrentState}");
            }

            LogInfo($"Lifecycle test completed for {panelName}");
            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator TestAnimations()
        {
            LogInfo("Testing UI Animations...");
            
            if (_loadingPanel == null)
            {
                FailTest("No panels available for animation testing");
                yield break;
            }

            // Test different transition types by cycling through show/hide
            var transitions = new string[]
            {
                "Fade Animation",
                "Scale Animation", 
                "Slide Animation (Left)",
                "Slide Animation (Right)"
            };

            foreach (var transition in transitions)
            {
                BeginTest($"Animation Test: {transition}");
                
                // Test animation completion without errors
                yield return StartCoroutine(ExecuteAsyncTask(_loadingPanel.ShowAsync()));
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(ExecuteAsyncTask(_loadingPanel.HideAsync()));
                yield return new WaitForSeconds(0.5f);
                
                PassTest($"{transition} completed successfully");
            }
        }

        private IEnumerator TestAllPanels()
        {
            yield return StartCoroutine(RunAllTests());
        }

        #endregion

        #region Async Helper

        /// <summary>
        /// Converts async Task to coroutine-compatible execution with timeout protection
        /// </summary>
        private IEnumerator ExecuteAsyncTask(Task task)
        {
            float timeout = 10f; // 10 second timeout
            float elapsedTime = 0f;
            
            while (!task.IsCompleted && elapsedTime < timeout)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            if (elapsedTime >= timeout)
            {
                LogError($"Async task timed out after {timeout} seconds");
                yield break;
            }

            if (task.IsFaulted)
            {
                LogError($"Async task failed: {task.Exception?.GetBaseException()?.Message}");
            }
        }

        #endregion

        #region Test Utilities

        private void BeginTest(string testName)
        {
            _totalTests++;
            LogInfo($"[TEST] {testName}");
        }

        private void PassTest(string message)
        {
            _passedTests++;
            LogSuccess($"[PASS] {message}");
        }

        private void FailTest(string message)
        {
            _failedTests++;
            LogError($"[FAIL] {message}");
        }

        private void ShowTestResults()
        {
            string resultMessage = $"Tests Complete: {_passedTests}/{_totalTests} passed, {_failedTests} failed";
            
            if (_failedTests == 0)
            {
                LogSuccess($"=== ALL TESTS PASSED! === {resultMessage}");
                UpdateStatusText($"✅ {resultMessage}");
            }
            else
            {
                LogError($"=== SOME TESTS FAILED === {resultMessage}");
                UpdateStatusText($"❌ {resultMessage}");
            }
        }

        private void UpdateStatusText(string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
            }
        }

        #endregion

        #region Logging

        private void LogInfo(string message)
        {
            if (_logDetailedResults)
            {
                Debug.Log($"[UIFrameworkTester] {message}", this);
            }
        }

        private void LogSuccess(string message)
        {
            Debug.Log($"[UIFrameworkTester] ✅ {message}", this);
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[UIFrameworkTester] ⚠️ {message}", this);
        }

        private void LogError(string message)
        {
            Debug.LogError($"[UIFrameworkTester] ❌ {message}", this);
        }

        #endregion
    }
} 