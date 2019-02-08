using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Betlln.Data
{
    public abstract class SqlDatabaseAdapter : IDisposable
    {
        protected SqlDatabaseAdapter(string connectionName)
        {
            ConnectionAddress = GetConnectionAddressByName(connectionName);
        }

        internal static string GetConnectionAddressByName(string connectionName)
        {
            string connectionAddress = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            SqlConnectionStringBuilder addressBuilder = new SqlConnectionStringBuilder(connectionAddress);
            if (string.IsNullOrWhiteSpace(addressBuilder.ApplicationName))
            {
                addressBuilder.ApplicationName = RuntimeContext.ApplicationAndVersion;
            }
            return addressBuilder.ToString();
        }

        private string ConnectionAddress { get; }
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

        protected DataTable ExecuteQueryStoredProcedure(string storedProcedure)
        {
            return ExecuteStoredProcedure(ConnectionAddress, storedProcedure, null, BuildDataTable);
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

        protected static T? ReadNullableValue<T>(IDataReader reader, string columnName)
            where T : struct 
        {
            return (T?) (reader.IsDBNull(reader.GetOrdinal(columnName))
                ? null
                : reader[columnName]);
        }

        protected static string ReadString(IDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName))
                ? null
                : reader[columnName].ToString();
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