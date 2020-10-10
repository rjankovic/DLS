using CD.DLS.Serialization;
using CD.DLS.API;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Parse.Mssql.Ssas;
using System.Data.SqlClient;
using CD.DLS.Parse.Mssql;
using CD.DLS.DAL.Managers;

namespace CD.DLS.Operations
{
    internal class ListReportElementsRequestProcessor : BIDocRequestProcessor<ListReportElementsRequest>
    {
        public ListReportElementsRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(ListReportElementsRequest request, ProjectConfig projectConfig)
        {
            var attachments = new List<Attachment>();
            var result = new ListReportElementsRequestResponse();
            var reports = InspectManager.ListModelReports(projectConfig.ProjectConfigId);
            result.Reports = reports.Select(x => new ListReportElementsRequestResponse.SsrsReportElement()
            {
                ItemPath = x.ItemPath,
                ModelElementId = x.ModelElementId,
                Name = x.Caption,
                ModelRefPath = x.RefPath
            }).ToList();

            var stringResult = result.Serialize();
        
            return new ProcessingResult()
            {
                Content = stringResult,
                Attachments = attachments
            };
        }
    }
}
