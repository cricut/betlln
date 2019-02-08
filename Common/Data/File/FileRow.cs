using System;
using System.Collections.Generic;

namespace Betlln.Data.File
{
    public class FileRow
    {
        public FileRow(object rawContent)
        {
            RawContent = rawContent;
            Cells = new List<DataCell>();
        }

        // (useful for debugging)
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private object RawContent { get; }

        public uint RowNumber { get; set; }
        public List<DataCell> Cells { get; }
        
        public string GetText(uint columnNumber)
        {
            DataCell cell = GetCell(columnNumber);
            return cell?.TextValue ?? string.Empty;
        }

        public object GetValue(uint columnNumber)
        {
            DataCell cell = GetCell(columnNumber);
            return cell?.Value;
        }

        private DataCell GetCell(uint columnNumber)
        {
            return Cells.Find(x => x.ColumnNumber == columnNumber);
        }
    }

    public static class FileRowExtensions
    {
        public static void AdvanceToRow(this IEnumerator<FileRow> fileData, uint rowNumber)
        {
            if (fileData.Current != null && fileData.Current.RowNumber > rowNumber)
            {
                throw new InvalidOperationException($"Row # {rowNumber} has already been passed.");
            }

            while ((fileData.Current?.RowNumber).GetValueOrDefault() < rowNumber)
            {
                fileData.MoveNext();
            }

            if (fileData.Current == null || fileData.Current.RowNumber != rowNumber)
            {
                throw new DocumentFormatException($"Row # {rowNumber} does not exist.");
            }
        }
    }
}