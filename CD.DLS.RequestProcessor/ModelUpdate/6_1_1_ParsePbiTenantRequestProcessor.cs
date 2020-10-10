using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.Parse.Mssql.Pbi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    class ParsePbiReportRequestProcessor : RequestProcessorBase, IRequestProcessor<ParsePbiTenantRequest, DLSApiMessage>
    {
        public DLSApiMessage Process(ParsePbiTenantRequest request, ProjectConfig projectConfig)
        {
            var itemIdx = request.ItemIndex;

            if (itemIdx >= request.TenantItems.Count)
            {
                return new DLSApiMessage();
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // RJ: This was meant to prevent service timeout - Azure functions can run for 10 minutes top, after that the process is terminated.
            // if there are many reports in the tenant, this limit could be reached (?). It has happened with SSRS and SSIS projects.
            // So, the the reports should be parsed one at a time, checking whether we're nearing the timeout after each
            // and if over half time is already spent, create a new contunutation request of the same type that will pick up where this one left off.
            // That is the purpose of the item index as it is applied in SSIS and SSRS.
            // Nevertheless, this is sufficient for now (we don't have any large report sets yet).
            do
            {
                PbiModelExtractor extractor = new PbiModelExtractor(projectConfig, StageManager, request.ExtractId, GraphManager, request.PbiComponentId, request.TenantRefPath);
                extractor.ParseModel();

                itemIdx++;

            } while (sw.ElapsedMilliseconds / 1000 < ConfigManager.ServiceTimeout / 2 && itemIdx < request.TenantItems.Count);

            if (itemIdx == request.TenantItems.Count)
            {
                return new DLSApiMessage();
            }
            else
            {
                return new DLSApiProgressResponse()
                {
                    ContinueWith = new ParsePbiTenantRequest()
                    {
                        ExtractId = request.ExtractId,
                        ItemIndex = itemIdx,//request.ItemIndex + 1,
                        TenantItems = request.TenantItems,
                        TenantRefPath = request.TenantRefPath,
                        PbiComponentId = request.PbiComponentId
                    }
                };
            }
        }
    }
}