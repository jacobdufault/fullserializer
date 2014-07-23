using FullSerializer.Internal;
using System;

namespace FullSerializer {
    /// <summary>
    /// The serialization converter allows for customization of the serialization process.
    /// </summary>
    public abstract class fsConverter {
        /// <summary>
        /// The serializer that was owns this converter.
        /// </summary>
        public fsSerializer Serializer;

        /// <summary>
        /// Can this converter serialize and deserialize the given object type?
        /// </summary>
        /// <param name="type">The given object type.</param>
        /// <returns>True if the converter can serialize it, false otherwise.</returns>
        public abstract bool CanProcess(Type type);

        /// <summary>
        /// If true, then the serializer will support cyclic references with the given converted
        /// type.
        /// </summary>
        /// <param name="storageType">The field/property type that is currently storing the object
        /// that is being serialized.</param>
        public virtual bool RequestCycleSupport(Type storageType) {
            if (storageType == typeof(string)) return false;

            return storageType.Resolve().IsClass || storageType.Resolve().IsInterface;
        }

        /// <summary>
        /// If true, then the serializer will include inheritance data for the given converter.
        /// </summary>
        /// <param name="storageType">The field/property type that is currently storing the object
        /// that is being serialized.</param>
        public virtual bool RequestInheritanceSupport(Type storageType) {
            return storageType.Resolve().IsSealed == false;
        }

        /// <summary>
        /// Serialize the actual object into the given data storage.
        /// </summary>
        /// <param name="instance">The object instance to serialize. This will never be null.</param>
        /// <param name="serialized">The serialized state.</param>
        /// <param name="storageType">The field/property type that is storing this instance.</param>
        /// <returns>If serialization was successful.</returns>
        public abstract fsFailure TrySerialize(object instance, out fsData serialized, Type storageType);

        /// <summary>
        /// Deserialize data into the object instance.
        /// </summary>
        /// <param name="storage">Serialization data to deserialize from.</param>
        /// <param name="instance">The object instance to deserialize into.</param>
        /// <param name="storageType">The field/property type that is storing the instance.</param>
        /// <returns>True if serialization was successful, false otherwise.</returns>
        public abstract fsFailure TryDeserialize(fsData data, ref object instance, Type storageType);

        /// <summary>
        /// Construct an object instance that will be passed to TryDeserialize. This should **not**
        /// deserialize the object.
        /// </summary>
        /// <param name="data">The data the object was serialized with.</param>
        /// <param name="storageType">The field/property type that is storing the instance.</param>
        /// <returns>An object instance</returns>
        public virtual object CreateInstance(fsData data, Type storageType) {
            if (RequestCycleSupport(storageType)) {
                throw new InvalidOperationException("Please override CreateInstance for " +
                    this.GetType().FullName + "; the object graph for " + storageType +
                    " can contain potentially contain cycles, so separated instance creation " +
                    "is needed");
            }

            return storageType;
        }

        protected fsFailure CheckType(fsData data, fsDataType type) {
            if (data.Type != type) {
                return fsFailure.Fail("BoundsConverter expected " + type + " but got " + data.Type);
            }
            return fsFailure.Success;
        }

        protected fsFailure CheckKey(fsData data, string key, out fsData subitem) {
            if (data.AsDictionary.TryGetValue(key, out subitem) == false) {
                return fsFailure.Fail(GetType().Name + " requires a <" + key + "> key in the data " + data);
            }
            return fsFailure.Success;
        }
    }
}