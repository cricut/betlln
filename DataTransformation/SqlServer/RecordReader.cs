using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.SqlServer
{
    // ReSharper disable once ClassTooBig
    internal class RecordReader : IDataReader
    {
        private IDataRecordIterator _iterator;
        private readonly List<DataElementPairing> _columnMappings;

        public RecordReader(List<DataElementPairing> columnMappings, IDataRecordIterator iterator)
        {
            _iterator = iterator;
            _columnMappings = columnMappings;
        }

        public string GetName(int i)
        {
            return _columnMappings[i].SourceName;
        }

        public string GetDataTypeName(int i)
        {
            return _columnMappings[i].ElementType.Name;
        }

        public Type GetFieldType(int i)
        {
            return _columnMappings[i].ElementType;
        }

        public object GetValue(int i)
        {
            if (_iterator?.Current == null)
            {
                throw new InvalidOperationException();
            }

            DataElementPairing columnMapping = _columnMappings[i];
            string name = columnMapping.SourceName;
            object value = _iterator.Current[name];

            if (value is string && columnMapping.MaximumLength.HasValue)
            {
                int actualLength = value.ToString().Length;
                if(actualLength > columnMapping.MaximumLength.Value)
                {
                    string message = $"A too-long value for '{name}' was found (max: {columnMapping.MaximumLength.Value}, actual: {actualLength})";
                    Dts.Events.RaiseInformation(message);
                }
            }

            return value;
        }

        public int GetValues(object[] values)
        {
            for (int i = 0; i < FieldCount; i++)
            {
                values[i] = GetValue(i);
            }
            return FieldCount;
        }

        public int GetOrdinal(string name)
        {
            return _columnMappings.FindIndex(x => x.SourceName == name);
        }

        public bool GetBoolean(int i)
        {
            return (bool) GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte) GetValue(i);
        }

        public char GetChar(int i)
        {
            return (char) GetValue(i);
        }

        // ReSharper disable once TooManyArguments
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            byte[] columnValue = (byte[])GetValue(i);
            return ReadToBuffer(columnValue, fieldOffset, buffer, bufferOffset, length);
        }

        // ReSharper disable once TooManyArguments
        public long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            IList<char> columnValue = GetString(i).ToList();
            return ReadToBuffer(columnValue, fieldOffset, buffer, bufferOffset, length);
        }

        private static long ReadToBuffer<T>(IList<T> sourceValue, long fieldOffset, T[] buffer, int bufferOffset, int lengthToRead)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }

            long readCount = 0;

            for (int bufferIndex = bufferOffset; bufferIndex < bufferOffset + lengthToRead; bufferIndex++)
            {
                int columnValueIndex = bufferOffset - bufferIndex + (int)fieldOffset;
                if (columnValueIndex > sourceValue.Count)
                {
                    readCount++;
                    buffer[bufferIndex] = sourceValue[columnValueIndex];
                }
            }

            return readCount;
        }

        public Guid GetGuid(int i)
        {
            return (Guid) GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short) GetValue(i);
        }

        public int GetInt32(int i)
        {
            return (int) GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long) GetValue(i);
        }

        public float GetFloat(int i)
        {
            return (float) GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double) GetValue(i);
        }

        public string GetString(int i)
        {
            return (string) GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal) GetValue(i);
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime) GetValue(i);
        }

        public IDataReader GetData(int i)
        {
            throw new NotSupportedException();
        }

        public bool IsDBNull(int i)
        {
            return GetValue(i) == null;
        }

        public int FieldCount
        {
            get { return _columnMappings.Count; }
        }

        public object this[int i]
        {
            get { return GetValue(i); }
        }

        public object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        public void Close()
        {
            _iterator?.Dispose();
            _iterator = null;
        }

        public DataTable GetSchemaTable()
        {
            DataTable schemaTable = new DataTable();
            schemaTable.Columns.Add("ColumnName", typeof(string));
            schemaTable.Columns.Add("DataType", typeof(string));
            schemaTable.Columns.Add("AllowDBNull", typeof(bool));

            for (int i = 0; i < _columnMappings.Count; i++)
            {
                DataElementPairing mapping = _columnMappings[i];
                DataRow schemaRow = schemaTable.NewRow();
                schemaRow["ColumnName"] = mapping.SourceName;
                schemaRow["DataType"] = GetDataTypeName(i);
                schemaRow["AllowDBNull"] = true;
                schemaTable.Rows.Add(schemaRow);
            }

            return schemaTable;
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            return _iterator.MoveNext();
        }

        public int Depth
        {
            get { return 0; }
        }

        public bool IsClosed
        {
            get { return _iterator == null; }
        }

        public int RecordsAffected
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public void Dispose()
        {
            _iterator.Dispose();
        }
    }
}