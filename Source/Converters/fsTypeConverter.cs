using System;
using System.Collections;
using UnityEngine;

namespace FullSerializer.Internal {
    public class fsTypeConverter : fsConverter {
        public override bool CanProcess(Type type) {
            return typeof(Type).IsAssignableFrom(type);
        }

        public override bool RequestCycleSupport(Type type) {
            return false;
        }

        public override bool RequestInheritanceSupport(Type type) {
            return false;
        }

        public override fsFailure TrySerialize(object instance, out fsData serialized, Type storageType) {
            var type = (Type)instance;
            serialized = new fsData(type.FullName);
            return fsFailure.Success;
        }

        public override fsFailure TryDeserialize(fsData data, ref object instance, Type storageType) {
            if (data.IsString == false) {
                return fsFailure.Fail("Type converter requires a string");
            }

            instance = fsTypeLookup.GetType(data.AsString);
            if (instance == null) {
                return fsFailure.Fail("Unable to find type " + data.AsString);
            }
            return fsFailure.Success;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return storageType;
        }
    }
}