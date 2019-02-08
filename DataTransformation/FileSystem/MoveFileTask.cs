using System;
using System.IO;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.FileSystem
{
    public class MoveFileTask : Task
    {
        public string CurrentPath { get; set; }
        public string DestinationFolder { get; set; }

        protected override void ExecuteTasks()
        {
            string fileName = Path.GetFileName(CurrentPath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new InvalidOperationException("The source must be a file path.");
            }

            string destinationPath = Path.Combine(DestinationFolder, fileName);
            System.IO.File.Move(CurrentPath, destinationPath);
        }
    }
}