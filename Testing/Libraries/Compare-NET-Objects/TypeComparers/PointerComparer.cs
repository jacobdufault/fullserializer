using System;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Logic to compare to pointers
    /// </summary>
    public class PointerComparer : BaseTypeComparer
    {
        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public PointerComparer(RootComparer rootComparer) : base(rootComparer)
        {
        }

        /// <summary>
        /// Returns true if both types are a pointer
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return TypeHelper.IsPointer(type1) && TypeHelper.IsPointer(type2);
        }

        /// <summary>
        /// Compare two pointers
        /// </summary>
        /// <param name="result">The comparison result</param>
        /// <param name="object1">The first object to compare</param>
        /// <param name="object2">The second object to compare</param>
        /// <param name="breadCrumb">The breadcrumb</param>
        public override void CompareType(ComparisonResult result, object object1, object object2, string breadCrumb)
        {
            if ((object1 is IntPtr && object2 is IntPtr && ((IntPtr)object1) != ((IntPtr)object2))
                || (object1 is UIntPtr && object2 is UIntPtr && ((UIntPtr)object1) != ((UIntPtr)object2)))
            {
                Difference difference = new Difference
                {
                    PropertyName = breadCrumb,
                    Object1 = new WeakReference(object1),
                    Object2 = new WeakReference(object2)
                };

                AddDifference(result, difference);
            }
        }
    }
}
