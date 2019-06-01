using System.Collections.Generic;
using System.IO;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.FileSystem
{
    public class DeleteFolderContentsTask : Task
    {
        internal DeleteFolderContentsTask()
        {
        }

        public string Folder { get; set; }

        protected override void ExecuteTasks()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(Folder);
            IEnumerable<FileInfo> files = directoryInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                file.Delete();
            }
        }
    }
}