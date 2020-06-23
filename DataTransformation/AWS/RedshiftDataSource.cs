using System;
using System.Collections;
using System.Collections.Generic;
using Betlln.Data.Integration.Core;
using Npgsql;

namespace Betlln.Data.Integration.AWS
{
    public class RedshiftDataSource : DataSource
    {
        internal RedshiftDataSource()
        {
            Parameters = new Dictionary<string, object>();
        }

        public string QueryText { get; set; }
        public Dictionary<string, object> Parameters { get; }
        
        protected override IDataRecordIterator CreateReader()
        {
            return new PostgresRecordIterator(Connection, QueryText, Parameters);
        }

        private class PostgresRecordIterator : IDataRecordIterator
        {
            private readonly ResourceStack _dataPipeline;

            public PostgresRecordIterator(IConnectionManager connectionManager, string queryText, Dictionary<string, object> parameters)
            {
                _dataPipeline = new ResourceStack();

                NpgsqlConnection connection = connectionManager.GetConnection() as NpgsqlConnection;
                _dataPipeline.Push(connection);

                NpgsqlCommand command = new NpgsqlCommand(queryText, connection);
                foreach (KeyValuePair<string, object> parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
                _dataPipeline.Push(command);

                _dataPipeline.Push(command.ExecuteReader());
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<DataRecord> GetEnumerator()
            {
                return this;
            }

            public bool MoveNext()
            {
                NpgsqlDataReader reader = (NpgsqlDataReader) _dataPipeline.Tip;
                if (reader.Read())
                {
                    DataRecord record = new DataRecord();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string fieldName = reader.GetName(i);
                        record[fieldName] = reader[i];
                    }
                    Current = record;

                    return true;
                }

                return false;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator.Current => Current;
            public DataRecord Current { get; private set; }
            
            public void Dispose()
            {
                _dataPipeline.Dispose();
            }
        }
    }
}