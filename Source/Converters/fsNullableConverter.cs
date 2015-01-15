using System;

namespace FullSerializer.Internal {
    /// <summary>
    /// The reflected converter will properly serialize nullable types. However, we do it here
    /// instead as we can emit less serialization data.
    /// </summary>
    public class fsNullableConverter : fsConverter {
        public override bool CanProcess(Type type) {
            return
                type.Resolve().IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
            // null is automatically serialized
            return Serializer.TrySerialize(Nullable.GetUnderlyingType(storageType), instance, out serialized);
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
            // null is automatically deserialized
            return Serializer.TryDeserialize(data, Nullable.GetUnderlyingType(storageType), ref instance);
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return storageType;
        }
    }
}