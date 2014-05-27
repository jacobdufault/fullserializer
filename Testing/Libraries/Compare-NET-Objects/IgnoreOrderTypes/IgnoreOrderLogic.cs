using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KellermanSoftware.CompareNetObjects.TypeComparers;

namespace KellermanSoftware.CompareNetObjects.IgnoreOrderTypes
{
    /// <summary>
    /// Logic for comparing lists that are out of order based on a key
    /// </summary>
    public class IgnoreOrderLogic : BaseComparer
    {
        private readonly RootComparer _rootComparer;
        private readonly List<string> _alreadyCompared = new List<string>();


        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreOrderLogic"/> class.
        /// </summary>
        /// <param name="rootComparer">The root comparer.</param>
        public IgnoreOrderLogic(RootComparer rootComparer)
        {
            _rootComparer = rootComparer;
        }

        /// <summary>
        /// Compares the enumerators and ignores the order
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="enumerable1">The first enumerator.</param>
        /// <param name="enumerable2">The second enumerator.</param>
        /// <param name="breadCrumb">The bread crumb.</param>
        public void CompareEnumeratorIgnoreOrder(ComparisonResult result,
            IEnumerable enumerable1,
            IEnumerable enumerable2, 
            string breadCrumb)
        {
            if (!CompareInOrder(result, enumerable1, enumerable2, breadCrumb) && !result.ExceededDifferences)
                CompareOutOfOrder(result, enumerable1, enumerable2, breadCrumb, false);

            if (!result.ExceededDifferences)
            {
                if (!CompareInOrder(result, enumerable2, enumerable1, breadCrumb) && !result.ExceededDifferences)
                {
                    CompareOutOfOrder(result, enumerable2, enumerable1, breadCrumb, true);
                }
            }
        }

        private bool CompareInOrder(ComparisonResult result,
            IEnumerable enumerable1,
            IEnumerable enumerable2,
            string breadCrumb)
        {
            IEnumerator enumerator1 = enumerable1.GetEnumerator();
            IEnumerator enumerator2 = enumerable2.GetEnumerator();
            List<string> matchingSpec = null;

            while (enumerator1.MoveNext())
            {
                if (enumerator1.Current == null)
                {
                    return false;
                }

                if (matchingSpec == null)
                    matchingSpec = GetMatchingSpec(result, enumerator1.Current.GetType());

                string matchIndex1 = GetMatchIndex(result, matchingSpec, enumerator1.Current);

                if (enumerator2.MoveNext())
                {
                    if (_alreadyCompared.Contains(matchIndex1))
                    {
                        continue;
                    }

                    if (enumerator2.Current == null)
                    {
                        return false;
                    }

                    string matchIndex2 = GetMatchIndex(result, matchingSpec, enumerator2.Current);

                    if (matchIndex1 == matchIndex2)
                    {
                        string currentBreadCrumb = string.Format("{0}[{1}]", breadCrumb, matchIndex1);
                        _rootComparer.Compare(result, enumerator1.Current, enumerator2.Current, currentBreadCrumb);
                        _alreadyCompared.Add(matchIndex1);
                    }
                    else
                    {
                        return false;
                    }
                }

                if (result.ExceededDifferences)
                    break;
            }

            return true;
        }

        private void CompareOutOfOrder(ComparisonResult result,
            IEnumerable enumerable1,
            IEnumerable enumerable2,
            string breadCrumb,
            bool reverseCompare)
        {
            IEnumerator enumerator1 = enumerable1.GetEnumerator();            
            List<string> matchingSpec = null;

            while (enumerator1.MoveNext())
            {
                if (enumerator1.Current == null)
                {
                    continue;
                }

                if (matchingSpec == null)
                    matchingSpec = GetMatchingSpec(result, enumerator1.Current.GetType());

                string matchIndex1 = GetMatchIndex(result, matchingSpec, enumerator1.Current);

                if (_alreadyCompared.Contains(matchIndex1))
                    continue;

                string currentBreadCrumb = string.Format("{0}[{1}]", breadCrumb, matchIndex1);
                IEnumerator enumerator2 = enumerable2.GetEnumerator();
                bool found = false;

                while (enumerator2.MoveNext())
                {
                    if (enumerator2.Current == null)
                    {
                        continue;
                    }

                    string matchIndex2 = GetMatchIndex(result, matchingSpec, enumerator2.Current);

                    if (matchIndex1 == matchIndex2)
                    {                        
                        _rootComparer.Compare(result, enumerator1.Current, enumerator2.Current, currentBreadCrumb);
                        _alreadyCompared.Add(matchIndex1);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Difference difference = new Difference
                    {
                        PropertyName = currentBreadCrumb,
                        Object1Value = reverseCompare ? "(null)" : NiceString(enumerator1.Current),
                        Object2Value = reverseCompare ? NiceString(enumerator1.Current) : "(null)",
                        ChildPropertyName = "Item",
                        Object1 = reverseCompare ? null : new WeakReference(enumerator1),
                        Object2 = reverseCompare ? new WeakReference(enumerator1) : null
                    };

                    AddDifference(result, difference);                    
                }
                if (result.ExceededDifferences)
                    return;

            }

         
        }



        private string GetMatchIndex(ComparisonResult result, List<string> spec, object currentObject)
        {
            List<PropertyInfo> properties = Cache.GetPropertyInfo(result, currentObject.GetType()).ToList();
            StringBuilder sb = new StringBuilder();

            foreach (var item in spec)
            {
                var info = properties.FirstOrDefault(o => o.Name == item);

                if (info == null)
                {
                    throw new Exception(string.Format("Invalid CollectionMatchingSpec.  No such property {0} for type {1} ",
                        item,
                        currentObject.GetType().Name));
                }

                var propertyValue = info.GetValue(currentObject, null);

                if (propertyValue == null)
                {
                    sb.AppendFormat("{0}:(null),",item);
                }
                else
                {
                    sb.AppendFormat("{0}:{1},", item, propertyValue);
                }
            }

            if (sb.Length == 0)
                sb.Append(currentObject);

            return sb.ToString().TrimEnd(',');
        }



        private List<string> GetMatchingSpec(ComparisonResult result,Type type)
        {
            if (result.Config.CollectionMatchingSpec.Keys.Contains(type))
            {
                return result.Config.CollectionMatchingSpec.First(p => p.Key == type).Value.ToList();
            }
            
            return Cache.GetPropertyInfo(result, type)
                .Where(o => o.CanWrite && (TypeHelper.IsSimpleType(o.PropertyType) || TypeHelper.IsEnum(o.PropertyType)))
                .Select(o => o.Name).ToList();
        }












    }
}
