using CD.DLS.DAL.Objects.SsisDiagram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.BIDoc.Core.Parse.Mssql.Ssis
{
    //class SsisXmlObjects
    //{
    //}


    public class SsisProject
    { 
        public List<SsisConnectionManager> ProjectConnectionManagers { get; set; }
        public List<SsisParameter> ProjectParameters { get; set; }
        public List<SsisPackage> Packages { get; set; }
    }

    public abstract class SsisObject
    {
        public string XmlDefinition { get; set; }

        public string RefId { get; set; }

        public List<SsisProperty> Properties { get; set; } = new List<SsisProperty>();

        public string GetPropertyValue(string name)
        {
            var prop = Properties.FirstOrDefault(x => x.Name == name);
            if (prop == null)
            {
                return null;
            }
            return prop.Value;
        }
    }

    public class SsisDesignObject : SsisObject
    {
        public ElementLayoutDesign Layout { get; set; }
    }

    public class SsisPackage : SsisDesignObject
    {
        //public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsisPackage;

        public string Urn { get; set; }
        //public string Name { get; set; }
        public List<SsisParameter> Parameters { get; set; } = new List<SsisParameter>();
        public List<SsisConnectionManager> ConnectionManagers { get; set; } = new List<SsisConnectionManager>();
        public SsisExecutable Executable { get; set; }
        public string PackageName { get; set; }
        //public override string Name => PackageName;
        //public string XmlDefinition { get; set; }

        public SsisProject Project { get; set; }

        public string FileName { get; set; }
    }

    public class SsisParameter : SsisObject
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
        public bool Sensitive { get; set; }
    }

    public enum SsisConnectionManagerScope
    {
        Package = 0,
        Project = 1
    }

    public class SsisConnectionManager : SsisObject
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string CreationName { get; set; }
        public string ConnectionString { get; set; }
        public SsisConnectionManagerScope Scope { get; set; }
    }

    public class SsisExecutable : SsisDesignObject
    {
        public string Name { get; set; }
        public List<SsisVariable> Variables { get; set; } = new List<SsisVariable>();
        //public List<SsisExpression> Expressions { get; set; }
        public List<SsisExecutable> Children { get; set; } = new List<SsisExecutable>();


        public bool Enabled { get; set; }
        public List<SsisPrecedenceConstraint> PrecedenceConstraints { get; set; } = new List<SsisPrecedenceConstraint>();
        public string CreationName { get; set; }
        public string ID { get; set; }
        //public string TypeName { get; set; }

    }

    public class SsisTask : SsisExecutable
    {
    }

    public class SsisExecuteSsisPackabeTask : SsisTask
    {

    }

    public class SsisExpressionTask : SsisTask
    {

    }

    public class SsisSqlTask : SsisTask
    {
        public string ConnectionName { get; set; }
        public string ConnectionID { get; set; }
        public string StatementSource { get; set; }
        public string StatementSourceType { get; set; }
        public List<SsisParameterMapping> Parameters { get; set; } = new List<SsisParameterMapping>();
    }

    public class SsisDfTask : SsisTask
    {
        public List<SsisDfComponent> Components { get; set; } = new List<SsisDfComponent>();
        public List<SsisDfPath> Paths { get; set; } = new List<SsisDfPath>();
    }

    public class SsisDfComponent : SsisDesignObject
    {
        public string Name { get; set; }

        //public string Contract { get; set; }
        //public string ContractBase { get; set; }
        //public string ObjectType { get; set; }
        //public string IdString { get; set; }
        public string ClassId { get; set; }
        //public int ID { get; set; }

        public List<SsisDfInput> Inputs { get; set; } = new List<SsisDfInput>();
        public List<SsisDfOutput> Outputs { get; set; } = new List<SsisDfOutput>();
        public List<SsisRuntimeConnection> Connections { get; set; } = new List<SsisRuntimeConnection>();
    }

    public class SsisRuntimeConnection : SsisObject
    {
        //public int ID { get; set; }
        //public string IdentificationString { get; set; }
        public string Name { get; set; }
        public string ConnectionManagerID { get; set; }
    }

    public class SsisDfPath : SsisObject
    {
        public string Name { get; set; }
        public string IdString { get; set; }
        public string SourceIdString { get; set; }
        public string TargetIdString { get; set; }

        public string SourceComponentRefId => SourceIdString.Substring(0, Math.Max(SourceIdString.LastIndexOf(".Inputs["), SourceIdString.LastIndexOf(".Outputs[")));

        public string TargetComponentRefId => TargetIdString.Substring(0, Math.Max(TargetIdString.LastIndexOf(".Inputs["), TargetIdString.LastIndexOf(".Outputs[")));
        public DesignArrow DesignArrow { get; set; }
    }

    public abstract class SsisDfInputOutput : SsisObject
    {
        //public int ID { get; set; }
        public string Name { get; set; }
        //public string Description { get; set; }
        //public string IdString { get; set; }

        public List<DfColumn> Columns { get; set; } = new List<DfColumn>();
        public List<DfColumn> ExternalColumns { get; set; } = new List<DfColumn>();

    }

    public class SsisDfOutput : SsisDfInputOutput
    {
        public bool IsErrorOutput { get; set; }
    }

    public class SsisDfInput : SsisDfInputOutput
    {

    }

    public class SsisPrecedenceConstraint : SsisObject
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string PrecedenceExecutableID { get; set; }
        public string ConstrainedExecutableID { get; set; }
        public DesignArrow DesignArrow { get; set; }
    }

    public class DfColumn : SsisObject
    {
        //public int ID { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        //public string IdentificationString { get; set; }
        public string DataType { get; set; } = string.Empty;
        public int Length { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public int CodePage { get; set; }
        //public int MappedColumnID { get; set; }
        public string ExternalColumnID { get; set; }
        public string LineageID { get; set; }

    }

    public enum SsisParameterDirection
    {
        Input = 1,
        Output = 2,
        ReturnValue = 4
    }

    public class SsisParameterMapping
    {
        public string Name { get; set; }
        public string Variablename { get; set; }
        public SsisParameterDirection Direction { get; set; }
    }

    public class SsisVariable : SsisObject
    {
        public string ID { get; set; }
        public string QualifiedName { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }
        public string Value { get; set; }
        public string Namespace { get; set; }
    }

    public class SsisProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Expression { get; set; }
    }

    public class ElementLayoutDesign
    {
        public DesignPoint TopLeft { get; set; }
        public DesignPoint Size { get; set; }
    }
}
