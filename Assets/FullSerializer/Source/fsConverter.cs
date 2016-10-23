using System;

namespace FullSerializer {
    /// <summary>
    /// The serialization converter allows for customization of the serialization
    /// process.
    /// </summary>
    public abstract class fsConverter : fsBaseConverter {
        /// <summary>
        /// Can this converter serialize and deserialize the given object type?
        /// </summary>
        /// <param name="type">The given object type.</param>
        /// <returns>
        /// True if the converter can serialize it, false otherwise.
        /// </returns>
        public abstract bool CanProcess(Type type);
    }
}