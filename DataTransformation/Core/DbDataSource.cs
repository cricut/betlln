using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Betlln.Data.Integration.Core
{
    public class DbDataSource : DataSource, ISqlActivity, IColumnMapper
    {
        protected string _commandText;
        protected CommandType _commandType;
        private readonly List<DataElementPairing> _columnMappings;

        protected DbDataSource()
        {
            Parameters = new ParameterSet();
            _columnMappings = new List<DataElementPairing>();
        }

        public string CommandText
        {
            get
            {
                return _commandText;
            }
            set
            {
                _commandType = CommandType.Text;
                _commandText = value;
            }
        }

        public ParameterSet Parameters { get; }
        public uint Timeout { get; set; }

        public void MapColumns<T>(string sourceName, string outputAlias)
        {
            _columnMappings.Add(new DataElementPairing(sourceName, outputAlias, typeof(T)));
        }

        public override DataTable GetResults()
        {
            DataTable rawDataTable = this.Execute(_commandText, _commandType, GetDataTable);
            if (_columnMappings.Any())
            {
                return rawDataTable.RepackageTable(_columnMappings);
            }
            return rawDataTable;
        }

        private DataTable GetDataTable(DbCommand command)
        {
            DataSet container = new DataSet();

            Type adapterType = Connection.GetDataAdapterType();
            using (DbDataAdapter dataAdapter = (DbDataAdapter)Activator.CreateInstance(adapterType))
            {
                dataAdapter.SelectCommand = command;
                dataAdapter.Fill(container);
            }

            return container.Tables[0];
        }

        public override T GetScalar<T>()
        {
            if (_columnMappings.Any())
            {
                throw new InvalidOperationException();
            }
            return (T)this.Execute(_commandText, _commandType, x => x.ExecuteScalar());
        }

        protected override IDataRecordIterator CreateReader()
        {
            return new DbRecordIterator(this, _commandType, _columnMappings);
        }

        private class DbRecordIterator : IDataRecordIterator
        {
            private readonly ResourceStack _sqlPipeline;
            private readonly List<DataElementPairing> _columnNameMappings;
            private readonly Dictionary<int, DataElementPairing> _fieldMappings;

            public DbRecordIterator(ISqlActivity sqlSettings, CommandType commandType, List<DataElementPairing> columnNameMappings)
            {
                _sqlPipeline = new ResourceStack();
                _columnNameMappings = columnNameMappings;
                _fieldMappings = new Dictionary<int, DataElementPairing>();

                DbConnection connection = sqlSettings.GetConnection();
                _sqlPipeline.Push(connection);

                DbCommand command = sqlSettings.BuildCommand(connection, sqlSettings.CommandText, commandType);
                _sqlPipeline.Push(command);

                DbDataReader reader = command.ExecuteReader();
                _sqlPipeline.Push(reader);
            }

            private DbDataReader Reader => (DbDataReader)_sqlPipeline.Tip;

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
                if (Reader.Read())
                {
                    DataRecord record = new DataRecord();
                    for (int i = 0; i < Reader.FieldCount; i++)
                    {
                        PopulateRecordFromField(record, i);
                    }
                    Current = record;
                    return true;
                }

                Current = null;
                return false;
            }

            private void PopulateRecordFromField(DataRecord record, int fieldIndex)
            {
                if (_columnNameMappings.Any())
                {
                    if (_fieldMappings.ContainsKey(fieldIndex))
                    {
                        PopulateRecordFromKnownField(record, fieldIndex);
                    }
                    else
                    {
                        PopulateRecordFromUnknownField(record, fieldIndex);
                    }
                }
                else
                {
                    string fieldName = Reader.GetName(fieldIndex);
                    record[fieldName] = Reader[fieldIndex];
                }
            }

            private void PopulateRecordFromKnownField(DataRecord record, int fieldIndex)
            {
                DataElementPairing pairing = _fieldMappings[fieldIndex];
                if (pairing != null)
                {
                    record[pairing.DestinationName] = Reader[fieldIndex];
                }
            }

            private void PopulateRecordFromUnknownField(DataRecord record, int fieldIndex)
            {
                string fieldName = Reader.GetName(fieldIndex);

                DataElementPairing pairing =
                    _columnNameMappings.Find(x => fieldName.Equals(x.SourceName, StringComparison.InvariantCultureIgnoreCase));
                if (pairing != null)
                {
                    object rawValue = Reader[fieldIndex];

                    if (rawValue != null && rawValue != DBNull.Value)
                    {
                        _fieldMappings.Add(fieldIndex, pairing);
                        if (pairing.ElementType != rawValue.GetType())
                        {
                            throw new InvalidCastException();
                        }
                    }

                    record[pairing.DestinationName] = rawValue;
                }
                else
                {
                    _fieldMappings.Add(fieldIndex, null);
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
                _sqlPipeline.Dispose();
            }
        }
    }
}