using System;
using System.Collections;
using System.Globalization;
using KellermanSoftware.CompareNetObjects.IgnoreOrderTypes;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Logic to compare two dictionaries
    /// </summary>
    public class DictionaryComparer : BaseTypeComparer
    {
        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public DictionaryComparer(RootComparer rootComparer) : base(rootComparer)
        {
        }

        /// <summary>
        /// Returns true if both types are dictionaries
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return TypeHelper.IsIDictionary(type1) && TypeHelper.IsIDictionary(type2);
        }

        /// <summary>
        /// Compare two dictionaries
        /// </summary>
        /// <param name="result">The comparison result</param>
        /// <param name="object1">The first object to compare</param>
        /// <param name="object2">The second object to compare</param>
        /// <param name="breadCrumb">The breadcrumb</param>
        public override void CompareType(ComparisonResult result, object object1, object object2, string breadCrumb)
        {
            IDictionary iDict1 = object1 as IDictionary;
            IDictionary iDict2 = object2 as IDictionary;

            //This should never happen, null check happens one level up
            if (iDict1 == null || iDict2 == null)
                return;

            try
            {
                result.AddParent(object1.GetHashCode());
                result.AddParent(object2.GetHashCode());

                //Objects must be the same length
                if (DictionaryCountsDifferent(result, breadCrumb, iDict1, iDict2)) return;

                if (result.Config.IgnoreCollectionOrder)
                {
                    IgnoreOrderLogic logic = new IgnoreOrderLogic(RootComparer);
                    logic.CompareEnumeratorIgnoreOrder(result, iDict1, iDict2, breadCrumb);
                }
                else
                {
                    CompareEachItem(result, breadCrumb, iDict1, iDict2);
                }

            }
            finally
            {
                result.RemoveParent(object1.GetHashCode());
                result.RemoveParent(object2.GetHashCode());
            }
        }

        private void CompareEachItem(ComparisonResult result, string breadCrumb, IDictionary iDict1, IDictionary iDict2)
        {
            var enumerator1 = iDict1.GetEnumerator();
            var enumerator2 = iDict2.GetEnumerator();

            while (enumerator1.MoveNext() && enumerator2.MoveNext())
            {
                string currentBreadCrumb = AddBreadCrumb(result.Config, breadCrumb, "Key");

                RootComparer.Compare(result, enumerator1.Key, enumerator2.Key, currentBreadCrumb);

                if (result.ExceededDifferences)
                    return;

                currentBreadCrumb = AddBreadCrumb(result.Config, breadCrumb, "Value");

                RootComparer.Compare(result, enumerator1.Value, enumerator2.Value, currentBreadCrumb);

                if (result.ExceededDifferences)
                    return;
            }
        }

        private bool DictionaryCountsDifferent(ComparisonResult result, string breadCrumb, IDictionary iDict1,
                                               IDictionary iDict2)
        {
            if (iDict1.Count != iDict2.Count)
            {
                Difference difference = new Difference
                                            {
                                                PropertyName = breadCrumb,
                                                Object1Value = iDict1.Count.ToString(CultureInfo.InvariantCulture),
                                                Object2Value = iDict2.Count.ToString(CultureInfo.InvariantCulture),
                                                ChildPropertyName = "Count",
                                                Object1 = new WeakReference(iDict1),
                                                Object2 = new WeakReference(iDict2)
                                            };

                AddDifference(result, difference);

                if (result.ExceededDifferences)
                    return true;
            }
            return false;
        }
    }
}
