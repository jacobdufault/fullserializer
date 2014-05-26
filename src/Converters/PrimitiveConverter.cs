using System;

namespace FullJson.Internal {
    public class PrimitiveConverter : ISerializationConverter {
        public JsonConverter Converter {
            get;
            set;
        }

        public bool CanProcess(Type type) {
            return type.IsPrimitive || type == typeof(string) || type == typeof(decimal);
        }

        public JsonFailure TrySerialize(object instance, out JsonData serialized, Type storageType) {
            if (instance is bool) {
                serialized = new JsonData((bool)instance);
                return JsonFailure.Success;
            }

            if (instance is byte ||
                instance is short || instance is ushort ||
                instance is int || instance is uint ||
                instance is long || instance is ulong ||
                instance is float || instance is double || instance is decimal) {

                serialized = new JsonData((float)Convert.ChangeType(instance, typeof(float)));
                return JsonFailure.Success;
            }

            if (instance is string || instance is char) {
                serialized = new JsonData((string)Convert.ChangeType(instance, typeof(string)));
                return JsonFailure.Success;
            }

            serialized = null;
            return JsonFailure.Fail("Unhandled primitive type " + instance.GetType());
        }

        public JsonFailure TryDeserialize(JsonData storage, ref object instance, Type storageType) {
            if (storage.IsBool) {
                instance = storage.AsBool;
                return JsonFailure.Success;
            }

            if (storage.IsFloat) {
                instance = Convert.ChangeType(storage.AsFloat, storageType);
                return JsonFailure.Success;
            }

            if (storage.IsString) {
                instance = storage.AsString;
                return JsonFailure.Success;
            }

            return JsonFailure.Fail("Bad JsonData " + storage);
        }

        public object CreateInstance(JsonData data, Type storageType) {
            if (storageType == typeof(string)) {
                return string.Empty;
            }

            return Activator.CreateInstance(storageType);
        }
    }
}