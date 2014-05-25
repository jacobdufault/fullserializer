using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace FullJson {
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

#if ENABLE_IMPLICIT_CONVERSIONS
        #region Implicit Casts (if enabled)
        public static implicit operator SerializedData(bool boolean) {
            return new SerializedData(boolean);
        }

        public static implicit operator SerializedData(float f) {
            return new SerializedData(f);
        }

        public static implicit operator SerializedData(string str) {
            return new SerializedData(str);
        }

        public static implicit operator SerializedData(List<SerializedData> list) {
            return new SerializedData(list);
        }

        public static implicit operator SerializedData(Dictionary<string, SerializedData> dict) {
            return new SerializedData(dict);
        }

        public SerializedData this[int index] {
            get {
                return AsList[index];
            }
            set {
                AsList[index] = value;
            }
        }

        public SerializedData this[string key] {
            get {
                return AsDictionary[key];
            }
            set {
                AsDictionary[key] = value;
            }
        }

        public static implicit operator float(SerializedData value) {
            return value.Cast<float>();
        }

        public static implicit operator string(SerializedData value) {
            return value.Cast<string>();
        }

        public static implicit operator bool(SerializedData value) {
            return value.Cast<bool>();
        }
        #endregion
#endif

        #region Pretty Printing
        /// <summary>
        /// Inserts the given number of indents into the builder.
        /// </summary>
        private void InsertSpacing(StringBuilder builder, int count) {
            for (int i = 0; i < count; ++i) {
                builder.Append("    ");
            }
        }

        private void BuildCompressedString(StringBuilder builder) {
            if (IsNull) {
                builder.Append("null");
            }

            else if (IsBool) {
                if (AsBool) {
                    builder.Append("true");
                }
                else {
                    builder.Append("false");
                }
            }

            else if (IsFloat) {
                builder.Append(AsFloat);
            }

            else if (IsString) {
                // we don't support escaping
                builder.Append('"');
                builder.Append(AsString);
                builder.Append('"');
            }

            else if (IsDictionary) {
                builder.Append('{');
                foreach (var entry in AsDictionary) {
                    builder.Append('"');
                    builder.Append(entry.Key);
                    builder.Append('"');
                    builder.Append(":");
                    entry.Value.BuildCompressedString(builder);
                    builder.Append(' ');
                }
                builder.Append('}');
            }

            else if (IsList) {
                builder.Append('[');
                foreach (var entry in AsList) {
                    entry.BuildCompressedString(builder);
                    builder.Append(' ');
                }
                builder.Append(']');
            }

            else {
                throw new NotImplementedException("Unknown stored value type of " + _value);
            }
        }

        /// <summary>
        /// Formats this data into the given builder.
        /// </summary>
        private void BuildPrettyString(StringBuilder builder, int depth) {
            if (IsNull) {
                builder.Append("null");
            }

            else if (IsBool) {
                if (AsBool) {
                    builder.Append("true");
                }
                else {
                    builder.Append("false");
                }
            }

            else if (IsFloat) {
                builder.Append(AsFloat);
            }

            else if (IsString) {
                // we don't support escaping
                builder.Append('"');
                builder.Append(AsString);
                builder.Append('"');
            }

            else if (IsDictionary) {
                builder.Append('{');
                builder.AppendLine();
                foreach (var entry in AsDictionary) {
                    InsertSpacing(builder, depth + 1);
                    builder.Append('"');
                    builder.Append(entry.Key);
                    builder.Append('"');
                    builder.Append(": ");
                    entry.Value.BuildPrettyString(builder, depth + 1);
                    builder.AppendLine();
                }
                InsertSpacing(builder, depth);
                builder.Append('}');
            }

            else if (IsList) {
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
                        builder.AppendLine();
                    }
                    InsertSpacing(builder, depth);
                    builder.Append(']');
                }
            }

            else {
                throw new NotImplementedException("Unknown stored value type of " + _value);
            }
        }

        /// <summary>
        /// Returns this SerializedData in a pretty printed format.
        /// </summary>
        public string PrettyPrintedJson {
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
            if (other == null) {
                return false;
            }

            if (IsNull) {
                return other.IsNull;
            }

            if (IsFloat) {
                return
                    other.IsFloat &&
                    AsFloat == other.AsFloat;
            }

            if (IsBool) {
                return
                    other.IsBool &&
                    AsBool == other.AsBool;
            }

            if (IsString) {
                return
                    other.IsString &&
                    AsString == other.AsString;
            }

            if (IsList) {
                if (other.IsList == false) return false;

                var thisList = AsList;
                var otherList = other.AsList;

                if (thisList.Count != otherList.Count) return false;

                for (int i = 0; i < thisList.Count; ++i) {
                    if (thisList[i].Equals(otherList[i]) == false) {
                        return false;
                    }
                }

                return true;
            }

            if (IsDictionary) {
                if (other.IsDictionary == false) return false;

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