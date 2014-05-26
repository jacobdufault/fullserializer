using System;

namespace FullJson {
    /// <summary>
    /// Serializes and deserializes enums by their current name.
    /// </summary>
    public class EnumConverter : ISerializationConverter {
        public JsonConverter Converter {
            get;
            set;
        }

        public bool CanProcess(Type type) {
            return type.IsEnum;
        }

        public JsonFailure TrySerialize(object instance, out JsonData serialized, Type storageType) {
            if (Attribute.IsDefined(storageType, typeof(FlagsAttribute))) {
                serialized = new JsonData(Convert.ToInt32(instance));
            }
            else {
                serialized = new JsonData(Enum.GetName(storageType, instance));
            }
            return JsonFailure.Success;
        }

        public JsonFailure TryDeserialize(JsonData data, ref object instance, Type storageType) {
            if (data.IsString) {
                string enumValue = data.AsString;
                instance = Enum.Parse(storageType, enumValue);
                return JsonFailure.Success;
            }
            else if (data.IsFloat) {
                int enumValue = (int)data.AsFloat;
                instance = Enum.ToObject(storageType, enumValue);
                return JsonFailure.Success;
            }

            return JsonFailure.Fail("EnumConverter encountered an unknown JSON data type");
        }

        public object CreateInstance(JsonData data, Type storageType) {
            return 0;
        }
    }
}