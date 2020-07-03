using System.Data;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.SqlServer
{
    public class SqlDataSource : DbDataSource
    {
        internal SqlDataSource()
        {
        }

        public string ProcedureName
        {
            get
            {
                return _commandText;
            }
            set
            {
                _commandType = CommandType.StoredProcedure;
                _commandText = value;
            }
        }
    }
}