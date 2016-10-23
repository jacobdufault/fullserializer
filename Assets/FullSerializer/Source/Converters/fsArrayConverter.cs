using System;
using System.Collections;

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

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
            // note: IList[index] is **significantly** faster than Array.Get, so
            //       make sure we use that instead.

            IList arr = (Array)instance;
            Type elementType = storageType.GetElementType();

            var result = fsResult.Success;

            serialized = fsData.CreateList(arr.Count);
            var serializedList = serialized.AsList;

            for (int i = 0; i < arr.Count; ++i) {
                object item = arr[i];

                fsData serializedItem;

                var itemResult = Serializer.TrySerialize(elementType, item, out serializedItem);
                result.AddMessages(itemResult);
                if (itemResult.Failed) continue;

                serializedList.Add(serializedItem);
            }

            return result;
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
            var result = fsResult.Success;

            // Verify that we actually have an List
            if ((result += CheckType(data, fsDataType.Array)).Failed) {
                return result;
            }

            Type elementType = storageType.GetElementType();

            var serializedList = data.AsList;
            var list = new ArrayList(serializedList.Count);
            int existingCount = list.Count;

            for (int i = 0; i < serializedList.Count; ++i) {
                var serializedItem = serializedList[i];
                object deserialized = null;
                if (i < existingCount) deserialized = list[i];

                var itemResult = Serializer.TryDeserialize(serializedItem, elementType, ref deserialized);
                result.AddMessages(itemResult);
                if (itemResult.Failed) continue;

                if (i < existingCount) list[i] = deserialized;
                else list.Add(deserialized);
            }

            instance = list.ToArray(elementType);
            return result;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return fsMetaType.Get(Serializer.Config, storageType).CreateInstance();
        }
    }
}