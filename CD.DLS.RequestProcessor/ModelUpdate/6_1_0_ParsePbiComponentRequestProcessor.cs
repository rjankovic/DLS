using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Objects.Extract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    class ParsePbiComponentRequestProcessor : RequestProcessorBase, IRequestProcessor<ParsePbiComponentRequest, DLSApiProgressResponse>
    {        
        public DLSApiProgressResponse Process(ParsePbiComponentRequest request, ProjectConfig projectConfig)
        {
            List<DLSApiMessage> parseReportRequests = new List<DLSApiMessage>();

            var tenants = StageManager.GetExtractItems(request.ExtractId, request.PbiComponentId, ExtractTypeEnum.Tenant);

            List<ParsePbiTenantItem> reportItems = new List<ParsePbiTenantItem>();

            foreach (var tenant in tenants)
            {
                reportItems.Add(new ParsePbiTenantItem() {ExtractItemId = tenant.ExtractItemId});
            }

            return new DLSApiProgressResponse()
            {
                ContinueWith = new ParsePbiTenantRequest()
                {
                    ExtractId = request.ExtractId,
                    ItemIndex = 0,
                    TenantItems = reportItems,
                    TenantRefPath = request.TenantRefPath,
                    PbiComponentId = request.PbiComponentId
                }
            };
        }
    }
}
