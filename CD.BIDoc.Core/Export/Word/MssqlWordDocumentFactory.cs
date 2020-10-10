//using CD.BIDoc.Core.BusinessLogic.DependencyGraph.MssqlDb;
//using CD.BIDoc.Core.Interfaces.Export;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CD.BIDoc.Core.Interfaces.DependencyGraph;
//using ColorCode;
//using CD.BIDoc.Core.DataLayer;

//namespace CD.BIDoc.Core.Export.Word
//{
//    class MssqlWordDocumentFactory : IDocumentFactory<DependencyNode>
//    {
//        private ProjectConfig _projectConfig;
//        private Predicate<DependencyNode> _filter;
//        public Predicate<DependencyNode> Filter
//        {
//            get
//            {
//                return _filter;
//            }

//            set
//            {
//                _filter = value;
//            }
//        }

//        public MssqlWordDocumentFactory(ProjectConfig projectConfig)
//        {
//            _projectConfig = projectConfig;
//            _filter = new Predicate<DependencyNode>(x => true);
//        }

//        public List<NodeDocument> GenerateDocuments(IDependencyGraph graph)
//        {
//            return GenerateDocuments(
//                graph.Nodes
//                .Where(x => x is DependencyNode)
//                .Select(x => x as DependencyNode)
//                ).ToList();
//        }

//        public List<NodeDocument> GenerateDocuments(IEnumerable<DependencyNode> nodes)
//        {
//            return nodes
//                .Where(x => _filter(x) && x is ScriptNode).Where(x => ((ScriptNode)x).ScriptRoot == (ScriptNode)x)
//                .Select((x) =>
//            {
//                return new NodeDocument()
//                {
//                    Content = new CodeColorizer().Colorize(x.Definition, Languages.Sql),
//                    Name = x.Caption,
//                    RefPath = x.RefPath, TargetType = ExportDocumentTargetEnum.Docx,
//                    Project = _projectConfig
//                };
//            }).ToList();
//        }
//    }
//}
