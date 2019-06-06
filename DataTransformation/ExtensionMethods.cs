using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Betlln.Data.Integration.Collections;

namespace Betlln.Data.Integration
{
    public static class ExtensionMethods
    {
        public static string ToFormattedString(this DateTime dateTime, string format)
        {
            return dateTime.ToString(format);
        }

        public static string ToPaddedString(this int value, string padding)
        {
            if (padding.Any(c => c != padding.First()))
            {
                throw new ArgumentException("Not consistent padding");
            }

            return value.ToString().PadLeft(padding.Length, padding.First());
        }

        public static ListOf<T> AsListOf<T>(this DataTable dataTable)
        {
            ListOf<T> list = new ListOf<T>();
            foreach (DataRow row in dataTable.Rows)
            {
                list.Add((T)row[0]);
            }
            return list;
        }

        public static DataRow FirstRow(this DataTable dataTable)
        {
            return dataTable.Rows[0];
        }

        public static DataTable ToSlimmedDataTable(this DataTable wideDataTable, params string[] columnNames)
        {
            List<DataElementPairing> columnMappings = new List<DataElementPairing>();
            foreach (string columnName in columnNames)
            {
                columnMappings.Add(new DataElementPairing(columnName, columnName, wideDataTable.Columns[columnName].DataType));
            }
            return wideDataTable.RepackageTable(columnMappings);
        }

        internal static DataTable RepackageTable(this DataTable sourceDataTable, List<DataElementPairing> newColumnMappings)
        {
            DataTable convertedDataTable = new DataTable();

            foreach (DataElementPairing columnMapping in newColumnMappings)
            {
                DataColumn sourceColumn = sourceDataTable.Columns[columnMapping.SourceName];
                if (sourceColumn == null)
                {
                    throw new DataException($"The column '{columnMapping.SourceName}' does not belong to the table.");
                }
                if (sourceColumn.DataType != columnMapping.ElementType)
                {
                    throw new InvalidCastException();
                }
                convertedDataTable.Columns.Add(new DataColumn(columnMapping.DestinationName, columnMapping.ElementType));
            }

            foreach (DataRow sourceRow in sourceDataTable.Rows)
            {
                DataRow convertedRow = convertedDataTable.NewRow();
                foreach (DataColumn destinationColumn in convertedDataTable.Columns)
                {
                    string sourceColumnName = newColumnMappings.Find(x => x.DestinationName == destinationColumn.ColumnName).SourceName;
                    convertedRow[destinationColumn] = sourceRow[sourceColumnName];
                }

                convertedDataTable.Rows.Add(convertedRow);
            }

            return convertedDataTable;
        }

        public static ListOf<string> GetColumnNames(this DataTable dataTable)
        {
            ListOf<string> columnNames = new ListOf<string>();
            foreach (DataColumn column in dataTable.Columns)
            {
                columnNames.Add(column.ColumnName);
            }
            return columnNames;
        }
    }
}