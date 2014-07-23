using System;

namespace FullSerializer.Internal {
    public class fsPrimitiveConverter : fsConverter {
        public override bool CanProcess(Type type) {
            return
                type.Resolve().IsPrimitive ||
                type == typeof(string) ||
                type == typeof(decimal);
        }

        public override bool RequestCycleSupport(Type storageType) {
            return false;
        }

        public override bool RequestInheritanceSupport(Type storageType) {
            return false;
        }

        public override fsFailure TrySerialize(object instance, out fsData serialized, Type storageType) {
            if (instance is bool) {
                serialized = new fsData((bool)instance);
                return fsFailure.Success;
            }

            if (instance is byte ||
                instance is short || instance is ushort ||
                instance is int || instance is uint ||
                instance is long || instance is ulong ||
                instance is float || instance is double || instance is decimal) {

                serialized = new fsData((float)Convert.ChangeType(instance, typeof(float)));
                return fsFailure.Success;
            }

            if (instance is string || instance is char) {
                serialized = new fsData((string)Convert.ChangeType(instance, typeof(string)));
                return fsFailure.Success;
            }

            serialized = null;
            return fsFailure.Fail("Unhandled primitive type " + instance.GetType());
        }

        public override fsFailure TryDeserialize(fsData storage, ref object instance, Type storageType) {
            if (storage.IsBool) {
                instance = storage.AsBool;
                return fsFailure.Success;
            }

            if (storage.IsFloat) {
                instance = Convert.ChangeType(storage.AsFloat, storageType);
                return fsFailure.Success;
            }

            if (storage.IsString) {
                instance = storage.AsString;
                return fsFailure.Success;
            }

            return fsFailure.Fail("Bad JsonData " + storage);
        }
    }
}

