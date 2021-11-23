using Betlln.IO;
using NUnit.Framework;

namespace BetllnTests
{
    [TestFixture]
    public class LocalFileSystemTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void IsValidPath_ReturnsFalse_ForEmptyStrings(string input)
        {
            Assert.False(LocalFileSystem.IsValidPath(input));
        }
        
        [TestCase("https://zarg.com/")]
        [TestCase("s3://not-a-real-bucket/thing.zip")]
        public void IsValidPath_ReturnsFalse_ForUri(string input)
        {
            Assert.False(LocalFileSystem.IsValidPath(input));
        }

        [TestCase("c:\\")]
        [TestCase("d:\\q.zip")]
        [TestCase("z:\\i\\like\\paths\\you\\see.txt")]
        [TestCase("C:\\")]
        [TestCase("D:\\Q.ZIP")]
        [TestCase("Z:\\I\\LIKE\\PATHS\\YOU\\SEE.TXT")]
        public void IsValidPath_ReturnTrue_ForAbsoluteDiskPath(string input)
        {
            Assert.True(LocalFileSystem.IsValidPath(input));
        }

        [TestCase("test.zip")]
        [TestCase("zips\\zipsA-z\\zipq.net.two.gzip")]
        [TestCase("TEST.ZIP")]
        [TestCase("ZIPS\\ZIPSA-Z\\ZIPQ.NET.TWO.GZIP")]
        public void IsValidPath_ReturnTrue_ForRelativeDiskPath(string input)
        {
            Assert.True(LocalFileSystem.IsValidPath(input));
        }
    }
}