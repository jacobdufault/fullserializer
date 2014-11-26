using System;
using System.Collections.Generic;

namespace FullSerializer {
    /// <summary>
    /// fsContext stores global metadata that can be used to customize how fsConverters operate
    /// during serialization.
    /// </summary>
    public sealed class fsContext {
        /// <summary>
        /// All of the context objects.
        /// </summary>
        private readonly Dictionary<Type, object> _contextObjects = new Dictionary<Type, object>();

        /// <summary>
        /// Removes all context objects from the context.
        /// </summary>
        public void Reset() {
            _contextObjects.Clear();
        }

        /// <summary>
        /// Sets the context object for the given type with the given value.
        /// </summary>
        public void Set<T>(T obj) {
            _contextObjects[typeof(T)] = obj;
        }

        /// <summary>
        /// Returns true if there is a context object for the given type.
        /// </summary>
        public bool Has<T>() {
            return _contextObjects.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Fetches the context object for the given type.
        /// </summary>
        public T Get<T>() {
            object val;
            if (_contextObjects.TryGetValue(typeof(T), out val)) {
                return (T)val;
            }
            throw new InvalidOperationException("There is no context object of type " + typeof(T));
        }
    }
}