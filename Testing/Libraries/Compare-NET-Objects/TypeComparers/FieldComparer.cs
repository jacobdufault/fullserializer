using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Compare all the fields of a class or struct (Note this derrives from BaseComparer, not TypeComparer)
    /// </summary>
    public class FieldComparer : BaseComparer
    {
        private readonly RootComparer _rootComparer;

        /// <summary>
        /// Constructor with a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public FieldComparer(RootComparer rootComparer)
        {
            _rootComparer = rootComparer;
        }

        /// <summary>
        /// Compare the fields of a class
        /// </summary>
        /// <param name="result">The comparison result</param>
        /// <param name="t1">The type of the first object</param>
        /// <param name="object1">The first object to compare</param>
        /// <param name="object2">The second object to compare</param>
        /// <param name="structCompare">If true, we are comparing a structure</param>
        /// <param name="breadCrumb">The breadcrumb</param>
        public void PerformCompareFields(ComparisonResult result, Type t1,
            object object1,
            object object2,
            bool structCompare,
            string breadCrumb)
        {
            IEnumerable<FieldInfo> currentFields = null;

            //Interface Member Logic
            if (result.Config.InterfaceMembers.Count > 0)
            {
                Type[] interfaces = t1.GetInterfaces();

                foreach (var type in result.Config.InterfaceMembers)
                {
                    if (interfaces.Contains(type))
                    {
                        currentFields = Cache.GetFieldInfo(result.Config, type);
                        break;
                    }
                }
            }

            if (currentFields == null)
                currentFields = Cache.GetFieldInfo(result.Config, t1);


            foreach (FieldInfo item in currentFields)
            {
                //Ignore invalid struct fields
                if (structCompare && !TypeHelper.ValidStructSubType(item.FieldType))
                    continue;

                //Skip if this is a shallow compare
                if (!result.Config.CompareChildren && TypeHelper.CanHaveChildren(item.FieldType))
                    continue;

                //Skip if it should be excluded based on the configuration
                if (ExcludeLogic.ShouldExcludeMember(result.Config, item))
                    continue;                

                object objectValue1 = item.GetValue(object1);
                object objectValue2 = item.GetValue(object2);

                bool object1IsParent = objectValue1 != null && (objectValue1 == object1 || result.Parents.ContainsKey(objectValue1.GetHashCode()));
                bool object2IsParent = objectValue2 != null && (objectValue2 == object2 || result.Parents.ContainsKey(objectValue2.GetHashCode()));

                //Skip fields that point to the parent
                if (TypeHelper.IsClass(item.FieldType)
                    && (object1IsParent || object2IsParent))
                {
                    continue;
                }

                string currentCrumb = AddBreadCrumb(result.Config, breadCrumb, item.Name);

                _rootComparer.Compare(result, objectValue1, objectValue2, currentCrumb);

                if (result.ExceededDifferences)
                    return;
            }
        }


    }
}
