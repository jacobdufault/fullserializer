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
        /// Should deserialization be case sensitive? If this is false and the JSON has multiple members with the
        /// same keys only separated by case, then this results in undefined behavior.
        /// </summary>
        public static bool IsCaseSensitive = true;
    }
}