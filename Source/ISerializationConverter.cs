using System;

namespace FullJson {
    /// <summary>
    /// The serialization converter allows for customization of the serialization process.
    /// </summary>
    public abstract class SerializationConverter {
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
        /// Serialize the actual object into the given data storage.
        /// </summary>
        /// <param name="instance">The object instance to serialize. This will never be null.</param>
        /// <param name="serialized">The serialized state.</param>
        /// <param name="storageType">The field/property type that is storing this instance.</param>
        /// <returns>If serialization was successful.</returns>
        public abstract JsonFailure TrySerialize(object instance, out JsonData serialized, Type storageType);

        /// <summary>
        /// Deserialize data into the object instance.
        /// </summary>
        /// <param name="storage">Serialization data to deserialize from.</param>
        /// <param name="instance">The object instance to deserialize into.</param>
        /// <param name="storageType">The field/property type that is storing the instance.</param>
        /// <returns>True if serialization was successful, false otherwise.</returns>
        public abstract JsonFailure TryDeserialize(JsonData data, ref object instance, Type storageType);

        /// <summary>
        /// Construct an object instance that will be passed to TryDeserialize. This should **not**
        /// deserialize the object.
        /// </summary>
        /// <param name="data">The data the object was serialized with.</param>
        /// <param name="storageType">The field/property type that is storing the instance.</param>
        /// <returns>An object instance</returns>
        public abstract object CreateInstance(JsonData data, Type storageType);
    }
}