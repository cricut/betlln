using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.FileSystem
{
    public class CompressedFileTransformation
    {
        public CompressedFileTransformation()
        {
            BufferSize = ExtensionMethods.DefaultBufferSize;
            Sources = new List<NamedStream>();
        }

        public List<NamedStream> Sources { get; }
        public int BufferSize { get; set; }
        public string OutputName { get; set; }

        private NamedStream _output;
        public NamedStream Output
        {
            get
            {
                if (_output == null)
                {
                    _output = new NamedStream();
                    _output.Name = OutputName;
                    _output.Content = new MemoryStream();
                    WriteToStream();
                    _output.Content.Position = 0;
                }

                return _output;
            }
        }

        private void WriteToStream()
        {
            using (ZipArchive archive = new ZipArchive(_output.Content, ZipArchiveMode.Create, true))
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