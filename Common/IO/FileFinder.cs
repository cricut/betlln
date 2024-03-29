﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Betlln.IO
{
    public class FileFinder : IFileFinder
    {
        public FileFinder(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public IFileSystem FileSystem { get; }
        public event EventHandler<ProgressChangedEventArgs> ProgressUpdated;

        public string FindFile(MultiFileDemand demand, string folderPath, TimeSpan timeout)
        {
            return FindFileWithTimeLimit(() => FindFile(demand, folderPath), timeout);
        }

        public string FindFile(FileDemand demand, string folderPath, TimeSpan timeout)
        {
            return FindFileWithTimeLimit(() => FindFile(demand, folderPath), timeout);
        }

        private string FindFileWithTimeLimit(Func<string> finder, TimeSpan timeout)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string downloadedFileName = null;
            while (stopwatch.Elapsed < timeout && string.IsNullOrWhiteSpace(downloadedFileName))
            {
                UpdateProgress(0, "Waiting for file download...");
                downloadedFileName = finder();
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
                return CanFindAllFiles(demand.RequiredItems, folderPath);
            }

            return demand.OptionGroups.Any(demandGroup => CanFindAllFiles(demandGroup, folderPath));
        }

        private bool CanFindAllFiles(IEnumerable<FileDemand> requiredItems, string folderPath)
        {
            return requiredItems.All(requiredItem => CanFindFile(requiredItem, folderPath));
        }

        [Obsolete("Use" + nameof(FindFile) + " instead.")]
        public string FindDownloadedFile(FileDemand fileDemand, string folderPath, TimeSpan timeout)
        {
            return FindFile(fileDemand, folderPath, timeout);
        }

        public bool CanFindFile(FileDemand demand, string folderPath)
        {
            //this block will return true if demand.ImplicitFileName is a full path and we don't need to search the folder
            if (FileSystem.DoesFileExist(demand.ImplicitFileName))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(FindFile(demand, folderPath));
        }

        public string FindFile(MultiFileDemand demand, string folderPath)
        {
            return demand.OptionGroups
                            .Select(demandGroup => FindFile(demandGroup.First(), folderPath))
                            .FirstOrDefault(filePath => !string.IsNullOrWhiteSpace(filePath));
        }

        public string FindFile(FileDemand demand, string folderPath)
        {
            return FileSystem.GetFiles(folderPath).FirstOrDefault(demand.IsSatisfiedBy);
        }
    }
}