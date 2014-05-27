using System;
using System.Globalization;
using System.Net;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Logic to compare two IP End Points
    /// </summary>
    public class IpEndPointComparer : BaseTypeComparer 
    {
        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public IpEndPointComparer(RootComparer rootComparer)
            : base(rootComparer)
        {}

        /// <summary>
        /// Returns true if both objects are an IP End Point
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return TypeHelper.IsIpEndPoint(type1) && TypeHelper.IsIpEndPoint(type2);
        }

        /// <summary>
        /// Compare two IP End Points
        /// </summary>
        /// <param name="result">The comparison result</param>
        /// <param name="object1">The first object to compare</param>
        /// <param name="object2">The second object to compare</param>
        /// <param name="breadCrumb">The breadcrumb</param>
        public override void CompareType(ComparisonResult result, object object1, object object2, string breadCrumb)
        {
            IPEndPoint ipEndPoint1 = object1 as IPEndPoint;
            IPEndPoint ipEndPoint2 = object2 as IPEndPoint;

            //Null check happens above
            if (ipEndPoint1 == null || ipEndPoint2 == null)
                return;

            ComparePort(result, breadCrumb, ipEndPoint1, ipEndPoint2);

            if (result.ExceededDifferences)
                return;

            CompareAddress(result, breadCrumb, ipEndPoint1, ipEndPoint2);
        }



        private void ComparePort(ComparisonResult result, string breadCrumb, IPEndPoint ipEndPoint1, IPEndPoint ipEndPoint2)
        {
            if (ipEndPoint1.Port != ipEndPoint2.Port)
            {
                Difference difference = new Difference
                                            {
                                                PropertyName = breadCrumb,
                                                Object1Value = ipEndPoint1.Port.ToString(CultureInfo.InvariantCulture),
                                                Object2Value = ipEndPoint2.Port.ToString(CultureInfo.InvariantCulture),
                                                ChildPropertyName = "Port",
                                                Object1 = new WeakReference(ipEndPoint1),
                                                Object2 = new WeakReference(ipEndPoint2)
                                            };

                AddDifference(result, difference);
            }
        }

        private void CompareAddress(ComparisonResult result, string breadCrumb, IPEndPoint ipEndPoint1, IPEndPoint ipEndPoint2)
        {
            if (ipEndPoint1.Address.ToString() != ipEndPoint2.Address.ToString())
            {
                Difference difference = new Difference
                {
                    PropertyName = breadCrumb,
                    Object1Value = ipEndPoint1.Address.ToString(),
                    Object2Value = ipEndPoint2.Address.ToString(),
                    ChildPropertyName = "Address",
                    Object1 = new WeakReference(ipEndPoint1),
                    Object2 = new WeakReference(ipEndPoint2)
                };

                AddDifference(result, difference);
            }
        }
    }
}
