using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using KellermanSoftware.CompareNetObjects.TypeComparers;

namespace KellermanSoftware.CompareNetObjects
{
    /// <summary>
    /// The base comparer which contains all the type comparers
    /// </summary>
    public class RootComparer : BaseComparer
    {
        #region Properties


        /// <summary>
        /// A list of the type comparers
        /// </summary>
        internal List<BaseTypeComparer> TypeComparers { get; set; }
        #endregion

        #region Methods

        /// <summary>
        /// Compare two objects
        /// </summary>
        /// <param name="result">The comparison result</param>
        /// <param name="object1">The first object to compare</param>
        /// <param name="object2">The second object to compare</param>
        /// <param name="breadCrumb">Where we are in the object hiearchy</param>
        public bool Compare(ComparisonResult result, object object1, object object2, string breadCrumb)
        {
            if (object1 == null && object2 == null)
                return true;

            Type t1 = object1 != null ? object1.GetType() : null;
            Type t2 = object2 != null ? object2.GetType() : null;           

            BaseTypeComparer customComparer = result.Config.CustomComparers.FirstOrDefault(o => o.IsTypeMatch(t1, t2));

            if (customComparer != null)
            {
                customComparer.CompareType(result, object1, object2, breadCrumb);
            }
            else
            {
                BaseTypeComparer typeComparer = TypeComparers.FirstOrDefault(o => o.IsTypeMatch(t1, t2));

                if (typeComparer != null)
                {
                    if (result.Config.IgnoreObjectTypes || !TypesDifferent(result, object1, object2, breadCrumb, t1, t2))
                    {
                        typeComparer.CompareType(result, object1, object2, breadCrumb);
                    }
                }
                else
                {
                    if (EitherObjectIsNull(result, object1, object2, breadCrumb)) return false;

                    if (!result.Config.IgnoreObjectTypes)
                        throw new NotSupportedException("Cannot compare object of type " + t1.Name);
                }
            }

            return result.AreEqual;
        }

        private bool TypesDifferent(ComparisonResult result, object object1, object object2, string breadCrumb, Type t1, Type t2)
        {
            //Objects must be the same type and not be null
            if (!result.Config.IgnoreObjectTypes
                && object1 != null 
                && object2 != null 
                && t1 != t2)
            {
                Difference difference = new Difference
                {
                    PropertyName = breadCrumb,
                    Object1Value = t1.FullName,
                    Object2Value = t2.FullName,
                    ChildPropertyName = "GetType()",
                    MessagePrefix = "Different Types",
                    Object1 = new WeakReference(object1),
                    Object2 = new WeakReference(object2)
                };

                AddDifference(result, difference);
                return true;
            }

            return false;
        }

        private bool EitherObjectIsNull(ComparisonResult result, object object1, object object2, string breadCrumb)
        {
            //Check if one of them is null
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

        #endregion
    }
}
