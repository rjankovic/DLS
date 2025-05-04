using CD.DLS.Common.Interfaces;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Engine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.DAL.Objects.BIDoc;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects;
using CD.DLS.DAL.Objects.SsrsStructures;
using System.Threading;

namespace CD.DLS.DAL.Managers
{
    public class GraphManager
    {

        private NetBridge _netBridge;
        public NetBridge NetBridge { get { return _netBridge; } }

        public GraphManager(NetBridge netBridge)
        {
            _netBridge = netBridge;
        }

        public GraphManager()
        {
            _netBridge = new NetBridge();
        }

        public void BuildAggregationsAsync(Guid projectId, Guid requestId)
        {
            NetBridge.ExecuteProcedureAsync("[BIDoc].[sp_BuildAggregations]", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId },
                { "requestId", requestId }
            });
        }

        public void BuildAggregations(Guid projectId, Guid requestId)
        {
            NetBridge.ExecuteProcedure("[BIDoc].[sp_BuildAggregations]", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId },
                { "requestId", requestId }
            });
        }
        /// <summary>
        /// Returns the first ID in a sequence of {elementCount} IDs reserved
        /// </summary>
        /// <param name="elementCount"></param>
        /// <returns></returns>
        public int GetModelElementsIdSequence(int elementCount)
        {
            return GetSequenceRange("BIDoc.ModelElementsSequence", elementCount);   
        }

        /// <summary>
        /// Reutrns the first ID in a sequence of {nodeCount} IDs reserved
        /// </summary>
        /// <param name="nodeCount"></param>
        /// <returns></returns>
        public int GetGraphNodesIdSequence(int nodeCount)
        {
            return GetSequenceRange("BIDoc.BasicGraphNodesSequence", nodeCount);
        }

        public int GetGraphLinksIdSequence(int nodeCount)
        {
            return GetSequenceRange("BIDoc.BasicGraphLinksSequence", nodeCount);
        }

        public int GetModelLinksIdSequence(int nodeCount)
        {
            return GetSequenceRange("BIDoc.ModelLinksSequence", nodeCount);
        }

        private int GetSequenceRange(string sequenceName, int elementCount)
        {
            /*
            sp_sequence_get_range[@sequence_name = ] N'<sequence>'
     , [@range_size = ]
        range_size  
     , [@range_first_value = ]
        range_first_value OUTPUT
            */

            var procOutputs = NetBridge.ExecuteProcedureWithOutParams("sp_sequence_get_range", new Dictionary<string, object>()
            {
                { "sequence_name", sequenceName},
                { "range_size", elementCount}
            }, new Dictionary<string, SqlDbType>()
            {
                { "range_first_value", SqlDbType.Variant }
            }
            );
            var rangeFirstValue = int.Parse(procOutputs["range_first_value"].ToString());
            return rangeFirstValue;
        }

        public Dictionary<int, int> SaveModelElements(List<BIDocModelElement> elements, List<BIDocModelLink> links, HashSet<int> premappedElementIds, bool enableUpdate = false)
        {
            var elementsDt = FlattenModelElements(elements);

            //Console.WriteLine("Saving elements to DB: " + elementsDt.AsEnumerable().Count());
            ConfigManager.Log.Info("Saving elements to DB: " + elementsDt.AsEnumerable().Count());

            string elementsSpName = "[BIDoc].[sp_AddOrUpdateElements]";
            if (!enableUpdate)
            {
                elementsSpName = "BIDoc.sp_AddElementsToModel";
            }

            Dictionary<int, int> elementIdMap = new Dictionary<int, int>();

            if (elements.Any())
            {
                var elemIdMapDt = NetBridge.ExecuteProcedureTable(elementsSpName, new Dictionary<string, object>()
            {
                { "projectconfigid", elements.First().ProjectConfigId },
                { "elements", elementsDt}
            });
                elementIdMap = ReadInsertedElementIds(elemIdMapDt);
            }


            foreach (var link in links)
            {
                // if the endpoint is premapped (already saved to DB), the link source / target does not need to be mapped
                if (!premappedElementIds.Contains(link.ElementFromId))
                {
                    link.ElementFromId = elementIdMap[link.ElementFromId];
                }
                if (!premappedElementIds.Contains(link.ElementToId))
                {
                    link.ElementToId = elementIdMap[link.ElementToId];
                }
            }
            var linksDt = FlattenModelLinks(links);

            //Console.WriteLine("Saving links to DB: " + linksDt.AsEnumerable().Count());
            ConfigManager.Log.Info("Saving links to DB: " + linksDt.AsEnumerable().Count());

            NetBridge.ExecuteProcedure("BIDoc.sp_AddLinksToModel", new Dictionary<string, object>()
            {
                { "links", linksDt }
            });

            Console.WriteLine("Done");

            return elementIdMap;
        }
        

        public void SaveGraphNodes(List<BIDocGraphInfoNode> nodes, List<BIDocGraphInfoLink> links, out Dictionary<int,int> newNodeIds)
        {
            var nodesDt = FlattenBasicGrphNodes(nodes);

            Dictionary<int, int> nodeIdMap = new Dictionary<int, int>();

            if (nodes.Any())
            {
                var mapTable = NetBridge.ExecuteProcedureTable("BIDoc.sp_AddNodesToGraph", new Dictionary<string, object>()
                {
                    { "projectconfigid", nodes.First().ProjectConfigId },
                    { "graphkind", nodes.First().GraphKind.ToString() },
                    { "nodes", nodesDt }
                });

                nodeIdMap = ReadInsertedGraphNodeIds(mapTable);
            }

            foreach (var link in links)
            {
                link.NodeFromId = nodeIdMap[link.NodeFromId];
                link.NodeToId = nodeIdMap[link.NodeToId];
            }
            var linksDt = FlattenBasicGraphLinks(links);

            NetBridge.ExecuteProcedure("BIDoc.sp_AddLinksToGraph", new Dictionary<string, object>()
            {
                { "links", linksDt }
            });

            newNodeIds = nodeIdMap;
        }

        public void ClearModel(Guid projectId, Guid requestId)
        {
            //NetBridge.ExecuteProcedureAsync("BIDoc.sp_ClearModel", new Dictionary<string, object>()
            //{
            //    { "projectconfigid", projectId },
            //    { "requestId", requestId }
            //});

            NetBridge.ExecuteProcedure("BIDoc.sp_ClearModel", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId },
                { "requestId", requestId }
            });
        }

        public void ClearModelPartWithAggregations(Guid projectId, string refPath)
        {
            NetBridge.ExecuteProcedure("BIDoc.sp_ClearModelPartWithAggregations", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId },
                { "path", refPath }
            });
        }


        public void SetRefPathIntervals(Guid projectId, Guid? requestId = null)
        {

            try
            {
                NetBridge.ExecuteProcedureAsync("BIDoc.sp_SetRefPathIntervals", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId },
                { "requestId", requestId }
            });

            }
            catch (Exception ex)
            {
                ConfigManager.Log.Error("Error setting ref path intervals", ex);
                ConfigManager.Log.Important("Waiting 120 s");
                Thread.Sleep(120000);
                ConfigManager.Log.Important("Retry");

                try
                {
                    NetBridge.ExecuteProcedureAsync("BIDoc.sp_SetRefPathIntervals", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId },
                { "requestId", requestId }
            });

                }
                catch (Exception ex2)
                {
                    ConfigManager.Log.Error("Error setting ref path intervals", ex2);
                    ConfigManager.Log.Important("Waiting 300 s");
                    Thread.Sleep(300000);
                    ConfigManager.Log.Important("Retry");

                    NetBridge.ExecuteProcedureAsync("BIDoc.sp_SetRefPathIntervals", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId },
                { "requestId", requestId }
            });
                }
            }
        }

        public void ClearGraph(Guid projectId, DependencyGraphKind graphKind)
        {
            NetBridge.ExecuteProcedure("BIDoc.sp_ClearGraph", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId },
                { "graphkind", graphKind.ToString() }
            });
        }
        
        public List<BIDocModelElement> GetModel(out List<BIDocModelLink> links, Guid projectId, string refPathPrefix = null, bool loadDefinitions = true)
        {
            DataTable elementsDt;
            DataTable linksDt;
            string elemFName;

            if (string.IsNullOrEmpty(refPathPrefix))
            {
                elemFName = "BIDoc.f_GetModelElements";
                if (!loadDefinitions)
                {
                    elemFName = "BIDoc.f_GetModelElementsNoDef";
                }

                elementsDt = NetBridge.ExecuteTableFunction(elemFName, new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId}
                });
                linksDt = NetBridge.ExecuteTableFunction("BIDoc.f_GetModelLinks", new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId }
                });
            }
            else
            {
                elemFName = "BIDoc.sp_GetModelElementsUnderPath";
                if (!loadDefinitions)
                {
                    elemFName = "BIDoc.sp_GetModelElementsUnderPathNoDef";
                }
                elementsDt = NetBridge.ExecuteProcedureTable(elemFName, new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId},
                    { "path", refPathPrefix },
                    { "type", DBNull.Value }
                });
                linksDt = NetBridge.ExecuteProcedureTable("BIDoc.sp_GetModelLinksUnderPath", new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId },
                    { "path", refPathPrefix }//,
                    //{ "linktype", DBNull.Value }
                });
            }

            var elements = ReadModelElements(elementsDt);
            links = ReadModelLinks(linksDt);
            return elements;
        }

        public List<BIDocModelElement> GetModelFromRootToChildrenOfType(out List<BIDocModelLink> links, Guid projectId, string refPathPrefix, Type childType, bool loadDefinitions = true)
        {
            DataTable elementsDt;
            DataTable linksDt;
            var elemFName = "BIDoc.sp_GetModelElementsUnderPathToChildrenOfType";
            if (!loadDefinitions)
            {
                elemFName = "BIDoc.sp_GetModelElementsUnderPathToChildrenOfTypeNoDef";
            }

                elementsDt = NetBridge.ExecuteProcedureTable(elemFName, new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId},
                    { "path", refPathPrefix },
                    { "type", childType.FullName }
                });
                linksDt = NetBridge.ExecuteProcedureTable("BIDoc.sp_GetModelLinksUnderPathToChildrenOfType", new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId },
                    { "path", refPathPrefix },
                    { "type", childType.FullName }
                });
        
            var elements = ReadModelElements(elementsDt);
            links = ReadModelLinks(linksDt);
            return elements;
        }

        public List<BIDocModelElement> GetModelFromRootToChildrenOfTypes(out List<BIDocModelLink> links, Guid projectId, string refPathPrefix, List<Type> childTypes, bool loadDefinitions = true)
        {
            DataTable elementsDt;
            DataTable linksDt;
            var elemFName = "BIDoc.sp_GetModelElementsUnderPathToChildrenOfTypes";
            if (!loadDefinitions)
            {
                elemFName = "BIDoc.sp_GetModelElementsUnderPathToChildrenOfTypesNoDef";
            }
            else
            {
                throw new NotImplementedException();
            }
            var typeList = GetStringListTable(childTypes.Select(x => x.FullName).ToList());

            elementsDt = NetBridge.ExecuteProcedureTable(elemFName, new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId},
                    { "path", refPathPrefix },
                    { "types", typeList }
                });
            linksDt = NetBridge.ExecuteProcedureTable("BIDoc.sp_GetModelLinksUnderPathToChildrenOfTypes", new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId },
                    { "path", refPathPrefix },
                    { "types", typeList }
                });

            var elements = ReadModelElements(elementsDt);
            links = ReadModelLinks(linksDt);
            return elements;
        }

        public List<BIDocModelElement> GetElementsById(out List<BIDocModelLink> links, List<int> idList)
        {
            DataTable elementsDt = GetIdListTable(idList);
            var resDt = NetBridge.ExecuteTableFunction("BIDoc.f_GetModelElementsByIds", new Dictionary<string, object>()
                {
                    { "idList", elementsDt}
                });

            var elements = ReadModelElements(resDt);
            var linksDt = NetBridge.ExecuteTableFunction("BIDoc.f_GetModelLinksFromElementIds", new Dictionary<string, object>()
                {
                    { "idList", elementsDt}
                });
            links = ReadModelLinks(linksDt);
            return elements;
        }

        public static DataTable GetIdListTable(List<int> ids)
        {
            DataTable res = new DataTable();
            res.TableName = "BIDoc.UDTT_IdList";
            res.Columns.Add("Id");
            foreach (int id in ids)
            {
                var nr = res.NewRow();
                nr[0] = id;
                res.Rows.Add(nr);
            }
            return res;
        }

        public static DataTable GetStringListTable(List<string> vals)
        {
            DataTable res = new DataTable();
            res.TableName = "BIDoc.UDTT_StringList";
            res.Columns.Add("Value");
            foreach (string val in vals)
            {
                var nr = res.NewRow();
                nr[0] = val;
                res.Rows.Add(nr);
            }
            return res;
        }

        public List<BIDocModelElement> GetModelElementsOfType(Guid projectId, Type elementType, string refPathPrefix = "")
        {
            DataTable elementsDt;
            

            elementsDt = NetBridge.ExecuteProcedureTable("BIDoc.sp_GetModelElementsUnderPath", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId },
                { "path", refPathPrefix },
                { "type", elementType.FullName }
            });
        
            var elements = ReadModelElements(elementsDt);
            return elements;
        }

        public List<BIDocModelElement> GetModelSecondLevelAncestor(out List<BIDocModelLink> links, int rootId)
        {
            //ConfigManager.Log.Info("Loading Second Level Ancestor Elements " + rootId.ToString());

                var elementsDt = NetBridge.ExecuteTableFunction("[BIDoc].[f_GetModelElementsSecondLevelAncestor]", new Dictionary<string, object>()
                {
                    { "rootId", rootId}
                });
                var linksDt = NetBridge.ExecuteTableFunction("[BIDoc].[f_GetModelLinksSecondLevelAncestors]", new Dictionary<string, object>()
                {
                    { "rootId", rootId }
                });
            
            var elements = ReadModelElements(elementsDt);
            links = ReadModelLinks(linksDt);

            //ConfigManager.Log.Info("Done Loading Second Level Ancestor Elements " + rootId.ToString());
            return elements;
        }

        public BIDocModelElement GetModelElementById(int id)
        {
            DataTable elementsDt;


            elementsDt = NetBridge.ExecuteTableFunction("BIDoc.f_GetModelElementById", new Dictionary<string, object>()
            {
                { "elementid", id }
            });

            var elements = ReadModelElements(elementsDt);
            return elements.First();
        }

        public BIDocModelElement GetModelElementByNodeId(int nodeId)
        {
            DataTable elementsDt;
            
            elementsDt = NetBridge.ExecuteTableFunction("BIDoc.f_GetModelElementByNodeId", new Dictionary<string, object>()
            {
                { "nodeId", nodeId }
            });

            var elements = ReadModelElements(elementsDt);
            return elements.First();
        }
        
        public List<BIDocGraphInfoNodeExtended> GetGraphExtended(out List<BIDocGraphInfoLink> links, Guid projectId, DependencyGraphKind graphKind, string refPathPrefix, string nodeType = null)
        {
            DataTable nodesDt;
            DataTable linksDt;
            
                nodesDt = NetBridge.ExecuteProcedureTable("BIDoc.sp_GetGraphNodesUnderPath", new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId },
                    { "graphkind", graphKind.ToString() },
                    { "path", refPathPrefix },
                    { "nodeType", nodeType }
                });

                linksDt = NetBridge.ExecuteProcedureTable("BIDoc.sp_GetGraphLinksUnderPath", new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId },
                    { "graphkind", graphKind.ToString() },
                    { "path", refPathPrefix },
                    { "linktype", DBNull.Value }
                });
            
            var nodes = InspectManager.ReadBasicGraphNodesExtended(nodesDt);
            links = ReadBasicGraphLinks(linksDt);
            return nodes;
        }

        public void RebindAnnotations(Guid projectId)
        {
            NetBridge.ExecuteProcedure("BIDoc.sp_RebindAnnotations", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId }
            });
        }

        public List<BIDocGraphInfoNodeExtended> GetNodesExtended(Guid projectId, DependencyGraphKind graphKind, string refPathPrefix, string nodeType = null)
        {
            DataTable nodesDt;
            DataTable linksDt;

            nodesDt = NetBridge.ExecuteProcedureTable("BIDoc.sp_GetGraphNodesUnderPath", new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId },
                    { "graphkind", graphKind.ToString() },
                    { "path", refPathPrefix },
                    { "nodeType", nodeType }
                });

            var nodes = InspectManager.ReadBasicGraphNodesExtended(nodesDt);
            return nodes;
        }


        public List<BIDocGraphInfoNode> GetGraph(out List<BIDocGraphInfoLink> links, Guid projectId, DependencyGraphKind graphKind)
        {
            DataTable nodesDt;
            DataTable linksDt;
            
                nodesDt = NetBridge.ExecuteTableFunction("BIDoc.f_GetGraphNodes", new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId },
                    { "graphkind", graphKind }
                });

                linksDt = NetBridge.ExecuteTableFunction("BIDoc.f_GetGraphLinks", new Dictionary<string, object>()
                {
                    { "projectconfigid", projectId },
                    { "graphkind", graphKind }
                });

            var nodes = ReadBasicGraphNodes(nodesDt);  //InspectManager.ReadBasicGraphNodesExtended(nodesDt);
            links = ReadBasicGraphLinks(linksDt);
            return nodes;
        }

        public string GetOlapFieldRefPath(Guid projectId, string refPathPrefix, string fieldname)
        {
            var path = (string)NetBridge.ExecuteProcedureScalar("Inspect.sp_FindOlapField", new Dictionary<string, object>()
            {
                { "CubePath", refPathPrefix },
                { "FieldName", fieldname}
            });
            return path;
            //var query = db.Database.SqlQuery<FindOlapFieldRequestResponse>("Inspect.sp_FindOlapField @cubePath, @fieldName",
            //    new SqlParameter("@cubePath", cubePath), new SqlParameter("@fieldName", request.FieldName));
        }

        /// <summary>
        /// Inserts the documents; if a document of the given type already exists for the node, it is overwritten.
        /// </summary>
        /// <param name="documents"></param>
        public void SaveGraphDocuments(List<BIDocGraphDocument> documents)
        {
            var documentsDt = FlattenGraphDocuments(documents);
            NetBridge.ExecuteProcedure("BIDoc.sp_AddDocumentsToGraph", new Dictionary<string, object>()
            {
                { "documents", documentsDt }
            });
        }

        public int GetGraphNodeId(Guid projectId, string refPath, DependencyGraphKind graphKind)
        {
            /*
             CREATE FUNCTION [BIDoc].[f_GetGraphNodeId]
   (
       @projectconfigid UNIQUEIDENTIFIER,
       @graphkind NVARCHAR(50),
       @refpath NVARCHAR(MAX)
             */
            var dt = NetBridge.ExecuteTableFunction("BIDoc.f_GetGraphNodeIdByRefPath", new Dictionary<string, object>()
             {
                 { "projectconfigid", projectId },
                 { "graphkind", graphKind.ToString() },
                 { "refpath", refPath }
             });
            return (int)(dt.Rows[0][0]);
        }

        public int GetGraphNodeId(int elementId, DependencyGraphKind graphKind)
        {
            /*
             CREATE FUNCTION [BIDoc].[f_GetGraphNodeId]
   (
       @projectconfigid UNIQUEIDENTIFIER,
       @graphkind NVARCHAR(50),
       @refpath NVARCHAR(MAX)
             */
            var dt = NetBridge.ExecuteTableFunction("BIDoc.f_GetGraphNodeIdByElementId", new Dictionary<string, object>()
             {
                 { "elementid", elementId },
                 { "graphkind", graphKind.ToString() }
             });
            return (int)(dt.Rows[0][0]);
        }


        public List<BIDocGraphDocument> GetGraphDocuments(int nodeId)
        {
            /*
             CREATE FUNCTION [BIDoc].[f_GetGraphDocumentsForNode]
(
	@nodeid INT,
	@documenttype NVARCHAR(50) = NULL
)
             */
            var documentsDt = NetBridge.ExecuteTableFunction("BIDoc.f_GetGraphDocumentsForNode", new Dictionary<string, object>()
             {
                 { "nodeid", nodeId},
                 { "documenttype", DBNull.Value }
             });
            var documents = ReadGraphDocuments(documentsDt);
            return documents;
        }

        public BIDocGraphDocument GetGraphDocument(int nodeId, DocumentTypeEnum documentType)
        {
            var documentsDt = NetBridge.ExecuteTableFunction("BIDoc.f_GetGraphDocumentsForNode", new Dictionary<string, object>()
             {
                 { "nodeid", nodeId},
                 { "documenttype", documentType }
             });
            var documents = ReadGraphDocuments(documentsDt);
            if (documents.Count > 0)
            {
                return documents.First();
            }
            return null;
        }

        public void ClearGraphDocuments(Guid projectid, DependencyGraphKind graphKind)
        {
            NetBridge.ExecuteProcedure("BIDoc.sp_ClearGraphDocuments", new Dictionary<string, object>()
            {
                { "projectconfigid", projectid },
                { "graphkind", graphKind }
            });

            /*
             CREATE PROCEDURE [BIDoc].[sp_ClearGraphDocuments]
	@projectconfigid UNIQUEIDENTIFIER,
	@graphkind NVARCHAR(50),
	@documentType NVARCHAR(50) = NULL
             */
        }

        public void ClearGraphDocuments(Guid projectid, DependencyGraphKind graphKind, DocumentTypeEnum documentType)
        {
            NetBridge.ExecuteProcedure("BIDoc.sp_ClearGraphDocuments", new Dictionary<string, object>()
            {
                { "projectconfigid", projectid },
                { "graphkind", graphKind },
                { "documentType", documentType }
            });

        }

        public int GetModelElementIdByRefPath(Guid projectConfigId, string refPath)
        {
            /*
            CREATE FUNCTION [BIDoc].[f_GetModelElementIdByRefPath]
(
	@projectconfigid UNIQUEIDENTIFIER,
	@path NVARCHAR(MAX)
)
             */

            var dt = NetBridge.ExecuteTableFunction("BIDoc.f_GetModelElementIdByRefPath", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId },
                { "path", refPath }
            });

            var r = dt.Rows[0];
            return (int)r["ModelElementId"];
        }

        public int? TryGetModelElementIdByRefPath(Guid projectConfigId, string refPath)
        {
            /*
            CREATE FUNCTION [BIDoc].[f_GetModelElementIdByRefPath]
(
	@projectconfigid UNIQUEIDENTIFIER,
	@path NVARCHAR(MAX)
)
             */

            var dt = NetBridge.ExecuteTableFunction("BIDoc.f_GetModelElementIdByRefPath", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId },
                { "path", refPath }
            });

            if (dt.Rows.Count == 1)
            {
                var r = dt.Rows[0];
                return (int)r["ModelElementId"];
            }
            return null;
        }
        
        public Dictionary<string, string> ListElemnetTypeClasses()
        {
            //BIDoc.ModelElementTypeClasses(ElementType, ClassCode)

            var dt = NetBridge.ExecuteSelectStatement("SELECT ElementType, ClassCode FROM BIDoc.ModelElementTypeClasses");

            return dt.AsEnumerable().ToDictionary(x => (string)x["ElementType"], x => (string)x["ClassCode"]);
        }

        public void CreateDataFlowMediumDetailGraph(Guid projectId)
        {
            NetBridge.ExecuteProcedure("[BIDoc].[sp_CreateDataFlowMediumDetailGraph]", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId }
            });
        }

        public void CreateDataFlowLowDetailGraph(Guid projectId)
        {
            NetBridge.ExecuteProcedure("[BIDoc].[sp_CreateDataFlowLowDetailGraph]", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId }
            });
        }

        private List<BIDocModelElement> ReadModelElements(DataTable table)
        {
            List<BIDocModelElement> elements = new List<BIDocModelElement>();
            foreach (DataRow row in table.Rows)
            {
                BIDocModelElement me = new BIDocModelElement();

                me.Id = (int)row["ModelElementId"];
                me.ExtendedProperties = (string)row["ExtendedProperties"];
                me.RefPath = (string)row["RefPath"];
                me.Definition = (row["Definition"] == DBNull.Value ? null : (string)row["Definition"]);
                me.Caption = row["Caption"] == DBNull.Value ? (string)null : (string)row["Caption"];
                me.Type = (string)row["Type"];
                me.ProjectConfigId = Guid.Parse(row["ProjectConfigId"].ToString());

                elements.Add(me);
            }

            return elements;

            /*
	[ModelElementId] [int] NOT NULL,
	[ExtendedProperties] [nvarchar](max) NULL,
	[RefPath] [nvarchar](max) NULL,
	[Definition] [nvarchar](max) NULL,
	[Caption] [nvarchar](max) NULL,
	[Type] [nvarchar](200) NULL,
	[ProjectConfigId] [uniqueidentifier] NOT NULL,
  */

        }

        private DataTable FlattenModelElements(List<BIDocModelElement> elements)
        {
            DataTable dt = new DataTable();
            dt.TableName = "BIDoc.UDTT_ModelElements";

            dt.Columns.Add("ModelElementId", typeof(int));
            dt.Columns.Add("ExtendedProperties", typeof(string));
            dt.Columns.Add("RefPath", typeof(string));
            dt.Columns.Add("Definition", typeof(string));
            dt.Columns.Add("Caption", typeof(string));
            dt.Columns.Add("Type", typeof(string));
            dt.Columns.Add("RefPathSuffix", typeof(string));
            //dt.Columns.Add("ProjectConfigId", typeof(Guid));

            int longestProps = 0;
            int longestRefPath = 0;
            int longestCaption = 0;
            int longestType = 0;
            int longestDefinition = 0;

            string longestDef = string.Empty;

            foreach (var e in elements)
            {
                var nr = dt.NewRow();

                nr[0] = e.Id;
                nr[1] = e.ExtendedProperties;
                nr[2] = e.RefPath;
                nr[3] = e.Definition;// == null ? null : e.Definition.Substring(0, Math.Min(100, e.Definition.Length));
                nr[4] = e.Caption == null ? (object)DBNull.Value : e.Caption;
                nr[5] = e.Type;
                nr[6] = e.RefPathSuffix;
                //nr[5] = ""; // e.Definition;
                
                //if (longestProps < e.ExtendedProperties.Length)
                //{
                //    longestProps = e.ExtendedProperties.Length;
                //}
                //if (longestRefPath < e.RefPath.Length)
                //{
                //    longestRefPath = e.RefPath.Length;
                //}
                //if (longestCaption < e.Caption.Length)
                //{
                //    longestCaption = e.Caption.Length;
                //}
                //if (longestType < e.Type.Length)
                //{
                //    longestType = e.Type.Length;
                //}
                //if (e.Definition != null)
                //{
                //    if (longestDefinition < e.Definition.Length)
                //    {
                //        longestDefinition = e.Definition.Length;
                //        longestDef = e.Definition;
                //    }
                //}
                //nr[5] = e.ProjectConfigId;

                dt.Rows.Add(nr);
            }

            return dt;
        }

        private List<BIDocModelLink> ReadModelLinks(DataTable table)
        {
            List<BIDocModelLink> links = new List<BIDocModelLink>();
            foreach (DataRow row in table.Rows)
            {
                BIDocModelLink ml = new BIDocModelLink();

                ml.Id = (int)row["ModelLinkId"];
                ml.ExtendedProperties = (string)row["ExtendedProperties"];
                ml.ElementFromId = (int)row["ElementFromId"];
                ml.ElementToId = (int)row["ElementToId"];
                ml.Type = (string)row["Type"];

                links.Add(ml);
            }

            return links;

            /*
	[ModelLinkId] [int] NULL,
	[ElementFromId] [int] NOT NULL,
	[ElementToId] [int] NOT NULL,
	[Type] [nvarchar](100) NULL,
	[ExtendedProperties] [nvarchar](max) NULL
  */

        }

        private DataTable FlattenModelLinks(List<BIDocModelLink> links)
        {
            DataTable dt = new DataTable();
            dt.TableName = "BIDoc.UDTT_ModelLinks";

            dt.Columns.Add("ModelLinkId", typeof(int));
            dt.Columns.Add("ElementFromId", typeof(int));
            dt.Columns.Add("ElementToId", typeof(int));
            dt.Columns.Add("Type", typeof(string));
            dt.Columns.Add("ExtendedProperties", typeof(string));

            foreach (var l in links)
            {
                var nr = dt.NewRow();

                nr[0] = l.Id;
                nr[1] = l.ElementFromId;
                nr[2] = l.ElementToId;
                nr[3] = l.Type;
                nr[4] = l.ExtendedProperties;

                dt.Rows.Add(nr);
            }

            return dt;
        }
        
        private List<BIDocGraphInfoNode> ReadBasicGraphNodes(DataTable table)
        {
            List<BIDocGraphInfoNode> nodes = new List<BIDocGraphInfoNode>();
            foreach (DataRow row in table.Rows)
            {
                BIDocGraphInfoNode me = new BIDocGraphInfoNode();

                me.Id = (int)row["BasicGraphNodeId"];
                me.Name = (string)row["Name"];
                me.GraphKind = (DependencyGraphKind)Enum.Parse(typeof(DependencyGraphKind), (string)row["GraphKind"]);
                me.Description = (string)row["Description"];
                me.NodeType = (string)row["Type"];
                me.ProjectConfigId = Guid.Parse(row["ProjectConfigId"].ToString());
                me.SourceElementId = (int)row["SourceElementId"];
                me.TopologicalOrder = (int)row["TopologicalOrder"];
                me.ParentId = (int)row["ParentId"];
                
                nodes.Add(me);
            }

            return nodes;

            /*
	[BasicGraphNodeId] [int] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[NodeType] [nvarchar](200) NULL,
	[Description] [nvarchar](max) NULL,
	[ParentId] [int] NULL,
	[GraphKind] [nvarchar](50) NOT NULL,
	[ProjectConfigId] [uniqueidentifier] NOT NULL,
	[SourceElementId] INT NOT NULL,
	[TopologicalOrder] [int] NULL,
  */

        }

        public List<LineageGridFavorite> GetLineageGridFavorites(Guid projectConfigId)
        {
            List<LineageGridFavorite> res = new List<LineageGridFavorite>();
            var userId = Identity.IdentityProvider.GetCurrentUser().UserId;

            var dt = NetBridge.ExecuteProcedureTable("[BIDoc].[sp_GetLineageGridFavorities]",
                new Dictionary<string, object>()
                {
                    { "userId", userId},
                    { "projectConfigId", projectConfigId }
                });

            foreach (DataRow dr in dt.Rows)
            {
                res.Add(new LineageGridFavorite()
                {
                    SourceElementType = (string)dr["SourceElementType"],
                    SourceRootDescriptivePath = (string)dr["SourceRootDescriptivePath"],
                    SourceRootElementId = (int)dr["SourceRootElementId"],
                    SourceTypeDescription = (string)dr["SourceTypeDescription"],
                    TargetElementType = (string)dr["TargetElementType"],
                    TargetRootDescriptivePath = (string)dr["TargetRootDescriptivePath"],
                    TargetRootElementId = (int)dr["TargetRootElementId"],
                    TargetTypeDescription = (string)dr["TargetTypeDescription"],
                    SourceRootElementPath = (string)dr["SourceRootElementPath"],
                    TargetRootElementPath = (string)dr["TargetRootElementPath"]
                });
            }
            
            return res;
        }

        public DataTable FlattenBasicGrphNodes(List<BIDocGraphInfoNode> nodes)
        {
            DataTable dt = new DataTable();
            dt.TableName = "BIDoc.UDTT_BasicGraphNodes";

            dt.Columns.Add("BasicGraphNodeId", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("NodeType", typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("ParentId", typeof(int));
            //dt.Columns.Add("GraphKind", typeof(string));
            //dt.Columns.Add("ProjectConfigId", typeof(Guid));
            dt.Columns.Add("SourceElementId", typeof(int));
            dt.Columns.Add("TopologicalOrder", typeof(int));

            foreach (var n in nodes)
            {
                var nr = dt.NewRow();

                nr[0] = n.Id;
                nr[1] = n.Name;
                nr[2] = n.NodeType;
                nr[3] = n.Description; //== null ? null : n.Description.Substring(0, Math.Min(1000, n.Description.Length));
                nr[4] = (n.ParentId != null ? (object)(n.ParentId) : DBNull.Value);
                //nr[5] = n.GraphKind.ToString();
                //nr[6] = n.ProjectConfigId;
                nr[5] = n.SourceElementId;
                nr[6] = n.TopologicalOrder;

                dt.Rows.Add(nr);
            }

            return dt;
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

        private DataTable FlattenBasicGraphLinks(List<BIDocGraphInfoLink> links)
        {
            DataTable dt = new DataTable();
            dt.TableName = "BIDoc.UDTT_BasicGraphLinks";

            dt.Columns.Add("BasicGraphLinkId", typeof(int));
            dt.Columns.Add("LinkType", typeof(string));
            dt.Columns.Add("NodeFromId", typeof(int));
            dt.Columns.Add("NodeToId", typeof(int));
            
            foreach (var l in links)
            {
                var nr = dt.NewRow();

                nr[0] = l.Id;
                nr[1] = l.LinkType;
                nr[2] = l.NodeFromId;
                nr[3] = l.NodeToId;
                
                dt.Rows.Add(nr);
            }

            return dt;
        }

        private List<BIDocGraphDocument> ReadGraphDocuments(DataTable table)
        {
            List<BIDocGraphDocument> documents = new List<BIDocGraphDocument>();
            foreach (DataRow row in table.Rows)
            {
                BIDocGraphDocument d = new BIDocGraphDocument();
                d.Id = (int)row["GraphDocumentId"];
                d.Content = (string)row["Content"];
                d.DocumentType = (DocumentTypeEnum)Enum.Parse(typeof(DocumentTypeEnum), (string)row["DocumentType"]);
                //d.NodeRefPath = (string)row["NodeRefPath"];
                d.GraphNodeId = (int)row["GraphNode_Id"];
                //d.GraphNode = new BIDocGraphInfoNode() { Id = (int)row["GraphNode_Id"] };

                documents.Add(d);
            }

            return documents;

            /*
CREATE TYPE [BIDoc].[GraphDocuments] AS TABLE(
	[GraphDocumentId] [int] NULL,
	[Content] [nvarchar](max) NULL,
	[DocumentType] NVARCHAR(50) NOT NULL,
	[GraphNode_Id] [int] NULL
)
  */
        }

        private DataTable FlattenGraphDocuments(List<BIDocGraphDocument> documents)
        {
            DataTable dt = new DataTable();
            dt.TableName = "BIDoc.UDTT_GraphDocuments";

            //dt.Columns.Add("GraphDocumentId", typeof(int));
            dt.Columns.Add("Content", typeof(string));
            dt.Columns.Add("DocumentType", typeof(string));
            dt.Columns.Add("GraphNode_Id", typeof(int));
            //dt.Columns.Add("NodeRefPath", typeof(string));

            foreach (var d in documents)
            {
                var nr = dt.NewRow();

                //nr[0] = d.Id;
                nr[0] = d.Content;
                nr[1] = d.DocumentType.ToString();
                nr[2] = d.GraphNodeId;
                //nr[3] = d.NodeRefPath;

                dt.Rows.Add(nr);
            }

            return dt;
        }


        public void PropagateDataFlowVertically(Guid projectConfigId)
        {

            NetBridge.ConnectionInfoMessage += NetBridge_ConnectionInfoMessage;

            NetBridge.ExecuteProcedure("BIDoc.sp_PropagateDataFlowVertically", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId }
            });

            NetBridge.ConnectionInfoMessage -= NetBridge_ConnectionInfoMessage;

        }

        public void BuildHighLevelGraph(Guid projectConfigId)
        {

            NetBridge.ConnectionInfoMessage += NetBridge_ConnectionInfoMessage;

            NetBridge.ExecuteProcedure("BIDoc.sp_BuildHighLevelGraph", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId }
            });

            NetBridge.ConnectionInfoMessage -= NetBridge_ConnectionInfoMessage;

        }

        public void CalculateTopologicalDataFlowOrder(Guid projectConfigId)
        {

            NetBridge.ConnectionInfoMessage += NetBridge_ConnectionInfoMessage;

            NetBridge.ExecuteProcedure("BIDoc.sp_CalculateTopologicalDataFlowOrder", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId }
            });

            NetBridge.ConnectionInfoMessage -= NetBridge_ConnectionInfoMessage;

        }
        
        public void BuildDataFlowSequences(Guid projectConfigId)
        {

            NetBridge.ConnectionInfoMessage += NetBridge_ConnectionInfoMessage;

            NetBridge.ExecuteProcedure("BIDoc.sp_BuildDataFlowSequences", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId }
            });

            NetBridge.ConnectionInfoMessage -= NetBridge_ConnectionInfoMessage;

        }

        public void BuildHigherDataFlowSequences(Guid projectConfigId)
        {
            NetBridge.ConnectionInfoMessage += NetBridge_ConnectionInfoMessage;


            NetBridge.ExecuteProcedure("BIDoc.sp_FillHigherLevelElementAncestors", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId }
            });

            NetBridge.ExecuteProcedure("BIDoc.sp_BuildHigherDataFlowSequences", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId }
            });

            NetBridge.ConnectionInfoMessage -= NetBridge_ConnectionInfoMessage;

        }

        public void ClenseDataFlowSequences(Guid projectConfigId)
        {

            NetBridge.ConnectionInfoMessage += NetBridge_ConnectionInfoMessage;

            NetBridge.ExecuteProcedure("BIDoc.sp_ClenseDataFlowSequences", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId }
            });

            NetBridge.ConnectionInfoMessage -= NetBridge_ConnectionInfoMessage;

        }

        public void RenameModelElement(int elementId, string newName)
        {
            NetBridge.ExecuteProcedure("[BIDoc].[sp_RenameModelElement]", new Dictionary<string, object>()
            {
                { "elementId", elementId },
                { "newName", newName }
            });
        }

        public void SetModelElementDescriptivePaths(Guid projectConfigId)
        {
            NetBridge.ConnectionInfoMessage += NetBridge_ConnectionInfoMessage;

            NetBridge.ExecuteProcedure("BIDoc.sp_SetModelElementDescriptivePaths", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId }
            });

            NetBridge.ConnectionInfoMessage -= NetBridge_ConnectionInfoMessage;
        }

        public void BuildTransitiveDataFlowGraph(Guid projectId)
        {
            /*
    CREATE PROCEDURE [BIDoc].[sp_BuildTransitiveGraph]
	@projectconfigid UNIQUEIDENTIFIER,
	@sourcegraphkind NVARCHAR(50),
	@targetgraphkind NVARCHAR(50),
	@linktype NVARCHAR(50)
AS
             */

            NetBridge.ConnectionInfoMessage += NetBridge_ConnectionInfoMessage;

            NetBridge.ExecuteProcedure("BIDoc.sp_BuildTransitiveGraph", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId },
                { "sourcegraphkind", "DataFlow" },
                { "targetgraphkind", "DataFlowTransitive" },
                { "linktype", "DataFlow" }
            });

            NetBridge.ConnectionInfoMessage -= NetBridge_ConnectionInfoMessage;

        }

        private void NetBridge_ConnectionInfoMessage(object sender, System.Data.SqlClient.SqlInfoMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }


        private Dictionary<int,int> ReadInsertedElementIds(DataTable table)
        {
            Dictionary<int, int> res = new Dictionary<int, int>();

            foreach (DataRow r in table.Rows)
            {
                var oldId = (int)r["SequentialId"];
                var newId = (int)r["ModelElementId"];
                //res[oldId] = newId;
                res.Add(oldId, newId);
            }

            return res;
        }
        
        private Dictionary<int, int> ReadInsertedGraphNodeIds(DataTable table)
        {
            Dictionary<int, int> res = new Dictionary<int, int>();

            foreach (DataRow r in table.Rows)
            {
                var oldId = (int)r["SequentialGraphNodeId"];
                var newId = (int)r["BasicGraphNodeId"];
                res.Add(oldId, newId);
            }

            return res;
        }
        

        public string GetModelElementDescriptivePath(int modelElementId)
        {
            var res = NetBridge.ExecuteScalarFunction("BIDoc.f_GetModelElementDescriptivePath", new Dictionary<string, object>()
             {
                 { "modelElementId", modelElementId }
             });
            return (string)(res);
        }

        public void FillDataMessages(Guid projectConfigId)
        {
            NetBridge.ConnectionInfoMessage += NetBridge_ConnectionInfoMessage;

            NetBridge.ExecuteProcedure("BIDoc.sp_FillDataMessages", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId }
            });

            NetBridge.ConnectionInfoMessage -= NetBridge_ConnectionInfoMessage;
        }

        public string GetType(int modelElementId)
        {
            string type;
            var resDt = NetBridge.ExecuteTableFunction("[BIDoc].[f_GetModelElementType]", new Dictionary<string, object>()
                {
                    { "modelElementId", modelElementId}
                });
            type = resDt.Rows[0][0].ToString();

            return type;
        }

        /*
         CREATE PROCEDURE [BIDoc].[sp_SaveLineageGridHistory]
	
	@ProjectConfigId UNIQUEIDENTIFIER,
	@SourceRootElementPath NVARCHAR(MAX),
	@TargetRootElementPath NVARCHAR(MAX),
	@SourceElementType NVARCHAR(MAX),
	@TargetElementType NVARCHAR(MAX),
	@SourceRootElementId INT,
	@TargetRootElementId INT,
	@UserId INT

AS
         */

        public List<SsrsReportListItem> ListSsrsReports(Guid projectConfigId)
        {
            var dt = NetBridge.ExecuteProcedureTable("BIDoc.sp_ListSsrsReports", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId }
            });


            List<SsrsReportListItem> res = new List<SsrsReportListItem>();

            foreach (DataRow dr in dt.Rows)
            {
                res.Add(new SsrsReportListItem()
                {
                    ModelElementId = (int)dr["ModelElementId"],
                    Name = (string)dr["Caption"],
                    RefPath = (string)dr["RefPath"],
                    SsrsPath = (string)dr["SsrsPath"],
                    SsrsComponentId = (int)dr["SsrsComponentId"]
                });
            }

            return res;
        }

        public void SaveLineageGridHistory(
            Guid projectConfigId,
            string sourceRootElementPath,
            string targetRootElementPath,
            string sourceElementType,
            string targetElementType,
            int sourceRootElementId,
            int targetRootElementId,
            int userId
            )
        {
            NetBridge.ExecuteProcedure("[BIDoc].[sp_SaveLineageGridHistory]", new Dictionary<string, object>()
            {
                { "ProjectConfigId", projectConfigId },
                { "SourceRootElementPath", sourceRootElementPath },
                { "TargetRootElementPath", targetRootElementPath },
                { "SourceElementType", sourceElementType },
                { "TargetElementType", targetElementType },
                { "SourceRootElementId", sourceRootElementId },
                { "TargetRootElementId", targetRootElementId },
                { "UserId", userId }
            });
        }

        //private List<OlapFieldsMLInputItem> ReadOlapFieldsMLInputItems(DataTable table)
        //{
        //    List<OlapFieldsMLInputItem> res = new List<OlapFieldsMLInputItem>();

        //    foreach (DataRow r in table.Rows)
        //    {
        //        var i = new OlapFieldsMLInputItem();

        //        i.FieldName = (string)r["FieldName"];
        //        i.FieldReference = (string)r["FieldReference"];
        //        i.FieldType = (OLAPFieldsMLInputItemType)Enum.Parse(typeof(OLAPFieldsMLInputItemType), r["FieldType"].ToString());
        //        i.GroupPath = (string)r["GroupPath"];
        //        i.OlapElementId = (int)r["OlapElementId"];
        //        i.ProjectConfigId = Guid.Parse(r["ProjectConfigId"].ToString());
        //        i.ReferenceRefPath = (string)r["RefPath"];

        //        res.Add(i);
        //    }

        //    return res;
        //}

        //public List<OlapFieldsMLInputItem> GetOlapFieldsMLInputItems(Guid projectConfigId)
        //{
        //    var resDt = NetBridge.ExecuteTableFunction("[BIDoc].[sp_OLAPFieldsMLInput]", new Dictionary<string, object>()
        //        {
        //            { "projectconfigid", projectConfigId}
        //        });

        //    var res = ReadOlapFieldsMLInputItems(resDt);
        //    return res;
        //}

    }
}