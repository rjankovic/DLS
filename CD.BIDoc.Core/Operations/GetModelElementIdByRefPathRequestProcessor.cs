using CD.DLS.API;
using CD.DLS.Common.Structures;
using System.Collections.Generic;
using CD.DLS.DAL.Managers;

namespace CD.DLS.Operations
{
    internal class GetModelElementIdByRefPathRequestProcessor : BIDocRequestProcessor<GetModelElementIdByRefPathRequest>
    {
        public GetModelElementIdByRefPathRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(GetModelElementIdByRefPathRequest request, ProjectConfig projectConfig)
        {
            int elementId = GraphManager.GetModelElementIdByRefPath(projectConfig.ProjectConfigId, request.RefPath);
            GetModelElementIdByRefPathRequestResponse result = new GetModelElementIdByRefPathRequestResponse()
            {
                ModelElementId = elementId
            };

            var attachments = new List<Attachment>();
            var stringResult = result.Serialize();
            return new ProcessingResult()
            {
                Content = stringResult,
                Attachments = attachments
            };
        }
    }
}
