
using System;
using System.Linq;
using System.Reflection;

namespace KellermanSoftware.CompareNetObjects
{
    /// <summary>
    /// Exclude types depending upon the configuration
    /// </summary>
    internal static class ExcludeLogic 
    {
        /// <summary>
        /// Returns true if the property or field should be excluded
        /// </summary>
        /// <param name="config"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool ShouldExcludeMember(ComparisonConfig config, MemberInfo info)
        {
            //Only compare specific field names
            if (config.MembersToInclude.Count > 0 && !config.MembersToInclude.Contains(info.Name))
                return true;

            //If we should ignore it, skip it
            if (config.MembersToIgnore.Count > 0 && config.MembersToIgnore.Contains(info.Name))
                return true;

            if (IgnoredByAttribute(config, info))
                return true;

            return false;
        }

        /// <summary>
        /// Check if the class type should be excluded based on the configuration
        /// </summary>
        /// <param name="config"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static bool ShouldExcludeClassType(ComparisonConfig config, Type t1, Type t2)
        {
            //Only include specific class types
            if (config.ClassTypesToInclude.Count > 0 
                && (!config.ClassTypesToInclude.Contains(t1) 
                    || !config.ClassTypesToInclude.Contains(t2)))
            {
                return true;
            }

            //Ignore specific class types
            if (config.ClassTypesToIgnore.Count > 0
                && (config.ClassTypesToIgnore.Contains(t1)
                    || config.ClassTypesToIgnore.Contains(t2)))
            {
                return true;
            }

            //The class is ignored by an attribute
            if (IgnoredByAttribute(config, t1))
                return true;

            return false;
        }

        /// <summary>
        /// Check if any type has attributes that should be bypassed
        /// </summary>
        /// <returns></returns>
        public static bool IgnoredByAttribute(ComparisonConfig config, MemberInfo info)
        {
            var attributes = info.GetCustomAttributes(true);

            return attributes.Any(a => config.AttributesToIgnore.Contains(a.GetType()));
        }

    }
}
