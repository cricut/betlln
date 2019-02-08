using log4net;
using log4net.Config;

namespace Betlln.Logging
{
    public class Log4NetAdapter : Logger
    {
        private readonly ILog _logger;

        public Log4NetAdapter()
        {
            XmlConfigurator.Configure();
            _logger = LogManager.GetLogger(typeof(Log4NetAdapter));
        }

        protected override void SaveLog(LogEntry logEntry)
        {
            switch (logEntry.EventType)
            {
                case LogEventType.Debug:
                    _logger.Debug(logEntry.Message);
                    break;
                case LogEventType.Info:
                    _logger.Info(logEntry.Message);
                    break;
                case LogEventType.Warn:
                    _logger.Warn(logEntry.Message);
                    break;
                case LogEventType.Error:
                    _logger.Error(logEntry.Message);
                    break;
                case LogEventType.Fatal:
                    _logger.Fatal(logEntry.Message);
                    break;
            }
        }
    }
}