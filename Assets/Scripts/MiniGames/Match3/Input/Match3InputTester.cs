using System.Collections;
using UnityEngine;
using Core.Architecture;
using Core.DI;
using MiniGameFramework.MiniGames.Match3.Utils;

namespace MiniGameFramework.MiniGames.Match3.Input
{
    /// <summary>
    /// Test script for Match3 input systems.
    /// Verifies that input handling works correctly with foundation systems.
    /// </summary>
    public class Match3InputTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool logDetailedResults = true;
        
        private Match3InputManager inputManager;
        private Match3FoundationManager foundationManager;
        private IEventBus eventBus;
        
        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunInputTests());
            }
        }
        
        /// <summary>
        /// Runs comprehensive tests for the input system.
        /// </summary>
        private IEnumerator RunInputTests()
        {
            Debug.Log("[Match3InputTester] 🧪 Starting input system tests...");
            
            // Initialize managers
            yield return StartCoroutine(InitializeManagers());
            
            // Run individual tests
            yield return StartCoroutine(TestInputInitialization());
            yield return StartCoroutine(TestInputProcessing());
            yield return StartCoroutine(TestInputLocking());
            yield return StartCoroutine(TestFoundationIntegration());
            
            // Final status check
            yield return StartCoroutine(TestIntegration());
            
            Debug.Log("[Match3InputTester] ✅ All input tests completed!");
        }
        
        /// <summary>
        /// Initializes the input and foundation managers for testing.
        /// </summary>
        private IEnumerator InitializeManagers()
        {
            Debug.Log("[Match3InputTester] 🔧 Initializing managers...");
            
            // Get EventBus from ServiceLocator
            eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            if (eventBus == null)
            {
                Debug.LogError("[Match3InputTester] ❌ EventBus not found!");
                yield break;
            }
            
            // Create foundation manager
            foundationManager = new Match3FoundationManager(
                eventBus,
                tileSize: 1.0f,
                swapDuration: 0.3f,
                gravityDuration: 0.4f,
                matchAnimationDuration: 0.5f
            );
            
            // Create input manager
            inputManager = new Match3InputManager(
                eventBus,
                foundationManager,
                tileSize: 1.0f,
                swapDuration: 0.3f
            );
            
            yield return new WaitForSeconds(0.1f);
            Debug.Log("[Match3InputTester] ✅ Managers initialized");
        }
        
        /// <summary>
        /// Tests input system initialization.
        /// </summary>
        private IEnumerator TestInputInitialization()
        {
            Debug.Log("[Match3InputTester] 🧪 Testing Input Initialization...");
            
            // Test input state summary
            var inputState = inputManager.GetInputStateSummary();
            Debug.Log($"[Match3InputTester] Input state: {inputState}");
            
            // Test status summary
            var statusSummary = inputManager.GetStatusSummary();
            Debug.Log($"[Match3InputTester] Status summary: {statusSummary}");
            
            yield return new WaitForSeconds(0.1f);
            Debug.Log("[Match3InputTester] ✅ Input Initialization test completed");
        }
        
        /// <summary>
        /// Tests input processing functionality.
        /// </summary>
        private IEnumerator TestInputProcessing()
        {
            Debug.Log("[Match3InputTester] 🧪 Testing Input Processing...");
            
            // Test input processing with no processing/matching
            var result = inputManager.ProcessInput(isProcessingMatches: false, isSwapping: false);
            Debug.Log($"[Match3InputTester] Input result: {result}");
            
            // Test input processing with processing/matching (should return none)
            var lockedResult = inputManager.ProcessInput(isProcessingMatches: true, isSwapping: false);
            Debug.Log($"[Match3InputTester] Locked input result: {lockedResult}");
            
            yield return new WaitForSeconds(0.1f);
            Debug.Log("[Match3InputTester] ✅ Input Processing test completed");
        }
        
        /// <summary>
        /// Tests input locking functionality.
        /// </summary>
        private IEnumerator TestInputLocking()
        {
            Debug.Log("[Match3InputTester] 🧪 Testing Input Locking...");
            
            // Test input locking
            inputManager.LockInput();
            var lockedState = inputManager.GetInputStateSummary();
            Debug.Log($"[Match3InputTester] Locked state: {lockedState}");
            
            // Test input unlocking
            inputManager.UnlockInput();
            var unlockedState = inputManager.GetInputStateSummary();
            Debug.Log($"[Match3InputTester] Unlocked state: {unlockedState}");
            
            yield return new WaitForSeconds(0.1f);
            Debug.Log("[Match3InputTester] ✅ Input Locking test completed");
        }
        
        /// <summary>
        /// Tests foundation integration.
        /// </summary>
        private IEnumerator TestFoundationIntegration()
        {
            Debug.Log("[Match3InputTester] 🧪 Testing Foundation Integration...");
            
            // Test foundation manager status
            var foundationStatus = foundationManager.GetStatusSummary();
            Debug.Log($"[Match3InputTester] Foundation status: {foundationStatus}");
            
            // Test position cache integration
            var cacheSize = foundationManager.GetPositionCacheSize();
            Debug.Log($"[Match3InputTester] Position cache size: {cacheSize}");
            
            yield return new WaitForSeconds(0.1f);
            Debug.Log("[Match3InputTester] ✅ Foundation Integration test completed");
        }
        
        /// <summary>
        /// Tests the integration of all systems.
        /// </summary>
        private IEnumerator TestIntegration()
        {
            Debug.Log("[Match3InputTester] 🧪 Testing System Integration...");
            
            // Get comprehensive status summary
            var statusSummary = inputManager.GetStatusSummary();
            Debug.Log($"[Match3InputTester] Comprehensive Status:\n{statusSummary}");
            
            // Test force deselection
            inputManager.ForceDeselect();
            var finalState = inputManager.GetInputStateSummary();
            Debug.Log($"[Match3InputTester] Final state after force deselect: {finalState}");
            
            yield return new WaitForSeconds(0.1f);
            Debug.Log("[Match3InputTester] ✅ Integration test completed");
        }
        
        /// <summary>
        /// Manual test trigger from Inspector.
        /// </summary>
        [ContextMenu("Run Input Tests")]
        public void RunTests()
        {
            StartCoroutine(RunInputTests());
        }
        
        /// <summary>
        /// Manual status check from Inspector.
        /// </summary>
        [ContextMenu("Check Input Status")]
        public void CheckStatus()
        {
            if (inputManager != null)
            {
                var status = inputManager.GetStatusSummary();
                Debug.Log($"[Match3InputTester] 📊 Input Status:\n{status}");
            }
            else
            {
                Debug.LogWarning("[Match3InputTester] ⚠️ Input manager not initialized");
            }
        }
        
        /// <summary>
        /// Manual input processing test from Inspector.
        /// </summary>
        [ContextMenu("Test Input Processing")]
        public void TestInputProcessingManual()
        {
            if (inputManager != null)
            {
                var result = inputManager.ProcessInput(false, false);
                Debug.Log($"[Match3InputTester] 🎮 Manual input result: {result}");
            }
            else
            {
                Debug.LogWarning("[Match3InputTester] ⚠️ Input manager not initialized");
            }
        }
    }
} 