﻿using System;
using System.ComponentModel;

namespace Betlln.IO
{
    public interface IFileFinder
    {
        bool CanFindFile(FileDemand demand, string folderPath);
        string FindFile(FileDemand demand, string folderPath);
        string FindFile(FileDemand demand, string folderPath, TimeSpan timeout);
        bool CanFindAllFiles(MultiFileDemand demand, string folderPath);
        string FindDownloadedFile(FileDemand fileDemand, string folderPath, TimeSpan timeout);
        event EventHandler<ProgressChangedEventArgs> ProgressUpdated;
    }
}