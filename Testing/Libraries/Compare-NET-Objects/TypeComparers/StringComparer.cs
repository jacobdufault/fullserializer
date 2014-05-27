using System;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Compare two strings
    /// </summary>
    public class StringComparer : BaseTypeComparer
    {
        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public StringComparer(RootComparer rootComparer) : base(rootComparer)
        {
        }

        /// <summary>
        /// Returns true if both objects are a string or if one is a string and one is a a null
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return (TypeHelper.IsString(type1) && TypeHelper.IsString(type2))
                   || (TypeHelper.IsString(type1) && type2 == null)
                   || (TypeHelper.IsString(type2) && type1 == null);
        }

        /// <summary>
        /// Compare two strings
        /// </summary>
        /// <param name="result">The comparison result</param>
        /// <param name="object1">The first object to compare</param>
        /// <param name="object2">The second object to compare</param>
        /// <param name="breadCrumb">The breadcrumb</param>
        public override void CompareType(ComparisonResult result, object object1, object object2, string breadCrumb)
        {
            if (result.Config.TreatStringEmptyAndNullTheSame 
                && ((object1 == null && object2 != null && object2.ToString() == string.Empty)
                    || (object2 == null && object1 != null && object1.ToString() == string.Empty)))
            {
                return;
            }

            if (OneOfTheStringsIsNull(result, object1, object2, breadCrumb)) return;

            IComparable valOne = object1 as IComparable;

            if (valOne != null && valOne.CompareTo(object2) != 0)
            {
                Difference difference = new Difference
                {
                    PropertyName = breadCrumb,
                    Object1Value = object1.ToString(),
                    Object2Value = object2.ToString(),
                    Object1 = new WeakReference(object1),
                    Object2 = new WeakReference(object2)
                };

                AddDifference(result, difference);
            }
        }

        private bool OneOfTheStringsIsNull(ComparisonResult result, object object1, object object2, string breadCrumb)
        {
            if (object1 == null || object2 == null)
            {
                Difference difference = new Difference
                                            {
                                                PropertyName = breadCrumb,
                                                Object1Value = NiceString(object1),
                                                Object2Value = NiceString(object2),
                                                Object1 = new WeakReference(object1),
                                                Object2 = new WeakReference(object2)
                                            };

                AddDifference(result, difference);
                return true;
            }
            return false;
        }
    }
}
