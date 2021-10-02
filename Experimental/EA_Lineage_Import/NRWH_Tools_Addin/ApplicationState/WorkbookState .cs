using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRWH_Tools_Addin.ApplicationState
{
    class WorkbookState
    {
        public WorkbookState()
        {
            Sheets = new List<SheetState>();
        }
        public List<SheetState> Sheets { get; }
    }
}
