using System.Collections.Generic;

namespace Betlln.Data.Integration.Core
{
    public interface IDataRecordIterator : IEnumerable<DataRecord>, IEnumerator<DataRecord>
    {
    }
}