using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.FileSystem
{
    public class CompressFolderTask : Task
    {
        public string SourceFolderPath { get; set; }
        public string CompressedFilePath { get; set; }

        protected override void ExecuteTasks()
        {
            throw new System.NotImplementedException();
        }
    }
}