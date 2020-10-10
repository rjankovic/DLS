using CD.DLS.API.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.BusinessObjects
{
    public class GetPivotTableTemplateRequest : DLSApiRequest<GetPivotTableTemplateRequestResponse>
    {
        public string PivotTableTemplateRefPath { get; set; }
    }

    public class GetPivotTableTemplateRequestResponse : DLSApiMessage
    {
        public PivotTableStructure Structure { get; set; }
    }
}
