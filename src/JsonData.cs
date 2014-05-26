using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace FullJson {
    /// <summary>
    /// The actual type that a JsonData instance can store.
    /// </summary>
    public enum JsonType {
        Array,
        Object,
        Number,
        Boolean,
        String,
        Null
    }

    /// <summary>
    /// A union type that stores a serialized value. The stored type can be one of six different
    /// types: null, boolean, float, string, Dictionary, or List.
    /// </summary>
    public sealed class JsonData {
        /// <summary>
        /// The raw value that this serialized data stores. It can be one of six different types; a
        /// boolean, a float, a string, a Dictionary, or a List.
        /// </summary>
        private object _value;

        #region Constructors
        /// <summary>
        /// Creates a SerializedData instance that holds null.
        /// </summary>
        public JsonData() {
            _value = null;
        }

        /// <summary>
        /// Creates a SerializedData instance that holds a boolean.
        /// </summary>
        public JsonData(bool boolean) {
            _value = boolean;
        }

        /// <summary>
        /// Creates a SerializedData instance that holds a float.
        /// </summary>
        public JsonData(float f) {
            _value = f;
        }

        /// <summary>
        /// Creates a SerializedData instance that holds a string.
        /// </summary>
        public JsonData(string str) {
            _value = str;
        }

        /// <summary>
        /// Creates a SerializedData instance that holds a dictionary of values.
        /// </summary>
        public JsonData(Dictionary<string, JsonData> dict) {
            _value = dict;
        }

        /// <summary>
        /// Creates a SerializedData instance that holds a list of values.
        /// </summary>
        public JsonData(List<JsonData> list) {
            _value = list;
        }

        /// <summary>
        /// Helper method to create a SerializedData instance that holds a dictionary.
        /// </summary>
        public static JsonData CreateDictionary() {
            return new JsonData(new Dictionary<string, JsonData>());
        }

        /// <summary>
        /// Helper method to create a SerializedData instance that holds a list.
        /// </summary>
        public static JsonData CreateList() {
            return new JsonData(new List<JsonData>());
        }
        #endregion

        #region Casting Predicates
        public JsonType Type {
            get {
                if (_value == null) return JsonType.Null;
                if (_value is float) return JsonType.Number;
                if (_value is bool) return JsonType.Boolean;
                if (_value is string) return JsonType.String;
                if (_value is Dictionary<string, JsonData>) return JsonType.Object;
                if (_value is List<JsonData>) return JsonType.Array;

                throw new InvalidOperationException("unknown JSON data type");
            }
        }

        /// <summary>
        /// Returns true if this SerializedData instance maps back to null.
        /// </summary>
        public bool IsNull {
            get {
                return _value == null;
            }
        }

        /// <summary>
        /// Returns true if this SerializedData instance maps back to a float.
        /// </summary>
        public bool IsFloat {
            get {
                return _value is float;
            }
        }

        /// <summary>
        /// Returns true if this SerializedData instance maps back to a boolean.
        /// </summary>
        public bool IsBool {
            get {
                return _value is bool;
            }
        }

        /// <summary>
        /// Returns true if this SerializedData instance maps back to a string.
        /// </summary>
        public bool IsString {
            get {
                return _value is string;
            }
        }

        /// <summary>
        /// Returns true if this SerializedData instance maps back to a Dictionary.
        /// </summary>
        public bool IsDictionary {
            get {
                return _value is Dictionary<string, JsonData>;
            }
        }

        /// <summary>
        /// Returns true if this SerializedData instance maps back to a List.
        /// </summary>
        public bool IsList {
            get {
                return _value is List<JsonData>;
            }
        }
        #endregion

        #region Casts
        /// <summary>
        /// Casts this SerializedData to a float. Throws an exception if it is not a float.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public float AsFloat {
            get {
                return Cast<float>();
            }
        }

        /// <summary>
        /// Casts this SerializedData to a boolean. Throws an exception if it is not a boolean.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool AsBool {
            get {
                return Cast<bool>();
            }
        }

        /// <summary>
        /// Casts this SerializedData to a string. Throws an exception if it is not a string.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public string AsString {
            get {
                return Cast<string>();
            }
        }

        /// <summary>
        /// Casts this SerializedData to a Dictionary. Throws an exception if it is not a
        /// Dictionary.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Dictionary<string, JsonData> AsDictionary {
            get {
                return Cast<Dictionary<string, JsonData>>();
            }
        }

        /// <summary>
        /// Casts this SerializedData to a List. Throws an exception if it is not a List.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public List<JsonData> AsList {
            get {
                return Cast<List<JsonData>>();
            }
        }

        /// <summary>
        /// Internal helper method to cast the underlying storage to the given type or throw a
        /// pretty printed exception on failure.
        /// </summary>
        private T Cast<T>() {
            if (_value is T) {
                return (T)_value;
            }

            throw new InvalidCastException("Unable to cast <" + CompressedJson + "> (with type = " +
                _value.GetType() + ") to type " + typeof(T));
        }
        #endregion

        #region Pretty Printing
        /// <summary>
        /// Inserts the given number of indents into the builder.
        /// </summary>
        private void InsertSpacing(StringBuilder builder, int count) {
            for (int i = 0; i < count; ++i) {
                builder.Append("    ");
            }
        }

        /// <summary>
        /// Escapes a string.
        /// </summary>
        private string EscapeString(string str) {
            // Escaping a string is pretty allocation heavy, so we try hard to not do it.

            bool needsEscape = false;
            for (int i = 0; i < str.Length; ++i) {
                char c = str[i];

                // unicode code point
                int intChar = Convert.ToInt32(c);
                if (intChar < 0 || intChar > 127) {
                    needsEscape = true;
                    break;
                }

                // standard escape character
                switch (c) {
                    case '"':
                    case '\\':
                    case '\a':
                    case '\b':
                    case '\f':
                    case '\n':
                    case '\r':
                    case '\t':
                    case '\0':
                        needsEscape = true;
                        break;
                }

                if (needsEscape) {
                    break;
                }
            }

            if (needsEscape == false) {
                return str;
            }


            StringBuilder result = new StringBuilder();

            for (int i = 0; i < str.Length; ++i) {
                char c = str[i];

                // unicode code point
                int intChar = Convert.ToInt32(c);
                if (intChar < 0 || intChar > 127) {
                    result.Append(string.Format("\\u{0:x4} ", intChar).Trim());
                    continue;
                }

                // standard escape character
                switch (c) {
                    case '"': result.Append("\\\""); continue;
                    case '\\': result.Append(@"\"); continue;
                    case '\a': result.Append(@"\a"); continue;
                    case '\b': result.Append(@"\b"); continue;
                    case '\f': result.Append(@"\f"); continue;
                    case '\n': result.Append(@"\n"); continue;
                    case '\r': result.Append(@"\r"); continue;
                    case '\t': result.Append(@"\t"); continue;
                    case '\0': result.Append(@"\0"); continue;
                }

                // no escaping needed
                result.Append(c);
            }
            return result.ToString();
        }

        private void BuildCompressedString(StringBuilder builder) {
            switch (Type) {
                case JsonType.Null:
                    builder.Append("null");
                    break;

                case JsonType.Boolean:
                    if (AsBool) builder.Append("true");
                    else builder.Append("false");
                    break;

                case JsonType.Number:
                    builder.Append(AsFloat);
                    break;

                case JsonType.String:
                    builder.Append('"');
                    builder.Append(EscapeString(AsString));
                    builder.Append('"');
                    break;

                case JsonType.Object:
                    builder.Append('{');
                    foreach (var entry in AsDictionary) {
                        builder.Append('"');
                        builder.Append(entry.Key);
                        builder.Append('"');
                        builder.Append(":");
                        entry.Value.BuildCompressedString(builder);
                        builder.Append(',');
                    }
                    builder.Append('}');
                    break;

                case JsonType.Array:
                    builder.Append('[');
                    foreach (var entry in AsList) {
                        entry.BuildCompressedString(builder);
                        builder.Append(',');
                    }
                    builder.Append(']');
                    break;
            }
        }

        /// <summary>
        /// Formats this data into the given builder.
        /// </summary>
        private void BuildPrettyString(StringBuilder builder, int depth) {
            switch (Type) {
                case JsonType.Null:
                    builder.Append("null");
                    break;

                case JsonType.Boolean:
                    if (AsBool) builder.Append("true");
                    else builder.Append("false");
                    break;

                case JsonType.Number:
                    builder.Append(AsFloat);
                    break;

                case JsonType.String:
                    // we don't support escaping
                    builder.Append('"');
                    builder.Append(EscapeString(AsString));
                    builder.Append('"');
                    break;

                case JsonType.Object:
                    builder.Append('{');
                    builder.AppendLine();
                    foreach (var entry in AsDictionary) {
                        InsertSpacing(builder, depth + 1);
                        builder.Append('"');
                        builder.Append(entry.Key);
                        builder.Append('"');
                        builder.Append(": ");
                        entry.Value.BuildPrettyString(builder, depth + 1);
                        builder.Append(',');
                        builder.AppendLine();
                    }
                    InsertSpacing(builder, depth);
                    builder.Append('}');
                    break;

                case JsonType.Array:
                    // special case for empty lists; we don't put an empty line between the brackets
                    if (AsList.Count == 0) {
                        builder.Append("[]");
                    }

                    else {
                        builder.Append('[');
                        builder.AppendLine();
                        foreach (var entry in AsList) {
                            InsertSpacing(builder, depth + 1);
                            entry.BuildPrettyString(builder, depth + 1);
                            builder.Append(',');
                            builder.AppendLine();
                        }
                        InsertSpacing(builder, depth);
                        builder.Append(']');
                    }
                    break;
            }
        }

        /// <summary>
        /// Returns this SerializedData in a pretty printed format.
        /// </summary>
        public string PrettyJson {
            get {
                StringBuilder builder = new StringBuilder();
                BuildPrettyString(builder, 0);
                return builder.ToString();
            }
        }

        public string CompressedJson {
            get {
                var builder = new StringBuilder();
                BuildCompressedString(builder);
                return builder.ToString();
            }
        }
        #endregion

        #region ToString Implementation
        public override string ToString() {
            return CompressedJson;
        }
        #endregion

        #region Equality Comparisons
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object obj) {
            return Equals(obj as JsonData);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public bool Equals(JsonData other) {
            if (other == null || Type != other.Type) {
                return false;
            }

            switch (Type) {
                case JsonType.Null:
                    return true;

                case JsonType.Number:
                    return AsFloat == other.AsFloat;

                case JsonType.Boolean:
                    return AsBool == other.AsBool;

                case JsonType.String:
                    return AsString == other.AsString;

                case JsonType.Array:
                    var thisList = AsList;
                    var otherList = other.AsList;

                    if (thisList.Count != otherList.Count) return false;

                    for (int i = 0; i < thisList.Count; ++i) {
                        if (thisList[i].Equals(otherList[i]) == false) {
                            return false;
                        }
                    }

                    return true;

                case JsonType.Object:
                    var thisDict = AsDictionary;
                    var otherDict = other.AsDictionary;

                    if (thisDict.Count != otherDict.Count) return false;

                    foreach (string key in thisDict.Keys) {
                        if (otherDict.ContainsKey(key) == false) {
                            return false;
                        }

                        if (thisDict[key].Equals(otherDict[key]) == false) {
                            return false;
                        }
                    }

                    return true;
            }

            throw new Exception("Unknown data type");
        }

        /// <summary>
        /// Returns true iff a == b.
        /// </summary>
        public static bool operator ==(JsonData a, JsonData b) {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) {
                return false;
            }

            return a.Equals(b);
        }

        /// <summary>
        /// Returns true iff a != b.
        /// </summary>
        public static bool operator !=(JsonData a, JsonData b) {
            return !(a == b);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table.</returns>
        public override int GetHashCode() {
            return _value.GetHashCode();
        }
        #endregion
    }

}