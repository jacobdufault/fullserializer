using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace FullSerializer.Internal {
    public interface fsIEnumerableSerializationAdapter {
        /// <summary>
        /// Can this adapter be used for this object type?
        /// </summary>
        bool IsValid(Type objectType);

        /// <summary>
        /// Return an iterator to go over every item inside of the collection.
        /// </summary>
        IEnumerable Iterate(object collection);

        /// <summary>
        /// Attempt to fetch a hint size for the collection so we can allocate objects
        /// move efficiently. If no hint size can be computed, then this should return 0.
        /// </summary>
        int HintSize(object collection);

        /// <summary>
        /// Adds the given item to the collection.
        /// </summary>
        void Add(object collection, object item);

        /// <summary>
        /// Fetches the element type for objects inside of the collection.
        /// </summary>
        Type GetElementType(Type objectType);
    }

    public class fsIDictionaryAdapter : fsIEnumerableSerializationAdapter {
        // The mono dictionary implementations have lots of bugs w.r.t. iteration types. They don't
        // always return the type we expect, so we get around that by creating our own iteration
        // type that we convert everything to.
        public struct DictionaryItem {
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
                yield return new DictionaryItem() {
                    Key = enumerator.Key,
                    Value = enumerator.Value
                };
            }
        }

        public int HintSize(object collection) {
            var dict = (IDictionary)collection;
            return dict.Count;
        }

        public void Add(object collection, object item) {
            var dict = (IDictionary)collection;
            var pair = (DictionaryItem)item;

            // Because we're operating through the IDictionary interface by default (and not the
            // generic one), we normally send items through IDictionary.Add(object, object). This 
            // works fine in the general case, except that the add method verifies that it's
            // parameter types are proper types. However, mono is buggy and these type checks do
            // not consider null a subtype of the parameter types, and exceptions get thrown. So,
            // we have to special case adding null items via the generic functions (which do not
            // do the null check), which is slow and messy.
            //
            // An example of a collection that fails deserialization without this method is
            // `new SortedList<int, string> { { 0, null } }`. (SortedDictionary is fine because
            // it properly handles null values).
            if (pair.Key == null || pair.Value == null) {
                // Life would be much easier if we had MakeGenericType available, but we don't. So
                // we're going to find the correct generic KeyValuePair type via a bit of trickery.
                // All dictionaries extend ICollection<KeyValuePair<TKey, TValue>>, so we just
                // fetch the ICollection<> type with the proper generic arguments, and then we take
                // the KeyValuePair<> generic argument, and whola! we have our proper generic type.

                var collectionType = fsReflectionUtility.GetInterface(collection.GetType(), typeof(ICollection<>));
                if (collectionType != null) {
                    var keyValuePairType = collectionType.GetGenericArguments()[0];
                    object keyValueInstance = Activator.CreateInstance(keyValuePairType, pair.Key, pair.Value);
                    MethodInfo add = collectionType.GetFlattenedMethod("Add");
                    add.Invoke(collection, new object[] { keyValueInstance });
                    return;
                }
            }

            dict.Add(pair.Key, pair.Value);
        }

        public Type GetElementType(Type objectType) {
            return typeof(DictionaryItem);
        }
    }

    public class fsReflectedAdapter : fsIEnumerableSerializationAdapter {
        public bool IsValid(Type objectType) {
            return GetAddMethod(objectType) != null;
        }

        public IEnumerable Iterate(object collection) {
            return (IEnumerable)collection;
        }

        public int HintSize(object collection) {
            if (collection is ICollection) {
                return ((ICollection)collection).Count;
            }
            return 0;
        }

        public void Add(object collection, object item) {
            GetAddMethod(collection.GetType()).Invoke(collection, new object[] { item });
        }

        public Type GetElementType(Type objectType) {
            if (objectType.HasElementType) return objectType.GetElementType();

            Type enumerableList = fsReflectionUtility.GetInterface(objectType, typeof(IEnumerable<>));
            if (enumerableList != null) return enumerableList.GetGenericArguments()[0];

            return typeof(object);
        }

        private MethodInfo GetAddMethod(Type type) {
            // There is a really good chance the type will extend ICollection{}, which will contain
            // the add method we want. Just doing type.GetFlattenedMethod() may return the incorrect one --
            // for example, with dictionaries, it'll return Add(TKey, TValue), and we want
            // Add(KeyValuePair<TKey, TValue>).
            Type collectionInterface = fsReflectionUtility.GetInterface(type, typeof(ICollection<>));
            if (collectionInterface != null) {
                MethodInfo add = collectionInterface.GetDeclaredMethod("Add");
                if (add != null) return add;
            }

            // Otherwise try and look up a general Add method.
            return
                type.GetFlattenedMethod("Add") ??
                type.GetFlattenedMethod("Push") ??
                type.GetFlattenedMethod("Enqueue");
        }
    }

    public class fsIEnumerableConverter : fsConverter {
        private fsIEnumerableSerializationAdapter[] _adaptors = new fsIEnumerableSerializationAdapter[] {
            new fsIDictionaryAdapter(),
            new fsReflectedAdapter()
        };

        private fsIEnumerableSerializationAdapter GetAdapter(Type type) {
            for (int i = 0; i < _adaptors.Length; ++i) {
                if (_adaptors[i].IsValid(type)) return _adaptors[i];
            }

            throw new InvalidOperationException("No adapter found for " + type);
        }

        public override bool RequestCycleSupport(Type storageType) {
            return false;
        }

        public override bool RequestInheritanceSupport(Type storageType) {
            return false;
        }

        public override bool CanProcess(Type type) {
            if (typeof(IEnumerable).IsAssignableFrom(type) == false) return false;

            // Just because the type extends IEnumerable does not necessarily mean that
            // an adaptor can convert it. For example, user types can extend IEnumerable but
            // not have an Add method, which means that the reflected property editor will
            // need to be used.
            for (int i = 0; i < _adaptors.Length; ++i) {
                if (_adaptors[i].IsValid(type)) return true;
            }

            return false;
        }

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
            var result = fsResult.Success;

            fsIEnumerableSerializationAdapter adapter = GetAdapter(storageType);
            Type elementType = adapter.GetElementType(storageType);

            serialized = fsData.CreateList(adapter.HintSize(instance));
            var serializedList = serialized.AsList;

            foreach (object item in adapter.Iterate(instance)) {
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


            fsIEnumerableSerializationAdapter adapter = GetAdapter(storageType);

            Type elementType = adapter.GetElementType(storageType);

            var serializedList = data.AsList;
            for (int i = 0; i < serializedList.Count; ++i) {
                var serializedItem = serializedList[i];
                object deserialized = null;

                var itemResult = Serializer.TryDeserialize(serializedItem, elementType, ref deserialized);
                result.AddMessages(itemResult);
                if (itemResult.Failed) continue;

                adapter.Add(instance, deserialized);
            }

            return result;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return fsMetaType.Get(storageType).CreateInstance();
        }
    }
}