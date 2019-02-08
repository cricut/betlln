using System.Globalization;
using Betlln.Data;
using NUnit.Framework;

namespace DtsTests
{
    [TestFixture]
    public class NumberConverterTests
    {
        [TestCase("(1891)")]
        [TestCase("(1891.0)")]
        [TestCase("(1891.0000)")]
        public void ParseNumericalString_ReturnsCorrectValue_ForAccountingNegativeFormats(string input)
        {
            Assert.AreEqual(-1891, NumberConverter.Parse(input, NumberFormatInfo.CurrentInfo));
        }

        [TestCase("1891")]
        [TestCase("1891.0")]
        [TestCase("1891.0000")]
        public void ParseNumericalString_ReturnsCorrectValue_ForEasyFormats(string input)
        {
            Assert.AreEqual(1891, NumberConverter.Parse(input, NumberFormatInfo.CurrentInfo));
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("   ")]
        [TestCase("           ")]
        public void ParseNumericalString_ReturnsNull_ForEmptyStrings(string input)
        {
            Assert.IsNull(NumberConverter.Parse(input, NumberFormatInfo.CurrentInfo));
        }

        [Test]
        public void ParseNumericalString_ParsesScientificNotationValues()
        {
            decimal? actual = NumberConverter.Parse("1.66E-2", NumberFormatInfo.CurrentInfo);

            Assert.NotNull(actual);
            Assert.AreEqual(0.0166M, actual.Value);
        }

        [Test]
        public void ParseNumericalString_ParsesNonScientificNotationValues()
        {
            decimal? actual = NumberConverter.Parse("0.0166", NumberFormatInfo.CurrentInfo);

            Assert.NotNull(actual);
            Assert.AreEqual(0.0166M, actual.Value);
        }

        [Test]
        public void ParseNumericalString_ParsesNegativeNumbers()
        {
            decimal? actual = NumberConverter.Parse("-0.0166", NumberFormatInfo.CurrentInfo);

            Assert.NotNull(actual);
            Assert.AreEqual(-0.0166M, actual.Value);
        }
    }
}