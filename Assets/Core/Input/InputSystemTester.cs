using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.Core.DI;

namespace MiniGameFramework.Core.Input
{
    /// <summary>
    /// Comprehensive tester for the Input System.
    /// Tests input event distribution, context switching, and input responsiveness.
    /// </summary>
    public class InputSystemTester : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform testUIParent;
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI logText;
        [SerializeField] private Scrollbar logScrollbar;

        [Header("Test Configuration")]
        [SerializeField] private bool autoRunTests = true;
        [SerializeField] private float testInterval = 3f;
        [SerializeField] private int maxLogEntries = 20;

        private IInputManager inputManager;
        private IEventBus eventBus;
        private List<string> testLogs = new List<string>();
        private int testsPassed = 0;
        private int testsFailed = 0;
        private float nextTestTime = 0f;
        private int currentTestIndex = 0;

        // Event counters for testing
        private int uiClickEvents = 0;
        private int jumpEvents = 0;
        private int moveEvents = 0;
        private int swipeEvents = 0;
        private int laneChangeEvents = 0;

        private readonly List<Action> tests = new List<Action>();

        private void Start()
        {
            InitializeReferences();
            SetupUI();
            SetupEventSubscriptions();
            InitializeTests();
            
            if (autoRunTests)
            {
                nextTestTime = Time.time + 2f; // Initial delay
            }
        }

        private void InitializeReferences()
        {
            inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
            eventBus = ServiceLocator.Instance.Resolve<IEventBus>();

            if (inputManager == null)
            {
                LogTest("ERROR: InputManager service not found!", false);
                return;
            }

            if (eventBus == null)
            {
                LogTest("ERROR: EventBus service not found!", false);
                return;
            }

            LogTest("Input System Tester initialized successfully", true);
        }

        private void SetupUI()
        {
            if (testUIParent == null) return;

            CreateTestButton("Test Menu Context", () => TestContextSwitch(InputContext.Menu));
            CreateTestButton("Test Match3 Context", () => TestContextSwitch(InputContext.Match3));
            CreateTestButton("Test Runner Context", () => TestContextSwitch(InputContext.EndlessRunner));
            CreateTestButton("Test Pause Context", () => TestContextSwitch(InputContext.Paused));
            CreateTestButton("Enable Input", () => TestInputToggle(true));
            CreateTestButton("Disable Input", () => TestInputToggle(false));
            CreateTestButton("Simulate Click", SimulateClick);
            CreateTestButton("Reset Counters", ResetCounters);
            CreateTestButton("Run All Tests", RunAllTests);
        }

        private void CreateTestButton(string label, Action onClick)
        {
            if (testUIParent == null) return;

            // Create button manually since we don't have prefab
            var buttonObj = new GameObject($"Button_{label}");
            buttonObj.transform.SetParent(testUIParent, false);
            
            var rectTransform = buttonObj.AddComponent<RectTransform>();
            var button = buttonObj.AddComponent<Button>();
            var image = buttonObj.AddComponent<Image>();
            
            // Button styling
            image.color = new Color(0.2f, 0.3f, 0.5f, 1f);
            var colors = button.colors;
            colors.highlightedColor = new Color(0.3f, 0.4f, 0.6f, 1f);
            colors.pressedColor = new Color(0.1f, 0.2f, 0.4f, 1f);
            button.colors = colors;
            
            // Button text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            var text = textObj.AddComponent<TextMeshProUGUI>();
            
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            text.text = label;
            text.fontSize = 12;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            
            button.onClick.AddListener(() => onClick?.Invoke());
        }

        private void SetupEventSubscriptions()
        {
            if (eventBus == null) return;

            eventBus.Subscribe<UIActionInputEvent>(OnUIActionEvent);
            eventBus.Subscribe<PlayerActionInputEvent>(OnPlayerActionEvent);
            eventBus.Subscribe<PlayerMoveInputEvent>(OnMoveEvent);
            eventBus.Subscribe<SwipeInputEvent>(OnSwipeEvent);
            eventBus.Subscribe<LaneChangeInputEvent>(OnLaneChangeEvent);
            eventBus.Subscribe<TileClickInputEvent>(OnTileClickEvent);
        }

        private void InitializeTests()
        {
            tests.Add(TestInputManagerService);
            tests.Add(TestEventBusIntegration);
            tests.Add(TestContextSwitching);
            tests.Add(TestInputEnableDisable);
            tests.Add(TestEventDistribution);
        }

        private void Update()
        {
            if (autoRunTests && Time.time >= nextTestTime && tests.Count > 0)
            {
                if (currentTestIndex < tests.Count)
                {
                    tests[currentTestIndex]?.Invoke();
                    currentTestIndex++;
                    nextTestTime = Time.time + testInterval;
                }
                else
                {
                    // All tests completed
                    LogTest($"=== AUTO TESTS COMPLETED === Passed: {testsPassed}, Failed: {testsFailed}", true);
                    autoRunTests = false;
                }
            }

            UpdateStatusText();
        }

        #region Test Methods

        private void TestInputManagerService()
        {
            LogTest("=== Testing InputManager Service ===", true);
            
            bool serviceFound = inputManager != null;
            LogTest($"InputManager service registration: {(serviceFound ? "PASS" : "FAIL")}", serviceFound);
            
            if (serviceFound)
            {
                bool isEnabled = inputManager.IsInputEnabled;
                LogTest($"Input enabled state: {isEnabled}", true);
                
                var context = inputManager.CurrentContext;
                LogTest($"Current input context: {context}", true);
            }
        }

        private void TestEventBusIntegration()
        {
            LogTest("=== Testing EventBus Integration ===", true);
            
            bool eventBusFound = eventBus != null;
            LogTest($"EventBus service registration: {(eventBusFound ? "PASS" : "FAIL")}", eventBusFound);
        }

        private void TestContextSwitching()
        {
            LogTest("=== Testing Context Switching ===", true);
            
            if (inputManager == null) return;

            var contexts = new[] { InputContext.Menu, InputContext.Match3, InputContext.EndlessRunner, InputContext.Paused };
            bool allPassed = true;

            foreach (var context in contexts)
            {
                inputManager.SwitchContext(context);
                bool contextSet = inputManager.CurrentContext == context;
                LogTest($"Switch to {context}: {(contextSet ? "PASS" : "FAIL")}", contextSet);
                allPassed = allPassed && contextSet;
            }

            LogTest($"Context switching test: {(allPassed ? "PASS" : "FAIL")}", allPassed);
        }

        private void TestInputEnableDisable()
        {
            LogTest("=== Testing Input Enable/Disable ===", true);
            
            if (inputManager == null) return;

            inputManager.DisableInput();
            bool disabledCorrectly = !inputManager.IsInputEnabled;
            LogTest($"Input disable: {(disabledCorrectly ? "PASS" : "FAIL")}", disabledCorrectly);

            inputManager.EnableInput();
            bool enabledCorrectly = inputManager.IsInputEnabled;
            LogTest($"Input enable: {(enabledCorrectly ? "PASS" : "FAIL")}", enabledCorrectly);

            bool testPassed = disabledCorrectly && enabledCorrectly;
            LogTest($"Enable/Disable test: {(testPassed ? "PASS" : "FAIL")}", testPassed);
        }

        private void TestEventDistribution()
        {
            LogTest("=== Testing Event Distribution ===", true);
            
            int initialClickEvents = uiClickEvents;
            int initialMoveEvents = moveEvents;
            int initialJumpEvents = jumpEvents;
            
            // Reset and check if events are being received
            LogTest($"Initial event counts - Clicks: {uiClickEvents}, Moves: {moveEvents}, Jumps: {jumpEvents}", true);
            
            bool eventsWorking = uiClickEvents > 0 || moveEvents > 0 || jumpEvents > 0;
            LogTest($"Event distribution test: {(eventsWorking ? "PASS - Events detected" : "INFO - No events yet (use inputs to test)")}", eventsWorking);
        }

        #endregion

        #region Test Actions

        private void TestContextSwitch(InputContext context)
        {
            if (inputManager == null) return;
            
            inputManager.SwitchContext(context);
            LogTest($"Switched to context: {context}", true);
        }

        private void TestInputToggle(bool enable)
        {
            if (inputManager == null) return;
            
            if (enable)
                inputManager.EnableInput();
            else
                inputManager.DisableInput();
            
            LogTest($"Input {(enable ? "enabled" : "disabled")}", true);
        }

        private void SimulateClick()
        {
            // This would normally be triggered by actual input
            var clickPos = inputManager?.GetPointerPosition() ?? Vector2.zero;
            LogTest($"Simulated click at position: {clickPos}", true);
        }

        private void ResetCounters()
        {
            uiClickEvents = 0;
            jumpEvents = 0;
            moveEvents = 0;
            swipeEvents = 0;
            laneChangeEvents = 0;
            LogTest("Event counters reset", true);
        }

        private void RunAllTests()
        {
            LogTest("=== MANUAL TEST RUN STARTED ===", true);
            testsPassed = 0;
            testsFailed = 0;
            
            foreach (var test in tests)
            {
                test?.Invoke();
            }
            
            LogTest($"=== MANUAL TESTS COMPLETED === Passed: {testsPassed}, Failed: {testsFailed}", true);
        }

        #endregion

        #region Event Handlers

        private void OnUIActionEvent(UIActionInputEvent evt)
        {
            uiClickEvents++;
            LogTest($"UI Action: {evt.Action} at {evt.PointerPosition}", true);
        }

        private void OnPlayerActionEvent(PlayerActionInputEvent evt)
        {
            if (evt.Action == PlayerActionInputEvent.ActionType.Jump)
            {
                jumpEvents++;
                LogTest($"Jump event detected! Total: {jumpEvents}", true);
            }
        }

        private void OnMoveEvent(PlayerMoveInputEvent evt)
        {
            moveEvents++;
            LogTest($"Move: {evt.MoveDirection} (Pressed: {evt.IsPressed})", true);
        }

        private void OnSwipeEvent(SwipeInputEvent evt)
        {
            swipeEvents++;
            LogTest($"Swipe detected! Direction: {evt.Direction}, Distance: {evt.Distance:F1}", true);
        }

        private void OnLaneChangeEvent(LaneChangeInputEvent evt)
        {
            laneChangeEvents++;
            LogTest($"Lane change: {evt.Direction}", true);
        }

        private void OnTileClickEvent(TileClickInputEvent evt)
        {
            LogTest($"Tile click at screen: {evt.ScreenPosition}, world: {evt.WorldPosition}", true);
        }

        #endregion

        #region Utility Methods

        private void LogTest(string message, bool passed)
        {
            if (passed) testsPassed++;
            else testsFailed++;

            string logEntry = $"[{Time.time:F1}s] {message}";
            testLogs.Add(logEntry);

            if (testLogs.Count > maxLogEntries)
            {
                testLogs.RemoveAt(0);
            }

            UpdateLogDisplay();
            Debug.Log($"InputSystemTester: {logEntry}");
        }

        private void UpdateLogDisplay()
        {
            if (logText != null)
            {
                logText.text = string.Join("\n", testLogs);
                
                // Auto-scroll to bottom
                if (logScrollbar != null)
                {
                    logScrollbar.value = 0f;
                }
            }
        }

        private void UpdateStatusText()
        {
            if (statusText == null) return;

            string status = $"Input System Status\n";
            status += $"Service Active: {(inputManager != null ? "✓" : "✗")}\n";
            status += $"Input Enabled: {(inputManager?.IsInputEnabled == true ? "✓" : "✗")}\n";
            status += $"Current Context: {inputManager?.CurrentContext ?? InputContext.Menu}\n";
            status += $"Tests Passed: {testsPassed} | Failed: {testsFailed}\n\n";
            status += $"Event Counts:\n";
            status += $"UI Clicks: {uiClickEvents}\n";
            status += $"Jumps: {jumpEvents}\n";
            status += $"Moves: {moveEvents}\n";
            status += $"Swipes: {swipeEvents}\n";
            status += $"Lane Changes: {laneChangeEvents}";

            statusText.text = status;
        }

        #endregion

        private void OnDestroy()
        {
            if (eventBus != null)
            {
                eventBus.Unsubscribe<UIActionInputEvent>(OnUIActionEvent);
                eventBus.Unsubscribe<PlayerActionInputEvent>(OnPlayerActionEvent);
                eventBus.Unsubscribe<PlayerMoveInputEvent>(OnMoveEvent);
                eventBus.Unsubscribe<SwipeInputEvent>(OnSwipeEvent);
                eventBus.Unsubscribe<LaneChangeInputEvent>(OnLaneChangeEvent);
                eventBus.Unsubscribe<TileClickInputEvent>(OnTileClickEvent);
            }
        }
    }
}