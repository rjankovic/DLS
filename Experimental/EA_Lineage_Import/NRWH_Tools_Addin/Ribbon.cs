using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using NRWH_Tools_Addin.ApplicationState;
using NRWH_Tools_Addin.EaManager;
using NRWH_Tools_Addin.ExcelManager;
using Xls = Microsoft.Office.Interop.Excel;

namespace NRWH_Tools_Addin
{
    public partial class Ribbon
    {
        private WorkbookState _currentState = null;
        private ExcelOperations _excelOps;
        private EaOperations _eaOps;

        private void Ribbon_Load(object sender, RibbonUIEventArgs e)
        {
            Globals.ThisAddIn.Application.SheetChange += Application_SheetChange;
            var repoName = "Enterprise_Architect_NOIS";
            //Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=Enterprise_Architect_NOIS;Data Source=fsczprsa0010;

            //nw ea
            //Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=Enterprise_Architect_NOIS;Data Source=fsczprsa0010;
            var connectionString = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=Enterprise_Architect_NOIS;Data Source=fsczprsa0010;";
            _excelOps = new ExcelOperations();
            _eaOps = new EaOperations(connectionString, repoName);
        }

        private void Application_SheetChange(object sh, Xls.Range target)
        {
            var sheet = sh as Xls.Worksheet;
            if (sheet == null)
            {
                return;
            }
            _excelOps.OnSheetChanged(_currentState, sheet, target);
        }

        private void btnSynchronize_Click(object sender, RibbonControlEventArgs e)
        {
            var workbook = Globals.ThisAddIn.Application.ActiveWorkbook;
            _currentState = _excelOps.UpdateWorkbookState(workbook, _currentState);
            List<EditConflict> conflicts;
            _eaOps.SynchronizeWorkbookState(_currentState, out conflicts);
            if (conflicts.Any())
            {
                throw new NotImplementedException();
            }
            _excelOps.UpdateLocalWorkbook(workbook, _currentState);
        }
    }
}
