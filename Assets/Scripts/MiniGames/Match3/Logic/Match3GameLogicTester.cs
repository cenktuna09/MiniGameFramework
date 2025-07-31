using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniGameFramework.Core.DI;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.MiniGames.Match3.Logic;
using MiniGameFramework.MiniGames.Match3.Config;
using MiniGameFramework.MiniGames.Match3.ErrorHandling;
using MiniGameFramework.MiniGames.Match3.Board;
using MiniGameFramework.MiniGames.Match3.Data;

namespace MiniGameFramework.MiniGames.Match3.Logic
{
    /// <summary>
    /// Test script for Match3GameLogicManager functionality.
    /// Provides comprehensive testing and debugging tools for the game logic system.
    /// </summary>
    public class Match3GameLogicTester : MonoBehaviour
    {
        #region Private Fields
        
        private Match3GameLogicManager gameLogicManager;
        private Match3ErrorHandler errorHandler;
        private Match3Config testConfig;
        private IEventBus eventBus;
        
        // Test state
        private bool isRunningTests;
        private int testPassCount;
        private int testFailCount;
        private List<string> testResults;
        
        // UI elements for testing
        private bool showTestUI = true;
        private Vector2 scrollPosition;
        private float testInterval = 2f;
        private float lastTestTime;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeTestSystem();
            StartCoroutine(RunAutomatedTests());
        }
        
        private void Update()
        {
            // Auto-run tests at intervals
            if (isRunningTests && Time.time - lastTestTime > testInterval)
            {
                lastTestTime = Time.time;
                RunQuickTests();
            }
        }
        
        private void OnDestroy()
        {
            CleanupTestSystem();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeTestSystem()
        {
            try
            {
                // Get dependencies
                eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
                if (eventBus == null)
                {
                    Debug.LogError("[Match3GameLogicTester] ❌ Failed to get IEventBus from ServiceLocator");
                    return;
                }
                
                // Initialize components
                gameLogicManager = new Match3GameLogicManager(eventBus);
                errorHandler = new Match3ErrorHandler(eventBus, true, true, 50);
                
                // Create test configuration
                testConfig = ScriptableObject.CreateInstance<Match3Config>();
                testConfig.boardWidth = 6;
                testConfig.boardHeight = 6;
                testConfig.ValidateSettings();
                
                // Initialize test state
                testResults = new List<string>();
                testPassCount = 0;
                testFailCount = 0;
                
                // Subscribe to events
                SubscribeToEvents();
                
                Debug.Log("[Match3GameLogicTester] ✅ Test system initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Match3GameLogicTester] ❌ Failed to initialize test system: {ex.Message}");
            }
        }
        
        private void SubscribeToEvents()
        {
            if (gameLogicManager != null)
            {
                gameLogicManager.OnPossibleSwapsUpdated += OnPossibleSwapsUpdated;
                gameLogicManager.OnMatchesFound += OnMatchesFound;
                gameLogicManager.OnValidSwapExecuted += OnValidSwapExecuted;
                gameLogicManager.OnInvalidSwapAttempted += OnInvalidSwapAttempted;
            }
            
            if (errorHandler != null)
            {
                errorHandler.OnErrorOccurred += OnErrorOccurred;
                errorHandler.OnWarningOccurred += OnWarningOccurred;
            }
        }
        
        #endregion
        
        #region Automated Tests
        
        private IEnumerator RunAutomatedTests()
        {
            Debug.Log("[Match3GameLogicTester] 🧪 Starting automated tests...");
            
            yield return new WaitForSeconds(1f);
            
            // Test 1: Basic initialization
            TestBasicInitialization();
            yield return new WaitForSeconds(0.5f);
            
            // Test 2: Board creation and validation
            TestBoardCreation();
            yield return new WaitForSeconds(0.5f);
            
            // Test 3: Swap validation
            TestSwapValidation();
            yield return new WaitForSeconds(0.5f);
            
            // Test 4: Match detection
            TestMatchDetection();
            yield return new WaitForSeconds(0.5f);
            
            // Test 5: Error handling
            TestErrorHandling();
            yield return new WaitForSeconds(0.5f);
            
            // Test 6: Performance tests
            TestPerformance();
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log($"[Match3GameLogicTester] 🎯 Automated tests completed. Pass: {testPassCount}, Fail: {testFailCount}");
        }
        
        private void RunQuickTests()
        {
            if (!isRunningTests) return;
            
            // Quick validation tests
            TestBasicInitialization();
            TestBoardCreation();
        }
        
        #endregion
        
        #region Test Methods
        
        private void TestBasicInitialization()
        {
            try
            {
                if (gameLogicManager == null)
                {
                    LogTestResult("Basic Initialization", false, "GameLogicManager is null");
                    return;
                }
                
                if (errorHandler == null)
                {
                    LogTestResult("Basic Initialization", false, "ErrorHandler is null");
                    return;
                }
                
                if (testConfig == null)
                {
                    LogTestResult("Basic Initialization", false, "TestConfig is null");
                    return;
                }
                
                LogTestResult("Basic Initialization", true, "All components initialized successfully");
            }
            catch (System.Exception ex)
            {
                LogTestResult("Basic Initialization", false, $"Exception: {ex.Message}");
            }
        }
        
        private void TestBoardCreation()
        {
            try
            {
                // Create a test board
                var testBoard = BoardGenerator.GenerateBoard();
                
                if (testBoard == null)
                {
                    LogTestResult("Board Creation", false, "Generated board is null");
                    return;
                }
                
                // Initialize game logic with board
                gameLogicManager.InitializeBoard(testBoard);
                
                // Check if possible swaps were detected
                var possibleSwaps = gameLogicManager.GetPossibleSwaps();
                
                if (possibleSwaps == null)
                {
                    LogTestResult("Board Creation", false, "Possible swaps list is null");
                    return;
                }
                
                LogTestResult("Board Creation", true, $"Board created successfully. Possible swaps: {possibleSwaps.Count}");
            }
            catch (System.Exception ex)
            {
                LogTestResult("Board Creation", false, $"Exception: {ex.Message}");
            }
        }
        
        private void TestSwapValidation()
        {
            try
            {
                // Create test board
                var testBoard = BoardGenerator.GenerateBoard();
                gameLogicManager.InitializeBoard(testBoard);
                
                // Test valid swap
                var validSwap = new Swap(new Vector2Int(0, 0), new Vector2Int(1, 0));
                bool isValid = gameLogicManager.ValidateAndExecuteSwap(validSwap);
                
                // Test invalid swap (same position)
                var invalidSwap = new Swap(new Vector2Int(0, 0), new Vector2Int(0, 0));
                bool isInvalid = !gameLogicManager.ValidateAndExecuteSwap(invalidSwap);
                
                if (isValid && isInvalid)
                {
                    LogTestResult("Swap Validation", true, "Valid and invalid swaps handled correctly");
                }
                else
                {
                    LogTestResult("Swap Validation", false, $"Valid: {isValid}, Invalid: {isInvalid}");
                }
            }
            catch (System.Exception ex)
            {
                LogTestResult("Swap Validation", false, $"Exception: {ex.Message}");
            }
        }
        
        private void TestMatchDetection()
        {
            try
            {
                // Create test board with known matches
                var testBoard = CreateTestBoardWithMatches();
                gameLogicManager.InitializeBoard(testBoard);
                
                // Process matches
                var matches = gameLogicManager.ProcessMatches();
                
                if (matches != null && matches.Count > 0)
                {
                    LogTestResult("Match Detection", true, $"Found {matches.Count} matches");
                }
                else
                {
                    LogTestResult("Match Detection", false, "No matches detected in test board");
                }
            }
            catch (System.Exception ex)
            {
                LogTestResult("Match Detection", false, $"Exception: {ex.Message}");
            }
        }
        
        private void TestErrorHandling()
        {
            try
            {
                // Test null swap validation
                var testBoard = BoardGenerator.GenerateBoard();
                var invalidSwap = new Swap(Vector2Int.zero, Vector2Int.zero);
                errorHandler.ValidateSwap(invalidSwap, testBoard, "Test invalid swap");
                LogTestResult("Error Handling", false, "Should have thrown exception for invalid swap");
            }
            catch (System.Exception ex)
            {
                LogTestResult("Error Handling", true, $"Correctly caught exception: {ex.GetType().Name}");
            }
            
            try
            {
                // Test configuration validation
                var invalidConfig = ScriptableObject.CreateInstance<Match3Config>();
                invalidConfig.boardWidth = 1; // Invalid size
                errorHandler.ValidateConfiguration(invalidConfig);
                LogTestResult("Error Handling", false, "Should have thrown exception for invalid config");
            }
            catch (System.Exception ex)
            {
                LogTestResult("Error Handling", true, $"Correctly caught configuration exception: {ex.GetType().Name}");
            }
        }
        
        private void TestPerformance()
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // Test board generation performance
                var testBoard = BoardGenerator.GenerateBoard();
                gameLogicManager.InitializeBoard(testBoard);
                
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                
                if (elapsedMs < 100) // Should be very fast
                {
                    LogTestResult("Performance", true, $"Board initialization took {elapsedMs}ms");
                }
                else
                {
                    LogTestResult("Performance", false, $"Board initialization too slow: {elapsedMs}ms");
                }
            }
            catch (System.Exception ex)
            {
                LogTestResult("Performance", false, $"Exception: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private BoardData CreateTestBoardWithMatches()
        {
            // Create a simple test board with known matches
            var board = new BoardData(6, 6);
            
            // Fill with same tile type to create matches
            var tileType = TileType.Red;
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    board = board.SetTile(new Vector2Int(x, y), new TileData(tileType, new Vector2Int(x, y)));
                }
            }
            
            return board;
        }
        
        private void LogTestResult(string testName, bool passed, string message)
        {
            var result = passed ? "✅ PASS" : "❌ FAIL";
            var logMessage = $"[{testName}] {result}: {message}";
            
            if (passed)
            {
                testPassCount++;
                Debug.Log(logMessage);
            }
            else
            {
                testFailCount++;
                Debug.LogError(logMessage);
            }
            
            testResults.Add($"{System.DateTime.Now:HH:mm:ss} - {logMessage}");
            
            // Keep only recent results
            if (testResults.Count > 50)
            {
                testResults.RemoveAt(0);
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnPossibleSwapsUpdated(List<Swap> swaps)
        {
            Debug.Log($"[Match3GameLogicTester] 📋 Possible swaps updated: {swaps?.Count ?? 0} swaps");
        }
        
        private void OnMatchesFound(List<MatchDetector.Match> matches)
        {
            Debug.Log($"[Match3GameLogicTester] 🎯 Matches found: {matches?.Count ?? 0} matches");
        }
        
        private void OnValidSwapExecuted(Swap swap)
        {
            Debug.Log($"[Match3GameLogicTester] ✅ Valid swap executed: {swap.tileA} ↔ {swap.tileB}");
        }
        
        private void OnInvalidSwapAttempted(Swap swap)
        {
            Debug.Log($"[Match3GameLogicTester] ❌ Invalid swap attempted: {swap.tileA} ↔ {swap.tileB}");
        }
        
        private void OnErrorOccurred(System.Exception exception)
        {
            Debug.LogError($"[Match3GameLogicTester] 🚨 Error occurred: {exception.Message}");
        }
        
        private void OnWarningOccurred(string message)
        {
            Debug.LogWarning($"[Match3GameLogicTester] ⚠️ Warning: {message}");
        }
        
        #endregion
        
        #region UI Methods
        
        private void OnGUI()
        {
            if (!showTestUI) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 400, Screen.height - 20));
            
            GUILayout.Label("Match3 Game Logic Tester", GUI.skin.box);
            
            // Test controls
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Run All Tests"))
            {
                StartCoroutine(RunAutomatedTests());
            }
            if (GUILayout.Button("Clear Results"))
            {
                testResults.Clear();
                testPassCount = 0;
                testFailCount = 0;
            }
            GUILayout.EndHorizontal();
            
            // Test statistics
            GUILayout.Label($"Tests Passed: {testPassCount}", GUI.skin.box);
            GUILayout.Label($"Tests Failed: {testFailCount}", GUI.skin.box);
            
            // Test results
            GUILayout.Label("Test Results:", GUI.skin.box);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            
            foreach (var result in testResults)
            {
                GUILayout.Label(result, GUI.skin.label);
            }
            
            GUILayout.EndScrollView();
            
            // Manual test buttons
            GUILayout.Label("Manual Tests:", GUI.skin.box);
            
            if (GUILayout.Button("Test Basic Initialization"))
            {
                TestBasicInitialization();
            }
            
            if (GUILayout.Button("Test Board Creation"))
            {
                TestBoardCreation();
            }
            
            if (GUILayout.Button("Test Swap Validation"))
            {
                TestSwapValidation();
            }
            
            if (GUILayout.Button("Test Match Detection"))
            {
                TestMatchDetection();
            }
            
            if (GUILayout.Button("Test Error Handling"))
            {
                TestErrorHandling();
            }
            
            if (GUILayout.Button("Test Performance"))
            {
                TestPerformance();
            }
            
            GUILayout.EndArea();
        }
        
        #endregion
        
        #region Cleanup
        
        private void CleanupTestSystem()
        {
            try
            {
                // Unsubscribe from events
                if (gameLogicManager != null)
                {
                    gameLogicManager.OnPossibleSwapsUpdated -= OnPossibleSwapsUpdated;
                    gameLogicManager.OnMatchesFound -= OnMatchesFound;
                    gameLogicManager.OnValidSwapExecuted -= OnValidSwapExecuted;
                    gameLogicManager.OnInvalidSwapAttempted -= OnInvalidSwapAttempted;
                }
                
                if (errorHandler != null)
                {
                    errorHandler.OnErrorOccurred -= OnErrorOccurred;
                    errorHandler.OnWarningOccurred -= OnWarningOccurred;
                }
                
                Debug.Log("[Match3GameLogicTester] 🧹 Test system cleaned up");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Match3GameLogicTester] ❌ Error during cleanup: {ex.Message}");
            }
        }
        
        #endregion
    }
} 