using FullJson.Internal;
using System;
using System.Collections.Generic;

namespace FullJson {
    public class fsSerializer {
        /// <summary>
        /// Key used after a cycle has been encountered.
        /// </summary>
        private const string ReferenceIdString = "ReferenceId";

        /// <summary>
        /// Key used for an initial object reference id.
        /// </summary>
        private const string SourceIdString = "SourceId";

        /// <summary>
        /// Key used for the data for an initial object reference.
        /// </summary>
        private const string SourceDataString = "Data";

        /// <summary>
        /// Key used to identify the runtime type of an object.
        /// </summary>
        private const string TypeString = "Type";

        /// <summary>
        /// Key used for the serialized content of the type identified object.
        /// </summary>
        private const string TypeDataString = "Data";

        /// <summary>
        /// A cache from type to it's converter.
        /// </summary>
        private Dictionary<Type, SerializationConverter> _cachedConverters;

        /// <summary>
        /// Converters that are available.
        /// </summary>
        private List<SerializationConverter> _converters;

        /// <summary>
        /// Reference manager for cycle detection.
        /// </summary>
        private CyclicReferenceManager _references;

        public fsSerializer() {
            _cachedConverters = new Dictionary<Type, SerializationConverter>();
            _references = new CyclicReferenceManager();

            _converters = new List<SerializationConverter>() {
                new EnumConverter() { Serializer = this },
                new PrimitiveConverter() { Serializer = this },
                new ArrayConverter() { Serializer = this },
                new IEnumerableConverter() { Serializer = this },
                new ReflectedConverter() { Serializer = this }
            };
        }

        /// <summary>
        /// Adds a new converter that can be used to customize how an object is serialized and
        /// deserialized.
        /// </summary>
        public void AddConverter(SerializationConverter converter) {
            if (converter.Serializer != null) {
                throw new InvalidOperationException("Cannot add a single converter instance to " +
                    "multiple JsonConverters -- please construct a new instance for " + converter);
            }

            _converters.Insert(0, converter);
            converter.Serializer = this;

            // We need to reset our cached converter set, as it could be invalid with the new
            // converter. Ideally, _cachedConverters should be empty, but there is no guarantee.
            _cachedConverters = new Dictionary<Type, SerializationConverter>();
        }

        /// <summary>
        /// Fetches a converter that can serialize/deserialize the given type.
        /// </summary>
        private SerializationConverter GetConverter(Type type) {
            SerializationConverter converter = null;

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

        /// <summary>
        /// Returns true if the object can be part of a cyclic graph. If this is false, then
        /// graph tracking will be disabled which will cause less serialization data to be emitted
        /// and the serializer will run a little bit faster.
        /// </summary>
        private bool CanBeCyclic(object instance) {
            var instanceType = instance.GetType();

            if (instanceType == typeof(string)) return false;

            return instanceType.IsClass || instanceType.IsInterface;
        }

        /// <summary>
        /// Serialize the given value.
        /// </summary>
        /// <param name="storageType">The type of field/property that stores the object instance. This is
        /// important particularly for inheritance, as a field storing an IInterface instance
        /// should have type information included.</param>
        /// <param name="instance">The actual object instance to serialize.</param>
        /// <param name="data">The serialized state of the object.</param>
        /// <returns>If serialization was successful.</returns>
        public JsonFailure TrySerialize(Type storageType, object instance, out JsonData data) {
            if (instance == null) {
                data = new JsonData();
                return JsonFailure.Success;
            }

            // Not a cyclic type, ignore cycles
            if (CanBeCyclic(instance) == false) {
                return InternalSerialize(storageType, instance, out data);
            }

            // We have to handle cycles.
            try {
                _references.Enter();

                // This is the second time we've encountered this object instance. Just serialize a
                // reference to it to escape the cycle.
                // 
                // note: We serialize the long as a string to so that we don't lose any information
                //       in a conversion to/from floats.
                if (_references.IsReference(instance)) {
                    data = JsonData.CreateDictionary();
                    data.AsDictionary[ReferenceIdString] = new JsonData(_references.GetReferenceId(instance).ToString());
                    return JsonFailure.Success;
                }

                // Mark inside the object graph that we've serialized the instance. We do this
                // *before* serialization so that if we get back here, it'll already be marked and
                // we can handle the cycle properly without going into an infinite loop.
                _references.MarkSerialized(instance);
                data = JsonData.CreateDictionary();
                data.AsDictionary[SourceIdString] = new JsonData(_references.GetReferenceId(instance).ToString());

                // We've created the cycle metadata, so we can now serialize the actual object.
                // InternalSerialize will handle inheritance correctly for us.
                JsonData sourceData;
                var fail = InternalSerialize(storageType, instance, out sourceData);
                if (fail.Failed) return fail;
                data.AsDictionary[SourceDataString] = sourceData;

                return JsonFailure.Success;
            }
            finally {
                _references.Exit();
            }
        }

        /// <summary>
        /// Actually serializes an object instance. This does *not* handle cycles but *does* handle
        /// inheritance.
        /// </summary>
        private JsonFailure InternalSerialize(Type type, object instance, out JsonData data) {
            // We need to add type information - the field type and the instance type are different
            // so we will not be able to recover the correct instance type from the field type when
            // we deserialize the object.
            if (type != instance.GetType()) {
                data = JsonData.CreateDictionary();

                // Serialize the actual object with the field type being the same as the object
                // type so that we won't go into an infinite loop.
                JsonData state;
                var fail = InternalSerialize(instance.GetType(), instance, out state);
                if (fail.Failed) return fail;

                // Add the inheritance metadata
                data.AsDictionary[TypeString] = new JsonData(instance.GetType().FullName);
                data.AsDictionary[TypeDataString] = state;
                return JsonFailure.Success;
            }

            return GetConverter(type).TrySerialize(instance, out data, type);
        }

        /// <summary>
        /// Returns true if the data represents an object reference.
        /// </summary>
        private bool IsObjectReference(JsonData data) {
            if (data.IsDictionary == false) return false;
            var dict = data.AsDictionary;
            return
                dict.Count == 1 &&
                dict.ContainsKey(ReferenceIdString);
        }

        /// <summary>
        /// Returns true if the data represents an object definition.
        /// </summary>
        private bool IsObjectDefinition(JsonData data) {
            if (data.IsDictionary == false) return false;
            var dict = data.AsDictionary;
            return
                dict.Count == 2 &&
                dict.ContainsKey(SourceIdString) &&
                dict.ContainsKey(SourceDataString);
        }

        /// <summary>
        /// Does this data represent a "type" marker, ie, does it specify a type that should be
        /// created? This is used for inheritance when the field type is not the same as the object
        /// type.
        /// </summary>
        private bool IsTypeMarker(JsonData data) {
            if (data.IsDictionary == false) return false;
            var dict = data.AsDictionary;
            return
                dict.Count == 2 &&
                dict.ContainsKey(TypeString) &&
                dict.ContainsKey(TypeDataString);
        }

        /// <summary>
        /// Actually deserializes a value, ignoring cycles and object instance construction.
        /// </summary>
        /// <param name="data">The data to deserialize.</param>
        /// <param name="storageType">The field type of the object to deserialize. Used for
        /// fetching the converter to use when deserializing.</param>
        /// <param name="result">The deserialized result. This cannot be null.</param>
        /// <returns>If deserialization was successful.</returns>
        private JsonFailure InternalDeserialize(JsonData data, Type storageType, ref object result) {
            if (result == null) {
                throw new InvalidOperationException("InternalDeserialize requires a preconstructed object instance");
            }

            // It may be tempting to try and remove the storageType parameter and just use
            // result.GetType(), as by this point inheritance should have been handled
            // automatically, but you must resist as there is no guarantee that result is
            // actually a proper instance of the correct type.

            return GetConverter(storageType).TryDeserialize(data, ref result, storageType);
        }

        /// <summary>
        /// Constructs an object instance (but does not populate it's values!) and updates the
        /// object type if necessary.
        /// </summary>
        /// <param name="data">The data that potentially contains additional type info. If this
        /// does contain extra type information, it will automatically be scoped to the proper
        /// deserialization data after the instance has been constructed.</param>
        /// <param name="objectType">The object type to construct. This can change if the
        /// serialized data has more information.</param>
        /// <param name="instance">The constructed object instance.</param>
        /// <returns>Any errors that occurred while constructing the instance.</returns>
        private JsonFailure ConstructInstance(ref JsonData data, ref Type objectType, out object instance) {
            // It looks like the data *does* contain more type information. This likely means that
            // the field type and the object type are going to be different (ie, inheritance), so
            // we need to make sure to update our objectType variable as well.
            if (IsTypeMarker(data)) {
                string typeName = data.AsDictionary[TypeString].AsString;
                Type type = TypeLookup.GetType(typeName);
                if (type == null) {
                    instance = null;
                    return JsonFailure.Fail("Unable to find type " + typeName);
                }

                objectType = type;
                data = data.AsDictionary[TypeDataString];
            }

            instance = GetConverter(objectType).CreateInstance(data, objectType);
            return JsonFailure.Success;
        }

        /// <summary>
        /// Attempts to deserialize a value from a serialized state.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="objectType"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public JsonFailure TryDeserialize(JsonData data, Type objectType, ref object result) {
            JsonFailure failed;

            if (data.IsNull) {
                result = null;
                return JsonFailure.Success;
            }

            try {
                _references.Enter();

                // While object construction should technically be two-pass, we can do it in
                // one pass because of how serialization happens. We traverse the serialization
                // graph in the same order during serialization and deserialization, so the first
                // time we encounter an object it'll always be the definition. Any times after that
                // it will be a reference. Because of this, if we encounter a reference then we
                // will have *always* already encountered the definition for it.
                if (IsObjectReference(data)) {
                    long refId = long.Parse(data.AsDictionary[ReferenceIdString].AsString);
                    result = _references.GetReferenceObject(refId);
                    return JsonFailure.Success;
                }

                // We have an object definition, so deserialize it with the additional metadata
                // in mind.
                if (IsObjectDefinition(data)) {
                    var dict = data.AsDictionary;

                    long sourceId = long.Parse(dict[SourceIdString].AsString);
                    var sourceData = dict[SourceDataString];

                    // to get the reference object, we need to deserialize it, but doing so sends a
                    // request back to our _references group... so we just construct an instance
                    // before deserialization so that our _references group resolves correctly.
                    if (result == null) {
                        failed = ConstructInstance(ref sourceData, ref objectType, out result);
                        if (failed.Failed) return failed;

                        _references.AddReferenceWithId(sourceId, result);
                    }

                    var fail = InternalDeserialize(sourceData, objectType, ref result);
                    if (fail.Failed) return fail;

                    return JsonFailure.Success;
                }

                // Nothing special, go through the standard deserialization logic.
                failed = ConstructInstance(ref data, ref objectType, out result);
                if (failed.Failed) return failed;
                return InternalDeserialize(data, objectType, ref result);
            }
            finally {
                _references.Exit();
            }
        }

    }
}