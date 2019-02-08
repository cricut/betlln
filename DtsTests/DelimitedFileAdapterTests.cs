using Betlln.Data.File;
using NUnit.Framework;

namespace DtsTests
{
    [TestFixture]
    public class DelimitedFileAdapterTests
    {
        [Test]
        public void SanitizeValue_RemovesAllFileDelimiters()
        {
            Assert.AreEqual("13-6238", DelimitedFileAdapter.SanitizeValue("\"=\"\"13-6238              \"\"\""));
            Assert.AreEqual("13-6238", DelimitedFileAdapter.SanitizeValue("=\"\"13-6238              \"\""));
        }
    }
}