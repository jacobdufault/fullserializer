using System;
using System.Collections.Generic;
using System.Reflection;

namespace FullSerializer.Internal {
    public static class fsVersionManager {
        private static readonly Dictionary<Type, fsOption<fsVersionedType>> _cache = new Dictionary<Type, fsOption<fsVersionedType>>();

        public static fsResult GetVersionImportPath(string currentVersion, fsVersionedType targetVersion, out List<fsVersionedType> path) {
            path = new List<fsVersionedType>();

            if (GetVersionImportPathRecursive(path, currentVersion, targetVersion) == false) {
                return fsResult.Fail("There is no migration path from \"" + currentVersion + "\" to \"" + targetVersion.VersionString + "\"");
            }

            path.Add(targetVersion);
            return fsResult.Success;
        }

        private static bool GetVersionImportPathRecursive(List<fsVersionedType> path, string currentVersion, fsVersionedType current) {
            for (int i = 0; i < current.Ancestors.Length; ++i) {
                fsVersionedType ancestor = current.Ancestors[i];

                if (ancestor.VersionString == currentVersion ||
                    GetVersionImportPathRecursive(path, currentVersion, ancestor)) {
                    path.Add(ancestor);
                    return true;
                }
            }

            return false;
        }

        public static fsOption<fsVersionedType> GetVersionedType(Type type) {
            fsOption<fsVersionedType> optionalVersionedType;

            if (_cache.TryGetValue(type, out optionalVersionedType) == false) {
                var attr = fsPortableReflection.GetAttribute<fsObjectAttribute>(type);

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

                        // finally, verify that the versioned type passes some
                        // sanity checks
                        VerifyUniqueVersionStrings(versionedType);
                        VerifyConstructors(versionedType);

                        optionalVersionedType = fsOption.Just(versionedType);
                    }
                }

                _cache[type] = optionalVersionedType;
            }

            return optionalVersionedType;
        }

        /// <summary>
        /// Verifies that the given type has constructors to migrate from all
        /// ancestor types.
        /// </summary>
        private static void VerifyConstructors(fsVersionedType type) {
            ConstructorInfo[] publicConstructors = type.ModelType.GetDeclaredConstructors();

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
                    throw new fsMissingVersionConstructorException(type.ModelType, requiredConstructorType);
                }
            }
        }

        /// <summary>
        /// Verifies that the given version graph contains only unique versions.
        /// </summary>
        private static void VerifyUniqueVersionStrings(fsVersionedType type) {
            // simple tree traversal

            var found = new Dictionary<string, Type>();

            var remaining = new Queue<fsVersionedType>();
            remaining.Enqueue(type);

            while (remaining.Count > 0) {
                fsVersionedType item = remaining.Dequeue();

                // Verify we do not already have the version string. Take into
                // account that we're not just comparing the same model twice,
                // since we can have a valid import graph that has the same model
                // multiple times.
                if (found.ContainsKey(item.VersionString) && found[item.VersionString] != item.ModelType) {
                    throw new fsDuplicateVersionNameException(found[item.VersionString], item.ModelType, item.VersionString);
                }
                found[item.VersionString] = item.ModelType;

                // scan the ancestors as well
                foreach (var ancestor in item.Ancestors) {
                    remaining.Enqueue(ancestor);
                }
            }
        }
    }
}