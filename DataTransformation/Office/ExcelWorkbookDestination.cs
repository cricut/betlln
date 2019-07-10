using System.Collections.Generic;
using System.IO;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.Office
{
    public class ExcelWorkbookDestination : Task
    {
        private readonly ExcelWorkbookConverter _baseConverter;

        internal ExcelWorkbookDestination()
        {
            _baseConverter = new ExcelWorkbookConverter();
        }

        public List<ExcelSheetDirective> Sheets
        {
            get { return _baseConverter.Sheets; }
        }

        public string OutputFileName
        {
            get { return _baseConverter.OutputName; }
            set { _baseConverter.OutputName = value; }
        }

        protected override void ExecuteTasks()
        {
            using (Stream contentStream = _baseConverter.Output.Content)
            {
                if (contentStream.Length > 0)
                {
                    using (FileStream fileStream = OpenDestinationFile())
                    {
                        contentStream.CopyTo(fileStream);
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