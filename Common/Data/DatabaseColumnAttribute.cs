using System;
using System.Linq;
using System.Reflection;

namespace Betlln.Data
{
    public class DatabaseColumnAttribute : Attribute
    {
        public DatabaseColumnAttribute(string columnName, int order)
        {
            ColumnName = columnName;
            Order = order;
        }

        public string ColumnName { get; }
        public int Order { get; }

        public PropertyInfo Parent { get; private set; }

        public static DatabaseColumnAttribute ClrPropertyToDatabaseColumnAttribute(PropertyInfo dataProperty)
        {
            return dataProperty
                .GetCustomAttributes(typeof(DatabaseColumnAttribute), true)
                .Cast<DatabaseColumnAttribute>()
                .Select(attribute =>
                {
                    attribute.Parent = dataProperty;
                    return attribute;
                })
                .FirstOrDefault();
        }
    }
}
