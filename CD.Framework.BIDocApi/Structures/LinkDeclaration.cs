using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API
{
    public class LinkDeclaration
    {
        public int NodeFromId { get; set; }
        public int NodeToId { get; set; }
        public LinkTypeEnum LinkType { get; set; }
    }
}
