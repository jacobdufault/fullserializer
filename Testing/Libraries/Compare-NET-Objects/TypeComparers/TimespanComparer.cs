using System;
using System.Globalization;


namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Logic to compare two timespans
    /// </summary>
    public class TimespanComparer : BaseTypeComparer
    {
        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public TimespanComparer(RootComparer rootComparer) : base(rootComparer)
        {
        }

        /// <summary>
        /// Returns true if both objects are timespans
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return TypeHelper.IsTimespan(type1) && TypeHelper.IsTimespan(type2);
        }

        /// <summary>
        /// Compare two timespans
        /// </summary>
        /// <param name="result">The comparison result</param>
        /// <param name="object1">The first object to compare</param>
        /// <param name="object2">The second object to compare</param>
        /// <param name="breadCrumb">The breadcrumb</param>
        public override void CompareType(ComparisonResult result, object object1, object object2, string breadCrumb)
        {
            if (((TimeSpan)object1).Ticks != ((TimeSpan)object2).Ticks)
            {
                Difference difference = new Difference
                {
                    PropertyName = breadCrumb,
                    Object1Value = ((TimeSpan)object1).Ticks.ToString(CultureInfo.InvariantCulture),
                    Object2Value = ((TimeSpan)object1).Ticks.ToString(CultureInfo.InvariantCulture),
                    ChildPropertyName = "Ticks",
                    Object1 = new WeakReference(object1),
                    Object2 = new WeakReference(object2)
                };

                AddDifference(result, difference);
            }
        }
    }
}
