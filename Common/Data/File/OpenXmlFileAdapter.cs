using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Betlln.Spreadsheets;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Betlln.Data.File
{
    //thanks to http://dotnetthoughts.net/read-excel-as-datatable-using-openxml-and-c/
    public class OpenXmlFileAdapter : IDataFileAdapter
    {
        private SpreadsheetDocument _document;
        private Enumerator _enumerator;
        private string _sheetId;
        private string _sheetName;

        public OpenXmlFileAdapter(string filePath)
        {
            FilePath = filePath;
            OpenFile();
        }

        private void OpenFile()
        {
            if (_document != null)
            {
                throw new InvalidOperationException();
            }

            _document = SpreadsheetDocument.Open(FilePath, isEditable: false);

            SelectSheet(selector: IsSheetVisible);
        }
        
        private static bool IsSheetVisible(Sheet sheet)
        {
            return (sheet.State?.Value).GetValueOrDefault(SheetStateValues.Visible) == SheetStateValues.Visible;
        }

        private string FilePath { get; }
        
        private IEnumerable<Sheet> Sheets
        {
            get
            {
                EnsureLoadedState();
                return _document.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
            }
        }

        public string CurrentSectionName
        {
            get
            {
                EnsureLoadedState();
                return _sheetName;
            }
            set
            {
                string sectionName = value;
                if (string.IsNullOrWhiteSpace(sectionName))
                {
                    throw new ArgumentNullException(nameof(sectionName), @"Cannot select a null/blank sheet.");
                }

                SelectSheet(sheet => CompareStrings(sheet.Name, sectionName));
            }
        }

        private void SelectSheet(Func<Sheet, bool> selector)
        {
            Sheet firstSheet = Sheets.First(selector);
            _sheetId = firstSheet.Id;
            _sheetName = firstSheet.Name.Value;
        }

        private static bool CompareStrings(StringValue stringValue, string compareValue)
        {
            string actualValue = stringValue?.Value ?? string.Empty;
            return actualValue.Equals(compareValue ?? string.Empty, StringComparison.InvariantCultureIgnoreCase);
        }

        public void SelectSection(Func<IEnumerable<string>, string> sectionSelector)
        {
            IEnumerable<string> sheetNames = Sheets.Select(x => x.Name.Value);
            string sheetName = sectionSelector(sheetNames);
            CurrentSectionName = sheetName;
        }

        public IEnumerable<FileRow> PlainData
        {
            get
            {
                if (_enumerator == null)
                {
                    EnsureLoadedState();
                    WorksheetPart worksheetPart = (WorksheetPart)_document.WorkbookPart.GetPartById(_sheetId);
                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                    SharedStringTablePart stringTablePart = _document.WorkbookPart.SharedStringTablePart;
                    _enumerator = new Enumerator(sheetData, stringTablePart);
                }
                return _enumerator;
            }
        }

        private void EnsureLoadedState()
        {
            if (_document == null)
            {
                OpenFile();
            }
        }

        private class Enumerator : IEnumerable<FileRow>, IEnumerator<FileRow>
        {
            private readonly SharedStringTablePart _stringTablePart;
            private readonly IEnumerator<Row> _rowEnumerator;

            // ReSharper disable once SuggestBaseTypeForParameter
            public Enumerator(SheetData sheetData, SharedStringTablePart stringTablePart)
            {
                _stringTablePart = stringTablePart;
                _rowEnumerator = sheetData.Descendants<Row>().GetEnumerator();
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
                return _rowEnumerator.MoveNext();
            }

            public void Reset()
            {
                _rowEnumerator.Reset();
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
            
            public FileRow Current
            {
                get
                {
                    FileRow fileRow = null;

                    if (_rowEnumerator.Current != null)
                    {
                        uint lastColumnNumber = 0;
                        Row row = _rowEnumerator.Current;

                        fileRow = new FileRow(row);
                        
                        List<Cell> rowCells = row.Descendants<Cell>().ToList();
                        for (int i = 0; i < rowCells.Count; i++)
                        {
                            Cell cell = rowCells.ElementAt(i);

                            string cellAddress = cell.CellReference.Value;
                            CellReference reference = CellReference.ParseCellReference(cellAddress);
                            fileRow.RowNumber = reference.RowNumber;
                            uint columnNumber = reference.ColumnNumber;

                            string cellValue = GetCellValue(_stringTablePart, cell);

                            for (uint j = lastColumnNumber + 1; j < columnNumber; j++)
                            {
                                fileRow.Cells.Add(new DataCell(j, null));
                            }
                            fileRow.Cells.Add(new DataCell(columnNumber, cellValue));
                            
                            lastColumnNumber = columnNumber;
                        }
                    }

                    return fileRow;
                }
            }

            private static string GetCellValue(SharedStringTablePart stringTablePart, Cell cell)
            {
                string cellValue;
                if (cell.CellFormula != null)
                {
                    cellValue = cell.CellValue.Text;
                    Debug.Assert(!(cell.CellFormula.Text == "" && cell.InnerText != cell.CellValue.Text));
                }
                else if (IsSharedString(cell))
                {
                    cellValue = stringTablePart.SharedStringTable.ChildElements[int.Parse(cell.CellValue.InnerXml)].InnerText;
                }
                else
                {
                    cellValue = cell.InnerText;
                }

                // ReSharper disable once StringLiteralTypo
                Debug.Assert(!cellValue.StartsWith("IFERROR"));

                return UnMirrorValue(cellValue);
            }

            // ReSharper disable once SuggestBaseTypeForParameter
            private static bool IsSharedString(Cell cell)
            {
                return cell.DataType != null && cell.DataType.Value == CellValues.SharedString;
            }

            public void Dispose()
            {
                _rowEnumerator.Dispose();
            }
        }

        internal static string UnMirrorValue(string rawValue)
        {
            if (rawValue != null && rawValue.Contains(" ") && rawValue.EndsWith(" "))
            {
                int unMirroredLength = rawValue.Length / 2;
                string firstHalf = rawValue.Substring(0, unMirroredLength);
                if (firstHalf == rawValue.Substring(unMirroredLength))
                {
                    return firstHalf;
                }
            }
            
            return rawValue;
        }

        public void Dispose()
        {
            _document?.Dispose();
            _document = null;
        }
    }
}