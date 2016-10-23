using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace FullSerializer.Internal {
    // While the generic IEnumerable converter can handle dictionaries, we
    // process them separately here because we support a few more advanced
    // use-cases with dictionaries, such as inline strings. Further, dictionary
    // processing in general is a bit more advanced because a few of the
    // collection implementations are buggy.
    public class fsDictionaryConverter : fsConverter {
        public override bool CanProcess(Type type) {
            return typeof(IDictionary).IsAssignableFrom(type);
        }

        public override object CreateInstance(fsData data, Type storageType) {
            return fsMetaType.Get(Serializer.Config, storageType).CreateInstance();
        }

        public override fsResult TryDeserialize(fsData data, ref object instance_, Type storageType) {
            var instance = (IDictionary)instance_;
            var result = fsResult.Success;

            Type keyStorageType, valueStorageType;
            GetKeyValueTypes(instance.GetType(), out keyStorageType, out valueStorageType);

            if (data.IsList) {
                var list = data.AsList;
                for (int i = 0; i < list.Count; ++i) {
                    var item = list[i];

                    fsData keyData, valueData;
                    if ((result += CheckType(item, fsDataType.Object)).Failed) return result;
                    if ((result += CheckKey(item, "Key", out keyData)).Failed) return result;
                    if ((result += CheckKey(item, "Value", out valueData)).Failed) return result;

                    object keyInstance = null, valueInstance = null;
                    if ((result += Serializer.TryDeserialize(keyData, keyStorageType, ref keyInstance)).Failed) return result;
                    if ((result += Serializer.TryDeserialize(valueData, valueStorageType, ref valueInstance)).Failed) return result;

                    AddItemToDictionary(instance, keyInstance, valueInstance);
                }
            }
            else if (data.IsDictionary) {
                foreach (var entry in data.AsDictionary) {
                    if (fsSerializer.IsReservedKeyword(entry.Key)) continue;

                    fsData keyData = new fsData(entry.Key), valueData = entry.Value;
                    object keyInstance = null, valueInstance = null;

                    if ((result += Serializer.TryDeserialize(keyData, keyStorageType, ref keyInstance)).Failed) return result;
                    if ((result += Serializer.TryDeserialize(valueData, valueStorageType, ref valueInstance)).Failed) return result;

                    AddItemToDictionary(instance, keyInstance, valueInstance);
                }
            }
            else {
                return FailExpectedType(data, fsDataType.Array, fsDataType.Object);
            }

            return result;
        }

        public override fsResult TrySerialize(object instance_, out fsData serialized, Type storageType) {
            serialized = fsData.Null;

            var result = fsResult.Success;

            var instance = (IDictionary)instance_;

            Type keyStorageType, valueStorageType;
            GetKeyValueTypes(instance.GetType(), out keyStorageType, out valueStorageType);

            // No other way to iterate dictionaries and still have access to the
            // key/value info
            IDictionaryEnumerator enumerator = instance.GetEnumerator();

            bool allStringKeys = true;
            var serializedKeys = new List<fsData>(instance.Count);
            var serializedValues = new List<fsData>(instance.Count);
            while (enumerator.MoveNext()) {
                fsData keyData, valueData;
                if ((result += Serializer.TrySerialize(keyStorageType, enumerator.Key, out keyData)).Failed) return result;
                if ((result += Serializer.TrySerialize(valueStorageType, enumerator.Value, out valueData)).Failed) return result;

                serializedKeys.Add(keyData);
                serializedValues.Add(valueData);

                allStringKeys &= keyData.IsString;
            }

            if (allStringKeys) {
                serialized = fsData.CreateDictionary();
                var serializedDictionary = serialized.AsDictionary;

                for (int i = 0; i < serializedKeys.Count; ++i) {
                    fsData key = serializedKeys[i];
                    fsData value = serializedValues[i];
                    serializedDictionary[key.AsString] = value;
                }
            }
            else {
                serialized = fsData.CreateList(serializedKeys.Count);
                var serializedList = serialized.AsList;

                for (int i = 0; i < serializedKeys.Count; ++i) {
                    fsData key = serializedKeys[i];
                    fsData value = serializedValues[i];

                    var container = new Dictionary<string, fsData>();
                    container["Key"] = key;
                    container["Value"] = value;
                    serializedList.Add(new fsData(container));
                }
            }

            return result;
        }

        private fsResult AddItemToDictionary(IDictionary dictionary, object key, object value) {
            // Because we're operating through the IDictionary interface by
            // default (and not the generic one), we normally send items through
            // IDictionary.Add(object, object). This works fine in the general
            // case, except that the add method verifies that it's parameter
            // types are proper types. However, mono is buggy and these type
            // checks do not consider null a subtype of the parameter types, and
            // exceptions get thrown. So, we have to special case adding null
            // items via the generic functions (which do not do the null check),
            // which is slow and messy.
            //
            // An example of a collection that fails deserialization without this
            // method is `new SortedList<int, string> { { 0, null } }`.
            // (SortedDictionary is fine because it properly handles null
            // values).
            if (key == null || value == null) {
                // Life would be much easier if we had MakeGenericType available,
                // but we don't. So we're going to find the correct generic
                // KeyValuePair type via a bit of trickery. All dictionaries
                // extend ICollection<KeyValuePair<TKey, TValue>>, so we just
                // fetch the ICollection<> type with the proper generic
                // arguments, and then we take the KeyValuePair<> generic
                // argument, and whola! we have our proper generic type.

                var collectionType = fsReflectionUtility.GetInterface(dictionary.GetType(), typeof(ICollection<>));
                if (collectionType == null) {
                    return fsResult.Warn(dictionary.GetType() + " does not extend ICollection");
                }

                var keyValuePairType = collectionType.GetGenericArguments()[0];
                object keyValueInstance = Activator.CreateInstance(keyValuePairType, key, value);
                MethodInfo add = collectionType.GetFlattenedMethod("Add");
                add.Invoke(dictionary, new object[] { keyValueInstance });
                return fsResult.Success;
            }

            // We use the inline set methods instead of dictionary.Add;
            // dictionary.Add will throw an exception if the key already exists.
            dictionary[key] = value;
            return fsResult.Success;
        }

        private static void GetKeyValueTypes(Type dictionaryType, out Type keyStorageType, out Type valueStorageType) {
            // All dictionaries extend IDictionary<TKey, TValue>, so we just
            // fetch the generic arguments from it
            var interfaceType = fsReflectionUtility.GetInterface(dictionaryType, typeof(IDictionary<,>));
            if (interfaceType != null) {
                var genericArgs = interfaceType.GetGenericArguments();
                keyStorageType = genericArgs[0];
                valueStorageType = genericArgs[1];
            }
            else {
                // Fetching IDictionary<,> failed... we have to encode full type
                // information :(
                keyStorageType = typeof(object);
                valueStorageType = typeof(object);
            }
        }
    }
}