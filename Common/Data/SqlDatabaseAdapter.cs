using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Betlln.Data
{
    public abstract class SqlDatabaseAdapter : DatabaseAdapter, IDisposable
    {
        protected SqlDatabaseAdapter(ConnectionInfo connectionInfo) 
            : base(connectionInfo)
        {
            SetApplicationName();
        }

        protected SqlDatabaseAdapter(string connectionName) 
            : base(connectionName)
        {
            SetApplicationName();
        }

        private void SetApplicationName()
        {
            SqlConnectionStringBuilder addressBuilder = new SqlConnectionStringBuilder(ConnectionAddress);
            if (string.IsNullOrWhiteSpace(addressBuilder.ApplicationName))
            {
                addressBuilder.ApplicationName = RuntimeContext.ApplicationAndVersion;
            }

            ConnectionAddress = addressBuilder.ToString();
        }

        protected override string BuildConnectionAddressFrom(ConnectionInfo connectionInfo)
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.DataSource = connectionInfo.Destination;
            connectionStringBuilder.InitialCatalog = connectionInfo.SubSectionName;
            connectionStringBuilder.UserID = connectionInfo.User;
            connectionStringBuilder.Password = connectionInfo.Password;
            connectionStringBuilder.ApplicationName = RuntimeContext.ApplicationAndVersion;
            connectionStringBuilder.ConnectTimeout = 30;
            return connectionStringBuilder.ToString();
        }
        
        public SqlTransaction Transaction { get; private set; }

        public void BeginTransaction()
        {
            if (Transaction != null)
            {
                throw new InvalidOperationException("A transaction is already running.");
            }

            SqlConnection connection = OpenConnectionFromAddress(ConnectionAddress);
            Transaction = connection.BeginTransaction();
        }

        protected DataTable ExecuteQueryStoredProcedure(string storedProcedure, IEnumerable<SqlParameter> parameters = null)
        {
            return ExecuteStoredProcedure(ConnectionAddress, storedProcedure, parameters, BuildDataTable);
        }

        private static DataTable BuildDataTable(SqlCommand command)
        {
            DataSet container = new DataSet();
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(container);
            }
            return container.Tables[0];
        }

        protected List<T> ExecuteQueryStoredProcedure<T>(string procedureName, IEnumerable<SqlParameter> parameters, Func<IDataReader, T> objectBuilder)
        {
            return ExecuteStoredProcedure(ConnectionAddress, procedureName, parameters, command => BuildObjectList(command, objectBuilder));
        }

        private static List<T> BuildObjectList<T>(SqlCommand command, Func<IDataReader, T> objectBuilder)
        {
            List<T> list = new List<T>();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(objectBuilder(reader));
                }
            }

            return list;
        }

        protected void ExecuteNonQueryStoredProcedure(string procedureName, params SqlParameter[] parameters)
        {
            ExecuteNonQueryStoredProcedure(ConnectionAddress, procedureName, parameters);
        }

        public static void ExecuteNonQueryStoredProcedure(string connectionAddress, string procedureName, params SqlParameter[] parameters)
        {
            ExecuteStoredProcedure(connectionAddress, procedureName, parameters, command => command.ExecuteNonQuery());
        }

        // ReSharper disable once TooManyArguments
        private static T ExecuteStoredProcedure<T>(string connectionAddress, string procedureName, IEnumerable<SqlParameter> parameters, Func<SqlCommand, T> action)
        {
            using (SqlConnection connection = new SqlConnection(connectionAddress))
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = procedureName;
                    command.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        foreach (SqlParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }

                    connection.Open();

                    return action(command);
                }
            }
        }
        
        protected async Task<List<T>> ExecuteDirectQueryAsync<T>(string query, Func<IDataRecord, T> builder)
        {
            List<T> list = new List<T>();

            await using (SqlConnection connection = new SqlConnection(ConnectionAddress))
            {
                await using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    await command.Connection.OpenAsync();
                    await using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(builder(reader));
                        }
                    }
                }
            }

            return list;
        }

        protected SqlConnection OpenDatabaseConnection()
        {
            if (Transaction != null)
            {
                return Transaction.Connection;
            }

            return OpenConnectionFromAddress(ConnectionAddress);
        }

        public static SqlConnection OpenDatabaseConnection(string connectionName)
        {
            ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings[connectionName];
            return OpenConnectionFromAddress(connectionSettings.ConnectionString);
        }

        private static SqlConnection OpenConnectionFromAddress(string connectionAddress)
        {
            SqlConnection connection = new SqlConnection(connectionAddress);
            connection.Open();
            return connection;
        }

        public void RollbackTransaction()
        {
            EndTransaction(transaction => transaction.Rollback());
        }

        public void CommitTransaction()
        {
            EndTransaction(transaction => transaction.Commit());
        }

        private void EndTransaction(Action<SqlTransaction> transactionAction)
        {
            if (Transaction == null)
            {
                throw new InvalidOperationException("There is no current transaction.");
            }

            transactionAction(Transaction);

            Transaction.Connection?.Dispose();
            Transaction.Dispose();
            Transaction = null;
        }

        public void Dispose()
        {
            if (Transaction != null)
            {
                RollbackTransaction();
            }
        }
    }
}