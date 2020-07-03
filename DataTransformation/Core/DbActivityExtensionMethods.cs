using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Betlln.Data.Integration.Core
{
    internal static class DbActivityExtensionMethods
    {
        internal static void Execute(this ISqlActivity sqlActivity, string actualCommandText, CommandType commandType = CommandType.Text)
        {
            Execute(sqlActivity, actualCommandText, commandType, command => command.ExecuteNonQuery());
        }

        internal static T Execute<T>(this ISqlActivity sqlActivity, string actualCommandText, CommandType commandType, Func<DbCommand, T> commandExecutor)
        {
            using (DbConnection connection = sqlActivity.GetConnection())
            {
                using (DbCommand command = sqlActivity.BuildCommand(connection, actualCommandText, commandType))
                {
                    return commandExecutor(command);
                }
            }
        }

        internal static DbConnection GetConnection(this ISqlActivity sqlActivity)
        {
            return (DbConnection) sqlActivity.Connection.GetConnection();
        }

        internal static DbCommand BuildCommand(this ISqlActivity sqlActivity, DbConnection connection, string actualCommandText, CommandType commandType)
        {
            DbCommand command = connection.CreateCommand();
            command.CommandText = actualCommandText;
            command.CommandType = commandType;
            
            foreach (KeyValuePair<string, object> parameter in sqlActivity.Parameters)
            {
                DbParameter dbParameter = command.CreateParameter();
                dbParameter.ParameterName = parameter.Key;
                dbParameter.Value = parameter.Value;
                command.Parameters.Add(dbParameter);
            }

            if (sqlActivity.Parameters.OutputParameter != null)
            {
                command.Parameters.Add(sqlActivity.Parameters.OutputParameter);
            }

            int commandTimeout = int.MaxValue;
            if (sqlActivity.Timeout != 0)
            {
                commandTimeout = (int) (sqlActivity.Timeout * SystemExtensions.SecondsPerMinute);
            }
            command.CommandTimeout = commandTimeout;

            return command;
        }
    }
}