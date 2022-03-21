using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Ssas;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CD.DLS.Model.Mssql.Tabular
{
    public enum TabularRelationshipEndCardinality
    {
        None = 0,
        One = 1,
        Many = 2
    }

    abstract public class TabularModelElement : SsasModelElement
    {
        protected TabularModelElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null)
           : base(refPath, caption)
        {
            Definition = definition;
            Parent = parent;
        }

        public IEnumerable<SsasTabularAnnotationElement> Annotations { get { return ChildrenOfType<SsasTabularAnnotationElement>(); } }
    }

    public class SsasTabularDatabaseElement : SsasDatabaseElement // TabularModelElement
    {
        
        public SsasTabularDatabaseElement(RefPath refPath, string caption, string definition, ServerElement parent = null) 
            : base(refPath, caption, definition, parent)
        {
            SsasType = Common.Structures.SsasTypeEnum.Tabular;
        }

        [DataMember]
        public string Collation { get; set; }

        public IEnumerable<SsasTabularTableElement> Tables { get { return ChildrenOfType<SsasTabularTableElement>(); } }

        public IEnumerable<SsasTabularRelationshipElement> Relationships { get { return ChildrenOfType<SsasTabularRelationshipElement>(); } }

    }

    //public class SsasTabularModelElement : TabularModelElement
    //{
    //    public SsasTabularModelElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null) 
    //        : base(refPath, caption, definition, parent)
    //    {
    //    }
    //}

    public class SsasTabularDataSourceElement : TabularModelElement
    {
        public SsasTabularDataSourceElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null) 
            : base(refPath, caption, definition, parent)
        {
        }

        [DataMember]
        public string ServerName { get; set; }

        [DataMember]
        public string DbName { get; set; }
    }

    public class SsasTabularTableElement : TabularModelElement
    {
        public SsasTabularTableElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null) 
            : base(refPath, caption, definition, parent)
        {
        }

        public IEnumerable<SsasTabularTableColumnElement> Columns { get { return ChildrenOfType<SsasTabularTableColumnElement>(); } }
        public IEnumerable<SsasTabularMeasureElement> Measures { get { return ChildrenOfType<SsasTabularMeasureElement>(); } }

    }

    public class SsasTabularTableColumnElement : TabularModelElement
    {

        public SsasTabularTableColumnElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null) 
            : base(refPath, caption, definition, parent)
        {
        }

        
        public SsasTabularTableElement Table { get => (SsasTabularTableElement)Parent; }

        //[ModelLink]
        //public MssqlModelElement SqlColumn { get; set; }
    }

    public class SsasTabularAnnotationElement : TabularModelElement
    {
        public SsasTabularAnnotationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null) 
            : base(refPath, caption, definition, parent)
        {
        }
    }

    public class SsasTabularAttributeHierarchyElement : TabularModelElement
    {
        public SsasTabularAttributeHierarchyElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null) 
            : base(refPath, caption, definition, parent)
        {
        }
    }

    public class SsasTabularPartitionElement : TabularModelElement
    {
        public SsasTabularPartitionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null) 
            : base(refPath, caption, definition, parent)
        {
        }

        public IEnumerable<SsasTabularPartitionColumnElement> Columns { get { return ChildrenOfType<SsasTabularPartitionColumnElement>(); } }
    }

    public class SsasTabularPartitionColumnElement : TabularModelElement
    {
        public SsasTabularPartitionColumnElement(RefPath refPath, string caption, string definition, SsasTabularPartitionElement parent = null)
            : base(refPath, caption, definition, parent)
        {
        }

        [ModelLink]
        public MssqlModelElement SourceElement { get; set; }
        [ModelLink]
        public SsasTabularTableColumnElement TargetTableColumn { get; set; }
    }

    public class SsasTabularMeasureElement : TabularModelElement
    {
        public SsasTabularMeasureElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null) 
            : base(refPath, caption, definition, parent)
        {
        }
    }

    public class SsasTabularHierarchyElement : TabularModelElement
    {
        public SsasTabularHierarchyElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null) 
            : base(refPath, caption, definition, parent)
        {
        }
    }

    public class SsasTabularRelationshipElement : TabularModelElement
    {
        public SsasTabularRelationshipElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null) 
            : base(refPath, caption, definition, parent)
        {
        }

        [ModelLink]
        public SsasTabularTableColumnElement FromColumn { get; set; }

        [ModelLink]
        public SsasTabularTableColumnElement ToColumn { get; set; }

        [DataMember]
        public TabularRelationshipEndCardinality FromColumnCardinality { get; set; }

        [DataMember]
        public TabularRelationshipEndCardinality ToColumnCardinality { get; set; }

        [DataMember]
        public bool IsActive { get; set; }
    }

    public class SsasTabularPerspectiveElement : TabularModelElement
    {
        public SsasTabularPerspectiveElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null) 
            : base(refPath, caption, definition, parent)
        {
        }
    }

    public class SsasTabularHierarchyLevelElement : TabularModelElement
    {
        public SsasTabularHierarchyLevelElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null) 
            : base(refPath, caption, definition, parent)
        {
        }
    }
}
