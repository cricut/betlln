using System.Data;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.AWS
{
    public class RedshiftDataSource : DbDataSource
    {
        internal RedshiftDataSource()
        {
            _commandType = CommandType.Text;
        }

        public string QueryText
        {
            get { return _commandText; }
            set { _commandText = value; }
        }
    }
}