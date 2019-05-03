using System;
using System.Collections.Generic;
using Betlln.Data.File;
using NUnit.Framework;

namespace DtsTests
{
    [TestFixture]
    [Timeout(10000)]
    public class FileRowTests
    {
        [Test]
        public void AdvanceToRow_ThrowsException_IfNoElementsInEnumerator()
        {
            Assert.Throws<DocumentFormatException>(() =>
            {
                using (IEnumerator<FileRow> classUnderTest = new List<FileRow>().GetEnumerator())
                {
                    classUnderTest.AdvanceToRow(3);
                }
            });
        }

        [Test]
        public void AdvanceToRow_MovesToRow_IfRowExists()
        {
            List<FileRow> rows = new List<FileRow>();
            rows.Add(new FileRow("row 1") {RowNumber = 1});
            rows.Add(new FileRow("row 2") {RowNumber = 2});
            rows.Add(new FileRow("row 3") {RowNumber = 3});
            rows.Add(new FileRow("row 4") {RowNumber = 4});

            using (IEnumerator<FileRow> classUnderTest = rows.GetEnumerator())
            {
                classUnderTest.AdvanceToRow(3);

                Assert.AreEqual("row 3", classUnderTest.Current?.RawContent);
            }
        }

        [Test]
        public void AdvanceToRow_DoesNothing_IfAlreadyOnRow()
        {
            List<FileRow> rows = new List<FileRow>();
            rows.Add(new FileRow("row 1") {RowNumber = 1});
            rows.Add(new FileRow("row 2") {RowNumber = 2});
            rows.Add(new FileRow("row 3") {RowNumber = 3});
            rows.Add(new FileRow("row 4") {RowNumber = 4});

            using (IEnumerator<FileRow> classUnderTest = rows.GetEnumerator())
            {
                classUnderTest.MoveNext();
                classUnderTest.MoveNext();
                classUnderTest.MoveNext();

                classUnderTest.AdvanceToRow(3);

                Assert.AreEqual("row 3", classUnderTest.Current?.RawContent);
            }
        }

        [Test]
        public void AdvanceToRow_ThrowsException_IfTryingToReverse()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                List<FileRow> rows = new List<FileRow>();
                rows.Add(new FileRow("row 1") {RowNumber = 1});
                rows.Add(new FileRow("row 2") {RowNumber = 2});

                using (IEnumerator<FileRow> classUnderTest = rows.GetEnumerator())
                {
                    classUnderTest.MoveNext();
                    classUnderTest.MoveNext();

                    classUnderTest.AdvanceToRow(1);
                }
            });
        }

        [Test]
        public void AdvanceToRow_ThrowsException_IfSeekingToUnknownRow_AtFirstElement()
        {
            Assert.Throws<DocumentFormatException>(() =>
            {
                List<FileRow> rows = new List<FileRow>();
                rows.Add(new FileRow("row 1") {RowNumber = 1});
                rows.Add(new FileRow("row 2") {RowNumber = 2});

                using (IEnumerator<FileRow> classUnderTest = rows.GetEnumerator())
                {
                    classUnderTest.AdvanceToRow(4);
                }
            });
        }

        [Test]
        public void AdvanceToRow_ThrowsException_IfSeekingToUnknownRow_OnLastElement()
        {
            Assert.Throws<DocumentFormatException>(() =>
            {
                List<FileRow> rows = new List<FileRow>();
                rows.Add(new FileRow("row 1") {RowNumber = 1});
                rows.Add(new FileRow("row 2") {RowNumber = 2});

                using (IEnumerator<FileRow> classUnderTest = rows.GetEnumerator())
                {
                    classUnderTest.MoveNext();
                    classUnderTest.MoveNext();

                    classUnderTest.AdvanceToRow(4);
                }
            });
        }

        [Test]
        public void AdvanceToRow_ThrowsException_IfSeekingToUnknownRow_PassedTheEnd()
        {
            Assert.Throws<DocumentFormatException>(() =>
            {
                List<FileRow> rows = new List<FileRow>();
                rows.Add(new FileRow("row 1") {RowNumber = 1});
                rows.Add(new FileRow("row 2") {RowNumber = 2});

                using (IEnumerator<FileRow> classUnderTest = rows.GetEnumerator())
                {
                    classUnderTest.MoveNext();
                    classUnderTest.MoveNext();
                    classUnderTest.MoveNext();

                    classUnderTest.AdvanceToRow(4);
                }
            });
        }
    }
}