using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Architecture;

namespace Core.DI
{
    /// <summary>
    /// Enhanced ServiceLocator pattern for dependency management.
    /// Supports both global and scene-scoped services with lifecycle management.
    /// </summary>
    public class ServiceLocator
    {
        private static ServiceLocator instance;
        private readonly Dictionary<Type, object> globalServices = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Func<object>> globalFactories = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, object> sceneServices = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Func<object>> sceneFactories = new Dictionary<Type, Func<object>>();
        
        /// <summary>
        /// Singleton instance of the ServiceLocator.
        /// </summary>
        public static ServiceLocator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ServiceLocator();
                }
                return instance;
            }
        }
        
        #region Global Services (Persistent across scenes)
        
        /// <summary>
        /// Register a global service instance that persists across scenes.
        /// </summary>
        /// <typeparam name="T">The type of service to register.</typeparam>
        /// <param name="service">The service instance.</param>
        public void RegisterGlobal<T>(T service) where T : class
        {
            var type = typeof(T);
            
            if (globalServices.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Global service of type {type.Name} is already registered. Overwriting.");
            }
            
            globalServices[type] = service;
            Debug.Log($"[ServiceLocator] Registered global service: {type.Name}");
        }
        
        /// <summary>
        /// Register a global factory function for lazy instantiation.
        /// </summary>
        /// <typeparam name="T">The type of service to register.</typeparam>
        /// <param name="factory">The factory function to create the service.</param>
        public void RegisterGlobal<T>(Func<T> factory) where T : class
        {
            var type = typeof(T);
            
            if (globalFactories.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Global factory for type {type.Name} is already registered. Overwriting.");
            }
            
            globalFactories[type] = () => factory();
            Debug.Log($"[ServiceLocator] Registered global factory for: {type.Name}");
        }
        
        #endregion
        
        #region Scene Services (Cleared on scene change)
        
        /// <summary>
        /// Register a scene-scoped service instance.
        /// </summary>
        /// <typeparam name="T">The type of service to register.</typeparam>
        /// <param name="service">The service instance.</param>
        public void RegisterScene<T>(T service) where T : class
        {
            var type = typeof(T);
            
            if (sceneServices.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Scene service of type {type.Name} is already registered. Overwriting.");
            }
            
            sceneServices[type] = service;
            Debug.Log($"[ServiceLocator] Registered scene service: {type.Name}");
        }
        
        /// <summary>
        /// Register a scene-scoped factory function for lazy instantiation.
        /// </summary>
        /// <typeparam name="T">The type of service to register.</typeparam>
        /// <param name="factory">The factory function to create the service.</param>
        public void RegisterScene<T>(Func<T> factory) where T : class
        {
            var type = typeof(T);
            
            if (sceneFactories.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Scene factory for type {type.Name} is already registered. Overwriting.");
            }
            
            sceneFactories[type] = () => factory();
            Debug.Log($"[ServiceLocator] Registered scene factory for: {type.Name}");
        }
        
        #endregion
        
        #region Legacy Support (Backward compatibility)
        
        /// <summary>
        /// Register a service instance (legacy method - registers as global).
        /// </summary>
        /// <typeparam name="T">The type of service to register.</typeparam>
        /// <param name="service">The service instance.</param>
        public void Register<T>(T service) where T : class
        {
            RegisterGlobal(service);
        }
        
        /// <summary>
        /// Register a factory function (legacy method - registers as global).
        /// </summary>
        /// <typeparam name="T">The type of service to register.</typeparam>
        /// <param name="factory">The factory function to create the service.</param>
        public void Register<T>(Func<T> factory) where T : class
        {
            RegisterGlobal(factory);
        }
        
        #endregion
        
        #region Resolution
        
        /// <summary>
        /// Resolve a service instance (checks scene first, then global).
        /// </summary>
        /// <typeparam name="T">The type of service to resolve.</typeparam>
        /// <returns>The service instance, or null if not found.</returns>
        public T Resolve<T>() where T : class
        {
            var type = typeof(T);
            
            // Check scene services first
            if (sceneServices.TryGetValue(type, out var sceneService))
            {
                return (T)sceneService;
            }
            
            // Check scene factories
            if (sceneFactories.TryGetValue(type, out var sceneFactory))
            {
                var instance = sceneFactory();
                if (instance != null)
                {
                    sceneServices[type] = instance;
                    Debug.Log($"[ServiceLocator] Created and cached scene service: {type.Name}");
                    return (T)instance;
                }
            }
            
            // Check global services
            if (globalServices.TryGetValue(type, out var globalService))
            {
                return (T)globalService;
            }
            
            // Check global factories
            if (globalFactories.TryGetValue(type, out var globalFactory))
            {
                var instance = globalFactory();
                if (instance != null)
                {
                    globalServices[type] = instance;
                    Debug.Log($"[ServiceLocator] Created and cached global service: {type.Name}");
                    return (T)instance;
                }
            }
            
            Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} not found in scene or global scope.");
            return null;
        }
        
        /// <summary>
        /// Resolve a service with fallback creation if not found.
        /// </summary>
        /// <typeparam name="T">The type of service to resolve.</typeparam>
        /// <param name="fallbackFactory">Factory to create service if not found.</param>
        /// <returns>The service instance.</returns>
        public T ResolveOrCreate<T>(Func<T> fallbackFactory) where T : class
        {
            var service = Resolve<T>();
            if (service == null)
            {
                service = fallbackFactory();
                RegisterScene(service); // Register as scene service
                Debug.Log($"[ServiceLocator] Created fallback service: {typeof(T).Name}");
            }
            return service;
        }
        
        #endregion
        
        #region Validation and Queries
        
        /// <summary>
        /// Check if a service is registered (scene or global).
        /// </summary>
        /// <typeparam name="T">The type of service to check.</typeparam>
        /// <returns>True if the service is registered, false otherwise.</returns>
        public bool IsRegistered<T>() where T : class
        {
            var type = typeof(T);
            return globalServices.ContainsKey(type) || globalFactories.ContainsKey(type) ||
                   sceneServices.ContainsKey(type) || sceneFactories.ContainsKey(type);
        }
        
        /// <summary>
        /// Check if a service is registered globally.
        /// </summary>
        /// <typeparam name="T">The type of service to check.</typeparam>
        /// <returns>True if the service is registered globally, false otherwise.</returns>
        public bool IsRegisteredGlobally<T>() where T : class
        {
            var type = typeof(T);
            return globalServices.ContainsKey(type) || globalFactories.ContainsKey(type);
        }
        
        /// <summary>
        /// Check if a service is registered in scene scope.
        /// </summary>
        /// <typeparam name="T">The type of service to check.</typeparam>
        /// <returns>True if the service is registered in scene scope, false otherwise.</returns>
        public bool IsRegisteredInScene<T>() where T : class
        {
            var type = typeof(T);
            return sceneServices.ContainsKey(type) || sceneFactories.ContainsKey(type);
        }
        
        #endregion
        
        #region Lifecycle Management
        
        /// <summary>
        /// Clear all scene-scoped services and factories.
        /// Call this when changing scenes.
        /// </summary>
        public void ClearSceneServices()
        {
            var sceneServiceCount = sceneServices.Count;
            var sceneFactoryCount = sceneFactories.Count;
            
            sceneServices.Clear();
            sceneFactories.Clear();
            
            Debug.Log($"[ServiceLocator] Cleared {sceneServiceCount} scene services and {sceneFactoryCount} scene factories.");
        }
        
        /// <summary>
        /// Clear all registered services and factories (global and scene).
        /// </summary>
        public void Clear()
        {
            var globalServiceCount = globalServices.Count;
            var globalFactoryCount = globalFactories.Count;
            var sceneServiceCount = sceneServices.Count;
            var sceneFactoryCount = sceneFactories.Count;
            
            globalServices.Clear();
            globalFactories.Clear();
            sceneServices.Clear();
            sceneFactories.Clear();
            
            Debug.Log($"[ServiceLocator] Cleared all services: {globalServiceCount} global services, {globalFactoryCount} global factories, {sceneServiceCount} scene services, {sceneFactoryCount} scene factories.");
        }
        
        /// <summary>
        /// Unregister a specific service (checks both global and scene).
        /// </summary>
        /// <typeparam name="T">The type of service to unregister.</typeparam>
        public void Unregister<T>() where T : class
        {
            var type = typeof(T);
            bool unregistered = false;
            
            if (globalServices.Remove(type))
            {
                Debug.Log($"[ServiceLocator] Unregistered global service: {type.Name}");
                unregistered = true;
            }
            
            if (globalFactories.Remove(type))
            {
                Debug.Log($"[ServiceLocator] Unregistered global factory: {type.Name}");
                unregistered = true;
            }
            
            if (sceneServices.Remove(type))
            {
                Debug.Log($"[ServiceLocator] Unregistered scene service: {type.Name}");
                unregistered = true;
            }
            
            if (sceneFactories.Remove(type))
            {
                Debug.Log($"[ServiceLocator] Unregistered scene factory: {type.Name}");
                unregistered = true;
            }
            
            if (!unregistered)
            {
                Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} was not found to unregister.");
            }
        }
        
        #endregion
        
        #region Diagnostics
        
        /// <summary>
        /// Get all registered service types (global and scene).
        /// </summary>
        /// <returns>Array of registered service types.</returns>
        public Type[] GetRegisteredTypes()
        {
            var types = new List<Type>();
            types.AddRange(globalServices.Keys);
            types.AddRange(globalFactories.Keys);
            types.AddRange(sceneServices.Keys);
            types.AddRange(sceneFactories.Keys);
            return types.ToArray();
        }
        
        /// <summary>
        /// Get registered service types by scope.
        /// </summary>
        /// <param name="includeGlobal">Include global services.</param>
        /// <param name="includeScene">Include scene services.</param>
        /// <returns>Array of registered service types.</returns>
        public Type[] GetRegisteredTypes(bool includeGlobal = true, bool includeScene = true)
        {
            var types = new List<Type>();
            
            if (includeGlobal)
            {
                types.AddRange(globalServices.Keys);
                types.AddRange(globalFactories.Keys);
            }
            
            if (includeScene)
            {
                types.AddRange(sceneServices.Keys);
                types.AddRange(sceneFactories.Keys);
            }
            
            return types.ToArray();
        }
        
        /// <summary>
        /// Get service registration statistics.
        /// </summary>
        /// <returns>Dictionary with registration statistics.</returns>
        public Dictionary<string, int> GetStatistics()
        {
            return new Dictionary<string, int>
            {
                ["Global Services"] = globalServices.Count,
                ["Global Factories"] = globalFactories.Count,
                ["Scene Services"] = sceneServices.Count,
                ["Scene Factories"] = sceneFactories.Count,
                ["Total"] = globalServices.Count + globalFactories.Count + sceneServices.Count + sceneFactories.Count
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// Extension methods for easier ServiceLocator usage.
    /// </summary>
    public static class ServiceLocatorExtensions
    {
        /// <summary>
        /// Register a global service with automatic type inference.
        /// </summary>
        public static void RegisterGlobal<T>(this ServiceLocator locator, T service) where T : class
        {
            locator.RegisterGlobal(service);
        }
        
        /// <summary>
        /// Register a scene service with automatic type inference.
        /// </summary>
        public static void RegisterScene<T>(this ServiceLocator locator, T service) where T : class
        {
            locator.RegisterScene(service);
        }
        
        /// <summary>
        /// Resolve a service with automatic type inference.
        /// </summary>
        public static T Resolve<T>(this ServiceLocator locator) where T : class
        {
            return locator.Resolve<T>();
        }
        
        /// <summary>
        /// Resolve or create a service with automatic type inference.
        /// </summary>
        public static T ResolveOrCreate<T>(this ServiceLocator locator, Func<T> fallbackFactory) where T : class
        {
            return locator.ResolveOrCreate(fallbackFactory);
        }
    }
}