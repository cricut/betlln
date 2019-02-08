using Betlln.Spreadsheets;
using NUnit.Framework;

namespace DtsTests
{
    [TestFixture]
    public class CellReferenceTests
    {
        [TestCase(1U, "A")]
        [TestCase(2U, "B")]
        [TestCase(26U, "Z")]
        [TestCase(27U, "AA")]
        [TestCase(28U, "AB")]
        [TestCase(36U, "AJ")]
        [TestCase(52U, "AZ")]
        [TestCase(53U, "BA")]
        [TestCase(65U, "BM")]
        public void GetColumnNumberFromLetter_ReturnsColumnNumber(uint expected, string letter)
        {
            Assert.AreEqual(expected, CellReference.GetColumnNumberFromLetter(letter));
        }

        [TestCase("A1", 1, "A")]
        [TestCase("B4", 4, "B")]
        [TestCase("AB93", 93, "AB")]
        public void ParseCellReference_ReturnsCorrectColumnAndNumber(string reference, int rowNumber, string columnLetter)
        {
            CellReference actual = CellReference.ParseCellReference(reference);

            Assert.AreEqual(rowNumber, actual.RowNumber);
            Assert.AreEqual(columnLetter, actual.ColumnLetter);
        }

        [TestCase(1U, "A")]
        [TestCase(2U, "B")]
        [TestCase(3U, "C")]
        [TestCase(4U, "D")]
        [TestCase(5U, "E")]
        [TestCase(26U, "Z")]
        [TestCase(27U, "AA")]
        [TestCase(28U, "AB")]
        [TestCase(29U, "AC")]
        [TestCase(29U, "AC")]
        [TestCase(52U, "AZ")]
        [TestCase(53U, "BA")]
        [TestCase(54U, "BB")]
        public void GetColumnLetterFromNumber_ReturnsCorrectLetter(uint number, string letter)
        {
            Assert.AreEqual(letter, CellReference.GetColumnLetterFromNumber(number));
        }
    }
}