using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Extract.PowerBi.PowerBiAPI
{
    class LiveConnectionDataSource
    {
        public int Version { get; set; }
        public LiveConnectionConnection[] Connections { get; set; }
    }

    class LiveConnectionConnection
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string ConnectionType { get; set; }
    }

    public class Diagram
    {
        public IList<string> tables { get; set; }
  
    }

    public class Example
    {
        public IList<Diagram> diagrams { get; set; }
    }



}
