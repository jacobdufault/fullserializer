using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FullSerializer.Internal {
    public class fsArrayConverter : fsConverter {
        public override bool CanProcess(Type type) {
            return type.IsArray;
        }

        public override bool RequestCycleSupport(Type storageType) {
            return false;
        }

        public override bool RequestInheritanceSupport(Type storageType) {
            return false;
        }

        public override fsFailure TrySerialize(object instance, out fsData serialized, Type storageType) {
            serialized = fsData.CreateList();

            Array arr = (Array)instance;
            Type elementType = storageType.GetElementType();
            for (int i = 0; i < arr.Length; ++i) {
                object item = arr.GetValue(i);
                fsData serializedItem;

                var fail = Serializer.TrySerialize(elementType, item, out serializedItem);
                if (fail.Failed) return fail;

                serialized.AsList.Add(serializedItem);
            }

            return fsFailure.Success;
        }

        public override fsFailure TryDeserialize(fsData data, ref object instance, Type storageType) {
            Type elementType = storageType.GetElementType();

            var list = new ArrayList();
            var serializedList = data.AsList;
            for (int i = 0; i < serializedList.Count; ++i) {
                var serializedItem = serializedList[i];
                object deserialized = null;

                var fail = Serializer.TryDeserialize(serializedItem, elementType, ref deserialized);
                if (fail.Failed) return fail;

                list.Add(deserialized);
            }

            instance = list.ToArray(elementType);
            return fsFailure.Success;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return fsMetaType.Get(storageType).CreateInstance();
        }
    }
}