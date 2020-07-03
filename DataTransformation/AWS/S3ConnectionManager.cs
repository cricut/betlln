using System;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.AWS
{
    // ReSharper disable once HollowTypeName
    public class S3ConnectionManager : IConnectionManager, IS3Client
    {
        private IAmazonS3 _client;

        ~S3ConnectionManager()
        {
            _client?.Dispose();   
        }

        public IDisposable GetConnection()
        {
            return this;
        }

        public Type GetDataAdapterType()
        {
            throw new NotSupportedException();
        }

        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public RegionEndpoint Endpoint { get; set; }

        public IAmazonS3 Service
        {
            get
            {
                if (_client == null)
                {
                    AWSCredentials credentials = new BasicAWSCredentials(AccessKey, SecretKey);
                    _client = new AmazonS3Client(credentials, Endpoint);
                }

                return _client;
            }
        }

        public void Dispose()
        {
        }
    }
}