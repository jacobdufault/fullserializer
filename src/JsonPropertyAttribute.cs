using System;

namespace FullJson {
    /// <summary>
    /// Explicitly mark a property to be serialized. This can also be used to give the name that the
    /// property should use during serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class JsonPropertyAttribute : Attribute {
        /// <summary>
        /// The name of that the property will use in JSON serialization.
        /// </summary>
        public string Name;

        public JsonPropertyAttribute()
            : this(string.Empty) {
        }

        public JsonPropertyAttribute(string name) {
            Name = name;
        }
    }
}