using System;
using System.Globalization;
using System.Linq;

namespace Betlln.Data
{
    public static class NumberConverter
    {
        public static int? ConvertToInt(decimal? number)
        {
            if (number.HasValue)
            {
                return (int?)Math.Round(number.Value, 0);
            }

            return null;
        }

        internal static decimal? Parse(string nativeValue, NumberFormatInfo numberFormatInfo)
        {
            if (!string.IsNullOrWhiteSpace(nativeValue))
            {
                if (nativeValue.First() == '(' && nativeValue.Last() == ')')
                {
                    nativeValue = $"-{nativeValue.Substring(1, nativeValue.Length - 2)}";
                }
                nativeValue = nativeValue.Replace(numberFormatInfo.NumberGroupSeparator, string.Empty);

                try
                {
                    return decimal.Parse(nativeValue, NumberStyles.Float);
                }
                catch (FormatException formatException)
                {
                    throw new FormatException($"Could not parse \'{nativeValue}\'", formatException);
                }
            }

            return null;
        }

        public static decimal? ConvertObjectToNumber(object obj, NumberFormatInfo numberFormatInfo)
        {
            decimal? actualValue = null;

            if (obj == null)
            {
                actualValue = 0;
            }
            else if (obj is double)
            {
                actualValue = Convert.ToDecimal(obj);
            }
            else if (obj is string)
            {
                string stringValue = ((string) obj).Trim();
                actualValue =
                    IsNoneValue(stringValue)
                        ? 0
                        : Parse(obj.ToString(), numberFormatInfo);
            }

            return actualValue;
        }

        private static bool IsNoneValue(string stringValue)
        {
            return stringValue == "NA" || 
                   stringValue == string.Empty || 
                   stringValue == LongDash;
        }

        private static string LongDash
        {
            get { return ((char) 8212).ToString(); }
        }

        public static bool IsNumber(this string rawValue)
        {
            return TryParseDecimal(rawValue, out _);
        }

        public static int ToInt32(this string rawValue)
        {
            return (int) ToDecimal(rawValue);
        }

        public static decimal ToDecimal(this string rawValue)
        {
            decimal value;
            if (TryParseDecimal(rawValue, out value))
            {
                return value;
            }
            throw new ArgumentException();
        }

        private static bool TryParseDecimal(string rawValue, out decimal decimalValue)
        {
            rawValue = GetUnformattedNumber(rawValue);

            decimal tempValue;
            bool couldParse = decimal.TryParse(rawValue, out tempValue);
            decimalValue = couldParse ? tempValue : default(decimal);
            return couldParse;
        }

        private static string GetUnformattedNumber(string formattedValue)
        {
            return formattedValue
                .Replace(NumberFormat.NumberGroupSeparator, string.Empty)
                .Replace(NumberFormat.PercentSymbol, string.Empty)
                .Replace(NumberFormat.CurrencySymbol, string.Empty);
        }

        private static NumberFormatInfo NumberFormat
        {
            get { return CultureInfo.GetCultureInfo("en-US").NumberFormat; }
        }
    }
}