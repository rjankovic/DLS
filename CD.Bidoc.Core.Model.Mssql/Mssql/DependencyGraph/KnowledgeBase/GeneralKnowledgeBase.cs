using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Common.Structures;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Interfaces.DependencyGraph;
using CD.DLS.Model.Mssql;

namespace CD.DLS.Model.DependencyGraph.KnowledgeBase
{   
    public class GeneralKnowledgeBase
    {
        private List<IRule> _rules;
        private Dictionary<IRule, List<MssqlModelElement>> _ruleApplications;
        protected RuleApplicationContext _context;
        protected DependencyGraph _graph;
        private DependencyGraphKind _graphKind;

        public GeneralKnowledgeBase(DependencyGraphKind graphKind, IEnumerable<IRule> rules)
        {
            _graphKind = graphKind;
            _rules = new List<IRule>(rules);
            _graph = new DependencyGraph(_graphKind);
            _ruleApplications = new Dictionary<IRule, List<MssqlModelElement>>();
            _context = new RuleApplicationContext(_graph);
        }

        public virtual IDependencyGraph BuildGraph(IModelElement model)
        {
            var mssqlModel = (MssqlModelElement)model;
            
            ExtendDependencyGraphHierarchy(mssqlModel);
            BuildRuleApplicatoinSchedule(mssqlModel);
            ApplyRules();
            return _graph;
        }

        private void ApplyRules()
        {
            foreach (var rule in _ruleApplications.Keys)
            {
                var elements = _ruleApplications[rule];
                int applicationCount = 0;
                foreach (var element in elements)
                {
                    rule.Apply(element, _context);
                    applicationCount++;
                }
            }
        }

        private void BuildRuleApplicatoinSchedule(MssqlModelElement modelElement)
        {
            foreach (var rule in _rules)
            {
                if (rule.AppliesTo(modelElement, _context))
                {
                    if (!_ruleApplications.ContainsKey(rule))
                    {
                        _ruleApplications.Add(rule, new List<MssqlModelElement>());
                    }
                    _ruleApplications[rule].Add(modelElement);
                }
            }
            foreach (var child in modelElement.Children)
            {
                BuildRuleApplicatoinSchedule(child);
            }
        }

        protected virtual DependencyGraphNode ExtendDependencyGraphHierarchy(MssqlModelElement modelElement)
        {
            DependencyGraphNode node = new DependencyGraphNode(_graph, modelElement);
            _context.MapElementToNode(modelElement, node);
            _graph.AddNode(node);
            //if (modelElement.RefPath.Path.StartsWith("IntegrationServices[@Name='RJ-THINK']/Catalog[@Name='SSISDB']/CatalogFolder[@Name='Manpower_SSIS']/ProjectInfo[@Name='DWH']/PackageInfo[@Name='LoadHelios_FactOrganizationAcmMonth.dtsx']/Executable[@Name='Organization']/Executable[@Name='Generate AcM for TT Orgs']/[;_0]/[SELECT_37]"))

            //if (modelElement.RefPath.Path.StartsWith("IntegrationServices[@Name='RJ-THINK']/Catalog[@Name='SSISDB']/CatalogFolder[@Name='Manpower_SSIS']/ProjectInfo[@Name='DWH']/PackageInfo[@Name='LoadHelios_FactOrganizationAcmMonth.dtsx']/Executable[@Name='Organization']/Executable[@Name='Generate AcM for TT Orgs']"))
            //{

            //}
            foreach (var child in modelElement.Children)
            {
                DependencyGraphNode childNode = ExtendDependencyGraphHierarchy(child);
                DependencyGraphLink link = new DependencyGraphLink(childNode, node, DependencyKind.Parent);
                //if (node._modelElement.RefPath.Path == "SSASServer[@Name='RJ-THINK']/Db[@Name='Manpower_SSAS']/Cube[@Name='Manpower']/MdxScript[@Name='MdxScript']/CalculatedMeasure[@Name='BLC']")
                //{

                //}
                _graph.AddLink(link);
            }

            return node;
        }

    }
}
