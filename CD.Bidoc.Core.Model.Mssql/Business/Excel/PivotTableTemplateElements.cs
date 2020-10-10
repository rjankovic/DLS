using CD.DLS.API.Structures;
using CD.DLS.Model.Business.Organization;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Ssas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Business.Excel
{
    public class PivotTableTemplateElement : ExcelModelElement
    {
        public PivotTableTemplateElement(RefPath refPath, string caption, string definition, BusinessFolderElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        [DataMember]
        public PivotTableStructure PivotTableStructure { get; set; }
        [DataMember]
        public string ConnectionString { get; set; }
        [DataMember]
        public string CubeName { get; set; }
        [DataMember]
        public PivotFieldOrientation ValuesOrientation { get; set; }
        [DataMember]
        public string TableStyle { get; set; }

        public List<PivotTableFieldElement> VisibleFields { get { return ChildrenOfType<PivotTableFieldElement>().ToList(); } }
    }

    
    public class PivotTableFieldElement : ExcelModelElement
    {
        public PivotTableFieldElement(RefPath refPath, string caption, string definition, PivotTableTemplateElement parent = null)
            : base(refPath, caption, definition, parent)
        {
        }

        [DataMember]
        public int Position { get; set; }
        [DataMember]
        public PivotFieldOrientation Orientation { get; set; }
        [DataMember]
        public string Hierarchy { get; set; }
        [DataMember]
        public string Dimension { get; set; }
        [DataMember]
        public string Attribute { get; set; }
        [DataMember]
        public string SourceName { get; set; }
        [DataMember]
        public List<PivotFieldItem> VisibleItems { get; set; }

        public List<PivotTableValuesFilterElement> Filters { get { return ChildrenOfType<PivotTableValuesFilterElement>().ToList(); } }

        [ModelLink]
        public SsasModelElement SourceField { get; set; }

        public string OlapFieldName
        {
            get
            {
                if (Orientation == PivotFieldOrientation.Data || Orientation == PivotFieldOrientation.Filter)
                {
                    return string.Format("[{0}].[{1}]", Dimension, Hierarchy);
                }
                return string.Format("[{0}].[{1}].[{2}]", Dimension, Hierarchy, Attribute);
            }
        }
    }

    public class PivotFieldItem
    {
        public string ItemName { get; set; }
    }

    
    public class PivotTableValuesFilterElement : ExcelModelElement
    {
        public PivotTableValuesFilterElement(RefPath refPath, string caption, string definition, PivotTableFieldElement parent = null)
            : base(refPath, caption, definition, parent)
        {
        }

        [DataMember]
        public string Value1 { get; set; }
        [DataMember]
        public string Value2 { get; set; }
        [DataMember]
        public string MeasureName { get; set; }
        [DataMember]
        public ValuesFilterType Type { get; set; }

        [ModelLink]
        public SsasModelElement SourceMeasure { get; set; }
    }
    

}
