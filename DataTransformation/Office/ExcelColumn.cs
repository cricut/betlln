using Betlln.Data.Integration.Core;
using ClosedXML.Excel;

namespace Betlln.Data.Integration.Office
{
    public class ExcelColumn
    {
        public ExcelColumn()
        {
            HorizontalAlignment = XLAlignmentHorizontalValues.General;
        }

        public XLAlignmentHorizontalValues HorizontalAlignment { get; set; }
        public string Format { get; set; }
        public XLTotalsRowFunction? TotalFunction { get; set; }
    }
}