using System;

namespace Betlln.Data.Integration
{
    public class DataElementPairing
    {
        public DataElementPairing(string sourceName, string destinationName, Type elementType)
        {
            SourceName = sourceName;
            DestinationName = destinationName;
            ElementType = elementType;
        }

        public string SourceName { get; }
        public string DestinationName { get; }
        public Type ElementType { get; }
        public int? MaximumLength { get; set; }
    }
}