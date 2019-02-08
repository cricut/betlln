namespace Betlln.Data.Integration.Core
{
    public interface IColumnMapper
    {
        void MapColumns<T>(string sourceName, string outputAlias);
    }
}