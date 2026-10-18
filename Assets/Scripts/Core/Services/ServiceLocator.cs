using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClawMachine.Core.Services
{
    /// <summary>
    /// Lightweight, zero-allocation service locator for decoupled runtime dependencies.
    /// Provides central access to core services (IEconomyService, ICollectionService, ISaveService, IAssetProvider, IAudioService).
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            services[typeof(T)] = service;
        }

        public static void Unregister<T>() where T : class
        {
            services.Remove(typeof(T));
        }

        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out var service))
            {
                return service as T;
            }

            Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} not found! Check registration order.");
            return null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            if (services.TryGetValue(type, out var obj))
            {
                service = obj as T;
                return service != null;
            }

            service = null;
            return false;
        }

        public static void Clear()
        {
            services.Clear();
        }
    }
}
