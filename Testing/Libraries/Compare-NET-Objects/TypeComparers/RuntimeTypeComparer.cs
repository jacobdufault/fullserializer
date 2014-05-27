using System;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Logic to compare two runtime types
    /// </summary>
    public class RuntimeTypeComparer : BaseTypeComparer 
    {
        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public RuntimeTypeComparer(RootComparer rootComparer)
            : base(rootComparer)
        {}


        /// <summary>
        /// Returns true if both types are of type runtme type
        /// </summary>
        /// <param name="type1"></param>
        /// <param name="type2"></param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return TypeHelper.IsRuntimeType(type1) && TypeHelper.IsRuntimeType(type2);
        }

        /// <summary>
        /// Compare two runtime types
        /// </summary>
        /// <param name="result">The comparison result</param>
        /// <param name="object1">The first object to compare</param>
        /// <param name="object2">The second object to compare</param>
        /// <param name="breadCrumb">The breadcrumb</param>
        public override void CompareType(ComparisonResult result, object object1, object object2, string breadCrumb)
        {
            Type t1 = (Type)object1;
            Type t2 = (Type)object2;

            if (t1.FullName != t2.FullName)
            {
                Difference difference = new Difference
                {
                    PropertyName = breadCrumb,
                    Object1Value = t1.FullName,
                    Object2Value = t2.FullName,
                    ChildPropertyName = "FullName",
                    Object1 = new WeakReference(object1),
                    Object2 = new WeakReference(object2)
                };

                AddDifference(result, difference);
            }
        }
    }
}
