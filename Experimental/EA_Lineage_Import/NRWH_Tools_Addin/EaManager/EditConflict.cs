using NRWH_Tools_Addin.ApplicationState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRWH_Tools_Addin.EaManager
{
    class EditConflict
    {
        public RowState LocalRow { get; set; }
        public RowState EaRow { get; set; }
    }
}
