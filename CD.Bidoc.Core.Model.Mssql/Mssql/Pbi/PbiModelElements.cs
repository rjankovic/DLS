using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Model.Interfaces;

namespace CD.DLS.Model.Mssql.Pbi
{
    abstract public class PbiModelElement : MssqlModelElement
    {
        public PbiModelElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null) : base(refPath, caption)
        {
            Definition = definition;
            Parent = parent;
        }

        public PbiModelElement(RefPath refPath, string caption)
           : base(refPath, caption)
        { }
    }

    public class TenantElement : PbiModelElement
    {
        public TenantElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null)
               : base(refPath, caption.ToString(), definition)
        { }

    }


    public class ReportElement : PbiModelElement
    {
        public ReportElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
               : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public string ReportName { get; set; }
    }


    // RJ: Never created??
    public class PbiColumnElement : PbiModelElement
    {
        public PbiColumnElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
               : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public string Querry { get; set; }

        [DataMember]
        public string ColumnName { get; set; }
    }

    public class FilterElement : PbiModelElement
    {
        public FilterElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
               : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public int HowCreated { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string FilterName { get; set; }

        [ModelLink]
        public MssqlModelElement Property { get; set; }
    }

    public class PbiTableElement : PbiModelElement
    {
        public PbiTableElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
               : base(refPath, caption, definition, parent)
        { }
     
        [DataMember]
        public string TableName { get; set; }
    }

    public class ConnectionElement : PbiModelElement
    {
        public ConnectionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
               : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public int ConnId { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Server { get; set; }

        [DataMember]
        public string Database { get; set; }
    }

    public class ReportSectionElement : PbiModelElement
    {
        public ReportSectionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
               : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public string SectionName { get; set; }

        [DataMember]
        public string DisplayName { get; set; }
    }

    public class VisualElement : PbiModelElement
    {
        public VisualElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
               : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public string Type { get; set; }

    }

    public class ProjectionElement : PbiModelElement
    {
        public ProjectionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
               : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public string Type { set; get; }

        [ModelLink]
        public PbiColumnElement Column { get; set; }
    }


}
