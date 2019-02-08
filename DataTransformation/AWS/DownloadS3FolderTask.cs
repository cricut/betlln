using System.IO;
using Amazon.S3.Transfer;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.AWS
{
    public class DownloadS3FolderTask : Task
    {
        public IConnectionManager Connection { get; set; }
        public string SourceBucketName { get; set; }
        public string SourceFolder { get; set; }
        public string DestinationFolder { get; set; }
        public string SourceFilter { get; set; }

        protected override void ExecuteTasks()
        {
            DirectoryInfo downloadTarget = new DirectoryInfo(DestinationFolder);
            if (!downloadTarget.Exists)
            {
                downloadTarget.Create();
            }

            using (IS3Client client = (IS3Client) Connection.GetConnection())
            {
                using (TransferUtility downloader = new TransferUtility(client.Service))
                {
                    downloader.DownloadDirectory(SourceBucketName, SourceFolder, downloadTarget.FullName);
                }
            }

            if (!string.IsNullOrWhiteSpace(SourceFilter))
            {
                FileInfo[] files = downloadTarget.GetFiles();
                foreach (FileInfo fileInfo in files)
                {
                    string fileExtension = fileInfo.Extension.Replace(".", "").Trim().ToLower();
                    string targetExtension = SourceFilter.Replace("*.", "").Trim().ToLower();
                    if (fileExtension != targetExtension)
                    {
                        fileInfo.Delete();
                    }
                }
            }
        }
    }
}