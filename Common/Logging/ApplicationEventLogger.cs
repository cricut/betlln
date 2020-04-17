using System;
using System.Diagnostics;

namespace Betlln.Logging
{
    public class ApplicationEventLogger : Logger
    {
        protected override void SaveLog(LogEntry logEntry)
        {
            if (!EventLog.SourceExists(logEntry.ApplicationName))
            {
                EventLog.CreateEventSource(logEntry.ApplicationName, "Application");
            }

            const int eventID = ushort.MaxValue;
            EventLog.WriteEntry(logEntry.ApplicationName, logEntry.Message, GetEntryKind(logEntry.EventType), eventID);
        }

        private static EventLogEntryType GetEntryKind(LogEventType logLevel)
        {
            switch (logLevel)
            {
                case LogEventType.Debug:
                case LogEventType.Info:
                    return EventLogEntryType.Information;
                case LogEventType.Warn:
                    return EventLogEntryType.Warning;
                case LogEventType.Error:
                case LogEventType.Fatal:
                    return EventLogEntryType.Error;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}