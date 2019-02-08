using System;
using System.IO;
using System.IO.Compression;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.FileSystem
{
    public class DecompressFileTask : Task
    {
        public string CompressedFilePath { get; set; }
        public string DestinationFolder { get; set; }

        protected override void ExecuteTasks()
        {
            FileInfo sourceFile = new FileInfo(CompressedFilePath);
            FileInfo destinationFile = new FileInfo(Path.Combine(DestinationFolder, Path.GetFileName(CompressedFilePath)));

            if (destinationFile.Exists && sourceFile.LastWriteTime <= destinationFile.LastWriteTime)
            {
                throw new Exception("File already exists");
            }

            using (Stream sourceFileStream = sourceFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (GZipStream deflationStream = new GZipStream(sourceFileStream, CompressionMode.Decompress))
                {
                    using (FileStream destinationFileStream = destinationFile.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                    {
                        const int bufferSize = 255;
                        int bytesRead;
                        do
                        {
                            byte[] buffer = new byte[bufferSize];
                            bytesRead = deflationStream.Read(buffer, 0, bufferSize);
                            destinationFileStream.Write(buffer, 0, bytesRead);
                        } while (bytesRead == bufferSize);
                    }
                }
            }
        }
    }
}