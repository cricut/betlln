using System;
using System.Data;
using System.Data.SqlClient;
using Betlln.Data;

namespace Betlln.Logging
{
    public class DatabaseLogger : Logger
    {
        private readonly string _connectionAddress;

        private DatabaseLogger(string connectionAddress, string procedureName)
        {
            _connectionAddress = connectionAddress;
            ProcedureName = procedureName;
        }

        protected override void SaveLog(LogEntry logEntry)
        {
            #if !DEBUG
            if (logEntry.EventType == LogEventType.Debug)
            {
                return;
            }
            #endif

            try
            {
                SqlDatabaseAdapter.ExecuteNonQueryStoredProcedure
                (
                    _connectionAddress,
                    ProcedureName, 
                    new SqlParameter("@ApplicationName", SqlDbType.VarChar) {Value = logEntry.ApplicationName},
                    new SqlParameter("@ApplicationVersion", SqlDbType.VarChar) {Value = logEntry.ApplicationVersion.ToString()},
                    new SqlParameter("@MachineName", SqlDbType.NVarChar) {Value = Environment.MachineName},
                    new SqlParameter("@username", SqlDbType.NVarChar) {Value = logEntry.UserName},
                    new SqlParameter("@EventType", SqlDbType.VarChar) {Value = logEntry.EventType.ToString()},
                    new SqlParameter("@EventMessage", SqlDbType.NVarChar) {Value = logEntry.Message},
                    new SqlParameter("@LogDate", SqlDbType.DateTime) {Value = logEntry.LogDate }
                );
            }
            catch (Exception loggingError)
            {
                System.Diagnostics.Debug.Print(loggingError.ToString());
            }
        }

        private string ProcedureName { get; }

        public static DatabaseLogger GetLoggerByName(string connectionName, string procedureName)
        {
            string connectionAddress = SqlDatabaseAdapter.GetConnectionAddressByName(connectionName);
            return GetLoggerByAddress(connectionAddress, procedureName);
        }

        public static DatabaseLogger GetLoggerByAddress(string connectionAddress, string procedureName)
        {
            return new DatabaseLogger(connectionAddress, procedureName);
        }
    }
}