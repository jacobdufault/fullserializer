using System;
using System.Collections;
using UnityEngine;

namespace FullJson {
    public class ReflectedConverter : ISerializationConverter {
        public JsonConverter Converter {
            get;
            set;
        }

        public bool CanProcess(Type type) {
            if (type.IsArray || typeof(ICollection).IsAssignableFrom(type)) {
                return false;
            }

            return true;
        }

        public JsonFailure TrySerialize(object instance, out JsonData serialized, Type storageType) {
            serialized = JsonData.CreateDictionary();

            MetaType metaType = MetaType.Get(instance.GetType());
            for (int i = 0; i < metaType.Properties.Length; ++i) {
                MetaProperty property = metaType.Properties[i];

                JsonData serializedData;

                var failed = Converter.TrySerialize(property.StorageType, property.Read(instance), out serializedData);
                if (failed.Failed) return failed;

                serialized.AsDictionary[property.Name] = serializedData;
            }

            return JsonFailure.Success;
        }

        public JsonFailure TryDeserialize(JsonData data, ref object instance, Type storageType) {
            if (data.IsDictionary == false) {
                return JsonFailure.Fail("Reflected converter requires a dictionary for data");
            }

            MetaType metaType = MetaType.Get(storageType);

            for (int i = 0; i < metaType.Properties.Length; ++i) {
                MetaProperty property = metaType.Properties[i];

                JsonData propertyData;
                if (data.AsDictionary.TryGetValue(property.Name, out propertyData)) {
                    object deserializedValue = null;
                    var failed = Converter.TryDeserialize(propertyData, property.StorageType, ref deserializedValue);
                    if (failed.Failed) return failed;

                    property.Write(instance, deserializedValue);
                }
                else {
                    Debug.LogWarning("No data for " + property.Name + " in " + data.PrettyJson);
                }
            }

            return JsonFailure.Success;
        }

        public object CreateInstance(JsonData data, Type storageType) {
            MetaType metaType = MetaType.Get(storageType);
            return metaType.CreateInstance();
        }
    }
}