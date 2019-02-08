using System.Data;
using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.SqlServer
{
    public class ExecuteSqlCommandTask : Task, ISqlActivity
    {
        private CommandType _commandType;
        private string _commandText;

        internal ExecuteSqlCommandTask()
        {
            Parameters = new ParameterSet();
        }

        public IConnectionManager Connection { get; set; }

        public string CommandText
        {
            get
            {
                return _commandText;
            }
            set
            {
                _commandType = CommandType.Text;
                _commandText = value;
            }
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

        public ParameterSet Parameters { get; }

        protected override void ExecuteTasks()
        {
            this.Execute(_commandText, _commandType);
        }
    }
}