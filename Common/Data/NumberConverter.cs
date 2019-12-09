using System;
using System.Globalization;
using System.Linq;

namespace Betlln.Data
{
    public static class NumberConverter
    {
        public static int? RoundToInt32(this decimal? number)
        {
            #pragma warning disable 618
            return ConvertToInt(number);
            #pragma warning restore 618
        }

        [Obsolete("Use " + nameof(RoundToInt32) + " instead.")]
        public static int? ConvertToInt(decimal? number)
        {
            if (number.HasValue)
            {
                return (int?)Math.Round(number.Value, 0);
            }

            return null;
        }

        public static int ToInt32(this string rawValue)
        {
            return (int) ToDecimal(rawValue);
        }

        internal static decimal? Parse(string nativeValue, NumberFormatInfo numberFormatInfo)
        {
            if (!string.IsNullOrWhiteSpace(nativeValue))
            {
                if (nativeValue.First() == '(' && nativeValue.Last() == ')')
                {
                    nativeValue = $"-{nativeValue.Substring(1, nativeValue.Length - 2)}";
                }
                nativeValue = GetUnformattedNumber(nativeValue, numberFormatInfo);

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

        private static bool TryParseDecimal(string rawValue, NumberFormatInfo numberFormatInfo, out decimal decimalValue)
        {
            if (rawValue == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                decimal? parseResult = Parse(rawValue, numberFormatInfo);
                if(parseResult == null)
                {
                    throw new ArgumentException();
                }

                decimalValue = parseResult.GetValueOrDefault();
                return true;
            }
            catch
            {
                decimalValue = default(decimal);
                return false;
            }
        }

        private static string GetUnformattedNumber(string formattedValue, NumberFormatInfo numberFormatInfo)
        {
            return formattedValue
                .Replace(numberFormatInfo.NumberGroupSeparator, string.Empty)
                .Replace(numberFormatInfo.PercentSymbol, string.Empty)
                .Replace(numberFormatInfo.CurrencySymbol, string.Empty);
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
            return TryParseDecimal(rawValue, NumberFormatInfo.CurrentInfo, out _);
        }

        public static decimal ToDecimal(this string rawValue)
        {
            decimal value;
            if (TryParseDecimal(rawValue, NumberFormatInfo.CurrentInfo, out value))
            {
                return value;
            }
            throw new ArgumentException();
        }
    }
}