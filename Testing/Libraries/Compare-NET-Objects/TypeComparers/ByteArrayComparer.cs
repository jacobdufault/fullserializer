using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Compare two byte arrays
    /// </summary>
    public class ByteArrayComparer : BaseTypeComparer
    {
        /// <summary>
        /// Protected constructor that references the root comparer
        /// </summary>
        /// <param name="rootComparer">The root comparer.</param>
        public ByteArrayComparer(RootComparer rootComparer) : base(rootComparer)
        {
        }

        /// <summary>
        /// If true the type comparer will handle the comparison for the type
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns><c>true</c> if it is a byte array; otherwise, <c>false</c>.</returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return TypeHelper.IsByteArray(type1)
                   && TypeHelper.IsByteArray(type2);
        }

        /// <summary>
        /// Compare two byte array objects
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

            if (ListsHaveDifferentCounts(result, breadCrumb, ilist1, ilist2)) 
                return;
                
            CompareItems(result, breadCrumb, ilist1, ilist2);                
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
            int differenceCount = 0;
            IEnumerator enumerator1 = ilist1.GetEnumerator();
            IEnumerator enumerator2 = ilist2.GetEnumerator();

            while (enumerator1.MoveNext() && enumerator2.MoveNext())
            {
                byte? b1 = enumerator1.Current as byte?;
                byte? b2 = enumerator2.Current as byte?;

                if (b1 != b2)
                {
                    string currentBreadCrumb = AddBreadCrumb(result.Config, breadCrumb, string.Empty, string.Empty, count);

                    Difference difference = new Difference
                    {
                        PropertyName = currentBreadCrumb,
                        Object1Value = NiceString(b1),
                        Object2Value = NiceString(b2),
                        Object1 = new WeakReference(ilist1),
                        Object2 = new WeakReference(ilist2)
                    };

                    AddDifference(result, difference);
                    differenceCount++;

                    if (differenceCount >= result.Config.MaxByteArrayDifferences)
                        return;
                }

                count++;
            }
        }
    }
}
