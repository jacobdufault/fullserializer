using System;
using System.Collections.Generic;
using System.Reflection;

namespace FullSerializer {
    public struct fsOption<T> {
        private bool _hasValue;
        private T _value;

        public bool HasValue {
            get { return _hasValue; }
        }
        public bool IsEmpty {
            get { return _hasValue == false; }
        }
        public T Value {
            get {
                if (IsEmpty) throw new InvalidOperationException("fsOption is empty");
                return _value;
            }
        }

        public fsOption(T value) {
            _hasValue = true;
            _value = value;
        }

        public static fsOption<T> Empty;
    }

    public static class fsOption {
        public static fsOption<T> Just<T>(T value) {
            return new fsOption<T>(value);
        }
    }

    public class fsVersionedImport {
        private static Dictionary<Type, fsOption<fsVersionedType>> _cache = new Dictionary<Type, fsOption<fsVersionedType>>();

        public static List<fsVersionedType> GetVersionImportPath(string currentVersion, fsVersionedType targetVersion) {
            throw new NotImplementedException();
        }

        public static fsOption<fsVersionedType> GetVersionedType(Type type) {
            fsOption<fsVersionedType> optionalVersionedType;

            if (_cache.TryGetValue(type, out optionalVersionedType) == false) {
                fsObjectAttribute attr = (fsObjectAttribute)Attribute.GetCustomAttribute(type, typeof(fsObjectAttribute), inherit: true);

                if (attr != null) {
                    if (string.IsNullOrEmpty(attr.VersionString) == false || attr.PreviousModels != null) {
                        // Version string must be provided
                        if (attr.PreviousModels != null && string.IsNullOrEmpty(attr.VersionString)) {
                            throw new Exception("fsObject attribute on " + type + " contains a PreviousModels specifier - it must also include a VersionString modifier");
                        }

                        // Map the ancestor types into versioned types
                        fsVersionedType[] ancestors = new fsVersionedType[attr.PreviousModels != null ? attr.PreviousModels.Length : 0];
                        for (int i = 0; i < ancestors.Length; ++i) {
                            fsOption<fsVersionedType> ancestorType = GetVersionedType(attr.PreviousModels[i]);
                            if (ancestorType.IsEmpty) {
                                throw new Exception("Unable to create versioned type for ancestor " + ancestorType + "; please add an [fsObject(VersionString=\"...\")] attribute");
                            }
                            ancestors[i] = ancestorType.Value;
                        }

                        // construct the actual versioned type instance
                        fsVersionedType versionedType = new fsVersionedType {
                            Ancestors = ancestors,
                            VersionString = attr.VersionString,
                            ModelType = type
                        };

                        // finally, verify that the versioned type passes some sanity checks
                        VerifyUniqueVersionStrings(versionedType);
                        VerifyConstructors(versionedType);

                        optionalVersionedType = fsOption.Just(versionedType);
                    }
                }

                _cache[type] = optionalVersionedType;
            }

            return optionalVersionedType;
        }

        private static void VerifyConstructors(fsVersionedType type) {
            var flags =
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly | BindingFlags.Instance;

            ConstructorInfo[] publicConstructors = type.ModelType.GetConstructors(flags);
            
            for (int i = 0; i < type.Ancestors.Length; ++i) {
                Type requiredConstructorType = type.Ancestors[i].ModelType;

                bool found = false;
                for (int j = 0; j < publicConstructors.Length; ++j) {
                    var parameters = publicConstructors[j].GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == requiredConstructorType) {
                        found = true;
                        break;
                    }
                }

                if (found == false) {
                    throw new Exception(type.ModelType + " is missing a required constructor for previous model type " + requiredConstructorType);
                }
            }
        }

        private static void VerifyUniqueVersionStrings(fsVersionedType type) {
            // simple tree traversal

            var found = new Dictionary<string, Type>();

            var remaining = new Queue<fsVersionedType>();
            remaining.Enqueue(type);

            while (remaining.Count > 0) {
                fsVersionedType item = remaining.Dequeue();

                // verify we do not already have the version string
                if (found.ContainsKey(item.VersionString)) {
                    throw new Exception("Types " + found[item.VersionString] + " and " + item.ModelType + " cannot have the same version string (" + item.VersionString + ")");
                }
                found[item.VersionString] = item.ModelType;

                // scan the ancestors as well
                foreach (var ancestor in item.Ancestors) {
                    remaining.Enqueue(ancestor);
                }
            }
        }
    }

    public struct fsVersionedType {
        /// <summary>
        /// The direct ancestors that this type can import.
        /// </summary>
        public fsVersionedType[] Ancestors;

        /// <summary>
        /// The identifying string that is unique among all ancestors.
        /// </summary>
        public string VersionString;

        /// <summary>
        /// The modeling type that this versioned type maps back to.
        /// </summary>
        public Type ModelType;

        public override string ToString() {
            return "fsVersionedType [ModelType=" + ModelType + ", VersionString=" + VersionString + ", Ancestors.Length=" + Ancestors.Length + "]";
        }
    }
}