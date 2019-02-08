using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.AWS
{
    public interface IS3FolderSource
    {
        IConnectionManager Connection { get; set; }
        string BucketName { get; set; }
        string Directory { get; set; }
        string FilePattern { get; set; }
    }
}