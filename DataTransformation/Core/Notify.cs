using System;
using System.IO;
using Betlln.Logging;
using Betlln.Data.Integration.SqlServer;

namespace Betlln.Data.Integration.Core
{
    public class Notify
    {
        private Logger _actualLogger;

        public Notify()
        {
            string filePath = $"{ProjectInfo.Name}.LOG";
            filePath = Path.Combine(SystemVariables.WorkingFolder, filePath);
            SetLogger(new FileAppendLogger(filePath));
        }

        public void SetLogTarget(string target, string subTarget = null)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                throw new ArgumentNullException();
            }

            if (target.Substring(1, 1) == ":")
            {
                SetLogger(new FileAppendLogger(target));
            }
            else
            {
                object connectionManagerObject = ProjectInfo.ConnectionManagers?.GetPropertyValue(target);
                if (connectionManagerObject == null)
                {
                    Console($"Could not start logger for connection {target}");
                }
                else
                {
                    SqlConnectionManager sqlConnectionManager = connectionManagerObject as SqlConnectionManager;
                    if (sqlConnectionManager != null)
                    {
                        if (string.IsNullOrWhiteSpace(subTarget))
                        {
                            throw new ArgumentNullException(nameof(subTarget));
                        }

                        DatabaseLogger sqlLogger = DatabaseLogger.GetLoggerByAddress(sqlConnectionManager.ConnectionAddress, subTarget);
                        SetLogger(sqlLogger);
                    }
                }
            }
        }

        private void SetLogger(Logger logger)
        {
            _actualLogger = logger;
            RuntimeContext.DefaultLogger = logger;
        }

        // ReSharper disable once FlagArgument
        public void All(string message, LogEventType level = LogEventType.Info)
        {
            if (level != LogEventType.Debug)
            {
                Console(message);
            }

            Log(message, level);
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void Console(string message)
        {
            System.Console.WriteLine(message);
        }

        public void Log(string message, LogEventType level = LogEventType.Info)
        {
            if (_actualLogger == null)
            {
                throw new ObjectDisposedException("The final flush has already been invoked.");
            }

            _actualLogger.Log(message, level);
        }

        public void Flush()
        {
            IDisposable disposable = _actualLogger as IDisposable;
            disposable?.Dispose();
            SetLogger(null);
        }
    }
}