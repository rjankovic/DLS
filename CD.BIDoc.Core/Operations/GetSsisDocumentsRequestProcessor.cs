using CD.DLS.Serialization;
using CD.DLS.API;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Operations
{
    internal class GetSsisDocumentsRequestProcessor : GetDocumentsRequestProcessor<GetSsisDocumentsRequest>
    {
        public GetSsisDocumentsRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(GetSsisDocumentsRequest request, ProjectConfig projectConfig)
        {
            var attachments = new List<Attachment>();
            
            //using (var db = new CDFrameworkContext())
            //{
                var documents = GetDocumentsByType(projectConfig, DocumentTypeEnum.SsisGraph);
                //var attachment = _core.StorageProvider.CreateDocumentZipAttachment(string.Format("{0}: SsisGraphs", projectConfig.Name), AttachmentTypeEnum.SVG, documents);
                //attachments.Add(attachment);
            //}

            return new ProcessingResult()
            {
                Content = string.Empty,
                Attachments = attachments
            };
        }
    }
}
