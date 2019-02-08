using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Betlln.Data.Integration.SqlServer
{
    internal static class SqlActivityExtensionMethods
    {
        internal static void Execute(this ISqlActivity sqlActivity, string commandText, CommandType commandType = CommandType.Text)
        {
            Execute(sqlActivity, commandText, commandType, command => command.ExecuteNonQuery());
        }

        // ReSharper disable once TooManyArguments
        internal static T Execute<T>(this ISqlActivity sqlActivity, string commandText, CommandType commandType, Func<SqlCommand, T> commandExecutor)
        {
            int commandTimeout = int.MaxValue;
            if (sqlActivity.Timeout != 0)
            {
                commandTimeout = (int) (sqlActivity.Timeout * SystemExtensions.SecondsPerMinute);
            }

            using (SqlConnection connection = (SqlConnection) sqlActivity.Connection.GetConnection())
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    command.CommandType = commandType;

                    foreach (KeyValuePair<string, object> parameter in sqlActivity.Parameters)
                    {
                        command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                    if (sqlActivity.Parameters.OutputParameter != null)
                    {
                        command.Parameters.Add(sqlActivity.Parameters.OutputParameter);
                    }

                    command.CommandTimeout = commandTimeout;

                    return commandExecutor(command);
                }
            }
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