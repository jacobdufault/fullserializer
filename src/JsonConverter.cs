using FullJson.Internal;
using System;
using System.Collections.Generic;

namespace FullJson {
    public class JsonConverter {
        private Dictionary<Type, ISerializationConverter> _cachedConverters;
        private List<ISerializationConverter> _converters;
        private CyclicReferenceManager _references;

        public JsonConverter() {
            _cachedConverters = new Dictionary<Type, ISerializationConverter>();
            _references = new CyclicReferenceManager();

            _converters = new List<ISerializationConverter>() {
                new EnumConverter(),
                new PrimitiveConverter(),
                new ArrayConverter(),
                new IEnumerableConverter(),
                new ReflectedConverter()
            };
        }

        public void AddConverter(ISerializationConverter converter) {
            _converters.Insert(0, converter);
        }

        private ISerializationConverter GetConverter(Type type) {
            ISerializationConverter converter = null;

            if (_cachedConverters.TryGetValue(type, out converter) == false) {
                for (int i = 0; i < _converters.Count; ++i) {
                    if (_converters[i].CanProcess(type)) {
                        converter = _converters[i];
                        _cachedConverters[type] = converter;
                        break;
                    }
                }
            }

            if (converter == null) {
                throw new InvalidOperationException("Internal error -- could not find a converter for " + type);
            }
            return converter;
        }

        public bool ShouldCheckForReferences(object instance) {
            var instanceType = instance.GetType();

            if (instanceType == typeof(string)) return false;

            return instanceType.IsClass || instanceType.IsInterface;
        }

        public JsonFailure TrySerialize(Type type, object instance, out JsonData data) {
            if (instance == null) {
                data = new JsonData();
                return JsonFailure.Success;
            }

            // Not a cyclic type, ignore cycles
            if (ShouldCheckForReferences(instance) == false) {
                return InternalSerialize(type, instance, out data);
            }

            try {
                _references.Enter();
                if (_references.IsReference(instance)) {
                    data = JsonData.CreateDictionary();
                    data.AsDictionary[ReferenceIdString] = new JsonData(_references.GetReferenceId(instance).ToString());
                    return JsonFailure.Success;
                }

                _references.MarkSerialized(instance);
                data = JsonData.CreateDictionary();
                data.AsDictionary[SourceIdString] = new JsonData(_references.GetReferenceId(instance).ToString());

                JsonData sourceData;
                var fail = InternalSerialize(type, instance, out sourceData);
                if (fail.Failed) return fail;
                data.AsDictionary[SourceDataString] = sourceData;

                return JsonFailure.Success;
            }
            finally {
                _references.Exit();
            }
        }

        private JsonFailure InternalSerialize(Type type, object instance, out JsonData data) {
            // We need to add type information
            if (type != instance.GetType()) {
                data = JsonData.CreateDictionary();

                JsonData state;
                var fail = InternalSerialize(instance.GetType(), instance, out state);
                if (fail.Failed) return fail;

                data.AsDictionary[TypeString] = new JsonData(instance.GetType().FullName);
                data.AsDictionary[TypeDataString] = state;
                return JsonFailure.Success;
            }

            return GetConverter(type).TrySerialize(instance, out data, type);
        }

        private bool IsObjectReference(JsonData data) {
            if (data.IsDictionary == false) return false;
            var dict = data.AsDictionary;
            return
                dict.Count == 1 &&
                dict.ContainsKey(ReferenceIdString);
        }

        private bool IsObjectDefinition(JsonData data) {
            if (data.IsDictionary == false) return false;
            var dict = data.AsDictionary;
            return
                dict.Count == 2 &&
                dict.ContainsKey(SourceIdString) &&
                dict.ContainsKey(SourceDataString);
        }

        private bool IsTypeMarker(JsonData data) {
            if (data.IsDictionary == false) return false;
            var dict = data.AsDictionary;
            return
                dict.Count == 2 &&
                dict.ContainsKey(TypeString) &&
                dict.ContainsKey(TypeDataString);
        }

        private JsonFailure InternalDeserialize(JsonData data, Type objectType, ref object result) {
            if (result == null) {
                throw new InvalidOperationException("InternalDeserialize requires a preconstructed object instance");
            }

            if (IsTypeMarker(data)) {
                data = data.AsDictionary[TypeDataString];
            }

            return GetConverter(objectType).TryDeserialize(data, ref result, objectType);
        }

        private JsonFailure ConstructInstance(JsonData data, ref Type objectType, out object instance) {
            if (IsTypeMarker(data)) {
                string typeName = data.AsDictionary[TypeString].AsString;
                Type type = TypeLookup.GetType(typeName);
                if (type == null) {
                    instance = null;
                    return JsonFailure.Fail("Unable to find type " + typeName);
                }

                objectType = type;
                instance = GetConverter(type).CreateInstance(data, type);
            }

            else {
                instance = GetConverter(objectType).CreateInstance(data, objectType);
            }

            return JsonFailure.Success;
        }

        private const string ReferenceIdString = "ReferenceId";
        private const string SourceIdString = "SourceId";
        private const string SourceDataString = "SourceData";
        private const string TypeString = "Type";
        private const string TypeDataString = "Data";

        public JsonFailure TryDeserialize(JsonData data, Type objectType, ref object result) {
            JsonFailure failed;

            if (data.IsNull) {
                result = null;
                return JsonFailure.Success;
            }

            try {

                _references.Enter();

                if (IsObjectReference(data)) {
                    long refId = long.Parse(data.AsDictionary[ReferenceIdString].AsString);
                    result = _references.GetReferenceObject(refId);
                    return JsonFailure.Success;
                }

                if (IsObjectDefinition(data)) {
                    var dict = data.AsDictionary;

                    long sourceId = long.Parse(dict[SourceIdString].AsString);
                    var sourceData = dict[SourceDataString];

                    // to get the reference object, we need to deserialize it, but doing so sends a
                    // request back to our _references group... so we just construct an instance
                    // before deserialization so that our _references group resolves correctly.
                    if (result == null) {
                        failed = ConstructInstance(sourceData, ref objectType, out result);
                        if (failed.Failed) return failed;

                        _references.AddReferenceWithId(sourceId, result);
                    }

                    var fail = InternalDeserialize(sourceData, objectType, ref result);
                    if (fail.Failed) return fail;

                    return JsonFailure.Success;
                }

                failed = ConstructInstance(data, ref objectType, out result);
                if (failed.Failed) return failed;
                return InternalDeserialize(data, objectType, ref result);
            }
            finally {
                _references.Exit();
            }
        }

    }
}