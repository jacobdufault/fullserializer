using System;
using System.Collections;


namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Compare two objects of type class
    /// </summary>
    public class ClassComparer : BaseTypeComparer
    {
        private readonly PropertyComparer _propertyComparer;
        private readonly FieldComparer _fieldComparer;

        /// <summary>
        /// Constructor for the class comparer
        /// </summary>
        /// <param name="rootComparer">The root comparer instantiated by the RootComparerFactory</param>
        public ClassComparer(RootComparer rootComparer) : base(rootComparer)
        {
            _propertyComparer = new PropertyComparer(rootComparer);
            _fieldComparer = new FieldComparer(rootComparer);
        }

        /// <summary>
        /// Returns true if the both objects are a class
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return TypeHelper.IsClass(type1) && TypeHelper.IsClass(type2);
        }

        /// <summary>
        /// Compare two classes
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

                //Custom classes that implement IEnumerable may have the same hash code
                //Ignore objects with the same hash code
                if (!(object1 is IEnumerable)
                    && ReferenceEquals(object1, object2))
                {
                    return;
                }

                Type t1 = object1.GetType();
                Type t2 = object2.GetType();

                //Check if the class type should be excluded based on the configuration
                if (ExcludeLogic.ShouldExcludeClassType(result.Config, t1, t2))
                    return;

                //Compare the properties
                if (result.Config.CompareProperties)
                    _propertyComparer.PerformCompareProperties(t1, object1, object2, false, result, breadCrumb);

                //Compare the fields
                if (result.Config.CompareFields)
                    _fieldComparer.PerformCompareFields(result, t1, object1, object2, false, breadCrumb);
            }
            finally
            {
                result.RemoveParent(object1.GetHashCode());
                result.RemoveParent(object2.GetHashCode());
            }
        }
    }
}
