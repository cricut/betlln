using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.SqlServer
{
    internal class TableMetaDataCache
    {
        private readonly ConcurrentDictionary<string, DataTable> _cache;

        private TableMetaDataCache()
        {
            _cache = new ConcurrentDictionary<string, DataTable>();
        }

        private class CacheKey
        {
            public CacheKey(SqlConnectionManager connection, string tableAddress)
            {
                Connection = connection;
                TableAddress = tableAddress;
            }

            public SqlConnectionManager Connection { get; }
            public string TableAddress { get; }

            public override string ToString()
            {
                return Connection.ConnectionAddress + "::" + TableAddress;
            }
        }

        public int? GetStringMaximumLength(SqlConnectionManager connectionManager, string tableAddress, string columnName)
        {
            CacheKey key = new CacheKey(connectionManager, tableAddress);

            Load(key);
            
            if (_cache.ContainsKey(key.ToString()))
            {
                DataTable metaData = _cache[key.ToString()];
                DataRow targetColumnInfo = metaData.Select($"column_name = \'{columnName}\'").FirstOrDefault();
                if (targetColumnInfo != null && targetColumnInfo["max_chars"] != DBNull.Value)
                {
                    return (int) targetColumnInfo["max_chars"];
                }
            }

            return null;
        }

        private void Load(CacheKey key)
        {
            if (!_cache.ContainsKey(key.ToString()))
            {
                Match match = Regex.Match(key.TableAddress.Trim(), @"^(\[?(?'schema'.+?)\]?\.)?\[?(?'object'.+?)\]?$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    DataTable metaData = ReadMetaData(key.Connection, match.Groups["schema"].Value, match.Groups["object"].Value);
                    _cache.AddOrUpdate(key.ToString(), surrogateKey => metaData, delegate { return metaData; });
                }
            }
        }

        private DataTable ReadMetaData(IConnectionManager connectionManager, string schemaName, string tableName)
        {
            DataTable metaData;

            using (SqlDataSource metaDataQuery = new SqlDataSource())
            {
                metaDataQuery.Connection = connectionManager;
                metaDataQuery.CommandText = @"
                    SELECT 
                        columns.[name] AS column_name,
                        types.[name] AS data_type_name,
                        CASE WHEN types.[name] LIKE '%char' AND columns.max_length > 0 
                             THEN columns.max_length 
                              / CASE WHEN types.[name] LIKE 'n%' THEN 2 ELSE 1 END
                             END AS max_chars
                    FROM sys.schemas
                        INNER JOIN sys.tables
                            ON schemas.schema_id = tables.schema_id
                        INNER JOIN sys.columns
                            ON tables.object_id = columns.object_id
                        INNER JOIN sys.types
                            ON columns.user_type_id = types.user_type_id
                    WHERE 
                        schemas.[name] = @SchemaName
                        AND
                        tables.[name] = @ObjectName";
                metaDataQuery.Parameters.Add("@SchemaName", schemaName);
                metaDataQuery.Parameters.Add("@ObjectName", tableName);
                metaData = metaDataQuery.GetResults();
            }

            return metaData;
        }

        private static TableMetaDataCache _default;
        internal static TableMetaDataCache Default
        {
            get
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (_default == null)
                {
                    _default = new TableMetaDataCache();
                }
                return _default;
            }
        }
    }
}