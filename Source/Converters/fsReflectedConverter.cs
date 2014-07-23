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

        public override fsFailure TrySerialize(object instance, out fsData serialized, Type storageType) {
            serialized = fsData.CreateDictionary();

            fsMetaType metaType = fsMetaType.Get(instance.GetType());
            for (int i = 0; i < metaType.Properties.Length; ++i) {
                fsMetaProperty property = metaType.Properties[i];

                fsData serializedData;

                var failed = Serializer.TrySerialize(property.StorageType, property.Read(instance), out serializedData);
                if (failed.Failed) return failed;

                serialized.AsDictionary[property.Name] = serializedData;
            }

            return fsFailure.Success;
        }

        public override fsFailure TryDeserialize(fsData data, ref object instance, Type storageType) {
            if (data.IsDictionary == false) {
                return fsFailure.Fail("Reflected converter requires a dictionary for data");
            }

            fsMetaType metaType = fsMetaType.Get(storageType);

            for (int i = 0; i < metaType.Properties.Length; ++i) {
                fsMetaProperty property = metaType.Properties[i];

                fsData propertyData;
                if (data.AsDictionary.TryGetValue(property.Name, out propertyData)) {
                    object deserializedValue = null;
                    var failed = Serializer.TryDeserialize(propertyData, property.StorageType, ref deserializedValue);
                    if (failed.Failed) return failed;

                    property.Write(instance, deserializedValue);
                }
            }

            return fsFailure.Success;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            fsMetaType metaType = fsMetaType.Get(storageType);
            return metaType.CreateInstance();
        }
    }
}