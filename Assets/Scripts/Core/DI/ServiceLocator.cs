using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Architecture;

namespace Core.DI
{
    /// <summary>
    /// Simple ServiceLocator pattern for dependency management.
    /// Provides a centralized way to register and resolve services.
    /// </summary>
    public class ServiceLocator
    {
        private static ServiceLocator instance;
        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Func<object>> factories = new Dictionary<Type, Func<object>>();
        
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
        
        /// <summary>
        /// Register a service instance.
        /// </summary>
        /// <typeparam name="T">The type of service to register.</typeparam>
        /// <param name="service">The service instance.</param>
        public void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            
            if (services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} is already registered. Overwriting.");
            }
            
            services[type] = service;
            Debug.Log($"[ServiceLocator] Registered service: {type.Name}");
        }
        
        /// <summary>
        /// Register a factory function for lazy instantiation.
        /// </summary>
        /// <typeparam name="T">The type of service to register.</typeparam>
        /// <param name="factory">The factory function to create the service.</param>
        public void Register<T>(Func<T> factory) where T : class
        {
            var type = typeof(T);
            
            if (factories.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Factory for type {type.Name} is already registered. Overwriting.");
            }
            
            factories[type] = () => factory();
            Debug.Log($"[ServiceLocator] Registered factory for: {type.Name}");
        }
        
        /// <summary>
        /// Resolve a service instance.
        /// </summary>
        /// <typeparam name="T">The type of service to resolve.</typeparam>
        /// <returns>The service instance, or null if not found.</returns>
        public T Resolve<T>() where T : class
        {
            var type = typeof(T);
            
            // Check if we have a direct instance
            if (services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            
            // Check if we have a factory
            if (factories.TryGetValue(type, out var factory))
            {
                var instance = factory();
                if (instance != null)
                {
                    // Cache the instance for future use
                    services[type] = instance;
                    Debug.Log($"[ServiceLocator] Created and cached service: {type.Name}");
                    return (T)instance;
                }
            }
            
            Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} not found.");
            return null;
        }
        
        /// <summary>
        /// Check if a service is registered.
        /// </summary>
        /// <typeparam name="T">The type of service to check.</typeparam>
        /// <returns>True if the service is registered, false otherwise.</returns>
        public bool IsRegistered<T>() where T : class
        {
            var type = typeof(T);
            return services.ContainsKey(type) || factories.ContainsKey(type);
        }
        
        /// <summary>
        /// Unregister a service.
        /// </summary>
        /// <typeparam name="T">The type of service to unregister.</typeparam>
        public void Unregister<T>() where T : class
        {
            var type = typeof(T);
            
            if (services.Remove(type))
            {
                Debug.Log($"[ServiceLocator] Unregistered service: {type.Name}");
            }
            
            if (factories.Remove(type))
            {
                Debug.Log($"[ServiceLocator] Unregistered factory: {type.Name}");
            }
        }
        
        /// <summary>
        /// Clear all registered services and factories.
        /// </summary>
        public void Clear()
        {
            services.Clear();
            factories.Clear();
            Debug.Log("[ServiceLocator] All services cleared.");
        }
        
        /// <summary>
        /// Get all registered service types.
        /// </summary>
        /// <returns>Array of registered service types.</returns>
        public Type[] GetRegisteredTypes()
        {
            var types = new List<Type>();
            types.AddRange(services.Keys);
            types.AddRange(factories.Keys);
            return types.ToArray();
        }
    }
    
    /// <summary>
    /// Extension methods for easier ServiceLocator usage.
    /// </summary>
    public static class ServiceLocatorExtensions
    {
        /// <summary>
        /// Register a service with automatic type inference.
        /// </summary>
        public static void Register<T>(this ServiceLocator locator, T service) where T : class
        {
            locator.Register(service);
        }
        
        /// <summary>
        /// Resolve a service with automatic type inference.
        /// </summary>
        public static T Resolve<T>(this ServiceLocator locator) where T : class
        {
            return locator.Resolve<T>();
        }
    }
}