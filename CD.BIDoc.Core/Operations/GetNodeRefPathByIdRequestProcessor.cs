using CD.DLS.API;
using CD.DLS.Common.Structures;
using System.Collections.Generic;

namespace CD.DLS.Operations
{
    internal class GetNodeRefPathByIdRequestProcessor : BIDocRequestProcessor<GetNodeRefPathByIdRequest>
    {
        public GetNodeRefPathByIdRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(GetNodeRefPathByIdRequest request, ProjectConfig projectConfig)
        {
            throw new KeyNotFoundException();
            /*
            var attachments = new List<Attachment>();
            
            GetNodeRefPathByIdResponse result = new GetNodeRefPathByIdResponse();
            using (var db = new CDFrameworkContext())
            {
                result.RefPath = db.GraphNodes.Find(request.NodeId).RefPath;
            }

            string stringResult = null;
            if (result != null)
            {
                stringResult = result.Serialize();
            }

            return new ProcessingResult()
            {
                Content = stringResult,
                Attachments = attachments
            };
            */
        }
    }
}
