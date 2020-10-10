using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.Render
{
    public class RenderReportRequest : DLSApiRequest<RenderReportResponse>
    {
        public string ReportPath { get; set; }
        public int ModelElementId { get; set; }

        public enum ReportFormatEnum
        {
            Xml, Nhtml, Excel, ReportDataMap
        }

        public ReportFormatEnum ReportFormat { get; set; }

        public List<ReportParameterValue> ParameterValues { get; set; }

        public class ReportParameterValue
        {
            public string ParameterName { get; set; }
            public string ParameterValue { get; set; }
        }
    }

    public class RenderReportResponse : DLSApiMessage
    {
    }
}
