using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.Extract
{
    
    #region SSIS
    public class SsisProjectFile : ExtractObject
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsisProjectFile;
        
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public string Content { get; set; }

        public override string Name => FileName;
    }

    public class SsisPackageFile : ExtractObject
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsisPackageFile;

        public string FileName { get; set; }
        public string FullPath { get; set; }
        public string Content { get; set; }

        public override string Name => FileName;
    }

    public class SsisProjectConnectionManager : ExtractObject
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsisProjectConnectionManager;

        public override string Name => ConnectionManager.Name;

        public SsisConnectionManager ConnectionManager { get; set; }
    }

    public class SsisProjectParameters : ExtractObject
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsisProjectsParameters;

        public List<SsisParameter> Parameters { get; set; }

        public override string Name => "ProjectParameters";
    }

    public class SsisPackage : ExtractObject
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsisPackage;
        
        public string Urn { get; set; }
        //public string Name { get; set; }
        public List<SsisParameter> Parameters { get; set; }
        public List<SsisConnectionManager> ConnectionManagers { get; set; }
        public DtsContainer Container { get; set; }
        public string PackageName { get; set; }
        public override string Name => PackageName;
    }

    public class SsisParameter
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
    }

    public enum SsisConnectionManagerScope
    {
        Package = 0,
        Project = 1
    }

    public class SsisConnectionManager
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string CreationName { get; set; }
        public string ConnectionString { get; set; }
        public List<SsisProperty> Properties { get; set; }
        public SsisConnectionManagerScope Scope { get; set; }
    }

    public class DtsContainer
    {
        public string Name { get; set; }
        public List<SsisVariable> Variables { get; set; }
        //public List<SsisExpression> Expressions { get; set; }
        public List<DtsContainer> Children { get; set; }


        public bool Enabled { get; set; }
        public List<SsisProperty> Properties { get; set; }
        public List<SsisPrecedenceConstraint> PrecedenceConstraints { get; set; }
        public string CreationName { get; set; }
        public string ID { get; set; }
        //public string TypeName { get; set; }

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

    public class SsisTask : DtsContainer
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
        public List<SsisParameterMapping> Parameters { get; set; }
    }

    public class SsisDfTask : SsisTask
    {
        public List<SsisDfComponent> Components { get; set; }
        public List<SsisDfPath> Paths { get; set; }
    }

    public class SsisDfComponent
    {
        public string Name { get; set; }
        
        public string Contract { get; set; }
        public string ContractBase { get; set; }
        public string ObjectType { get; set; }
        public string IdString { get; set; }
        public string ClassId { get; set; }
        public List<SsisProperty> Properties { get; set; }
        public int ID { get; set; }

        public List<SsisDfInput> Inputs { get; set; }
        public List<SsisDfOutput> Outputs { get; set; }
        public List<SsisRuntimeConnection> Connections { get; set; }

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

    public class SsisRuntimeConnection
    {
        public int ID { get; set; }
        public string IdentificationString { get; set; }
        public string Name { get; set; }
        public string ConnectionManagerID { get; set; }
    }

    public class SsisDfPath
    {
        public string Name { get; set; }
        public string IdString { get; set; }
        public int ID { get; set; }
        public string SourceComponentIdString { get; set; }
        public string TargetComponentIdString { get; set; }
        public string SourceIdString { get; set; }
        public string TargetIdString { get; set; }
    }

    public abstract class SsisDfInputOutput
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IdString { get; set; }

        public List<DfColumn> Columns { get; set; }
        public List<DfColumn> ExternalColumns { get; set; }
    }
    
    public class SsisDfOutput : SsisDfInputOutput
    {
        public bool IsErrorOutput { get; set; }
    }

    public class SsisDfInput : SsisDfInputOutput
    {

    }

    public class SsisPrecedenceConstraint
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string PrecedenceExecutableID { get; set; }
        public string ConstrainedExecutableID { get; set; }
    }

    public class DfColumn
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string IdentificationString { get; set; }
        public string DataType { get; set; }
        public int Length { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public int CodePage { get; set; }
        public int MappedColumnID { get; set; }
        public int ExternalColumnID { get; set; }
        public List<SsisProperty> CustomProperties { get; set; }
        public int LineageID { get; set; }

        public string GetPropertyValue(string name)
        {
            var prop = CustomProperties.FirstOrDefault(x => x.Name == name);
            if (prop == null)
            {
                return null;
            }
            return prop.Value;
        }
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

    public class SsisVariable
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

    #endregion
    

}
