using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Common.Structures;
using CD.DLS.API;
using CD.DLS.Serialization;
using Newtonsoft.Json;
using CD.DLS.DAL.Objects.BIDoc;

namespace CD.DLS.Operations
{
    class GetBasicDGRequestProcessor : BIDocRequestProcessor<GetBasicDGRequest>
    {
        public GetBasicDGRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(GetBasicDGRequest request, ProjectConfig projectConfig)
        {
            BIDocGraphInfoConverter bidocGraphInfoConverter = new BIDocGraphInfoConverter();
            BasicGraphInfo bgi;

            //using (var db = new CDFrameworkContext())
            //{
                BIDocGraphStored graph = new BIDocGraphStored(projectConfig.ProjectConfigId, DependencyGraphKind.DataFlow);
                bgi = bidocGraphInfoConverter.ConvertToBasicGraphInfo(graph);
            //}

            //var attachment = _core.StorageProvider.CreateJsonAttachment(bgi, string.Format("{0}: BasicNodeInfo", projectConfig.Name));
            var attachments = new List<Attachment>() { }; // { attachment };

            return new ProcessingResult()
            {
                Content = string.Empty,
                Attachments = attachments
            };

        }
    }
}
