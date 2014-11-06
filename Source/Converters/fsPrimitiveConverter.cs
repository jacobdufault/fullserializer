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

            if (instance is sbyte || instance is byte ||
                instance is Int16 || instance is UInt16 ||
                instance is Int32 || instance is UInt32 ||
                instance is Int64 || instance is UInt64) {
                serialized = new fsData((Int64)Convert.ChangeType(instance, typeof(Int64)));
                return fsFailure.Success;

            }

            if (instance is float || instance is double || instance is decimal) {
                serialized = new fsData((double)Convert.ChangeType(instance, typeof(double)));
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

            if (storage.IsDouble) {
                instance = Convert.ChangeType(storage.AsDouble, storageType);
                return fsFailure.Success;
            }

            if (storage.IsInt64) {
                instance = Convert.ChangeType(storage.AsInt64, storageType);
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

