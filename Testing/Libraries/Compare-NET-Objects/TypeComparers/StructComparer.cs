using System;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Compare two structs
    /// </summary>
    public class StructComparer : BaseTypeComparer
    {
        private readonly PropertyComparer _propertyComparer;
        private readonly FieldComparer _fieldComparer;

        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public StructComparer(RootComparer rootComparer) : base(rootComparer)
        {
            _propertyComparer = new PropertyComparer(rootComparer);
            _fieldComparer = new FieldComparer(rootComparer);
        }

        /// <summary>
        /// Returns true if both objects are of type struct
        /// </summary>
        /// <param name="type1"></param>
        /// <param name="type2"></param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return TypeHelper.IsStruct(type1) && TypeHelper.IsStruct(type2);
        }

        /// <summary>
        /// Compare two structs
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

                _fieldComparer.PerformCompareFields(result, t1, object1, object2, true, breadCrumb);
                 _propertyComparer.PerformCompareProperties(t1, object1, object2, true, result, breadCrumb);
            }
            finally
            {
                result.RemoveParent(object1.GetHashCode());
                result.RemoveParent(object2.GetHashCode());
            }
        }
    }
}
