using CD.DLS.Interfaces.DependencyGraph;
using CD.DLS.API;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Export.Html.Mssql
{
    class SsisDocumentExporter
    {

        #region HtmlConstants
        private const string SVG_PREFIX = @"


<!DOCTYPE html>
<html>
<head>
    <script src=""http://ajax.googleapis.com/ajax/libs/jquery/2.1.0/jquery.min.js""></script>
    <script src=""http://ajax.googleapis.com/ajax/libs/jqueryui/1.10.4/jquery-ui.min.js""></script>
    

    <script>

        $(document).ready(function () {
				/*
            $('pre code').each(function (i, block) {
                hljs.highlightBlock(block);
            });
						*/
            $(""#details"").children().hide();

            $("".hasDetails"").click(function () {
                $(""#details"").children().hide();
                //alert($(""#details____"" + $(this).attr(""id"")).html());
                $(""#details____"" + $(this).attr(""id"")).show();
            });
        });

    </script>
<style>
    html, body{
    height: 100%;
    height: 100%;
}
rect{
fill:rgb(230,230,230);
stroke-width:1;
stroke:rgb(200,200,200);
}
text
{

}
line
{
stroke-width:5;
stroke:rgb(50,100,200);
}
.arrowPolygon
{
stroke:rgb(50,100,200);
fill:rgb(50,100,200);
}
.detailsPolygon
{
fill:rgb(250,250,250);
stroke-width:0;
}
.noDetailsPolygon
{
fill:rgb(230,230,230);
stroke-width:0;
}

    #details_Package__GetCurrentDWDateId {
        /*display: none;*/
    }
</style>
<!--
        <link rel=""stylesheet"" href=""http://cdnjs.cloudflare.com/ajax/libs/highlight.js/8.9.1/styles/default.min.css"">
    
<script src=""http://cdnjs.cloudflare.com/ajax/libs/highlight.js/8.9.1/highlight.min.js""></script>
-->
</head>
<body>
    <div style=""width:100%;height:50%;overflow:scroll;position: relative;"">

";

        private string SVG_SUFFIX = @"
    </div>
</body>
</html>
";
        #endregion

        public IEnumerable<GraphDocument> ExportDocuments(IDependencyGraph graph)
        {
            yield break;

#if false
            //List<GraphDocument> res = new List<GraphDocument>();
            //StringBuilder manifest = new StringBuilder();
#if false
            foreach (var file in Directory.GetFiles(@"D:\CDFramework\output"))
            {
                File.Delete(file);
            }
#endif

            int id = 1;

            List<ScriptNode> sqlScripts = new List<ScriptNode>();
            foreach (PackageNode package in graph.Nodes.Where(x => x is PackageNode))
            {
                //Console.WriteLine("Package {0}", package.Caption);
                var svg = package.GenerateSvgGraph();
                var doc = new GraphDocument()
                {
                    Id = id++,
                    NodeRefPath = package.RefPath,
                    DocumentType = DocumentTypeEnum.SsisGraph,
                    Content = svg
                };
                sqlScripts.AddRange(FindSqlScripts(package));
                yield return doc;
#if false
                File.WriteAllText(Path.Combine(@"D:\CDFramework\output", "CF_" + id.ToString() /*package.GetHtmlRefPath()*/ + ".htm"), SVG_PREFIX + svg + SVG_SUFFIX);
#endif
            }

            foreach (DfInnerNode dataflow in graph.Nodes.Where(x => x is DfInnerNode))
            {
                var svg = dataflow.GenerateSvgGraph();
                yield return new GraphDocument()
                {
                    Id = id++,
                    NodeRefPath = dataflow.RefPath,
                    DocumentType = DocumentTypeEnum.SsisGraph,
                    Content = svg
                };
#if false
                File.WriteAllText(Path.Combine(@"D:\CDFramework\output", "DF_" + id.ToString() /*dataflow.GetHtmlRefPath()*/ + ".htm"), SVG_PREFIX + svg + SVG_SUFFIX);
#endif
            }

            foreach (var scriptNode in sqlScripts)
            {
                var html = scriptNode.GenerateHtmlDocument();
                yield return new GraphDocument()
                {
                    Id = id++,
                    NodeRefPath = scriptNode.RefPath,
                    DocumentType = DocumentTypeEnum.SqlCode,
                    Content = html
                };
            }

            //return res;
        }

        private List<ScriptNode> FindSqlScripts(BasicDependencyNode node)
        {
            if (node is ScriptNode)
            {
                return new List<ScriptNode>() { node as ScriptNode };
            }
            return new List<ScriptNode>(node.Nodes.SelectMany(x => FindSqlScripts((BasicDependencyNode)x)));
#endif
        }

    }
}
