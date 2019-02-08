namespace Betlln.Data.Integration.Core
{
    public abstract class DataSource : DataFeed
    {
        public virtual IConnectionManager Connection { get; set; }
    }
}