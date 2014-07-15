using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FullSerializer {
    /// <summary>
    /// The actual type that a JsonData instance can store.
    /// </summary>
    public enum fsDataType {
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
    public sealed class fsData {
        /// <summary>
        /// The raw value that this serialized data stores. It can be one of six different types; a
        /// boolean, a float, a string, a Dictionary, or a List.
        /// </summary>
        private object _value;

        #region Constructors
        /// <summary>
        /// Creates a SerializedData instance that holds null.
        /// </summary>
        public fsData() {
            _value = null;
        }

        /// <summary>
        /// Creates a SerializedData instance that holds a boolean.
        /// </summary>
        public fsData(bool boolean) {
            _value = boolean;
        }

        /// <summary>
        /// Creates a SerializedData instance that holds a float.
        /// </summary>
        public fsData(float f) {
            _value = f;
        }

        /// <summary>
        /// Creates a SerializedData instance that holds a string.
        /// </summary>
        public fsData(string str) {
            _value = str;
        }

        /// <summary>
        /// Creates a SerializedData instance that holds a dictionary of values.
        /// </summary>
        public fsData(Dictionary<string, fsData> dict) {
            _value = dict;
        }

        /// <summary>
        /// Creates a SerializedData instance that holds a list of values.
        /// </summary>
        public fsData(List<fsData> list) {
            _value = list;
        }

        /// <summary>
        /// Helper method to create a SerializedData instance that holds a dictionary.
        /// </summary>
        public static fsData CreateDictionary() {
            return new fsData(new Dictionary<string, fsData>());
        }

        /// <summary>
        /// Helper method to create a SerializedData instance that holds a list.
        /// </summary>
        public static fsData CreateList() {
            return new fsData(new List<fsData>());
        }
        #endregion

        #region Casting Predicates
        public fsDataType Type {
            get {
                if (_value == null) return fsDataType.Null;
                if (_value is float) return fsDataType.Number;
                if (_value is bool) return fsDataType.Boolean;
                if (_value is string) return fsDataType.String;
                if (_value is Dictionary<string, fsData>) return fsDataType.Object;
                if (_value is List<fsData>) return fsDataType.Array;

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
                return _value is Dictionary<string, fsData>;
            }
        }

        /// <summary>
        /// Returns true if this SerializedData instance maps back to a List.
        /// </summary>
        public bool IsList {
            get {
                return _value is List<fsData>;
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
        public Dictionary<string, fsData> AsDictionary {
            get {
                return Cast<Dictionary<string, fsData>>();
            }
        }

        /// <summary>
        /// Casts this SerializedData to a List. Throws an exception if it is not a List.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public List<fsData> AsList {
            get {
                return Cast<List<fsData>>();
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

            throw new InvalidCastException("Unable to cast <" + this + "> (with type = " +
                _value.GetType() + ") to type " + typeof(T));
        }
        #endregion

        #region ToString Implementation
        public override string ToString() {
            return fsJsonPrinter.CompressedJson(this);
        }
        #endregion

        #region Equality Comparisons
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object obj) {
            return Equals(obj as fsData);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public bool Equals(fsData other) {
            if (other == null || Type != other.Type) {
                return false;
            }

            switch (Type) {
                case fsDataType.Null:
                    return true;

                case fsDataType.Number:
                    return AsFloat == other.AsFloat;

                case fsDataType.Boolean:
                    return AsBool == other.AsBool;

                case fsDataType.String:
                    return AsString == other.AsString;

                case fsDataType.Array:
                    var thisList = AsList;
                    var otherList = other.AsList;

                    if (thisList.Count != otherList.Count) return false;

                    for (int i = 0; i < thisList.Count; ++i) {
                        if (thisList[i].Equals(otherList[i]) == false) {
                            return false;
                        }
                    }

                    return true;

                case fsDataType.Object:
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
        public static bool operator ==(fsData a, fsData b) {
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
        public static bool operator !=(fsData a, fsData b) {
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