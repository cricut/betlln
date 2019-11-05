using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Betlln.IO
{
    public class FileFinder : IFileFinder
    {
        private readonly IFileSystem _fileSystem;

        public FileFinder(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public event EventHandler<ProgressChangedEventArgs> ProgressUpdated;

        public string FindFile(FileDemand demand, string folderPath, TimeSpan timeout)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string downloadedFileName = null;
            while (stopwatch.Elapsed < timeout && string.IsNullOrWhiteSpace(downloadedFileName))
            {
                UpdateProgress(0, "Waiting for file download...");
                downloadedFileName = FindFile(demand, folderPath);
            }
            stopwatch.Stop();
            UpdateProgress(100, null);

            if (string.IsNullOrWhiteSpace(downloadedFileName))
            {
                throw new TimeoutException("The download took too long or did not complete.");
            }

            return downloadedFileName;
        }

        private void UpdateProgress(int percentComplete, string message)
        {
            ProgressUpdated?.Invoke(this, new ProgressChangedEventArgs(percentComplete, message));
        }

        public bool CanFindAllFiles(MultiFileDemand demand, string folderPath)
        {
            if (demand.RequiredItems.Any())
            {
                foreach (FileDemand requiredItem in demand.RequiredItems)
                {
                    if (!CanFindFile(requiredItem, folderPath))
                    {
                        return false;
                    }
                }

                return true;
            }

            foreach (IReadOnlyList<FileDemand> demandGroup in demand.OptionGroups)
            {
                bool isGroupComplete = true;
                foreach (FileDemand fileDemand in demandGroup)
                {
                    if (!CanFindFile(fileDemand, folderPath))
                    {
                        isGroupComplete = false;
                    }
                }

                if (isGroupComplete)
                {
                    return true;
                }
            }

            return false;
        }

        public string FindDownloadedFile(FileDemand fileDemand, string folderPath, TimeSpan timeout)
        {
            return FindFile(fileDemand, folderPath, timeout);
        }

        public bool CanFindFile(FileDemand demand, string folderPath)
        {
            //this block will return true if demand.ImplicitFileName is a full path and we don't need to search the folder
            if (_fileSystem.DoesFileExist(demand.ImplicitFileName))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(FindFile(demand, folderPath));
        }

        public string FindFile(FileDemand demand, string folderPath)
        {
            return _fileSystem.GetFiles(folderPath).FirstOrDefault(demand.IsSatisfiedBy);
        }
    }
}