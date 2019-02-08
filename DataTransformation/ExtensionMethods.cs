using System;
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
            DataTable slimmedDataTable = new DataTable();

            foreach (string columnName in columnNames)
            {
                DataColumn sourceColumn = wideDataTable.Columns[columnName];
                slimmedDataTable.Columns.Add(new DataColumn(sourceColumn.ColumnName, sourceColumn.DataType));
            }

            foreach (DataRow sourceRow in wideDataTable.Rows)
            {
                DataRow row = slimmedDataTable.NewRow();
                foreach (DataColumn column in slimmedDataTable.Columns)
                {
                    row[column] = sourceRow[column.ColumnName];
                }
                slimmedDataTable.Rows.Add(row);
            }

            return slimmedDataTable;
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