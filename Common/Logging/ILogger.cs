namespace Betlln.Logging
{
    public interface ILogger
    {
        void Debug(string message);
        void Info(string message);
        void Error(string message);
        void Log(string message, LogEventType eventType = LogEventType.Debug);
        void Log(LogEntry entry);
    }
}