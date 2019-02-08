using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Betlln
{
    public static class SystemExtensions
    {
        public const int DaysPerWeek = 7;
        public const ushort SecondsPerMinute = 60;

        public static IEnumerable<T> ToEnumerable<T>(this T singleItem)
        {
            return new List<T> { singleItem };
        }

        public static Uri Resolve(this Uri currentUrl, string newRelativeUrl)
        {
            string newPath;

            if (newRelativeUrl.StartsWith("/"))
            {
                string portSection = currentUrl.IsDefaultPort ? string.Empty : $":{currentUrl.Port}";
                newPath = $"{currentUrl.Scheme}://{currentUrl.Host}{portSection}{newRelativeUrl}";
            }
            else
            {
                string[] urlParts = currentUrl.ToString().Split('/');
                urlParts[urlParts.Length - 1] = newRelativeUrl;
                newPath = string.Join("/", urlParts);
            }
            
            return ToUri(newPath);
        }

        public static Uri ToUri(this string url)
        {
            return new Uri(url);
        }

        public static DateTime AddWeek(this DateTime date)
        {
            return date.AddWeeks(1);
        }

        public static DateTime AddWeeks(this DateTime date, int weeks)
        {
            return date.AddDays(DaysPerWeek * weeks);
        }

        public static DateTime GetWeekStart(this DateTime date)
        {
            while (date.DayOfWeek != DayOfWeek.Sunday)
            {
                date = date.AddDays(-1);
            }
            return date.Date;
        }

        public static DateTime GetNextMonthStart(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1).AddMonths(1);
        }

        public static string NormalizeSpaces(this string s)
        {
            return s.Replace((char) 160, (char)32);
        }

        public static string GetFirstLine(this string content)
        {
            return content
                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
        }

        public static DateTime GetWeekEnd(this DateTime weekStart)
        {
            return weekStart.AddWeek().AddSeconds(-1);
        }

        public static string GetFileExtension(string filePath)
        {
            string extension = Path.GetExtension(filePath) ?? string.Empty;
            if (extension.FirstOrDefault() == '.')
            {
                extension = extension.Substring(1);
            }
            return extension.ToLower();
        }

        public static void RemoveLast(this StringBuilder sb, int length)
        {
            sb.Remove(sb.Length - length, length);
        }

        public static bool CanParseToLong(this string s)
        {
            long value;
            return long.TryParse(s, out value);
        }

        public static DateTime DateTimeMin(DateTime a, DateTime b)
        {
            if (a < b)
            {
                return a;
            }
            return b < a ? b : a;
        }

        // ReSharper disable PossibleMultipleEnumeration

        public static int? SumNullable<TObject>(this IEnumerable<TObject> list, Func<TObject, int?> selector)
        {
            if (list.All(x => !selector(x).HasValue))
            {
                return null;
            }

            return list.Sum(selector);
        }

        public static decimal? SumNullable<TObject>(this IEnumerable<TObject> list, Func<TObject, decimal?> selector)
        {
            if (list.All(x => !selector(x).HasValue))
            {
                return null;
            }

            return list.Sum(selector);
        }

        // ReSharper restore PossibleMultipleEnumeration
    }
}