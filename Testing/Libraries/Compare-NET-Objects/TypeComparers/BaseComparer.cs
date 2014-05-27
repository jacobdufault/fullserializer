using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace KellermanSoftware.CompareNetObjects.TypeComparers
{
    /// <summary>
    /// Common functionality for all Comparers
    /// </summary>
    public class BaseComparer
    {

        /// <summary>
        /// Add a breadcrumb to an existing breadcrumb
        /// </summary>
        /// <param name="config">Comparison configuration</param>
        /// <param name="existing">The existing breadcrumb</param>
        /// <param name="name">The field or property name</param>
        /// <returns>The new breadcrumb</returns>
        protected string AddBreadCrumb(ComparisonConfig config, string existing, string name)
        {
            return AddBreadCrumb(config, existing, name, string.Empty, null);
        }

        /// <summary>
        /// Add a breadcrumb to an existing breadcrumb
        /// </summary>
        /// <param name="config">The comparison configuration</param>
        /// <param name="existing">The existing breadcrumb</param>
        /// <param name="name">The property or field name</param>
        /// <param name="extra">Extra information to output after the name</param>
        /// <param name="index">The index for an array, list, or row</param>
        /// <returns>The new breadcrumb</returns>
        protected string AddBreadCrumb(ComparisonConfig config, string existing, string name, string extra, int index)
        {
            return AddBreadCrumb(config, existing, name, extra, index >= 0 ? index.ToString(CultureInfo.InvariantCulture) : null);
        }

        /// <summary>
        /// Add a breadcrumb to an existing breadcrumb
        /// </summary>
        /// <param name="config">Comparison configuration</param>
        /// <param name="existing">The existing breadcrumb</param>
        /// <param name="name">The field or property name</param>
        /// <param name="extra">Extra information to append after the name</param>
        /// <param name="index">The index if it is an array, list, row etc.</param>
        /// <returns>The new breadcrumb</returns>
        protected string AddBreadCrumb(ComparisonConfig config, string existing, string name, string extra, string index)
        {
            bool useIndex = !String.IsNullOrEmpty(index);
            bool useName = name.Length > 0;
            StringBuilder sb = new StringBuilder();

            sb.Append(existing);

            if (useName)
            {
                sb.AppendFormat(".");
                sb.Append(name);
            }

            sb.Append(extra);

            if (useIndex)
            {
                // ReSharper disable RedundantAssignment
                int result = -1;
                // ReSharper restore RedundantAssignment
                sb.AppendFormat(Int32.TryParse(index, out result) ? "[{0}]" : "[\"{0}\"]", index);
            }

            if (config.ShowBreadcrumb)
                Debug.WriteLine(sb.ToString());

            return sb.ToString();
        }

        /// <summary>
        /// Add a difference to the result
        /// </summary>
        /// <param name="difference">The difference to add to the result</param>
        /// <param name="result">The comparison result</param>
        protected void AddDifference(ComparisonResult result, Difference difference)
        {
            difference.ActualName = result.Config.ActualName;
            difference.ExpectedName = result.Config.ExpectedName;

            difference.Object1TypeName = difference.Object1 != null && difference.Object1.Target != null 
                ? difference.Object1.Target.GetType().Name : "null";

            difference.Object2TypeName = difference.Object2 != null && difference.Object2.Target != null 
                ? difference.Object2.Target.GetType().Name : "null";    

            result.Differences.Add(difference);
            result.Config.DifferenceCallback(difference);
        }



        /// <summary>
        /// Convert an object to a nicely formatted string
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected string NiceString(object obj)
        {
            try
            {
                if (obj == null)
                    return "(null)";

                #if !PORTABLE
                    if (obj == DBNull.Value)
                        return "System.DBNull.Value";
                #endif

                return obj.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }




    }
}
