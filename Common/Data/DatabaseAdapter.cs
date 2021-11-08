using System.Configuration;
using System.Data;

namespace Betlln.Data
{
    public abstract class DatabaseAdapter
    {
        protected DatabaseAdapter(ConnectionInfo connectionInfo)
        {
            ConnectionAddress = BuildConnectionAddressFrom(connectionInfo);
        }

        protected abstract string BuildConnectionAddressFrom(ConnectionInfo connectionInfo);

        protected DatabaseAdapter(string connectionName)
        {
            ConnectionAddress = GetConnectionAddressByName(connectionName);
        }

        public static string GetConnectionAddressByName(string connectionName)
        {
            return ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
        }

        protected string ConnectionAddress { get; set; }
        
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
    }
}