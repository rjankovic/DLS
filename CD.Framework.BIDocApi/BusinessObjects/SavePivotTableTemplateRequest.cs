using CD.DLS.API.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.BusinessObjects
{
    public class SavePivotTableTemplateRequest : DLSApiRequest<SavePivotTableTemplateRequestResponse>
    {
        public string FolderRefPath { get; set; }
        public string PivotTableName { get; set; }
        public PivotTableStructure Structure { get; set; }
    }

    public class SavePivotTableTemplateRequestResponse : DLSApiMessage
    {
        public string PivotTableTemplateRefPath { get; set; }
    }
}
