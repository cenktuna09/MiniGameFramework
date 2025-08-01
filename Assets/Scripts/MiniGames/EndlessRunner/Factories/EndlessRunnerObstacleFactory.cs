using System;
using UnityEngine;
using Core.Factories;
using EndlessRunner.Obstacles;

namespace EndlessRunner.Factories
{
    /// <summary>
    /// Factory for creating EndlessRunner obstacles.
    /// Extends framework's factory pattern for reusability.
    /// </summary>
    public class EndlessRunnerObstacleFactory : BaseGameObjectFactory<ObstacleController>
    {
        #region Private Fields
        
        private readonly ObstacleType _obstacleType;
        private readonly float _damageAmount;
        private readonly float _spawnChance;
        
        #endregion
        
        #region Constructor
        
        public EndlessRunnerObstacleFactory(
            GameObject prefab, 
            ObstacleType obstacleType,
            float damageAmount = 1f,
            float spawnChance = 1f,
            Transform parent = null,
            bool usePooling = true,
            int poolSize = 20) 
            : base(prefab, parent, usePooling, poolSize)
        {
            _obstacleType = obstacleType;
            _damageAmount = damageAmount;
            _spawnChance = spawnChance;
            
            Debug.Log($"[EndlessRunnerObstacleFactory] ✅ Factory created for {obstacleType}");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Create obstacle with random spawn chance
        /// </summary>
        /// <param name="position">Position to spawn</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="parent">Parent transform</param>
        /// <returns>Created obstacle or null if spawn chance failed</returns>
        public ObstacleController CreateWithChance(Vector3 position, Quaternion rotation = default, Transform parent = null)
        {
            if (UnityEngine.Random.Range(0f, 1f) > _spawnChance)
            {
                return null;
            }
            
            return Create(position, rotation, parent);
        }
        
        /// <summary>
        /// Create multiple obstacles
        /// </summary>
        /// <param name="positions">Array of positions</param>
        /// <param name="rotation">Rotation for all obstacles</param>
        /// <param name="parent">Parent transform</param>
        /// <returns>Array of created obstacles</returns>
        public ObstacleController[] CreateMultiple(Vector3[] positions, Quaternion rotation = default, Transform parent = null)
        {
            var obstacles = new ObstacleController[positions.Length];
            
            for (int i = 0; i < positions.Length; i++)
            {
                obstacles[i] = Create(positions[i], rotation, parent);
            }
            
            return obstacles;
        }
        
        /// <summary>
        /// Get obstacle type
        /// </summary>
        public ObstacleType GetObstacleType()
        {
            return _obstacleType;
        }
        
        /// <summary>
        /// Get damage amount
        /// </summary>
        public float GetDamageAmount()
        {
            return _damageAmount;
        }
        
        /// <summary>
        /// Get spawn chance
        /// </summary>
        public float GetSpawnChance()
        {
            return _spawnChance;
        }
        
        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// Configure obstacle with custom parameters
        /// </summary>
        protected override void ConfigureObject(ObstacleController obstacle, object parameters)
        {
            if (parameters is ObstacleParameters obstacleParams)
            {
                obstacle.SetDamageAmount(obstacleParams.DamageAmount);
                obstacle.SetObstacleType(obstacleParams.ObstacleType);
                obstacle.SetSpawnChance(obstacleParams.SpawnChance);
            }
        }
        
        /// <summary>
        /// Called when a new obstacle is created
        /// </summary>
        protected override void OnObjectCreated(ObstacleController obstacle)
        {
            if (obstacle != null)
            {
                obstacle.SetDamageAmount(_damageAmount);
                obstacle.SetObstacleType(_obstacleType);
                obstacle.SetSpawnChance(_spawnChance);
                
                Debug.Log($"[EndlessRunnerObstacleFactory] ✅ Obstacle created: {_obstacleType} at {obstacle.transform.position}");
            }
        }
        
        #endregion
        
        #region Nested Classes
        
        /// <summary>
        /// Parameters for obstacle creation
        /// </summary>
        public class ObstacleParameters
        {
            public ObstacleType ObstacleType { get; set; }
            public float DamageAmount { get; set; }
            public float SpawnChance { get; set; }
            
            public ObstacleParameters(ObstacleType obstacleType, float damageAmount = 1f, float spawnChance = 1f)
            {
                ObstacleType = obstacleType;
                DamageAmount = damageAmount;
                SpawnChance = spawnChance;
            }
        }
        
        #endregion
    }
} 