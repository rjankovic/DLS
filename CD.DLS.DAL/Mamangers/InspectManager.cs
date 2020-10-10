using CD.DLS.DAL.Engine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CD.DLS.DAL.Objects.BIDoc;
using CD.DLS.DAL.Objects.Inspect;
using CD.DLS.Common.Structures;

namespace CD.DLS.DAL.Managers
{
    public class InspectManager
    {
        
        private NetBridge _netBridge;
        public NetBridge NetBridge { get { return _netBridge; } }
        public InspectManager(NetBridge netBridge)
        {
            _netBridge = netBridge;
        }

        public InspectManager()
        {        
                _netBridge = new NetBridge();          
        }

        public List<FoundOlapFiled> FindOlapField(string cubePath, string fieldName)
        {
            /*
  CREATE PROCEDURE Inspect.sp_FindOlapField
    @cubePath NVARCHAR(MAX), 
	@fieldName NVARCHAR(MAX)	
AS
             */

            var resDt = NetBridge.ExecuteProcedureTable("Inspect.sp_FindOlapField",
                new Dictionary<string, object>()
                {
                    { "cubePath", cubePath },
                    { "fieldName", fieldName }
                });

            var res = ReadFoundOlapFields(resDt);
            return res;
        }

        public List<FoundOlapFiled> ReadFoundOlapFields(DataTable dt)
        {
            List<FoundOlapFiled> res = new List<FoundOlapFiled>();

            foreach (DataRow r in dt.Rows)
            {
                res.Add(new FoundOlapFiled()
                {
                    ProjectConfigId = (Guid)r["ProjectConfigId"],
                    RefPath = (string)r["RefPath"],
                    ModelElementId = (int)r["ModelElementId"]
                });
            }

            return res;
        }


        public BIDocGraphInfoNodeExtended GetGraphNodeExtended(int nodeId)
        {
            var nodeDt = NetBridge.ExecuteTableFunction("Inspect.f_GetGraphNodeExtended", new Dictionary<string, object>()
             {
                 { "nodeId", nodeId}
             });
            var nodes = ReadBasicGraphNodesExtended(nodeDt);
            return nodes.First();
        }

        public List<BIDocGraphInfoNodeExtended> GraphNodeLineageOriginOneLevel(int nodeId)
        {
            var nodesDt = NetBridge.ExecuteTableFunction("Inspect.f_GraphNodeLineageOriginOneLevel", new Dictionary<string, object>()
             {
                 { "nodeId", nodeId}
             });
            var nodes = ReadBasicGraphNodesExtended(nodesDt);
            return nodes;
        }


        public List<BIDocGraphInfoNodeExtended> ElementHighLevelSourcesTransitive(int elementId)
        {
            var nodesDt = NetBridge.ExecuteTableFunction("[Inspect].[f_GetHighLevelLineageSources]", new Dictionary<string, object>()
             {
                 { "elementId", elementId}
             });
            var nodes = ReadBasicGraphNodesExtended(nodesDt);
            return nodes;
        }

        public List<BIDocGraphInfoNodeExtended> ElementHighLevelDestinationsTransitive(int elementId)
        {
            var nodesDt = NetBridge.ExecuteTableFunction("[Inspect].[f_GetHighLevelLineageDestinations]", new Dictionary<string, object>()
             {
                 { "elementId", elementId}
             });
            var nodes = ReadBasicGraphNodesExtended(nodesDt);
            return nodes;
        }

        public List<BIDocGraphInfoNodeExtended> GetGraphNodeChildrenExtended(int parentId)
        {

            /*
             CREATE FUNCTION [Inspect].[f_GetGraphNodeChildrenExtended]
    (
        @parentId INT
    )
             */

            var nodeDt = NetBridge.ExecuteTableFunction("Inspect.f_GetGraphNodeChildrenExtended", new Dictionary<string, object>()
             {
                 { "parentId", parentId}
             });
            var nodes = ReadBasicGraphNodesExtended(nodeDt);
            return nodes;
        }

        public List<BIDocGraphInfoNodeExtended> GetGraphExplorerSuggestions(Guid projectConfigId, DependencyGraphKind graphKind, string pattern = null)
        {
            var nodeDt = NetBridge.ExecuteProcedureTable("[Inspect].[sp_GetGraphExplorerSuggestions]", new Dictionary<string, object>()
             {
                 { "projecconfigid", projectConfigId},
                { "graphkind", graphKind },
                { "pattern", pattern }
             });
            var nodes = ReadBasicGraphNodesExtended(nodeDt);
            return nodes;
        }

        public List<BIDocGraphInfoNodeExtended> GetExtendedGraphExplorerSuggestions(Guid projectConfigId, DependencyGraphKind graphKind, string pattern = null)
        {
            var nodeDt = NetBridge.ExecuteProcedureTable("[Inspect].[sp_GetExtendedGraphExplorerSuggestions]", new Dictionary<string, object>()
             {
                 { "projecconfigid", projectConfigId},
                { "graphkind", graphKind },
                { "pattern", pattern }
             });
            var nodes = ReadBasicGraphNodesExtended(nodeDt);
            return nodes;
        }

        public List<BIDocGraphInfoNodeExtended> GetGraphExtended(out List<BIDocGraphInfoLink> links, Guid projectId, DependencyGraphKind graphKind)
        {
            DataTable nodesDt;
            DataTable linksDt;

            bool forceSeek = graphKind == DependencyGraphKind.DFHigh;

            nodesDt = NetBridge.ExecuteTableFunction(forceSeek ? "Inspect.f_GetGraphNodesExtended_ForceSeek" : "Inspect.f_GetGraphNodesExtended", new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId },
                    { "graphkind", graphKind.ToString() }
                });

            linksDt = NetBridge.ExecuteTableFunction("BIDoc.f_GetGraphLinks", new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId },
                    { "graphkind", graphKind.ToString() },
                    {"linktype", null }
                });

            var nodes = ReadBasicGraphNodesExtended(nodesDt);  //InspectManager.ReadBasicGraphNodesExtended(nodesDt);
            links = ReadBasicGraphLinks(linksDt);
            return nodes;
        }


        public List<BIDocGraphInfoLink> ReadBasicGraphLinks(DataTable table)
        {
            List<BIDocGraphInfoLink> links = new List<BIDocGraphInfoLink>();
            foreach (DataRow row in table.Rows)
            {
                BIDocGraphInfoLink gl = new BIDocGraphInfoLink();
                gl.Id = (int)row["BasicGraphLinkId"];
                gl.LinkType = (LinkTypeEnum)Enum.Parse(typeof(LinkTypeEnum), (string)row["LinkType"]);
                gl.NodeFromId = (int)row["NodeFromId"];
                gl.NodeToId = (int)row["NodeToId"];
                gl.ExtendedProperties = (row["ExtendedProperties"] == DBNull.Value ? null : (string)row["ExtendedProperties"]);
                links.Add(gl);
            }

            return links;

            /*
	[BasicGraphLinkId] [int] IDENTITY(1,1) NOT NULL,
	[LinkType] NVARCHAR(50) NOT NULL,
	[NodeFromId] [int] NOT NULL,
	[NodeToId] [int] NOT NULL,
  */

        }

        public List<BIDocGraphInfoNodeExtended> GetNodesExtended(List<int> nodeIds)
        {
            /*
             CREATE FUNCTION [Inspect].[f_GetGraphNodesByIdExtended]
(
	@nodeIds [BIDoc].[UDTT_IdList] READONLY
)
             */

            var idTable = GraphManager.GetIdListTable(nodeIds);

            var nodesDt = NetBridge.ExecuteTableFunction("[Inspect].[f_GetGraphNodesByIdExtended]", new Dictionary<string, object>()
                {
                    { "nodeIds", idTable },
                });

            var nodes = ReadBasicGraphNodesExtended(nodesDt);
            return nodes;
        }

        public List<BIDocGraphInfoNodeExtended> TranslateDataFlowLNodeDetailLevel(int nodeId, int sourceLevel, int targetLevel)
        {
            /*
CREATE PROCEDURE [Inspect].[sp_TranslateDataFlowLNodeDetailLevel]
@nodeId INT,
@sourceDetailLevel INT,
@targetDetailLevel INT
             */


            var nodesDt = NetBridge.ExecuteProcedureTable("[Inspect].[sp_TranslateDataFlowNodeDetailLevel]", new Dictionary<string, object>()
                {
                    { "nodeId", nodeId },
                    { "sourceDetailLevel", sourceLevel},
                    { "targetDetailLevel", targetLevel}
                });

            var nodes = ReadBasicGraphNodesExtended(nodesDt);
            //if (nodes is null)
            //{

            //}
            return nodes;
        }

        public List<ReportElementListItem> ListModelReports(Guid projectId)
        {
            var reportsDt = NetBridge.ExecuteTableFunction("Inspect.f_ListModelReports", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId }
            });

            List<ReportElementListItem> res = new List<ReportElementListItem>();
            foreach (DataRow r in reportsDt.Rows)
            {
                var item = new ReportElementListItem();

                item.Caption = (string)r["Caption"];
                item.ItemPath = (string)r["ItemPath"];
                item.ModelElementId = (int)r["ModelElementId"];
                item.RefPath = (string)r["RefPath"];

                res.Add(item);
            }
            return res;
        }

        /* BIDocMessages */

        public List<WarningMessagesItem> GetWarningMessages(Guid projectId)
        {
            var wmDt = NetBridge.ExecuteTableFunction("BIDoc.f_GetMessages", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId }
            });
            var dataFlow = ReadWarningMessages(wmDt);
            return dataFlow;
        }

        /*
         SELECT 
DISTINCT s.BasicGraphNodeId SourceNodeId, s.Name SourceNodeName, s.RefPath SourceNodePath, s.SourceElementId SourceElementId, 
t.BasicGraphNodeId TargetNodeId, t.Name TargetNodeName, t.RefPath TargetNodePath, t.SourceElementId TargetElementId
FROM BIDoc.BasicGraphLinks l
INNER JOIN #sourceDescendants sd ON sd.BasicGraphNodeId = l.NodeFromId
INNER JOIN #targetDescendants td ON td.BasicGraphNodeId = l.NodeToId
INNER JOIN #sourceNodes s ON s.BasicGraphNodeId = sd.OriginalNodeId
INNER JOIN #targetNodes t ON t.BasicGraphNodeId = td.OriginalNodeId
WHERE l.LinkType = 'DataFlow'

            [Inspect].[sp_GetDataFlowBetweenGroups]
            	@projectConfigId UNIQUEIDENTIFIER,
	@sourcePrefix NVARCHAR(MAX),
	@targetPrefix NVARCHAR(MAX),
	@sourceType NVARCHAR(200),
	@targetType NVARCHAR(200)
         */
        public List<Tuple<BIDocGraphInfoNodeExtended, BIDocGraphInfoNodeExtended>> GetDataFlowBetweenGroups(Guid projectConfigId,
            string sourcePrefix, string targetPrefix, string sourceType, string targetType)
        {
            var dfDt = NetBridge.ExecuteProcedureTable("[Inspect].[sp_GetDataFlowBetweenGroups]", new Dictionary<string, object>()
             {
                 { "projectConfigId", projectConfigId},
                { "sourcePrefix", sourcePrefix },
                { "targetPrefix", targetPrefix },
                { "sourceType", sourceType },
                { "targetType", targetType }
             });
            var dataFlow = ReadDataFlowBetweenGroups(dfDt, projectConfigId);
            return dataFlow;
        }


        public List<DataFlowBetweenGroupsItem> GetDataFlowBetweenGroupsFlat(Guid projectConfigId,
            string sourcePrefix, string targetPrefix, string sourceType, string targetType)
        {
            var dfDt = NetBridge.ExecuteProcedureTable("[Inspect].[sp_GetDataFlowBetweenGroups]", new Dictionary<string, object>()
             {
                 { "projectConfigId", projectConfigId},
                { "sourcePrefix", sourcePrefix },
                { "targetPrefix", targetPrefix },
                { "sourceType", sourceType },
                { "targetType", targetType }
             });
            var dataFlow = ReadDataFlowBetweenGroupsFlat(dfDt);
            return dataFlow;
        }


        public static List<BIDocGraphInfoNodeExtended> ReadBasicGraphNodesExtended(DataTable table)
        {
            List<BIDocGraphInfoNodeExtended> nodes = new List<BIDocGraphInfoNodeExtended>();
            foreach (DataRow row in table.Rows)
            {
                BIDocGraphInfoNodeExtended me = new BIDocGraphInfoNodeExtended();

                me.Id = (int)row["BasicGraphNodeId"];
                me.Name = (string)row["Name"];
                me.GraphKind = (DependencyGraphKind)Enum.Parse(typeof(DependencyGraphKind), (string)row["GraphKind"]);
                me.Description = (row["Description"] == DBNull.Value ? null : (string)row["Description"]);
                me.NodeType = (string)row["NodeType"];
                me.ProjectConfigId = Guid.Parse(row["ProjectConfigId"].ToString());
                me.SourceElementId = (int)row["SourceElementId"];
                me.TopologicalOrder = row["TopologicalOrder"] == DBNull.Value ? 0 : Convert.ToInt32(row["TopologicalOrder"]);
                me.ParentId = row["ParentId"] == DBNull.Value ? 0 : (int)row["ParentId"];
                me.RefPath = (string)row["RefPath"];
                me.TypeDescription = (string)row["TypeDescription"];
                me.ElementType = (string)row["ElementType"];
                me.DescriptivePath = row["DescriptivePath"] == DBNull.Value ? string.Empty : (string)row["DescriptivePath"];
                nodes.Add(me);
            }

            return nodes;

        }

        public List<Tuple<BIDocGraphInfoNodeExtended, BIDocGraphInfoNodeExtended>> ReadDataFlowBetweenGroups(DataTable table, Guid projectConfigId)
        {
            /*
             DISTINCT s.BasicGraphNodeId SourceNodeId, s.Name SourceNodeName, s.RefPath SourceNodePath, s.SourceElementId SourceElementId, 
t.BasicGraphNodeId TargetNodeId, t.Name TargetNodeName, t.RefPath TargetNodePath, t.SourceElementId TargetElementId

             */

            List<Tuple<BIDocGraphInfoNodeExtended, BIDocGraphInfoNodeExtended>> tuples = new List<Tuple<BIDocGraphInfoNodeExtended, BIDocGraphInfoNodeExtended>>();
            foreach (DataRow row in table.Rows)
            {
                BIDocGraphInfoNodeExtended from = new BIDocGraphInfoNodeExtended();
                BIDocGraphInfoNodeExtended to = new BIDocGraphInfoNodeExtended();

                from.Id = (int)row["SourceNodeId"];
                to.Id = (int)row["TargetNodeId"];

                from.Name = (string)row["SourceNodeName"];
                to.Name = (string)row["TargetNodeName"];

                from.GraphKind = DependencyGraphKind.ControlFlowTransitive;
                to.GraphKind = DependencyGraphKind.ControlFlowTransitive;

                from.ProjectConfigId = projectConfigId;
                to.ProjectConfigId = projectConfigId;

                from.SourceElementId = (int)row["SourceElementId"];
                to.SourceElementId = (int)row["TargetElementId"];

                from.RefPath = (string)row["SourceNodePath"];
                to.RefPath = (string)row["TargetNodePath"];

                tuples.Add(new Tuple<BIDocGraphInfoNodeExtended, BIDocGraphInfoNodeExtended>(from, to));
            }

            return tuples;
        }

        public List<DataFlowBetweenGroupsItem> ReadDataFlowBetweenGroupsFlat(DataTable table)
        {
            List<DataFlowBetweenGroupsItem> res = new List<DataFlowBetweenGroupsItem>();
            foreach (DataRow row in table.Rows)
            {
                DataFlowBetweenGroupsItem item = new DataFlowBetweenGroupsItem();

                item.SourceNodeId = (int)row["SourceNodeId"];
                item.TargetNodeId = (int)row["TargetNodeId"];

                item.SourceNodeName = (string)row["SourceNodeName"];
                item.TargetNodeName = (string)row["TargetNodeName"];

                item.SourceElementId = (int)row["SourceElementId"];
                item.TargetElementId = (int)row["TargetElementId"];

                item.SourceElementRefPath = (string)row["SourceNodePath"];
                item.TargetElementRefPath = (string)row["TargetNodePath"];

                item.SourceDescriptivePath = (string)row["SourceDescriptivePath"];
                item.TargetDescriptivePath = (string)row["TargetDescriptivePath"];

                res.Add(item);
            }

            return res;
        }

        public List<WarningMessagesItem> ReadWarningMessages(DataTable table)
        {
        List<WarningMessagesItem> res = new List<WarningMessagesItem>();
            foreach (DataRow r in table.Rows)
            {
                var item = new WarningMessagesItem();

                item.SourceName = (string)r["SourceName"];
                item.SourcePath = (string)r["SourcePath"];
                item.TargetName = (string)r["TargetName"];
                item.TargetPath = (string)r["TargetPath"];
                item.Message = (string)r["Message"];
                item.DataMessageType = (string)r["DataMessageType"];

                res.Add(item);
            }
            return res;
        }

        public List<ElementTreeListItem> ReadElementTreeListItems(DataTable dt)
        {
            List<ElementTreeListItem> res = new List<ElementTreeListItem>();
            HashSet<int> availableParents = new HashSet<int>();
            foreach (DataRow r in dt.Rows)
            {
                ElementTreeListItem item = new ElementTreeListItem()
                {
                    Caption = (string)r["Caption"],
                    Alias = (string)r["Caption"],
                    ModelElementId = (int)r["ModelElementId"],
                    TypeDescription = (string)r["TypeDescription"],
                    Type = (string)r["Type"],
                    MaxParentLevel = (int)r["MaxParentLevel"],
                    ParentElementId = (r["ParentElementId"] == DBNull.Value ? (int?)null : (int)r["ParentElementId"]),
                    RefPath = (string)r["RefPath"]
                };
                availableParents.Add(item.ModelElementId);
                res.Add(item);
            }
            foreach (var r in res)
            {
                if (r.ParentElementId.HasValue)
                {
                    if (!availableParents.Contains(r.ParentElementId.Value))
                    {
                        r.ParentElementId = null;
                    }
                }
            }
            return res;
        }

        /*
     ModelElementId	Caption	Type	TypeDescription	MaxParentLevel	ParentElementId	RefPath
3	NDWH_L1	CD.BIDoc.Core.Model.Mssql.Db.DatabaseElement	SQL Database	2	2	Server[@Name='FSCZPRCT0041']/Database[@Name='NDWH_L1']

    CREATE PROCEDURE [Inspect].[sp_GetSolutionMidSubtree]
@projectConfigId UNIQUEIDENTIFIER,
@rootElementId INT

    CREATE FUNCTION Inspect.f_GetHiglLevelSolutionTree
(@projectConfigId UNIQUEIDENTIFIER)
RETURNS TABLE
AS RETURN

AS

     */


        public List<ElementTreeListItem> GetSolutionMidSubtree(Guid projectConfigId,
    int rootElementId)
        {
            var dt = NetBridge.ExecuteProcedureTable("[Inspect].[sp_GetSolutionMidSubtree]", new Dictionary<string, object>()
             {
                 { "projectConfigId", projectConfigId},
                { "rootElementId", rootElementId }
             });
            var res = ReadElementTreeListItems(dt);
            return res;
        }

        public List<ElementTreeListItem> GetHighLevelSolutionTree(Guid projectConfigId)
        {
            var dt = NetBridge.ExecuteTableFunction("[Inspect].[f_GetHighLevelSolutionTree]", new Dictionary<string, object>()
             {
                 { "projectConfigId", projectConfigId}
             });
            var res = ReadElementTreeListItems(dt);
            return res;
        }

        public List<ElementTreeListItem> GetBusinessTree(Guid projectConfigId)
        {
            var dt = NetBridge.ExecuteTableFunction("[Inspect].[f_GetBusinessTree]", new Dictionary<string, object>()
             {
                 { "projectConfigId", projectConfigId}
             });
            var res = ReadElementTreeListItems(dt);
            return res;
        }

        public List<ElementTypeDescription> GetHighLevelTypesUnderElement(Guid projectConfigId, int rootElementId)
        {
            var dt = NetBridge.ExecuteTableFunction("[Inspect].[f_GetHighLevelTypesUnderElement]", new Dictionary<string, object>()
             {
                 { "projectConfigId", projectConfigId},
                { "rootElementId", rootElementId}
             });
            var res = ReadElementTypeDescriptions(dt);
            return res;
        }

        public List<ElementTypeDescription> ReadElementTypeDescriptions(DataTable dt)
        {
            List<ElementTypeDescription> res = new List<ElementTypeDescription>();
            foreach (DataRow r in dt.Rows)
            {
                ElementTypeDescription item = new ElementTypeDescription()
                {
                    ElementType = (string)r["ElementType"],
                    NodeType = (string)r["NodeType"],
                    TypeDescription = (string)r["TypeDescription"]
                };
                res.Add(item);
            }
            return res;
        }

        /* BIDocGraphInfoLink */

        public List<BIDocGraphInfoLink> GetDataFlowLinksBetweenNodes(Guid projectId, int sourceNodeId, int targetNodeId, int detailLevel)
        {
            var dt = NetBridge.ExecuteProcedureTable("[Inspect].[sp_GetDataFlowLinksBetweenNodes]", new Dictionary<string, object>()
             {
                 { "sourceNodeId", sourceNodeId},
                { "targetNodeId", targetNodeId},
                { "detailLevel", detailLevel},
                { "projectConfigId", projectId }
             });
            var res = ReaddataFlowLinksBetweenNodes(dt, projectId, sourceNodeId, targetNodeId);
            return res;
        }
        
        public List<BIDocGraphInfoLink> ReaddataFlowLinksBetweenNodes(DataTable dt, Guid projectConfigId,
            int sourceNodeId, int targetNodeId)
        {
            /* u.BasicGraphLinkId, u.NodeFromId, u.NodeToId */

            List<BIDocGraphInfoLink> res = new List<BIDocGraphInfoLink>();
            foreach (DataRow r in dt.Rows)
            {
                BIDocGraphInfoLink item = new BIDocGraphInfoLink()
                {
                    ProjectConfigId = projectConfigId,
                    LinkType = Common.Structures.LinkTypeEnum.DataFlow,
                    NodeFromId = (int)r["SourceNodeId"],
                    NodeToId = (int)r["TargetNodeId"],
                    Id = (int)r["SequenceStepId"]
                };

                //// exclude outbound links that are consequences of (cut) cyclic dataflow
                //if (item.NodeFromId == targetNodeId || item.NodeToId == sourceNodeId)
                //{
                //    continue;
                //}

                res.Add(item);
            }
            return res;
        }

        public List<BIDocGraphInfoLink> GetDataFlowLinksFromNode(Guid projectId, int sourceNodeId)
        {
            var dt = NetBridge.ExecuteProcedureTable("[Inspect].[sp_GetDataFlowLinksFromNode]", new Dictionary<string, object>()
             {
                 { "sourceNodeId", sourceNodeId}
             });
            var res = ReadBasicDataFlowLinks(dt, projectId);
            return res;
        }

        public List<BIDocGraphInfoLink> GetDataFlowLinksToNode(Guid projectId, int sourceNodeId)
        {
            var dt = NetBridge.ExecuteProcedureTable("[Inspect].[sp_GetDataFlowLinksToNode]", new Dictionary<string, object>()
             {
                 { "targetNodeId", sourceNodeId}
             });
            var res = ReadBasicDataFlowLinks(dt, projectId);
            return res;
        }

        //public List<BIDocGraphInfoLink> TranslateDataFlowLinksBetweenGraphs(List<BIDocGraphInfoLink> originalLinks, DependencyGraphKind targetGraphKind)
        //{
        //    var dt = NetBridge.ExecuteProcedureTable("[Inspect].[sp_GetDataFlowLinksToNode]", new Dictionary<string, object>()
        //     {
        //         { "targetNodeId", sourceNodeId}
        //     });
        //    var res = ReadBasicDataFlowLinks(dt, projectId);
        //    return res;
        //}

        private List<BIDocGraphInfoLink> ReadBasicDataFlowLinks(DataTable dt, Guid projectId)
        {
            List<BIDocGraphInfoLink> res = new List<BIDocGraphInfoLink>();
            foreach (DataRow r in dt.Rows)
            {
                BIDocGraphInfoLink item = new BIDocGraphInfoLink()
                {
                    ProjectConfigId = projectId,
                    LinkType = Common.Structures.LinkTypeEnum.DataFlow,
                    NodeFromId = (int)r["NodeFromId"],
                    NodeToId = (int)r["NodeToId"],
                    Id = (int)r["BasicGraphLinkId"]
                };
                res.Add(item);
            }
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="linkIds"></param>
        /// <param name="sourceLevel">1 = high detail, 2 = medium, 3 = low detail</param>
        /// <param name="targetLevel"></param>
        /// <returns></returns>
        public List<BIDocGraphInfoLink> TranslateDataFlowLinksDetailLevel(Guid projectId, List<int> linkIds, int sourceLevel, int targetLevel)
        {
            /*
            CREATE PROCEDURE [Inspect].[sp_TranslateDataFlowLinksDetailLevel]
@linkIds [BIDoc].[UDTT_IdList] READONLY,
@sourceDetailLevel INT,
@targetDetailLevel INT
             */

            var linkIdTable = GraphManager.GetIdListTable(linkIds);
            
            var dt = NetBridge.ExecuteProcedureTable("[Inspect].[sp_TranslateDataFlowLinksDetailLevel]", new Dictionary<string, object>()
             {
                 { "linkIds", linkIdTable},
                { "sourceDetailLevel", sourceLevel},
                { "targetDetailLevel", targetLevel}
             });
            var res = ReadBasicDataFlowLinks(dt, projectId);
            return res;
        }


        /*
CREATE PROCEDURE [Inspect].[sp_GetDataFlowLinksAmongNodes]
@nodeIds [BIDoc].[UDTT_IdList] READONLY
AS
         */
        public List<BIDocGraphInfoLink> GetDataFlowLinksAmongNodes(Guid projectId, List<int> nodeIds)
        {
            var nodeIdTable = GraphManager.GetIdListTable(nodeIds);

            var dt = NetBridge.ExecuteProcedureTable("[Inspect].[sp_GetDataFlowLinksAmongNodes]", new Dictionary<string, object>()
             {
                 { "nodeIds", nodeIdTable}
             });
            var res = ReadBasicDataFlowLinks(dt, projectId);
            return res;
        }


        public int? GetVisualNodeAncestor(int nodeId)
        {
            var res = NetBridge.ExecuteProcedureScalar("[Inspect].[sp_GetVisualNodeAncestor]", new Dictionary<string, object>()
             {
                 { "nodeId", nodeId}
             });
            return res as int?;
        }

        public DataTable GetDataTable(string connectionString, string schemaTable)
        {
            DataTable res = NetBridge.ExecuteSelectStatement("SELECT TOP 100 * FROM " + schemaTable, connectionString);
            return res;
        }

        public List<DfSource> ListExternalDfSources(Guid projectId)
        {
            var sourcesDt = NetBridge.ExecuteTableFunction("[Inspect].[f_ListExternalDfSources]", new Dictionary<string, object>()
             {
                 { "projectconfigid", projectId}
             });

            var columnsDt = NetBridge.ExecuteTableFunction("[Inspect].[f_ListExternalDfSourceColumns]", new Dictionary<string, object>()
             {
                 { "projectconfigid", projectId}
             });

            List<DfSource> res = ReadDfSources(sourcesDt, columnsDt);

            return res;
        }

        public List<DfSource> ReadDfSources(DataTable sourcesDt, DataTable columnsDt)
        {

            /*
        ModelElementId INT,
        Command NVARCHAR(MAX),
        RefPath NVARCHAR(MAX),
        Caption NVARCHAR(MAX),
        ManagerCaption NVARCHAR(MAX),
        SourceType NVARCHAR(MAX),
        ConnectionString NVARCHAR(MAX),
        LocaleID INT,
        CodePage INT,
        FileFormat NVARCHAR(MAX),
        PackageElementId INT,
        PackageRefPath NVARCHAR(MAX),
        PackageCaption NVARCHAR(MAX)
             */

            List<DfSource> res = new List<DfSource>();
            Dictionary<int, DfSource> sourcesByElementId = new Dictionary<int, DfSource>();

            foreach (DataRow r in sourcesDt.Rows)
            {
                DfSource src = new DfSource()
                {
                    ModelElementId = (int)r["ModelElementId"],
                    Command = r["Command"] as string,
                    Refpath = (string)r["RefPath"],
                    Name = (string)r["Caption"],
                    ManagerName = (string)r["ManagerCaption"],
                    SourceType = (string)r["SourceType"],
                    ConnectionString = (string)r["ConnectionString"],
                    LocaleID = int.Parse(r["LocaleID"] == DBNull.Value ? "0" : (string)r["LocaleID"]),
                    CodePage = int.Parse(r["CodePage"] == DBNull.Value ? "0" : (string)r["CodePage"]),
                    FileFormat = r["FileFormat"] as string,
                    PackageElementId = (int)r["PackageElementId"],
                    PackageRefPath = (string)r["PackageRefPath"],
                    PackageName = (string)r["PackageCaption"],
                    Columns = new List<DfSourceColumn>()
                };
                res.Add(src);
                sourcesByElementId.Add(src.ModelElementId, src);
            }

            /*
 * 
SELECT se.ModelElementId SourceElementId, ce.ModelElementId, ce.Caption ColumnName
,JSON_VALUE(ce.ExtendedProperties, '$.DtsDataType') DataType
,JSON_VALUE(ce.ExtendedProperties, '$.Length') Length
,JSON_VALUE(ce.ExtendedProperties, '$.Precision') Precision
,JSON_VALUE(ce.ExtendedProperties, '$.Scale') Scale
,ce.RefPath
*/
            foreach (DataRow r in columnsDt.Rows)
            {
                DfSourceColumn col = new DfSourceColumn()
                {
                    ColumnName = (string)r["ColumnName"],
                    DataType = (string)r["DataType"],
                    Length = int.Parse(r["Length"] == DBNull.Value ? "0" : (string)r["Length"]),
                    Precision = int.Parse((string)r["Precision"]),
                    Scale = int.Parse((string)r["Scale"]),
                    ModelElementId = (int)r["ModelElementId"]
                };

                var sourceId = (int)r["SourceElementId"];
                sourcesByElementId[sourceId].Columns.Add(col);
            }


            return res;
        }

        /*
         CREATE FUNCTION [Inspect].[f_GetElementTypeDetailLevel]
(
	@elementType NVARCHAR(255)
)

         */
        public int GetElementTypeDetailLevel(string typeName)
        {
            var res = NetBridge.ExecuteScalarFunction("[Inspect].[f_GetElementTypeDetailLevel]", new Dictionary<string, object>()
            {
                { "elementType", typeName }
            });

            return (int)res;
        }
        
        public List<ModelElementListingItem> ListElementsUnderPath(Guid projectId, string refPath, string elementType)
        {
            var dt = NetBridge.ExecuteProcedureTable("[Inspect].[sp_GetModelElementsUnderPathDisplay]", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId },
                { "path", refPath },
                { "type", elementType}
            });

            List<ModelElementListingItem> res = new List<ModelElementListingItem>();
            foreach (DataRow dr in dt.Rows)
            {
                var item = new ModelElementListingItem()
                {
                    ModelElementId = (int)dr["ModelElementId"],
                    RefPath = (string)dr["RefPath"],
                    Caption = (string)dr["Caption"],
                    Type = (string)dr["Type"],
                    TypeDescription = (string)dr["TypeDescription"],
                    DescriptivePath = (string)dr["DescriptivePath"]
                };
                res.Add(item);
            }
            return res;
        }

        public AnnotatedDependencySet GetCloseAnnotatedSources(int modelElementId)
        {
            AnnotatedDependencySet res = null;

            DataTable dt = NetBridge.ExecuteProcedureTable("[Inspect].[sp_GetCloseAnnotatedSources]", new Dictionary<string, object>()
            {
                { "modelElementId", modelElementId}
            });

            /*
             * ModelElementId	ModelElementName	TypeDescription	FieldName	FieldValue
             * 12529742	Contract Building Unit GLA	Cube Measure	Description	Pronajatá plocha v budovách dle GLA
             */

            if (dt.Rows.Count == 0)
            {
                return res;
            }

            res = new AnnotatedDependencySet() { ElementsTable = new DataTable(), FieldNames = new List<string>() };

            res.FieldNames = dt.AsEnumerable().Select(x => (string)x["FieldName"]).Distinct().ToList();
            var elementGroups = dt.AsEnumerable().GroupBy(x => (int)x["ModelElementId"]).Select(
                g => new
                {
                    ModelElementId = g.Key,
                    ModelElementName = (string)g.First()["ModelElementName"],
                    TypeDescription = (string)g.First()["TypeDescription"],
                    Fields = g.ToDictionary(x => (string)x["FieldName"], x => (string)x["FieldValue"])
                });

            res.ElementsTable.Columns.Add("ModelElementId", typeof(int));
            res.ElementsTable.Columns.Add("ModelElementName", typeof(string));
            res.ElementsTable.Columns.Add("TypeDescription", typeof(string));
            foreach (var field in res.FieldNames)
            {
                res.ElementsTable.Columns.Add(field, typeof(string));
            }

            foreach (var element in elementGroups)
            {
                var nr = res.ElementsTable.NewRow();

                nr["ModelElementId"] = element.ModelElementId;
                nr["ModelElementName"] = element.ModelElementName;
                nr["TypeDescription"] = element.TypeDescription;
                foreach (var field in res.FieldNames)
                {
                    nr[field] = element.Fields.ContainsKey(field) ? element.Fields[field] : string.Empty;
                }

                res.ElementsTable.Rows.Add(nr);
            }

            return res;
        }
        
    }
    
}
