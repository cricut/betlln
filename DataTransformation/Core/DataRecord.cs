using System;
using System.Collections.Generic;

namespace Betlln.Data.Integration.Core
{
    public class DataRecord
    {
        private class DataElement
        {
            public DataElement(string columnName, object value)
            {
                ColumnName = columnName;
                Value = value;
            }

            public string ColumnName { get; }
            public object Value { get; set; }
        }

        private readonly List<DataElement> _elements;
        private readonly Dictionary<string, int> _nameMap;

        public DataRecord()
        {
            _elements = new List<DataElement>();
            _nameMap = new Dictionary<string, int>();
        }

        public object this[int i]
        {
            get
            {
                return _elements[i].Value;
            }
            set
            {
                if (i < 0 || i >= _elements.Count)
                {
                    throw new IndexOutOfRangeException();
                }

                _elements[i].Value = value;
            }
        }

        public object this[string name]
        {
            get
            {
                int i = GetOrdinal(name);
                return i < 0 ? null : _elements[i].Value;
            }
            set
            {
                int i = GetOrdinal(name);

                if (i < 0)
                {
                    _elements.Add(new DataElement(name, null));
                    i = _elements.Count - 1;
                    _nameMap.Add(name.ToLower(), i);
                }

                _elements[i].Value = value;
            }
        }

        private int GetOrdinal(string name)
        {
            string key = name.ToLower();
            return !_nameMap.ContainsKey(key) ? -1 : _nameMap[key];
        }

        public List<ColumnInfo> GetLayout()
        {
            List<ColumnInfo> columns = new List<ColumnInfo>();
            foreach (DataElement dataElement in _elements)
            {
                Type type = dataElement.Value == null ? typeof(string) : dataElement.Value.GetType();
                ColumnInfo columnInfo = new ColumnInfo(dataElement.ColumnName, type);
                columns.Add(columnInfo);
            }
            return columns;
        }
    }
}