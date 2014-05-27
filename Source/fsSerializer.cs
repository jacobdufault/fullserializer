using FullSerializer.Internal;
using System;
using System.Collections.Generic;

namespace FullSerializer {
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
        private Dictionary<Type, fsConverter> _cachedConverters;

        /// <summary>
        /// Converters that are available.
        /// </summary>
        private List<fsConverter> _converters;

        /// <summary>
        /// Reference manager for cycle detection.
        /// </summary>
        private fsCyclicReferenceManager _references;

        public fsSerializer() {
            _cachedConverters = new Dictionary<Type, fsConverter>();
            _references = new fsCyclicReferenceManager();

            _converters = new List<fsConverter>() {
                new fsDateConverter() { Serializer = this },
                new fsEnumConverter() { Serializer = this },
                new fsPrimitiveConverter() { Serializer = this },
                new fsArrayConverter() { Serializer = this },
                new fsIEnumerableConverter() { Serializer = this },
                new fsReflectedConverter() { Serializer = this }
            };

            Context = new fsContext();
        }

        /// <summary>
        /// A context object that fsConverters can use to customize how they operate.
        /// </summary>
        public fsContext Context;

        /// <summary>
        /// Adds a new converter that can be used to customize how an object is serialized and
        /// deserialized.
        /// </summary>
        public void AddConverter(fsConverter converter) {
            if (converter.Serializer != null) {
                throw new InvalidOperationException("Cannot add a single converter instance to " +
                    "multiple JsonConverters -- please construct a new instance for " + converter);
            }

            _converters.Insert(0, converter);
            converter.Serializer = this;

            // We need to reset our cached converter set, as it could be invalid with the new
            // converter. Ideally, _cachedConverters should be empty, but there is no guarantee.
            _cachedConverters = new Dictionary<Type, fsConverter>();
        }

        /// <summary>
        /// Fetches a converter that can serialize/deserialize the given type.
        /// </summary>
        private fsConverter GetConverter(Type type) {
            fsConverter converter = null;

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
        /// Serialize the given value.
        /// </summary>
        /// <param name="storageType">The type of field/property that stores the object instance. This is
        /// important particularly for inheritance, as a field storing an IInterface instance
        /// should have type information included.</param>
        /// <param name="instance">The actual object instance to serialize.</param>
        /// <param name="data">The serialized state of the object.</param>
        /// <returns>If serialization was successful.</returns>
        public fsFailure TrySerialize(Type storageType, object instance, out fsData data) {
            if (instance == null) {
                data = new fsData();
                return fsFailure.Success;
            }

            // Not a cyclic type, ignore cycles
            if (fsReflectionUtility.CanContainCycles(instance.GetType()) == false) {
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
                    data = fsData.CreateDictionary();
                    data.AsDictionary[ReferenceIdString] = new fsData(_references.GetReferenceId(instance).ToString());
                    return fsFailure.Success;
                }

                // Mark inside the object graph that we've serialized the instance. We do this
                // *before* serialization so that if we get back here, it'll already be marked and
                // we can handle the cycle properly without going into an infinite loop.
                _references.MarkSerialized(instance);
                data = fsData.CreateDictionary();
                data.AsDictionary[SourceIdString] = new fsData(_references.GetReferenceId(instance).ToString());

                // We've created the cycle metadata, so we can now serialize the actual object.
                // InternalSerialize will handle inheritance correctly for us.
                fsData sourceData;
                var fail = InternalSerialize(storageType, instance, out sourceData);
                if (fail.Failed) return fail;
                data.AsDictionary[SourceDataString] = sourceData;

                return fsFailure.Success;
            }
            finally {
                _references.Exit();
            }
        }

        /// <summary>
        /// Actually serializes an object instance. This does *not* handle cycles but *does* handle
        /// inheritance.
        /// </summary>
        private fsFailure InternalSerialize(Type type, object instance, out fsData data) {
            // We need to add type information - the field type and the instance type are different
            // so we will not be able to recover the correct instance type from the field type when
            // we deserialize the object.
            if (type != instance.GetType()) {
                data = fsData.CreateDictionary();

                // Serialize the actual object with the field type being the same as the object
                // type so that we won't go into an infinite loop.
                fsData state;
                var fail = InternalSerialize(instance.GetType(), instance, out state);
                if (fail.Failed) return fail;

                // Add the inheritance metadata
                data.AsDictionary[TypeString] = new fsData(instance.GetType().FullName);
                data.AsDictionary[TypeDataString] = state;
                return fsFailure.Success;
            }

            return GetConverter(type).TrySerialize(instance, out data, type);
        }

        /// <summary>
        /// Returns true if the data represents an object reference.
        /// </summary>
        private bool IsObjectReference(fsData data) {
            if (data.IsDictionary == false) return false;
            var dict = data.AsDictionary;
            return
                dict.Count == 1 &&
                dict.ContainsKey(ReferenceIdString);
        }

        /// <summary>
        /// Returns true if the data represents an object definition.
        /// </summary>
        private bool IsObjectDefinition(fsData data) {
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
        private bool IsTypeMarker(fsData data) {
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
        private fsFailure InternalDeserialize(fsData data, Type storageType, ref object result) {
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
        private fsFailure ConstructInstance(ref fsData data, ref Type objectType, out object instance) {
            // It looks like the data *does* contain more type information. This likely means that
            // the field type and the object type are going to be different (ie, inheritance), so
            // we need to make sure to update our objectType variable as well.
            if (IsTypeMarker(data)) {
                string typeName = data.AsDictionary[TypeString].AsString;
                Type type = fsTypeLookup.GetType(typeName);
                if (type == null) {
                    instance = null;
                    return fsFailure.Fail("Unable to find type " + typeName);
                }

                objectType = type;
                data = data.AsDictionary[TypeDataString];
            }

            instance = GetConverter(objectType).CreateInstance(data, objectType);
            return fsFailure.Success;
        }

        /// <summary>
        /// Attempts to deserialize a value from a serialized state.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="objectType"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public fsFailure TryDeserialize(fsData data, Type objectType, ref object result) {
            fsFailure failed;

            if (data.IsNull) {
                result = null;
                return fsFailure.Success;
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
                    return fsFailure.Success;
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

                    return fsFailure.Success;
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