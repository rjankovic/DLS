using System.Collections.Generic;

namespace CD.DLS.Clients.Controls.Dialogs.SqlConnection
{
    public interface ISmoTasks
    {
        IEnumerable<string> SqlServers {get;}
        List<string> GetDatabases(SqlConnectionString connectionString);
        List<DatabaseTable> GetTables(SqlConnectionString connectionString);
    }
}
