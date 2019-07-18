using System;
using System.Text.RegularExpressions;

namespace Betlln.Spreadsheets
{
    public class CellReference
    {
        private const int LettersInAlphabet = 26;

        public CellReference(uint rowNumber, uint columnNumber)
        {
            RowNumber = rowNumber;
            ColumnLetter = GetColumnLetterFromNumber(columnNumber);
        }

        private CellReference(uint rowNumber, string columnLetter)
        {
            RowNumber = rowNumber;
            ColumnLetter = columnLetter;
        }

        public uint RowNumber { get; }
        public string ColumnLetter { get; }

        public uint ColumnNumber
        {
            get { return GetColumnNumberFromLetter(ColumnLetter); }
        }

        public override string ToString()
        {
            return ColumnLetter + RowNumber;
        }

        public static uint GetColumnNumberFromLetter(string columnLetter)
        {
            if (columnLetter == null)
            {
                throw new ArgumentNullException(nameof(columnLetter));
            }

            columnLetter = columnLetter.Trim().ToUpper();

            uint value = 0;
            for (int i = columnLetter.Length - 1; i >= 0; i--)
            {
                uint place = (uint)(columnLetter.Length - i);
                uint placeValue = (uint)Math.Pow(LettersInAlphabet, place - 1);
                char digit = columnLetter[i];
                uint digitValue = digit - (uint) 'A' + 1;
                value += digitValue * placeValue;
            }

            return value;
        }

        internal static string GetColumnLetterFromNumber(uint columnNumber)
        {
            string letter = string.Empty;

            uint multiplier = columnNumber / LettersInAlphabet;

            bool isFirstInGroup = columnNumber % LettersInAlphabet == 0;
            if (columnNumber > LettersInAlphabet && isFirstInGroup)
            {
                letter = ((char)('A' + (multiplier - 2))).ToString();
                letter += "Z";
                return letter;
            }

            if (columnNumber > LettersInAlphabet)
            {
                letter = ((char)('A' + (multiplier - 1))).ToString();
                columnNumber -= multiplier * LettersInAlphabet;
            }

            letter += (char)('A' + (columnNumber - 1));

            return letter;
        }

        public static CellReference ParseCellReference(string reference)
        {
            CellReference parsedReference = null;

            Match match = Regex.Match(reference, @"(?'column'[A-Z]{1,2})(?'row'\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                uint rowNumber = uint.Parse(match.Groups["row"].Value);
                string columnLetter = match.Groups["column"].Value;
                parsedReference = new CellReference(rowNumber, columnLetter);
            }

            return parsedReference;
        }

        // ReSharper disable once TooManyArguments
        public static string GetRangeFormula(uint topLeftRowNumber, uint topLeftColumnNumber, uint bottomRightRowNumber, uint bottomRightColumnNumber)
        {
            CellReference topLeft = new CellReference(topLeftRowNumber, topLeftColumnNumber);
            CellReference bottomRight = new CellReference(bottomRightRowNumber, bottomRightColumnNumber);
            return topLeft + ":" + bottomRight;
        }
    }
}