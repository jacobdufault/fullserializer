using System;
using System.Collections.Generic;

namespace FullSerializer {
    /// <summary>
    /// The direct converter is similar to a regular converter, except that it targets specifically only one type.
    /// This means that it can be used without performance impact when discovering converters. It is strongly
    /// recommended that you derive from fsDirectConverter{TModel}.
    /// </summary>
    /// <remarks>Due to the way that direct converters operate, inheritance is *not* supported. Direct converters
    /// will only be used with the exact ModelType object.</remarks>
    public abstract class fsDirectConverter : fsBaseConverter {
        public abstract Type ModelType { get; }
    }

    public abstract class fsDirectConverter<TModel> : fsDirectConverter {
        public override Type ModelType { get { return typeof(TModel); } }

        public sealed override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
            var serializedDictionary = new Dictionary<string, fsData>();
            var result = DoSerialize((TModel)instance, serializedDictionary);
            serialized = new fsData(serializedDictionary);
            return result;
        }

        public sealed override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
            var result = fsResult.Success;
            if ((result += CheckType(data, fsDataType.Object)).Failed) return result;

            var obj = (TModel)instance;
            result += DoDeserialize(data.AsDictionary, ref obj);
            instance = obj;
            return result;
        }

        protected abstract fsResult DoSerialize(TModel model, Dictionary<string, fsData> serialized);
        protected abstract fsResult DoDeserialize(Dictionary<string, fsData> data, ref TModel model);
    }
}
