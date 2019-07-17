using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Betlln.Data.Integration.Core;
using Betlln.Logging;
using Betlln.Spreadsheets;
using ClosedXML.Excel;

namespace Betlln.Data.Integration.Office
{
    public class ExcelWorkbookConverter : StreamConverter
    {
        internal ExcelWorkbookConverter()
        {
            Sheets = new List<ExcelSheetDirective>();
            Dts.Notify.Log($"Started {GetType().Name}", LogEventType.Debug);
        }

        public List<ExcelSheetDirective> Sheets { get; }

        private string _outputName;
        public override string OutputName
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

        protected override void WriteToStream(Stream outputStream)
        {
            using (XLWorkbook workbook = new XLWorkbook())
            {
                foreach (ExcelSheetDirective sheet in Sheets)
                {
                    DataTable dataTable = sheet.DataSource.GetResults();
                    if (ShouldWriteToSheet(sheet, dataTable))
                    {
                        WriteToSheet(workbook, sheet, dataTable);
                    }
                }

                if (workbook.Worksheets.Any())
                {
                    workbook.SaveAs(outputStream);
                }
            }
        }

        private bool ShouldWriteToSheet(ExcelSheetDirective sheet, DataTable dataTable)
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

        private void WriteToSheet(XLWorkbook workbook, ExcelSheetDirective sheetInfo, DataTable dataTable)
        {
            using (IXLWorksheet worksheet = workbook.Worksheets.Add(sheetInfo.DestinationSheetName))
            {
                IXLTable table = CreateFormattedTable(sheetInfo, worksheet, dataTable);

                foreach (string columnLetter in sheetInfo.CustomColumnLetters)
                {
                    int columnIndex = (int) (CellReference.GetColumnNumberFromLetter(columnLetter) - 1);
                    string excelColumnName = dataTable.Columns[columnIndex].ColumnName;

                    ExcelColumn columnSettings = sheetInfo.Column(columnLetter);
                    if (columnSettings.TotalFunction.HasValue)
                    {
                        table.ShowTotalsRow = true;
                        table.Field(excelColumnName).TotalsRowFunction = columnSettings.TotalFunction.Value;
                    }
                }
            }
        }

        private IXLTable CreateFormattedTable(ExcelSheetDirective sheetInfo, IXLWorksheet worksheet, DataTable dataTable)
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

            return table;
        }
    }
}