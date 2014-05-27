using System;
using System.Linq;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Logic to compare two LINQ enumerators
    /// </summary>
    public class EnumerableComparer :BaseTypeComparer
    {
        private readonly ListComparer _compareIList;

        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public EnumerableComparer(RootComparer rootComparer) : base(rootComparer)
        {
            _compareIList = new ListComparer(rootComparer);
        }

        /// <summary>
        /// Returns true if either object is of type LINQ Enumerator
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            if (type1 == null || type2 == null)
                return false;

            return TypeHelper.IsEnumerable(type1) || TypeHelper.IsEnumerable(type2);
        }

        /// <summary>
        /// Compare two objects that implement LINQ Enumerator
        /// </summary>
        /// <param name="result">The comparison result</param>
        /// <param name="object1">The first object to compare</param>
        /// <param name="object2">The second object to compare</param>
        /// <param name="breadCrumb">The breadcrumb</param>
        public override void CompareType(ComparisonResult result, object object1, object object2, string breadCrumb)
        {
            Type t1 = object1.GetType();
            Type t2 = object2.GetType();

            var l1 = TypeHelper.IsEnumerable(t1) ? ConvertEnumerableToList(object1) : object1;
            var l2 = TypeHelper.IsEnumerable(t2) ? ConvertEnumerableToList(object2) : object2;

            _compareIList.CompareType(result, l1, l2, breadCrumb);
        }

        private object ConvertEnumerableToList(object source)
        {
            var type = source.GetType();

            if (type.IsArray)
                return source;

            var genArgs = type.GetGenericArguments();
            var toList = typeof(Enumerable).GetMethod("ToList");
            var constructedToList = toList.MakeGenericMethod(genArgs[0]);
            var resultList = constructedToList.Invoke(null, new[] { source });

            return resultList;
        }
    }
}
