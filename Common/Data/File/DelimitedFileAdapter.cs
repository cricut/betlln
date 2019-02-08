using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic.FileIO;

namespace Betlln.Data.File
{
    public class DelimitedFileAdapter : IDataFileAdapter
    {
        private Enumerator _enumerator;

        public DelimitedFileAdapter(string filePath, char delimiter)
        {
            FilePath = filePath;
            Delimiter = delimiter;
        }

        private string FilePath { get; }
        private char Delimiter { get; }

        public string CurrentSectionName
        {
            get { return "default"; }
            set { }
        }
        
        public void SelectSection(Func<IEnumerable<string>, string> sectionSelector)
        {
        }

        public IEnumerable<FileRow> PlainData
        {
            get
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (_enumerator == null)
                {
                    _enumerator = new Enumerator(FilePath, Delimiter);
                }
                return _enumerator;
            }
        }

        private class Enumerator : IEnumerable<FileRow>, IEnumerator<FileRow>
        {
            private TextFieldParser _parser;
            private string[] _rowData;

            public Enumerator(string filePath, char delimiter)
            {
                FilePath = filePath;
                Delimiter = delimiter;
                Initialize();
            }

            private void Initialize()
            {
                _parser = new TextFieldParser(FilePath);
                _parser.TextFieldType = FieldType.Delimited;
                _parser.SetDelimiters(Delimiter.ToString());
                _rowData = null;
            }

            private string FilePath { get; }
            private char Delimiter { get; }

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
                if (_parser.EndOfData)
                {
                    _rowData = null;
                    return false;
                }

                _rowData = _parser.ReadFields();
                return true;
            }

            public void Reset()
            {
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
                    FileRow gridRow = null;

                    if (_rowData != null)
                    {
                        gridRow = new FileRow(string.Join(Delimiter.ToString(), _rowData));
                        gridRow.RowNumber = (uint) _parser.LineNumber - 1;  //starts at line 2 for some reason
                        for (int c = 0; c < _rowData.Length; c++)
                        {
                            string value = _rowData[c];
                            value = SanitizeValue(value);
                            DataCell cell = new DataCell((uint) (c + 1), value);
                            gridRow.Cells.Add(cell);
                        }
                    }

                    return gridRow;
                }
            }

            public void Dispose()
            {
                _parser.Dispose();
                _rowData = null;
            }
        }

        internal static string SanitizeValue(string value)
        {
            while ("\"=".Contains(value.FirstOrDefault()))
            {
                value = value.Trim('"');
                if (value.FirstOrDefault() == '=')
                {
                    value = value.Substring(1);
                }
            }
            value = value.Trim();
            return value;
        }

        public void Dispose()
        {
            _enumerator.Dispose();
            _enumerator = null;
        }
    }
}