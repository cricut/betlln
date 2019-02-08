using System;
using System.IO;

namespace Betlln.Logging
{
    public class FileAppendLogger : Logger, IDisposable
    {
        private readonly ResourceStack _resources;

        public FileAppendLogger(string filePath)
        {
            FilePath = filePath;
            _resources = new ResourceStack();
        }

        ~FileAppendLogger()
        {
            Dispose();
        }

        private string FilePath { get; }

        protected override void SaveLog(LogEntry logEntry)
        {
            if (_resources.Count == 0)
            {
                FileStream fileStream = File.Open(FilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                _resources.Push(fileStream);
                _resources.Push(new StreamWriter(fileStream));
            }

            StreamWriter writer = _resources.Tip as StreamWriter;
            System.Diagnostics.Debug.Assert(writer != null);

            writer.WriteLine($"[{logEntry.LogDate}] [{logEntry.EventType}] {logEntry.Message}");
            writer.Flush();
        }

        public void Dispose()
        {
            _resources.Dispose();
        }
    }
}