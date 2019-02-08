using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Betlln.Collections;

namespace Betlln.Data.File
{
    public class FileDataRow
    {
        public FileDataRow()
        {
            Values = new KeyValueBag<object>();
        }

        public uint FileRowNumber { get; set; }
        public KeyValueBag<object> Values { get; }

        public bool IsBlank
        {
            get { return Values.Values.All(IsBlankValue); }
        }

        private static bool IsBlankValue(object value)
        {
            string s = value as string;
            return s != null
                ? string.IsNullOrWhiteSpace(s)
                : value == null;
        }

        public decimal? GetSumOfColumns(IEnumerable<string> columnNames, NumberFormatInfo numberFormatInfo)
        {
            decimal? grandTotal = null;
            foreach (string columnName in columnNames)
            {
                decimal? columnValue = GetNumber(columnName, numberFormatInfo);
                if (columnValue.HasValue)
                {
                    grandTotal = grandTotal.GetValueOrDefault() + columnValue.Value;
                }
            }

            return grandTotal;
        }

        public decimal? GetNumber(string columnName, NumberFormatInfo numberFormatInfo)
        {
            decimal? actualValue = null;

            if (!string.IsNullOrWhiteSpace(columnName))
            {
                object nativeValue = Values[columnName];
                actualValue = NumberConverter.ConvertObjectToNumber(nativeValue, numberFormatInfo);

                if (!actualValue.HasValue)
                {
                    throw new DocumentFormatException($"Row # {FileRowNumber}, Column {columnName} - value is not recognized.");
                }
            }

            return actualValue;
        }
    }
}