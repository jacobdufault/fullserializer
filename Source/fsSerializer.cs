using FullSerializer.Internal;
using System;
using System.Collections.Generic;

namespace FullSerializer {
    public class fsSerializer {
        #region Keys
        /// <summary>
        /// This is an object reference in part of a cyclic graph.
        /// </summary>
        private const string Key_ObjectReference = "$ref";

        /// <summary>
        /// This is an object definition, as part of a cyclic graph.
        /// </summary>
        private const string Key_ObjectDefinition = "$id";

        /// <summary>
        /// This specifies the actual type of an object (the instance type was different from
        /// the field type).
        /// </summary>
        private const string Key_InstanceType = "$type";

        /// <summary>
        /// The version string for the serialized data.
        /// </summary>
        private const string Key_Version = "$version";

        /// <summary>
        /// If we have to add metadata but the original serialized state was not a dictionary,
        /// then this will contain the original data.
        /// </summary>
        private const string Key_Content = "$content";

        private static bool IsObjectReference(fsData data) {
            if (data.IsDictionary == false) return false;
            return data.AsDictionary.ContainsKey(Key_ObjectReference);
        }
        private static bool IsObjectDefinition(fsData data) {
            if (data.IsDictionary == false) return false;
            return data.AsDictionary.ContainsKey(Key_ObjectDefinition);
        }
        private static bool IsVersioned(fsData data) {
            if (data.IsDictionary == false) return false;
            return data.AsDictionary.ContainsKey(Key_Version);
        }
        private static bool IsTypeSpecified(fsData data) {
            if (data.IsDictionary == false) return false;
            return data.AsDictionary.ContainsKey(Key_InstanceType);
        }
        private static bool IsWrappedData(fsData data) {
            if (data.IsDictionary == false) return false;
            return data.AsDictionary.ContainsKey(Key_Content);
        }

        /// <summary>
        /// This function converts legacy serialization data into the new format, so that
        /// the import process can be unified and ignore the old format.
        /// </summary>
        private static void ConvertLegacyData(ref fsData data) {
            if (data.IsDictionary == false) return;

            var dict = data.AsDictionary;

            // fast-exit: metadata never had more than two items
            if (dict.Count > 2) return;

            // Key strings used in the legacy system
            string referenceIdString = "ReferenceId";
            string sourceIdString = "SourceId";
            string sourceDataString = "Data";
            string typeString = "Type";
            string typeDataString = "Data";

            // type specifier
            if (dict.Count == 2 && dict.ContainsKey(typeString) && dict.ContainsKey(typeDataString)) {
                data = dict[typeDataString];
                EnsureDictionary(ref data);
                ConvertLegacyData(ref data);

                data.AsDictionary[Key_InstanceType] = dict[typeString];
            }

            // object definition
            else if (dict.Count == 2 && dict.ContainsKey(sourceIdString) && dict.ContainsKey(sourceDataString)) {
                data = dict[sourceDataString];
                EnsureDictionary(ref data);
                ConvertLegacyData(ref data);

                data.AsDictionary[Key_ObjectDefinition] = dict[sourceIdString];
            }

            // object reference
            else if (dict.Count == 1 && dict.ContainsKey(referenceIdString)) {
                data = fsData.CreateDictionary();
                data.AsDictionary[Key_ObjectReference] = dict[referenceIdString];
            }
        }
        #endregion


        /// <summary>
        /// Ensures that the data is a dictionary. If it is not, then it is wrapped inside of one.
        /// </summary>
        private static void EnsureDictionary(ref fsData data) {
            if (data.IsDictionary == false) {
                var dict = fsData.CreateDictionary();
                dict.AsDictionary[Key_Content] = data;
                data = dict;
            }
        }

        /// <summary>
        /// This manages instance writing so that we do not write unnecessary $id fields. We
        /// only need to write out an $id field when there is a corresponding $ref field.
        /// </summary>
        internal class fsLazyCycleDefinitionWriter {
            private Dictionary<int, Dictionary<string, fsData>> _definitions = new Dictionary<int, Dictionary<string, fsData>>();
            private HashSet<int> _references = new HashSet<int>();

            public void WriteDefinition(int id, Dictionary<string, fsData> dict) {
                if (_references.Contains(id)) {
                    dict[Key_ObjectDefinition] = new fsData(id.ToString());
                }

                else {
                    _definitions[id] = dict;
                }
            }

            public void WriteReference(int id, Dictionary<string, fsData> dict) {
                // Write the actual definition if necessary
                if (_definitions.ContainsKey(id)) {
                    _definitions[id][Key_ObjectDefinition] = new fsData(id.ToString());
                    _definitions.Remove(id);
                }
                else {
                    _references.Add(id);
                }

                // Write the reference
                dict[Key_ObjectReference] = new fsData(id.ToString());
            }

            public void Clear() {
                _definitions.Clear();
            }
        }

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
        private fsLazyCycleDefinitionWriter _lazyReferenceWriter;

        public fsSerializer() {
            _cachedConverters = new Dictionary<Type, fsConverter>();
            _references = new fsCyclicReferenceManager();
            _lazyReferenceWriter = new fsLazyCycleDefinitionWriter();

            _converters = new List<fsConverter>() {
                new fsNullableConverter() { Serializer = this },
                new fsGuidConverter() { Serializer = this },
                new fsTypeConverter() { Serializer = this },
                new fsDateConverter() { Serializer = this },
                new fsEnumConverter() { Serializer = this },
                new fsPrimitiveConverter() { Serializer = this },
                new fsArrayConverter() { Serializer = this },
                new fsIEnumerableConverter() { Serializer = this },
                new fsKeyValuePairConverter() { Serializer = this },
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
        /// Helper method that simply forwards the call to TrySerialize(typeof(T), instance, out data);
        /// </summary>
        public fsFailure TrySerialize<T>(T instance, out fsData data) {
            return TrySerialize(typeof(T), instance, out data);
        }

        /// <summary>
        /// Generic wrapper around TryDeserialize that simply forwards the call.
        /// </summary>
        public fsFailure TryDeserialize<T>(fsData data, ref T instance) {
            object boxed = instance;
            var fail = TryDeserialize(data, typeof(T), ref boxed);
            if (fail.Succeeded) {
                instance = (T)boxed;
            }
            return fail;
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
            // We always serialize null directly as null
            if (ReferenceEquals(instance, null)) {
                data = new fsData();
                return fsFailure.Success;
            }

            return InternalSerialize_1_ProcessCycles(storageType, instance, out data);
        }

        private fsFailure InternalSerialize_1_ProcessCycles(Type storageType, object instance, out fsData data) {
            // This type does not need cycle support.
            if (GetConverter(instance.GetType()).RequestCycleSupport(instance.GetType()) == false) {
                return InternalSerialize_2_Inheritance(storageType, instance, out data);
            }

            // We've already serialized this object instance (or it is pending higher up on the call stack).
            // Just serialize a reference to it to escape the cycle.
            // 
            // note: We serialize the int as a string to so that we don't lose any information
            //       in a conversion to/from floats.
            if (_references.IsReference(instance)) {
                data = fsData.CreateDictionary();

                _lazyReferenceWriter.WriteReference(_references.GetReferenceId(instance), data.AsDictionary);
                return fsFailure.Success;
            }

            // We have an object definition to serialize.
            try {
                _references.Enter();

                // Mark inside the object graph that we've serialized the instance. We do this *before*
                // serialization so that if we get back into this function recursively, it'll already
                // be marked and we can handle the cycle properly without going into an infinite loop.
                _references.MarkSerialized(instance);

                // We've created the cycle metadata, so we can now serialize the actual object.
                // InternalSerialize will handle inheritance correctly for us.
                var fail = InternalSerialize_2_Inheritance(storageType, instance, out data);
                if (fail.Failed) return fail;

                EnsureDictionary(ref data);
                _lazyReferenceWriter.WriteDefinition(_references.GetReferenceId(instance), data.AsDictionary);

                return fsFailure.Success;
            }
            finally {
                if (_references.Exit()) {
                    _lazyReferenceWriter.Clear();
                }
            }
        }
        private fsFailure InternalSerialize_2_Inheritance(Type storageType, object instance, out fsData data) {
            // Serialize the actual object with the field type being the same as the object
            // type so that we won't go into an infinite loop.
            var fail = InternalSerialize_3_ProcessVersioning(instance, out data);
            if (fail.Failed) return fail;

            // Do we need to add type information? If the field type and the instance type are different
            // then we will not be able to recover the correct instance type from the field type when
            // we deserialize the object.
            //
            // Note: We allow converters to request that we do *not* add type information.
            if (storageType != instance.GetType() &&
                GetConverter(storageType).RequestInheritanceSupport(storageType)) {

                EnsureDictionary(ref data);

                // Add the inheritance metadata
                data.AsDictionary[Key_InstanceType] = new fsData(instance.GetType().FullName);
            }

            return fsFailure.Success;
        }

        private fsFailure InternalSerialize_3_ProcessVersioning(object instance, out fsData data) {
            // note: We do not have to take a Type parameter here, since at this point in the serialization
            //       algorithm inheritance has *always* been handled. If we took a type parameter, it will
            //       *always* be equal to instance.GetType(), so why bother taking the parameter?

            // Check to see if there is versioning information for this type. If so, then we need to serialize it.
            fsOption<fsVersionedType> optionalVersionedType = fsVersionManager.GetVersionedType(instance.GetType());
            if (optionalVersionedType.HasValue) {
                fsVersionedType versionedType = optionalVersionedType.Value;

                data = fsData.CreateDictionary();

                // Serialize the actual object content; we'll just wrap it with versioning metadata here.
                var fail = InternalSerialize_4_Converter(instance, out data);
                if (fail.Failed) return fail;

                // Add the versioning information
                EnsureDictionary(ref data);
                data.AsDictionary[Key_Version] = new fsData(versionedType.VersionString);

                return fsFailure.Success;
            }

            // This type has no versioning information -- directly serialize it using the selected converter.
            return InternalSerialize_4_Converter(instance, out data);
        }
        private fsFailure InternalSerialize_4_Converter(object instance, out fsData data) {
            var instanceType = instance.GetType();
            return GetConverter(instanceType).TrySerialize(instance, out data, instanceType);
        }

        /// <summary>
        /// Attempts to deserialize a value from a serialized state.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="storageType"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public fsFailure TryDeserialize(fsData data, Type storageType, ref object result) {
            if (data.IsNull) {
                result = null;
                return fsFailure.Success;
            }

            // Convert legacy data into modern style data
            ConvertLegacyData(ref data);

            return InternalDeserialize_1_CycleReference(data, storageType, ref result);
        }

        private fsFailure InternalDeserialize_1_CycleReference(fsData data, Type storageType, ref object result) {
            // We handle object references first because we could be deserializing a cyclic type that is
            // inherited. If that is the case, then if we handle references after inheritances we will try
            // to create an object instance for an abstract/interface type.

            // While object construction should technically be two-pass, we can do it in
            // one pass because of how serialization happens. We traverse the serialization
            // graph in the same order during serialization and deserialization, so the first
            // time we encounter an object it'll always be the definition. Any times after that
            // it will be a reference. Because of this, if we encounter a reference then we
            // will have *always* already encountered the definition for it.
            if (IsObjectReference(data)) {
                int refId = int.Parse(data.AsDictionary[Key_ObjectReference].AsString);
                result = _references.GetReferenceObject(refId);
                return fsFailure.Success;
            }

            return InternalDeserialize_2_Version(data, storageType, ref result);
        }

        private fsFailure InternalDeserialize_2_Version(fsData data, Type storageType, ref object result) {
            if (IsVersioned(data)) {
                // data is versioned, but we might not need to do a migration
                string version = data.AsDictionary[Key_Version].AsString;

                fsOption<fsVersionedType> versionedType = fsVersionManager.GetVersionedType(storageType);
                if (versionedType.HasValue &&
                    versionedType.Value.VersionString != version) {

                    // we have to do a migration

                    List<fsVersionedType> path;
                    fsFailure fail = fsVersionManager.GetVersionImportPath(version, versionedType.Value, out path);
                    if (fail.Failed) return fail;

                    // deserialize as the original type
                    fail = InternalDeserialize_3_Inheritance(data, path[0].ModelType, ref result);
                    if (fail.Failed) return fail;

                    for (int i = 1; i < path.Count; ++i) {
                        result = path[i].Migrate(result);
                    }

                    return fsFailure.Success;
                }
            }

            return InternalDeserialize_3_Inheritance(data, storageType, ref result);
        }

        private fsFailure InternalDeserialize_3_Inheritance(fsData data, Type storageType, ref object result) {
            Type objectType = storageType;

            // If the serialized state contains type information, then we need to make sure to update our
            // objectType and data to the proper values so that when we construct an object instance later
            // and run deserialization we run it on the proper type.
            if (IsTypeSpecified(data)) {
                string typeName = data.AsDictionary[Key_InstanceType].AsString;
                Type type = fsTypeLookup.GetType(typeName);
                if (type == null) {
                    return fsFailure.Fail("Unable to find type " + typeName);
                }

                objectType = type;
            }

            // Construct an object instance if we don't have one already. We also need to construct
            // an instance if the result type is of the wrong type, which is also important for
            // versioning.
            if (ReferenceEquals(result, null) || result.GetType() != objectType) {
                result = GetConverter(objectType).CreateInstance(data, objectType);
            }

            // NOTE: It is critically important that we pass the actual objectType down instead of
            //       using result.GetType() because it is not guaranteed that result.GetType()
            //       will equal objectType, especially because some converters are known to
            //       return dummy values for CreateInstance() (for example, the default behavior
            //       for structs is to just return the type of the struct).

            return InternalDeserialize_4_Cycles(data, objectType, ref result);
        }

        private fsFailure InternalDeserialize_4_Cycles(fsData data, Type resultType, ref object result) {
            // object references are handled at stage 1

            if (IsObjectDefinition(data)) {
                try {
                    _references.Enter();

                    var dict = data.AsDictionary;

                    int sourceId = int.Parse(dict[Key_ObjectDefinition].AsString);

                    // to get the reference object, we need to deserialize it, but doing so sends a
                    // request back to our _references group... so we just construct an instance
                    // before deserialization so that our _references group resolves correctly.
                    _references.AddReferenceWithId(sourceId, result);
                    return InternalDeserialize_4_Converter(data, resultType, ref result);
                }
                finally {
                    _references.Exit();
                }
            }

            // Nothing special, go through the standard deserialization logic.
            return InternalDeserialize_4_Converter(data, resultType, ref result);
        }

        private fsFailure InternalDeserialize_4_Converter(fsData data, Type resultType, ref object result) {
            if (IsWrappedData(data)) {
                data = data.AsDictionary[Key_Content];
            }

            return GetConverter(resultType).TryDeserialize(data, ref result, resultType);
        }
    }
}