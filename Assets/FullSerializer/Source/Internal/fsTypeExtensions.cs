using System;
using System.Collections.Generic;
using System.Linq;

#if !UNITY_EDITOR && UNITY_WSA
// For System.Reflection.TypeExtensions
using System.Reflection;
// For Reflection Extensions in non-win10 builds.
using FullSerializer.Internal;
#endif

namespace FullSerializer {
    public static class fsTypeExtensions {
        /// <summary>
        /// Returns a pretty name for the type in the style of one that you'd see
        /// in C# without the namespace.
        /// </summary>
        public static string CSharpName(this Type type) {
            return CSharpName(type, /*includeNamespace:*/false);
        }

        public static string CSharpName(this Type type, bool includeNamespace, bool ensureSafeDeclarationName) {
            var name = CSharpName(type, includeNamespace);
            if (ensureSafeDeclarationName)
                name = name.Replace('>', '_').Replace('<', '_').Replace('.', '_').Replace(',', '_');
            return name;
        }

        /// <summary>
        /// Returns a pretty name for the type in the style of one that you'd see
        /// in C#.
        /// </summary>
        /// <parparam name="includeNamespace">
        /// Should the name include namespaces?
        /// </parparam>
        public static string CSharpName(this Type type, bool includeNamespace) {
            // we special case some of the common type names
            if (type == typeof(void)) return "void";
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(double)) return "double";
            if (type == typeof(string)) return "string";

            // Generic parameter, ie, T in Okay<T> We special-case this logic
            // otherwise we will recurse on the T
            if (type.IsGenericParameter) {
                return type.ToString();
            }

            string name = "";

            var genericArguments = (IEnumerable<Type>)type.GetGenericArguments();
            if (type.IsNested) {
                name += type.DeclaringType.CSharpName() + ".";

                // The declaring type generic parameters are considered part of
                // the nested types generic parameters so we need to remove them,
                // otherwise it will get included again.
                //
                // Say we have type `class Parent<T> { class Child {} }` If we
                // did not do the removal, then we would output
                // Parent<T>.Child<T>, but we really want to output
                // Parent<T>.Child
                if (type.DeclaringType.GetGenericArguments().Length > 0) {
                    genericArguments = genericArguments.Skip(type.DeclaringType.GetGenericArguments().Length);
                }
            }

            if (genericArguments.Any() == false) {
                name += type.Name;
            }
            else {
                var genericsTic = type.Name.IndexOf('`');
                if (genericsTic > 0) {
                    name += type.Name.Substring(0, genericsTic);
                }
                name += "<" + String.Join(",", genericArguments.Select(t => CSharpName(t, includeNamespace)).ToArray()) + ">";
            }

            if (includeNamespace && type.Namespace != null) {
                name = type.Namespace + "." + name;
            }

            return name;
        }

        public static bool IsInterface(this Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().IsInterface;
#else
            return type.IsInterface;
#endif
        }

        public static bool IsAbstract(this Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().IsAbstract;
#else
            return type.IsAbstract;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }
    }
}
