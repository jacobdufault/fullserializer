using System;
using System.Globalization;
using System.Reflection;
using KellermanSoftware.CompareNetObjects.IgnoreOrderTypes;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Logic to compare an integer indexer (Note, inherits from BaseComparer, not TypeComparer)
    /// </summary>
    public class IndexerComparer : BaseComparer
    {
        private readonly RootComparer _rootComparer;

        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public IndexerComparer(RootComparer rootComparer)
        {
            _rootComparer = rootComparer;
        }

        /// <summary>
        /// Compare an integer indexer
        /// </summary>
        /// <param name="result">The comparison result</param>
        /// <param name="info">The property info for the indexer</param>
        /// <param name="object1">The first indexer to compare</param>
        /// <param name="object2">The second indexer to compare</param>
        /// <param name="breadCrumb">The current breadcrumb</param>
        public void CompareIndexer(ComparisonResult result, PropertyInfo info, object object1, object object2, string breadCrumb)
        {

            int indexerCount1 = (int)info.ReflectedType.GetProperty("Count").GetGetMethod().Invoke(object1, new object[] { });
            int indexerCount2 = (int)info.ReflectedType.GetProperty("Count").GetGetMethod().Invoke(object2, new object[] { });

            //Indexers must be the same length
            if (IndexersHaveDifferentLength(info, object1, object2, result, breadCrumb)) return;

            if (result.Config.IgnoreCollectionOrder)
            {
                var enumerable1 = new IndexerCollectionLooper(object1, info, indexerCount1);
                var enumerable2 = new IndexerCollectionLooper(object2, info, indexerCount2);
                IgnoreOrderLogic logic = new IgnoreOrderLogic(_rootComparer);
                logic.CompareEnumeratorIgnoreOrder(result, enumerable1, enumerable2, breadCrumb);
            }
            else
            {
                string currentCrumb;

                // Run on indexer
                for (int i = 0; i < indexerCount1; i++)
                {
                    currentCrumb = AddBreadCrumb(result.Config, breadCrumb, info.Name, string.Empty, i);
                    object objectValue1 = info.GetValue(object1, new object[] { i });
                    object objectValue2 = null;

                    if (i < indexerCount2)
                        objectValue2 = info.GetValue(object2, new object[] { i });

                    _rootComparer.Compare(result, objectValue1, objectValue2, currentCrumb);

                    if (result.ExceededDifferences)
                        return;
                }

                if (indexerCount1 < indexerCount2)
                {
                    for (int j = indexerCount1; j < indexerCount2; j++)
                    {
                        currentCrumb = AddBreadCrumb(result.Config, breadCrumb, info.Name, string.Empty, j);
                        object objectValue2 = info.GetValue(object2, new object[] { j });
                        object objectValue1 = null;

                        _rootComparer.Compare(result, objectValue1, objectValue2, currentCrumb);

                        if (result.ExceededDifferences)
                            return;
                    }
                }
            }
        }

        private bool IndexersHaveDifferentLength(PropertyInfo info, object object1, object object2, ComparisonResult result,
                                                 string breadCrumb)
        {
            int indexerCount1 = (int)info.ReflectedType.GetProperty("Count").GetGetMethod().Invoke(object1, new object[] { });
            int indexerCount2 = (int)info.ReflectedType.GetProperty("Count").GetGetMethod().Invoke(object2, new object[] { });

            if (indexerCount1 != indexerCount2)
            {
                string currentCrumb = AddBreadCrumb(result.Config, breadCrumb, info.Name);
                Difference difference = new Difference
                                            {
                                                PropertyName = currentCrumb,
                                                Object1Value = indexerCount1.ToString(CultureInfo.InvariantCulture),
                                                Object2Value = indexerCount2.ToString(CultureInfo.InvariantCulture),
                                                ChildPropertyName = "Count",
                                                Object1 = new WeakReference(object1),
                                                Object2 = new WeakReference(object2)
                                            };

                AddDifference(result, difference);

                if (result.ExceededDifferences)
                    return true;
            }
            return false;
        }
    }
}
