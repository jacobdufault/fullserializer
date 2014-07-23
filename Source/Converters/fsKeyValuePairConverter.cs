using System;
using System.Collections.Generic;
using System.Reflection;

namespace FullSerializer.Internal {
    public class fsKeyValuePairConverter : fsConverter {
        public override bool CanProcess(Type type) {
            return
                type.Resolve().IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
        }

        public override bool RequestCycleSupport(Type storageType) {
            return false;
        }

        public override bool RequestInheritanceSupport(Type storageType) {
            return false;
        }

        public override fsFailure TryDeserialize(fsData data, ref object instance, Type storageType) {
            fsFailure failure;

            fsData keyData, valueData;
            if ((failure = CheckKey(data, "Key", out keyData)).Failed) return failure;
            if ((failure = CheckKey(data, "Value", out valueData)).Failed) return failure;

            var genericArguments = storageType.GetGenericArguments();
            Type keyType = genericArguments[0], valueType = genericArguments[1];

            object keyObject = null, valueObject = null;
            if ((failure = Serializer.TryDeserialize(keyData, keyType, ref keyObject)).Failed) return failure;
            if ((failure = Serializer.TryDeserialize(valueData, valueType, ref valueObject)).Failed) return failure;

            instance = Activator.CreateInstance(storageType, new object[] { keyObject, valueObject });
            return fsFailure.Success;
        }

        public override fsFailure TrySerialize(object instance, out fsData serialized, Type storageType) {
            serialized = null;

            PropertyInfo keyProperty = storageType.GetDeclaredProperty("Key");
            PropertyInfo valueProperty = storageType.GetDeclaredProperty("Value");

            object keyObject = keyProperty.GetValue(instance, null);
            object valueObject = valueProperty.GetValue(instance, null);

            var genericArguments = storageType.GetGenericArguments();
            Type keyType = genericArguments[0], valueType = genericArguments[1];

            fsFailure failure;

            fsData keyData, valueData;
            if ((failure = Serializer.TrySerialize(keyType, keyObject, out keyData)).Failed) return failure;
            if ((failure = Serializer.TrySerialize(valueType, valueObject, out valueData)).Failed) return failure;

            serialized = fsData.CreateDictionary();
            serialized.AsDictionary["Key"] = keyData;
            serialized.AsDictionary["Value"] = valueData;
            return fsFailure.Success;
        }
    }
}