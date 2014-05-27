using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KellermanSoftware.CompareNetObjects
{
    /// <summary>
    /// Cache for properties, fields, and methods to speed up reflection
    /// </summary>
    public static class Cache
    {
        /// <summary>
        /// Reflection Cache for property info
        /// </summary>
        private static readonly Dictionary<Type, PropertyInfo[]> _propertyCache;

        /// <summary>
        /// Reflection Cache for field info
        /// </summary>
        private static readonly Dictionary<Type, FieldInfo[]> _fieldCache;

        /// <summary>
        /// Reflection Cache for methods
        /// </summary>
        private static readonly Dictionary<Type, MethodInfo[]> _methodList;

        /// <summary>
        /// Static constructor
        /// </summary>
        static Cache()
        {
            _propertyCache = new Dictionary<Type, PropertyInfo[]>();
            _fieldCache = new Dictionary<Type, FieldInfo[]>();
            _methodList = new Dictionary<Type, MethodInfo[]>();
        }

        /// <summary>
        /// Clear the cache
        /// </summary>
        public static void ClearCache()
        {
            _propertyCache.Clear();
            _fieldCache.Clear();
            _methodList.Clear();
        }

        /// <summary>
        /// Get a list of the fields within a type
        /// </summary>
        /// <param name="config"> </param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetFieldInfo(ComparisonConfig config, Type type)
        {
            lock (_fieldCache)
            {
                if (config.Caching && _fieldCache.ContainsKey(type))
                    return _fieldCache[type];

                FieldInfo[] currentFields;

#if !PORTABLE
                if (config.ComparePrivateFields && !config.CompareStaticFields)
                {
                    List<FieldInfo> list = new List<FieldInfo>();
                    Type t = type;
                    do
                    {
                        list.AddRange(t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                        t = t.BaseType;
                    } while (t != null);
                    currentFields = list.ToArray();
                }
                else if (config.ComparePrivateFields && config.CompareStaticFields)
                {
                    List<FieldInfo> list = new List<FieldInfo>();
                    Type t = type;
                    do
                    {
                        list.AddRange(
                            t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic |
                                        BindingFlags.Static));
                        t = t.BaseType;
                    } while (t != null);
                    currentFields = list.ToArray();
                }
                else
#endif
                    currentFields = type.GetFields(); //Default is public instance and static

                if (config.Caching)
                    _fieldCache.Add(type, currentFields);

                return currentFields;
            }
        }

        /// <summary>
        /// Get the value of a property
        /// </summary>
        /// <param name="result"> </param>
        /// <param name="type"></param>
        /// <param name="objectValue"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object GetPropertyValue(ComparisonResult result, Type type, object objectValue, string propertyName)
        {
            return GetPropertyInfo(result, type).First(o => o.Name == propertyName).GetValue(objectValue, null);
        }

        /// <summary>
        /// Get a list of the properties in a type
        /// </summary>
        /// <param name="result"> </param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetPropertyInfo(ComparisonResult result, Type type)
        {
            lock (_propertyCache)
            {
                if (result.Config.Caching && _propertyCache.ContainsKey(type))
                    return _propertyCache[type];

                PropertyInfo[] currentProperties;

#if PORTABLE
            if (!result.Config.CompareStaticProperties)
                currentProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            else
                currentProperties = type.GetProperties(); //Default is public instance and static
#else
                if (result.Config.ComparePrivateProperties && !result.Config.CompareStaticProperties)
                    currentProperties =
                        type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                else if (result.Config.ComparePrivateProperties && result.Config.CompareStaticProperties)
                    currentProperties =
                        type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic |
                                           BindingFlags.Static);
                else if (!result.Config.CompareStaticProperties)
                    currentProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                else
                    currentProperties = type.GetProperties(); //Default is public instance and static
#endif

                if (result.Config.Caching)
                    _propertyCache.Add(type, currentProperties);

                return currentProperties;
            }
        }

        /// <summary>
        /// Get a method by name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static MethodInfo GetMethod(Type type, string methodName)
        {
            return GetMethods(type).FirstOrDefault(m => m.Name == methodName);
        }

        /// <summary>
        /// Get the cached methods for a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<MethodInfo> GetMethods(Type type)
        {
            lock (_methodList)
            {
                if (_methodList.ContainsKey(type))
                    return _methodList[type];

                MethodInfo[] myMethodInfo = type.GetMethods();
                _methodList.Add(type, myMethodInfo);
                return myMethodInfo;
            }
        }

    }
}
