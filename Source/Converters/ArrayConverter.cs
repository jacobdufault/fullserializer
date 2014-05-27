using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FullJson.Internal {
    public class ArrayConverter : ISerializationConverter {
        public JsonConverter Converter {
            get;
            set;
        }

        public bool CanProcess(Type type) {
            return type.IsArray;
        }

        public JsonFailure TrySerialize(object instance, out JsonData serialized, Type storageType) {
            serialized = JsonData.CreateList();

            Array arr = (Array)instance;
            Type elementType = storageType.GetElementType();
            for (int i = 0; i < arr.Length; ++i) {
                object item = arr.GetValue(i);
                JsonData serializedItem;

                var fail = Converter.TrySerialize(elementType, item, out serializedItem);
                if (fail.Failed) return fail;

                serialized.AsList.Add(serializedItem);
            }

            return JsonFailure.Success;
        }

        public JsonFailure TryDeserialize(JsonData data, ref object instance, Type storageType) {
            Type elementType = storageType.GetElementType();

            var list = new ArrayList();
            var serializedList = data.AsList;
            for (int i = 0; i < serializedList.Count; ++i) {
                var serializedItem = serializedList[i];
                object deserialized = null;

                var fail = Converter.TryDeserialize(serializedItem, elementType, ref deserialized);
                if (fail.Failed) return fail;

                list.Add(deserialized);
            }

            instance = list.ToArray(elementType);
            return JsonFailure.Success;
        }

        public object CreateInstance(JsonData data, Type storageType) {
            return MetaType.Get(storageType).CreateInstance();
        }
    }
}