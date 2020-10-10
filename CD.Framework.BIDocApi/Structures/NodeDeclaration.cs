using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API
{
    public class NodeDeclaration
    {
        public string RefPath { get; set; }
        public int ModelElementId { get; set; }
        public string NodeType { get; set; }
        public string Name { get; set; }
        public int NodeId { get; set; }
    }
}
