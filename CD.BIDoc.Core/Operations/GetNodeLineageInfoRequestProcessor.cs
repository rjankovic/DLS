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
using CD.DLS.Tools;
using CD.DLS.DAL.Objects.BIDoc;
using CD.DLS.DAL.Managers;
//using CD.Framework.EF.Entities;

namespace CD.DLS.Operations
{
    internal class GetElementLineageInfoRequestProcessor : BIDocRequestProcessor<GetElementLineageInfoRequest>
    {
        public GetElementLineageInfoRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(GetElementLineageInfoRequest request, ProjectConfig projectConfig)
        {
            var elementId = request.ModelElementId;
      
            GetElementLineageInfoRequestResponse result = new GetElementLineageInfoRequestResponse();

            var nodeId  = GraphManager.GetGraphNodeId(elementId, DependencyGraphKind.DataFlow);

            var documents = GraphManager.GetGraphDocuments(nodeId);
            var currentNode = InspectManager.GetGraphNodeExtended(nodeId);
            var origins = InspectManager.GraphNodeLineageOriginOneLevel(nodeId);
            var children = InspectManager.GetGraphNodeChildrenExtended(nodeId);
            var highLevelSources = InspectManager.ElementHighLevelSourcesTransitive(elementId);
            var highLevelDestinations = InspectManager.ElementHighLevelDestinationsTransitive(elementId);

            var path = currentNode.RefPath;
            BIDocGraphInfoNodeExtended parent = null;
            if (currentNode.ParentId.HasValue)
            {
                parent = InspectManager.GetGraphNodeExtended(currentNode.ParentId.Value);
            }

            result.Documents = documents.Select(x => new GraphDocument()
            {
                Content = x.Content,
                DocumentType = x.DocumentType,
                Id = x.Id,
                NodeRefPath = x.NodeRefPath,
                GraphNodeId = x.GraphNodeId
            }).ToList();
            result.Declaration = NodeDeclarationConverter.ToNodeDeclaration(currentNode);
            result.Definition = currentNode.Description;
            result.Parent = NodeDeclarationConverter.ToNodeDeclaration(parent);
            result.Children = children.Select(x => NodeDeclarationConverter.ToNodeDeclaration(x)).ToList();
            result.LineageSources = origins.Select(x => NodeDeclarationConverter.ToNodeDeclaration(x)).ToList();
            result.HighLevelLineageSources = highLevelSources.Select(x => NodeDeclarationConverter.ToNodeDeclaration(x)).ToList();
            result.HighLevelLineageDestinations = highLevelDestinations.Select(x => NodeDeclarationConverter.ToNodeDeclaration(x)).ToList();

            var stringResult = result.Serialize();

            return new ProcessingResult()
            {
                Content = stringResult,
                Attachments = new List<Attachment>()
            };
        }
        
    }
}
