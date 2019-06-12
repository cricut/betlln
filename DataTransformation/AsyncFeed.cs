using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Betlln.Data.Integration.Core;
using Betlln.Logging;

namespace Betlln.Data.Integration
{
    internal class AsyncFeed : DataFeed, IDataRecordIterator
    {
        private volatile bool _finished;
        private readonly ConcurrentQueue<DataRecord> _records;

        internal AsyncFeed()
        {
            _finished = false;
            _records = new ConcurrentQueue<DataRecord>();
            ColumnsToExclude = new List<string>();
        }

        public string Name { get; set; }
        public List<string> ColumnsToExclude { get; }
        public Func<DataRecord, bool> Predicate { get; set; }

        public void Push(DataRecord record)
        {
            _records.Enqueue(record);
        }

        protected override IDataRecordIterator CreateReader()
        {
            return this;
        }

        public void Finish()
        {
            _finished = true;
            Debug.Print($"Stream {Name} has {_records.Count} left to emit");
        }

        public IEnumerator<DataRecord> GetEnumerator()
        {
            Dts.Notify.Log($"Starting stream {Name}", LogEventType.Debug);
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool MoveNext()
        {
            while(!_finished || _records.Count > 0)
            {
                DataRecord record;
                if (_records.TryDequeue(out record))
                {
                    foreach (string columnName in ColumnsToExclude)
                    {
                        record.DeleteColumn(columnName);
                    }

                    Current = record;
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public DataRecord Current { get; private set; }
        object IEnumerator.Current => Current;

        public override void Dispose()
        {
            if (!_finished)
            {
                throw new ThreadInterruptedException();
            }
        }
    }
}