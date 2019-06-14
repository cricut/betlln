using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.FileSystem
{
    public class CompressedFileDestination : Task
    {
        internal CompressedFileDestination()
        {
            BufferSize = 1024;
            Sources = new List<NamedStream>();
        }

        public List<NamedStream> Sources { get; }
        public int BufferSize { get; set; }
        public string OutputPath { get; set; }

        protected override void ExecuteTasks()
        {
            using (Stream outputStream = System.IO.File.Open(OutputPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (ZipArchive archive = new ZipArchive(outputStream, ZipArchiveMode.Create))
                {
                    foreach (NamedStream source in Sources)
                    {
                        ZipArchiveEntry entry = archive.CreateEntry(source.Name);
                        using (Stream entryWriter = entry.Open())
                        {
                            int? bytesRead = null;
                            while (!bytesRead.HasValue || bytesRead == BufferSize)
                            {
                                byte[] buffer = new byte[BufferSize];
                                bytesRead = source.Content.Read(buffer, 0, BufferSize);
                                entryWriter.Write(buffer, 0, BufferSize);
                            }
                        }
                    }
                }
            }
        }
    }
}