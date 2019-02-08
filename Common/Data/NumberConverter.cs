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
    }
}