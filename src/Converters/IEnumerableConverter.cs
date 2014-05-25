using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace FullJson {
    public interface IEnumerableSerializationAdapter {
        bool IsValid(Type objectType);
        IEnumerable Iterate(object collection);
        void Add(object collection, object item);
        Type GetElementType(Type objectType);
    }

    public class IDictionaryAdapter : IEnumerableSerializationAdapter {
        // The mono dictionary implementations have lots of bugs w.r.t. iteration types. They don't
        // always return the type we expect, so we get around that by creating our own iteration
        // type that we convert everything to.
        public struct KeyValuePair {
            public object Key;
            public object Value;
        }

        public bool IsValid(Type objectType) {
            return typeof(IDictionary).IsAssignableFrom(objectType);
        }

        public IEnumerable Iterate(object collection) {
            var dict = (IDictionary)collection;

            IDictionaryEnumerator enumerator = dict.GetEnumerator();
            while (enumerator.MoveNext()) {
                yield return new KeyValuePair() {
                    Key = enumerator.Key,
                    Value = enumerator.Value
                };
            }
        }

        public void Add(object collection, object item) {
            var dict = (IDictionary)collection;
            var pair = (KeyValuePair)item;
            dict.Add(pair.Key, pair.Value);
        }

        public Type GetElementType(Type objectType) {
            return typeof(KeyValuePair);
        }
    }

    public class ReflectedAdapter : IEnumerableSerializationAdapter {
        public bool IsValid(Type objectType) {
            return true;
        }

        public IEnumerable Iterate(object collection) {
            return (IEnumerable)collection;
        }

        public void Add(object collection, object item) {
            GetAddMethod(collection.GetType()).Invoke(collection, new object[] { item });
        }

        public Type GetElementType(Type objectType) {
            if (objectType.HasElementType) return objectType.GetElementType();

            Type enumerableList = ReflectionUtilities.GetInterface(objectType, typeof(IEnumerable<>));
            if (enumerableList != null) return enumerableList.GetGenericArguments()[0];

            return typeof(object);
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
    }

    public class IEnumerableConverter : ISerializationConverter {
        private IEnumerableSerializationAdapter[] _adaptors = new IEnumerableSerializationAdapter[] {
            new IDictionaryAdapter(),
            new ReflectedAdapter()
        };

        private IEnumerableSerializationAdapter GetAdapter(Type type) {
            for (int i = 0; i < _adaptors.Length; ++i) {
                if (_adaptors[i].IsValid(type)) return _adaptors[i];
            }

            throw new InvalidOperationException("No adapter found for " + type);
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
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        public JsonFailure TrySerialize(object instance, out JsonData serialized, Type storageType) {
            serialized = JsonData.CreateList();

            IEnumerableSerializationAdapter adapter = GetAdapter(storageType);
            Type elementType = adapter.GetElementType(storageType);

            foreach (object item in adapter.Iterate(instance)) {
                JsonData serializedItem;

                var fail = Converter.TrySerialize(elementType, item, out serializedItem);
                if (fail.Failed) return fail;

                serialized.AsList.Add(serializedItem);
            }

            return JsonFailure.Success;
        }

        public JsonFailure TryDeserialize(JsonData data, ref object instance, Type storageType) {
            IEnumerableSerializationAdapter adapter = GetAdapter(storageType);

            Type elementType = adapter.GetElementType(storageType);

            var serializedList = data.AsList;
            for (int i = 0; i < serializedList.Count; ++i) {
                var serializedItem = serializedList[i];
                object deserialized = null;

                var fail = Converter.TryDeserialize(serializedItem, elementType, ref deserialized);
                if (fail.Failed) return fail;

                adapter.Add(instance, deserialized);
            }

            return JsonFailure.Success;
        }

        public object CreateInstance(JsonData data, Type storageType) {
            return MetaType.Get(storageType).CreateInstance();
        }
    }
}