using System;

namespace FullJson {
    /// <summary>
    /// The serialization converter allows for customization of the serialization process.
    /// </summary>
    public interface ISerializationConverter {
        /// <summary>
        /// The conversion chain. Use this to delegate / continue serialization for the current
        /// object being serialized.
        /// </summary>
        SerializationConverterChain Converters {
            get;
            set;
        }

        JsonConverter Converter {
            get;
            set;
        }

        /// <summary>
        /// Can this converter serialize and deserialize the given object type?
        /// </summary>
        /// <param name="type">The given object type.</param>
        /// <returns>True if the converter can serialize it, false otherwise.</returns>
        bool CanProcess(Type type);

        /// <summary>
        /// Serialize the actual object into the given data storage.
        /// </summary>
        /// <param name="instance">The object instance to serialize.</param>
        /// <param name="serialized">The serialized state.</param>
        /// <param name="storageType">The field/property type that is storing this instance.</param>
        /// <returns>If serialization was successful.</returns>
        JsonFailure TrySerialize(object instance, out JsonData serialized, Type storageType);

        /// <summary>
        /// Deserialize data into the object instance.
        /// </summary>
        /// <param name="storage">Serialization data to deserialize from.</param>
        /// <param name="instance">The object instance to deserialize into.</param>
        /// <param name="storageType">The field/property type that is storing the instance.</param>
        /// <returns>True if serialization was successful, false otherwise.</returns>
        JsonFailure TryDeserialize(JsonData data, ref object instance, Type storageType);

        /// <summary>
        /// Construct an object instance that will be passed to TryDeserialize. This should **not**
        /// deserialize the object.
        /// </summary>
        /// <param name="data">The data the object was serialized with.</param>
        /// <param name="storageType">The field/property type that is storing the instance.</param>
        /// <returns>An object instance</returns>
        object CreateInstance(JsonData data, Type storageType);
    }
}