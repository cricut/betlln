using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Betlln.Data.Integration.SqlServer
{
    internal static class SqlActivityExtensionMethods
    {
        internal static void Execute(this ISqlActivity sqlActivity, string actualCommandText, CommandType commandType = CommandType.Text)
        {
            Execute(sqlActivity, actualCommandText, commandType, command => command.ExecuteNonQuery());
        }

        internal static T Execute<T>(this ISqlActivity sqlActivity, string actualCommandText, CommandType commandType, Func<SqlCommand, T> commandExecutor)
        {
            using (SqlConnection connection = sqlActivity.GetConnection())
            {
                using (SqlCommand command = sqlActivity.BuildCommand(connection, actualCommandText, commandType))
                {
                    return commandExecutor(command);
                }
            }
        }

        internal static SqlConnection GetConnection(this ISqlActivity sqlActivity)
        {
            return (SqlConnection) sqlActivity.Connection.GetConnection();
        }

        internal static SqlCommand BuildCommand(this ISqlActivity sqlActivity, SqlConnection connection, string actualCommandText, CommandType commandType)
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandText = actualCommandText;
            command.CommandType = commandType;
            
            foreach (KeyValuePair<string, object> parameter in sqlActivity.Parameters)
            {
                command.Parameters.AddWithValue(parameter.Key, parameter.Value);
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

        internal static DataTable GetDataTable(this SqlCommand command)
        {
            DataSet container = new DataSet();

            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
            {
                dataAdapter.Fill(container);
            }

            return container.Tables[0];
        }
    }
}