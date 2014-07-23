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

            List<fsMetaProperty> properties = new List<fsMetaProperty>();
            CollectProperties(properties, reflectedType);
            Properties = properties.ToArray();
        }

        public Type ReflectedType;

        private static void CollectProperties(List<fsMetaProperty> properties, Type reflectedType) {
            // do we require a [SerializeField] or [fsProperty] attribute?
            bool requireAnnotation = false;
            fsObjectAttribute attr = fsPortableReflection.GetAttribute<fsObjectAttribute>(reflectedType);
            if (attr != null) {
                requireAnnotation = attr.MemberSerialization == fsMemberSerialization.OptIn;
            }

            MemberInfo[] members = reflectedType.GetDeclaredMembers();
            foreach (MemberInfo member in members) {
                // We don't serialize members annotated with [fsIgnore] or [NonSerialized].
                if (member.IsDefined(typeof(fsIgnoreAttribute), inherit: true) ||
                    member.IsDefined(typeof(NonSerializedAttribute), inherit: true)) {
                    continue;
                }

                PropertyInfo property = member as PropertyInfo;
                FieldInfo field = member as FieldInfo;

                // If an annotation is required, then skip the property if it doesn't have
                // [fsProperty] or [SerializeField] on it.
                if (requireAnnotation &&
                    (member.IsDefined(typeof(fsPropertyAttribute), inherit: true) == false &&
                     member.IsDefined(typeof(SerializeField), inherit: true) == false)) {

                    continue;
                }

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

            if (reflectedType.Resolve().BaseType != null) {
                CollectProperties(properties, reflectedType.Resolve().BaseType);
            }
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

            var publicGetMethod = property.GetGetMethod(nonPublic: false);
            var publicSetMethod = property.GetSetMethod(nonPublic: false);

            // If it's an auto-property and it has either a public get or a public set method,
            // then we serialize it
            if (IsAutoProperty(property, members) &&
                (publicGetMethod != null || publicSetMethod != null)) {
                return true;
            }

            // We do not bother to serialize static fields.
            if ((publicGetMethod != null && publicGetMethod.IsStatic) ||
                (publicSetMethod != null && publicSetMethod.IsStatic)) {
                return false;
            }

            // If a property is annotated with SerializeField or fsProperty, then it should
            // it should definitely be serialized.
            //
            // NOTE: We place this override check *after* the static check, because we *never*
            //       allow statics to be serialized.
            if (fsPortableReflection.GetAttribute<SerializeField>(property) != null ||
                fsPortableReflection.GetAttribute<fsPropertyAttribute>(property) != null) {
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

            // We don't serialize static fields
            if (field.IsStatic) {
                return false;
            }

            // We want to serialize any fields annotated with SerializeField or fsProperty.
            //
            // NOTE: This occurs *after* the static check, because we *never* want to serialize
            //       static fields.
            if (fsPortableReflection.GetAttribute<SerializeField>(field) != null ||
                fsPortableReflection.GetAttribute<fsPropertyAttribute>(field) != null) {
                return true;
            }

            // We use !IsPublic because that also checks for internal, protected, and private.
            if (!field.IsPublic) {
                return false;
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
                MemberInfo[] members = ReflectedType.GetFlattenedMember(propertyNames[i]);

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
                    if (ReflectedType.Resolve().IsArray) {
                        _hasDefaultConstructorCache = true;
                    }

                    // value types (ie, structs) always have a default constructor
                    else if (ReflectedType.Resolve().IsValueType) {
                        _hasDefaultConstructorCache = true;
                    }

                    else {
                        // consider private constructors as well
                        var ctor = ReflectedType.GetDeclaredConstructor(fsPortableReflection.EmptyTypes);
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
            if (ReflectedType.Resolve().IsInterface || ReflectedType.Resolve().IsAbstract) {
                throw new Exception("Cannot create an instance of an interface or abstract type for " + ReflectedType);
            }

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
#if !UNITY_EDITOR && (UNITY_WEBPLAYER || UNITY_WP8 || UNITY_METRO)
                throw new InvalidOperationException("The selected Unity platform requires " +
                    ReflectedType.FullName + " to have a default constructor. Please add one.");
#else
                return FormatterServices.GetSafeUninitializedObject(ReflectedType);
#endif
            }

            if (ReflectedType.Resolve().IsArray) {
                // we have to start with a size zero array otherwise it will have invalid data
                // inside of it
                return Array.CreateInstance(ReflectedType.GetElementType(), 0);
            }

            try {
                return Activator.CreateInstance(ReflectedType);
            }
#if (!UNITY_EDITOR && (UNITY_METRO)) == false
            catch (MissingMethodException e) {
                throw new InvalidOperationException("Unable to create instance of " + ReflectedType + "; there is no default constructor", e);
            }
#endif
            catch (TargetInvocationException e) {
                throw new InvalidOperationException("Constructor of " + ReflectedType + " threw an exception when creating an instance", e);
            }
            catch (MemberAccessException e) {
                throw new InvalidOperationException("Unable to access constructor of " + ReflectedType, e);
            }
        }
    }
}