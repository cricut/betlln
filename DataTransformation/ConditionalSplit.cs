using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Betlln.Data.Integration.Core;
using Betlln.Logging;
using Task = System.Threading.Tasks.Task;

namespace Betlln.Data.Integration
{
    public class ConditionalSplit : IConditionalSplit
    {
        internal const string DefaultStreamName = "DEFAULT";

        private Task _reader;
        private readonly Dictionary<string, Func<DataRecord, bool>> _namedStreams;
        private readonly ConcurrentDictionary<string, AsyncFeed> _realizedStreams;

        internal ConditionalSplit()
        {
            _realizedStreams = new ConcurrentDictionary<string, AsyncFeed>();
            _namedStreams = new Dictionary<string, Func<DataRecord, bool>>();
        }

        public DataFeed Source { get; set; }
        private string DynamicColumnName { get; set; }
        private bool ExcludeDynamicColumnFromOutput { get; set; }

        private bool Finished
        {
            get { return _reader != null && _reader.IsCompleted; }
        }

        public void DefineOutput(string outputName, Func<DataRecord, bool> filter)
        {
            if (string.IsNullOrWhiteSpace(outputName))
            {
                throw new ArgumentNullException(nameof(outputName));
            }

            if (outputName.ToUpper() == DefaultStreamName || _namedStreams.ContainsKey(outputName))
            {
                throw new ArgumentException("An output with this name has already been defined.");
            }

            if (!string.IsNullOrWhiteSpace(DynamicColumnName))
            {
                ThrowInvalidUsageException();
            }

            _namedStreams.Add(outputName, filter);
        }

        public void DefineOutputsBy(string columnName, bool excludeColumn = false)
        {
            if (_namedStreams.Count > 0)
            {
                ThrowInvalidUsageException();
            }

            DynamicColumnName = columnName;
            ExcludeDynamicColumnFromOutput = excludeColumn;
        }

        private static void ThrowInvalidUsageException()
        {
            throw new InvalidOperationException("Cannot combine named and dynamic outputs.");
        }

        public DataFeed Output(string outputName)
        {
            if (string.IsNullOrWhiteSpace(outputName))
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrWhiteSpace(DynamicColumnName) && _realizedStreams.Count == 0)
            {
                _realizedStreams[DefaultStreamName] = new AsyncFeed { Name = DefaultStreamName };
                foreach (KeyValuePair<string, Func<DataRecord, bool>> streamInfo in _namedStreams)
                {
                    string streamName = streamInfo.Key;
                    AsyncFeed namedFeed = new AsyncFeed
                    {
                        Name = streamName,
                        Predicate = streamInfo.Value
                    };
                    _realizedStreams[streamName] = namedFeed;
                }
            }
            else
            {
                RealizeDynamicFeed(outputName);
            }

            if (_reader == null)
            {
                _reader = Task.Factory.StartNew(ReadSource);
            }

            return _realizedStreams[outputName];
        }
        
        private void ReadSource()
        {
            try
            {
                using (IDataRecordIterator reader = Source.GetReader())
                {
                    foreach (DataRecord record in reader)
                    {
                        AsyncFeed targetStream = FindTargetStream(record);
                        targetStream.Push(record);
                    }
                }

                Debug.Print("Finishing streams");
                foreach (AsyncFeed stream in _realizedStreams.Values)
                {
                    stream.Finish();
                }
            }
            catch (Exception error)
            {
                Dts.Notify.All(error.ToString(), LogEventType.Error);
            }
        }

        private AsyncFeed FindTargetStream(DataRecord record)
        {
            if (!string.IsNullOrWhiteSpace(DynamicColumnName))
            {
                string actualOutputName = record[DynamicColumnName]?.ToString() ?? string.Empty;
                return RealizeDynamicFeed(actualOutputName);
            }

            AsyncFeed targetStream = null;

            foreach (KeyValuePair<string, AsyncFeed> activeComponent in _realizedStreams)
            {
                string streamName = activeComponent.Key;
                AsyncFeed stream = activeComponent.Value;
                Func<DataRecord, bool> predicate = stream.Predicate;

                if (streamName != DefaultStreamName && predicate(record))
                {
                    targetStream = stream;
                }
            }

            return targetStream ?? _realizedStreams[DefaultStreamName];
        }

        private AsyncFeed RealizeDynamicFeed(string outputName)
        {
            return _realizedStreams.AddOrUpdate(outputName, BuildDynamicFeed, (key, current) => current);
        }

        private AsyncFeed BuildDynamicFeed(string name)
        {
            Debug.Print($"Realizing stream {name}");
            AsyncFeed dynamicFeed = new AsyncFeed {Name = name};

            if (ExcludeDynamicColumnFromOutput)
            {
                dynamicFeed.ColumnsToExclude.Add(DynamicColumnName);
            }

            if (Finished)
            {
                Debug.Print($"Started finished stream {dynamicFeed.Name}");
                dynamicFeed.Finish();
            }

            return dynamicFeed;
        }

        public void Dispose()
        {
            if (!Finished)
            {
                throw new ThreadInterruptedException();
            }

            List<Exception> exceptions = new List<Exception>();

            foreach (AsyncFeed stream in _realizedStreams.Values)
            {
                try
                {
                    stream.Dispose();
                }
                catch (Exception disposeError)
                {
                    exceptions.Add(disposeError);
                }
            }

            if (exceptions.Any())
            {
                throw new ObjectDisposedException("Could not close all output streams.",
                                                  new AggregateException(exceptions));
            }
        }
    }
}