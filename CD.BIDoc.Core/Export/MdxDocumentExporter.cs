using CD.DLS.Interfaces.DependencyGraph;
using CD.DLS.Model.Mssql.Ssas;
using CD.DLS.API;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Export.Html.Mssql
{
    class MdxDocumentExporter
    {
        private readonly IGraphNodeHtmlGenerator _nodeHtmlGenerator;

        public MdxDocumentExporter(IGraphNodeHtmlGenerator nodeHtmlGenerator)
        {
            _nodeHtmlGenerator = nodeHtmlGenerator;
        }

        private IEnumerable<IDependencyGraphNode> FindScriptRootNodes(IDependencyGraph graph)
        {
            return graph.AllNodes.Where(x => x.ModelElement is MdxStatementElement && (x.ModelElement.Parent == null 
            || !(x.ModelElement.Parent is MdxStatementElement)));



            // TODO: VD: only root nodes
            //&& ((DbScriptElement)x.ModelElement).ScriptRoot == x)
        }

        public IEnumerable<GraphDocument> ExportDocuments(IDependencyGraph graph)
        {
            List<GraphDocument> res = new List<GraphDocument>();

            int id = 1;
            foreach (var node in FindScriptRootNodes(graph))
            {
                var html = _nodeHtmlGenerator.GenerateHtmlDocument(graph, node);
                yield return new GraphDocument()
                {
                    Id = id++,
                    //GraphNodeId = node.
                    NodeRefPath = node.ModelElement.RefPath.ToString(),
                    DocumentType = DocumentTypeEnum.MdxCode,
                    Content = html,
                    GraphNodeId = node.Id
                };
            }
            //return res;
        }

    }
}
