using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Betlln.Data.Integration.Core;
using DataTable = System.Data.DataTable;

namespace Betlln.Data.Integration.SqlServer
{
    public class SqlDataSource : DataSource, ISqlActivity
    {
        internal SqlDataSource()
        {
            Parameters = new ParameterSet();
        }

        public string CommandText { get; set; }
        public ParameterSet Parameters { get; }
        public uint Timeout { get; set; }

        public override DataTable GetResults()
        {
            return this.Execute(CommandText, CommandType.Text, SqlActivityExtensionMethods.GetDataTable);
        }

        public override T GetScalar<T>()
        {
            return (T) this.Execute(CommandText, CommandType.Text, x => x.ExecuteScalar());
        }

        protected override IDataRecordIterator CreateReader()
        {
            SqlDataReader reader = this.Execute(CommandText, CommandType.Text, x => x.ExecuteReader());
            return new SqlRecordIterator(reader);
        }

        private class SqlRecordIterator : IDataRecordIterator
        {
            private readonly SqlDataReader _reader;

            public SqlRecordIterator(SqlDataReader reader)
            {
                _reader = reader;
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
                if (_reader.Read())
                {
                    DataRecord record = new DataRecord();
                    for (int i = 0; i < _reader.FieldCount; i++)
                    {
                        string name = _reader.GetName(i);
                        record[name] = _reader[i];
                    }
                    Current = record;
                    return true;
                }
                else
                {
                    Current = null;
                    return false;
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public DataRecord Current { get; private set; }
            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _reader.Dispose();
            }
        }
    }
}