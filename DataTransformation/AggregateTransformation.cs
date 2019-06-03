using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration
{
    public class AggregateTransformation : DataFeed
    {
        //TODO: ensure column not added twice
        private readonly List<DataElementPairing> _columns;

        internal AggregateTransformation()
        {
            _columns = new List<DataElementPairing>();
        }

        public DataFeed DataSource { get; set; }

        public void AddGroupingColumn(string sourceColumnName, string outputColumnName = null)
        {
            _columns.Add(new DataElementPairing(sourceColumnName, outputColumnName ?? sourceColumnName, TransformationKind.GroupingKey));
        }

        public void AddSumColumn(string sourceColumnName, string outputColumnName = null)
        {
            _columns.Add(new DataElementPairing(sourceColumnName, outputColumnName ?? sourceColumnName, TransformationKind.Sum));
        }

        protected override IDataRecordIterator CreateReader()
        {
            List<DataRecord> aggregatedRecords = new List<DataRecord>();
            
            List<DataElementPairing> dimensionColumns = _columns.Where(columnInfo => columnInfo.Transform == TransformationKind.GroupingKey).ToList();
            List<DataElementPairing> factColumns = _columns.Where(columnInfo => columnInfo.Transform != TransformationKind.GroupingKey).ToList();

            using (IDataRecordIterator source = DataSource.GetReader())
            {
                foreach(DataRecord sourceRecord in source)
                {
                    DataRecord dataRecord = aggregatedRecords.Find(resultRecord => 
                    {
                        foreach (DataElementPairing dimensionColumn in dimensionColumns)
                        {
                            object expectedValue = resultRecord[dimensionColumn.DestinationName];
                            object actualValue = sourceRecord[dimensionColumn.SourceName];
                            if (!Equals(expectedValue, actualValue))
                            {
                                return false;
                            }
                        }

                        return true;
                    });

                    if (dataRecord == null)
                    {
                        dataRecord = new DataRecord();
                        foreach (DataElementPairing column in dimensionColumns)
                        {
                            dataRecord[column.DestinationName] = sourceRecord[column.SourceName];
                        }
                        aggregatedRecords.Add(dataRecord);
                    }

                    foreach (DataElementPairing column in factColumns)
                    {
                        if (column.Transform == TransformationKind.Sum)
                        {
                            decimal? currentValue = (decimal?) dataRecord[column.DestinationName];
                            decimal valueToAdd = Convert.ToDecimal(sourceRecord[column.SourceName]);
                            decimal newValue = currentValue.GetValueOrDefault(0) + valueToAdd;
                            dataRecord[column.DestinationName] = newValue;
                        }
                    }
                }
            }

            return new RecordListIterator(aggregatedRecords);
        }

        private class RecordListIterator : IDataRecordIterator
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
                _enumerator.Dispose();
                _enumerator = null;
            }
        }
    }
}