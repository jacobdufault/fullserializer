using System;
using System.Reflection;

namespace FullSerializer.Internal {
    /// <summary>
    /// Provides APIs for looking up types based on their name.
    /// </summary>
    internal static class fsTypeLookup {
        /// <summary>
        /// Attempts to lookup the given type. Returns null if the type lookup fails.
        /// </summary>
        public static Type GetType(string typeName) {
            Type type = null;

            // Try a direct type lookup
            type = Type.GetType(typeName);
            if (type != null) {
                return type;
            }

#if (!UNITY_EDITOR && UNITY_METRO) == false // no AppDomain on WinRT
            // If we still haven't found the proper type, we can enumerate all of the loaded
            // assemblies and see if any of them define the type
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                // See if that assembly defines the named type
                type = assembly.GetType(typeName);
                if (type != null) {
                    return type;
                }
            }
#endif

            return null;
        }
    }
}