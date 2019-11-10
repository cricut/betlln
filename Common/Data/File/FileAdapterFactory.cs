using System;

namespace Betlln.Data.File
{
    public class FileAdapterFactory : IFileAdapterFactory
    {
        public IDataFileAdapter GetFileAdapter(string filePath, bool useCached)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            IDataFileAdapter baseAdapter;

            string fileExtension = SystemExtensions.GetFileExtension(filePath);
            switch (fileExtension)
            {
                case "pdf":
                    baseAdapter = new PdfFileAdapter(filePath);
                    break;
                case "txt":
                    baseAdapter = new DelimitedFileAdapter(filePath, '\t');
                    break;
                case "csv":
                    baseAdapter = new DelimitedFileAdapter(filePath, ',');
                    break;
                case "xlsx":
                case "xlsm":
                    baseAdapter = new OpenXmlFileAdapter(filePath);
                    break;
                default:
                    baseAdapter = new ExcelFileAdapter(filePath);
                    break;
            }

            return useCached ? new FileAdapterCache(baseAdapter) : baseAdapter;
        }
    }
}