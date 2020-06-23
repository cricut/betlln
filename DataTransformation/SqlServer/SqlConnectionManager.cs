using System;
using System.Data.SqlClient;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.SqlServer
{
    // ReSharper disable once HollowTypeName
    public class SqlConnectionManager : IConnectionManager, IDatabaseConnection
    {
        public IDisposable GetConnection()
        {
            SqlConnection connection = new SqlConnection(ConnectionAddress);
            connection.Open();
            return connection;
        }

        public string ServerName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DatabaseName { get; set; }
        public string ApplicationName { get; set; }

        internal string ConnectionAddress
        {
            get
            {
                return $"Data Source={ServerName};Initial Catalog={DatabaseName};Integrated Security=SSPI;Application Name={ApplicationName}";
            }
        }
    }
}