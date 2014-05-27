using System;
using System.Collections;
using System.Reflection;

namespace KellermanSoftware.CompareNetObjects.IgnoreOrderTypes
{
    internal class IndexerCollectionLooper : IEnumerable
    {
        private readonly object _indexer;
        private readonly PropertyInfo _info;
        private readonly int _cnt;

        public IndexerCollectionLooper(object obj, PropertyInfo info, int cnt)
        {
            _indexer = obj;
            if (info == null)
                throw new ArgumentNullException("info");

            _info = info;
            _cnt = cnt;
        }

        public IEnumerator GetEnumerator()
        {
            for (var i = 0; i < _cnt; i++)
            {
                object value = _info.GetValue(_indexer, new object[] { i });
                yield return value;
            }
        }
    }
}
