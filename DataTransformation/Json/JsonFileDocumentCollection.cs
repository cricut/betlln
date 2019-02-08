using System.IO;

namespace Betlln.Data.Integration.Json
{
    internal class JsonFileDocumentCollection : JsonDocumentCollection
    {
        public JsonFileDocumentCollection(string fileName)
        {
            SourceObjectName = fileName;
        }

        protected override string SourceObjectName { get; }

        protected override void PopulateReadPipeline()
        {
            FileStream fileStream = System.IO.File.Open(SourceObjectName, FileMode.Open, FileAccess.Read, FileShare.Read);
            _readPipeline.Push(fileStream);
        }
    }
}