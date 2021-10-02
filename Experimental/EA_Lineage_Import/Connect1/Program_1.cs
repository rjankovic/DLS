//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Connect1
//{
//    class Program_1
//    {
//        static void Main_1(string[] args)
//        {


//            string connString = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=Enterprise_Architect_NOIS;Data Source=fsczprsa0010;";
//            int packageID;
//            using (SqlConnection conn = new SqlConnection(connString))
//            {
//                conn.Open();
//                SqlCommand cmd = new SqlCommand("SELECT PDATA1 FROM t_object WHERE Object_Type = 'Package' AND Author = 'DKX4KL4' AND Name = 'Gen1'", conn);
//                packageID = int.Parse((string)cmd.ExecuteScalar());
//            }

//                //SELECT Package_ID FROM t_object WHERE Object_Type = 'Package' AND Author = 'DKX4KL4' AND Name = 'Gen1'


//                EA.Repository r = new EA.Repository();
//            r.OpenFile(@"Enterprise_Architect_NOIS --- DBType=1;Connect=Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=Enterprise_Architect_NOIS;Data Source=fsczprsa0010;LazyLoad=1;");
//            var pkg = r.GetPackageByID(packageID);

//            for (short i = 0; i < pkg.Diagrams.Count; i++)
//            {
//                pkg.Diagrams.Delete(i);
//            }
//            pkg.Diagrams.Refresh();
//            for (short i = 0; i < pkg.Elements.Count; i++)
//            {
//                pkg.Elements.Delete(i);
//            }
//            pkg.Elements.Refresh();

//            List<EA.Element> elems = new List<EA.Element>();
            
//            for (int i = 0; i < 3; i++)
//            {
//                EA.Element e = pkg.Elements.AddNew(string.Format("SSIS_pkg{0}", i), "Class");
//                e.Update();
//                elems.Add(e);
//            }
//            pkg.Elements.Refresh();

            
//            EA.Diagram diagram = pkg.Diagrams.AddNew("Dataflow Diagram", "Logical");

//            diagram.Notes = "Hello there this is a test";
//            diagram.Update();
//            int offsetLeft = 200;

//            for (int i = 0; i < elems.Count - 1; i++)
//            {
//                var fromElemId = elems[i].ElementID;
//                var toElemId = elems[i + 1].ElementID;
//                EA.Connector connector = elems[i].Connectors.AddNew(string.Format("Link {0}", i), "Directed Association");
//                connector.SupplierID = toElemId;
//                if (!connector.Update())
//                {
//                    throw new Exception(connector.GetLastError());
//                }
//                elems[i].Connectors.Refresh();
//                //elems[i].Update();
//                //elems[i + 1].Update();
//            }


//            for (int i = 0; i < elems.Count - 1; i++)
//            {
//                var fromElemId = elems[i].ElementID;
//                var toElemId = elems[i + 1].ElementID;

//                //string.Format("l={0};r={1};t=200;b=600;", offsetLeft, offsetLeft + 200);

//                // add the source only the first time, then chain
//                if (i == 0)
//                {
//                    var positioningFrom = string.Format("l={0};r={1};t=200;b=600;", offsetLeft, offsetLeft + 200);
//                    offsetLeft += 400;
//                    EA.DiagramObject diagFrom = diagram.DiagramObjects.AddNew(positioningFrom, "");
//                    diagFrom.ElementID = fromElemId;
//                    diagFrom.Update();
//                }

//                var positioningTo = string.Format("l={0};r={1};t=200;b=600;", offsetLeft, offsetLeft + 200);
//                offsetLeft += 400;
//                EA.DiagramObject diagTo = diagram.DiagramObjects.AddNew(positioningTo, "");
//                diagTo.ElementID = toElemId;
//                diagTo.Update();
//            }


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
//    }
//}
