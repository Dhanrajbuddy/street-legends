using System;
using System.Collections.Generic;

namespace StreetLegends.Core
{
    /// <summary>
    /// Minimal service locator populated once by the Bootstrap composition root.
    /// Deliberately small: no lazy factories, no scopes. Replace with a DI container only if registration grows unwieldy.
    /// </summary>
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public int Count => _services.Count;

        public void Register<T>(T instance) where T : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            Type key = typeof(T);
            if (_services.ContainsKey(key))
            {
                throw new InvalidOperationException($"Service {key.Name} is already registered.");
            }

            _services[key] = instance;
        }

        public T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out object instance))
            {
                return (T)instance;
            }

            throw new InvalidOperationException($"Service {typeof(T).Name} is not registered.");
        }

        public bool TryGet<T>(out T instance) where T : class
        {
            if (_services.TryGetValue(typeof(T), out object found))
            {
                instance = (T)found;
                return true;
            }

            instance = null;
            return false;
        }

        public void Clear()
        {
            _services.Clear();
        }
    }
}
