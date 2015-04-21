using System;
using System.Collections;

namespace FullSerializer.Internal {
    public class fsReflectedConverter : fsConverter {
        public override bool CanProcess(Type type) {
            if (type.Resolve().IsArray ||
                typeof(ICollection).IsAssignableFrom(type)) {

                return false;
            }

            return true;
        }

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
            serialized = fsData.CreateDictionary();
            var result = fsResult.Success;

            fsMetaType metaType = fsMetaType.Get(instance.GetType());
            metaType.EmitAotData();

            for (int i = 0; i < metaType.Properties.Length; ++i) {
                fsMetaProperty property = metaType.Properties[i];
                if (property.CanRead == false) continue;

                fsData serializedData;

                var itemResult = Serializer.TrySerialize(property.StorageType, property.Read(instance), out serializedData);
                result.AddMessages(itemResult);
                if (itemResult.Failed) {
                    continue;
                }

                serialized.AsDictionary[property.JsonName] = serializedData;
            }

            return result;
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
            var result = fsResult.Success;

            // Verify that we actually have an Object
            if ((result += CheckType(data, fsDataType.Object)).Failed) {
                return result;
            }

            fsMetaType metaType = fsMetaType.Get(storageType);
            metaType.EmitAotData();

            for (int i = 0; i < metaType.Properties.Length; ++i) {
                fsMetaProperty property = metaType.Properties[i];
                if (property.CanWrite == false) continue;

                fsData propertyData;
                if (data.AsDictionary.TryGetValue(property.JsonName, out propertyData)) {
                    object deserializedValue = null;

                    // We have to read in the existing value, since we need to support partial
                    // deserialization. However, this is bad for perf.
                    // TODO: Find a way to avoid this call when we are not doing a partial deserialization
                    //       Maybe through a new property, ie, Serializer.IsPartialSerialization, which just
                    //       gets set when starting a new serialization? We cannot pipe the information
                    //       through CreateInstance unfortunately.
                    if (property.CanRead) {
                        deserializedValue = property.Read(instance);
                    }

                    var itemResult = Serializer.TryDeserialize(propertyData, property.StorageType, ref deserializedValue);
                    result.AddMessages(itemResult);
                    if (itemResult.Failed) continue;

                    property.Write(instance, deserializedValue);
                }
            }

            return result;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            fsMetaType metaType = fsMetaType.Get(storageType);
            return metaType.CreateInstance();
        }
    }
}