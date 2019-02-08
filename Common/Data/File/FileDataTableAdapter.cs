using System;
using System.Collections;
using System.Collections.Generic;

namespace Betlln.Data.File
{
    public class FileDataTableAdapter : IEnumerable<FileDataRow>
    {
        private readonly Enumerator _enumerator;

        public FileDataTableAdapter(IEnumerator<FileRow> source)
        {
            _enumerator = new Enumerator(source);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<FileDataRow> GetEnumerator()
        {
            return _enumerator;
        }

        private class Enumerator : IEnumerator<FileDataRow>
        {
            private readonly IEnumerator<FileRow> _source;
            private uint? _firstRowNumber;
            private Dictionary<uint, string> _columnMappings;

            public Enumerator(IEnumerator<FileRow> source)
            {
                _source = source;
                HandleFirstRow();
            }

            public bool MoveNext()
            {
                if (_source.MoveNext())
                {
                    HandleFirstRow();
                    return true;
                }

                return false;
            }

            private void HandleFirstRow()
            {
                if (!_firstRowNumber.HasValue && _source.Current != null)
                {
                    _firstRowNumber = _source.Current.RowNumber;
                    MapColumns();
                }
            }

            public void Reset()
            {
                _source.Reset();

                while (_source.Current == null || _source.Current.RowNumber < _firstRowNumber)
                {
                    _source.MoveNext();
                }
                
                MapColumns();
            }

            private void MapColumns()
            {
                if (_source.Current == null)
                {
                    throw new InvalidOperationException("Enumerator must be at a valid row.");
                }

                _columnMappings = new Dictionary<uint, string>();
                foreach (DataCell cell in _source.Current.Cells)
                {
                    object cellValue = cell.Value;
                    string columnName = cellValue != null
                        ? SanitizeHeaderValue(cellValue.ToString())
                        : null;
                    _columnMappings.Add(cell.ColumnNumber, columnName);
                }
            }

            public FileDataRow Current
            {
                get
                {
                    FileDataRow row = null;

                    if (_source.Current != null)
                    {
                        row = new FileDataRow();
                        row.FileRowNumber = _source.Current.RowNumber;

                        foreach (KeyValuePair<uint, string> columnMapping in _columnMappings)
                        {
                            uint columnNumber = columnMapping.Key;
                            string columnName = columnMapping.Value;
                            if (!string.IsNullOrWhiteSpace(columnName))
                            {
                                object value = _source.Current.GetValue(columnNumber);
                                row.Values.Add(columnName, value);
                            }
                        }
                    }

                    return row;
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
                _source.Dispose();
            }
        }

        internal static string SanitizeHeaderValue(string rawValue)
        {
            string sanitizedCellValue =
                rawValue
                    .Trim()
                    .Replace("\r", string.Empty)
                    .Replace("\n", " ");

            while (sanitizedCellValue.Contains("  "))
            {
                sanitizedCellValue = sanitizedCellValue.Replace("  ", " ");
            }

            return sanitizedCellValue;
        }
    }
}