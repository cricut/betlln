using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Betlln.Data.Integration.Collections;

namespace Betlln.Data.Integration.Core
{
    public abstract class DataFeed : IDisposable
    {
        private DataTable _fullResults;
        private IDataRecordIterator _reader;

        public IDataRecordIterator GetReader()
        {
            if (_fullResults != null)
            {
                return new DataTableRecordIterator(_fullResults);
            }

            if (_reader == null)
            {
                RuntimeContext.DefaultLogger.Debug($"Started {GetType().Name}");
                _reader = CreateReader();
            }
            return _reader;
        }

        protected abstract IDataRecordIterator CreateReader();

        public virtual DataTable GetResults()
        {
            if (_fullResults == null)
            {
                RuntimeContext.DefaultLogger.Debug($"Started {GetType().Name}");
                DataTable allResults = new DataTable();

                using (IDataRecordIterator dataRecordIterator = GetReader())
                {
                    foreach (DataRecord record in dataRecordIterator)
                    {
                        List<ColumnInfo> columns = record.GetLayout();
                        foreach (ColumnInfo recordColumn in columns)
                        {
                            if (!allResults.Columns.Contains(recordColumn.Name))
                            {
                                allResults.Columns.Add(recordColumn.Name, recordColumn.DataType);
                            }
                        }

                        DataRow row = allResults.NewRow();
                        allResults.Rows.Add(row);
                        foreach (ColumnInfo recordColumn in columns)
                        {
                            row[recordColumn.Name] = record[recordColumn.Name];
                        }
                    }
                }

                _fullResults = allResults;
            }

            return _fullResults;
        }

        public virtual T GetScalar<T>()
        {
            using (IDataRecordIterator dataRecordIterator = GetReader())
            {
                DataRecord firstRecord = dataRecordIterator.First();
                return (T) firstRecord[0];
            }
        }

        public virtual long GetRowCount()
        {
            return GetReader().Count();
        }

        public virtual void Dispose()
        {
            _reader?.Dispose();
            _reader = null;
        }
    }
}