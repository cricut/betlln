﻿using System;
using System.Data;
using System.Linq;
using Betlln.Data.Integration.Core;
using ClosedXML.Excel;

namespace Betlln.Data.Integration.Office
{
    public class ExcelDestination : Task
    {
        private const string ChangeTrackingCustomPropertyName = "dts_edit_status";

        internal ExcelDestination()
        {
        }

        public DataFeed DataSource { get; set; }

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
            
            using (XLWorkbook workbook = GetWorkbook())
            {
                using (IXLWorksheet worksheet = workbook.Worksheets.Add(DestinationSheetName))
                {
                    int firstColumnNumber = 1;
                    IXLCell topLeftCell = worksheet.Cell(firstColumnNumber, 1);

                    IXLTable table = topLeftCell.InsertTable(dataTable);
                    IXLColumns tableColumnsReference = worksheet.Columns(firstColumnNumber, table.ColumnCount());
                    tableColumnsReference.AdjustToContents();

                    IXLCustomProperty changeTrackingProperty = workbook.CustomProperties.CustomProperty(ChangeTrackingCustomPropertyName);
                    if (changeTrackingProperty.Value.ToString() == EditStatus.Creating.ToString())
                    {
                        workbook.CustomProperties.Delete(ChangeTrackingCustomPropertyName);
                        workbook.SaveAs(OutputFileName);
                    }
                    else
                    {
                        workbook.CustomProperties.Delete(ChangeTrackingCustomPropertyName);
                        workbook.Save();
                    }
                }
            }
        }

        private XLWorkbook GetWorkbook()
        {
            if (!System.IO.File.Exists(OutputFileName))
            {
                XLWorkbook workbook = new XLWorkbook();
                workbook.CustomProperties.Add(ChangeTrackingCustomPropertyName, EditStatus.Creating);
                return workbook;
            }
            else
            {
                XLWorkbook workbook = new XLWorkbook(OutputFileName);

                IXLCustomProperty xlCustomProperty = workbook.CustomProperties.FirstOrDefault(x => x.Name == ChangeTrackingCustomPropertyName);
                if (xlCustomProperty != null)
                {
                    xlCustomProperty.Value = EditStatus.Editing;
                }
                else
                {
                    workbook.CustomProperties.Add(ChangeTrackingCustomPropertyName, EditStatus.Editing);
                }

                return workbook;
            }
        }

        private enum EditStatus
        {
            Creating,
            Editing
        }
    }
}