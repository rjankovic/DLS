using CD.DLS.Common.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Common.Structures
{
    public enum NodeTypeEnum
    {
        SqlDbServer, SqlDb, SqlSchema,
        SqlTable, SqlView, SqlUdf, SqlProcedure, SqlScript,
        SqlStatement, SqlDerivedTableSource, SqlColumn, SqlFk,
        SqlDbNode,

        SsisServer, SsisCatalog, SsisFolder, SsisProject,
        SsisProjectParameter, SsisPackageParameter, SsisPackageVariable,
        SsisConnectionManager,
        SsisPackage, SsisContainer, SsisGeneralTask, SsisExecuteSqlTask, SsisExecutePackageTask, SsisDataflowTask,
        SsisGeneralDatasource, SsisGeneralDestiantion, SsisGeneralTransformation, SsisDataflowComponent,
        SsisOleDbSource, SsisOleDbDestination, SsisLookup,
        SsisPrecedenceConstraint, SsisDataflowPath, SsisColumnCollection,
        SsisColumn,
        SsisAnotation, SsisNode,

        GeneralAgentNode, MssqlAgent, AgentStepCommand, AgentOnStepSucces, AgentOnStepFailure, AgentJob,
        AgentStepActiveScripting, AgentStepAnalysisCommand, AgentStepAnalysisQuery, AgentStepCmdExec, AgentStepDistribution,
        AgentStepLogReader, AgentStepMerge, AgentStepPowerShell, AgentStepQueueReader, AgentStepSnapshot, AgentStepSsis, AgentStepTsql,

        GeneralSsasNode, SsasServer, SsasDb,
        SsasDsv, SsasDsvTable, SsasDsvColumn,
        SsasDimension, SsasDimensionAttribute, SsasNameColumn, SsasValueColumn, SsasKeyColumn, SsasCustomRollupColumn,
        SsasCustomRollupPropertiesColumn, SsasUnaryOperatorColumn,
        SsasHierarchy, SsasHierarchyLevel,
        SsasCube, SsasCubeDimension,
        SsasMeasureGroup, SsasMeasureGroupDimension, SsasPartition,
        SsasRegularMGDimension, SsasReferenceMGDimension, SsasManyToManyMGDimension, SsasDegenerateMGDimension, SsasDataMiningMGDimension,
        SsasMGColumnBinding, SsasMeasure
    }
    
    
    public class BasicGraphInfo
    {
        public BasicGraphInfo()
        {
            Nodes = new List<BasicGraphInfoNode>();
            Links = new List<BasicGraphInfoLink>();
        }

        public List<BasicGraphInfoNode> Nodes { get; set; }
        public List<BasicGraphInfoLink> Links { get; set; }
    }

    public class BasicGraphInfoNode
    {
        public int Id { get; set; }
        public string RefPath { get; set; }
        public string Name { get; set; }
        public string NodeType { get; set; }
        public string DocumentRelativePath { get; set; }
        public string Description { get; set; }
        public int? ParentId { get; set; }
        [JsonIgnore]
        public Guid ProjectConfigId { get; set; }
    }

    public class BasicGraphInfoLink
    {
        public int Id { get; set; }
        public LinkTypeEnum LinkType { get; set; }
        public int NodeFromId { get; set; }
        public int NodeToId { get; set; }
    }



}
