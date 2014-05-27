using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using KellermanSoftware.CompareNetObjects.IgnoreOrderTypes;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Logic to compare two hash sets
    /// </summary>
    public class HashSetComparer : BaseTypeComparer
    {
        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public HashSetComparer(RootComparer rootComparer) : base(rootComparer)
        {
        }

        /// <summary>
        /// Returns true if both objects are hash sets
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return TypeHelper.IsHashSet(type1) && TypeHelper.IsHashSet(type2);
        }

        /// <summary>
        /// Compare two hash sets
        /// </summary>
        /// <param name="result">The comparison result</param>
        /// <param name="object1">The first object to compare</param>
        /// <param name="object2">The second object to compare</param>
        /// <param name="breadCrumb">The breadcrumb</param>
        public override void CompareType(ComparisonResult result, object object1, object object2, string breadCrumb)
        {
            try
            {
                result.AddParent(object1.GetHashCode());
                result.AddParent(object2.GetHashCode());

                Type t1 = object1.GetType();

                if (HashSetsDifferentCount(result, object1, object2, breadCrumb, t1)) return;

                

                if (result.Config.IgnoreCollectionOrder)
                {
                    IgnoreOrderLogic logic = new IgnoreOrderLogic(RootComparer);
                    logic.CompareEnumeratorIgnoreOrder(result, object1 as IEnumerable, object2 as IEnumerable, breadCrumb);
                }
                else
                {
                    CompareItems(result, object1, object2, breadCrumb, t1);
                }
            }
            finally
            {
                result.RemoveParent(object1.GetHashCode());
                result.RemoveParent(object2.GetHashCode());
            }
        }

        private void CompareItems(ComparisonResult result, object object1, object object2, string breadCrumb, Type t1)
        {
            int count = 0;

            //Get enumerators by reflection
            MethodInfo methodInfo = Cache.GetMethod(t1, "GetEnumerator");
            IEnumerator enumerator1 = (IEnumerator) methodInfo.Invoke(object1, null);
            IEnumerator enumerator2 = (IEnumerator) methodInfo.Invoke(object2, null);

            while (enumerator1.MoveNext() && enumerator2.MoveNext())
            {
                string currentBreadCrumb = AddBreadCrumb(result.Config, breadCrumb, string.Empty, string.Empty, count);

                RootComparer.Compare(result, enumerator1.Current, enumerator2.Current, currentBreadCrumb);

                if (result.ExceededDifferences)
                    return;

                count++;
            }
        }

        private bool HashSetsDifferentCount(ComparisonResult result, object object1, object object2, string breadCrumb, Type t1)
        {
            //Get count by reflection since we can't cast it to HashSet<>
            int hashSet1Count = (int) Cache.GetPropertyValue(result, t1, object1, "Count");
            int hashSet2Count = (int) Cache.GetPropertyValue(result, t1, object2, "Count");

            //Objects must be the same length
            if (hashSet1Count != hashSet2Count)
            {
                Difference difference = new Difference
                                            {
                                                PropertyName = breadCrumb,
                                                Object1Value = hashSet1Count.ToString(CultureInfo.InvariantCulture),
                                                Object2Value = hashSet2Count.ToString(CultureInfo.InvariantCulture),
                                                ChildPropertyName = "Count",
                                                Object1 = new WeakReference(object1),
                                                Object2 = new WeakReference(object2)
                                            };

                AddDifference(result, difference);

                if (result.ExceededDifferences)
                    return true;
            }

            return false;
        }
    }
}
