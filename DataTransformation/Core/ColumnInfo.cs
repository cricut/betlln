using System;

namespace Betlln.Data.Integration.Core
{
    public class ColumnInfo
    {
        public ColumnInfo(string name, Type dataType)
        {
            Name = name;
            DataType = dataType;
        }

        public string Name { get; }
        public Type DataType { get; }
    }
}