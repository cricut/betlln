namespace Betlln.Data.File
{
    public class DataCell
    {
        public DataCell(uint columnNumber, object value)
        {
            ColumnNumber = columnNumber;
            Value = value;
        }

        public uint ColumnNumber { get; }
        public object Value { get; }

        public string TextValue
        {
            get { return Value?.ToString() ?? string.Empty; }
        }
    }
}