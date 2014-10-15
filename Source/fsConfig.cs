using System;
using UnityEngine;

namespace FullSerializer {
    /// <summary>
    /// Enables some top-level customization of Full Serializer.
    /// </summary>
    public static class fsConfig {
        /// <summary>
        /// The attributes that will force a field or property to be serialized.
        /// </summary>
        public static Type[] SerializeAttributes = new[] { typeof(SerializeField), typeof(fsPropertyAttribute) };

        /// <summary>
        /// The attributes that will force a field or property to *not* be serialized.
        /// </summary>
        public static Type[] IgnoreSerializeAttributes = new[] { typeof(NonSerializedAttribute), typeof(fsIgnoreAttribute) };
    }
}