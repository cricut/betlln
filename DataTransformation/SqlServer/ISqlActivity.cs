using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.SqlServer
{
    public interface ISqlActivity
    {
        IConnectionManager Connection { get; set; }
        uint Timeout { get; set; }
        string CommandText { get; set; }
        ParameterSet Parameters { get; }
    }
}