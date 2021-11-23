using System.IO;
using System.Text.RegularExpressions;

namespace Betlln.IO
{
    public class LocalFileSystem : FileSystem
    {
        public static bool IsValidPath(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) && 
                   Regex.IsMatch(filePath.Trim(), @"^([a-z]:\\)?([^|/:]+)?$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
        
        public static string GetTempFile()
        {
            return Path.GetTempFileName();
        }
    }
}