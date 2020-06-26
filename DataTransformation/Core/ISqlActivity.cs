namespace Betlln.Data.Integration.Core
{
    public interface ISqlActivity
    {
        IConnectionManager Connection { get; set; }
        uint Timeout { get; set; }
        string CommandText { get; set; }
        ParameterSet Parameters { get; }
    }
}