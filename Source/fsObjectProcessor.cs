using System;

namespace FullSerializer {
    /// <summary>
    /// <para>
    /// Enables injecting code before/after an object has been serialized. This is most
    /// useful if you want to run the default serialization process but apply a pre/post
    /// processing step.
    /// </para>
    /// <para>
    /// Multiple object processors can be active at the same time. When running they are
    /// called in a "nested" fashion - if we have processor1 and process2 added to the
    /// serializer in that order (p1 then p2), then the execution order will be
    /// p1#Before p2#Before /serialization/ p2#After p1#After.
    /// </para>
    /// </summary>
    public abstract class fsObjectProcessor {
        /// <summary>
        /// Is the processor interested in objects of the given type?
        /// </summary>
        /// <param name="type">The given type.</param>
        /// <returns>True if the processor should be applied, false otherwise.</returns>
        public abstract bool CanProcess(Type type);

        /// <summary>
        /// Called before serialization.
        /// </summary>
        /// <param name="storageType">The field/property type that is storing the instance.</param>
        /// <param name="instance">The type of the instance.</param>
        public abstract void OnBeforeSerialize(Type storageType, object instance);

        /// <summary>
        /// Caled after serialization.
        /// </summary>
        /// <param name="storageType">The field/property type that is storing the instance.</param>
        /// <param name="instance">The type of the instance.</param>
        public abstract void OnAfterSerialize(Type storageType, object instance, ref fsData data);

        /// <summary>
        /// Caled before deserialization.
        /// </summary>
        /// <param name="storageType">The field/property type that is storing the instance.</param>
        /// <param name="data">The data that will be used for deserialization.</param>
        public abstract void OnBeforeDeserialize(Type storageType, ref fsData data);

        /// <summary>
        /// Caled after deserialization.
        /// </summary>
        /// <param name="storageType">The field/property type that is storing the instance.</param>
        /// <param name="instance">The type of the instance.</param>
        public abstract void OnAfterDeserialize(Type storageType, object instance);
    }
}