using System;
using System.Data;
using Betlln.Data.Integration.Core;
using ClosedXML.Excel;

namespace Betlln.Data.Integration.Office
{
    public class ExcelDestination : Task
    {
        internal ExcelDestination()
        {
        }

        public DataSource DataSource { get; set; }

        private string _outputFileName;
        public string OutputFileName
        {
            get
            {
                return _outputFileName;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException();
                }

                string extension = System.IO.Path.GetExtension(value);
                if (extension.StartsWith("."))
                {
                    extension = extension.Substring(1);
                }
                if (extension.ToLower() != "xlsx")
                {
                    throw new NotSupportedException("File formats other than XLSX (OpenXML) are not supported.");
                }

                _outputFileName = value;
            }
        }

        public string DestinationSheetName { get; set; }

        protected override void ExecuteTasks()
        {
            DataTable dataTable = DataSource.GetResults();

            using (XLWorkbook workbook = new XLWorkbook())
            {
                using (IXLWorksheet worksheet = workbook.Worksheets.Add(DestinationSheetName))
                {
                    worksheet.Cell(1, 1).InsertTable(dataTable);
                    workbook.SaveAs(OutputFileName);
                }
            }
        }
    }
}