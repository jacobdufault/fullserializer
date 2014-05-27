//*********************************
// HEY THERE!!!!!!!!!!!!
// READ THIS PLEASE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// If you have downloaded this source branch, it is in the process of a giant refactoring.  
// It is not ready yet for production.  
// Please download the source for build 1.7.4 which is the current stable release:
// https://comparenetobjects.codeplex.com/SourceControl/changeset/80131
//
// If you are not developing something for production. You are certainly welcome to see what I am up to.
// You can see my progress by looking at the ToDo.txt
//
// See what is coming in Version 2.0:
// https://comparenetobjects.codeplex.com/releases/view/119173
//
// God bless you fellow developer.
//
// VERSION 2.0.0.0
// http://comparenetobjects.codeplex.com/

#region Includes
using System;
using System.Collections.Generic;
#endregion

namespace KellermanSoftware.CompareNetObjects
{
    /// <summary>
    /// Obsolete Use CompareLogic instead
    /// </summary>
    [Obsolete("Use CompareLogic instead", false)]
    public class CompareObjects
    {
        #region Class Variables

        private CompareLogic _logic;
        private ComparisonResult _result;
        #endregion

        #region Constructor

        /// <summary>
        /// Obsolete Use CompareLogic instead
        /// </summary>
        [Obsolete("Use CompareLogic instead", false)]
        public CompareObjects()
        {
            _logic = new CompareLogic();
            _result = new ComparisonResult(_logic.Config);
        }

#if !PORTABLE
        /// <summary>
        /// Obsolete Use CompareLogic instead
        /// </summary>
        [Obsolete("Use CompareLogic instead", false)]
        public CompareObjects(bool useAppConfigSettings)
        {
            _logic = new CompareLogic(useAppConfigSettings);
            _result = new ComparisonResult(_logic.Config);
        }
#endif

        #endregion

        #region Properties


#if !PORTABLE
        /// <summary>
        /// Obsolete Use the ComparisonResult.ElapsedMilliseconds returned from CompareLogic.Compare
        /// </summary>
        [Obsolete("Use the ComparisonResult.ElapsedMilliseconds returned from CompareLogic.Compare", false)]
        public long ElapsedMilliseconds
        {
            get { return _result.ElapsedMilliseconds; }
        }
#endif

        /// <summary>
        /// Obsolete Use CompareLogic.Config.ShowBreadcrumb instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.ShowBreadcrumb instead")]
        public bool ShowBreadcrumb
        {
            get { return _logic.Config.ShowBreadcrumb; }
            set { _logic.Config.ShowBreadcrumb = value; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.MembersToIgnore for members or CompareLogic.Config.ClassTypesToIgnore instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.MembersToIgnore for members or CompareLogic.Config.ClassTypesToIgnore instead")]
        public List<string> ElementsToIgnore
        {
            get { return _logic.Config.MembersToIgnore; }
            set { _logic.Config.MembersToIgnore = value; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.MembersToInclude or CompareLogic.Config.ClassTypesToInclude instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.MembersToInclude or CompareLogic.Config.ClassTypesToInclude instead")]
        public List<string> ElementsToInclude
        {
            get { return _logic.Config.MembersToInclude; }
            set { _logic.Config.MembersToInclude = value; }
        }

        //Security restriction in Silverlight prevents getting private properties and fields
#if !PORTABLE

        /// <summary>
        /// Obsolete Use CompareLogic.Config.ComparePrivateProperties instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.ComparePrivateProperties instead")]
        public bool ComparePrivateProperties
        {
            get { return _logic.Config.ComparePrivateProperties; }
            set { _logic.Config.ComparePrivateProperties = value; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.ComparePrivateFields instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.ComparePrivateFields instead")]
        public bool ComparePrivateFields
        {
            get { return _logic.Config.ComparePrivateFields; }
            set { _logic.Config.ComparePrivateFields = value; }
        }
#endif

        /// <summary>
        /// Obsolete Use CompareLogic.Config.CompareStaticProperties instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.CompareStaticProperties instead")]
        public bool CompareStaticProperties
        {
            get { return _logic.Config.CompareStaticProperties; }
            set { _logic.Config.CompareStaticProperties = value; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.CompareStaticFields instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.CompareStaticFields instead")]
        public bool CompareStaticFields
        {
            get { return _logic.Config.CompareStaticFields; }
            set { _logic.Config.CompareStaticFields = value; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.CompareChildren instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.CompareChildren instead")]
        public bool CompareChildren
        {
            get { return _logic.Config.CompareChildren; }
            set { _logic.Config.CompareChildren = value; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.CompareReadOnly instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.CompareReadOnly instead")]
        public bool CompareReadOnly
        {
            get { return _logic.Config.CompareReadOnly; }
            set { _logic.Config.CompareReadOnly = value; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.CompareFields instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.CompareFields instead")]
        public bool CompareFields
        {
            get { return _logic.Config.CompareFields; }
            set { _logic.Config.CompareFields = value; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.IgnoreCollectionOrder instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.IgnoreCollectionOrder instead")]
        public bool IgnoreCollectionOrder
        {
            get { return _logic.Config.IgnoreCollectionOrder; }
            set { _logic.Config.IgnoreCollectionOrder = value; }
        }


        /// <summary>
        /// Obsolete Use CompareLogic.Config.CompareProperties instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.CompareProperties instead")]
        public bool CompareProperties
        {
            get { return _logic.Config.CompareProperties; }
            set { _logic.Config.CompareProperties = value; }
        }


        /// <summary>
        /// Obsolete Use CompareLogic.Config.MaxDifferences instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.MaxDifferences instead")]
        public int MaxDifferences
        {
            get { return _logic.Config.MaxDifferences; }
            set { _logic.Config.MaxDifferences = value; }
        }

        /// <summary>
        /// Obsolete Use the ComparisonResult.Differences returned from CompareLogic.Compare
        /// </summary>
        [Obsolete("Use the ComparisonResult.Differences returned from CompareLogic.Compare", false)]
        public List<Difference> Differences
        {
            get { return _result.Differences; }
            set { _result.Differences = value; }
        }

        /// <summary>
        /// Obsolete Use the ComparisonResult.DifferencesString returned from CompareLogic.Compare
        /// </summary>
        [Obsolete("Use the ComparisonResult.DifferencesString returned from CompareLogic.Compare", false)]
        public string DifferencesString
        {
            get { return _result.DifferencesString; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.AutoClearCache instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.AutoClearCache instead")]
        public bool AutoClearCache
        {
            get { return _logic.Config.AutoClearCache; }
            set { _logic.Config.AutoClearCache = value; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.Caching instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.Caching instead")]
        public bool Caching
        {
            get { return _logic.Config.Caching; }
            set { _logic.Config.Caching = value; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.AttributesToIgnore instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.AttributesToIgnore instead")]
        public List<Type> AttributesToIgnore
        {
            get { return _logic.Config.AttributesToIgnore; }
            set { _logic.Config.AttributesToIgnore = value; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.IgnoreObjectTypes instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.IgnoreObjectTypes instead")]
        public bool IgnoreObjectTypes
        {
            get { return _logic.Config.IgnoreObjectTypes; }
            set { _logic.Config.IgnoreObjectTypes = value; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.CustomComparers instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.CustomComparers", true)]
        public Func<Type, bool> IsUseCustomTypeComparer { get; set; }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.CustomComparers instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.CustomComparers", true)]
        public Action<CompareObjects, object, object, string> CustomComparer { get; set; }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.ExpectedName instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.ExpectedName instead")]
        public string ExpectedName
        {
            get { return _logic.Config.ExpectedName; }
            set { _logic.Config.ExpectedName = value; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.ActualName instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.ActualName instead")]
        public string ActualName
        {
            get { return _logic.Config.ActualName; }
            set { _logic.Config.ActualName = value; }
        }

        /// <summary>
        /// Obsolete Use CompareLogic.Config.DifferenceCallback instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.DifferenceCallback instead")]
        public Action<Difference> DifferenceCallback
        {
            get { return _logic.Config.DifferenceCallback; }
            set { _logic.Config.DifferenceCallback = value; }
        }


        /// <summary>
        /// Obsolete Use CompareLogic.Config.CollectionMatchingSpec instead
        /// </summary>
        [Obsolete("Use CompareLogic.Config.CollectionMatchingSpec instead")]
        public Dictionary<Type, IEnumerable<string>> CollectionMatchingSpec
        {
            get { return _logic.Config.CollectionMatchingSpec; }
            set { _logic.Config.CollectionMatchingSpec = value; }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Obsolete Use CompareLogic.Compare instead
        /// </summary>
        [Obsolete("Use CompareLogic.Compare instead", false)]
        public bool Compare(object object1, object object2)
        {
            _result = _logic.Compare(object1, object2);

            return _result.AreEqual;
        }

        /// <summary>
        /// Obsolete Use CompareLogic.ClearCache instead
        /// </summary>
        [Obsolete("Use CompareLogic.ClearCache instead", false)]
        public void ClearCache()
        {
            _logic.ClearCache();
        }

        #endregion
    }
}
