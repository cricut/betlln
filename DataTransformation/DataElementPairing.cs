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

        public DataElementPairing(string sourceName, string destinationName, TransformationKind transform)
        {
            SourceName = sourceName;
            DestinationName = destinationName;
            Transform = transform;
        }

        public string SourceName { get; }
        public string DestinationName { get; }
        public Type ElementType { get; }
        public int? MaximumLength { get; set; }
        public TransformationKind Transform { get; set; }
    }
}