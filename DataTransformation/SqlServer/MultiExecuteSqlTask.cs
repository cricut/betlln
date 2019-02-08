using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.SqlServer
{
    public class MultiExecuteSqlTask : ExecuteSqlCommandTask, IColumnMapper
    {
        internal MultiExecuteSqlTask()
        {
        }

        public DataFeed Source { get; set; }

        private string SourceName { get; set; }
        private string VariableParameterName { get; set; }

        public int ExecutionCount { get; private set; }

        public void MapColumns<T>(string sourceName, string parameterName)
        {
            SourceName = sourceName;
            VariableParameterName = parameterName;
        }

        protected override void ExecuteTasks()
        {
            foreach (DataRecord record in Source.GetReader())
            {
                Parameters.Add(VariableParameterName, record[SourceName]);
                base.ExecuteTasks();
                Parameters.Remove(VariableParameterName);

                ExecutionCount++;
            }
        }
    }
}