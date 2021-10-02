using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRWH_Tools_Addin.ApplicationState
{
    class ColumnMapping
    {
        public int ColumnIndex { get; set; }
        public string EaAttributeGroup { get; set; }
        public string EaAttribute { get; set; }
        public string OutboundLinkStereotype { get; set; }
        public bool IsId { get; set; }
    }
}
