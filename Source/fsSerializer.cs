using FullSerializer.Internal;
using System;
using System.Collections.Generic;

namespace FullSerializer {
    public class fsSerializer {
        private const string Key_ObjectReference = "$ref";
        private const string Key_ObjectDefinition = "$id";
        private const string Key_InstanceType = "$type";
        private const string Key_Version = "$version";
        private const string Key_WrappedData = "$content";

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

            return InternalSerialize_1_Inheritance(storageType, instance, out data);
        }

        private fsFailure InternalSerialize_1_Inheritance(Type storageType, object instance, out fsData data) {
            // Serialize the actual object with the field type being the same as the object
            // type so that we won't go into an infinite loop.
            var fail = InternalSerialize_2_ProcessCycles(instance, out data);
            if (fail.Failed) return fail;


            // Do we need to add type information? If the field type and the instance type are different
            // then we will not be able to recover the correct instance type from the field type when
            // we deserialize the object.
            //
            // Note: We allow converters to request that we do *not* add type information.
            if (storageType != instance.GetType() &&
                GetConverter(storageType).RequestInheritanceSupport(storageType)) {

                if (data.IsDictionary == false) {
                    var dict = fsData.CreateDictionary();
                    dict.AsDictionary[Key_WrappedData] = data;
                    data = dict;
                }

                // Add the inheritance metadata
                data.AsDictionary[Key_InstanceType] = new fsData(instance.GetType().FullName);
            }

            return fsFailure.Success;
        }

        private fsFailure InternalSerialize_2_ProcessCycles(object instance, out fsData data) {
            // This type does not need cycle support.
            if (GetConverter(instance.GetType()).RequestCycleSupport(instance.GetType()) == false) {
                return InternalSerialize_3_ProcessVersioning(instance, out data);
            }

            // We've already serialized this object instance (or it is pending higher up on the call stack).
            // Just serialize a reference to it to escape the cycle.
            // 
            // note: We serialize the int as a string to so that we don't lose any information
            //       in a conversion to/from floats.
            if (_references.IsReference(instance)) {
                data = fsData.CreateDictionary();
                data.AsDictionary[Key_ObjectReference] = new fsData(_references.GetReferenceId(instance).ToString());
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
                var fail = InternalSerialize_3_ProcessVersioning(instance, out data);
                if (fail.Failed) return fail;

                if (data.IsDictionary == false) {
                    var dict = fsData.CreateDictionary();
                    dict.AsDictionary[Key_WrappedData] = data;
                    data = dict;
                }

                data.AsDictionary[Key_ObjectDefinition] = new fsData(_references.GetReferenceId(instance).ToString());

                return fsFailure.Success;
            }
            finally {
                _references.Exit();
            }
        }
        private fsFailure InternalSerialize_3_ProcessVersioning(object instance, out fsData data) {
            // note: We do not have to take a Type parameter here, since at this point in the serialization
            //       algorithm inheritance has *always* been handled. If we took a type parameter, it will
            //       *always* be equal to instance.GetType(), so why bother taking the parameter?

            // Check to see if there is versioning information for this type. If so, then we need to serialize it.
            fsOption<fsVersionedType> optionalVersionedType = fsVersionedImport.GetVersionedType(instance.GetType());
            if (optionalVersionedType.HasValue) {
                fsVersionedType versionedType = optionalVersionedType.Value;

                data = fsData.CreateDictionary();

                // Serialize the actual object content; we'll just wrap it with versioning metadata here.
                var fail = InternalSerialize_4_Converter(instance, out data);
                if (fail.Failed) return fail;

                if (data.IsDictionary == false) {
                    var dict = fsData.CreateDictionary();
                    dict.AsDictionary[Key_WrappedData] = data;
                    data = dict;
                }

                // Add the versioning information
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

        #region Data Metadata Queries
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
            return data.AsDictionary.ContainsKey(Key_WrappedData);
        }
        #endregion

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

            return InternalDeserialize_1_Inheritance(data, storageType, ref result);
        }


        private fsFailure InternalDeserialize_1_Inheritance(fsData data, Type storageType, ref object result) {
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

            // Construct an object instance if we don't have one already.
            // note: If result is *not* null, then that means that the user passed in an existing object instance.
            //       In that scenario, we simply assume that the user knows what they want (and can override
            //       inheritance support). Is this the right behavior?
            if (ReferenceEquals(result, null)) {
                result = GetConverter(objectType).CreateInstance(data, objectType);
            }

            // NOTE: It is critically important that we pass the actual objectType down instead of
            //       using result.GetType() because it is not guaranteed that result.GetType()
            //       will equal objectType, especially because some converters are known to
            //       return dummy values for CreateInstance() (for example, the default behavior
            //       for structs is to just return the type of the struct).

            return InternalDeserialize_2_Cycles(data, objectType, ref result);
        }

        private fsFailure InternalDeserialize_2_Cycles(fsData data, Type resultType, ref object result) {
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

            if (IsObjectDefinition(data)) {
                try {
                    _references.Enter();

                    var dict = data.AsDictionary;

                    int sourceId = int.Parse(dict[Key_ObjectDefinition].AsString);

                    // to get the reference object, we need to deserialize it, but doing so sends a
                    // request back to our _references group... so we just construct an instance
                    // before deserialization so that our _references group resolves correctly.
                    _references.AddReferenceWithId(sourceId, result);
                    return InternalDeserialize_3a_Versioning(data, resultType, ref result);
                }
                finally {
                    _references.Exit();
                }
            }

            // Nothing special, go through the standard deserialization logic.
            return InternalDeserialize_3a_Versioning(data, resultType, ref result);
        }

        private fsFailure InternalDeserialize_3a_Versioning(fsData data, Type resultType, ref object result) {
            /*
            if (IsVersioningInformation(data)) {
                fsOption<fsVersionedType> optionalVerisonedType = fsVersionedImport.GetVersionedType(resultType);
                if (optionalVerisonedType.HasValue) {
                    fsVersionedType versionedType = optionalVerisonedType.Value;
                    string version = data.AsDictionary[VersionString].AsString;

                    List<fsVersionedType> path = fsVersionedImport.GetVersionImportPath(version, versionedType);
                }
            }
            */

            return InternalDeserialize_4a_Converter(data, resultType, ref result);
        }
        private fsFailure InternalDeserialize_4a_Converter(fsData data, Type resultType, ref object result) {
            if (IsWrappedData(data)) {
                data = data.AsDictionary[Key_WrappedData];
            }

            return GetConverter(resultType).TryDeserialize(data, ref result, resultType);
        }
    }
}