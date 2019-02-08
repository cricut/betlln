namespace Betlln.Data.File
{
    public interface IFileAdapterFactory
    {
        IDataFileAdapter GetFileAdapter(string filePath, bool useCached);
    }
}