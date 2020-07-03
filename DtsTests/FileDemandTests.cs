using Betlln.IO;
using NUnit.Framework;

namespace DtsTests
{
    [TestFixture]
    public class FileDemandTests
    {
        [Test]
        public void FileDemandToString_ReturnsPrettyValue_ForComplexRegex()
        {
            FileDemand fileDemand = FileDemand.FromPattern(@"^CR 2018\-(6|06)\.csv$");
            string actual = fileDemand.ToString();

            StringAssert.Contains("CR 2018-*.csv", actual);
        }

        [Test]
        public void FileDemandToString_ReturnsPrettyValue_ForMoreComplexRegex()
        {
            FileDemand fileDemand = FileDemand.FromPattern(@"^66248\-\d{2}_2019\-04\-27_US(A[0-9])?\.xls(x?)$");
            string actual = fileDemand.ToString();

            StringAssert.Contains("66248-*_2019-04-27_USA#.xlsx", actual);
        }
    }
}
