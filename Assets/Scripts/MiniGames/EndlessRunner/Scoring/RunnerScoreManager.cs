using UnityEngine;
using Core.Architecture;
using Core.Common.ScoringManagement;
using EndlessRunner.Events;

namespace EndlessRunner.Scoring
{
    /// <summary>
    /// Score manager for 3D Endless Runner
    /// Extends BaseScoreManager with runner-specific scoring logic
    /// </summary>
    public class RunnerScoreManager : BaseScoreManager
    {
        #region Private Fields
        
        private float _distanceTraveled = 0f;
        private float _lastPositionZ = 0f;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Initialize runner score manager
        /// </summary>
        /// <param name="eventBus">Event bus for score notifications</param>
        public RunnerScoreManager(IEventBus eventBus) : base(eventBus)
        {
            // Subscribe to runner-specific events
            eventBus.Subscribe<PlayerMovementEvent>(OnPlayerMoved);
            eventBus.Subscribe<CollectiblePickedUpEvent>(OnCollectiblePickedUp);
            eventBus.Subscribe<GameStartedEvent>(OnGameStarted);
            eventBus.Subscribe<GameEndedEvent>(OnGameEnded);
            eventBus.Subscribe<PlayerJumpEvent>(OnPlayerJumped);
            eventBus.Subscribe<PlayerSlideEvent>(OnPlayerSlid);
            
            Debug.Log("[RunnerScoreManager] ✅ Runner score manager initialized");
        }
        
        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// Load high score from PlayerPrefs
        /// </summary>
        protected override void LoadHighScore()
        {
            _highScore = PlayerPrefs.GetInt("RunnerHighScore", 0);
            Debug.Log($"[RunnerScoreManager] 📈 Loaded high score: {_highScore}");
        }
        
        /// <summary>
        /// Save high score to PlayerPrefs
        /// </summary>
        protected override void SaveHighScore()
        {
            PlayerPrefs.SetInt("RunnerHighScore", _highScore);
            PlayerPrefs.Save();
            Debug.Log($"[RunnerScoreManager] 💾 Saved high score: {_highScore}");
        }
        
        /// <summary>
        /// Calculate score with runner-specific logic
        /// </summary>
        /// <param name="basePoints">Base points to add</param>
        /// <param name="multiplier">Score multiplier</param>
        /// <returns>Calculated score</returns>
        protected override int CalculateScore(int basePoints, int multiplier = 1)
        {
            // Runner-specific score calculation with distance bonus
            var distanceBonus = Mathf.FloorToInt(_distanceTraveled * 0.1f);
            var calculatedScore = (basePoints + distanceBonus) * multiplier;
            
            Debug.Log($"[RunnerScoreManager] 🧮 Score calculation: Base={basePoints}, DistanceBonus={distanceBonus}, Multiplier={multiplier}, Final={calculatedScore}");
            
            return calculatedScore;
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Handle player movement for distance-based scoring
        /// </summary>
        private void OnPlayerMoved(PlayerMovementEvent movementEvent)
        {
            // Calculate distance traveled
            var currentPositionZ = movementEvent.Position.z;
            var distanceDelta = currentPositionZ - _lastPositionZ;
            
            if (distanceDelta > 0)
            {
                _distanceTraveled += distanceDelta;
                _lastPositionZ = currentPositionZ;
                
                // Calculate score based on distance
                var distanceScore = Mathf.FloorToInt(_distanceTraveled);
                SetScore(distanceScore);
                
//                Debug.Log($"[RunnerScoreManager] 📊 Distance score: {distanceScore} (Total distance: {_distanceTraveled:F1})");
            }
        }
        
        /// <summary>
        /// Handle collectible pickup for bonus points
        /// </summary>
        private void OnCollectiblePickedUp(CollectiblePickedUpEvent collectibleEvent)
        {
            // Add bonus points for collectibles
            AddScore(collectibleEvent.CollectibleValue);
            
            Debug.Log($"[RunnerScoreManager] 💰 Collected {collectibleEvent.CollectibleValue} points from collectible at {collectibleEvent.PickupPosition}");
        }
        
        /// <summary>
        /// Handle player jump for bonus points
        /// </summary>
        private void OnPlayerJumped(PlayerJumpEvent jumpEvent)
        {
            // Add bonus points for successful jumps
            var jumpBonus = Mathf.FloorToInt(jumpEvent.JumpForce * 0.5f);
            AddScore(jumpBonus);
            
            Debug.Log($"[RunnerScoreManager] 🦘 Jump bonus: {jumpBonus} points (Jump force: {jumpEvent.JumpForce})");
        }
        
        /// <summary>
        /// Handle player slide for bonus points
        /// </summary>
        private void OnPlayerSlid(PlayerSlideEvent slideEvent)
        {
            // Add bonus points for successful slides
            var slideBonus = Mathf.FloorToInt(slideEvent.SlideDuration * 10f);
            AddScore(slideBonus);
            
            Debug.Log($"[RunnerScoreManager] 🛷 Slide bonus: {slideBonus} points (Slide duration: {slideEvent.SlideDuration}s)");
        }
        
        /// <summary>
        /// Handle game started event
        /// </summary>
        private void OnGameStarted(GameStartedEvent gameStartedEvent)
        {
            ResetScore();
            _distanceTraveled = 0f;
            _lastPositionZ = 0f;
            
            Debug.Log("[RunnerScoreManager] 🎮 Score reset for new game");
        }
        
        /// <summary>
        /// Handle game ended event
        /// </summary>
        private void OnGameEnded(GameEndedEvent gameEndedEvent)
        {
            EndGame();
            
            Debug.Log($"[RunnerScoreManager] 🏁 Game ended with score: {_currentScore}");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Get distance traveled
        /// </summary>
        /// <returns>Distance traveled in units</returns>
        public float GetDistanceTraveled()
        {
            return _distanceTraveled;
        }
        
        /// <summary>
        /// Get runner-specific score statistics
        /// </summary>
        /// <returns>Runner score statistics string</returns>
        public override string GetScoreStats()
        {
            return $"Current: {_currentScore}, High: {_highScore}, Distance: {_distanceTraveled:F1}, Multiplier: {_scoreMultiplier}x, History: {_scoreHistory.Count}";
        }
        
        #endregion
    }
} 