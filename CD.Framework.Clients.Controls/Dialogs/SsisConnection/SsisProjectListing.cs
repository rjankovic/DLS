using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using  CLI = System.Data.SqlClient;

namespace CD.DLS.Clients.Controls.Dialogs.SsisConnection
{
    public static class SsisProjectListing
    {
        public static List<SsisProject> ListPrjects(string serverName, out string error)
        {
            error = null;
            SqlConnection.SqlConnectionString str = new SqlConnection.SqlConnectionString() { Database = "SSISDB", Server = serverName, IntegratedSecurity = true };
            try
            {
                using (CLI.SqlConnection conn = new CLI.SqlConnection(str.ToString()))
                {
                    conn.Open();
                    var cmd = new CLI.SqlCommand(
                        @"SELECT f.name FolderName, p.name ProjectName 
FROM SSISDB.internal.folders f
INNER JOIN SSISDB.internal.projects p ON p.folder_id = f.folder_id
",
    conn);
                    var res = new List<SsisProject>();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.HasRows)
                        {
                            while (r.Read())
                            {
                                var folderName = r.GetString(0);
                                var projectName = r.GetString(1);
                                res.Add(new SsisProject()
                                {
                                    Server = serverName,
                                    Folder = folderName,
                                    Project = projectName
                                });
                            }
                        }
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

    public class SsisProject
    {
        public string Server { get; set; }
        public string Folder { get; set; }
        public string Project { get; set; }
        public string FullPath { get { return Folder + "/" + Project; } }
    }
}
