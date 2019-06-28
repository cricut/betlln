using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.FileSystem
{
    public class CompressedFileTransformation : Transformation
    {
        internal CompressedFileTransformation()
        {
            BufferSize = ExtensionMethods.DefaultBufferSize;
            Sources = new List<NamedStream>();
        }

        public List<NamedStream> Sources { get; }
        public int BufferSize { get; set; }

        protected override void WriteToStream(Stream outputStream)
        {
            using (ZipArchive archive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
            {
                foreach (NamedStream source in Sources)
                {
                    using (Stream sourceStream = source.Content)
                    {
                        if (sourceStream.Length > 0)
                        {
                            ZipArchiveEntry entry = archive.CreateEntry(source.Name);
                            using (Stream entryWriter = entry.Open())
                            {
                                entryWriter.WriteStream(sourceStream, BufferSize);
                            }
                        }
                    }
                }
            }
        }
    }
}