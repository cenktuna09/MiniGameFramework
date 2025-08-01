using System;
using UnityEngine;
using Core.Events;
using Core.Architecture;
using Core.Common.StateManagement;
using EndlessRunner.Input;
using EndlessRunner.StateManagement;
using EndlessRunner.Events;
using EndlessRunner.Player;
using EndlessRunner.Scoring;

namespace EndlessRunner.Core
{
    /// <summary>
    /// Manages all event subscriptions and publishing for Endless Runner.
    /// Uses framework's Core.Events system for consistency.
    /// </summary>
    public class EndlessRunnerEventHandler
    {
        #region Private Fields
        
        private readonly IEventBus _eventBus;
        private readonly PlayerController _playerController;
        private readonly RunnerInputManager _inputManager;
        
        // Event subscriptions
        private System.IDisposable _gameStateSubscription;
        private System.IDisposable _playerDeathSubscription;
        private System.IDisposable _scoreUpdateSubscription;
        private System.IDisposable _collectibleCollectedSubscription;
        private System.IDisposable _obstacleCollisionSubscription;
        
        #endregion
        
        #region Events
        
        public event Action<StateChangedEvent<RunnerGameState>> OnGameStateChanged;
        public event Action<PlayerDeathEvent> OnPlayerDeath;
        public event Action<EndlessRunner.Events.ScoreChangedEvent> OnScoreUpdated;
        public event Action<CollectibleCollectedEvent> OnCollectibleCollected;
        public event Action<ObstacleCollisionEvent> OnObstacleCollision;
        
        #endregion
        
        #region Constructor
        
        public EndlessRunnerEventHandler(
            IEventBus eventBus,
            PlayerController playerController,
            RunnerInputManager inputManager)
        {
            _eventBus = eventBus;
            _playerController = playerController;
            _inputManager = inputManager;
            
            Debug.Log("[EndlessRunnerEventHandler] ✅ Event handler initialized");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Subscribe to all game events
        /// </summary>
        public void SubscribeToEvents()
        {
            Debug.Log("[EndlessRunnerEventHandler] 📡 Subscribing to game events...");
            
            try
            {
                // Subscribe to state changes
                _gameStateSubscription = _eventBus.Subscribe<StateChangedEvent<RunnerGameState>>(HandleGameStateChanged);
                
                // Subscribe to player events
                _playerDeathSubscription = _eventBus.Subscribe<PlayerDeathEvent>(HandlePlayerDeath);
                
                // Subscribe to score events
                _scoreUpdateSubscription = _eventBus.Subscribe<EndlessRunner.Events.ScoreChangedEvent>(HandleScoreUpdated);
                
                // Subscribe to collectible events
                _collectibleCollectedSubscription = _eventBus.Subscribe<CollectibleCollectedEvent>(HandleCollectibleCollected);
                
                // Subscribe to obstacle events
                _obstacleCollisionSubscription = _eventBus.Subscribe<ObstacleCollisionEvent>(HandleObstacleCollision);
                
                Debug.Log("[EndlessRunnerEventHandler] ✅ All event subscriptions created");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EndlessRunnerEventHandler] ❌ Failed to subscribe to events: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Unsubscribe from all events
        /// </summary>
        public void UnsubscribeFromEvents()
        {
            Debug.Log("[EndlessRunnerEventHandler] 📡 Unsubscribing from game events...");
            
            try
            {
                _gameStateSubscription?.Dispose();
                _playerDeathSubscription?.Dispose();
                _scoreUpdateSubscription?.Dispose();
                _collectibleCollectedSubscription?.Dispose();
                _obstacleCollisionSubscription?.Dispose();
                
                Debug.Log("[EndlessRunnerEventHandler] ✅ All event subscriptions disposed");
            }
            catch (Exception e)
            {
                Debug.LogError($"[EndlessRunnerEventHandler] ❌ Failed to unsubscribe from events: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Publish game started event
        /// </summary>
        public void PublishGameStarted()
        {
            var gameStartedEvent = new GameStartedEvent(Time.time);
            _eventBus?.Publish(gameStartedEvent);
            
            Debug.Log("[EndlessRunnerEventHandler] 🎮 Game started event published");
        }
        
        /// <summary>
        /// Publish game over event
        /// </summary>
        /// <param name="finalScore">Final score</param>
        /// <param name="gameOverReason">Reason for game over</param>
        public void PublishGameOver(int finalScore, string gameOverReason = "GameOver")
        {
            var gameOverEvent = new OnGameOverEvent("EndlessRunner", finalScore, gameOverReason, Time.time);
            _eventBus?.Publish(gameOverEvent);
            
            Debug.Log($"[EndlessRunnerEventHandler] 🏁 Game over event published: {finalScore} points, reason: {gameOverReason}");
        }
        
        /// <summary>
        /// Publish player movement event
        /// </summary>
        /// <param name="position">Player position</param>
        /// <param name="movementType">Type of movement</param>
        public void PublishPlayerMovement(Vector3 position, string movementType = "Forward")
        {
            var playerMovementEvent = new PlayerMovementEvent(position, Vector3.zero, 0f, 0f);
            _eventBus?.Publish(playerMovementEvent);
        }
        
        /// <summary>
        /// Publish score changed event
        /// </summary>
        /// <param name="newScore">New score</param>
        /// <param name="scoreChange">Score change amount</param>
        public void PublishScoreChanged(int newScore, int scoreChange)
        {
            var scoreChangedEvent = new EndlessRunner.Events.ScoreChangedEvent(newScore, scoreChange, 1);
            _eventBus?.Publish(scoreChangedEvent);
        }
        
        /// <summary>
        /// Check if event handler is subscribed to events
        /// </summary>
        /// <returns>True if subscribed, false otherwise</returns>
        public bool IsSubscribedToEvents()
        {
            return _gameStateSubscription != null &&
                   _playerDeathSubscription != null &&
                   _scoreUpdateSubscription != null &&
                   _collectibleCollectedSubscription != null &&
                   _obstacleCollisionSubscription != null;
        }
        
        #endregion
        
        #region Private Event Handlers
        
        /// <summary>
        /// Handle game state changes
        /// </summary>
        private void HandleGameStateChanged(StateChangedEvent<RunnerGameState> stateEvent)
        {
            Debug.Log($"[EndlessRunnerEventHandler] 🔄 State changed: {stateEvent.OldState} -> {stateEvent.NewState}");
            
            switch (stateEvent.NewState)
            {
                case RunnerGameState.Ready:
                    Debug.Log("[EndlessRunnerEventHandler] 🎯 Game ready to start");
                    break;
                    
                case RunnerGameState.Running:
                    Debug.Log("[EndlessRunnerEventHandler] 🏃 Game running");
                    break;
                    
                case RunnerGameState.Jumping:
                    Debug.Log("[EndlessRunnerEventHandler] 🦘 Player jumping");
                    break;
                    
                case RunnerGameState.Sliding:
                    Debug.Log("[EndlessRunnerEventHandler] 🛷 Player sliding");
                    break;
                    
                case RunnerGameState.Paused:
                    Debug.Log("[EndlessRunnerEventHandler] ⏸️ Game paused");
                    break;
                    
                case RunnerGameState.GameOver:
                    Debug.Log("[EndlessRunnerEventHandler] 💀 Game over");
                    break;
            }
            
            OnGameStateChanged?.Invoke(stateEvent);
        }
        
        /// <summary>
        /// Handle player death
        /// </summary>
        private void HandlePlayerDeath(PlayerDeathEvent deathEvent)
        {
            Debug.Log($"[EndlessRunnerEventHandler] 💀 Player died: {deathEvent.DeathCause}");
            
            // Lock input when player dies
            _inputManager?.LockInput();
            
            OnPlayerDeath?.Invoke(deathEvent);
        }
        
        /// <summary>
        /// Handle score updates
        /// </summary>
        private void HandleScoreUpdated(EndlessRunner.Events.ScoreChangedEvent scoreEvent)
        {
            Debug.Log($"[EndlessRunnerEventHandler] 📊 Score updated: {scoreEvent.NewScore} (+{scoreEvent.ScoreChange})");
            
            OnScoreUpdated?.Invoke(scoreEvent);
        }
        
        /// <summary>
        /// Handle collectible collection
        /// </summary>
        private void HandleCollectibleCollected(CollectibleCollectedEvent collectionEvent)
        {
            Debug.Log($"[EndlessRunnerEventHandler] 💰 Collectible collected: {collectionEvent.CollectibleType} at {collectionEvent.Position}");
            
            OnCollectibleCollected?.Invoke(collectionEvent);
        }
        
        /// <summary>
        /// Handle obstacle collision
        /// </summary>
        private void HandleObstacleCollision(ObstacleCollisionEvent collisionEvent)
        {
            Debug.Log($"[EndlessRunnerEventHandler] 💥 Obstacle collision: {collisionEvent.ObstacleType} at {collisionEvent.CollisionPoint}");
            
            OnObstacleCollision?.Invoke(collisionEvent);
        }
        
        #endregion
    }
} 