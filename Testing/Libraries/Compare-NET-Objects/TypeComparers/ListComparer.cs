using System;
using System.Collections;
using System.Globalization;
using KellermanSoftware.CompareNetObjects.IgnoreOrderTypes;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Compare objects that implement IList
    /// </summary>
    public class ListComparer : BaseTypeComparer
    {

        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public ListComparer(RootComparer rootComparer) : base(rootComparer)
        {
            
        }

        /// <summary>
        /// Returns true if both objects implement IList
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return TypeHelper.IsIList(type1) && TypeHelper.IsIList(type2);
        }


        /// <summary>
        /// Compare two objects that implement IList
        /// </summary>
        /// <param name="result">The comparison result</param>
        /// <param name="object1">The first object to compare</param>
        /// <param name="object2">The second object to compare</param>
        /// <param name="breadCrumb">The breadcrumb</param>
        public override void CompareType(ComparisonResult result, object object1, object object2, string breadCrumb)
        {
            IList ilist1 = object1 as IList;
            IList ilist2 = object2 as IList;

            //This should never happen, null check happens one level up
            if (ilist1 == null || ilist2 == null)
                return;

            try
            {
                result.AddParent(object1.GetHashCode());
                result.AddParent(object2.GetHashCode());

                if (ListsHaveDifferentCounts(result, breadCrumb, ilist1, ilist2)) return;                

                if (result.Config.IgnoreCollectionOrder)
                {
                    IgnoreOrderLogic ignoreOrderLogic = new IgnoreOrderLogic(RootComparer);
                    ignoreOrderLogic.CompareEnumeratorIgnoreOrder(result, ilist1, ilist2, breadCrumb);
                }
                else
                {
                    CompareItems(result, breadCrumb, ilist1, ilist2);
                }
            }
            finally
            {
                result.RemoveParent(object1.GetHashCode());
                result.RemoveParent(object2.GetHashCode());
            }

        }

        private bool ListsHaveDifferentCounts(ComparisonResult result, string breadCrumb, IList ilist1, IList ilist2)
        {
            //Objects must be the same length
            if (ilist1.Count != ilist2.Count)
            {
                Difference difference = new Difference
                {
                    PropertyName = breadCrumb,
                    Object1Value = ilist1.Count.ToString(CultureInfo.InvariantCulture),
                    Object2Value = ilist2.Count.ToString(CultureInfo.InvariantCulture),
                    ChildPropertyName = "Count",
                    Object1 = new WeakReference(ilist1),
                    Object2 = new WeakReference(ilist2)
                };

                AddDifference(result, difference);

                if (result.ExceededDifferences)
                    return true;
            }
            return false;
        }

        private void CompareItems(ComparisonResult result, string breadCrumb, IList ilist1, IList ilist2)
        {
            int count = 0;
            IEnumerator enumerator1 = ilist1.GetEnumerator();
            IEnumerator enumerator2 = ilist2.GetEnumerator();

            while (enumerator1.MoveNext() && enumerator2.MoveNext())
            {
                string currentBreadCrumb = AddBreadCrumb(result.Config, breadCrumb, string.Empty, string.Empty, count);

                RootComparer.Compare(result, enumerator1.Current, enumerator2.Current, currentBreadCrumb);

                if (result.ExceededDifferences)
                    return;

                count++;
            }
        }


    }
}
