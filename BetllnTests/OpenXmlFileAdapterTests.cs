using System.IO;
using System.Linq;
using Betlln.Data.File;
using Betlln.Spreadsheets;
using NUnit.Framework;

namespace DtsTests
{
    [TestFixture]
    public class OpenXmlFileAdapterTests
    {
        [Test]
        public void CanReadMacroEnabled2007PlusWorkbook()
        {
            FileAdapterFactory fileAdapterFactory = new FileAdapterFactory();
            string fileName = Path.Combine(Directory.GetParent(GetType().Assembly.Location).FullName, "sample.xlsm");

            object cellK28Value;
            using (IDataFileAdapter dataFileAdapter = fileAdapterFactory.GetFileAdapter(fileName, useCached: false))
            {
                FileRow row28 = dataFileAdapter.PlainData.FirstOrDefault(x => x.RowNumber == 28);
                DataCell cellK28 = row28.Cells.FirstOrDefault(y => y.ColumnNumber == CellReference.GetColumnNumberFromLetter("K"));
                cellK28Value = cellK28.Value;
            }

            Assert.AreEqual(102.ToString(), cellK28Value);
        }

        [Test]
        public void UnMirrorValue_ReturnsCorrectValue_ForMirroredValue()
        {
            string actual = OpenXmlFileAdapter.UnMirrorValue("\"2001974              \"2001974              ");
            Assert.AreEqual("\"2001974              ", actual);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("0")]
        [TestCase("5")]
        [TestCase("55")]
        [TestCase("5555")]
        [TestCase("20012001")]
        [TestCase("12.12")]
        public void UnMirrorValue_ReturnsSameValue_ForNonMirroredValue(string value)
        {
            string actual = OpenXmlFileAdapter.UnMirrorValue(value);
            Assert.AreEqual(value, actual);
        }
    }
}