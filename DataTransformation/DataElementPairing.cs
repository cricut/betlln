using System;
using Betlln.Data.Integration.Core;

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

        public DataElementPairing(string sourceName, string destinationName, AggregationRole aggregationRole)
        {
            SourceName = sourceName;
            DestinationName = destinationName;
            AggregationRole = aggregationRole;
        }

        public string SourceName { get; }
        public string DestinationName { get; }
        public Type ElementType { get; }
        public int? MaximumLength { get; set; }
        public AggregationRole AggregationRole { get; set; }
    }
}