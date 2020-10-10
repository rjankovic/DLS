using CD.DLS.Interfaces.DependencyGraph;
using CD.DLS.Model.Mssql.Db;
using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.API;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Export.Html.Mssql
{
    /// <summary>
    /// Exports documents for SSIS expressions
    /// </summary>
    public class SsisExpressionDocumentExporter
    {
        private readonly IGraphNodeHtmlGenerator _nodeHtmlGenerator;

        public SsisExpressionDocumentExporter(IGraphNodeHtmlGenerator nodeHtmlGenerator)
        {
            _nodeHtmlGenerator = nodeHtmlGenerator;
        }

        private IEnumerable<IDependencyGraphNode> FindScriptRootNodes(IDependencyGraph graph)
        {
            // Root Ssis expression fragments
            return graph.AllNodes.Where(x => x.ModelElement is SsisExpressionFragmentElement && !(x.ModelElement.Parent is SsisExpressionFragmentElement));
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
                    NodeRefPath = node.ModelElement.RefPath.ToString(),
                    DocumentType = DocumentTypeEnum.SsisExpressionCode,
                    Content = html,
                    GraphNodeId = node.Id
                };
            }
        }

    }
}
