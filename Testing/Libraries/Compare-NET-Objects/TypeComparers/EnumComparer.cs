using System;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Logic to compare to enum values
    /// </summary>
    public class EnumComparer : BaseTypeComparer
    {
        /// <summary>
        /// Constructor with a default root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public EnumComparer(RootComparer rootComparer) : base(rootComparer)
        {
        }

        /// <summary>
        /// Returns true if both objects are of type enum
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return TypeHelper.IsEnum(type1) && TypeHelper.IsEnum(type2);
        }

        /// <summary>
        /// Compare two enums
        /// </summary>
        /// <param name="result">The comparison result</param>
        /// <param name="object1">The first object to compare</param>
        /// <param name="object2">The second object to compare</param>
        /// <param name="breadCrumb">The breadcrumb</param>
        public override void CompareType(ComparisonResult result, object object1, object object2, string breadCrumb)
        {
            if (object1.ToString() != object2.ToString())
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
    }
}
