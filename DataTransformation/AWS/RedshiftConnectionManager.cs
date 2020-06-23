using System;
using System.Diagnostics.CodeAnalysis;
using Betlln.Data.Integration.Core;
using Npgsql;

namespace Betlln.Data.Integration.AWS
{
    [SuppressMessage("ReSharper", "HollowTypeName")]
    public class RedshiftConnectionManager : IConnectionManager, IDatabaseConnection
    {
        private const int DefaultPort = 5439;

        public IDisposable GetConnection()
        {
            NpgsqlConnectionStringBuilder connectionAddressBuilder = new NpgsqlConnectionStringBuilder();
            connectionAddressBuilder.Host = ServerName;
            connectionAddressBuilder.Port = DefaultPort;
            connectionAddressBuilder.Username = Username;
            connectionAddressBuilder.Password = Password;
            connectionAddressBuilder.Database = DatabaseName;
            connectionAddressBuilder.ApplicationName = ApplicationName;

            NpgsqlConnection connection = new NpgsqlConnection(connectionAddressBuilder.ConnectionString);
            connection.Open();
            return connection;
        }

        public string ServerName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DatabaseName { get; set; }
        public string ApplicationName { get; set; }
    }
}