using Betlln.Data.File;
using NUnit.Framework;

namespace DtsTests
{
    [TestFixture]
    public class OpenXmlFileAdapterTests
    {
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