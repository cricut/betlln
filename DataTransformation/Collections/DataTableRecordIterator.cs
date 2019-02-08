using System.Collections;
using System.Collections.Generic;
using System.Data;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.Collections
{
    public class DataTableRecordIterator : IDataRecordIterator
    {
        private int _index;
        private readonly DataTable _baseTable;

        public DataTableRecordIterator(DataTable dataTable)
        {
            _index = -1;
            _baseTable = dataTable;
        }

        public IEnumerator<DataRecord> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool MoveNext()
        {
            if (_index == _baseTable.Rows.Count - 1)
            {
                Current = null;
                return false;
            }

            _index++;

            DataRecord record = new DataRecord();

            foreach (DataColumn column in _baseTable.Columns)
            {
                DataRow row = _baseTable.Rows[_index];
                record[column.ColumnName] = row[column.ColumnName];
            }

            Current = record;
            return true;
        }

        public void Reset()
        {
            _index = 0;
        }

        public DataRecord Current { get; private set; }
        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}