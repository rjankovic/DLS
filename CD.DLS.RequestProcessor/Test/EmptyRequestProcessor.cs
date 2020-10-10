using CD.DLS.API;
using CD.DLS.API.Test;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor.Test
{
    class EmptyRequestProcessor : RequestProcessorBase, IRequestProcessor<EmptyRequest, EmptyResponse>
    {
        public EmptyResponse Process(EmptyRequest request, ProjectConfig projectConfig)
        {
            ProjectConfigManager.ListProjectConfigs();
            return new EmptyResponse() { Message = "Request processed: " + DateTime.Now.ToLongDateString() };
        }
    }
}
