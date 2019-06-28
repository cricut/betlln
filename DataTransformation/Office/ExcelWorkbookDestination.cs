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
                if (contentStream.Length > 0)
                {
                    using (FileStream fileStream = OpenDestinationFile())
                    {
                        fileStream.WriteStream(contentStream);
                    }
                }
            }
        }

        private FileStream OpenDestinationFile()
        {
            return System.IO.File.Open(OutputFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        }
    }
}