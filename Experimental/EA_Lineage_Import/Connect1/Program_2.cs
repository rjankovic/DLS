//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Connect1
//{
//    class Program
//    {
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
//                { "sourcePrefix", "Server[@Name='FSCZPRCT0041']/Database[@Name='NDWH_L1']" },
//                { "targetPrefix", "IntegrationServices[@Name='FSCZPRCT0041']/Catalog[@Name='SSISDB']/CatalogFolder[@Name='NRWH_SSIS']/ProjectInfo[@Name='NRWH_SSIS']" },
//                { "sourceType", "SchemaTableElement" },
//                { "targetType", "PackageElement"}
//            });


//            var linksToL2Table = NetBridge.ExecuteProcedureTable("Inspect.sp_GetDataFlowBetweenGroups", new Dictionary<string, object>()
//            {
//                { "projectConfigId", fmwProjectId },
//                { "sourcePrefix", "IntegrationServices[@Name='FSCZPRCT0041']/Catalog[@Name='SSISDB']/CatalogFolder[@Name='NRWH_SSIS']/ProjectInfo[@Name='NRWH_SSIS']" },
//                { "targetPrefix", "Server[@Name='FSCZPRCT0041']/Database[@Name='NRWH_L2']" },
//                { "sourceType", "PackageElement" },
//                { "targetType", "SchemaTableElement"}
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
//                { "graphkind", "DataFlowTransitive" },
//                { "path", "IntegrationServices[@Name='FSCZPRCT0041']/Catalog[@Name='SSISDB']/CatalogFolder[@Name='NRWH_SSIS']/ProjectInfo[@Name='NRWH_SSIS']" },
//                { "nodeType", "PackageElement" }
//            });

//            // get EA data from DB
//            NetBridge.UseTemporaryConnstring(connStringEA);

//            var l1PackageTbl = NetBridge.ExecuteSelectStatement("SELECT PDATA1 FROM t_object WHERE Object_Type = 'Package' AND Author = 'DKX4KL4' AND Name = 'NDWH_L1'");
//            var l2PackageTbl = NetBridge.ExecuteSelectStatement("SELECT PDATA1 FROM t_object WHERE Object_Type = 'Package' AND Author = 'DKX4KL4' AND Name = 'NRWH_L2'");
//            var ssisPackageTbl = NetBridge.ExecuteSelectStatement("SELECT PDATA1 FROM t_object WHERE Object_Type = 'Package' AND Author = 'DKX4KL4' AND Name = 'NRWH_SSIS'");
//            var diagPackageTbl = NetBridge.ExecuteSelectStatement("SELECT PDATA1 FROM t_object WHERE Object_Type = 'Package' AND Author = 'DKX4KL4' AND Name = 'NRWH_Diagrams'");

//            var l1PackageId = int.Parse((string)l1PackageTbl.Rows[0][0]);
//            var l2PackageId = int.Parse((string)l2PackageTbl.Rows[0][0]);
//            var ssisPackageId = int.Parse((string)ssisPackageTbl.Rows[0][0]);
//            var diagPackageId = int.Parse((string)diagPackageTbl.Rows[0][0]);

//            var l1ElementList = NetBridge.ExecuteSelectStatement("SELECT Object_ID, Name FROM t_object WHERE Package_ID = " + l1PackageId.ToString());
//            var l2ElementList = NetBridge.ExecuteSelectStatement("SELECT Object_ID, Name FROM t_object WHERE Package_ID = " + l2PackageId.ToString());
//            Dictionary<string, int> l1IdMap = new Dictionary<string, int>();
//            foreach (DataRow dr in l1ElementList.Rows)
//            {
//                l1IdMap.Add((string)dr["Name"], (int)dr["Object_ID"]);
//            }
//            Dictionary<string, int> l2IdMap = new Dictionary<string, int>();
//            foreach (DataRow dr in l2ElementList.Rows)
//            {
//                l2IdMap.Add((string)dr["Name"], (int)dr["Object_ID"]);
//            }

//            // connect to repo and cleanup generated parts

//            EA.Repository r = new EA.Repository();
//            r.OpenFile(@"Enterprise_Architect_NOIS --- DBType=1;Connect=Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=Enterprise_Architect_NOIS;Data Source=fsczprsa0010;LazyLoad=1;");
//            var ssisPkg = r.GetPackageByID(ssisPackageId);
//            var diagPkg = r.GetPackageByID(diagPackageId);
//            var l1Pkg = r.GetPackageByID(l1PackageId);

//            ClearPackage(ssisPkg);
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

//            // create SSIS classes

//            Dictionary<string, EA.Element> ssisElemMap = new Dictionary<string, EA.Element>();
//            foreach(DataRow dr in packageList.Rows)
//            {
//                var pkgName = (string)dr["Name"];
//                EA.Element e = ssisPkg.Elements.AddNew(pkgName, "Class");
//                e.Update();
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
//                var sourceElemId = l1IdMap[sourceName];
//                var targetElemId = ssisElemMap[targetName].ElementID;
//                var sourceElem = l1Pkg.Elements.GetByName(sourceName);
                
//                EA.Connector connector = sourceElem.Connectors.AddNew(string.Format("Data Flow {0}", dataFlowCounter++), "Directed Association");
//                connector.SupplierID = targetElemId;
//                if (!connector.Update())
//                {
//                    throw new Exception(connector.GetLastError());
//                }
//                sourceElem.Connectors.Refresh();
//                includedL1Tables.Add(sourceElemId);
//                includedPackages.Add(targetElemId);
//            }

//            foreach (DataRow dr in linksToL2Table.Rows)
//            {
//                var sourceName = (string)dr["SourceNodeName"];
//                var targetName = (string)dr["TargetNodeName"];
//                var sourceElemId = ssisElemMap[sourceName].ElementID;
//                var targetElemId = l2IdMap[targetName];
//                var sourceElem = ssisElemMap[sourceName];

//                EA.Connector connector = sourceElem.Connectors.AddNew(string.Format("Data Flow {0}", dataFlowCounter++), "Directed Association");
//                connector.SupplierID = targetElemId;
//                if (!connector.Update())
//                {
//                    throw new Exception(connector.GetLastError());
//                }
//                sourceElem.Connectors.Refresh();
//                includedPackages.Add(sourceElemId);
//                includedL2Tables.Add(targetElemId);
//            }

//            Console.WriteLine("Creating diagram");
//            // create diagram

//            EA.Diagram diagram = diagPkg.Diagrams.AddNew("Dataflow Diagram", "Logical");

//            diagram.Notes = "Hello there this is a test";
//            diagram.Update();
//            int offsetLeft = 100;
//            int offsetTop = 100;
//            int rectH = 100;
//            int rectW = 200;
//            int rectMarginT = 50;
//            int rectMarginL = 600;

//            // L1
//            foreach (var l1Table in l1IdMap.Keys)
//            {
//                var elemId = l1IdMap[l1Table];
//                if (!includedL1Tables.Contains(elemId))
//                {
//                    continue;
//                }
                
//                var positioningTo = string.Format("l={0};r={1};t={2};b={3};", offsetLeft, offsetLeft + rectW, offsetTop, offsetTop + rectH);
//                //offsetLeft += rectW + rectMarginL;
//                offsetTop += rectH + rectMarginT;
//                EA.DiagramObject diagObj = diagram.DiagramObjects.AddNew(positioningTo, "");
//                diagObj.ElementID = elemId;
//                diagObj.Update();
//            }

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
//                diagObj.Update();
//            }

//            // L2
//            offsetTop = 100;
//            offsetLeft = offsetLeft + rectW + rectMarginL;
//            foreach (var l2Table in l2IdMap.Keys)
//            {
//                var elemId = l2IdMap[l2Table];
//                if (!includedL2Tables.Contains(elemId))
//                {
//                    continue;
//                }

//                var positioningTo = string.Format("l={0};r={1};t={2};b={3};", offsetLeft, offsetLeft + rectW, offsetTop, offsetTop + rectH);
//                //offsetLeft += rectW + rectMarginL;
//                offsetTop += rectH + rectMarginT;
//                EA.DiagramObject diagObj = diagram.DiagramObjects.AddNew(positioningTo, "");
//                diagObj.ElementID = elemId;
//                diagObj.Update();
//            }

            
//            //var packageListTable = NetBridge.ExecuteProcedure()


//            //int packageID;
//            //using (SqlConnection conn = new SqlConnection(connStringEA))
//            //{
//            //    conn.Open();
//            //    SqlCommand cmd = new SqlCommand("SELECT PDATA1 FROM t_object WHERE Object_Type = 'Package' AND Author = 'DKX4KL4' AND Name = 'Gen1'", conn);
//            //    packageID = int.Parse((string)cmd.ExecuteScalar());
//            //}

//            ////SELECT Package_ID FROM t_object WHERE Object_Type = 'Package' AND Author = 'DKX4KL4' AND Name = 'Gen1'


//            //EA.Repository r = new EA.Repository();
//            //r.OpenFile(@"Enterprise_Architect_NOIS --- DBType=1;Connect=Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=Enterprise_Architect_NOIS;Data Source=fsczprsa0010;LazyLoad=1;");
//            //var pkg = r.GetPackageByID(packageID);

//            //for (short i = 0; i < pkg.Diagrams.Count; i++)
//            //{
//            //    pkg.Diagrams.Delete(i);
//            //}
//            //pkg.Diagrams.Refresh();
//            //for (short i = 0; i < pkg.Connectors.Count; i++)
//            //{
//            //    pkg.Connectors.Delete(i);
//            //}
//            //pkg.Connectors.Refresh();
//            //for (short i = 0; i < pkg.Elements.Count; i++)
//            //{
//            //    pkg.Elements.Delete(i);
//            //}
//            //pkg.Elements.Refresh();

//            //List<EA.Element> elems = new List<EA.Element>();

//            //for (int i = 0; i < 3; i++)
//            //{
//            //    EA.Element e = pkg.Elements.AddNew(string.Format("SSIS_pkg{0}", i), "Class");
//            //    e.Update();
//            //    elems.Add(e);
//            //}
//            //pkg.Elements.Refresh();


//            //EA.Diagram diagram = pkg.Diagrams.AddNew("Dataflow Diagram", "Logical");

//            //diagram.Notes = "Hello there this is a test";
//            //diagram.Update();
//            //int offsetLeft = 200;

//            //for (int i = 0; i < elems.Count - 1; i++)
//            //{
//            //    var fromElemId = elems[i].ElementID;
//            //    var toElemId = elems[i + 1].ElementID;
//            //    EA.Connector connector = elems[i].Connectors.AddNew(string.Format("Link {0}", i), "Directed Association");
//            //    connector.SupplierID = toElemId;
//            //    if (!connector.Update())
//            //    {
//            //        throw new Exception(connector.GetLastError());
//            //    }
//            //    elems[i].Connectors.Refresh();
//            //    //elems[i].Update();
//            //    //elems[i + 1].Update();
//            //}


//            //for (int i = 0; i < elems.Count - 1; i++)
//            //{
//            //    var fromElemId = elems[i].ElementID;
//            //    var toElemId = elems[i + 1].ElementID;

//            //    //string.Format("l={0};r={1};t=200;b=600;", offsetLeft, offsetLeft + 200);

//            //    // add the source only the first time, then chain
//            //    if (i == 0)
//            //    {
//            //        var positioningFrom = string.Format("l={0};r={1};t=200;b=300;", offsetLeft, offsetLeft + 200);
//            //        offsetLeft += 400;
//            //        EA.DiagramObject diagFrom = diagram.DiagramObjects.AddNew(positioningFrom, "");
//            //        diagFrom.ElementID = fromElemId;
//            //        diagFrom.Update();
//            //    }

//            //    var positioningTo = string.Format("l={0};r={1};t=200;b=300;", offsetLeft, offsetLeft + 200);
//            //    offsetLeft += 400;
//            //    EA.DiagramObject diagTo = diagram.DiagramObjects.AddNew(positioningTo, "");
//            //    diagTo.ElementID = toElemId;
//            //    diagTo.Update();
//            //}


//            //o = package.Elements.AddNew("ReferenceType", "Class")
//            //o.Update
//            //'' add element to diagram -supply optional rectangle co - ordinates
//            //v = diagram.DiagramObjects.AddNew("l=200;r=400;t=200;b=600;", "")
//            //v.ElementID = o.ElementID
//            //v.Update


//            //foreach (EA.Package spkg in pkg.Packages)
//            //{
//            //    var n = spkg.Name;
//            //    var ns = spkg.PackageID;
//            //}
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
//    }
//}
