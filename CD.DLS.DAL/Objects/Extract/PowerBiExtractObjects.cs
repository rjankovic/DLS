using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.Extract
{

    public class Tenant : ExtractObject
    {
        public Tenant(string tenantID, List<Report> reports)
        {
            TenantID = tenantID;
            Reports = reports;
        }

        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.Tenant;
        public override string Name => TenantID.ToString();

        public string TenantID { get; set; }
        public List<Report> Reports { get; set; }
    }


    public class Report : ExtractObject
    {
        public Report(string name, List<Connection> connections, List<ReportSection> sections, Filter[] filters)
        {
            ReportName = name;
            Filters = filters;
            Connections = connections;
            Sections = sections;

        }
        public List<Connection> Connections { get; set; }
        public List<ReportSection> Sections { get; set; }
        public Filter[] Filters { get; set; }
        public string ReportName { get; set; }

        public override string Name => ReportName;

        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.PowerBiReport;
    }
    public class PbiColumn : ExtractObject
    {
        public PbiColumn(string name, string querry)
        {
            ColumnName = name;
            Querry = querry;
        }

        public string Querry { get; set; }
        public string ColumnName { get; set; }

        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.PowerBiColumn;

        public override string Name => ColumnName;
    }
    public class Connection : ExtractObject
    {

        public Connection(int connId, string type, string source, List<PbiTable> table)
        {
            ConnId = connId;
            Type = type;
            Source = source;
            Tables = new List<PbiTable>();
        }

        public int ConnId { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public List<PbiTable> Tables { get; set; }

        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.PowerBiConnection;

        public override string Name => Type;
    }
    public class Visual : ExtractObject
    {
        public Visual(string id, string type, Filter[] filters)
        {
            this.Id = id;
            this.Type = type;
            this.Projections = new List<Projection>();
            Filters = filters;
        }

        public List<Projection> Projections { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
        public Filter[] Filters { get; set; }

        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.PowerBiVisual;

        public override string Name => Type;

        public List<VisualExtensionMeasure> ExtensionMeasures { get; set; } = new List<VisualExtensionMeasure>();
    }

    public class VisualExtensionMeasure
    {   public string TableName { get; set; }
        public string MeasureName { get; set; }
        public string Expression { get; set; }
    }


    public class PbiTable : ExtractObject
    {
        public PbiTable(string name)
        {
            TableName = name;
        }

        public List<PbiColumn> Columns { get; set; }
        public string TableName { get; set; }

        public override string Name => TableName;

        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.PowerBiTable;
    }
    public class ReportSection : ExtractObject
    {
        public ReportSection(List<Visual> visuals, string name, string displayname, Filter[] filters)
        {
            Visuals = visuals;
            SectionName = name;
            Displayname = displayname;
            Filters = filters;
        }

        public List<Visual> Visuals { set; get; }
        public string SectionName { set; get; }

        public override string Name => SectionName;

        public string Displayname { set; get; }
        public Filter[] Filters { set; get; }

        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.PowerBiSection;
    }
    public class Projection : ExtractObject
    {
        public Projection(string queryRef, string type)
        {
            this.QueryRef = queryRef;
            this.Type = type;
        }

        public string QueryRef { get; set; }
        public string Type { get; set; }

        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.PowerBiProjection;

        public override string Name => QueryRef;
    }

    public class Filter : ExtractObject
    {

        public Filter(string name, expression expression, string type, int howCreated)
        {
            this.FilterName = name;
            this.Expression = expression;
            this.Type = type;
            this.HowCreated = howCreated;
        }

        public int HowCreated { get; set; }
        public string Type { get; set; }
        public string FilterName { get; set; }
        public expression Expression { get; set; }

        public override string Name => FilterName;

        public string Reference
        {
            get
            {

                if (Expression == null)
                    return null;
                if (Expression.Column == null)
                    return null;
                if (Expression.Column.Property == null)
                    return null;
                var r = "[" + Expression.Column.Property + "]";
                if (Expression.Column.Expression != null)
                {
                    if (Expression.Column.Expression.SourceRef != null)
                    {
                        if (Expression.Column.Expression.SourceRef.Entity != null)
                        {
                            r = "'" + Expression.Column.Expression.SourceRef.Entity + "'" + r;
                        }
                    }
                }

                return r;
            }
        }

        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.PowerBiFilter;



    }

    public class column : ExtractObject
    {
        public column(Expression expression, string property)
        {
            Expression = expression;
            Property = property;
        }
        public string Property { set; get; }
        public Expression Expression { set; get; }

        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.PowerBiFilterExpression;

        public override string Name => Property;
    }

    //--------------------------------------------------------------------------------------------------------------------



    public class Expression
    {
        public Expression(SourceRef sourceRef)
        {
            SourceRef = sourceRef;
        }
        public SourceRef SourceRef { get; set; }

    }
    public class SourceRef
    {
        public SourceRef(string entity)
        {
            Entity = entity;
        }
        public string Entity { get; set; }


    }
    public class expression
    {
        public expression(column column)
        {
            Column = column;
        }
        public column Column { get; set; }

    }




}
