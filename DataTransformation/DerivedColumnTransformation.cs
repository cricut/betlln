using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Betlln.Data.Integration.Core;
using Betlln.Logging;

namespace Betlln.Data.Integration
{
    public class DerivedColumnTransformation : DataFeed
    {
        private readonly Dictionary<string, Func<DataRecord, object>> _extraColumns;

        internal DerivedColumnTransformation()
        {
            _extraColumns = new Dictionary<string, Func<DataRecord, object>>();
        }

        public DataFeed Source { get; set; }

        protected override IDataRecordIterator CreateReader()
        {
            return new ExtraReader(Source.GetReader(), _extraColumns);
        }

        public void AddColumn<T>(string columnName, T constantValue)
        {
            _extraColumns.Add(columnName, r => constantValue);
        }

        public void AddColumn<T>(string columnName, Func<DataRecord, T> derivation)
        {
            _extraColumns.Add(columnName, record => derivation(record));
        }

        private class ExtraReader : IDataRecordIterator
        {
            private readonly IDataRecordIterator _baseIterator;
            private readonly Dictionary<string, Func<DataRecord, object>> _extraColumns;

            public ExtraReader(IDataRecordIterator baseIterator, Dictionary<string, Func<DataRecord, object>> extraColumns)
            {
                _baseIterator = baseIterator;
                _extraColumns = extraColumns;
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
                if (_baseIterator.MoveNext())
                {
                    DataRecord record = _baseIterator.Current;
                    Debug.Assert(record != null);
                    foreach (KeyValuePair<string, Func<DataRecord, object>> columnSetup in _extraColumns)
                    {
                        Func<DataRecord, object> calculation = columnSetup.Value;
                        try
                        {
                            record[columnSetup.Key] = calculation(record);
                        }
                        catch (Exception error)
                        {
                            Dts.Notify.All(error.ToString(), LogEventType.Error);
                        }
                    }
                    Current = record;

                    return true;
                }

                return false;
            }

            public void Reset()
            {
                _baseIterator.Reset();
            }

            public DataRecord Current { get; private set; }
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _baseIterator?.Dispose();
            }
        }
    }
}