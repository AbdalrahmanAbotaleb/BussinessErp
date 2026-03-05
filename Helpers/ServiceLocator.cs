using System;
using System.Collections.Generic;

namespace BussinessErp.Helpers
{
    /// <summary>
    /// Simple service locator / DI container for resolving services.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private static readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// Register a singleton service instance.
        /// </summary>
        public static void Register<T>(T instance) where T : class
        {
            _services[typeof(T)] = instance;
        }

        /// <summary>
        /// Register a factory for lazy/transient resolution.
        /// </summary>
        public static void RegisterFactory<T>(Func<T> factory) where T : class
        {
            _factories[typeof(T)] = () => factory();
        }

        /// <summary>
        /// Resolve a registered service. Throws if not found.
        /// </summary>
        public static T Resolve<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out object instance))
                return (T)instance;

            if (_factories.TryGetValue(typeof(T), out Func<object> factory))
                return (T)factory();

            throw new InvalidOperationException($"Service {typeof(T).Name} is not registered.");
        }

        /// <summary>
        /// Try to resolve a service without throwing.
        /// </summary>
        public static bool TryResolve<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out object instance))
            {
                service = (T)instance;
                return true;
            }
            if (_factories.TryGetValue(typeof(T), out Func<object> factory))
            {
                service = (T)factory();
                return true;
            }
            service = null;
            return false;
        }

        /// <summary>
        /// Clear all registrations (for testing).
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
            _factories.Clear();
        }
    }
}
