using CD.DLS.Interfaces.DependencyGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Db;
using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.Model.Mssql.Ssas;

namespace CD.DLS.DependencyGraph.Mssql.KnowledgeBase
{
    abstract class MssqlDataFlowRule : IRule
    {
        
        public DependencyKind DependencyKind
        {
            get
            {
                return DependencyKind.DataFlow;
            }
        }

        public bool AppliesTo(IModelElement element, IRuleApplicationContext context)
        {
            if(element is MssqlModelElement && context is RuleApplicationContext)
            {
                return AppliesTo((MssqlModelElement)element, (RuleApplicationContext)context);
            }
            return false;
        }

        public abstract bool AppliesTo(MssqlModelElement element, RuleApplicationContext context);


        public void Apply(IModelElement element, IRuleApplicationContext context)
        {
            if (element is MssqlModelElement && context is RuleApplicationContext)
            {
                Apply((MssqlModelElement)element, (RuleApplicationContext)context);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public abstract void Apply(MssqlModelElement element, RuleApplicationContext context);

        protected void AddLink(MssqlModelElement fromElement, MssqlModelElement toElement, RuleApplicationContext context)
        {
            //if (fromElement.RefPath.Path == "Server[@Name='LAPTOP-BRLOS479']/Database[@Name='ManpowerDWH']/Schema[@Name='dbo']/Table[@Name='DimProject']/Column[@Name='ProjectName']"
            //    && toElement.RefPath.Path == "IntegrationServices[@Name='LAPTOP-BRLOS479']/Catalog/Folder[@Name='Manpower_SSIS']/Project[@Name='DWH']/Package[@Name='LoadTargetty_Dimension_NakladovyOkruh.dtsx']/Executable[@Name='Sequence Container']/Executable[@Name='Load NakladovyOkruh']/SourceComponent[@IdString'Nakladovy okruh from DW']/Output[@Name='OLE DB Source Output']/Column[@Name='ProjectName_59']")
            //{

            //}
            var fromNode = context.GetNode(fromElement);
            var toNode = context.GetNode(toElement);
            
            if (fromNode == null || toNode == null)
            {
                throw new Exception();
            }
            context.AddLink(fromNode, toNode, this);
        }
    }

    class MssqlReferenceDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            // thre Reference here points to the insert target column - the data flow 
            // goes in the opposite direction, from this reference to the target
            if (element is SqlDmlTargetReferenceElement)
            {
                return false;
            }
            return element.Reference != null;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            AddLink(element.Reference, element, context);
            
        }
    }

    class MssqlDmlSourceDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            return element is SqlDmlSourceElement;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            var insertSource = (SqlDmlSourceElement)element;
            AddLink(insertSource, insertSource.TargetReference, context);
            //if (element.RefPath.Path.Contains("Merge DimEmployee"))
            //{

            //}
        }
    }

    class MssqlDmlTargetReferenceDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            // tempt tables and others may not be resolved
            if (element.Reference == null)
            {
                return false;
            }
            return element is SqlDmlTargetReferenceElement;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            var insertTarget = (SqlDmlTargetReferenceElement)element;
            AddLink(insertTarget, insertTarget.Reference, context);
        }
    }

    class MssqlNAryOperationOperandRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            var t = element as SqlNAryOperationOperandColumnElement;
            if (t == null)
            {
                return false;
            }
            return t.OperationOutputColumn != null;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            var operand = (SqlNAryOperationOperandColumnElement)element;
            AddLink(operand, operand.OperationOutputColumn, context);
        }
    }

    class SsisDfExternalColumnsDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            if (!(element is DfColumnElement))
            {
                return false;
            }
            return ((DfColumnElement)(element)).ExternalDestinationColumn != null 
                || ((DfColumnElement)(element)).ExternalSourceColumn != null;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            var colElement = element as DfColumnElement;
            if (colElement.ExternalSourceColumn != null)
            {
                AddLink(colElement.ExternalSourceColumn, colElement, context);
            }
            if (colElement.ExternalDestinationColumn != null)
            {
                AddLink(colElement, colElement.ExternalDestinationColumn, context);
            }
        }
    }

    class SsisDfTransformationDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            if (!(element is DfColumnElement))
            {
                return false;
            }
            return ((DfColumnElement)element).SourceDfColumn != null;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            AddLink(((DfColumnElement)element).SourceDfColumn, element, context);
        }
    }

    class SsisDfSourceDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            if (!(element is DfSourceElement))
            {
                return false;
            }
            return ((DfSourceElement)element).SourceConnection != null;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            AddLink(((DfSourceElement)element).SourceConnection, element, context);
        }
    }

    class SsisDfUnpivotSourceDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            if(!(element is DfUnpivotSourceReferenceElement))
            {
                return false;
            }
            return ((DfUnpivotSourceReferenceElement)element).TargetPivotKeyColumn != null
                && ((DfUnpivotSourceReferenceElement)element).TargetValueColumn != null;
        }
        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            AddLink(element,((DfUnpivotSourceReferenceElement)element).TargetPivotKeyColumn, context);
            AddLink(element, ((DfUnpivotSourceReferenceElement)element).TargetValueColumn, context);
        }
    }

    class SsisDfAggregationDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            if (!(element is DfColumnAggregationLinkElement))
            {
                return false;
            }
            return ((DfColumnAggregationLinkElement)element).SourceDfColumn != null
                && ((DfColumnAggregationLinkElement)element).TargetDfColumn != null;
        }
        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            AddLink(
                ((DfColumnAggregationLinkElement)element).SourceDfColumn,
                ((DfColumnAggregationLinkElement)element).TargetDfColumn, 
                context);
        }
    }


    class SsisDfLookupOutputJoinReferenceRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            if (!(element is DfLookupOutputJoinReferenceElement))
            {
                return false;
            }
            return ((DfLookupOutputJoinReferenceElement)element).InputJoinColumn != null && ((DfLookupOutputJoinReferenceElement)element).OutputColumn != null;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            var typed = (DfLookupOutputJoinReferenceElement)element;
            AddLink(typed.InputJoinColumn, typed.OutputColumn, context);
        }
    }

    class SsasDsvSourceColumnDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            if (!(element is DatasourceViewColumnElement))
            {
                return false;
            }
            return ((DatasourceViewColumnElement)element).Source != null;
             
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            AddLink(((DatasourceViewColumnElement)element).Source, element, context);
        }
    }

    class SsasDimensionKeyColumnDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            return element is KeyColumnElement;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            var keyCol = (KeyColumnElement)element;
            AddLink(keyCol.DsvColumn, keyCol, context);
        }
    }

    class SsasDimensionNameColumnDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            var x = element as NameColumnElement;
            if (x == null)
            {
                return false;
            }
            if (x.DsvColumn == null)
            {
                return false;
            }
            return true;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            var nameCol = (NameColumnElement)element;
            AddLink(nameCol.DsvColumn, nameCol, context);
        }
    }

    class SsasHierarchyLevelDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            return element is HierarchyLevelElement;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            AddLink(((HierarchyLevelElement)element).Attribute, element, context);
        }
    }

    class SsasPhysicalMeasureDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            return element is PhysicalMeasureElement;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            var measureElement = (PhysicalMeasureElement)element;
            foreach (var partitionSource in measureElement.Sources)
            {
                AddLink(partitionSource, measureElement, context);
            }
        }
    }

    class SsasPhysicalMeasurePartitionSourceDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            return element is PhysicalMeasurePartitionSourceElement;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            AddLink(((PhysicalMeasurePartitionSourceElement)element).Source, element, context);
        }
    }

    class SsasPartitionColumnDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            return element is PartitionColumnElement;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            AddLink(((PartitionColumnElement)element).Source, element, context);
        }
    }

    class SsasCubeDimensionDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            return element is CubeDimensionElement;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            AddLink(((CubeDimensionElement)element).DatabaseDimension, element, context);
        }
    }
    class SsrsDataSetFieldDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            if (!(element is Model.Mssql.Ssrs.DataSetFieldElement))
            {
                return false;
            }
            return ((Model.Mssql.Ssrs.DataSetFieldElement)element).Source != null;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            AddLink(((Model.Mssql.Ssrs.DataSetFieldElement)element).Source, element, context);
        }
    }

    class SsrsReportParameterValidValuesDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            if (!(element is Model.Mssql.Ssrs.ReportParameterValidValuesDataSetElement))
            {
                return false;
            }
            return ((Model.Mssql.Ssrs.ReportParameterValidValuesDataSetElement)element).ValueField != null;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            AddLink(((Model.Mssql.Ssrs.ReportParameterValidValuesDataSetElement)element).ValueField, element, context);
        }
    }

    class SsrsReportParameterDefaultValuesDataFlowRule : MssqlDataFlowRule
    {
        public override bool AppliesTo(MssqlModelElement element, RuleApplicationContext context)
        {
            if (!(element is Model.Mssql.Ssrs.ReportParameterDefaultValuesDataSetElement))
            {
                return false;
            }
            return ((Model.Mssql.Ssrs.ReportParameterDefaultValuesDataSetElement)element).ValueField != null;
        }

        public override void Apply(MssqlModelElement element, RuleApplicationContext context)
        {
            AddLink(((Model.Mssql.Ssrs.ReportParameterDefaultValuesDataSetElement)element).ValueField, element, context);
        }
    }
}
