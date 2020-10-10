using CD.DLS.Common.Structures;
using CD.DLS.Model.Interfaces.DependencyGraph;
using CD.DLS.Model.Mssql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CD.DLS.Model.DependencyGraph.KnowledgeBase
{
    class TransitiveDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            return true;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            //if (element.RefPath.Path == "SSASServer[@Name='RJ-THINK']/Db[@Name='Contoso_Retail']/Cube[@Name='StrategyPlan']/MeasureGroup[@Name='Fact Strategy Plan']/Measure[@Name='Amount']")
            //{

            //}
            //HashSet<int> inboundNodes = new HashSet<int>();
            var dataflowGraph = context.GetSourceGraphByKind(DependencyGraphKind.DataFlow);
            var correspondingNode = dataflowGraph.GetNode(element.RefPath.Path);

            //var firstLevelTargets = new List<IModelElement>() { correspondingNode.ModelElement };
            //var parentBatch1 = firstLevelTargets;
            //while (parentBatch1.Any())
            //{
            //    var children = parentBatch1.SelectMany(x => x.Children).ToList();
            //    parentBatch1 = children;
            //    firstLevelTargets.AddRange(children);
            //}
            //var firstLevelNodeTargets = firstLevelTargets.Select(x => dataflowGraph.GetNode(x.RefPath.Path));


            // cummulative
            var inboundLinks //= firstLevelNodeTargets.SelectMany(x => dataflowGraph.GetInboundLinks(x, DependencyKind.DataFlow))
                    //.Where(x => CheckInboundNodeIsUnique(x, inboundNodes)).ToList();
            = dataflowGraph.GetInboundLinks(correspondingNode, DependencyKind.DataFlow).ToList();

            var inboundBatch = inboundLinks;
            while (inboundBatch.Any())
            {
                // collect all the elements that can be the intermediate points for the next level of transitive relations (including all their children)
                var nextLevelTargets = inboundBatch.Select(x => x.NodeFrom.ModelElement).ToList();
                var parentBatch = nextLevelTargets;
                while (parentBatch.Any())
                {
                    var children = parentBatch.SelectMany(x => x.Children).ToList();
                    parentBatch = children;
                    nextLevelTargets.AddRange(children);
                }
                var nextLevelNodeTargets = nextLevelTargets.Select(x => dataflowGraph.GetNode(x.RefPath.Path));

                // find inbound links targetting intermediate nodes
                var inboundBatchUnfiltered = nextLevelNodeTargets.SelectMany(x => dataflowGraph.GetInboundLinks(x, DependencyKind.DataFlow));
                inboundBatch = inboundBatchUnfiltered.GroupBy( x=> x.NodeFrom.ModelElement.Id).Select(x => x.First()).ToList();
                    //.Where(x => CheckInboundNodeIsUnique(x, inboundNodes)).ToList();
                inboundLinks.AddRange(inboundBatch);
                inboundLinks = inboundLinks.GroupBy(x => x.NodeFrom.ModelElement.Id).Select(x => x.First()).ToList();
            }

            //var inbLinksE = inboundLinks.Where(x => x.NodeFrom.ModelElement.RefPath.Path == "SSASServer[@Name='RJ-THINK']/Db[@Name='Manpower_SSAS']/Dsv[@Name='Manpower DWH']/Table[@Name='dbo_FactGeneralLedger']/Column[@Name='GeneralLedgerVAT']/[SELECT_0]").ToList();

            var distinctSources = inboundLinks.Select(x => x.NodeFrom.ModelElement).Distinct();
            foreach (var inboundElem in distinctSources)
            {
                AddLink((MssqlModelElement)inboundElem, element, context);
            }
        }

        /// <summary>
        /// Transitive link can come from multiple paths; insert link only once
        /// </summary>
        /// <param name="link"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        private bool CheckInboundNodeIsUnique(IDependencyGraphLink link, HashSet<int> set)
        {
            if (set.Contains(link.NodeFrom.ModelElement.Id))
            {
                return false;
            }
            if (link.NodeFrom.ModelElement.RefPath.Path == "SSASServer[@Name='RJ-THINK']/Db[@Name='Manpower_SSAS']/Dsv[@Name='Manpower DWH']/Table[@Name='dbo_FactGeneralLedger']/Column[@Name='GeneralLedgerVAT']/[SELECT_0]")
            {
            }
            set.Add(link.NodeFrom.ModelElement.Id);
            return true;
        }
    }   
}
