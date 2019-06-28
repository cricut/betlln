using System.Collections.Generic;
using Betlln.Data.Integration.Core;
using Betlln.Logging;

namespace Betlln.Data.Integration.Office
{
    public class ExcelSheetDirective
    {
        private readonly Dictionary<string, ExcelColumn> _columnSettings;

        public ExcelSheetDirective()
        {
            _columnSettings = new Dictionary<string, ExcelColumn>();
            WriteEmptyTable = true;
            Dts.Notify.Log($"Started {GetType().Name}", LogEventType.Debug);
        }

        public DataFeed DataSource { get; set; }
        public string DestinationSheetName { get; set; }
        public bool WriteEmptyTable { get; set; }

        public IEnumerable<string> CustomColumnLetters
        {
            get { return _columnSettings.Keys; }
        }

        public ExcelColumn Column(string columnLetter)
        {
            columnLetter = columnLetter.ToUpper().Trim();
            if (!_columnSettings.ContainsKey(columnLetter))
            {
                _columnSettings.Add(columnLetter, new ExcelColumn());
            }

            return _columnSettings[columnLetter];
        }
    }
}