using System;
using System.Collections.Generic;

namespace Betlln.Data.File
{
    public class FileAdapterFactory : IFileAdapterFactory
    {
        public const string DefaultSectionName = "default";
        private static readonly Dictionary<string, FileAdapterCache> CachedAdapters = new Dictionary<string, FileAdapterCache>();

        static FileAdapterFactory()
        {
            AppDomain.CurrentDomain.DomainUnload += StaticDispose;
            AppDomain.CurrentDomain.ProcessExit += StaticDispose;
        }
        
        /// <inheritdoc />
        public IDataFileAdapter GetFileAdapter(string filePath)
        {
            return GetFileAdapter(filePath, true);
        }

        /// <inheritdoc />
        [Obsolete("Use GetFileAdapter(string filePath) instead.")]
        public IDataFileAdapter GetFileAdapter(string filePath, bool useCached)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            string cacheKey = filePath.ToUpper().Trim();

            FileAdapterCache adapter;
            if (!CachedAdapters.ContainsKey(cacheKey))
            {
                adapter = new FileAdapterCache(GetBaseAdapter(filePath));
                CachedAdapters.Add(cacheKey, adapter);
            }
            else
            {
                adapter = CachedAdapters[cacheKey];
            }

            return adapter;
        }

        private static IDataFileAdapter GetBaseAdapter(string filePath)
        {
            string fileExtension = SystemExtensions.GetFileExtension(filePath);
            switch (fileExtension)
            {
                case "pdf":
                    return new PdfFileAdapter(filePath);
                case "txt":
                    return new DelimitedFileAdapter(filePath, '\t');
                case "csv":
                    return new DelimitedFileAdapter(filePath, ',');
                case "xlsx":
                case "xlsm":
                    return new OpenXmlFileAdapter(filePath);
                default:
                    return new ExcelFileAdapter(filePath);
            }
        }

        private static void StaticDispose(object sender, EventArgs e)
        {
            foreach (var cachedAdapter in CachedAdapters)
            {
                cachedAdapter.Value.Dispose();
            }
            CachedAdapters.Clear();
        }
    }
}