using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Extract.PowerBi;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.TestConsole
{
    public class AsConnection
    { 
        public string Path { get; set; }
        public string Name { get; set; }
        public string ConnectionType { get; set; }
        public string ConnectionServer { get; set; }
        public string ConnectionDb { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<AsConnection> asConnections = new List<AsConnection>();
            
            var root = @"C:\Projects\Business Intelligence\Power BI\";
            //var pbix = @"C:\Projects\Business Intelligence\Power BI\Customer Reporting\Customer NetworkTraffic Usage Report.pbix";
            var pbixs = System.IO.Directory.GetFiles(root, "*.pbix", System.IO.SearchOption.AllDirectories); 
            var extract = new PowerBiExtractor(null, null, "C:\\TEMP", null, null, null);
            List<Report> reports = new List<Report>();
            foreach (var pbix in pbixs)
            {
                var report = extract.ExtractPbix(pbix);
                reports.Add(report);
                foreach (var asc in report.Connections.Where(x => x.Type.Contains("analysisServices")))
                {
                    var src = asc.Source;
                    var srcSplit = src.Split('\\');
                    asConnections.Add(new AsConnection()
                    {
                        Path = pbix,
                        Name = report.Name,
                        ConnectionType = asc.Type,
                        ConnectionServer = srcSplit[0],
                        ConnectionDb = srcSplit[1]
                    });
                }
            }

            DataTable tbl = new DataTable();
            tbl.Columns.Add("Path");
            tbl.Columns.Add("Name");
            tbl.Columns.Add("ConnectionType");
            tbl.Columns.Add("ConnectionServer");
            tbl.Columns.Add("ConnectionDb");

            foreach (var asc in asConnections)
            {
                var nr = tbl.NewRow();
                nr[0] = asc.Path;
                nr[1] = asc.Name;
                nr[2] = asc.ConnectionType;
                nr[3] = asc.ConnectionServer;
                nr[4] = asc.ConnectionDb;
                tbl.Rows.Add(nr);
            }


            //var report = extract.ExtractPbix(pbix);
        }
    }
}
