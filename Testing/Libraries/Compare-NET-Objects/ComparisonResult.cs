using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace KellermanSoftware.CompareNetObjects
{
    /// <summary>
    /// Details about the comparison
    /// </summary>
    public class ComparisonResult
    {
        #region Constructors
        /// <summary>
        /// Set the configuration for the comparison
        /// </summary>
        /// <param name="config"></param>
        public ComparisonResult(ComparisonConfig config)
        {
            Config = config;
            Differences = new List<Difference>();

            #if !PORTABLE
                Watch = new Stopwatch();
            #endif
        }
        #endregion

        #region Properties
        /// <summary>
        /// Configuration
        /// </summary>
        public ComparisonConfig Config { get; private set; }

        #if !PORTABLE
            internal Stopwatch Watch { get; set; }

            /// <summary>
            /// The amount of time in milliseconds it took for the comparison
            /// </summary>
            public long ElapsedMilliseconds
            {
                get { return Watch.ElapsedMilliseconds; }
            }
        #endif

        /// <summary>
        /// Keep track of parent objects in the object hiearchy
        /// </summary>
        internal readonly Dictionary<int, int> Parents = new Dictionary<int, int>();

        /// <summary>
        /// The differences found during the compare
        /// </summary>
        public List<Difference> Differences { get; set; }

        /// <summary>
        /// The differences found in a string suitable for a textbox
        /// </summary>
        public string DifferencesString
        {
            get
            {
                StringBuilder sb = new StringBuilder(4096);

                sb.AppendLine();
                sb.AppendFormat("Begin Differences ({0} differences):{1}", Differences.Count, Environment.NewLine);

                foreach (Difference item in Differences)
                {
                    sb.AppendLine(item.ToString());
                }

                sb.AppendFormat("End Differences (Maximum of {0} differences shown).", Config.MaxDifferences);

                return sb.ToString();
            }
        }

        /// <summary>
        /// Returns true if the objects are equal
        /// </summary>
        public bool AreEqual
        {
            get { return Differences.Count == 0; }
        }

        /// <summary>
        /// Returns true if the number of differences has reached the maximum
        /// </summary>
        public bool ExceededDifferences
        {
            get { return Differences.Count >= Config.MaxDifferences; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add parent, handle references count
        /// </summary>
        /// <param name="hash"></param>
        internal void AddParent(int hash)
        {
            if (!Parents.ContainsKey(hash))
            {
                Parents.Add(hash, 1);
            }
            else
            {
                Parents[hash]++;
            }
        }



        /// <summary>
        /// Remove parent, handle references count
        /// </summary>
        /// <param name="hash"></param>
        internal void RemoveParent(int hash)
        {
            if (Parents.ContainsKey(hash))
            {
                if (Parents[hash] <= 1)
                    Parents.Remove(hash);
                else Parents[hash]--;
            }
        }
        #endregion


    }
}
