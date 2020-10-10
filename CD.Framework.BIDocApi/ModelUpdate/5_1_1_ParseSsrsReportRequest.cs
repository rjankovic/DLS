using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.ModelUpdate
{
    /// <summary>
    /// WF pos: 5-1-1
    /// </summary>
    public class ParseSsrsReportRequest : DLSApiRequest<DLSApiMessage>
    {
        public Guid ExtractId { get; set; }
        public int ItemIndex { get; set; }
        public string ServerRefPath { get; set; }
        public List<ParseSsrsReportItem> Reports { get; set; }
        public int SsrsComponentId { get; set; }
    }

    public class ParseSsrsReportItem
    {
        public int ExtractItemId { get; set; }
    }
}
