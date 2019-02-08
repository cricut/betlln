using Betlln.Data.File;
using NUnit.Framework;

namespace DtsTests
{
    [TestFixture]
    public class FileDataTableAdapterTests
    {
        [Test]
        public void SanitizeHeaderValue_RemovesDoubleSpaces()
        {
            const string input = "Net   \r\nSales \r\nRetail";

            string actual = FileDataTableAdapter.SanitizeHeaderValue(input);

            Assert.AreEqual("Net Sales Retail", actual);
        }
    }
}