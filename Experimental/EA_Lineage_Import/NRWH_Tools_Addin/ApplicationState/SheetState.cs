using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRWH_Tools_Addin.ApplicationState
{
    abstract class SheetState
    {
        public string SheetName { get; set; }
        public string EaPackagePath { get; set; }
    }
}
