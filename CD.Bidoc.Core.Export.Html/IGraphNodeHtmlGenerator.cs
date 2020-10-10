using CD.DLS.Interfaces.DependencyGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Export.Html
{
    public enum LinkModeEnum { Span, Href };

    public interface IGraphNodeHtmlGenerator
    {
        string GenerateHtmlDocument(IDependencyGraph graph, IDependencyGraphNode node);
    }
}
