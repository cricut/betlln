using System.Collections.Generic;
using System.Data.SqlClient;

namespace Betlln.Data.Integration.Core
{
    public class ParameterSet : Dictionary<string, object>
    {
        public SqlParameter OutputParameter { get; set; }
    }
}