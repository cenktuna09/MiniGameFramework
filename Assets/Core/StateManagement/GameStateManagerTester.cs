using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.Core.DI;
using MiniGameFramework.Core.Bootstrap;

namespace MiniGameFramework.Core.StateManagement
{
    /// <summary>
    /// Comprehensive tester for the Game State Management system.
    /// Tests state transitions, validation, events, and integration with GameBootstrap.
    /// </summary>
    public class GameStateManagerTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool _autoStartTests = true;
        [SerializeField] private float _testInterval = 1.5f;
        [SerializeField] private bool _logDetailedResults = true;

        [Header("Manual State Controls")]
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _loadingButton;
        [SerializeField] private Button _playingButton;
        [SerializeField] private Button _pausedButton;
        [SerializeField] private Button _gameOverButton;

        [Header("Test Controls")]
        [SerializeField] private Button _testValidTransitionsButton;
        [SerializeField] private Button _testInvalidTransitionsButton;
        [SerializeField] private Button _testAllButton;
        [SerializeField] private Button _resetStateButton;

        [Header("UI Display")]
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _currentStateText;
        [SerializeField] private TextMeshProUGUI _validTransitionsText;
        [SerializeField] private TextMeshProUGUI _stateHistoryText;

        // Dependencies
        private IGameStateManager _stateManager;
        private IEventBus _eventBus;
        private GameBootstrap _gameBootstrap;

        // Test state
        private bool _testsRunning = false;
        private int _passedTests = 0;
        private int _failedTests = 0;
        private int _totalTests = 0;

        // Event subscriptions
        private IDisposable _stateChangeSubscription;
        private IDisposable _stateFailureSubscription;

        #region Unity Lifecycle

        private void Start()
        {
            InitializeTester();

            if (_autoStartTests)
            {
                StartCoroutine(DelayedAutoTest());
            }
        }

        private void OnDestroy()
        {
            CleanupSubscriptions();
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
            UpdateStatusText("Game State Manager Tester Ready");
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
            _stateManager = ServiceLocator.Instance.Resolve<IGameStateManager>();
            _eventBus = ServiceLocator.Instance.Resolve<IEventBus>();

            LogSuccess("GameBootstrap initialized successfully!");
            LogSuccess($"GameStateManager available: {_stateManager != null}");
            LogSuccess($"EventBus available: {_eventBus != null}");

            SetupEventSubscriptions();
            UpdateStateDisplay();
        }

        private void SetupEventSubscriptions()
        {
            if (_eventBus != null)
            {
                _stateChangeSubscription = _eventBus.Subscribe<GlobalGameStateChangedEvent>(OnStateChanged);
                _stateFailureSubscription = _eventBus.Subscribe<StateTransitionFailedEvent>(OnStateTransitionFailed);
            }
        }

        private void SetupTestButtons()
        {
            // Manual state controls
            if (_menuButton != null)
                _menuButton.onClick.AddListener(() => TransitionToState(GlobalGameState.Menu));

            if (_loadingButton != null)
                _loadingButton.onClick.AddListener(() => TransitionToState(GlobalGameState.Loading));

            if (_playingButton != null)
                _playingButton.onClick.AddListener(() => TransitionToState(GlobalGameState.Playing));

            if (_pausedButton != null)
                _pausedButton.onClick.AddListener(() => TransitionToState(GlobalGameState.Paused));

            if (_gameOverButton != null)
                _gameOverButton.onClick.AddListener(() => TransitionToState(GlobalGameState.GameOver));

            // Test controls
            if (_testValidTransitionsButton != null)
                _testValidTransitionsButton.onClick.AddListener(() => StartCoroutine(TestValidTransitions()));

            if (_testInvalidTransitionsButton != null)
                _testInvalidTransitionsButton.onClick.AddListener(() => StartCoroutine(TestInvalidTransitions()));

            if (_testAllButton != null)
                _testAllButton.onClick.AddListener(() => StartCoroutine(RunAllTests()));

            if (_resetStateButton != null)
                _resetStateButton.onClick.AddListener(ResetStateManager);
        }

        #endregion

        #region Event Handlers

        private void OnStateChanged(GlobalGameStateChangedEvent stateEvent)
        {
            LogSuccess($"State Changed: {stateEvent.PreviousState} -> {stateEvent.CurrentState}");
            
            if (stateEvent.StateData != null)
            {
                LogInfo($"State Data: {stateEvent.StateData}");
            }

            UpdateStateDisplay();
        }

        private void OnStateTransitionFailed(StateTransitionFailedEvent failureEvent)
        {
            LogWarning($"Transition Failed: {failureEvent.CurrentState} -> {failureEvent.RequestedState}");
            LogWarning($"Reason: {failureEvent.FailureReason}");
        }

        #endregion

        #region Manual Controls

        private void TransitionToState(GlobalGameState state)
        {
            if (_stateManager != null)
            {
                _stateManager.TransitionToState(state, $"Manual transition to {state}");
            }
        }

        private void ResetStateManager()
        {
            if (_stateManager != null)
            {
                _stateManager.Reset();
                LogInfo("State manager reset to initial state");
                UpdateStateDisplay();
            }
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

            LogInfo("=== Starting Game State Management Tests ===");
            UpdateStatusText("Running Tests...");

            // Test GameBootstrap and dependencies
            yield return StartCoroutine(TestBootstrapIntegration());
            yield return new WaitForSeconds(_testInterval);

            // Test basic state transitions
            yield return StartCoroutine(TestValidTransitions());
            yield return new WaitForSeconds(_testInterval);

            // Test invalid transitions
            yield return StartCoroutine(TestInvalidTransitions());

            // Show results
            ShowTestResults();
            _testsRunning = false;
        }

        #endregion

        #region Individual Tests

        private IEnumerator TestBootstrapIntegration()
        {
            LogInfo("Testing Bootstrap Integration...");

            BeginTest("GameBootstrap Initialization");
            if (_gameBootstrap != null && _gameBootstrap.IsInitialized)
            {
                PassTest("GameBootstrap is initialized");
            }
            else
            {
                FailTest("GameBootstrap is not initialized");
            }

            BeginTest("GameStateManager Resolution");
            if (_stateManager != null)
            {
                PassTest("GameStateManager is available");
            }
            else
            {
                FailTest("GameStateManager is not available");
            }

            BeginTest("EventBus Resolution");
            if (_eventBus != null)
            {
                PassTest("EventBus is available");
            }
            else
            {
                FailTest("EventBus is not available");
            }

            BeginTest("Initial State Check");
            if (_stateManager != null && _stateManager.CurrentState == GlobalGameState.Menu)
            {
                PassTest("Initial state is Menu");
            }
            else
            {
                FailTest($"Initial state is {_stateManager?.CurrentState}, expected Menu");
            }

            yield return null;
        }

        private IEnumerator TestValidTransitions()
        {
            LogInfo("Testing Valid Transitions...");

            if (_stateManager == null)
            {
                FailTest("StateManager not available for valid transition tests");
                yield break;
            }

            // Reset to known state
            _stateManager.Reset();
            yield return new WaitForSeconds(0.1f);

            // Test Menu -> Loading
            BeginTest("Menu to Loading Transition");
            bool success = _stateManager.TransitionToState(GlobalGameState.Loading, "Test transition");
            if (success && _stateManager.CurrentState == GlobalGameState.Loading)
            {
                PassTest("Menu to Loading transition successful");
            }
            else
            {
                FailTest($"Menu to Loading transition failed. Current: {_stateManager.CurrentState}");
            }
            yield return new WaitForSeconds(0.5f);

            // Test Loading -> Playing
            BeginTest("Loading to Playing Transition");
            success = _stateManager.TransitionToState(GlobalGameState.Playing, "Test transition");
            if (success && _stateManager.CurrentState == GlobalGameState.Playing)
            {
                PassTest("Loading to Playing transition successful");
            }
            else
            {
                FailTest($"Loading to Playing transition failed. Current: {_stateManager.CurrentState}");
            }
            yield return new WaitForSeconds(0.5f);

            // Test Playing -> Paused
            BeginTest("Playing to Paused Transition");
            success = _stateManager.TransitionToState(GlobalGameState.Paused, "Test transition");
            if (success && _stateManager.CurrentState == GlobalGameState.Paused)
            {
                PassTest("Playing to Paused transition successful");
            }
            else
            {
                FailTest($"Playing to Paused transition failed. Current: {_stateManager.CurrentState}");
            }
        }

        private IEnumerator TestInvalidTransitions()
        {
            LogInfo("Testing Invalid Transitions...");

            if (_stateManager == null)
            {
                FailTest("StateManager not available for invalid transition tests");
                yield break;
            }

            // Reset to known state
            _stateManager.Reset();
            yield return new WaitForSeconds(0.1f);

            // Test Menu -> Playing (should fail)
            BeginTest("Menu to Playing Invalid Transition");
            bool success = _stateManager.TransitionToState(GlobalGameState.Playing, "Test invalid transition");
            if (!success && _stateManager.CurrentState == GlobalGameState.Menu)
            {
                PassTest("Menu to Playing transition correctly rejected");
            }
            else
            {
                FailTest($"Menu to Playing transition should have failed. Current: {_stateManager.CurrentState}");
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

        private void UpdateStateDisplay()
        {
            if (_stateManager == null) return;

            if (_currentStateText != null)
            {
                _currentStateText.text = $"Current: {_stateManager.CurrentState}\nPrevious: {_stateManager.PreviousState}";
            }

            if (_validTransitionsText != null)
            {
                var transitions = _stateManager.GetValidTransitions();
                _validTransitionsText.text = $"Valid Transitions:\n{string.Join(", ", transitions)}";
            }

            if (_stateHistoryText != null)
            {
                var history = _stateManager.GetStateHistory();
                _stateHistoryText.text = $"History:\n{string.Join(" -> ", history)}";
            }
        }

        #endregion

        #region Logging

        private void LogInfo(string message)
        {
            if (_logDetailedResults)
            {
                Debug.Log($"[GameStateManagerTester] {message}", this);
            }
        }

        private void LogSuccess(string message)
        {
            Debug.Log($"[GameStateManagerTester] ✅ {message}", this);
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[GameStateManagerTester] ⚠️ {message}", this);
        }

        private void LogError(string message)
        {
            Debug.LogError($"[GameStateManagerTester] ❌ {message}", this);
        }

        #endregion

        #region Cleanup

        private void CleanupSubscriptions()
        {
            _stateChangeSubscription?.Dispose();
            _stateFailureSubscription?.Dispose();
        }

        #endregion
    }
} 