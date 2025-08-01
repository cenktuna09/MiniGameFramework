using System;
using UnityEngine;
using Core.Factories;
using EndlessRunner.Collectibles;

namespace EndlessRunner.Factories
{
    /// <summary>
    /// Factory for creating EndlessRunner collectibles.
    /// Extends framework's factory pattern for reusability.
    /// </summary>
    public class EndlessRunnerCollectibleFactory : BaseGameObjectFactory<CollectibleController>
    {
        #region Private Fields
        
        private readonly CollectibleType _collectibleType;
        private readonly int _pointValue;
        private readonly float _spawnChance;
        private readonly float _rotationSpeed;
        
        #endregion
        
        #region Constructor
        
        public EndlessRunnerCollectibleFactory(
            GameObject prefab, 
            CollectibleType collectibleType,
            int pointValue = 10,
            float spawnChance = 1f,
            float rotationSpeed = 90f,
            Transform parent = null,
            bool usePooling = true,
            int poolSize = 30) 
            : base(prefab, parent, usePooling, poolSize)
        {
            _collectibleType = collectibleType;
            _pointValue = pointValue;
            _spawnChance = spawnChance;
            _rotationSpeed = rotationSpeed;
            
            Debug.Log($"[EndlessRunnerCollectibleFactory] ✅ Factory created for {collectibleType}");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Create collectible with random spawn chance
        /// </summary>
        /// <param name="position">Position to spawn</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="parent">Parent transform</param>
        /// <returns>Created collectible or null if spawn chance failed</returns>
        public CollectibleController CreateWithChance(Vector3 position, Quaternion rotation = default, Transform parent = null)
        {
            if (UnityEngine.Random.Range(0f, 1f) > _spawnChance)
            {
                return null;
            }
            
            return Create(position, rotation, parent);
        }
        
        /// <summary>
        /// Create multiple collectibles
        /// </summary>
        /// <param name="positions">Array of positions</param>
        /// <param name="rotation">Rotation for all collectibles</param>
        /// <param name="parent">Parent transform</param>
        /// <returns>Array of created collectibles</returns>
        public CollectibleController[] CreateMultiple(Vector3[] positions, Quaternion rotation = default, Transform parent = null)
        {
            var collectibles = new CollectibleController[positions.Length];
            
            for (int i = 0; i < positions.Length; i++)
            {
                collectibles[i] = Create(positions[i], rotation, parent);
            }
            
            return collectibles;
        }
        
        /// <summary>
        /// Create collectible line (for coin trails)
        /// </summary>
        /// <param name="startPosition">Start position</param>
        /// <param name="endPosition">End position</param>
        /// <param name="spacing">Spacing between collectibles</param>
        /// <param name="parent">Parent transform</param>
        /// <returns>Array of created collectibles</returns>
        public CollectibleController[] CreateLine(Vector3 startPosition, Vector3 endPosition, float spacing = 2f, Transform parent = null)
        {
            var direction = (endPosition - startPosition).normalized;
            var distance = Vector3.Distance(startPosition, endPosition);
            var count = Mathf.FloorToInt(distance / spacing);
            
            var collectibles = new CollectibleController[count];
            
            for (int i = 0; i < count; i++)
            {
                var position = startPosition + direction * (spacing * i);
                collectibles[i] = Create(position, Quaternion.identity, parent);
            }
            
            return collectibles;
        }
        
        /// <summary>
        /// Get collectible type
        /// </summary>
        public CollectibleType GetCollectibleType()
        {
            return _collectibleType;
        }
        
        /// <summary>
        /// Get point value
        /// </summary>
        public int GetPointValue()
        {
            return _pointValue;
        }
        
        /// <summary>
        /// Get spawn chance
        /// </summary>
        public float GetSpawnChance()
        {
            return _spawnChance;
        }
        
        /// <summary>
        /// Get rotation speed
        /// </summary>
        public float GetRotationSpeed()
        {
            return _rotationSpeed;
        }
        
        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// Configure collectible with custom parameters
        /// </summary>
        protected override void ConfigureObject(CollectibleController collectible, object parameters)
        {
            if (parameters is CollectibleParameters collectibleParams)
            {
                collectible.SetPointValue(collectibleParams.PointValue);
                collectible.SetCollectibleType(collectibleParams.CollectibleType);
                collectible.SetSpawnChance(collectibleParams.SpawnChance);
                collectible.SetRotationSpeed(collectibleParams.RotationSpeed);
            }
        }
        
        /// <summary>
        /// Called when a new collectible is created
        /// </summary>
        protected override void OnObjectCreated(CollectibleController collectible)
        {
            if (collectible != null)
            {
                collectible.SetPointValue(_pointValue);
                collectible.SetCollectibleType(_collectibleType);
                collectible.SetSpawnChance(_spawnChance);
                collectible.SetRotationSpeed(_rotationSpeed);
                
                Debug.Log($"[EndlessRunnerCollectibleFactory] ✅ Collectible created: {_collectibleType} at {collectible.transform.position}");
            }
        }
        
        #endregion
        
        #region Nested Classes
        
        /// <summary>
        /// Parameters for collectible creation
        /// </summary>
        public class CollectibleParameters
        {
            public CollectibleType CollectibleType { get; set; }
            public int PointValue { get; set; }
            public float SpawnChance { get; set; }
            public float RotationSpeed { get; set; }
            
            public CollectibleParameters(CollectibleType collectibleType, int pointValue = 10, float spawnChance = 1f, float rotationSpeed = 90f)
            {
                CollectibleType = collectibleType;
                PointValue = pointValue;
                SpawnChance = spawnChance;
                RotationSpeed = rotationSpeed;
            }
        }
        
        #endregion
    }
} 