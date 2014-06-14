using System;

namespace FullSerializer.Internal {
    /// <summary>
    /// Serializes and deserializes guids.
    /// </summary>
    public class fsGuidConverter : fsConverter {
        public override bool CanProcess(Type type) {
            return type == typeof(Guid);
        }

        public override bool RequestCycleSupport(Type storageType) {
            return false;
        }

        public override bool RequestInheritanceSupport(Type storageType) {
            return false;
        }

        public override fsFailure TrySerialize(object instance, out fsData serialized, Type storageType) {
            var guid = (Guid)instance;
            serialized = new fsData(guid.ToString());
            return fsFailure.Success;
        }

        public override fsFailure TryDeserialize(fsData data, ref object instance, Type storageType) {
            if (data.IsString) {
                instance = new Guid(data.AsString);
                return fsFailure.Success;
            }

            return fsFailure.Fail("fsGuidConverter encountered an unknown JSON data type");
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return new Guid();
        }
    }
}