using CD.DLS.Interfaces.DependencyGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.DAL.Objects.BIDoc;

namespace CD.DLS.DependencyGraph.Mssql.KnowledgeBase
{
    public class DataFlowKnowledgeBase : GeneralKnowledgeBase
    {
        public DataFlowKnowledgeBase() 
            : base(DependencyGraphKind.DataFlow,
                  new MssqlDataFlowRule[]
            {
                new MssqlReferenceDataFlowRule(),
                new MssqlDmlSourceDataFlowRule(),
                new MssqlDmlTargetReferenceDataFlowRule(),
                new MssqlNAryOperationOperandRule(),

                new SsisDfExternalColumnsDataFlowRule(),
                new SsisDfTransformationDataFlowRule(),
                new SsisDfSourceDataFlowRule(),
                new SsisDfLookupOutputJoinReferenceRule(),
                new SsisDfUnpivotSourceDataFlowRule(),
                new SsisDfAggregationDataFlowRule(),

                new SsasDsvSourceColumnDataFlowRule(),
                new SsasDimensionKeyColumnDataFlowRule(),
                new SsasDimensionNameColumnDataFlowRule(),
                new SsasHierarchyLevelDataFlowRule(),
                new SsasPhysicalMeasureDataFlowRule(),
                new SsasPhysicalMeasurePartitionSourceDataFlowRule(),
                new SsasPartitionColumnDataFlowRule(),
                new SsasCubeDimensionDataFlowRule(),

                new SsrsDataSetFieldDataFlowRule(),
                new SsrsReportParameterValidValuesDataFlowRule(),
                new SsrsReportParameterDefaultValuesDataFlowRule()
            })
        {
        }

        public override IDependencyGraph BuildGraph(IModelElement model)
        {
            var res = base.BuildGraph(model);
            //SetTopologicalOrder(res);
            return res;
        }

        private void SetTopologicalOrder(IDependencyGraph graph)
        {
            graph.BuildIndexes();

            Dictionary<string, int> precedenceCounts = new Dictionary<string, int>();
            foreach (var node in graph.AllNodes)
            {
                precedenceCounts.Add(node.ModelElement.RefPath.Path, 0);
            }
            foreach (var link in graph.AllLinks.Where(x => x.DependencyKind == DependencyKind.DataFlow))
            {
                precedenceCounts[link.NodeTo.ModelElement.RefPath.Path]++;
            }

            //var tRefPath = "Server[@Name='RJ-THINK']/Database[@Name='ManpowerDWH']/UserDefinedFunction[@Name='f_GetExtractStatusId' and @Schema='Adm']/[CREATE_0]";
            //var tLinks = graph.AllLinks.Where(x => x.DependencyKind == DependencyKind.DataFlow && x.NodeTo.ModelElement.RefPath.Path == tRefPath).ToList();
            //var tDepCount = precedenceCounts[tRefPath];

            int topolCounter = 0;
            while (precedenceCounts.Any())
            {
                var independentNodes = precedenceCounts.Where(x => x.Value == 0).ToList();
                foreach (var independent in independentNodes)
                {
                    foreach (var outLink in graph.GetOutboundLinks(graph.GetNode(independent.Key), DependencyKind.DataFlow))
                    {
                        var targetNodePath = outLink.NodeTo.ModelElement.RefPath.Path;
                        //if (targetNodePath == tRefPath)
                        //{

                        //}
                        precedenceCounts[targetNodePath]--;
                    }

                    var indepNode = (DataFlowDependencyGraphNode)graph.GetNode(independent.Key);
                    indepNode.TopologicalOrder = topolCounter++;
                    
                }

                var independentKeys = independentNodes.Select(x => x.Key).ToList();
                foreach (var independentKey in independentKeys)
                {
                    precedenceCounts.Remove(independentKey);
                }
            }

        }
        

        protected override DependencyGraphNode ExtendDependencyGraphHierarchy(MssqlModelElement modelElement)
        {
            DataFlowDependencyGraphNode node = new DataFlowDependencyGraphNode(_graph, modelElement);
            _context.MapElementToNode(modelElement, node);
            _graph.AddNode(node);
            
            foreach (var child in modelElement.Children)
            {
                DependencyGraphNode childNode = ExtendDependencyGraphHierarchy(child);
                DependencyGraphLink link = new DependencyGraphLink(childNode, node, DependencyKind.Parent);
                _graph.AddLink(link);
            }

            return node;
        }

    }
}
