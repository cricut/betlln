using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Betlln.Data.Integration.Json;

namespace Betlln.Data.Integration.AWS
{
    internal class S3JsonFileDocumentCollection : JsonDocumentCollection
    {
        public S3JsonFileDocumentCollection(S3ConnectionManager connectionManager, S3Object s3Object)
        {
            ConnectionManager = connectionManager;
            S3Object = s3Object;
        }

        private S3ConnectionManager ConnectionManager { get; }
        private S3Object S3Object { get; }

        protected override void PopulateReadPipeline()
        {
            IS3Client client = (IS3Client) ConnectionManager.GetConnection();
            _readPipeline.Push(client);

            Task<GetObjectResponse> getTask = client.Service.GetObjectAsync(S3Object.BucketName, S3Object.Key);
            Task.WaitAll(getTask);
            GetObjectResponse apiResponse = getTask.Result;
            _readPipeline.Push(apiResponse);

            Stream responseStream = apiResponse.ResponseStream;
            _readPipeline.Push(responseStream);

            string contentEncoding = apiResponse.Headers.ContentEncoding ?? string.Empty;
            if (contentEncoding.Contains("gzip"))
            {
                _readPipeline.Push(new GZipStream(responseStream, CompressionMode.Decompress));
            }
        }

        protected override string SourceObjectName
        {
            get { return S3Object.Key; }
        }
    }
}