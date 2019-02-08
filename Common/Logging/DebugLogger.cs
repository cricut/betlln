namespace Betlln.Logging
{
    public class DebugLogger : Logger
    {
        protected override void SaveLog(LogEntry logEntry)
        {
            System.Diagnostics.Debug.Print($"{logEntry.EventType}: {logEntry.Message}");
        }
    }
}