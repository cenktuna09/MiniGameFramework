using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Common.Pooling
{
    /// <summary>
    /// Generic object pool for Unity GameObjects with automatic parent organization
    /// Provides efficient object reuse and memory management
    /// </summary>
    /// <typeparam name="T">Component type that extends MonoBehaviour</typeparam>
    public class ObjectPool<T> where T : MonoBehaviour
    {
        #region Private Fields
        
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly List<T> _activeObjects = new List<T>();
        private readonly Func<T> _createFunction;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onReturn;
        private readonly GameObject _poolParent;
        private readonly int _maxSize;
        private readonly string _poolName;
        
        #endregion
        
        #region Public Properties
        
        public int PooledCount => _pool.Count;
        public int ActiveCount => _activeObjects.Count;
        public int TotalCount => PooledCount + ActiveCount;
        public string PoolName => _poolName;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Create a new object pool
        /// </summary>
        /// <param name="poolName">Name for the pool parent GameObject</param>
        /// <param name="createFunction">Function to create new objects</param>
        /// <param name="maxSize">Maximum pool size</param>
        /// <param name="initialSize">Initial number of objects to create</param>
        /// <param name="onGet">Action to perform when getting object from pool</param>
        /// <param name="onReturn">Action to perform when returning object to pool</param>
        /// <param name="parentTransform">Parent transform for pool organization</param>
        public ObjectPool(
            string poolName,
            Func<T> createFunction,
            int maxSize = 50,
            int initialSize = 10,
            Action<T> onGet = null,
            Action<T> onReturn = null,
            Transform parentTransform = null)
        {
            _poolName = poolName;
            _createFunction = createFunction ?? throw new ArgumentNullException(nameof(createFunction));
            _maxSize = maxSize;
            _onGet = onGet;
            _onReturn = onReturn;
            
            // Create pool parent for organization
            _poolParent = new GameObject($"{poolName}_Pool");
            if (parentTransform != null)
            {
                _poolParent.transform.SetParent(parentTransform);
            }
            
            // Pre-populate pool
            for (int i = 0; i < initialSize; i++)
            {
                var obj = CreateNewObject();
                ReturnToPool(obj);
            }
            
            Debug.Log($"[ObjectPool<{typeof(T).Name}>] Created pool '{poolName}' with {initialSize} initial objects");
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Get an object from the pool
        /// </summary>
        /// <returns>Object from pool or newly created if pool is empty</returns>
        public T Get()
        {
            T obj;
            
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                obj = CreateNewObject();
            }
            
            // Activate and configure object
            obj.gameObject.SetActive(true);
            obj.transform.SetParent(null); // Remove from pool parent
            _activeObjects.Add(obj);
            
            // Execute custom get action
            _onGet?.Invoke(obj);
            
            return obj;
        }
        
        /// <summary>
        /// Return an object to the pool
        /// </summary>
        /// <param name="obj">Object to return</param>
        public void Return(T obj)
        {
            if (obj == null) return;
            
            // Remove from active list
            _activeObjects.Remove(obj);
            
            // Execute custom return action
            _onReturn?.Invoke(obj);
            
            // Deactivate and parent to pool
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_poolParent.transform);
            
            // Add to pool if under max size
            if (_pool.Count < _maxSize)
            {
                _pool.Enqueue(obj);
            }
            else
            {
                // Destroy excess objects
                UnityEngine.Object.Destroy(obj.gameObject);
            }
        }
        
        /// <summary>
        /// Return all active objects to the pool
        /// </summary>
        public void ReturnAll()
        {
            var activeList = new List<T>(_activeObjects);
            foreach (var obj in activeList)
            {
                Return(obj);
            }
        }
        
        /// <summary>
        /// Clear the entire pool and destroy all objects
        /// </summary>
        public void Clear()
        {
            // Return all active objects first
            ReturnAll();
            
            // Destroy all pooled objects
            while (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }
            
            // Destroy pool parent
            if (_poolParent != null)
            {
                UnityEngine.Object.Destroy(_poolParent);
            }
            
            Debug.Log($"[ObjectPool<{typeof(T).Name}>] Cleared pool '{_poolName}'");
        }
        
        /// <summary>
        /// Warm up the pool by creating additional objects
        /// </summary>
        /// <param name="count">Number of objects to create</param>
        public void WarmUp(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_pool.Count + _activeObjects.Count >= _maxSize) break;
                
                var obj = CreateNewObject();
                ReturnToPool(obj);
            }
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Create a new object using the create function
        /// </summary>
        /// <returns>Newly created object</returns>
        private T CreateNewObject()
        {
            var obj = _createFunction();
            if (obj == null)
            {
                throw new InvalidOperationException($"Create function returned null for pool '{_poolName}'");
            }
            
            return obj;
        }
        
        /// <summary>
        /// Internal method to return object to pool without activation changes
        /// </summary>
        /// <param name="obj">Object to return</param>
        private void ReturnToPool(T obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_poolParent.transform);
            _pool.Enqueue(obj);
        }
        
        #endregion
    }
}