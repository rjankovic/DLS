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
    public struct Document
    {
        public readonly string Path;
        public readonly string Content;

        public Document(string path, string content)
        {
            this.Path = path;
            this.Content = content;
        }
    }

    internal abstract class GetDocumentsRequestProcessor<TRequest> : BIDocRequestProcessor<TRequest>
        where TRequest : DLSApiMessage
    {
        public GetDocumentsRequestProcessor(BIDocCore core) : base(core)
        {
        }
        protected IEnumerable<Document> GetDocumentsByType(/*CDFrameworkContext db, */ProjectConfig projectConfig, DocumentTypeEnum documentType)
        {

            throw new NotImplementedException();

            //var documents = GraphManager.GetGraphDocuments(projectConfig.ProjectConfigId, graphKind, documentType);



            //Dictionary<string, string> paths = new Dictionary<string, string>();

            //BIDocGraphStored graphStored = new BIDocGraphStored(db, projectConfig.ProjectConfigId);

            //foreach (var node in graphStored.Nodes)
            //{
            //    paths.Add(node.RefPath, node.DocumentRelativePath);
            //}

            //foreach (var doc in db.Documents.Where(x => x.ProjectConfigId == projectConfig.ProjectConfigId
            //    && x.DocumentType == documentType))
            //{
            //    var path = paths[doc.NodeRefPath];

            //    yield return new Document(path, doc.Content);
            //}
        }
    }
}
