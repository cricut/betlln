using System;
using Betlln.Data.Integration.Collections;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration
{
    public class MetaDataFeed : IDisposable
    {
        internal MetaDataFeed()
        {
        }

        public DataFeed Source { get; set; }

        public ListOf<string> ColumnNames
        {
            get { return Source.GetResults().GetColumnNames(); }
        }

        public void Dispose()
        {
        }
    }
}