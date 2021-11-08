using System.Collections.Generic;
using System.IO;

namespace Betlln.IO
{
    public interface IFileSystem
    {
        bool IsValidPath(string filePath);
        bool DoesFileExist(string filePath);
        string GetParentPath(string path);
        List<string> GetFiles(string folder);
        void SetReadOnly(string filePath, bool throwIfUnsupported = true);
        void Copy(string source, string destination, bool overwrite = false);
        void Delete(string path);
        void DeleteAllFilesInFolder(string folderPath);

        // ReSharper disable once TooManyArguments
        Stream OpenStream(string path, FileMode streamMode, FileAccess accessMode, FileShare shareMode);

        void Unblock(string fileName);
        void Rename(string currentPath, string newPath);
        string GetExecutableForFileType(string extension);
        void CreateDirectoryIfNotExist(string directory);
    }
}