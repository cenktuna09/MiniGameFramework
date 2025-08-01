using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Factories;
using Core.DI;
using EndlessRunner.Obstacles;
using EndlessRunner.Collectibles;
using EndlessRunner.World;

namespace EndlessRunner.Factories
{
    /// <summary>
    /// Manages all EndlessRunner factories and coordinates object creation.
    /// Integrates with framework's ServiceLocator for extensibility.
    /// </summary>
    public class EndlessRunnerFactoryManager
    {
        #region Private Fields
        
        private readonly Dictionary<ObstacleType, EndlessRunnerObstacleFactory> _obstacleFactories;
        private readonly Dictionary<CollectibleType, EndlessRunnerCollectibleFactory> _collectibleFactories;
        private EndlessRunnerPlatformFactory _platformFactory;
        private readonly Transform _obstacleParent;
        private readonly Transform _collectibleParent;
        private readonly Transform _platformParent;
        
        #endregion
        
        #region Events
        
        public event Action<ObstacleController> OnObstacleCreated;
        public event Action<CollectibleController> OnCollectibleCreated;
        public event Action<EndlessRunnerPlatformController> OnPlatformCreated;
        
        #endregion
        
        #region Constructor
        
        public EndlessRunnerFactoryManager(
            Transform obstacleParent = null,
            Transform collectibleParent = null,
            Transform platformParent = null)
        {
            _obstacleFactories = new Dictionary<ObstacleType, EndlessRunnerObstacleFactory>();
            _collectibleFactories = new Dictionary<CollectibleType, EndlessRunnerCollectibleFactory>();
            _obstacleParent = obstacleParent;
            _collectibleParent = collectibleParent;
            _platformParent = platformParent;
            
            Debug.Log("[EndlessRunnerFactoryManager] ✅ Factory manager initialized");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Register obstacle factory
        /// </summary>
        /// <param name="obstacleType">Type of obstacle</param>
        /// <param name="prefab">Obstacle prefab</param>
        /// <param name="damageAmount">Damage amount</param>
        /// <param name="spawnChance">Spawn chance</param>
        public void RegisterObstacleFactory(ObstacleType obstacleType, GameObject prefab, float damageAmount = 1f, float spawnChance = 1f)
        {
            var factory = new EndlessRunnerObstacleFactory(
                prefab, 
                obstacleType, 
                damageAmount, 
                spawnChance, 
                _obstacleParent);
            
            _obstacleFactories[obstacleType] = factory;
            
            Debug.Log($"[EndlessRunnerFactoryManager] ✅ Registered obstacle factory: {obstacleType}");
        }
        
        /// <summary>
        /// Register collectible factory
        /// </summary>
        /// <param name="collectibleType">Type of collectible</param>
        /// <param name="prefab">Collectible prefab</param>
        /// <param name="pointValue">Point value</param>
        /// <param name="spawnChance">Spawn chance</param>
        /// <param name="rotationSpeed">Rotation speed</param>
        public void RegisterCollectibleFactory(CollectibleType collectibleType, GameObject prefab, int pointValue = 10, float spawnChance = 1f, float rotationSpeed = 90f)
        {
            var factory = new EndlessRunnerCollectibleFactory(
                prefab, 
                collectibleType, 
                pointValue, 
                spawnChance, 
                rotationSpeed, 
                _collectibleParent);
            
            _collectibleFactories[collectibleType] = factory;
            
            Debug.Log($"[EndlessRunnerFactoryManager] ✅ Registered collectible factory: {collectibleType}");
        }
        
        /// <summary>
        /// Register platform factory
        /// </summary>
        /// <param name="prefab">Platform prefab</param>
        /// <param name="platformLength">Platform length</param>
        /// <param name="platformWidth">Platform width</param>
        /// <param name="maxObstacles">Max obstacles</param>
        /// <param name="maxCollectibles">Max collectibles</param>
        public void RegisterPlatformFactory(GameObject prefab, float platformLength = 50f, float platformWidth = 10f, int maxObstacles = 5, int maxCollectibles = 10)
        {
            _platformFactory = new EndlessRunnerPlatformFactory(
                prefab, 
                platformLength, 
                platformWidth, 
                maxObstacles, 
                maxCollectibles, 
                false, 
                _platformParent);
            
            Debug.Log($"[EndlessRunnerFactoryManager] ✅ Registered platform factory");
        }
        
        /// <summary>
        /// Create obstacle
        /// </summary>
        /// <param name="obstacleType">Type of obstacle</param>
        /// <param name="position">Position to spawn</param>
        /// <param name="rotation">Rotation</param>
        /// <returns>Created obstacle or null</returns>
        public ObstacleController CreateObstacle(ObstacleType obstacleType, Vector3 position, Quaternion rotation = default)
        {
            if (!_obstacleFactories.TryGetValue(obstacleType, out var factory))
            {
                Debug.LogError($"[EndlessRunnerFactoryManager] ❌ No factory registered for obstacle type: {obstacleType}");
                return null;
            }
            
            var obstacle = factory.CreateWithChance(position, rotation);
            OnObstacleCreated?.Invoke(obstacle);
            
            return obstacle;
        }
        
        /// <summary>
        /// Create collectible
        /// </summary>
        /// <param name="collectibleType">Type of collectible</param>
        /// <param name="position">Position to spawn</param>
        /// <param name="rotation">Rotation</param>
        /// <returns>Created collectible or null</returns>
        public CollectibleController CreateCollectible(CollectibleType collectibleType, Vector3 position, Quaternion rotation = default)
        {
            if (!_collectibleFactories.TryGetValue(collectibleType, out var factory))
            {
                Debug.LogError($"[EndlessRunnerFactoryManager] ❌ No factory registered for collectible type: {collectibleType}");
                return null;
            }
            
            var collectible = factory.CreateWithChance(position, rotation);
            OnCollectibleCreated?.Invoke(collectible);
            
            return collectible;
        }
        
        /// <summary>
        /// Create platform
        /// </summary>
        /// <param name="position">Position to spawn</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="config">Platform configuration</param>
        /// <returns>Created platform or null</returns>
        public EndlessRunnerPlatformController CreatePlatform(Vector3 position, Quaternion rotation = default, EndlessRunnerPlatformFactory.PlatformConfig config = null)
        {
            if (_platformFactory == null)
            {
                Debug.LogError("[EndlessRunnerFactoryManager] ❌ Platform factory not registered");
                return null;
            }
            
            var platform = config != null ? 
                _platformFactory.CreateWithConfig(position, rotation, _platformParent, config) :
                _platformFactory.Create(position, rotation, _platformParent);
            
            OnPlatformCreated?.Invoke(platform);
            
            return platform;
        }
        
        /// <summary>
        /// Create multiple obstacles
        /// </summary>
        /// <param name="obstacleType">Type of obstacle</param>
        /// <param name="positions">Array of positions</param>
        /// <param name="rotation">Rotation for all obstacles</param>
        /// <returns>Array of created obstacles</returns>
        public ObstacleController[] CreateMultipleObstacles(ObstacleType obstacleType, Vector3[] positions, Quaternion rotation = default)
        {
            if (!_obstacleFactories.TryGetValue(obstacleType, out var factory))
            {
                Debug.LogError($"[EndlessRunnerFactoryManager] ❌ No factory registered for obstacle type: {obstacleType}");
                return new ObstacleController[0];
            }
            
            var obstacles = factory.CreateMultiple(positions, rotation, _obstacleParent);
            
            foreach (var obstacle in obstacles)
            {
                OnObstacleCreated?.Invoke(obstacle);
            }
            
            return obstacles;
        }
        
        /// <summary>
        /// Create multiple collectibles
        /// </summary>
        /// <param name="collectibleType">Type of collectible</param>
        /// <param name="positions">Array of positions</param>
        /// <param name="rotation">Rotation for all collectibles</param>
        /// <returns>Array of created collectibles</returns>
        public CollectibleController[] CreateMultipleCollectibles(CollectibleType collectibleType, Vector3[] positions, Quaternion rotation = default)
        {
            if (!_collectibleFactories.TryGetValue(collectibleType, out var factory))
            {
                Debug.LogError($"[EndlessRunnerFactoryManager] ❌ No factory registered for collectible type: {collectibleType}");
                return new CollectibleController[0];
            }
            
            var collectibles = factory.CreateMultiple(positions, rotation, _collectibleParent);
            
            foreach (var collectible in collectibles)
            {
                OnCollectibleCreated?.Invoke(collectible);
            }
            
            return collectibles;
        }
        
        /// <summary>
        /// Create collectible line
        /// </summary>
        /// <param name="collectibleType">Type of collectible</param>
        /// <param name="startPosition">Start position</param>
        /// <param name="endPosition">End position</param>
        /// <param name="spacing">Spacing between collectibles</param>
        /// <returns>Array of created collectibles</returns>
        public CollectibleController[] CreateCollectibleLine(CollectibleType collectibleType, Vector3 startPosition, Vector3 endPosition, float spacing = 2f)
        {
            if (!_collectibleFactories.TryGetValue(collectibleType, out var factory))
            {
                Debug.LogError($"[EndlessRunnerFactoryManager] ❌ No factory registered for collectible type: {collectibleType}");
                return new CollectibleController[0];
            }
            
            var collectibles = factory.CreateLine(startPosition, endPosition, spacing, _collectibleParent);
            
            foreach (var collectible in collectibles)
            {
                OnCollectibleCreated?.Invoke(collectible);
            }
            
            return collectibles;
        }
        
        /// <summary>
        /// Create platform chain
        /// </summary>
        /// <param name="startPosition">Start position</param>
        /// <param name="count">Number of platforms</param>
        /// <param name="spacing">Spacing between platforms</param>
        /// <returns>Array of created platforms</returns>
        public EndlessRunnerPlatformController[] CreatePlatformChain(Vector3 startPosition, int count, float spacing)
        {
            if (_platformFactory == null)
            {
                Debug.LogError("[EndlessRunnerFactoryManager] ❌ Platform factory not registered");
                return new EndlessRunnerPlatformController[0];
            }
            
            var platforms = _platformFactory.CreateChain(startPosition, count, spacing, _platformParent);
            
            foreach (var platform in platforms)
            {
                OnPlatformCreated?.Invoke(platform);
            }
            
            return platforms;
        }
        
        /// <summary>
        /// Get registered obstacle types
        /// </summary>
        /// <returns>Array of registered obstacle types</returns>
        public ObstacleType[] GetRegisteredObstacleTypes()
        {
            var types = new ObstacleType[_obstacleFactories.Count];
            _obstacleFactories.Keys.CopyTo(types, 0);
            return types;
        }
        
        /// <summary>
        /// Get registered collectible types
        /// </summary>
        /// <returns>Array of registered collectible types</returns>
        public CollectibleType[] GetRegisteredCollectibleTypes()
        {
            var types = new CollectibleType[_collectibleFactories.Count];
            _collectibleFactories.Keys.CopyTo(types, 0);
            return types;
        }
        
        /// <summary>
        /// Check if obstacle factory is registered
        /// </summary>
        /// <param name="obstacleType">Type to check</param>
        /// <returns>True if registered, false otherwise</returns>
        public bool IsObstacleFactoryRegistered(ObstacleType obstacleType)
        {
            return _obstacleFactories.ContainsKey(obstacleType);
        }
        
        /// <summary>
        /// Check if collectible factory is registered
        /// </summary>
        /// <param name="collectibleType">Type to check</param>
        /// <returns>True if registered, false otherwise</returns>
        public bool IsCollectibleFactoryRegistered(CollectibleType collectibleType)
        {
            return _collectibleFactories.ContainsKey(collectibleType);
        }
        
        /// <summary>
        /// Check if platform factory is registered
        /// </summary>
        /// <returns>True if registered, false otherwise</returns>
        public bool IsPlatformFactoryRegistered()
        {
            return _platformFactory != null;
        }
        
        #endregion
    }
} 