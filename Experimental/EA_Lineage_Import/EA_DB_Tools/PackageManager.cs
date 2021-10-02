using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Tools
{

    public class PackageManager
    {
        private Repository _repository;
        private NetBridge _db;
        
        public PackageManager(Repository repository)
        {
            _repository = repository;
            _db = new NetBridge(_repository.ConnectionString);
        }

        public void ClearPackage(EA.Package pkg)
        {
            for (short i = 0; i < pkg.Diagrams.Count; i++)
            {
                pkg.Diagrams.Delete(i);
            }
            pkg.Diagrams.Refresh();
            for (short i = 0; i < pkg.Connectors.Count; i++)
            {
                pkg.Connectors.Delete(i);
            }
            pkg.Connectors.Refresh();
            for (short i = 0; i < pkg.Elements.Count; i++)
            {
                pkg.Elements.Delete(i);
            }
            pkg.Elements.Refresh();
        }
    }
}
