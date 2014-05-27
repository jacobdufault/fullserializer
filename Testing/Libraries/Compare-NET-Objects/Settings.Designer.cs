using System.Collections.Specialized;

namespace KellermanSoftware.CompareNetObjects {
    public sealed class Settings {
        public static Settings Default = new Settings();

        public StringCollection MembersToIgnore;
        public StringCollection AttributesToIgnore;
        public bool CompareStaticFields = true;
        public bool CompareStaticProperties = true;
        public bool ComparePrivateProperties = true;
        public bool ComparePrivateFields = true;
        public bool CompareChildren = true;
        public bool CompareReadOnly = true;
        public bool CompareFields = true;
        public bool CompareProperties = true;
        public bool Caching = true;
        public bool AutoClearCache = true;
        public int MaxDifferences = 1;
        public bool IgnoreCollectionOrder = false;
        public bool IgnoreUnknownObjectTypes = false;
    }
}
