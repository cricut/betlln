using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using DataTable = System.Data.DataTable;

namespace Betlln.Data.File
{
    //implementation guided by https://www.codeproject.com/tips/705470/read-and-write-excel-documents-using-oledb
    public class ExcelFileAdapter : IDataFileAdapter
    {
        private ExcelSheetEnumerator _plainData;

        public ExcelFileAdapter(string filePath)
        {
            FilePath = filePath;
        }

        private string FilePath { get; }
        private string SheetName { get; set; }

        public string CurrentSectionName
        {
            get
            {
                EnsureLoadedState();
                return SheetName;
            }
            set
            {
                // ReSharper disable once UseStringInterpolation
                SheetName = SectionNames.FirstOrDefault(x => x.Equals(string.Format("{0}$", value)));
            }
        }
        
        public void SelectSection(Func<IEnumerable<string>, string> sectionSelector)
        {
            SheetName = sectionSelector(SectionNames);
        }

        private OleDbConnection _connection;
        private OleDbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    //thanks to https://www.connectionstrings.com/ace-oledb-12-0/treating-data-as-text/
                    //HDR=NO pulls in the first row as data, IMEX=1 allow mixed-type columns (otherwise values can get excluded)
                    string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Extended Properties=\"Excel 12.0 XML; HDR=NO; IMEX=1;\";Data Source={FilePath}";
                    _connection = new OleDbConnection(connectionString);
                    _connection.Open();
                }
                return _connection;
            }
        }

        private IEnumerable<string> _sheetNames;
        public IEnumerable<string> SectionNames
        {
            get
            {
                if (_sheetNames == null)
                {
                    DataTable tablesTable = Connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                    if (tablesTable == null)
                    {
                        throw new DocumentFormatException($"{FilePath} does not contain tabular data.");
                    }

                    _sheetNames = tablesTable.Rows.Cast<DataRow>()
                            .Select(tableInfoRow => tableInfoRow["TABLE_NAME"].ToString().Trim('\''))
                            .Where(sheetName => sheetName.EndsWith("$"));
                }
                return _sheetNames;
            }
        }

        public IEnumerable<FileRow> PlainData
        {
            get
            {
                EnsureLoadedState();
                return _plainData;
            }
        }

        private void EnsureLoadedState()
        {
            if (string.IsNullOrWhiteSpace(SheetName))
            {
                SheetName = SectionNames.FirstOrDefault();
            }

            if (_plainData == null)
            {
                _plainData = new ExcelSheetEnumerator(Connection, SheetName);
            }
        }

        public void Dispose()
        {
            DisposeEnumerator();
            _connection?.Dispose();
        }

        private void DisposeEnumerator()
        {
            if (_plainData != null)
            {
                _plainData.Dispose();
                _plainData = null;
            }
        }

        private class ExcelSheetEnumerator : IEnumerable<FileRow>, IEnumerator<FileRow>
        {
            private readonly OleDbConnection _connection;
            private readonly string _sheetName;
            private OleDbCommand _command;
            private OleDbDataReader _reader;
            private uint RowNumber { get; set; }

            public ExcelSheetEnumerator(OleDbConnection connection, string sheetName)
            {
                _connection = connection;
                _sheetName = sheetName;
                Initialize();
            }

            private void Initialize()
            {
                _command = _connection.CreateCommand();
                _command.CommandText = $"SELECT * FROM [{_sheetName}]";
                _reader = _command.ExecuteReader();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<FileRow> GetEnumerator()
            {
                return this;
            }

            public bool MoveNext()
            {
                bool read = _reader.Read();
                if (read)
                {
                    RowNumber++;
                }
                return read;
            }

            public void Reset()
            {
                RowNumber = 0;
                Dispose();
                Initialize();
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public FileRow Current
            {
                get
                {
                    FileRow row = null;

                    if (RowNumber > 0)
                    {
                        row = new FileRow(string.Empty) {RowNumber = RowNumber};

                        for (uint columnNumber = 1; columnNumber <= _reader.FieldCount; columnNumber++)
                        {
                            object cellValue = GetCellValue(columnNumber);
                            row.Cells.Add(new DataCell(columnNumber, cellValue));
                        }
                    }

                    return row;
                }
            }

            private object GetCellValue(uint columnIndex)
            {
                object rawValue = _reader[(int) (columnIndex - 1)];
                if (rawValue == DBNull.Value)
                {
                    rawValue = null;
                }
                return rawValue;
            }

            public void Dispose()
            {
                if (_reader != null)
                {
                    _reader.Dispose();
                    _reader = null;
                }

                if (_command != null)
                {
                    _command.Dispose();
                    _command = null;
                }
            }
        }
    }
}