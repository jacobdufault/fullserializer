using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEngine;

namespace FullJson {
    /// <summary>
    /// MetaType contains metadata about a type. This is used by the reflection serializer.
    /// </summary>
    public class MetaType {
        private static Dictionary<Type, MetaType> _metaTypes = new Dictionary<Type, MetaType>();
        public static MetaType Get(Type type) {
            MetaType metaType;
            if (_metaTypes.TryGetValue(type, out metaType) == false) {
                metaType = new MetaType(type);
                _metaTypes[type] = metaType;
            }

            return metaType;
        }

        private MetaType(Type reflectedType) {
            ReflectedType = reflectedType;
            Properties = CollectProperties(reflectedType).ToArray();
        }

        public Type ReflectedType;

        private static List<MetaProperty> CollectProperties(Type reflectedType) {
            // The binding flags that we use when scanning for properties.
            var flags =
                BindingFlags.NonPublic |
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.FlattenHierarchy;

            var properties = new List<MetaProperty>();

            foreach (MemberInfo member in reflectedType.GetMembers(flags)) {
                // We don't serialize members annotated with [JsonIgnore].
                if (Attribute.IsDefined(member, typeof(JsonIgnoreAttribute))) {
                    continue;
                }

                PropertyInfo property = member as PropertyInfo;
                FieldInfo field = member as FieldInfo;

                if (property != null) {
                    if (CanSerializeProperty(property)) {
                        properties.Add(new MetaProperty(property));
                    }
                }

                else if (field != null) {
                    if (CanSerializeField(field)) {
                        properties.Add(new MetaProperty(field));
                    }
                }
            }

            return properties;
        }

        private static bool CanSerializeProperty(PropertyInfo property) {
            // We don't serialize delegates
            if (typeof(Delegate).IsAssignableFrom(property.PropertyType)) {
                return false;
            }

            // If the property cannot be both read and written to, we don't serialize it
            if (property.CanRead == false || property.CanWrite == false) {
                return false;
            }

            // If the property is named "Item", it might be the this[int] indexer, which in that
            // case we don't serialize it We cannot just compare with "Item" because of explicit
            // interfaces, where the name of the property will be the full method name.
            if (property.Name.EndsWith("Item")) {
                ParameterInfo[] parameters = property.GetIndexParameters();
                if (parameters.Length > 0) {
                    return false;
                }
            }

            // One of the get or set methods is private, so we need to have a [SerializeField]
            // attribute. We use !IsPublic because that also checks for internal, protected, and
            // private.
            var getMethod = property.GetGetMethod();
            var setMethod = property.GetSetMethod();
            if ((getMethod != null && !getMethod.IsPublic) ||
                (setMethod != null && !setMethod.IsPublic)) {

                if (Attribute.IsDefined(property, typeof(SerializeField), inherit: true) == false) {
                    return false;
                }
            }

            return true;
        }

        private static bool CanSerializeField(FieldInfo field) {
            // We don't serialize delegates
            if (typeof(Delegate).IsAssignableFrom(field.FieldType)) {
                return false;
            }

            // We don't serialize compiler generated fields (an example would be a backing field for
            // an automatically generated property).
            if (field.IsDefined(typeof(CompilerGeneratedAttribute), false)) {
                return false;
            }

            // We use !IsPublic because that also checks for internal, protected, and private.
            if (!field.IsPublic) {
                if (Attribute.IsDefined(field, typeof(SerializeField), inherit: true) == false) {
                    return false;
                }
            }

            return true;
        }

        public MetaProperty[] Properties {
            get;
            private set;
        }

        /// <summary>
        /// Returns true if the type represented by this metadata contains a default constructor.
        /// </summary>
        public bool HasDefaultConstructor {
            get {
                if (_hasDefaultConstructorCache.HasValue == false) {
                    // arrays are considered to have a default constructor
                    if (ReflectedType.IsArray) {
                        _hasDefaultConstructorCache = true;
                    }

                    // value types (ie, structs) always have a default constructor
                    else if (ReflectedType.IsValueType) {
                        _hasDefaultConstructorCache = true;
                    }

                    else {
                        // consider private constructors as well
                        var ctor = ReflectedType.GetConstructor(
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                            null, Type.EmptyTypes, null);
                        _hasDefaultConstructorCache = ctor != null;
                    }
                }

                return _hasDefaultConstructorCache.Value;
            }
        }
        private bool? _hasDefaultConstructorCache;

        /// <summary>
        /// Creates a new instance of the type that this metadata points back to. If this type has a
        /// default constructor, then Activator.CreateInstance will be used to construct the type
        /// (or Array.CreateInstance if it an array). Otherwise, an uninitialized object created via
        /// FormatterServices.GetSafeUninitializedObject is used to construct the instance.
        /// </summary>
        public object CreateInstance() {
            // Unity requires special construction logic for types that derive from
            // ScriptableObject. The normal inspector reflection logic will create ScriptableObject
            // instances if FullInspectorSettings.AutomaticReferenceInstantation has been set to
            // true.
            if (typeof(ScriptableObject).IsAssignableFrom(ReflectedType)) {
                return ScriptableObject.CreateInstance(ReflectedType);
            }

            // Strings don't have default constructors but also fail when run through
            // FormatterSerivces.GetSafeUninitializedObject
            if (typeof(string) == ReflectedType) {
                return string.Empty;
            }

            if (HasDefaultConstructor == false) {
                return FormatterServices.GetSafeUninitializedObject(ReflectedType);
            }

            if (ReflectedType.IsArray) {
                // we have to start with a size zero array otherwise it will have invalid data
                // inside of it
                return Array.CreateInstance(ReflectedType.GetElementType(), 0);
            }

            try {
                return Activator.CreateInstance(ReflectedType, /*nonPublic:*/ true);
            }
            catch (MissingMethodException e) {
                throw new InvalidOperationException("Unable to create instance of " + ReflectedType + "; there is no default constructor", e);
            }
            catch (TargetInvocationException e) {
                throw new InvalidOperationException("Constructor of " + ReflectedType + " threw an exception when creating an instance", e);
            }
            catch (MemberAccessException e) {
                throw new InvalidOperationException("Unable to access constructor of " + ReflectedType, e);
            }
        }
    }
}