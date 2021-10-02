using Xls = Microsoft.Office.Interop.Excel;
using NRWH_Tools_Addin.ApplicationState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRWH_Tools_Addin.ExcelManager
{
    static class WorkbookDetector
    {
        public static WorkbookState Detect(Xls.Workbook workbook)
        {
            Xls.Worksheet sheet = workbook.ActiveSheet;
            WorkbookState state = new WorkbookState();
            state.Sheets.Add(SheetDetector.Detect(sheet));
            return state;
        }
    }
}
