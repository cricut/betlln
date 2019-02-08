using System;

namespace Betlln.Logging
{
    public class LogEntry
    {
        public string ApplicationName { get; set; }
        public Version ApplicationVersion { get; set; }
        public string UserName { get; set; }
        public LogEventType EventType { get; set; }
        public string Message { get; set; }
        public DateTime LogDate { get; set; }
    }
}