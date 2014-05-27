using System;
using System.Collections.Generic;
using System.Linq;
using KellermanSoftware.CompareNetObjects.TypeComparers;

namespace KellermanSoftware.CompareNetObjects
{
    /// <summary>
    /// Configuration
    /// </summary>
    public class ComparisonConfig
    {
        #region Class Variables
        private Action<Difference> _differenceCallback;
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ComparisonConfig()
        {
            Reset();
        }
        #endregion

        #region Properties

        /// <summary>
        /// If true, unknown object types will be ignored instead of throwing an exception.  The default is false.
        /// </summary>
        public bool IgnoreUnknownObjectTypes { get; set; }

        /// <summary>
        /// If true, invalid indexers will be skipped.  The default is false.
        /// </summary>
        public bool SkipInvalidIndexers { get; set; }

        /// <summary>
        /// If a class implements an interface then only members of the interface will be compared.  The default is all members are compared. 
        /// </summary>
        public List<Type> InterfaceMembers { get; set; } 

        /// <summary>
        /// Show breadcrumb at each stage of the comparision.  The default is false.
        /// This is useful for debugging deep object graphs.
        /// </summary>
        public bool ShowBreadcrumb { get; set; }

        /// <summary>
        /// A list of class types to be ignored in the comparison. The default is to compare all class types.
        /// </summary>
        public List<Type> ClassTypesToIgnore { get; set; }

        /// <summary>
        /// Only these class types will be compared. The default is to compare all class types.
        /// </summary>
        public List<Type> ClassTypesToInclude { get; set; }

        /// <summary>
        /// Ignore Data Table Names, Data Table Column Names, properties, or fields by name during the comparison. Case sensitive. The default is to compare all members.
        /// </summary>
        /// <example>MembersToIgnore.Add("CreditCardNumber")</example>
        public List<string> MembersToIgnore { get; set; }

        /// <summary>
        /// Only compare elements by name for Data Table Names, Data Table Column Names, properties and fields. Case sensitive. The default is to compare all members.
        /// </summary>
        /// <example>MembersToInclude.Add("FirstName")</example>
        public List<string> MembersToInclude { get; set; }

        //Security restriction in Silverlight prevents getting private properties and fields
#if !PORTABLE
        /// <summary>
        /// If true, private properties and fields will be compared. The default is false.  Silverlight and WinRT restricts access to private variables.
        /// </summary>
        public bool ComparePrivateProperties { get; set; }

        /// <summary>
        /// If true, private fields will be compared. The default is false.  Silverlight and WinRT restricts access to private variables.
        /// </summary>
        public bool ComparePrivateFields { get; set; }
#endif

        /// <summary>
        /// If true, static properties will be compared.  The default is true.
        /// </summary>
        public bool CompareStaticProperties { get; set; }

        /// <summary>
        /// If true, static fields will be compared.  The default is true.
        /// </summary>
        public bool CompareStaticFields { get; set; }

        /// <summary>
        /// If true, child objects will be compared. The default is true. 
        /// If false, and a list or array is compared list items will be compared but not their children.
        /// </summary>
        public bool CompareChildren { get; set; }

        /// <summary>
        /// If true, compare read only properties (only the getter is implemented). The default is true.
        /// </summary>
        public bool CompareReadOnly { get; set; }

        /// <summary>
        /// If true, compare fields of a class (see also CompareProperties).  The default is true.
        /// </summary>
        public bool CompareFields { get; set; }

        /// <summary>
        /// If true, compare each item within a collection to every item in the other.  The default is false. WARNING: setting this to true significantly impacts performance.  
        /// </summary>
        public bool IgnoreCollectionOrder { get; set; }

        /// <summary>
        /// If true, compare properties of a class (see also CompareFields).  The default is true.
        /// </summary>
        public bool CompareProperties { get; set; }

        /// <summary>
        /// The maximum number of differences to detect.  The default is 1 for performance reasons.
        /// </summary>
        public int MaxDifferences { get; set; }

        /// <summary>
        /// The maximum number of differences to detect when comparing byte arrays.  The default is 1.
        /// </summary>
        public int MaxByteArrayDifferences { get; set; }

        /// <summary>
        /// Reflection properties and fields are cached. By default this cache is cleared after each compare.  Set to false to keep the cache for multiple compares.
        /// </summary>
        /// <seealso cref="Caching"/>
        public bool AutoClearCache { get; set; }

        /// <summary>
        /// By default properties and fields for types are cached for each compare.  By default this cache is cleared after each compare.
        /// </summary>
        /// <seealso cref="AutoClearCache"/>
        public bool Caching { get; set; }

        /// <summary>
        /// A list of attributes to ignore a class, property or field
        /// </summary>
        /// <example>AttributesToIgnore.Add(typeof(XmlIgnoreAttribute));</example>
        public List<Type> AttributesToIgnore { get; set; }

        /// <summary>
        /// If true, objects will be compared ignore their type diferences.  The default is false.
        /// </summary>
        public bool IgnoreObjectTypes { get; set; }

        /// <summary>
        /// In the differences string, this is the name for expected name. The default is: Expected 
        /// </summary>
        public string ExpectedName { get; set; }

        /// <summary>
        /// In the differences string, this is the name for the actual name. The default is: Actual
        /// </summary>
        public string ActualName { get; set; }

        /// <summary>
        /// Callback invoked each time the comparer finds a difference. The default is no call back.
        /// </summary>
        public Action<Difference> DifferenceCallback
        {
            get { return _differenceCallback; }
            set
            {
                if (null != value)
                {
                    _differenceCallback = value;
                }
            }
        }

        /// <summary>
        /// Sometimes one wants to match items between collections by some key first, and then
        /// compare the matched objects.  Without this, the comparer basically says there is no 
        /// match in collection B for any given item in collection A that doesn't Compare with a result of true.  
        /// The results of this aren't particularly useful for object graphs that are mostly the same, but not quite. 
        /// Enter CollectionMatchingSpec
        /// 
        /// the enumerable strings should be property (not field, for now, to keep it simple) names of the
        /// Type when encountered that will be used for matching
        /// 
        /// You can use complex type properties, too, as part of the key to match.  To match on all props/fields on 
        /// such a matching key, Don't set this property (default comparer behavior)
        /// NOTE: types are looked up as exact.  e.g. if foo is an entry in the dictionary and bar is a 
        /// sub-class of foo, upon encountering a bar type, the comparer will not find the entry of foo
        /// </summary>
        public Dictionary<Type, IEnumerable<string>> CollectionMatchingSpec { get; set; }

        /// <summary>
        /// A list of custom comparers that take priority over the built in comparers
        /// </summary>
        public List<BaseTypeComparer> CustomComparers { get; set; }

        /// <summary>
        /// If true, string.empty and null will be treated as equal. The default is false.
        /// </summary>
        public bool TreatStringEmptyAndNullTheSame { get; set; }
        #endregion

        #region Methods

        internal bool HasSpec(Type type)
        {
            return CollectionMatchingSpec.Keys.Contains(type)
                   && CollectionMatchingSpec.First(p => p.Key == type).Value.Any();
        }

        /// <summary>
        /// Reset the configuration to the default values
        /// </summary>
        public void Reset()
        {
            AttributesToIgnore = new List<Type>();
            _differenceCallback = d => { };

            MembersToIgnore = new List<string>();
            MembersToInclude = new List<string>();
            ClassTypesToIgnore = new List<Type>();
            ClassTypesToInclude = new List<Type>();

            CompareStaticFields = true;
            CompareStaticProperties = true;
#if !PORTABLE
            ComparePrivateProperties = false;
            ComparePrivateFields = false;
#endif
            CompareChildren = true;
            CompareReadOnly = true;
            CompareFields = true;
            IgnoreCollectionOrder = false;
            CompareProperties = true;
            Caching = true;
            AutoClearCache = true;
            IgnoreObjectTypes = false;
            MaxDifferences = 1;
            ExpectedName = "Expected";
            ActualName = "Actual";
            CustomComparers = new List<BaseTypeComparer>();
            TreatStringEmptyAndNullTheSame = false;
            InterfaceMembers = new List<Type>();
            SkipInvalidIndexers = false;
            MaxByteArrayDifferences = 1;
            CollectionMatchingSpec = new Dictionary<Type, IEnumerable<string>>();
            IgnoreUnknownObjectTypes = false;
        }
        #endregion
    }
}
