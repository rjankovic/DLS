using CD.DLS.Interfaces;
using CD.DLS.Interfaces.DependencyGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Export.Debug
{
    /// <summary>
    /// Exports model or a dependency graph into a dot file (for graphviz).
    /// </summary>
    public class DotExport
    {
        private TextWriter writer;

        public DotExport(TextWriter writer)
        {
            this.writer = writer;
        }

        private string EscapeLabel(string label)
        {
            return label.Replace("\\", "\\\\");
        }

        private void ExportModelElement(IModelElement model)
        {
            writer.WriteLine("{0}[label=\"{1}\"];", model.GetHashCode(), EscapeLabel(model.Caption));
            foreach (var child in model.Children)
            {
                ExportModelElement(child);
                writer.WriteLine("{0}->{1};", model.GetHashCode(), child.GetHashCode());
            }

        }

        public void ExportModelGraph(IModelElement model)
        {
            writer.WriteLine("digraph{");

            ExportModelElement(model);
            
            writer.WriteLine("}");
        }


        public void ExportDependencyGraph(IDependencyGraph graph)
        {
            writer.WriteLine("digraph{");

            foreach(IDependencyGraphNode node in graph.AllNodes)
            {
                writer.WriteLine("{0}[label=\"{1}\"];", node.GetHashCode(), EscapeLabel(node.ModelElement.Caption));
            }

            foreach(IDependencyGraphLink link in graph.AllLinks)
            {
                writer.WriteLine("{0}->{1};", link.NodeFrom.GetHashCode(), link.NodeTo.GetHashCode());
            }

            writer.WriteLine("}");
        }
    }
}
