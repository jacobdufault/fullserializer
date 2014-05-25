using FullJson.Converters;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullJson {
    public class JsonConverter {
        private Dictionary<Type, SerializationConverterChain> _chains;
        private List<ISerializationConverter> _converterQueryReferences;
        private List<Type> _converterTypes;
        private CyclicReferenceManager _references = new CyclicReferenceManager();

        public JsonConverter() {
            _chains = new Dictionary<Type, SerializationConverterChain>();

            _converterQueryReferences = new List<ISerializationConverter>();
            _converterTypes = new List<Type>();

            AddConverter(typeof(EnumConverter));
            AddConverter(typeof(PrimitiveConverter));
            AddConverter(typeof(ArrayConverter));
            AddConverter(typeof(IEnumerableConverter));
            AddConverter(typeof(ReflectedConverter));
        }

        private ISerializationConverter GetConverterInstance(Type type) {
            var converter = (ISerializationConverter)Activator.CreateInstance(type);
            converter.Converter = this;
            return converter;
        }

        public void AddConverter(Type type) {
            _converterTypes.Add(type);
            _converterQueryReferences.Add(GetConverterInstance(type));
        }

        private SerializationConverterChain GetChain(Type type) {
            SerializationConverterChain converterChain;

            if (_chains.TryGetValue(type, out converterChain) == false) {
                var chain = new List<ISerializationConverter>();

                for (int i = 0; i < _converterQueryReferences.Count; ++i) {
                    ISerializationConverter converter = _converterQueryReferences[i];
                    if (converter.CanProcess(type)) {
                        chain.Add(GetConverterInstance(_converterTypes[i]));
                    }
                }

                converterChain = new SerializationConverterChain(chain.ToArray());
                _chains[type] = converterChain;
            }

            return converterChain;
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

            return GetChain(type).FirstConverter.TrySerialize(instance, out data, type);
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

            var converter = GetChain(objectType).FirstConverter;
            return converter.TryDeserialize(data, ref result, objectType);
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
                var converter = GetChain(type).FirstConverter;
                instance = converter.CreateInstance(data, type);
            }

            else {
                var converter = GetChain(objectType).FirstConverter;
                instance = converter.CreateInstance(data, objectType);
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