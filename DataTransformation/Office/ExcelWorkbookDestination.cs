using System.Collections.Generic;
using System.IO;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.Office
{
    public class ExcelWorkbookDestination : Task
    {
        private readonly ExcelWorkbookTransformation _baseTransformation;

        internal ExcelWorkbookDestination()
        {
            _baseTransformation = new ExcelWorkbookTransformation();
        }

        public List<ExcelSheetTransformation> Sheets
        {
            get { return _baseTransformation.Sheets; }
        }

        public string OutputFileName
        {
            get { return _baseTransformation.OutputName; }
            set { _baseTransformation.OutputName = value; }
        }

        protected override void ExecuteTasks()
        {
            using (Stream contentStream = _baseTransformation.Output.Content)
            {
                using (FileStream fileStream = System.IO.File.Open(OutputFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    const int bufferSize = 8192;
                    int? bytesRead = null;
                    while (!bytesRead.HasValue || bytesRead == bufferSize)
                    {
                        byte[] buffer = new byte[bufferSize];
                        bytesRead = contentStream.Read(buffer, 0, bufferSize);
                        fileStream.Write(buffer, 0, bufferSize);
                    }
                }
            }
        }
    }
}