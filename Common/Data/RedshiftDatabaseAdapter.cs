using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;

namespace Betlln.Data
{
    public abstract class RedshiftDatabaseAdapter
    {
        protected RedshiftDatabaseAdapter(string connectionName)
        {
            ConnectionAddress = GetConnectionAddressByName(connectionName);
        }

        internal static string GetConnectionAddressByName(string connectionName)
        {
            return ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
        }

        protected string ConnectionAddress { get; }

        protected static DataTable BuildDataTable(NpgsqlCommand command)
        {
            DataSet container = new DataSet();
            using (var adapter = new NpgsqlDataAdapter(command))
            {
                adapter.Fill(container);
            }
            return container.Tables[0];
        }

        private static List<T> BuildObjectList<T>(NpgsqlCommand command, Func<IDataReader, T> objectBuilder)
        {
            List<T> list = new List<T>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(objectBuilder(reader));
                }
            }

            return list;
        }

        protected List<T> ExecuteQueryByString<T>(string sqlAsString, Func<IDataReader, T> objectBuilder)
        {
            return ExecuteQuery(ConnectionAddress, sqlAsString, command => BuildObjectList(command, objectBuilder));
        }

        public void ExecuteNonQueryByString(string sqlAsString)
        {
            ExecuteQuery(ConnectionAddress, sqlAsString, command => command.ExecuteNonQuery());
        }

        // ReSharper disable once TooManyArguments
        private static T ExecuteQuery<T>(string connectionAddress, string sqlAsString, Func<NpgsqlCommand, T> action)
        {
            using (var connection = new NpgsqlConnection(connectionAddress))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlAsString;
                    command.CommandType = CommandType.Text;

                    connection.Open();

                    return action(command);
                }
            }
        }

        protected static T? ReadNullableValue<T>(IDataReader reader, string columnName)
            where T : struct
        {
            return (T?)(reader.IsDBNull(reader.GetOrdinal(columnName))
                ? null
                : reader[columnName]);
        }

        protected static string ReadString(IDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName))
                ? null
                : reader[columnName].ToString();
        }

        public static NpgsqlConnection OpenDatabaseConnection(string connectionName)
        {
            var connectionSettings = ConfigurationManager.ConnectionStrings[connectionName];
            return OpenConnectionFromAddress(connectionSettings.ConnectionString);
        }

        protected static NpgsqlConnection OpenConnectionFromAddress(string connectionAddress)
        {
            var connection = new NpgsqlConnection(connectionAddress);
            connection.Open();
            return connection;
        }
    }
}