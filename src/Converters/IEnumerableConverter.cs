using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FullJson {
    public class IEnumerableConverter : ISerializationConverter {
        private static Type GetElementType(Type type) {
            if (type.HasElementType) return type.GetElementType();

            Type enumerableList = ReflectionUtilities.GetInterface(type, typeof(IEnumerable<>));
            if (enumerableList != null) return enumerableList.GetGenericArguments()[0];

            return typeof(object);
        }

        public SerializationConverterChain Converters {
            get;
            set;
        }

        public JsonConverter Converter {
            get;
            set;
        }

        public bool CanProcess(Type type) {
            return typeof(IEnumerable).IsAssignableFrom(type) && GetAddMethod(type) != null;
        }

        public JsonFailure TrySerialize(object instance, out JsonData serialized, Type storageType) {
            serialized = JsonData.CreateList();

            IEnumerable enumerator = (IEnumerable)instance;
            Type elementType = GetElementType(storageType);

            foreach (object item in enumerator) {
                JsonData serializedItem;

                var fail = Converter.TrySerialize(elementType, item, out serializedItem);
                if (fail.Failed) return fail;

                serialized.AsList.Add(serializedItem);
            }

            return JsonFailure.Success;
        }

        public JsonFailure TryDeserialize(JsonData data, ref object instance, Type storageType) {
            Type elementType = GetElementType(storageType);

            var serializedList = data.AsList;
            for (int i = 0; i < serializedList.Count; ++i) {
                var serializedItem = serializedList[i];
                object deserialized = null;

                var fail = Converter.TryDeserialize(serializedItem, elementType, ref deserialized);
                if (fail.Failed) return fail;

                AddItem(instance, deserialized);
            }

            return JsonFailure.Success;
        }

        private MethodInfo GetAddMethod(Type type) {
            // There is a really good chance the type will extend ICollection{}, which will contain
            // the add method we want. Just doing type.GetMethod() may return the incorrect one --
            // for example, with dictionaries, it'll return Add(TKey, TValue), and we want
            // Add(KeyValuePair<TKey, TValue>).
            Type collectionInterface = ReflectionUtilities.GetInterface(type, typeof(ICollection<>));
            if (collectionInterface != null) {
                MethodInfo add = collectionInterface.GetMethod("Add");
                if (add != null) return add;
            }

            // Otherwise try and look up a general Add method.
            return type.GetMethod("Add");
        }

        private void AddItem(object list, object item) {
            GetAddMethod(list.GetType()).Invoke(list, new object[] { item });
        }

        public object CreateInstance(JsonData data, Type storageType) {
            return MetaType.Get(storageType).CreateInstance();
        }
    }
}