using CD.DLS.API.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.Query
{
    public class ReportItemPositionsRequest : DLSApiRequest<ReportItemPositionsResponse>
    {
        //public string ReportPath { get; set; }
        public int ReportElementId { get; set; }
    }

    public class ReportItemPositionsResponse : DLSApiMessage
    {
        public ReportElementAbsolutePosition RootElement { get; set; }
    }
}
