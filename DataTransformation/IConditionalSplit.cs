using System;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration
{
    internal interface IConditionalSplit : IDisposable
    {
        DataFeed Source { get; set; }
        void DefineOutput(string outputName, Func<DataRecord, bool> filter);
        void DefineOutputsBy(string columnName, bool excludeColumn = false);
        DataFeed Output(string outputName);
    }
}