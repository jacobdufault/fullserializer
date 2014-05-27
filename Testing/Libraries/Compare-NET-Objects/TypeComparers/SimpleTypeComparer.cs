using System;
using System.Collections.Generic;
using UnityEngine;

namespace KellermanSoftware.CompareNetObjects.TypeComparers {
    /// <summary>
    /// Compare primitive types (long, int, short, byte etc.) and DateTime, decimal, and Guid
    /// </summary>
    public class SimpleTypeComparer : BaseTypeComparer {
        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public SimpleTypeComparer(RootComparer rootComparer)
            : base(rootComparer) {
        }

        /// <summary>
        /// Returns true if the type is a simple type
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2) {
            return TypeHelper.IsSimpleType(type1) && TypeHelper.IsSimpleType(type2);
        }

        public bool NearlyEqual(double a, double b, double epsilon) {
            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double diff = Math.Abs(a - b);

            if (a == b) { // shortcut, handles infinities
                return true;
            }
            else if (a == 0 || b == 0 || diff < Double.MinValue) {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * Double.MinValue);
            }
            else { // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }

        /// <summary>
        /// Compare two simple types
        /// </summary>
        /// <param name="result">The comparison result</param>
        /// <param name="object1">The first object to compare</param>
        /// <param name="object2">The second object to compare</param>
        /// <param name="breadCrumb">The breadcrumb</param>
        public override void CompareType(ComparisonResult result, object object1, object object2, string breadCrumb) {
            //This should never happen, null check happens one level up
            if (object2 == null || object1 == null)
                return;

            bool equal = true;

            if ((object1 is float && object2 is float) || (object1 is double && object2 is double)) {
                equal = NearlyEqual(Convert.ToDouble(object1), Convert.ToDouble(object2), .001);
            }
            else {
                // handles boxing correctly
                equal = EqualityComparer<object>.Default.Equals(object1, object2);
            }

            if (equal == false) {
                Difference difference = new Difference {
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
