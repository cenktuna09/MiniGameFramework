using System;
using UnityEngine;
using Core.Factories;
using EndlessRunner.World;

namespace EndlessRunner.Factories
{
    /// <summary>
    /// Factory for creating EndlessRunner platforms.
    /// Extends framework's factory pattern for reusability.
    /// </summary>
    public class EndlessRunnerPlatformFactory : BaseGameObjectFactory<EndlessRunnerPlatformController>
    {
        #region Private Fields
        
        private readonly float _platformLength;
        private readonly float _platformWidth;
        private readonly int _maxObstacles;
        private readonly int _maxCollectibles;
        private readonly bool _skipFirstLoop;
        
        #endregion
        
        #region Constructor
        
        public EndlessRunnerPlatformFactory(
            GameObject prefab, 
            float platformLength = 50f,
            float platformWidth = 10f,
            int maxObstacles = 5,
            int maxCollectibles = 10,
            bool skipFirstLoop = false,
            Transform parent = null,
            bool usePooling = true,
            int poolSize = 10) 
            : base(prefab, parent, usePooling, poolSize)
        {
            _platformLength = platformLength;
            _platformWidth = platformWidth;
            _maxObstacles = maxObstacles;
            _maxCollectibles = maxCollectibles;
            _skipFirstLoop = skipFirstLoop;
            
            Debug.Log($"[EndlessRunnerPlatformFactory] ✅ Factory created with length: {platformLength}, width: {platformWidth}");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Create platform with specific configuration
        /// </summary>
        /// <param name="position">Position to spawn</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="parent">Parent transform</param>
        /// <param name="config">Platform configuration</param>
        /// <returns>Created platform</returns>
        public EndlessRunnerPlatformController CreateWithConfig(Vector3 position, Quaternion rotation, Transform parent, PlatformConfig config)
        {
            var platform = Create(position, rotation, parent);
            
            if (platform != null && config != null)
            {
                platform.SetPlatformLength(config.PlatformLength);
                platform.SetMaxObstacles(config.MaxObstacles);
                platform.SetMaxCollectibles(config.MaxCollectibles);
                platform.SetSkipFirstLoop(config.SkipFirstLoop);
            }
            
            return platform;
        }
        
        /// <summary>
        /// Create platform chain (for continuous world generation)
        /// </summary>
        /// <param name="startPosition">Start position</param>
        /// <param name="count">Number of platforms</param>
        /// <param name="spacing">Spacing between platforms</param>
        /// <param name="parent">Parent transform</param>
        /// <returns>Array of created platforms</returns>
        public EndlessRunnerPlatformController[] CreateChain(Vector3 startPosition, int count, float spacing, Transform parent = null)
        {
            var platforms = new EndlessRunnerPlatformController[count];
            
            for (int i = 0; i < count; i++)
            {
                var position = startPosition + Vector3.forward * (spacing * i);
                platforms[i] = Create(position, Quaternion.identity, parent);
            }
            
            return platforms;
        }
        
        /// <summary>
        /// Create platform with random obstacles and collectibles
        /// </summary>
        /// <param name="position">Position to spawn</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="parent">Parent transform</param>
        /// <param name="obstacleChance">Chance to spawn obstacles</param>
        /// <param name="collectibleChance">Chance to spawn collectibles</param>
        /// <returns>Created platform</returns>
        public EndlessRunnerPlatformController CreateWithRandomContent(
            Vector3 position, 
            Quaternion rotation, 
            Transform parent, 
            float obstacleChance = 0.7f, 
            float collectibleChance = 0.8f)
        {
            var platform = Create(position, rotation, parent);
            
            if (platform != null)
            {
                // Configure random content generation
                platform.SetObstacleSpawnChance(obstacleChance);
                platform.SetCollectibleSpawnChance(collectibleChance);
            }
            
            return platform;
        }
        
        /// <summary>
        /// Get platform length
        /// </summary>
        public float GetPlatformLength()
        {
            return _platformLength;
        }
        
        /// <summary>
        /// Get platform width
        /// </summary>
        public float GetPlatformWidth()
        {
            return _platformWidth;
        }
        
        /// <summary>
        /// Get max obstacles
        /// </summary>
        public int GetMaxObstacles()
        {
            return _maxObstacles;
        }
        
        /// <summary>
        /// Get max collectibles
        /// </summary>
        public int GetMaxCollectibles()
        {
            return _maxCollectibles;
        }
        
        /// <summary>
        /// Get skip first loop flag
        /// </summary>
        public bool GetSkipFirstLoop()
        {
            return _skipFirstLoop;
        }
        
        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// Configure platform with custom parameters
        /// </summary>
        protected override void ConfigureObject(EndlessRunnerPlatformController platform, object parameters)
        {
            if (parameters is PlatformConfig platformConfig)
            {
                platform.SetPlatformLength(platformConfig.PlatformLength);
                platform.SetMaxObstacles(platformConfig.MaxObstacles);
                platform.SetMaxCollectibles(platformConfig.MaxCollectibles);
                platform.SetSkipFirstLoop(platformConfig.SkipFirstLoop);
            }
        }
        
        /// <summary>
        /// Called when a new platform is created
        /// </summary>
        protected override void OnObjectCreated(EndlessRunnerPlatformController platform)
        {
            if (platform != null)
            {
                platform.SetPlatformLength(_platformLength);
                platform.SetMaxObstacles(_maxObstacles);
                platform.SetMaxCollectibles(_maxCollectibles);
                platform.SetSkipFirstLoop(_skipFirstLoop);
                
                Debug.Log($"[EndlessRunnerPlatformFactory] ✅ Platform created at {platform.transform.position}");
            }
        }
        
        #endregion
        
        #region Nested Classes
        
        /// <summary>
        /// Configuration for platform creation
        /// </summary>
        public class PlatformConfig
        {
            public float PlatformLength { get; set; }
            public float PlatformWidth { get; set; }
            public int MaxObstacles { get; set; }
            public int MaxCollectibles { get; set; }
            public bool SkipFirstLoop { get; set; }
            
            public PlatformConfig(float platformLength = 50f, float platformWidth = 10f, int maxObstacles = 5, int maxCollectibles = 10, bool skipFirstLoop = false)
            {
                PlatformLength = platformLength;
                PlatformWidth = platformWidth;
                MaxObstacles = maxObstacles;
                MaxCollectibles = maxCollectibles;
                SkipFirstLoop = skipFirstLoop;
            }
        }
        
        #endregion
    }
} 