using System;

namespace FullSerializer.Internal {
    /// <summary>
    /// Serializes and deserializes enums by their current name.
    /// </summary>
    public class fsEnumConverter : fsConverter {
        public override bool CanProcess(Type type) {
            return type.IsEnum;
        }

        public override fsFailure TrySerialize(object instance, out fsData serialized, Type storageType) {
            if (Attribute.IsDefined(storageType, typeof(FlagsAttribute))) {
                serialized = new fsData(Convert.ToInt32(instance));
            }
            else {
                serialized = new fsData(Enum.GetName(storageType, instance));
            }
            return fsFailure.Success;
        }

        public override fsFailure TryDeserialize(fsData data, ref object instance, Type storageType) {
            if (data.IsString) {
                string enumValue = data.AsString;
                instance = Enum.Parse(storageType, enumValue);
                return fsFailure.Success;
            }
            else if (data.IsFloat) {
                int enumValue = (int)data.AsFloat;
                instance = Enum.ToObject(storageType, enumValue);
                return fsFailure.Success;
            }

            return fsFailure.Fail("EnumConverter encountered an unknown JSON data type");
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return Enum.ToObject(storageType, 0);
        }
    }
}