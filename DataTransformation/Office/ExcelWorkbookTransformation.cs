using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Betlln.Data.Integration.Core;
using ClosedXML.Excel;

namespace Betlln.Data.Integration.Office
{
    public class ExcelWorkbookTransformation
    {
        public ExcelWorkbookTransformation()
        {
            Sheets = new List<ExcelSheetTransformation>();
        }

        public List<ExcelSheetTransformation> Sheets { get; }

        private string _outputName;
        public string OutputName
        {
            get
            {
                return _outputName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException();
                }

                string extension = Path.GetExtension(value);
                if (extension.StartsWith("."))
                {
                    extension = extension.Substring(1);
                }
                if (extension.ToLower() != "xlsx")
                {
                    throw new NotSupportedException("File formats other than XLSX (OpenXML) are not supported.");
                }

                _outputName = value;
            }
        }

        private NamedStream _output;
        public NamedStream Output
        {
            get
            {
                if (_output == null)
                {
                    _output = new NamedStream();
                    _output.Name = OutputName;
                    _output.Content = new MemoryStream();

                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        foreach (ExcelSheetTransformation sheet in Sheets)
                        {
                            DataTable dataTable = sheet.DataSource.GetResults();
                            if (ShouldWriteToSheet(sheet, dataTable))
                            {
                                WriteToSheet(workbook, sheet, dataTable);
                            }
                        }

                        workbook.SaveAs(_output.Content);
                    }

                    _output.Content.Position = 0;
                }

                return _output;
            }
        }

        private bool ShouldWriteToSheet(ExcelSheetTransformation sheet, DataTable dataTable)
        {
            if (dataTable.Columns.Count == 0)
            {
                return false;
            }

            if (dataTable.Rows.Count == 0)
            {
                return sheet.WriteEmptyTable;
            }

            return true;
        }

        private void WriteToSheet(XLWorkbook workbook, ExcelSheetTransformation sheetInfo, DataTable dataTable)
        {
            using (IXLWorksheet worksheet = workbook.Worksheets.Add(sheetInfo.DestinationSheetName))
            {
                CreateFormattedTable(sheetInfo, worksheet, dataTable);
            }
        }

        private void CreateFormattedTable(ExcelSheetTransformation sheetInfo, IXLWorksheet worksheet, DataTable dataTable)
        {
            IXLCell topLeftCell = worksheet.Cell(1, 1);

            IXLTable table = topLeftCell.InsertTable(dataTable);
            foreach (string columnLetter in sheetInfo.CustomColumnLetters)
            {
                IXLRangeColumns targetColumn = table.Columns(columnLetter);
                ExcelColumn columnSettings = sheetInfo.Column(columnLetter);
                targetColumn.Style.Alignment.SetHorizontal(columnSettings.HorizontalAlignment);
                if (!string.IsNullOrWhiteSpace(columnSettings.Format))
                {
                    targetColumn.Style.NumberFormat.Format = columnSettings.Format;
                }
            }

            IXLColumns tableColumnsReference = worksheet.Columns(1, table.ColumnCount());
            tableColumnsReference.AdjustToContents();
        }
    }
}