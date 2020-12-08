using System;
using System.ComponentModel;

namespace Betlln.IO
{
    public interface IFileFinder
    {
        /// <summary>
        /// The specified file can be found in the specified folder.
        /// </summary>
        /// <param name="demand">rules for finding the file</param>
        /// <param name="folderPath">the file system path to search</param>
        /// <returns>true or false</returns>
        bool CanFindFile(FileDemand demand, string folderPath);

        /// <summary>
        /// Finds the full path of a file that matches the specification in the folder
        /// </summary>
        /// <param name="demand">rules for finding the file</param>
        /// <param name="folderPath">the file system path to search</param>
        /// <returns>The full file path, or null if it cannot be found</returns>
        string FindFile(MultiFileDemand demand, string folderPath);

        /// <summary>
        /// Finds the full path of a file that matches the specification in the folder
        /// </summary>
        /// <param name="demand">rules for finding the file</param>
        /// <param name="folderPath">the file system path to search</param>
        /// <returns>The full file path, or null if it cannot be found</returns>
        string FindFile(FileDemand demand, string folderPath);

        /// <summary>
        /// Finds the full path of a file in a folder that may not yet have arrived in the folder.
        /// </summary>
        /// <remarks>
        /// <seealso cref="ProgressUpdated"/>
        /// </remarks>
        /// <param name="demand">rules for finding the file</param>
        /// <param name="folderPath">the file system path to search</param>
        /// <param name="timeout">the length of time to wait for the file to exist</param>
        /// <returns>The full file path</returns>
        /// <exception cref="TimeoutException">the file could not be found in the time limit specified</exception>
        string FindFile(MultiFileDemand demand, string folderPath, TimeSpan timeout);

        /// <summary>
        /// Finds the full path of a file in a folder that may not yet have arrived in the folder.
        /// </summary>
        /// <remarks>
        /// <seealso cref="ProgressUpdated"/>
        /// </remarks>
        /// <param name="demand">rules for finding the file</param>
        /// <param name="folderPath">the file system path to search</param>
        /// <param name="timeout">the length of time to wait for the file to exist</param>
        /// <returns>The full file path</returns>
        /// <exception cref="TimeoutException">the file could not be found in the time limit specified</exception>
        string FindFile(FileDemand demand, string folderPath, TimeSpan timeout);

        /// <summary>
        /// The specified fileset can be found in the specified folder.
        /// </summary>
        /// <param name="demand">rules for finding the file</param>
        /// <param name="folderPath">the file system path to search</param>
        /// <returns>true or false</returns>
        bool CanFindAllFiles(MultiFileDemand demand, string folderPath);
        
        [Obsolete("Use" + nameof(FindFile) + " instead.")]
        string FindDownloadedFile(FileDemand fileDemand, string folderPath, TimeSpan timeout);
        
        /// <summary>
        /// Raised when <see cref="FindFile(MultiFileDemand,string,TimeSpan)"/> or <see cref="FindFile(FileDemand,string,TimeSpan)"/> updates the progress of a find.
        /// </summary>
        event EventHandler<ProgressChangedEventArgs> ProgressUpdated;
    }
}