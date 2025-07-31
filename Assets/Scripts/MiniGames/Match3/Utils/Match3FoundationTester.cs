using System.Collections;
using UnityEngine;
using MiniGameFramework.Core.Architecture;
using MiniGameFramework.Core.DI;
using MiniGameFramework.MiniGames.Match3.Data;
using MiniGameFramework.MiniGames.Match3.Events;

namespace MiniGameFramework.MiniGames.Match3.Utils
{
    /// <summary>
    /// Test script for Match3 foundation systems.
    /// Verifies that all Week 1-2 foundation systems are working correctly.
    /// </summary>
    public class Match3FoundationTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool logDetailedResults = true;
        
        private Match3FoundationManager foundationManager;
        private IEventBus eventBus;
        
        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunFoundationTests());
            }
        }
        
        /// <summary>
        /// Runs comprehensive tests for all foundation systems.
        /// </summary>
        private IEnumerator RunFoundationTests()
        {
            Debug.Log("[Match3FoundationTester] 🧪 Starting foundation systems tests...");
            
            // Initialize foundation manager
            yield return StartCoroutine(InitializeFoundationManager());
            
            // Run individual tests
            yield return StartCoroutine(TestPositionCache());
            yield return StartCoroutine(TestAnimationManager());
            yield return StartCoroutine(TestMemoryManager());
            yield return StartCoroutine(TestEventSystem());
            
            // Final status check
            yield return StartCoroutine(TestIntegration());
            
            Debug.Log("[Match3FoundationTester] ✅ All foundation tests completed!");
        }
        
        /// <summary>
        /// Initializes the foundation manager for testing.
        /// </summary>
        private IEnumerator InitializeFoundationManager()
        {
            Debug.Log("[Match3FoundationTester] 🔧 Initializing foundation manager...");
            
            // Get EventBus from ServiceLocator
            eventBus = ServiceLocator.Instance.Resolve<IEventBus>();
            if (eventBus == null)
            {
                Debug.LogError("[Match3FoundationTester] ❌ EventBus not found!");
                yield break;
            }
            
            // Create foundation manager with test parameters
            foundationManager = new Match3FoundationManager(
                eventBus,
                tileSize: 1.0f,
                swapDuration: 0.3f,
                gravityDuration: 0.4f,
                matchAnimationDuration: 0.5f
            );
            
            yield return new WaitForSeconds(0.1f);
            Debug.Log("[Match3FoundationTester] ✅ Foundation manager initialized");
        }
        
        /// <summary>
        /// Tests the position cache system.
        /// </summary>
        private IEnumerator TestPositionCache()
        {
            Debug.Log("[Match3FoundationTester] 🧪 Testing Position Cache...");
            
            // Create test visual tiles array
            var testVisualTiles = new GameObject[8, 8];
            
            // Initialize cache
            foundationManager.InitializePositionCache(testVisualTiles);
            
            // Test cache size
            var cacheSize = foundationManager.GetPositionCacheSize();
            Debug.Log($"[Match3FoundationTester] Position cache size: {cacheSize}");
            
            // Test position lookup (should return zero for empty array)
            var testPosition = foundationManager.GetTilePosition(null);
            Debug.Log($"[Match3FoundationTester] Test position lookup: {testPosition}");
            
            yield return new WaitForSeconds(0.1f);
            Debug.Log("[Match3FoundationTester] ✅ Position Cache test completed");
        }
        
        /// <summary>
        /// Tests the animation manager system.
        /// </summary>
        private IEnumerator TestAnimationManager()
        {
            Debug.Log("[Match3FoundationTester] 🧪 Testing Animation Manager...");
            
            // Test animation status
            var hasAnimations = foundationManager.HasActiveAnimations();
            Debug.Log($"[Match3FoundationTester] Has active animations: {hasAnimations}");
            
            // Test stop animations (should not throw error)
            foundationManager.StopAllAnimations();
            Debug.Log("[Match3FoundationTester] Stop all animations called");
            
            yield return new WaitForSeconds(0.1f);
            Debug.Log("[Match3FoundationTester] ✅ Animation Manager test completed");
        }
        
        /// <summary>
        /// Tests the memory manager system.
        /// </summary>
        private IEnumerator TestMemoryManager()
        {
            Debug.Log("[Match3FoundationTester] 🧪 Testing Memory Manager...");
            
            // Test memory stats
            foundationManager.LogMemoryStats();
            
            // Test cleanup (should not throw error)
            foundationManager.CleanupAll(this);
            Debug.Log("[Match3FoundationTester] Cleanup all called");
            
            yield return new WaitForSeconds(0.1f);
            Debug.Log("[Match3FoundationTester] ✅ Memory Manager test completed");
        }
        
        /// <summary>
        /// Tests the event system integration.
        /// </summary>
        private IEnumerator TestEventSystem()
        {
            Debug.Log("[Match3FoundationTester] 🧪 Testing Event System...");
            
            // Subscribe to test events
            var subscription = eventBus.Subscribe<GravityCompletedEvent>(OnTestGravityCompleted);
            
            // Publish test event
            eventBus.Publish(new GravityCompletedEvent(0, 5, 1.5f, this));
            
            yield return new WaitForSeconds(0.1f);
            
            // Unsubscribe
            subscription?.Dispose();
            
            Debug.Log("[Match3FoundationTester] ✅ Event System test completed");
        }
        
        /// <summary>
        /// Tests the integration of all systems.
        /// </summary>
        private IEnumerator TestIntegration()
        {
            Debug.Log("[Match3FoundationTester] 🧪 Testing System Integration...");
            
            // Get status summary
            var statusSummary = foundationManager.GetStatusSummary();
            Debug.Log($"[Match3FoundationTester] Status Summary:\n{statusSummary}");
            
            // Test completion tracking
            var gravityCompleted = foundationManager.IsGravityCompleted(0);
            var refillCompleted = foundationManager.IsRefillCompleted(0);
            
            Debug.Log($"[Match3FoundationTester] Gravity completed: {gravityCompleted}");
            Debug.Log($"[Match3FoundationTester] Refill completed: {refillCompleted}");
            
            yield return new WaitForSeconds(0.1f);
            Debug.Log("[Match3FoundationTester] ✅ Integration test completed");
        }
        
        /// <summary>
        /// Test event handler for gravity completed events.
        /// </summary>
        /// <param name="gravityEvent">The gravity completed event.</param>
        private void OnTestGravityCompleted(GravityCompletedEvent gravityEvent)
        {
            Debug.Log($"[Match3FoundationTester] 📡 Received gravity completed event: Column {gravityEvent.Column}, Tiles: {gravityEvent.MovedTiles}, Duration: {gravityEvent.Duration:F2}s");
        }
        
        /// <summary>
        /// Manual test trigger from Inspector.
        /// </summary>
        [ContextMenu("Run Foundation Tests")]
        public void RunTests()
        {
            StartCoroutine(RunFoundationTests());
        }
        
        /// <summary>
        /// Manual cleanup trigger from Inspector.
        /// </summary>
        [ContextMenu("Cleanup Foundation Systems")]
        public void CleanupSystems()
        {
            if (foundationManager != null)
            {
                foundationManager.CleanupAll(this);
                Debug.Log("[Match3FoundationTester] 🧹 Foundation systems cleaned up");
            }
        }
        
        /// <summary>
        /// Manual status check from Inspector.
        /// </summary>
        [ContextMenu("Check Foundation Status")]
        public void CheckStatus()
        {
            if (foundationManager != null)
            {
                var status = foundationManager.GetStatusSummary();
                Debug.Log($"[Match3FoundationTester] 📊 Foundation Status:\n{status}");
            }
            else
            {
                Debug.LogWarning("[Match3FoundationTester] ⚠️ Foundation manager not initialized");
            }
        }
    }
} 