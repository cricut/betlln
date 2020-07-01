using System.Collections;
using System.Collections.Generic;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.Collections
{
    internal class RecordListIterator : IDataRecordIterator
    {
        private IEnumerator<DataRecord> _enumerator;

        public RecordListIterator(IEnumerable<DataRecord> recordList)
        {
            _enumerator = recordList.GetEnumerator();
        }

        public IEnumerator<DataRecord> GetEnumerator()
        {
            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }

        public DataRecord Current => _enumerator.Current;
        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _enumerator?.Dispose();
            _enumerator = null;
        }
    }
}