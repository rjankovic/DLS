using CD.DLS.Serialization;
using CD.DLS.API;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Operations
{
    internal class GetMssqlDocumentsRequestProcessor : GetDocumentsRequestProcessor<GetMssqlDocumentsRequest>
    {
        public GetMssqlDocumentsRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(GetMssqlDocumentsRequest request, ProjectConfig projectConfig)
        {
            var attachments = new List<Attachment>();

            //using (var db = new CDFrameworkContext())
            //{
                var documents = GetDocumentsByType(projectConfig, DocumentTypeEnum.SqlCode);
                //var attachment = _core.StorageProvider.CreateDocumentZipAttachment(string.Format("{0}: MssqlHtml", projectConfig.Name), AttachmentTypeEnum.XML, documents);
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
