using System;
using System.Collections.Generic;
using System.Linq;
using Betlln.Data.Integration.Collections;
using Betlln.Data.Integration.Core;
using Betlln.Logging;

namespace Betlln.Data.Integration
{
    public class ConditionalSplitCached : IConditionalSplit
    {
        private Dictionary<string, List<DataRecord>> _records;
        private readonly Dictionary<string, Func<DataRecord, bool>> _namedStreams;

        internal ConditionalSplitCached()
        {
            _namedStreams = new Dictionary<string, Func<DataRecord, bool>>();    
        }

        public DataFeed Source { get; set; }
        private string DynamicColumnName { get; set; }
        private bool ExcludeDynamicColumnFromOutput { get; set; }

        public void DefineOutput(string outputName, Func<DataRecord, bool> filter)
        {
            if (!string.IsNullOrWhiteSpace(DynamicColumnName))
            {
                throw new InvalidOperationException();
            }

            _namedStreams.Add(outputName, filter);
        }

        public void DefineOutputsBy(string columnName, bool excludeColumn = false)
        {
            if (_namedStreams.Any() || !string.IsNullOrWhiteSpace(DynamicColumnName))
            {
                throw new InvalidOperationException();
            }

            DynamicColumnName = columnName;
            ExcludeDynamicColumnFromOutput = excludeColumn;
        }

        public DataFeed Output(string outputName)
        {
            EnsureCache();

            if (!_records.ContainsKey(outputName))
            {
                if (_namedStreams.Any())
                {
                    throw new ArgumentException($"There is not output named '{outputName}'.");
                }

                Dts.Notify.All($"The output '{outputName}' does not exist.");
                return ListDataFeed.Empty;
            }

            return new ListDataFeed(_records[outputName]);
        }

        private void EnsureCache()
        {
            if (_records == null)
            {
                _records = new Dictionary<string, List<DataRecord>>();
                if (_namedStreams.Any() && !_records.ContainsKey(ConditionalSplit.DefaultStreamName))
                {
                    _records.Add(ConditionalSplit.DefaultStreamName, new List<DataRecord>());
                }

                using (IDataRecordIterator reader = Source.GetReader())
                {
                    foreach (DataRecord record in reader)
                    {
                        AddRecordToMatchingOutput(record);
                    }
                }
            }
        }

        private void AddRecordToMatchingOutput(DataRecord record)
        {
            if (_namedStreams.Any())
            {
                if (!AddRecordToNamedOutput(record))
                {
                    _records[ConditionalSplit.DefaultStreamName].Add(record);
                }
            }
            else
            {
                string dynamicOutputName = record[DynamicColumnName]?.ToString() ?? ConditionalSplit.DefaultStreamName;
                if (ExcludeDynamicColumnFromOutput)
                {
                    record.DeleteColumn(DynamicColumnName);
                }
                AddRecordToOutput(dynamicOutputName, record);
            }
        }

        private bool AddRecordToNamedOutput(DataRecord record)
        {
            foreach (var namedStream in _namedStreams)
            {
                string outputName = namedStream.Key;
                Func<DataRecord, bool> predicate = namedStream.Value;

                if (predicate(record))
                {
                    AddRecordToOutput(outputName, record);
                    return true;
                }
            }

            return false;
        }

        private void AddRecordToOutput(string outputName, DataRecord record)
        {
            if (!_records.ContainsKey(outputName))
            {
                _records.Add(outputName, new List<DataRecord>());
            }

            _records[outputName].Add(record);
        }

        public void Dispose()
        {
        }

        private class ListDataFeed : DataFeed
        {
            private readonly IEnumerable<DataRecord> _records;

            public ListDataFeed(IEnumerable<DataRecord> records)
            {
                _records = records;
            }

            protected override IDataRecordIterator CreateReader()
            {
                return new RecordListIterator(_records);
            }

            public static DataFeed Empty => new ListDataFeed(new List<DataRecord>());
        }
    }
}