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

namespace CD.DLS.Operations
{
    internal class SaveBusinessDictionaryEntryRequestProcessor : BIDocRequestProcessor<SaveBusinessDictionaryEntryRequest>
    {
        public SaveBusinessDictionaryEntryRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(SaveBusinessDictionaryEntryRequest request, ProjectConfig projectConfig)
        {
            var refPath = request.RefPath;
            var content = request.Content;
    // TODO: reenable with non-graph-bound documents
            /*
            using (var db = new CDFrameworkContext())
            {
                var existentEntry = db.Documents.Where(x => x.NodeRefPath == refPath && x.DocumentType == DocumentTypeEnum.BusinessDictionary).FirstOrDefault();
                if (existentEntry != null)
                {
                    existentEntry.Content = content;
                }
                else
                {
                    db.Documents.Add(new DataLayer.BIDocGraphDocument()
                    {
                        NodeRefPath = refPath,
                        DocumentType = DocumentTypeEnum.BusinessDictionary,
                        Content = content//,
                        //GraphNodeId = -1,
                        //ProjectConfigId = new Guid()
                    });
                }

                db.SaveChanges();
            }
      */      
            return new ProcessingResult()
            {
                Content = string.Empty,
                Attachments = new List<Attachment>()
            };
        }

        //private NodeDeclaration ToNodeDeclaration(DataLayer.BIDocGraphInfoNode graphNode)
        //{
        //    return new NodeDeclaration()
        //    {
        //        RefPath = graphNode.RefPath,
        //        Name = graphNode.Name,
        //        NodeType = graphNode.NodeType
        //    };
        //}
    }
}
