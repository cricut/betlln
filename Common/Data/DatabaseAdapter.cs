using System.Configuration;

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
    }
}