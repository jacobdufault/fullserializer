using System;
using System.Collections.Generic;
using FullSerializer.Internal;

namespace FullSerializer {
    public class fsSerializer {
        #region Keys
        private static HashSet<string> _reservedKeywords;
        static fsSerializer() {
            _reservedKeywords = new HashSet<string> {
                Key_ObjectReference,
                Key_ObjectDefinition,
                Key_InstanceType,
                Key_Version,
                Key_Content
            };
        }
        /// <summary>
        /// Returns true if the given key is a special keyword that full serializer uses to
        /// add additional metadata on top of the emitted JSON.
        /// </summary>
        public static bool IsReservedKeyword(string key) {
            return _reservedKeywords.Contains(key);
        }

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
        /// Strips all deserialization metadata from the object, like $type and $content fields.
        /// </summary>
        /// <remarks>After making this call, you will *not* be able to deserialize the same object instance. The metadata is
        /// strictly necessary for deserialization!</remarks>
        public static void StripDeserializationMetadata(ref fsData data) {
            if (data.IsDictionary && data.AsDictionary.ContainsKey(Key_Content)) {
                data = data.AsDictionary[Key_Content];
            }

            if (data.IsDictionary) {
                var dict = data.AsDictionary;
                dict.Remove(Key_ObjectReference);
                dict.Remove(Key_ObjectDefinition);
                dict.Remove(Key_InstanceType);
                dict.Remove(Key_Version);
            }
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
                EnsureDictionary(data);
                ConvertLegacyData(ref data);

                data.AsDictionary[Key_InstanceType] = dict[typeString];
            }

            // object definition
            else if (dict.Count == 2 && dict.ContainsKey(sourceIdString) && dict.ContainsKey(sourceDataString)) {
                data = dict[sourceDataString];
                EnsureDictionary(data);
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

        #region Utility Methods
        private static void Invoke_OnBeforeSerialize(List<fsObjectProcessor> processors, Type storageType, object instance) {
            for (int i = 0; i < processors.Count; ++i) {
                processors[i].OnBeforeSerialize(storageType, instance);
            }
        }
        private static void Invoke_OnAfterSerialize(List<fsObjectProcessor> processors, Type storageType, object instance, ref fsData data) {
            // We run the after calls in reverse order; this significantly reduces the interaction burden between
            // multiple processors - it makes each one much more independent and ignorant of the other ones.

            for (int i = processors.Count - 1; i >= 0; --i) {
                processors[i].OnAfterSerialize(storageType, instance, ref data);
            }
        }
        private static void Invoke_OnBeforeDeserialize(List<fsObjectProcessor> processors, Type storageType, ref fsData data) {
            for (int i = 0; i < processors.Count; ++i) {
                processors[i].OnBeforeDeserialize(storageType, ref data);
            }
        }
        private static void Invoke_OnBeforeDeserializeAfterInstanceCreation(List<fsObjectProcessor> processors, Type storageType, object instance, ref fsData data) {
            for (int i = 0; i < processors.Count; ++i) {
                processors[i].OnBeforeDeserializeAfterInstanceCreation(storageType, instance, ref data);
            }
        }
        private static void Invoke_OnAfterDeserialize(List<fsObjectProcessor> processors, Type storageType, object instance) {
            for (int i = processors.Count - 1; i >= 0; --i) {
                processors[i].OnAfterDeserialize(storageType, instance);
            }
        }
        #endregion

        /// <summary>
        /// Ensures that the data is a dictionary. If it is not, then it is wrapped inside of one.
        /// </summary>
        private static void EnsureDictionary(fsData data) {
            if (data.IsDictionary == false) {
                var existingData = data.Clone();
                data.BecomeDictionary();
                data.AsDictionary[Key_Content] = existingData;
            }
        }

        /// <summary>
        /// This manages instance writing so that we do not write unnecessary $id fields. We
        /// only need to write out an $id field when there is a corresponding $ref field. This is able
        /// to write $id references lazily because the fsData instance is not actually written out to text
        /// until we have entirely finished serializing it.
        /// </summary>
        internal class fsLazyCycleDefinitionWriter {
            private Dictionary<int, fsData> _pendingDefinitions = new Dictionary<int, fsData>();
            private HashSet<int> _references = new HashSet<int>();

            public void WriteDefinition(int id, fsData data) {
                if (_references.Contains(id)) {
                    EnsureDictionary(data);
                    data.AsDictionary[Key_ObjectDefinition] = new fsData(id.ToString());
                }

                else {
                    _pendingDefinitions[id] = data;
                }
            }

            public void WriteReference(int id, Dictionary<string, fsData> dict) {
                // Write the actual definition if necessary
                if (_pendingDefinitions.ContainsKey(id)) {
                    var data = _pendingDefinitions[id];
                    EnsureDictionary(data);
                    data.AsDictionary[Key_ObjectDefinition] = new fsData(id.ToString());
                    _pendingDefinitions.Remove(id);
                }
                else {
                    _references.Add(id);
                }

                // Write the reference
                dict[Key_ObjectReference] = new fsData(id.ToString());
            }

            public void Clear() {
                _pendingDefinitions.Clear();
            }
        }

        /// <summary>
        /// A cache from type to it's converter.
        /// </summary>
        private Dictionary<Type, fsBaseConverter> _cachedConverters;

        /// <summary>
        /// A cache from type to the set of processors that are interested in it.
        /// </summary>
        private Dictionary<Type, List<fsObjectProcessor>> _cachedProcessors;

        /// <summary>
        /// Converters that can be used for type registration.
        /// </summary>
        private readonly List<fsConverter> _availableConverters;

        /// <summary>
        /// Direct converters (optimized _converters). We use these so we don't have to
        /// perform a scan through every item in _converters and can instead just do an O(1)
        /// lookup. This is potentially important to perf when there are a ton of direct
        /// converters.
        /// </summary>
        private readonly Dictionary<Type, fsDirectConverter> _availableDirectConverters;

        /// <summary>
        /// Processors that are available.
        /// </summary>
        private readonly List<fsObjectProcessor> _processors;

        /// <summary>
        /// Reference manager for cycle detection.
        /// </summary>
        private readonly fsCyclicReferenceManager _references;
        private readonly fsLazyCycleDefinitionWriter _lazyReferenceWriter;

        public fsSerializer() {
            _cachedConverters = new Dictionary<Type, fsBaseConverter>();
            _cachedProcessors = new Dictionary<Type, List<fsObjectProcessor>>();

            _references = new fsCyclicReferenceManager();
            _lazyReferenceWriter = new fsLazyCycleDefinitionWriter();

            // note: The order here is important. Items at the beginning of this
            //       list will be used before converters at the end. Converters
            //       added via AddConverter() are added to the front of the list.
            _availableConverters = new List<fsConverter> {
                new fsNullableConverter { Serializer = this },
                new fsGuidConverter { Serializer = this },
                new fsTypeConverter { Serializer = this },
                new fsDateConverter { Serializer = this },
                new fsEnumConverter { Serializer = this },
                new fsPrimitiveConverter { Serializer = this },
                new fsArrayConverter { Serializer = this },
                new fsDictionaryConverter { Serializer = this },
                new fsIEnumerableConverter { Serializer = this },
                new fsKeyValuePairConverter { Serializer = this },
                new fsWeakReferenceConverter { Serializer = this },
                new fsReflectedConverter { Serializer = this }
            };
            _availableDirectConverters = new Dictionary<Type, fsDirectConverter>();

            _processors = new List<fsObjectProcessor>() {
                new fsSerializationCallbackProcessor()
            };

            Context = new fsContext();

            // Register the converters from the registrar
            foreach (var converterType in fsConverterRegistrar.Converters) {
                AddConverter((fsBaseConverter)Activator.CreateInstance(converterType));
            }
        }

        /// <summary>
        /// A context object that fsConverters can use to customize how they operate.
        /// </summary>
        public fsContext Context;

        /// <summary>
        /// Add a new processor to the serializer. Multiple processors can run at the same time in the
        /// same order they were added in.
        /// </summary>
        /// <param name="processor">The processor to add.</param>
        public void AddProcessor(fsObjectProcessor processor) {
            _processors.Add(processor);

            // We need to reset our cached processor set, as it could be invalid with the new
            // processor. Ideally, _cachedProcessors should be empty (as the user should fully setup
            // the serializer before actually using it), but there is no guarantee.
            _cachedProcessors = new Dictionary<Type, List<fsObjectProcessor>>();
        }

        /// <summary>
        /// Fetches all of the processors for the given type.
        /// </summary>
        private List<fsObjectProcessor> GetProcessors(Type type) {
            List<fsObjectProcessor> processors;

            // Check to see if the user has defined a custom processor for the type. If they
            // have, then we don't need to scan through all of the processor to check which
            // one can process the type; instead, we directly use the specified processor.
            var attr = fsPortableReflection.GetAttribute<fsObjectAttribute>(type);
            if (attr != null && attr.Processor != null) {
                var processor = (fsObjectProcessor)Activator.CreateInstance(attr.Processor);
                processors = new List<fsObjectProcessor>();
                processors.Add(processor);
                _cachedProcessors[type] = processors;
            }

            else if (_cachedProcessors.TryGetValue(type, out processors) == false) {
                processors = new List<fsObjectProcessor>();

                for (int i = 0; i < _processors.Count; ++i) {
                    var processor = _processors[i];
                    if (processor.CanProcess(type)) {
                        processors.Add(processor);
                    }
                }

                _cachedProcessors[type] = processors;
            }

            return processors;
        }


        /// <summary>
        /// Adds a new converter that can be used to customize how an object is serialized and
        /// deserialized.
        /// </summary>
        public void AddConverter(fsBaseConverter converter) {
            if (converter.Serializer != null) {
                throw new InvalidOperationException("Cannot add a single converter instance to " +
                    "multiple fsConverters -- please construct a new instance for " + converter);
            }

            // TODO: wrap inside of a ConverterManager so we can control _converters and _cachedConverters lifetime
            if (converter is fsDirectConverter) {
                var directConverter = (fsDirectConverter)converter;
                _availableDirectConverters[directConverter.ModelType] = directConverter;
            }
            else if (converter is fsConverter) {
                _availableConverters.Insert(0, (fsConverter)converter);
            }
            else {
                throw new InvalidOperationException("Unable to add converter " + converter +
                    "; the type association strategy is unknown. Please use either " +
                    "fsDirectConverter or fsConverter as your base type.");
            }

            converter.Serializer = this;

            // We need to reset our cached converter set, as it could be invalid with the new
            // converter. Ideally, _cachedConverters should be empty (as the user should fully setup
            // the serializer before actually using it), but there is no guarantee.
            _cachedConverters = new Dictionary<Type, fsBaseConverter>();
        }

        /// <summary>
        /// Fetches a converter that can serialize/deserialize the given type.
        /// </summary>
        private fsBaseConverter GetConverter(Type type) {
            fsBaseConverter converter;
            if (_cachedConverters.TryGetValue(type, out converter)) {
                return converter;
            }

            // Check to see if the user has defined a custom converter for the type. If they
            // have, then we don't need to scan through all of the converters to check which
            // one can process the type; instead, we directly use the specified converter.
            {
                var attr = fsPortableReflection.GetAttribute<fsObjectAttribute>(type);
                if (attr != null && attr.Converter != null) {
                    converter = (fsBaseConverter)Activator.CreateInstance(attr.Converter);
                    converter.Serializer = this;
                    return _cachedConverters[type] = converter;
                }
            }

            // Check for a [fsForward] attribute.
            {
                var attr = fsPortableReflection.GetAttribute<fsForwardAttribute>(type);
                if (attr != null) {
                    converter = new fsForwardConverter(attr);
                    converter.Serializer = this;
                    return _cachedConverters[type] = converter;
                }
            }


            // There is no specific converter specified; try all of the general ones to see
            // which ones matches.
            if (_cachedConverters.TryGetValue(type, out converter) == false) {
                if (_availableDirectConverters.ContainsKey(type)) {
                    converter = _availableDirectConverters[type];
                    return _cachedConverters[type] = converter;
                }
                else {
                    for (int i = 0; i < _availableConverters.Count; ++i) {
                        if (_availableConverters[i].CanProcess(type)) {
                            converter = _availableConverters[i];
                            return _cachedConverters[type] = converter;
                        }
                    }
                }
            }

            throw new InvalidOperationException("Internal error -- could not find a converter for " + type);
        }

        /// <summary>
        /// Helper method that simply forwards the call to TrySerialize(typeof(T), instance, out data);
        /// </summary>
        public fsResult TrySerialize<T>(T instance, out fsData data) {
            return TrySerialize(typeof(T), instance, out data);
        }

        /// <summary>
        /// Generic wrapper around TryDeserialize that simply forwards the call.
        /// </summary>
        public fsResult TryDeserialize<T>(fsData data, ref T instance) {
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
        public fsResult TrySerialize(Type storageType, object instance, out fsData data) {
            var processors = GetProcessors(instance == null ? storageType : instance.GetType());

            Invoke_OnBeforeSerialize(processors, storageType, instance);

            // We always serialize null directly as null
            if (ReferenceEquals(instance, null)) {
                data = new fsData();
                Invoke_OnAfterSerialize(processors, storageType, instance, ref data);
                return fsResult.Success;
            }

            var result = InternalSerialize_1_ProcessCycles(storageType, instance, out data);
            Invoke_OnAfterSerialize(processors, storageType, instance, ref data);
            return result;
        }

        private fsResult InternalSerialize_1_ProcessCycles(Type storageType, object instance, out fsData data) {
            // We have an object definition to serialize.
            try {
                // Note that we enter the reference group at the beginning of serialization so that we support
                // references that are at equal serialization levels, not just nested serialization levels, within
                // the given subobject. A prime example is serialization a list of references.
                _references.Enter();

                // This type does not need cycle support.
                if (GetConverter(instance.GetType()).RequestCycleSupport(instance.GetType()) == false) {
                    return InternalSerialize_2_Inheritance(storageType, instance, out data);
                }

                // We've already serialized this object instance (or it is pending higher up on the call stack).
                // Just serialize a reference to it to escape the cycle.
                // 
                // note: We serialize the int as a string to so that we don't lose any information
                //       in a conversion to/from double.
                if (_references.IsReference(instance)) {
                    data = fsData.CreateDictionary();
                    _lazyReferenceWriter.WriteReference(_references.GetReferenceId(instance), data.AsDictionary);
                    return fsResult.Success;
                }

                // Mark inside the object graph that we've serialized the instance. We do this *before*
                // serialization so that if we get back into this function recursively, it'll already
                // be marked and we can handle the cycle properly without going into an infinite loop.
                _references.MarkSerialized(instance);

                // We've created the cycle metadata, so we can now serialize the actual object.
                // InternalSerialize will handle inheritance correctly for us.
                var result = InternalSerialize_2_Inheritance(storageType, instance, out data);
                if (result.Failed) return result;

                _lazyReferenceWriter.WriteDefinition(_references.GetReferenceId(instance), data);

                return result;
            }
            finally {
                if (_references.Exit()) {
                    _lazyReferenceWriter.Clear();
                }
            }
        }
        private fsResult InternalSerialize_2_Inheritance(Type storageType, object instance, out fsData data) {
            // Serialize the actual object with the field type being the same as the object
            // type so that we won't go into an infinite loop.
            var serializeResult = InternalSerialize_3_ProcessVersioning(instance, out data);
            if (serializeResult.Failed) return serializeResult;

            // Do we need to add type information? If the field type and the instance type are different
            // then we will not be able to recover the correct instance type from the field type when
            // we deserialize the object.
            //
            // Note: We allow converters to request that we do *not* add type information.
            if (storageType != instance.GetType() &&
                GetConverter(storageType).RequestInheritanceSupport(storageType)) {

                // Add the inheritance metadata
                EnsureDictionary(data);
                data.AsDictionary[Key_InstanceType] = new fsData(instance.GetType().FullName);
            }

            return serializeResult;
        }

        private fsResult InternalSerialize_3_ProcessVersioning(object instance, out fsData data) {
            // note: We do not have to take a Type parameter here, since at this point in the serialization
            //       algorithm inheritance has *always* been handled. If we took a type parameter, it will
            //       *always* be equal to instance.GetType(), so why bother taking the parameter?

            // Check to see if there is versioning information for this type. If so, then we need to serialize it.
            fsOption<fsVersionedType> optionalVersionedType = fsVersionManager.GetVersionedType(instance.GetType());
            if (optionalVersionedType.HasValue) {
                fsVersionedType versionedType = optionalVersionedType.Value;

                // Serialize the actual object content; we'll just wrap it with versioning metadata here.
                var result = InternalSerialize_4_Converter(instance, out data);
                if (result.Failed) return result;

                // Add the versioning information
                EnsureDictionary(data);
                data.AsDictionary[Key_Version] = new fsData(versionedType.VersionString);

                return result;
            }

            // This type has no versioning information -- directly serialize it using the selected converter.
            return InternalSerialize_4_Converter(instance, out data);
        }
        private fsResult InternalSerialize_4_Converter(object instance, out fsData data) {
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
        public fsResult TryDeserialize(fsData data, Type storageType, ref object result) {
            if (data.IsNull) {
                result = null;
                var processors = GetProcessors(storageType);
                Invoke_OnBeforeDeserialize(processors, storageType, ref data);
                Invoke_OnAfterDeserialize(processors, storageType, null);
                return fsResult.Success;
            }

            // Convert legacy data into modern style data
            ConvertLegacyData(ref data);

            try {
                // We wrap the entire deserialize call in a reference group so that we can properly
                // deserialize a "parallel" set of references, ie, a list of objects that are cyclic
                // w.r.t. the list
                _references.Enter();

                List<fsObjectProcessor> processors;
                var r = InternalDeserialize_1_CycleReference(data, storageType, ref result, out processors);
                if (r.Succeeded) {
                    Invoke_OnAfterDeserialize(processors, storageType, result);
                }
                return r;
            }
            finally {
                _references.Exit();
            }
        }

        private fsResult InternalDeserialize_1_CycleReference(fsData data, Type storageType, ref object result, out List<fsObjectProcessor> processors) {
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
                processors = GetProcessors(result.GetType());
                return fsResult.Success;
            }

            return InternalDeserialize_2_Version(data, storageType, ref result, out processors);
        }

        private fsResult InternalDeserialize_2_Version(fsData data, Type storageType, ref object result, out List<fsObjectProcessor> processors) {
            if (IsVersioned(data)) {
                // data is versioned, but we might not need to do a migration
                string version = data.AsDictionary[Key_Version].AsString;

                fsOption<fsVersionedType> versionedType = fsVersionManager.GetVersionedType(storageType);
                if (versionedType.HasValue &&
                    versionedType.Value.VersionString != version) {

                    // we have to do a migration
                    var deserializeResult = fsResult.Success;

                    List<fsVersionedType> path;
                    deserializeResult += fsVersionManager.GetVersionImportPath(version, versionedType.Value, out path);
                    if (deserializeResult.Failed) {
                        processors = GetProcessors(storageType);
                        return deserializeResult;
                    }

                    // deserialize as the original type
                    deserializeResult += InternalDeserialize_3_Inheritance(data, path[0].ModelType, ref result, out processors);
                    if (deserializeResult.Failed) return deserializeResult;

                    // TODO: we probably should be invoking object processors all along this pipeline
                    for (int i = 1; i < path.Count; ++i) {
                        result = path[i].Migrate(result);
                    }

                    processors = GetProcessors(deserializeResult.GetType());
                    return deserializeResult;
                }
            }

            return InternalDeserialize_3_Inheritance(data, storageType, ref result, out processors);
        }

        private fsResult InternalDeserialize_3_Inheritance(fsData data, Type storageType, ref object result, out List<fsObjectProcessor> processors) {
            var deserializeResult = fsResult.Success;

            // We wait until here to actually Invoke_OnBeforeDeserialize because we do not
            // have the correct set of processors to invoke until *after* we have resolved
            // the proper type to use for deserialization.
            // TODO: Consider if we want to use the objectType for fetching processors instead
            //       the type that the user passed in. If we move this check to also consider
            //       the object type, then we have to handle the scenario where a processor is
            //       recovering stale data and the object type lookup fails.
            processors = GetProcessors(storageType);
            Invoke_OnBeforeDeserialize(processors, storageType, ref data);

            Type objectType = storageType;

            // If the serialized state contains type information, then we need to make sure to update our
            // objectType and data to the proper values so that when we construct an object instance later
            // and run deserialization we run it on the proper type.
            if (IsTypeSpecified(data)) {
                fsData typeNameData = data.AsDictionary[Key_InstanceType];

                // we wrap everything in a do while false loop so we can break out it
                do {
                    if (typeNameData.IsString == false) {
                        deserializeResult.AddMessage(Key_InstanceType + " value must be a string (in " + data + ")");
                        break;
                    }

                    string typeName = typeNameData.AsString;
                    Type type = fsTypeLookup.GetType(typeName);
                    if (type == null) {
                        deserializeResult.AddMessage("Unable to locate specified type \"" + typeName + "\"");
                        break;
                    }

                    if (storageType.IsAssignableFrom(type) == false) {
                        deserializeResult.AddMessage("Ignoring type specifier; a field/property of type " + storageType + " cannot hold an instance of " + type);
                        break;
                    }

                    objectType = type;
                } while (false);
            }

            // Construct an object instance if we don't have one already. We also need to construct
            // an instance if the result type is of the wrong type, which may be the case when we
            // have a versioned import graph.
            if (ReferenceEquals(result, null) || result.GetType() != objectType) {
                result = GetConverter(objectType).CreateInstance(data, objectType);
            }

            // We call OnBeforeDeserializeAfterInstanceCreation here because we still want to invoke the
            // method even if the user passed in an existing instance.
            Invoke_OnBeforeDeserializeAfterInstanceCreation(processors, storageType, result, ref data);

            // NOTE: It is critically important that we pass the actual objectType down instead of
            //       using result.GetType() because it is not guaranteed that result.GetType()
            //       will equal objectType, especially because some converters are known to
            //       return dummy values for CreateInstance() (for example, the default behavior
            //       for structs is to just return the type of the struct).

            return deserializeResult += InternalDeserialize_4_Cycles(data, objectType, ref result);
        }

        private fsResult InternalDeserialize_4_Cycles(fsData data, Type resultType, ref object result) {
            if (IsObjectDefinition(data)) {
                // NOTE: object references are handled at stage 1

                // If this is a definition, then we have a serialization invariant that this is the
                // first time we have encountered the object (TODO: verify in the deserialization logic)

                // Since at this stage in the deserialization process we already have access to the
                // object instance, so we just need to sync the object id to the references database
                // so that when we encounter the instance we lookup this same object. We want to do
                // this before actually deserializing the object because when deserializing the object
                // there may be references to itself.

                int sourceId = int.Parse(data.AsDictionary[Key_ObjectDefinition].AsString);
                _references.AddReferenceWithId(sourceId, result);
            }

            // Nothing special, go through the standard deserialization logic.
            return InternalDeserialize_5_Converter(data, resultType, ref result);
        }

        private fsResult InternalDeserialize_5_Converter(fsData data, Type resultType, ref object result) {
            if (IsWrappedData(data)) {
                data = data.AsDictionary[Key_Content];
            }

            return GetConverter(resultType).TryDeserialize(data, ref result, resultType);
        }
    }
}