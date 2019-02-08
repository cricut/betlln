using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.SqlServer
{
    public class SqlBulkCopyTask : Task, IColumnMapper
    {
        private readonly List<DataElementPairing> _columnMappings;

        internal SqlBulkCopyTask()
        {
            BatchSize = 1000;
            _columnMappings = new List<DataElementPairing>();
        }

        public DataFeed Source { get; set; }
        public IConnectionManager DestinationConnectionManager { get; set; }
        public string DestinationTableName { get; set; }
        public int BatchSize { get; set; }

        public void MapColumns<T>(string sourceName, string destinationColumnName)
        {
            if (string.IsNullOrWhiteSpace(DestinationTableName))
            {
                throw new InvalidOperationException("To map columns, the table name must first be specified.");
            }

            DataElementPairing columnMapping = new DataElementPairing(sourceName, destinationColumnName, typeof(T));
            columnMapping.MaximumLength = TableMetaDataCache.Default.GetStringMaximumLength((SqlConnectionManager) DestinationConnectionManager, DestinationTableName, destinationColumnName);

            _columnMappings.Add(columnMapping);
        }

        protected override void ExecuteTasks()
        {
            RecordReader sourceReader = new RecordReader(_columnMappings, Source.GetReader());

            SqlConnectionManager connectionManager = (SqlConnectionManager) DestinationConnectionManager;
            string destinationAddress = connectionManager.ConnectionAddress;

            using (SqlBulkCopy bulkInsert = new SqlBulkCopy(destinationAddress, SqlBulkCopyOptions.TableLock))
            {
                bulkInsert.DestinationTableName = DestinationTableName;
                foreach (DataElementPairing columnMapping in _columnMappings)
                {
                    bulkInsert.ColumnMappings.Add(columnMapping.SourceName, columnMapping.DestinationName);
                }

                bulkInsert.BatchSize = BatchSize;
                bulkInsert.NotifyAfter = bulkInsert.BatchSize;
                bulkInsert.BulkCopyTimeout = (int) (Timeout * SystemExtensions.SecondsPerMinute);

                bulkInsert.WriteToServer(sourceReader);
            }
        }
    }
}