using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;

namespace Betlln.Data
{
    public abstract class RedshiftDatabaseAdapter : DatabaseAdapter
    {
        protected RedshiftDatabaseAdapter(string connectionName)
            : base(connectionName)
        {
        }

        protected RedshiftDatabaseAdapter(ConnectionInfo dataSourceInfo)
             : base(dataSourceInfo)
        {
        }

        protected override string BuildConnectionAddressFrom(ConnectionInfo connectionInfo)
        {
            return $"Host={connectionInfo.Destination};Port=5439;Username={connectionInfo.User};Password={connectionInfo.Password};Database={connectionInfo.SubSectionName}";
        }

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
        
        private static async Task<List<T>> BuildObjectListAsync<T>(NpgsqlCommand command, Func<IDataReader, Task<T>> objectBuilder)
        {
            List<T> list = new List<T>();

            using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    list.Add(await objectBuilder(reader));
                }
            }

            return list;
        }

        protected List<T> ExecuteQueryByString<T>(string sqlAsString, Func<IDataReader, T> objectBuilder)
        {
            return ExecuteQuery(ConnectionAddress, sqlAsString, command => BuildObjectList(command, objectBuilder));
        }
        
        protected async Task<List<T>> ExecuteQueryByStringAsync<T>(string sqlAsString, Func<IDataReader, Task<T>> objectBuilder)
        {
            return await ExecuteQueryAsync(ConnectionAddress, sqlAsString, command => BuildObjectListAsync(command, objectBuilder));
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
        
        // ReSharper disable once TooManyArguments
        private static async Task<T> ExecuteQueryAsync<T>(string connectionAddress, string sqlAsString, Func<NpgsqlCommand, Task<T>> action)
        {
            using (var connection = new NpgsqlConnection(connectionAddress))
            {
                using (NpgsqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sqlAsString;
                    command.CommandType = CommandType.Text;

                    await connection.OpenAsync();

                    return await action(command);
                }
            }
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