using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace Betlln.Data.File
{
    public class PdfFileAdapter : IDataFileAdapter
    {
        public PdfFileAdapter(string filePath)
        {
            FilePath = filePath;
        }

        private string FilePath { get; }

        public string CurrentSectionName
        {
            get { return FileAdapterFactory.DefaultSectionName; }
            set { }
        }

        public IEnumerable<string> SectionNames
        {
            get { return FileAdapterFactory.DefaultSectionName.ToEnumerable(); }
        }

        public void SelectSection(Func<IEnumerable<string>, string> sectionSelector)
        {
        }

        public IEnumerable<FileRow> PlainData
        {
            get
            {
                string content = GetPdfText();
                return ConvertContentToRows(content);
            }
        }

        public static IEnumerable<FileRow> ConvertContentToRows(string content)
        {
            string[] contentLines = content.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
            return new Enumerator(contentLines);
        }

        /// <summary>
        /// Converts a PDF document into raw text. (Does not work well with PDFs that contain images)
        /// </summary>
        /// <remarks>
        /// This code was originally published on StackOverflow (https://stackoverflow.com/a/30478021/1687106)
        /// by Dissimilis (https://stackoverflow.com/users/743848/dissimilis)
        /// and is used by permission. It has been modified from its original form.
        /// </remarks>
        /// <returns></returns>
        private string GetPdfText()
        {
            StringBuilder documentContent = new StringBuilder();

            using (PdfReader reader = new PdfReader(FilePath))
            {
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    string pageContent = PdfTextExtractor.GetTextFromPage(reader, page, new SimpleTextExtractionStrategy());
                    pageContent = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(pageContent)));
                    documentContent.Append(pageContent);
                }
            }

            return documentContent.ToString();
        }

        private class Enumerator : IEnumerable<FileRow>, IEnumerator<FileRow>
        {
            private readonly string[] _lines;
            private int _index;

            public Enumerator(string[] lines)
            {
                _lines = lines;
                _index = -1;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<FileRow> GetEnumerator()
            {
                return this;
            }

            public bool MoveNext()
            {
                _index++;
                return IsValidIndex;
            }

            public void Reset()
            {
                _index = -1;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public FileRow Current
            {
                get
                {
                    FileRow row = null;

                    if (IsValidIndex)
                    {
                        string rawLine = _lines[_index];

                        row = new FileRow(rawLine);
                        row.RowNumber = (uint) (_index + 1);

                        string[] segments = rawLine.Split(' ');
                        for (int i = 0; i < segments.Length; i++)
                        {
                            string segment = segments[i];
                            row.Cells.Add(new DataCell((uint) (i + 1), segment));
                        }
                    }

                    return row;
                }
            }

            private bool IsValidIndex
            {
                get { return _index >= 0 && _index < _lines.Length; }
            }

            public void Dispose()
            {
            }
        }

        public void Dispose()
        {
        }
    }
}