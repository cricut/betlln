using System.Collections.Generic;
using System.Data.SqlClient;

namespace Betlln.Data.Integration.SqlServer
{
    public class ParameterSet : Dictionary<string, object>
    {
        public SqlParameter OutputParameter { get; set; }
    }
}