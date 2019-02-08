using Amazon.S3.Model;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.AWS
{
    public class S3JsonFileSource : DataSource
    {
        internal S3JsonFileSource()
        {
        }

        public string BucketName { get; set; }
        public string Key { get; set; }
        
        protected override IDataRecordIterator CreateReader()
        {
            S3Object s3Object = new S3Object { BucketName = BucketName, Key = Key };
            return new S3JsonFileDocumentCollection((S3ConnectionManager) Connection, s3Object);
        }
    }
}