using System;
using System.Security.Principal;

namespace Betlln.Logging
{
    public abstract class Logger
    {
        public void Debug(string message)
        {
            Log(new LogEntry
            {
                Message = message,
                EventType = LogEventType.Debug
            });
        }

        public void Info(string message)
        {
            Log(new LogEntry
            {
                Message = message,
                EventType = LogEventType.Info
            });
        }

        public void Error(string message)
        {
            Log(new LogEntry
            {
                Message = message,
                EventType = LogEventType.Error
            });
        }

        public void Log(string message, LogEventType eventType = LogEventType.Debug)
        {
            Log(new LogEntry
            {
                Message = message,
                EventType = eventType
            });
        }

        public void Log(LogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (entry.LogDate == DateTime.MinValue)
            {
                entry.LogDate = DateTime.Now;
            }
            if (string.IsNullOrWhiteSpace(entry.ApplicationName))
            {
                entry.ApplicationName = RuntimeContext.ApplicationName;
            }
            if (entry.ApplicationVersion == null)
            {
                entry.ApplicationVersion = RuntimeContext.ApplicationVersion;
            }
            if (string.IsNullOrWhiteSpace(entry.UserName))
            {
                WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
                entry.UserName = currentIdentity.Name;
            }

            try
            {
                SaveLog(entry);
            }
            catch (Exception loggingError)
            {
                System.Diagnostics.Debug.Print(loggingError.ToString());
            }
        }

        protected abstract void SaveLog(LogEntry logEntry);
    }
}