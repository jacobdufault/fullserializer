#if !UNITY_EDITOR && UNITY_METRO
#define USE_TYPEINFO
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if USE_TYPEINFO
namespace System {
    public static class AssemblyExtensions {
        public static Type[] GetTypes(this Assembly assembly) {
            TypeInfo[] infos = assembly.DefinedTypes.ToArray();
            Type[] types = new Type[infos.Length];
            for (int i = 0; i < infos.Length; ++i) {
                types[i] = infos[i].AsType();
            }
            return types;
        }

        public static Type GetType(this Assembly assembly, string name, bool throwOnError) {
            var types = assembly.GetTypes();
            for (int i = 0; i < types.Length; ++i) {
                if (types[i].Name == name) {
                    return types[i];
                }   
            }

            if (throwOnError) throw new Exception("Type " + name + " was not found");
            return null;
        }
    }
}
#endif

namespace FullSerializer.Internal {
    /// <summary>
    /// This wraps reflection types so that it is portable across different Unity runtimes.
    /// </summary>
    public static class fsPortableReflection {
        public static Type[] EmptyTypes = new Type[] { };

#if USE_TYPEINFO
        public static TAttribute GetAttribute<TAttribute>(Type type)
            where TAttribute : Attribute {

            return GetAttribute<TAttribute>(type.GetTypeInfo());
        }
#endif

        public static TAttribute GetAttribute<TAttribute>(MemberInfo memberInfo)
            where TAttribute : Attribute {

            return (TAttribute)memberInfo.GetCustomAttributes(typeof(TAttribute), inherit: true).FirstOrDefault();
        }

#if !USE_TYPEINFO
        private static BindingFlags DeclaredFlags =
            BindingFlags.NonPublic |
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.DeclaredOnly;
#endif

        public static PropertyInfo GetDeclaredProperty(this Type type, string propertyName) {
            var props = GetDeclaredProperties(type);

            for (int i = 0; i < props.Length; ++i) {
                if (props[i].Name == propertyName) {
                    return props[i];
                }
            }

            return null;
        }

        public static MethodInfo GetDeclaredMethod(this Type type, string methodName) {
            var methods = GetDeclaredMethods(type);

            for (int i = 0; i < methods.Length; ++i) {
                if (methods[i].Name == methodName) {
                    return methods[i];
                }
            }

            return null;
        }


        public static ConstructorInfo GetDeclaredConstructor(this Type type, Type[] parameters) {
            var ctors = GetDeclaredConstructors(type);

            for (int i = 0; i < ctors.Length; ++i) {
                var ctor = ctors[i];
                var ctorParams = ctor.GetParameters();

                if (parameters.Length != ctorParams.Length) continue;

                for (int j = 0; j < ctorParams.Length; ++j) {
                    // require an exact match
                    if (ctorParams[j].ParameterType != parameters[j]) continue;
                }

                return ctor;
            }

            return null;
        }

        public static ConstructorInfo[] GetDeclaredConstructors(this Type type) {
#if USE_TYPEINFO
            return type.GetTypeInfo().DeclaredConstructors.ToArray();
#else
            return type.GetConstructors(DeclaredFlags);
#endif
        }

        public static MemberInfo[] GetFlattenedMember(this Type type, string memberName) {
            var result = new List<MemberInfo>();

            while (type != null) {
                var members = GetDeclaredMembers(type);

                for (int i = 0; i < members.Length; ++i) {
                    if (members[i].Name == memberName) {
                        result.Add(members[i]);
                    }
                }

                type = type.Resolve().BaseType;
            }

            return result.ToArray();
        }

        public static MethodInfo GetFlattenedMethod(this Type type, string methodName) {
            while (type != null) {
                var methods = GetDeclaredMethods(type);

                for (int i = 0; i < methods.Length; ++i) {
                    if (methods[i].Name == methodName) {
                        return methods[i];
                    }
                }

                type = type.Resolve().BaseType;
            }

            return null;
        }

        public static PropertyInfo GetFlattenedProperty(this Type type, string propertyName) {
            while (type != null) {
                var properties = GetDeclaredProperties(type);

                for (int i = 0; i < properties.Length; ++i) {
                    if (properties[i].Name == propertyName) {
                        return properties[i];
                    }
                }

                type = type.Resolve().BaseType;
            }

            return null;
        }

        public static MemberInfo GetDeclaredMember(this Type type, string memberName) {
            var members = GetDeclaredMembers(type);

            for (int i = 0; i < members.Length; ++i) {
                if (members[i].Name == memberName) {
                    return members[i];
                }
            }

            return null;
        }

        public static MethodInfo[] GetDeclaredMethods(this Type type) {
#if USE_TYPEINFO
            return type.GetTypeInfo().DeclaredMethods.ToArray();
#else
            return type.GetMethods(DeclaredFlags);
#endif
        }

        public static PropertyInfo[] GetDeclaredProperties(this Type type) {
#if USE_TYPEINFO
            return type.GetTypeInfo().DeclaredProperties.ToArray();
#else
            return type.GetProperties(DeclaredFlags);
#endif
        }

        public static MemberInfo[] GetDeclaredMembers(this Type type) {
#if USE_TYPEINFO
            return type.GetTypeInfo().DeclaredMembers.ToArray();
#else
            return type.GetMembers(DeclaredFlags);
#endif
        }

        public static MemberInfo AsMemberInfo(Type type) {
#if USE_TYPEINFO
            return type.GetTypeInfo();
#else
            return type;
#endif
        }

        public static bool IsType(MemberInfo member) {
#if USE_TYPEINFO
            return member is TypeInfo;
#else
            return member is Type;
#endif
        }

        public static Type AsType(MemberInfo member) {
#if USE_TYPEINFO
            return ((TypeInfo)member).AsType();
#else
            return (Type)member;
#endif
        }

#if USE_TYPEINFO
        public static TypeInfo Resolve(this Type type) {
            return type.GetTypeInfo();
        }
#else
        public static Type Resolve(this Type type) {
            return type;
        }
#endif


        #region Extensions

#if USE_TYPEINFO
        public static bool IsAssignableFrom(this Type parent, Type child) {
            return parent.GetTypeInfo().IsAssignableFrom(child.GetTypeInfo());
        }

        public static Type GetElementType(this Type type) {
            return type.GetTypeInfo().GetElementType();
        }

        public static MethodInfo GetSetMethod(this PropertyInfo member, bool nonPublic = false) {
            // only public requested but the set method is not public
            if (nonPublic == false && member.SetMethod != null && member.SetMethod.IsPublic == false) return null;

            return member.SetMethod;
        }

        public static MethodInfo GetGetMethod(this PropertyInfo member, bool nonPublic = false) {
            // only public requested but the set method is not public
            if (nonPublic == false && member.GetMethod != null && member.GetMethod.IsPublic == false) return null;

            return member.GetMethod;
        }

        public static MethodInfo GetBaseDefinition(this MethodInfo method) {
            return method.GetRuntimeBaseDefinition();
        }

        public static Type[] GetInterfaces(this Type type) {
            return type.GetTypeInfo().ImplementedInterfaces.ToArray();
        }

        public static Type[] GetGenericArguments(this Type type) {
            return type.GetTypeInfo().GenericTypeArguments.ToArray();
        }

        private const BindingFlags Default = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        public static ConstructorInfo GetConstructor(this Type type, Type[] paramTypes) {
            return GetConstructors(type, Default).FirstOrDefault(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(paramTypes));
        }

        public static ConstructorInfo[] GetConstructors(this Type type) {
            return GetConstructors(type, Default);
        }

        public static ConstructorInfo[] GetConstructors(this Type type, BindingFlags flags) {
            var props = type.GetTypeInfo().DeclaredConstructors;
            return props.Where(p =>
              ((flags.HasFlag(BindingFlags.Static) == p.IsStatic) ||
               (flags.HasFlag(BindingFlags.Instance) == !p.IsStatic)
              ) &&
              ((flags.HasFlag(BindingFlags.Public) == p.IsPublic) ||
                (flags.HasFlag(BindingFlags.NonPublic) == p.IsPrivate)
              )).ToArray();
        }
#endif
        #endregion
    }
}