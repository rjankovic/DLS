using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorCode;
using HtmlAgilityPack;

using CD.DLS.Interfaces.DependencyGraph;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Ssas;
using System.Web;
using System.Net;
using Irony.Parsing;
using CD.DLS.Export.Html.Formatting;
using System.IO;
using CD.DLS.Model.Mssql.Ssis;

namespace CD.DLS.Export.Html
{
    /// <summary>
    /// Generates syntax-highlighted HTML files for 
    /// </summary>
    public class SsisExpressionHtmlGenerator : IGraphNodeHtmlGenerator
    {
        private IronyScriptExport _exporter;

        public SsisExpressionHtmlGenerator(LinkModeEnum linkMode, Grammar ssisExpressionGrammar)
        {
            var tagger = new GrammarTagger();
            _exporter = new IronyScriptExport(ssisExpressionGrammar, tagger, new HtmlTagWriter(linkMode));
        }

        public string GenerateHtmlDocument(IDependencyGraph graph, IDependencyGraphNode node)
        {
            TextWriter writer = new StringWriter();
            writer.Write("<code><pre>");
            _exporter.Export(writer, node.ModelElement.Definition, (SsisExpressionFragmentElement)node.ModelElement);
            writer.Write("</pre></code>");

            return writer.ToString();
        }
    }
}
