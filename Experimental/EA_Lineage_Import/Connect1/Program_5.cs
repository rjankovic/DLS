//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Connect1
//{
//    public struct EATableProps
//    {
//        public string Name;
//        public int ColumnCount;
//        public int Id;
//        public List<string> Columns;
//    }

    
//    class Program
//    {
//        public const string F_GET_GRAPH_NODES_UNDER_PATH = "[BIDoc].[f_GetGraphNodesUnderPath]";
//        public const string SP_GET_DATA_FLOW_BETWEEN_GROUPS = "Inspect.sp_GetDataFlowBetweenGroups";
//        public const string L1_REF_PATH = "Server[@Name='FSCZPRCT0041']/Database[@Name='NDWH_L1']";
//        public const string L2_REF_PATH = "Server[@Name='FSCZPRCT0041']/Database[@Name='NRWH_L2']";
//        public const string SSIS_REF_PATH = "IntegrationServices[@Name='FSCZPRCT0041']/Catalog[@Name='SSISDB']/CatalogFolder[@Name='NRWH_SSIS']/ProjectInfo[@Name='NRWH_SSIS']";
//        public const string OLAP_REF_PATH = "SSASServer[@Name='FSCZPRCT0041']/Db[@Name='NRWH_OLAP']";
//        public const string OLAP_CUBE_REF_PATH = "SSASServer[@Name='FSCZPRCT0041']/Db[@Name='NRWH_OLAP']/Cube[@Name='NRWH']";
//        public const string TABLE_ELEM_TYPE = "SchemaTableElement";
//        public const string PACKAGE_ELEM_TYPE = "PackageElement";
//        public const string OLAP_DIM_ELEM_TYPE = "DimensionElement";
//        public const string OLAP_CUBE_DIM_ELEM_TYPE = "CubeDimensionElement";
//        public const string OLAP_DIM_ATTR_ELEM_TYPE = "DimensionAttributeElement";
//        public const string VIEW_ELEM_TYPE = "ViewElement";
//        public const string MG_ELEM_TYPE = "MeasureGroupElement";
//        public const string P_MEASURE_ELEM_TYPE = "PhysicalMeasureElement";
//        public const string DFT_GRPH_KIND = "DataFlowTransitive";

//        static void Main(string[] args)
//        {

//            Console.WriteLine("Loading data");

//            string connStringEA = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=Enterprise_Architect_NOIS;Data Source=fsczprsa0010;";
//            string connStringFmw = @"Data Source=fsczprct0041;Initial Catalog=CDFramework;Integrated Security=True";
//            Guid fmwProjectId = new Guid("18258D08-66CC-4B97-A695-226C7BA64AFE");

//            // get link & package lists
//            NetBridge.UseTemporaryConnstring(connStringFmw);

//            /*
//             CREATE PROCEDURE [Inspect].[sp_GetDataFlowBetweenGroups]
//	@projectConfigId UNIQUEIDENTIFIER,
//	@sourcePrefix NVARCHAR(MAX),
//	@targetPrefix NVARCHAR(MAX),
//	@sourceType NVARCHAR(200),
//	@targetType NVARCHAR(200)
//AS
//--DECLARE @sourcePrefix NVARCHAR(MAX) = N'IntegrationServices[@Name=''FSCZPRCT0041'']/Catalog[@Name=''SSISDB'']/CatalogFolder[@Name=''NRWH_SSIS'']/ProjectInfo[@Name=''NRWH_SSIS'']'
//--DECLARE @targetPrefix NVARCHAR(MAX) = N'Server[@Name=''FSCZPRCT0041'']/Database[@Name=''NRWH_L2'']'
//--DECLARE @sourceType NVARCHAR(200) = N'PackageElement'
//--DECLARE @targetType NVARCHAR(200) = N'SchemaTableElement'

//            s.BasicGraphNodeId SourceNodeId, s.Name SourceNodeName, s.RefPath SourceNodePath,
//t.BasicGraphNodeId TargetNodeId, t.Name TargeNodeName, t.RefPath TargeNodePath
//             */


//            var linksFromL1Table = NetBridge.ExecuteProcedureTable("Inspect.sp_GetDataFlowBetweenGroups", new Dictionary<string, object>()
//            {
//                { "projectConfigId", fmwProjectId },
//                { "sourcePrefix", L1_REF_PATH },
//                { "targetPrefix", SSIS_REF_PATH },
//                { "sourceType", TABLE_ELEM_TYPE },
//                { "targetType", PACKAGE_ELEM_TYPE}
//            });


//            var linksToL2Table = NetBridge.ExecuteProcedureTable("Inspect.sp_GetDataFlowBetweenGroups", new Dictionary<string, object>()
//            {
//                { "projectConfigId", fmwProjectId },
//                { "sourcePrefix", SSIS_REF_PATH },
//                { "targetPrefix", L2_REF_PATH },
//                { "sourceType", PACKAGE_ELEM_TYPE },
//                { "targetType", TABLE_ELEM_TYPE}
//            });

//            /*
//             CREATE FUNCTION [BIDoc].[f_GetGraphNodesUnderPath]
//(
//	@projectconfigid UNIQUEIDENTIFIER,
//	@graphkind NVARCHAR(50),
//	@path NVARCHAR(MAX),
//	@nodeType NVARCHAR(200) = NULL
//)
//RETURNS TABLE AS RETURN
//(
//SELECT n.[BasicGraphNodeId]
//      ,n.[Name]
//      ,n.[NodeType]
//      ,n.[Description]
//      ,n.[ParentId]
//      ,n.[GraphKind]
//      ,n.[ProjectConfigId]
//      ,n.[SourceElementId]
//      ,n.[TopologicalOrder]
//	  ,e.[RefPath]
//             */

//            var packageList = NetBridge.ExecuteTableFunction("[BIDoc].[f_GetGraphNodesUnderPath]", new Dictionary<string, object>()
//            {
//                { "projectConfigId", fmwProjectId },
//                { "graphkind", DFT_GRPH_KIND },
//                { "path", SSIS_REF_PATH },
//                { "nodeType", PACKAGE_ELEM_TYPE }
//            });

//            var dimensionList = NetBridge.ExecuteTableFunction("[BIDoc].[f_GetGraphNodesUnderPath]", new Dictionary<string, object>()
//            {
//                { "projectConfigId", fmwProjectId },
//                { "graphkind", DFT_GRPH_KIND },
//                { "path", OLAP_REF_PATH },
//                { "nodeType", OLAP_DIM_ELEM_TYPE }
//            });

//            var dimensionAttributeList = NetBridge.ExecuteTableFunction("[BIDoc].[f_GetGraphNodesUnderPath]", new Dictionary<string, object>()
//            {
//                { "projectConfigId", fmwProjectId },
//                { "graphkind", DFT_GRPH_KIND },
//                { "path", OLAP_REF_PATH },
//                { "nodeType", OLAP_DIM_ATTR_ELEM_TYPE }
//            });

//            var cubeDimensionList = NetBridge.ExecuteTableFunction("[BIDoc].[f_GetGraphNodesUnderPath]", new Dictionary<string, object>()
//            {
//                { "projectConfigId", fmwProjectId },
//                { "graphkind", DFT_GRPH_KIND },
//                { "path", OLAP_CUBE_REF_PATH },
//                { "nodeType", OLAP_CUBE_DIM_ELEM_TYPE }
//            });

//            var mgList = NetBridge.ExecuteTableFunction("[BIDoc].[f_GetGraphNodesUnderPath]", new Dictionary<string, object>()
//            {
//                { "projectConfigId", fmwProjectId },
//                { "graphkind", DFT_GRPH_KIND },
//                { "path", OLAP_CUBE_REF_PATH },
//                { "nodeType", MG_ELEM_TYPE }
//            });

//            var mgMeasureList = NetBridge.ExecuteTableFunction("[BIDoc].[f_GetGraphNodesUnderPath]", new Dictionary<string, object>()
//            {
//                { "projectConfigId", fmwProjectId },
//                { "graphkind", DFT_GRPH_KIND },
//                { "path", OLAP_CUBE_REF_PATH },
//                { "nodeType", P_MEASURE_ELEM_TYPE }
//            });


//            var linksToOlapDbDimensions = NetBridge.ExecuteProcedureTable("Inspect.sp_GetDataFlowBetweenGroups", new Dictionary<string, object>()
//            {
//                { "projectConfigId", fmwProjectId },
//                { "sourcePrefix", L2_REF_PATH },
//                { "targetPrefix", OLAP_REF_PATH },
//                { "sourceType", TABLE_ELEM_TYPE },
//                { "targetType", OLAP_DIM_ELEM_TYPE }
//            });

//            var linksToCubeDimensions = NetBridge.ExecuteProcedureTable("Inspect.sp_GetDataFlowBetweenGroups", new Dictionary<string, object>()
//            {
//                { "projectConfigId", fmwProjectId },
//                { "sourcePrefix", OLAP_REF_PATH },
//                { "targetPrefix", OLAP_CUBE_REF_PATH },
//                { "sourceType", OLAP_DIM_ELEM_TYPE },
//                { "targetType", OLAP_CUBE_DIM_ELEM_TYPE }
//            });

//            var linksFromL2ToMGs = NetBridge.ExecuteProcedureTable("Inspect.sp_GetDataFlowBetweenGroups", new Dictionary<string, object>()
//            {
//                { "projectConfigId", fmwProjectId },
//                { "sourcePrefix", L2_REF_PATH },
//                { "targetPrefix", OLAP_CUBE_REF_PATH },
//                { "sourceType", TABLE_ELEM_TYPE },
//                { "targetType", MG_ELEM_TYPE }
//            });

//            var linksToOlapViews = NetBridge.ExecuteProcedureTable("Inspect.sp_GetDataFlowBetweenGroups", new Dictionary<string, object>()
//            {
//                { "projectConfigId", fmwProjectId },
//                { "sourcePrefix", L2_REF_PATH },
//                { "targetPrefix", L2_REF_PATH },
//                { "sourceType", TABLE_ELEM_TYPE },
//                { "targetType", VIEW_ELEM_TYPE }
//            });

//            var linksFromViewsToMGs = NetBridge.ExecuteProcedureTable("Inspect.sp_GetDataFlowBetweenGroups", new Dictionary<string, object>()
//            {
//                { "projectConfigId", fmwProjectId },
//                { "sourcePrefix", L2_REF_PATH },
//                { "targetPrefix", OLAP_CUBE_REF_PATH },
//                { "sourceType", VIEW_ELEM_TYPE },
//                { "targetType", MG_ELEM_TYPE }
//            });

//            // get EA data from DB
//            NetBridge.UseTemporaryConnstring(connStringEA);

            
//            var l1PackageId = GetPackageId("Sandbox/Radovan Jankovic/NDWH_L1");
//            var l2PackageId = GetPackageId("Sandbox/Radovan Jankovic/NRWH_L2");
//            var ssisPackageId = GetPackageId("Sandbox/Radovan Jankovic/NRWH_SSIS");
//            var diagPackageId = GetPackageId("Sandbox/Radovan Jankovic/NRWH_Diagrams");
//            var dbDimensionsPackageId = GetPackageId("Sandbox/Radovan Jankovic/NRWH_OLAP/Dimensions");
//            var cubeDimensionsPackageId = GetPackageId("Sandbox/Radovan Jankovic/NRWH_OLAP/Cubes/NRWH/CubeDimensions");
//            var cubeMGsPackageId = GetPackageId("Sandbox/Radovan Jankovic/NRWH_OLAP/Cubes/NRWH/MeasureGroups");
//            var cubeCalculationsPackageId = GetPackageId("Sandbox/Radovan Jankovic/NRWH_OLAP/Cubes/NRWH/CalculatedMeasures");
            

//            var tableListingQuery = "SELECT o.Object_ID, o.Name, c.c ColCount FROM t_object o OUTER APPLY (SELECT COUNT(*) c FROM t_attribute a WHERE a.Stereotype = 'column' AND a.Object_ID = o.Object_ID) c WHERE Package_ID = ";
//            var l1ElementList = NetBridge.ExecuteSelectStatement(tableListingQuery + l1PackageId.ToString());
//            var l2ElementList = NetBridge.ExecuteSelectStatement(tableListingQuery + l2PackageId.ToString());
//            Dictionary<string, EATableProps> l1IdMap = new Dictionary<string, EATableProps>();
//            foreach (DataRow dr in l1ElementList.Rows)
//            {
//                var id = (int)dr["Object_ID"];
//                var colCount = (int)dr["ColCount"];
//                var name = (string)dr["Name"];
//                l1IdMap.Add(name, new EATableProps() { Id = id, Name = name, ColumnCount = colCount });
//            }
//            Dictionary<string, EATableProps> l2IdMap = new Dictionary<string, EATableProps>();
//            foreach (DataRow dr in l2ElementList.Rows)
//            {
//                var id = (int)dr["Object_ID"];
//                var colCount = (int)dr["ColCount"];
//                var name = (string)dr["Name"];
//                l2IdMap.Add(name, new EATableProps() { Id = id, Name = name, ColumnCount = colCount });
//            }

//            // connect to repo and cleanup generated parts

//            EA.Repository r = new EA.Repository();
//            r.OpenFile(@"Enterprise_Architect_NOIS --- DBType=1;Connect=Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=Enterprise_Architect_NOIS;Data Source=fsczprsa0010;LazyLoad=1;");
//            var ssisPkg = r.GetPackageByID(ssisPackageId);
//            var diagPkg = r.GetPackageByID(diagPackageId);
//            var l1Pkg = r.GetPackageByID(l1PackageId);
//            var l2Pkg = r.GetPackageByID(l2PackageId);
//            var dbDimensionsPackage = r.GetPackageByID(dbDimensionsPackageId);
//            var cubeDimensionsPackage = r.GetPackageByID(cubeDimensionsPackageId);
//            var cubeMGsPackage = r.GetPackageByID(cubeMGsPackageId);
//            var cubeCalculationsPackage = r.GetPackageByID(cubeCalculationsPackageId);

//            //return;

//            ClearPackage(ssisPkg);
//            ClearPackage(dbDimensionsPackage);
//            ClearPackage(cubeDimensionsPackage);
//            ClearPackage(cubeMGsPackage);
//            ClearPackage(cubeCalculationsPackage);
//            ClearPackage(diagPkg);

//            Console.WriteLine("Creating SSIS classes");
//            // delete links from L1 to SSIS

//            for (short i = 0; i < l1Pkg.Connectors.Count; i++)
//            {
//                EA.Connector con = l1Pkg.Connectors.GetAt(i);
//                if (con.Type == "Directed Association")
//                {
//                    l1Pkg.Connectors.Delete(i);
//                }
//            }
//            l1Pkg.Connectors.Refresh();

//            //return;

//            // create SSIS classes

//            Dictionary<string, EA.Element> ssisElemMap = new Dictionary<string, EA.Element>();
//            foreach(DataRow dr in packageList.Rows)
//            {
//                var pkgName = (string)dr["Name"];
//                EA.Element e = ssisPkg.Elements.AddNew(pkgName, "Class");
//                if (!e.Update())
//                {
//                    throw new Exception(e.GetLastError());
//                }
//                ssisElemMap.Add(pkgName, e);
//            }
//            ssisPkg.Elements.Refresh();

//            // create DF links

//            int dataFlowCounter = 0;

//            HashSet<int> includedL1Tables = new HashSet<int>();
//            HashSet<int> includedPackages = new HashSet<int>();
//            HashSet<int> includedL2Tables = new HashSet<int>();

//            foreach(DataRow dr in linksFromL1Table.Rows)
//            {
//                var sourceName = (string)dr["SourceNodeName"];
//                var targetName = (string)dr["TargetNodeName"];
//                var sourceElemStruct = l1IdMap[sourceName];
//                var targetElemId = ssisElemMap[targetName].ElementID;
//                var sourceElem = l1Pkg.Elements.GetByName(sourceName);
                
//                EA.Connector connector = sourceElem.Connectors.AddNew(string.Format("Data Flow {0}", dataFlowCounter++), "Directed Association");
//                connector.SupplierID = targetElemId;
//                if (!connector.Update())
//                {
//                    throw new Exception(connector.GetLastError());
//                }
//                sourceElem.Connectors.Refresh();
//                includedL1Tables.Add(sourceElemStruct.Id);
//                includedPackages.Add(targetElemId);
//            }

//            foreach (DataRow dr in linksToL2Table.Rows)
//            {
//                var sourceName = (string)dr["SourceNodeName"];
//                var targetName = (string)dr["TargetNodeName"];
//                var sourceElemId = ssisElemMap[sourceName].ElementID;
//                var targetElemStruct = l2IdMap[targetName];
//                var sourceElem = ssisElemMap[sourceName];

//                EA.Connector connector = sourceElem.Connectors.AddNew(string.Format("Data Flow {0}", dataFlowCounter++), "Directed Association");
//                connector.SupplierID = targetElemStruct.Id;
//                if (!connector.Update())
//                {
//                    throw new Exception(connector.GetLastError());
//                }
//                sourceElem.Connectors.Refresh();
//                includedPackages.Add(sourceElemId);
//                includedL2Tables.Add(targetElemStruct.Id);
//            }


//            Console.WriteLine("Creating OLAP dimensions");
            
//            // create OLAP DB dimensions

//            Dictionary<int, List<string>> olapDbDimAttrMap = new Dictionary<int, List<string>>();
//            foreach (DataRow dr in dimensionAttributeList.Rows)
//            {
//                var id = (int)dr["BasicGraphNodeId"];
//                var name = (string)dr["Name"];
//                var parentId = (int)dr["ParentId"];
//                if (!olapDbDimAttrMap.ContainsKey(parentId))
//                {
//                    olapDbDimAttrMap.Add(parentId, new List<string>());
//                }
//                olapDbDimAttrMap[parentId].Add(name);
//            }

//            Dictionary<string, EATableProps> olapDbDimMap = new Dictionary<string, EATableProps>();
//            Dictionary<string, EA.Element> olapDbDimElementMap = new Dictionary<string, EA.Element>();
//            foreach (DataRow dr in dimensionList.Rows)
//            {
//                var id = (int)dr["BasicGraphNodeId"];
//                var name = (string)dr["Name"];
//                olapDbDimMap.Add(name, new EATableProps() { Id = id, Name = name, ColumnCount = olapDbDimAttrMap[id].Count, Columns = olapDbDimAttrMap[id] });

//                EA.Element e = dbDimensionsPackage.Elements.AddNew(name, "Table");
//                if (!e.Update())
//                {
//                    throw new Exception(e.GetLastError());
//                }
//                foreach (var column in olapDbDimAttrMap[id])
//                {
//                    EA.Attribute a = e.Attributes.AddNew(column, "");
//                    if (!a.Update())
//                    {
//                        throw new Exception(a.GetLastError());
//                    }
//                }

//                olapDbDimElementMap.Add(name, e);
//            }
            
//            foreach (DataRow dr in linksToOlapDbDimensions.Rows)
//            {
//                var sourceName = (string)dr["SourceNodeName"];
//                var targetName = (string)dr["TargetNodeName"];
//                var sourceElemStruct = l2IdMap[sourceName];
//                var targetElemId = olapDbDimElementMap[targetName].ElementID;
//                var sourceElem = l2Pkg.Elements.GetByName(sourceName);

//                EA.Connector connector = sourceElem.Connectors.AddNew(string.Format("Data Flow {0}", dataFlowCounter++), "Directed Association");
//                connector.SupplierID = targetElemId;
//                if (!connector.Update())
//                {
//                    throw new Exception(connector.GetLastError());
//                }
//                sourceElem.Connectors.Refresh();
//                includedL2Tables.Add(sourceElemStruct.Id);
//            }

//            Console.WriteLine("Creating cube dimensions");

//            // create cube dimensions
            
//            Dictionary<string, EA.Element> cubeDimElementMap = new Dictionary<string, EA.Element>();
//            foreach (DataRow dr in cubeDimensionList.Rows)
//            {
//                var id = (int)dr["BasicGraphNodeId"];
//                var name = (string)dr["Name"];
                
//                EA.Element e = cubeDimensionsPackage.Elements.AddNew(name, "Class");
//                if (!e.Update())
//                {
//                    throw new Exception(e.GetLastError());
//                }
                
//                cubeDimElementMap.Add(name, e);
//            }

//            foreach (DataRow dr in linksToCubeDimensions.Rows)
//            {
//                var sourceName = (string)dr["SourceNodeName"];
//                var targetName = (string)dr["TargetNodeName"];
//                var sourceElemStruct = olapDbDimElementMap[sourceName].ElementID;
//                var targetElemId = cubeDimElementMap[targetName].ElementID;
//                var sourceElem = olapDbDimElementMap[sourceName];

//                EA.Connector connector = sourceElem.Connectors.AddNew(string.Format("Data Flow {0}", dataFlowCounter++), "Directed Association");
//                connector.SupplierID = targetElemId;
//                if (!connector.Update())
//                {
//                    throw new Exception(connector.GetLastError());
//                }
//                sourceElem.Connectors.Refresh();
//            }

//            Console.WriteLine("Creating measures groups");

//            // create measure groups

//            Dictionary<int, List<string>> mgMeasureMap = new Dictionary<int, List<string>>();
//            foreach (DataRow dr in mgMeasureList.Rows)
//            {
//                var id = (int)dr["BasicGraphNodeId"];
//                var name = (string)dr["Name"];
//                var parentId = (int)dr["ParentId"];
//                if (!mgMeasureMap.ContainsKey(parentId))
//                {
//                    mgMeasureMap.Add(parentId, new List<string>());
//                }
//                mgMeasureMap[parentId].Add(name);
//            }

//            Dictionary<string, EATableProps> mgMap = new Dictionary<string, EATableProps>();
//            Dictionary<string, EA.Element> mgElementMap = new Dictionary<string, EA.Element>();
//            foreach (DataRow dr in mgList.Rows)
//            {
//                var id = (int)dr["BasicGraphNodeId"];
//                var name = (string)dr["Name"];
//                mgMap.Add(name, new EATableProps() { Id = id, Name = name, ColumnCount = mgMeasureMap[id].Count, Columns = mgMeasureMap[id] });

//                EA.Element e = cubeMGsPackage.Elements.AddNew(name, "Table");
//                if (!e.Update())
//                {
//                    throw new Exception(e.GetLastError());
//                }
//                foreach (var measure in mgMeasureMap[id])
//                {
//                    EA.Attribute a = e.Attributes.AddNew(measure, "");
//                    if (!a.Update())
//                    {
//                        throw new Exception(a.GetLastError());
//                    }
//                }

//                mgElementMap.Add(name, e);
//            }

//            List<Tuple<string, string>> tableToMGLinks = new List<Tuple<string, string>>();

//            foreach (DataRow dr in linksFromL2ToMGs.Rows)
//            {
//                var sourceName = (string)dr["SourceNodeName"];
//                var targetName = (string)dr["TargetNodeName"];
//                tableToMGLinks.Add(new Tuple<string, string>(sourceName, targetName));
//            }

//            Dictionary<int, List<string>> viewIdToMGsLinksMap = new Dictionary<int, List<string>>();
//            foreach (DataRow dr in linksFromViewsToMGs.Rows)
//            {
//                var sourceId = (int)dr["SourceNodeId"];
//                var targetName = (string)dr["TargetNodeName"];
//                if (!viewIdToMGsLinksMap.ContainsKey(sourceId))
//                {
//                    viewIdToMGsLinksMap.Add(sourceId, new List<string>());
//                }
//                viewIdToMGsLinksMap[sourceId].Add(targetName);
//            }

//            foreach (DataRow dr in linksToOlapViews.Rows)
//            {
//                var sourceName = (string)dr["SourceNodeName"];
//                var targetId = (int)dr["TargetNodeId"];
//                if (viewIdToMGsLinksMap.ContainsKey(targetId))
//                {
//                    foreach (var mgName in viewIdToMGsLinksMap[targetId])
//                    {
//                        tableToMGLinks.Add(new Tuple<string, string>(sourceName, mgName));
//                    }
//                }
//            }

//            tableToMGLinks = tableToMGLinks.Distinct().ToList();

//            foreach (var tableToMGLink in tableToMGLinks)
//            {
//                var sourceElemStruct = l2IdMap[tableToMGLink.Item1];
//                var targetElemId = mgElementMap[tableToMGLink.Item2].ElementID;
//                var sourceElem = l2Pkg.Elements.GetByName(tableToMGLink.Item1);

//                EA.Connector connector = sourceElem.Connectors.AddNew(string.Format("Data Flow {0}", dataFlowCounter++), "Directed Association");
//                connector.SupplierID = targetElemId;
//                if (!connector.Update())
//                {
//                    throw new Exception(connector.GetLastError());
//                }
//                sourceElem.Connectors.Refresh();
//                includedL2Tables.Add(sourceElemStruct.Id);
//            }

//            //return;
//            Console.WriteLine("Creating diagram");
//            // create diagram

//            EA.Diagram diagram = diagPkg.Diagrams.AddNew("Dataflow Diagram", "Logical");

//            diagram.Notes = "Hello there this is a test";
//            if (!diagram.Update())
//            {
//                throw new Exception(diagram.GetLastError());
//            }
//            int offsetLeft = 100;
//            int offsetTop = 100;
//            int rectH = 100;
//            int rectW = 200;
//            int attrH = 20;
//            int rectMarginT = 50;
//            int rectMarginL = 200;

//            // L1
//            foreach (var l1Table in l1IdMap.Keys)
//            {
//                var elemStruct = l1IdMap[l1Table];
//                if (!includedL1Tables.Contains(elemStruct.Id))
//                {
//                    continue;
//                }

//                var tableAttrHeight = attrH * elemStruct.ColumnCount;
//                var positioningTo = string.Format("l={0};r={1};t={2};b={3};", offsetLeft, offsetLeft + rectW, offsetTop, offsetTop + rectH + tableAttrHeight);
//                //offsetLeft += rectW + rectMarginL;
//                offsetTop += rectH + tableAttrHeight + rectMarginT;
//                EA.DiagramObject diagObj = diagram.DiagramObjects.AddNew(positioningTo, "");
//                diagObj.ElementID = elemStruct.Id;
//                if (!diagObj.Update())
//                {
//                    throw new Exception(diagObj.GetLastError());
//                }
//            }

//            //return;

//            // SSIS
//            offsetTop = 100;
//            offsetLeft = offsetLeft + rectW + rectMarginL;
//            foreach (var ssisPkgName in ssisElemMap.Keys)
//            {
//                var elemId = ssisElemMap[ssisPkgName].ElementID;
//                if (!includedPackages.Contains(elemId))
//                {
//                    continue;
//                }

//                var positioningTo = string.Format("l={0};r={1};t={2};b={3};", offsetLeft, offsetLeft + rectW, offsetTop, offsetTop + rectH);
//                //offsetLeft += rectW + rectMarginL;
//                offsetTop += rectH + rectMarginT;
//                EA.DiagramObject diagObj = diagram.DiagramObjects.AddNew(positioningTo, "");
//                diagObj.ElementID = elemId;
//                if (!diagObj.Update())
//                {
//                    throw new Exception(diagObj.GetLastError());
//                }
//            }

//            //return;

//            // L2
//            offsetTop = 100;
//            offsetLeft = offsetLeft + rectW + rectMarginL;
//            foreach (var l2Table in l2IdMap.Keys)
//            {
//                var elemStruct = l2IdMap[l2Table];
//                if (!includedL2Tables.Contains(elemStruct.Id))
//                {
//                    continue;
//                }

//                var tableAttrHeight = attrH * elemStruct.ColumnCount;
//                var positioningTo = string.Format("l={0};r={1};t={2};b={3};", offsetLeft, offsetLeft + rectW, offsetTop, offsetTop + rectH + tableAttrHeight);
//                //offsetLeft += rectW + rectMarginL;
//                offsetTop += rectH + tableAttrHeight + rectMarginT;
//                EA.DiagramObject diagObj = diagram.DiagramObjects.AddNew(positioningTo, "");
//                diagObj.ElementID = elemStruct.Id;
//                if (!diagObj.Update())
//                {
//                    throw new Exception(diagObj.GetLastError());
//                }
//            }

//            // OLAP DB dimensions
//            offsetTop = 100;
//            offsetLeft = offsetLeft + rectW + rectMarginL;
//            foreach (var olapDbDimName in olapDbDimElementMap.Keys)
//            {
//                var elemId = olapDbDimElementMap[olapDbDimName].ElementID;

//                var tableAttrHeight = attrH * olapDbDimMap[olapDbDimName].ColumnCount;
//                var positioningTo = string.Format("l={0};r={1};t={2};b={3};", offsetLeft, offsetLeft + rectW, offsetTop, offsetTop + rectH + tableAttrHeight);
//                //offsetLeft += rectW + rectMarginL;
//                offsetTop += rectH + tableAttrHeight + rectMarginT;
//                EA.DiagramObject diagObj = diagram.DiagramObjects.AddNew(positioningTo, "");
//                diagObj.ElementID = elemId;
//                if (!diagObj.Update())
//                {
//                    throw new Exception(diagObj.GetLastError());
//                }
//            }

//            // cube dimensions
//            offsetTop = 100;
//            offsetLeft = offsetLeft + rectW + rectMarginL;
//            foreach (var cubeDimElemName in cubeDimElementMap.Keys)
//            {
//                var elemId = cubeDimElementMap[cubeDimElemName].ElementID;

//                var positioningTo = string.Format("l={0};r={1};t={2};b={3};", offsetLeft, offsetLeft + rectW, offsetTop, offsetTop + rectH);
//                //offsetLeft += rectW + rectMarginL;
//                offsetTop += rectH + rectMarginT;
//                EA.DiagramObject diagObj = diagram.DiagramObjects.AddNew(positioningTo, "");
//                diagObj.ElementID = elemId;
//                if (!diagObj.Update())
//                {
//                    throw new Exception(diagObj.GetLastError());
//                }
//            }

//            // MGs
//            offsetLeft = offsetLeft + rectW + rectMarginL;
//            foreach (var mgName in mgElementMap.Keys)
//            {
//                var elemId = mgElementMap[mgName].ElementID;
//                var elemStruct = mgMap[mgName];
//                var tableAttrHeight = attrH * elemStruct.ColumnCount / 2;

//                var positioningTo = string.Format("l={0};r={1};t={2};b={3};", offsetLeft, offsetLeft + rectW, offsetTop, offsetTop + rectH + tableAttrHeight);
//                //offsetLeft += rectW + rectMarginL;
//                offsetLeft = offsetLeft + rectW + rectMarginL;
//                //offsetTop += rectH + rectMarginT;
//                EA.DiagramObject diagObj = diagram.DiagramObjects.AddNew(positioningTo, "");
//                diagObj.ElementID = elemId;
//                if (!diagObj.Update())
//                {
//                    throw new Exception(diagObj.GetLastError());
//                }
//            }
//        }

//        private static void ClearPackage(EA.Package pkg)
//        {
//            for (short i = 0; i < pkg.Diagrams.Count; i++)
//            {
//                pkg.Diagrams.Delete(i);
//            }
//            pkg.Diagrams.Refresh();
//            for (short i = 0; i < pkg.Connectors.Count; i++)
//            {
//                pkg.Connectors.Delete(i);
//            }
//            pkg.Connectors.Refresh();
//            for (short i = 0; i < pkg.Elements.Count; i++)
//            {
//                pkg.Elements.Delete(i);
//            }
//            pkg.Elements.Refresh();
//        }

//        private static Dictionary<string, int> _eaPackageIdCache = new Dictionary<string, int>();
//        private static int GetPackageId(string path)
//        {
//            path = path.TrimEnd('/');
//            // Sandbox/Radovan Jankovic/NDWH_L1

//            // search the cache for a prefix of the path
//            string cachedPrefix = string.Empty;
//            int knownPrefixEnd = 0;
//            int cachedPrefixId = -1;
//            while (knownPrefixEnd < path.Length)
//            {
//                var slashPos = path.IndexOf('/', knownPrefixEnd + 1);
//                string checkPrefix = path;
//                if (slashPos > -1)
//                {
//                    checkPrefix = path.Substring(0, slashPos);
//                }
//                if (_eaPackageIdCache.ContainsKey(checkPrefix))
//                {
//                    cachedPrefix = checkPrefix;
//                    cachedPrefixId = _eaPackageIdCache[checkPrefix];
//                    knownPrefixEnd = cachedPrefix.Length;
//                }
//                else
//                {
//                    break;
//                }
//            }

//            var knownPrefix = cachedPrefix;
//            var lastKnownId = cachedPrefixId;
//            var remainingPath = path.Remove(0, cachedPrefix.Length).TrimStart('/');
//            while (knownPrefix.Length < path.Length)
//            {
//                var segment = remainingPath;
//                var slashPos = remainingPath.IndexOf('/');
//                if (slashPos > -1)
//                {
//                    segment = remainingPath.Substring(0, slashPos);
//                }

//                var segmentIdTTbl = NetBridge.ExecuteSelectStatement(
//                    "SELECT PDATA1 FROM t_object WHERE Object_Type = 'Package' AND Name = @name AND Package_ID = ISNULL(@package_ID, Package_ID)",
//                    new Dictionary<string, object>()
//                    {
//                        { "@name", segment},
//                        { "@package_ID", lastKnownId == -1 ? (object)DBNull.Value : lastKnownId}
//                    });

//                lastKnownId = int.Parse((string)segmentIdTTbl.Rows[0][0]);
//                if (knownPrefix.Length > 0)
//                {
//                    knownPrefix += '/';
//                }
//                knownPrefix += segment;
//                remainingPath = path.Remove(0, knownPrefix.Length).TrimStart('/');
//            }

//            return lastKnownId;
//        }
//    }
//}
