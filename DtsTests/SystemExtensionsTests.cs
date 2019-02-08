using System;
using System.Collections.Generic;
using Betlln;
using NUnit.Framework;

namespace DtsTests
{
    [TestFixture]
    public class SystemExtensionsTests
    {
        [Test]
        public void Resolve_HandlesSimpleRelativePathCorrectly()
        {
            Uri currentUri = new Uri("http://webportal.retailer.com/webportalNew/Default.aspx");
            string newRelativeUrl = "QueryInventory.aspx";

            Uri actual = currentUri.Resolve(newRelativeUrl);

            Uri expected = new Uri("http://webportal.retailer.com/webportalNew/QueryInventory.aspx");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GoToUrl_HandlesDirectRelativePathCorrectly()
        {
            Uri currentUri = new Uri("http://webportal.retailer.com/webportalNew/Default.aspx");
            string newRelativeUrl = "/QueryInventory.aspx";

            Uri actual = currentUri.Resolve(newRelativeUrl);

            Uri expected = new Uri("http://webportal.retailer.com/QueryInventory.aspx");
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SumNullable_intOverload_ReturnsNull_ForListOfAllNulls()
        {
            List<int?> list = new List<int?> {null, null, null};

            int? actual = list.SumNullable(x => x);

            Assert.Null(actual);
        }

        [Test]
        public void SumNullable_intOverload_ReturnsSum_ForListOfAllNotNulls()
        {
            List<int?> list = new List<int?> {1, 2, 3};

            int? actual = list.SumNullable(x => x);

            Assert.NotNull(actual);
            Assert.AreEqual(6, actual);
        }

        [Test]
        public void SumNullable_intOverload_ReturnsSum_ForMixOfNullAndNotNull()
        {
            List<int?> list = new List<int?> {1, null, 3};

            int? actual = list.SumNullable(x => x);

            Assert.NotNull(actual);
            Assert.AreEqual(4, actual);
        }

        [Test]
        public void SumNullable_decimalOverload_ReturnsNull_ForListOfAllNulls()
        {
            List<decimal?> list = new List<decimal?> {null, null, null};

            decimal? actual = list.SumNullable(x => x);

            Assert.Null(actual);
        }

        [Test]
        public void SumNullable_decimalOverload_ReturnsSum_ForListOfAllNotNulls()
        {
            List<decimal?> list = new List<decimal?> {1.1m, 2.5m, 3.98m};

            decimal? actual = list.SumNullable(x => x);

            Assert.NotNull(actual);
            Assert.AreEqual(7.58m, actual);
        }

        [Test]
        public void SumNullable_decimalOverload_ReturnsSum_ForMixOfNullAndNotNull()
        {
            List<decimal?> list = new List<decimal?> {0.1m, null, 3.5m};

            decimal? actual = list.SumNullable(x => x);

            Assert.NotNull(actual);
            Assert.AreEqual(3.6m, actual);
        }
    }
}