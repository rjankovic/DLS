using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Extract.PowerBi;
using CD.DLS.Model;
using CD.DLS.Model.Mssql.Pbi;
using CD.DLS.Model.Mssql.Tabular;
using CD.DLS.Model.Serialization;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.Parse.Mssql.Ssrs.Rdl2008;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CD.DLS.TestConsole
{
    //public class AsConnection
    //{ 
    //    public string Path { get; set; }
    //    public string Name { get; set; }
    //    public string ConnectionType { get; set; }
    //    public string ConnectionServer { get; set; }
    //    public string ConnectionDb { get; set; }
    //}


    class Program
    {
        static void Main(string[] args)
        {
            //CustomPbiScanParser parser = new CustomPbiScanParser();
            //parser.Run();

            CustomTabularBimParser parser = new CustomTabularBimParser();
            parser.Run();



        }


    }
}
