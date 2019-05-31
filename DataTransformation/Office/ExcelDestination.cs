using Betlln.Data.Integration.Core;

namespace Betlln.Data.Integration.Office
{
    public class ExcelDestination : Task
    {
        public DataSource DataSource { get; set; }
        public string OutputFileName { get; set; }  //TODO: validate file extension
        public string DestinationSheetName { get; set; }

        protected override void ExecuteTasks()
        {
            throw new System.NotImplementedException();
        }
    }
}