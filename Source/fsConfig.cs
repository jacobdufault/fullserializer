using System;

namespace FullSerializer {
    /// <summary>
    /// Enables some top-level customization of Full Serializer.
    /// </summary>
    public static class fsConfig {
        /// <summary>
        /// The attributes that will force a field or property to be serialized.
        /// </summary>
        public static Type[] SerializeAttributes = {
#if !NO_UNITY
            typeof(UnityEngine.SerializeField),
#endif
            typeof(fsPropertyAttribute)
        };

        /// <summary>
        /// The attributes that will force a field or property to *not* be serialized.
        /// </summary>
        public static Type[] IgnoreSerializeAttributes = { typeof(NonSerializedAttribute), typeof(fsIgnoreAttribute) };

        /// <summary>
        /// The default member serialization.
        /// </summary>
        public static fsMemberSerialization DefaultMemberSerialization {
            get {
                return _defaultMemberSerialization;
            }
            set {
                _defaultMemberSerialization = value;
                fsMetaType.ClearCache();
            }
        }

        private static fsMemberSerialization _defaultMemberSerialization = fsMemberSerialization.Default;

        /// <summary>
        /// Should the default serialization behaviour include non-auto properties?
        /// </summary>
        public static bool SerializeNonAutoProperties = false;

        /// <summary>
        /// Should the default serialization behaviour include properties with non-public setters?
        /// </summary>
        public static bool SerializeNonPublicSetProperties = true;

        /// <summary>
        /// Should deserialization be case sensitive? If this is false and the JSON has multiple members with the
        /// same keys only separated by case, then this results in undefined behavior.
        /// </summary>
        public static bool IsCaseSensitive = true;

        /// <summary>
        /// If not null, this string format will be used for DateTime instead of the default one.
        /// </summary>
        public static string CustomDateTimeFormatString = null;

        /// <summary>
        /// Int64 and UInt64 will be serialized and deserialized as string for compatibility
        /// </summary>
        public static bool Serialize64BitIntegerAsString = false;

        /// <summary>
        /// Enums are serialized using their names by default. Setting this to true will serialize them as integers instead.
        /// </summary>
        public static bool SerializeEnumsAsInteger = false;
    }
}