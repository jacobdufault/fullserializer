using FullSerializer.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEngine;

namespace FullSerializer {
    /// <summary>
    /// MetaType contains metadata about a type. This is used by the reflection serializer.
    /// </summary>
    public class fsMetaType {
        static fsMetaType() {
            // Setup properties for Unity types that don't work well with the auto-rules.
            fsMetaType.Get(typeof(Bounds)).SetProperties("center", "size");
            fsMetaType.Get(typeof(Keyframe)).SetProperties("time", "value", "tangentMode", "inTangent", "outTangent");
            fsMetaType.Get(typeof(AnimationCurve)).SetProperties("keys", "preWrapMode", "postWrapMode");
            fsMetaType.Get(typeof(LayerMask)).SetProperties("value");
            fsMetaType.Get(typeof(Gradient)).SetProperties("alphaKeys", "colorKeys");
            fsMetaType.Get(typeof(Rect)).SetProperties("xMin", "yMin", "xMax", "yMax");
        }

        /// <summary>
        /// The binding flags that we use when looking up properties.
        /// </summary>
        private static BindingFlags PropertyLookupFlags =
            BindingFlags.NonPublic |
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.FlattenHierarchy;

        private static Dictionary<Type, fsMetaType> _metaTypes = new Dictionary<Type, fsMetaType>();
        public static fsMetaType Get(Type type) {
            fsMetaType metaType;
            if (_metaTypes.TryGetValue(type, out metaType) == false) {
                metaType = new fsMetaType(type);
                _metaTypes[type] = metaType;
            }

            return metaType;
        }

        private fsMetaType(Type reflectedType) {
            ReflectedType = reflectedType;
            Properties = CollectProperties(reflectedType).ToArray();
        }

        public Type ReflectedType;

        private static List<fsMetaProperty> CollectProperties(Type reflectedType) {
            var properties = new List<fsMetaProperty>();

            MemberInfo[] members = reflectedType.GetMembers(PropertyLookupFlags);
            foreach (MemberInfo member in members) {
                // We don't serialize members annotated with [fsIgnore] or [NonSerialized].
                if (Attribute.IsDefined(member, typeof(fsIgnoreAttribute)) ||
                    Attribute.IsDefined(member, typeof(NonSerializedAttribute))) {
                    continue;
                }

                PropertyInfo property = member as PropertyInfo;
                FieldInfo field = member as FieldInfo;

                if (property != null) {
                    if (CanSerializeProperty(property, members)) {
                        properties.Add(new fsMetaProperty(property));
                    }
                }

                else if (field != null) {
                    if (CanSerializeField(field)) {
                        properties.Add(new fsMetaProperty(field));
                    }
                }
            }

            return properties;
        }

        private static bool IsAutoProperty(PropertyInfo property, MemberInfo[] members) {
            if (!property.CanWrite || !property.CanRead) {
                return false;
            }

            string backingFieldName = "<" + property.Name + ">k__BackingField";
            for (int i = 0; i < members.Length; ++i) {
                if (members[i].Name == backingFieldName) {
                    return true;
                }
            }

            return false;
        }

        private static bool CanSerializeProperty(PropertyInfo property, MemberInfo[] members) {
            // We don't serialize delegates
            if (typeof(Delegate).IsAssignableFrom(property.PropertyType)) {
                return false;
            }

            // If the property cannot be both read and written to, we cannot serialize it
            if (property.CanRead == false || property.CanWrite == false) {
                return false;
            }

            // If a property is annotated with SerializeField, it should definitely be serialized
            if (Attribute.IsDefined(property, typeof(SerializeField))) {
                return true;
            }

            var publicGetMethod = property.GetGetMethod(nonPublic: false);
            var publicSetMethod = property.GetSetMethod(nonPublic: false);

            // If it's an auto-property and it has either a public get or a public set method,
            // then we serialize it
            if (IsAutoProperty(property, members) &&
                (publicGetMethod != null || publicSetMethod != null)) {
                return true;
            }

            // Otherwise, we don't bother with serialization
            return false;
        }

        private static bool CanSerializeField(FieldInfo field) {
            // We don't serialize delegates
            if (typeof(Delegate).IsAssignableFrom(field.FieldType)) {
                return false;
            }

            // We don't serialize compiler generated fields.
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

        public fsMetaProperty[] Properties {
            get;
            private set;
        }

        /// <summary>
        /// Override the default property names and use the given ones instead of serialization.
        /// </summary>
        public void SetProperties(params string[] propertyNames) {
            Properties = new fsMetaProperty[propertyNames.Length];

            for (int i = 0; i < propertyNames.Length; ++i) {
                MemberInfo[] members = ReflectedType.GetMember(propertyNames[i], PropertyLookupFlags);

                if (members.Length == 0) {
                    throw new InvalidOperationException("Unable to find property " +
                        propertyNames[i] + " on " + ReflectedType.Name);
                }
                if (members.Length > 1) {
                    throw new InvalidOperationException("More than one property matches " +
                        propertyNames[i] + " on " + ReflectedType.Name);
                }

                MemberInfo member = members[0];
                if (member is FieldInfo) {
                    Properties[i] = new fsMetaProperty((FieldInfo)member);
                }
                else {
                    Properties[i] = new fsMetaProperty((PropertyInfo)member);
                }
            }
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
            // ScriptableObject.
            if (typeof(ScriptableObject).IsAssignableFrom(ReflectedType)) {
                return ScriptableObject.CreateInstance(ReflectedType);
            }

            // Strings don't have default constructors but also fail when run through
            // FormatterSerivces.GetSafeUninitializedObject
            if (typeof(string) == ReflectedType) {
                return string.Empty;
            }

            if (HasDefaultConstructor == false) {
#if !UNITY_EDITOR && (UNITY_WEBPLAYER || UNITY_WP8)
                throw new InvalidOperationException("The selected Unity platform requires " +
                    ReflectedType.FullName + " to have a default constructor. Please add one.");
#else
                return FormatterServices.GetSafeUninitializedObject(ReflectedType);
#endif
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