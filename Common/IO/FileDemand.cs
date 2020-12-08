using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Betlln.IO
{
    public class FileDemand
    {
        private FileDemand(string content, MatchKind matchKind)
        {
            MatchContent = content;
            MatchLevel = matchKind;
        }

        private string MatchContent { get; }
        private MatchKind MatchLevel { get; }

        public static FileDemand Exactly(string filePath)
        {
            return new FileDemand(filePath, MatchKind.FullPath);
        }

        public static FileDemand FromName(string fileName)
        {
            return new FileDemand(fileName, MatchKind.Name);
        }

        public static FileDemand FromNamePrefix(string fileNamePrefix)
        {
            return new FileDemand(fileNamePrefix, MatchKind.Prefix);
        }

        public static FileDemand FromPattern(string fileNamePattern)
        {
            return new FileDemand(fileNamePattern, MatchKind.Pattern);
        }

        public static FileDemand FromNameSuffix(string fileNameSuffix)
        {
            return new FileDemand(fileNameSuffix, MatchKind.Suffix);
        }

        public static FileDemand FromNameContaining(string fileNamePortion)
        {
            return new FileDemand(fileNamePortion, MatchKind.Contains);
        }

        /// <summary>
        /// Either the file name or the full path
        /// </summary>
        public string ImplicitFileName
        {
            get
            {
                switch (MatchLevel)
                {
                    case MatchKind.FullPath:
                    case MatchKind.Name:
                        return MatchContent;
                    default:
                        return null;
                }
            }
        }

        public bool IsSatisfiedBy(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            string fileName = Path.GetFileName(filePath);

            switch (MatchLevel)
            {
                case MatchKind.FullPath:
                    return filePath.Equals(MatchContent, StringComparison.InvariantCultureIgnoreCase);
                case MatchKind.Name:
                    return MatchContent.Equals(fileName, StringComparison.InvariantCultureIgnoreCase);
                case MatchKind.Prefix:
                    return fileName.StartsWith(MatchContent, StringComparison.InvariantCultureIgnoreCase);
                case MatchKind.Contains:
                    return fileName.ToLower().Contains(MatchContent.ToLower());
                case MatchKind.Suffix:
                    return fileName.EndsWith(MatchContent, StringComparison.InvariantCultureIgnoreCase);
                case MatchKind.Pattern:
                    return ToRegex().IsMatch(fileName);
                default:
                    throw new NotSupportedException();
            }
        }

        public override string ToString()
        {
            switch (MatchLevel)
            {
                case MatchKind.FullPath:
                case MatchKind.Name:
                    return $"a file named '{MatchContent}'";
                case MatchKind.Prefix:
                    return $"a file starting with '{MatchContent}'";
                case MatchKind.Contains:
                    return $"a file with '{MatchContent}' in the name";
                case MatchKind.Suffix:
                    return $"a file ending with '{MatchContent}'";
                case MatchKind.Pattern:
                    return $"a file matching the pattern '{GetPrintablePattern()}'";
                default:
                    throw new NotSupportedException();
            }
        }

        public Regex ToRegex()
        {
            switch (MatchLevel)
            {
                case MatchKind.Pattern:
                    return new Regex(MatchContent, RegexOptions.IgnoreCase);
                default:
                    return null;
            }
        }

        private string GetPrintablePattern()
        {
            string prettyPattern = MatchContent
                                    .Replace(@"\-", "-")
                                    .Replace(@"\.",".")
                                    .Replace(@"[0-9]", "#");

            if (prettyPattern.StartsWith("^"))
            {
                prettyPattern = prettyPattern.Substring(1);
            }

            if (prettyPattern.EndsWith("$"))
            {
                prettyPattern = prettyPattern.Substring(0, prettyPattern.Length - 1);
            }

            const string matchAnyOrNone = ".*?";
            const string matchAnyAtLeastOnce = ".+?";
            if (prettyPattern.Contains(matchAnyAtLeastOnce) || prettyPattern.Contains(matchAnyOrNone))
            {
                prettyPattern = prettyPattern.Replace(matchAnyAtLeastOnce, "*").Replace(matchAnyOrNone, "*");
            }

            prettyPattern = Regex.Replace(prettyPattern, @"\\s[*]?", " ");
            prettyPattern = Regex.Replace(prettyPattern, @"\(.+?\|.+?\)", "*");
            prettyPattern = Regex.Replace(prettyPattern, @"\\d\{(\d,)?\d\}", "*");
            prettyPattern = prettyPattern.Replace("?", "");

            prettyPattern = RemoveCharacterWhenNotEscaped(prettyPattern, "(");
            prettyPattern = RemoveCharacterWhenNotEscaped(prettyPattern, ")");

            return prettyPattern;
        }

        private static string RemoveCharacterWhenNotEscaped(string prettyPattern, string characterToRemove)
        {
            int indexOf = prettyPattern.IndexOf(characterToRemove, StringComparison.InvariantCultureIgnoreCase);
            while (indexOf > -1)
            {
                if (indexOf != 0)
                {
                    if (prettyPattern[indexOf - 1] != '\\')
                    {
                        prettyPattern = prettyPattern.Remove(indexOf, 1);
                    }
                }

                indexOf = prettyPattern.IndexOf(characterToRemove, StringComparison.InvariantCultureIgnoreCase);
            }

            return prettyPattern;
        }

        private enum MatchKind
        {
            FullPath,
            Name,
            Prefix,
            Contains,
            Suffix,
            Pattern
        }
    }
}