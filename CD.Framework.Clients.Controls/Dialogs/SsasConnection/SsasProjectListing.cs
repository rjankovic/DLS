using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using  CLI = System.Data.SqlClient;

namespace CD.DLS.Clients.Controls.Dialogs.SsasConnection
{
    public static class SsasProjectListing
    {
        public static List<SsasDatabase> ListPrjects(string serverName, out string error)
        {   
            /*
            string commandText = @"SELECT FLATTENED 
         PredictAssociation()
         From
         [Mining Structure Name]
         NATURAL PREDICTION JOIN
         (SELECT (SELECT 1 AS [UserId]) AS [Vm]) AS t ";
            AdomdCommand cmd = new AdomdCommand(commandText, conn);
            AdomdDataReader dr = cmd.ExecuteReader();
            */

            error = null;
            SqlConnection.SqlConnectionString str = new SqlConnection.SqlConnectionString() { Database = "SSISDB", Server = serverName, IntegratedSecurity = true };
            try
            {
                using (AdomdConnection conn = new AdomdConnection(
            string.Format("Data Source={0};", serverName)))
                {
                    conn.Open();
                    var cmd = new AdomdCommand("select [CATALOG_NAME] from $system.DBSCHEMA_CATALOGS", conn);
                    var res = new List<SsasDatabase>();
                    using (var r = cmd.ExecuteReader())
                    {
                        //if (r.RecordsAffected > 0)
                        //{
                            while (r.Read())
                            {
                                var dbName = r.GetString(0);
                                res.Add(new SsasDatabase()
                                {
                                    Server = serverName,
                                    Database = dbName
                                });
                            }
                        //}
                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }
    }

    public class SsasDatabase
    {
        public string Server { get; set; }
        public string Database { get; set; }
    }
}
