using System;
using System.Diagnostics;

namespace FullJson {
    /// <summary>
    /// Serialization of a type involves sending that type through a number of serialization
    /// converters.
    /// </summary>
    public class SerializationConverterChain {
        private ISerializationConverter[] _converters;

        [Conditional("DEBUG")]
        private static void VerifyNotReused(ISerializationConverter converter) {
            if (converter.Converters != null) {
                throw new InvalidOperationException("Converter " + converter + " is being reused");
            }
        }

        internal SerializationConverterChain(ISerializationConverter[] converters) {
            _converters = converters;

            for (int i = 0; i < _converters.Length; ++i) {
                VerifyNotReused(_converters[i]);
                _converters[i].Converters = this;
            }
        }

        public ISerializationConverter FirstConverter {
            get {
                return _converters[0];
            }
        }

        /// <summary>
        /// The current converter couldn't fully serialize the object; delegate serialization to the
        /// next converter.
        /// </summary>
        public JsonFailure TryDelegateSerialization(ISerializationConverter current,
            object instance, out JsonData serialized, Type storageType) {

            // Notice that we go to length - 1, so getting the i + 1 converter is fine.
            for (int i = 0; i < _converters.Length - 1; ++i) {
                if (_converters[i] == current) {
                    return _converters[i + 1].TrySerialize(instance, out serialized, storageType);
                }
            }

            serialized = null;
            return JsonFailure.Fail("No secondary delegate serializer for " + current);
        }

        public JsonFailure TryDelegateDeserialization(ISerializationConverter current, JsonData storage,
            ref object instance, Type storageType) {

            // Notice that we go to length - 1, so getting the i + 1 converter is fine.
            for (int i = 0; i < _converters.Length - 1; ++i) {
                if (_converters[i] == current) {
                    return _converters[i + 1].TryDeserialize(storage, ref instance, storageType);
                }
            }

            return JsonFailure.Fail("No secondary delegate deserializer for " + current);
        }
    }
}