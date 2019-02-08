using System;
using Amazon.S3;

namespace Betlln.Data.Integration.AWS
{
    internal interface IS3Client : IDisposable
    {
        IAmazonS3 Service { get;  }
    }
}