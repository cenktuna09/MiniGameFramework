using System;
using UnityEngine;
using Core.Architecture;

namespace Core.Factories
{
    /// <summary>
    /// Generic factory interface for creating game objects.
    /// Extends framework's extensibility for all mini-games.
    /// </summary>
    /// <typeparam name="T">The type of object to create</typeparam>
    public interface IGameObjectFactory<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Create an object at the specified position
        /// </summary>
        /// <param name="position">Position to create the object</param>
        /// <param name="rotation">Rotation of the object</param>
        /// <param name="parent">Parent transform (optional)</param>
        /// <returns>The created object</returns>
        T Create(Vector3 position, Quaternion rotation = default, Transform parent = null);
        
        /// <summary>
        /// Create an object with custom parameters
        /// </summary>
        /// <param name="position">Position to create the object</param>
        /// <param name="rotation">Rotation of the object</param>
        /// <param name="parent">Parent transform (optional)</param>
        /// <param name="parameters">Custom creation parameters</param>
        /// <returns>The created object</returns>
        T Create(Vector3 position, Quaternion rotation, Transform parent, object parameters);
        
        /// <summary>
        /// Create an object from pool (if available)
        /// </summary>
        /// <param name="position">Position to create the object</param>
        /// <param name="rotation">Rotation of the object</param>
        /// <param name="parent">Parent transform (optional)</param>
        /// <returns>The created object</returns>
        T CreateFromPool(Vector3 position, Quaternion rotation = default, Transform parent = null);
        
        /// <summary>
        /// Return an object to pool
        /// </summary>
        /// <param name="obj">Object to return to pool</param>
        void ReturnToPool(T obj);
        
        /// <summary>
        /// Validate if the factory can create objects
        /// </summary>
        /// <returns>True if factory is valid, false otherwise</returns>
        bool IsValid();
        
        /// <summary>
        /// Get the prefab used by this factory
        /// </summary>
        /// <returns>The prefab GameObject</returns>
        GameObject GetPrefab();
    }
    
    /// <summary>
    /// Base factory implementation with common functionality.
    /// Provides framework-compatible object creation.
    /// </summary>
    /// <typeparam name="T">The type of object to create</typeparam>
    public abstract class BaseGameObjectFactory<T> : IGameObjectFactory<T> where T : MonoBehaviour
    {
        #region Protected Fields
        
        protected GameObject _prefab;
        protected Transform _parent;
        protected bool _usePooling;
        protected int _poolSize;
        
        #endregion
        
        #region Constructor
        
        protected BaseGameObjectFactory(GameObject prefab, Transform parent = null, bool usePooling = true, int poolSize = 10)
        {
            _prefab = prefab;
            _parent = parent;
            _usePooling = usePooling;
            _poolSize = poolSize;
            
            if (_usePooling)
            {
                InitializePool();
            }
        }
        
        #endregion
        
        #region IGameObjectFactory Implementation
        
        public virtual T Create(Vector3 position, Quaternion rotation = default, Transform parent = null)
        {
            if (!IsValid())
            {
                Debug.LogError($"[BaseGameObjectFactory] ❌ Factory is not valid for type {typeof(T).Name}");
                return null;
            }
            
            if (_usePooling)
            {
                return CreateFromPool(position, rotation, parent);
            }
            
            return CreateNew(position, rotation, parent);
        }
        
        public virtual T Create(Vector3 position, Quaternion rotation, Transform parent, object parameters)
        {
            var obj = Create(position, rotation, parent);
            if (obj != null)
            {
                ConfigureObject(obj, parameters);
            }
            return obj;
        }
        
        public virtual T CreateFromPool(Vector3 position, Quaternion rotation = default, Transform parent = null)
        {
            if (!_usePooling)
            {
                Debug.LogWarning($"[BaseGameObjectFactory] ⚠️ Pooling not enabled for {typeof(T).Name}");
                return CreateNew(position, rotation, parent);
            }
            
            // This should be overridden by derived classes to use actual pooling
            return CreateNew(position, rotation, parent);
        }
        
        public virtual void ReturnToPool(T obj)
        {
            if (!_usePooling || obj == null) return;
            
            // This should be overridden by derived classes to use actual pooling
            UnityEngine.Object.Destroy(obj.gameObject);
        }
        
        public virtual bool IsValid()
        {
            return _prefab != null;
        }
        
        public virtual GameObject GetPrefab()
        {
            return _prefab;
        }
        
        #endregion
        
        #region Protected Methods
        
        /// <summary>
        /// Create a new object instance
        /// </summary>
        protected virtual T CreateNew(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (_prefab == null)
            {
                Debug.LogError($"[BaseGameObjectFactory] ❌ Prefab is null for {typeof(T).Name}");
                return null;
            }
            
            var instance = UnityEngine.Object.Instantiate(_prefab, position, rotation, parent ?? _parent);
            var component = instance.GetComponent<T>();
            
            if (component == null)
            {
                Debug.LogError($"[BaseGameObjectFactory] ❌ Prefab does not have component {typeof(T).Name}");
                UnityEngine.Object.Destroy(instance);
                return null;
            }
            
            OnObjectCreated(component);
            return component;
        }
        
        /// <summary>
        /// Configure object with custom parameters
        /// </summary>
        protected virtual void ConfigureObject(T obj, object parameters)
        {
            // Override in derived classes to configure objects
        }
        
        /// <summary>
        /// Called when a new object is created
        /// </summary>
        protected virtual void OnObjectCreated(T obj)
        {
            // Override in derived classes for custom initialization
        }
        
        /// <summary>
        /// Initialize object pool
        /// </summary>
        protected virtual void InitializePool()
        {
            // Override in derived classes to implement actual pooling
        }
        
        #endregion
    }
} 