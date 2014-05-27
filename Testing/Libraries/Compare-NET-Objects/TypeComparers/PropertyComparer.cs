using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Compare two properties (Note inherits from BaseComparer instead of TypeComparer
    /// </summary>
    public class PropertyComparer : BaseComparer
    {
        private readonly RootComparer _rootComparer;
        private readonly IndexerComparer _indexerComparer;

        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public PropertyComparer(RootComparer rootComparer)
        {
            _rootComparer = rootComparer;
            _indexerComparer = new IndexerComparer(rootComparer);
        }

        /// <summary>
        /// Compare the properties of a class
        /// </summary>
        /// <param name="t1">The type of the property</param>
        /// <param name="object1">The first object to compare</param>
        /// <param name="object2">The second object to compare</param>
        /// <param name="structCompare">True if a struct</param>
        /// <param name="result">The Comparison Result</param>
        /// <param name="breadCrumb"></param>
        public void PerformCompareProperties(Type t1,
            object object1,
            object object2,
            bool structCompare,
            ComparisonResult result,
            string breadCrumb)
        {
            IEnumerable<PropertyInfo> currentProperties = null;

            //Interface Member Logic
            if (result.Config.InterfaceMembers.Count > 0)
            {
                Type[] interfaces = t1.GetInterfaces();

                foreach (var type in result.Config.InterfaceMembers)
                {
                    if (interfaces.Contains(type))
                    {
                        currentProperties = Cache.GetPropertyInfo(result, type);
                        break;
                    }
                }
            }

            if (currentProperties == null)
                currentProperties = Cache.GetPropertyInfo(result, t1);

            foreach (PropertyInfo info in currentProperties)
            {
                //Ignore invalid struct fields
                if (structCompare && !TypeHelper.ValidStructSubType(info.PropertyType))
                    continue;

                //If we can't read it, skip it
                if (info.CanRead == false)
                    continue;

                //Skip if this is a shallow compare
                if (!result.Config.CompareChildren && TypeHelper.CanHaveChildren(info.PropertyType))
                    continue;

                //Skip if it should be excluded based on the configuration
                if (ExcludeLogic.ShouldExcludeMember(result.Config, info))
                    continue;    

                //If we should ignore read only, skip it
                if (!result.Config.CompareReadOnly && info.CanWrite == false)
                    continue;

                //If we ignore types then we must get correct PropertyInfo object
                PropertyInfo secondObjectInfo = null;
                if (result.Config.IgnoreObjectTypes)
                {
                    var secondObjectPropertyInfos = Cache.GetPropertyInfo(result, object2.GetType());

                    foreach (var propertyInfo in secondObjectPropertyInfos)
                    {
                        if (propertyInfo.Name != info.Name) continue;

                        secondObjectInfo = propertyInfo;
                        break;
                    }
                }
                else
                    secondObjectInfo = info;

                object objectValue1;
                object objectValue2;
                if (!IsValidIndexer(result.Config, info, breadCrumb))
                {
                    objectValue1 = info.GetValue(object1, null);
                    objectValue2 = secondObjectInfo != null ? secondObjectInfo.GetValue(object2, null) : null;
                }
                else
                {
                    _indexerComparer.CompareIndexer(result, info, object1, object2, breadCrumb);
                    continue;
                }

                bool object1IsParent = objectValue1 != null && (objectValue1 == object1 || result.Parents.ContainsKey(objectValue1.GetHashCode()));
                bool object2IsParent = objectValue2 != null && (objectValue2 == object2 || result.Parents.ContainsKey(objectValue2.GetHashCode()));

                //Skip properties where both point to the corresponding parent
                if ((TypeHelper.IsClass(info.PropertyType) || TypeHelper.IsStruct(info.PropertyType)) && (object1IsParent && object2IsParent))
                {
                    continue;
                }

                string currentCrumb = AddBreadCrumb(result.Config, breadCrumb, info.Name);

                _rootComparer.Compare(result, objectValue1, objectValue2, currentCrumb);

                if (result.ExceededDifferences)
                    return;
            }
        }

        private bool IsValidIndexer(ComparisonConfig config, PropertyInfo info, string breadCrumb)
        {
            ParameterInfo[] indexers = info.GetIndexParameters();

            if (indexers.Length == 0)
            {
                return false;
            }

            if (indexers.Length > 1)
            {
                if (!config.SkipInvalidIndexers)
                    throw new Exception("Cannot compare objects with more than one indexer for object " + breadCrumb);
            }

            if (indexers[0].ParameterType != typeof(Int32))
            {
                if (!config.SkipInvalidIndexers)
                    throw new Exception("Cannot compare objects with a non integer indexer for object " + breadCrumb);
            }

            if (info.ReflectedType.GetProperty("Count") == null)
            {
                if (!config.SkipInvalidIndexers)
                    throw new Exception("Indexer must have a corresponding Count property for object " + breadCrumb);
            }

            if (info.ReflectedType.GetProperty("Count").PropertyType != typeof(Int32))
            {
                if (!config.SkipInvalidIndexers)
                    throw new Exception("Indexer must have a corresponding Count property that is an integer for object " + breadCrumb);
            }

            return true;
        }
    }
}
