using System;

#if !UNITY_EDITOR && UNITY_WSA
// For System.Reflection.TypeExtensions
using System.Reflection;
#endif

namespace FullSerializer.Internal {
    public static class fsReflectionUtility {
        /// <summary>
        /// Searches for a particular implementation of the given interface type
        /// inside of the type. This is particularly useful if the interface type
        /// is an open type, ie, typeof(IFace{}), because this method will then
        /// return IFace{} but with appropriate type parameters inserted.
        /// </summary>
        /// <param name="type">The base type to search for interface</param>
        /// <param name="interfaceType">
        /// The interface type to search for. Can be an open generic type.
        /// </param>
        /// <returns>
        /// The actual interface type that the type contains, or null if there is
        /// no implementation of the given interfaceType on type.
        /// </returns>
        public static Type GetInterface(Type type, Type interfaceType) {
            if (interfaceType.Resolve().IsGenericType &&
                interfaceType.Resolve().IsGenericTypeDefinition == false) {
                throw new ArgumentException("GetInterface requires that if the interface " +
                    "type is generic, then it must be the generic type definition, not a " +
                    "specific generic type instantiation");
            };

            while (type != null) {
                foreach (var iface in type.GetInterfaces()) {
                    if (iface.Resolve().IsGenericType) {
                        if (interfaceType == iface.GetGenericTypeDefinition()) {
                            return iface;
                        }
                    }
                    else if (interfaceType == iface) {
                        return iface;
                    }
                }

                type = type.Resolve().BaseType;
            }

            return null;
        }
    }
}