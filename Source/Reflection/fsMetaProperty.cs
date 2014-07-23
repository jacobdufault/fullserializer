using System;
using System.Reflection;

namespace FullSerializer.Internal {
    /// <summary>
    /// A property or field on a MetaType.
    /// </summary>
    public class fsMetaProperty {
        internal fsMetaProperty(FieldInfo field) {
            _memberInfo = field;
            StorageType = field.FieldType;
            Name = GetName(field);
        }

        internal fsMetaProperty(PropertyInfo property) {
            _memberInfo = property;
            StorageType = property.PropertyType;
            Name = GetName(property);
        }

        /// <summary>
        /// Internal handle to the reflected member.
        /// </summary>
        private MemberInfo _memberInfo;

        /// <summary>
        /// The type of value that is stored inside of the property. For example, for an int field,
        /// StorageType will be typeof(int).
        /// </summary>
        public Type StorageType {
            get;
            private set;
        }

        /// <summary>
        /// The serialized name of the property, as it should appear in JSON.
        /// </summary>
        public string Name {
            get;
            private set;
        }

        /// <summary>
        /// Writes a value to the property that this MetaProperty represents, using given object
        /// instance as the context.
        /// </summary>
        public void Write(object context, object value) {
            FieldInfo field = _memberInfo as FieldInfo;
            PropertyInfo property = _memberInfo as PropertyInfo;

            if (field != null) {
                field.SetValue(context, value);
            }

            else if (property != null) {
                MethodInfo setMethod = property.GetSetMethod(/*nonPublic:*/ true);
                if (setMethod != null) {
                    setMethod.Invoke(context, new object[] { value });
                }
            }
        }

        /// <summary>
        /// Reads a value from the property that this MetaProperty represents, using the given
        /// object instance as the context.
        /// </summary>
        public object Read(object context) {
            if (_memberInfo is PropertyInfo) {
                return ((PropertyInfo)_memberInfo).GetValue(context, new object[] { });
            }

            else {
                return ((FieldInfo)_memberInfo).GetValue(context);
            }
        }

        /// <summary>
        /// Returns the name the given member wants to use for JSON serialization.
        /// </summary>
        private static string GetName(MemberInfo member) {
            var attr = fsPortableReflection.GetAttribute<fsPropertyAttribute>(member);
            if (attr != null && string.IsNullOrEmpty(attr.Name) == false) {
                return attr.Name;
            }

            return member.Name;
        }

    }
}