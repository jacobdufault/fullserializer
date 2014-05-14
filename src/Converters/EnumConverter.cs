using System;

namespace FullJson {
    /// <summary>
    /// Serializes and deserializes enums by their current name.
    /// </summary>
    public class EnumConverter : ISerializationConverter {
        public SerializationConverterChain Converters {
            get;
            set;
        }

        public JsonConverter Converter {
            get;
            set;
        }

        public bool CanProcess(Type type) {
            return type.IsEnum;
        }

        public JsonFailure TrySerialize(object instance, out JsonData serialized, Type storageType) {
            serialized = new JsonData(Enum.GetName(storageType, instance));
            return JsonFailure.Success;
        }

        public JsonFailure TryDeserialize(JsonData data, ref object instance, Type storageType) {
            if (data.IsString == false) return JsonFailure.Fail("Enum converter can only deserialize strings");

            string enumValue = data.AsString;
            instance = Enum.Parse(storageType, enumValue);

            return JsonFailure.Success;
        }

        public object CreateInstance(JsonData data, Type storageType) {
            return 0;
        }
    }
}