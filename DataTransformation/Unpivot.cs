using System;
using System.Collections;
using System.Collections.Generic;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration
{
    public class Unpivot : DataFeed
    {
        public DataSource Source { get; set; }
        public bool AddUniqueID { get; set; }

        protected override IDataRecordIterator CreateReader()
        {
            HorizontalToVerticalReader output = new HorizontalToVerticalReader(Source.GetReader());
            output.AddID = AddUniqueID;
            return output;
        }
        
        private class HorizontalToVerticalReader : IDataRecordIterator
        {
            private const string IDColumnName = "ID";
            private const string KeyColumnName = "PropertyName";
            private const string ValueColumnName = "PropertyValue";

            public HorizontalToVerticalReader(IDataRecordIterator source)
            {
                Source = source;
                _queue = new Queue<DataRecord>();
            }

            public bool AddID { get; set; }
            private IDataRecordIterator Source { get; }
            private readonly Queue<DataRecord> _queue;

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
                if (_queue.Count == 0)
                {
                    if (Source.MoveNext())
                    {
                        UnpivotRow(Source.Current);
                    }
                    else
                    {
                        return false;
                    }
                }

                Current = _queue.Dequeue();
                return true;
            }

            private void UnpivotRow(DataRecord horizontalRecord)
            {
                Guid id = Guid.NewGuid();

                IEnumerable<ColumnInfo> metaData = horizontalRecord.GetLayout();
                foreach (ColumnInfo columnInfo in metaData)
                {
                    DataRecord verticalRecord = new DataRecord();

                    if (AddID)
                    {
                        verticalRecord[IDColumnName] = id;
                    }
                    verticalRecord[KeyColumnName] = columnInfo.Name;
                    verticalRecord[ValueColumnName] = horizontalRecord[columnInfo.Name];

                    _queue.Enqueue(verticalRecord);
                }
            }

            public void Reset()
            {
                _queue.Clear();
                Source.Reset();
            }

            public DataRecord Current { get; private set; }
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                Source?.Dispose();
            }
        }
    }
}