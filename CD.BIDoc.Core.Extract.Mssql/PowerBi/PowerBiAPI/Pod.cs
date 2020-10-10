using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Extract.PowerBi.PowerBiAPI
{
    class Pod
    {
        public Pod(string id, string name, string objectId, string boundSection)
        {
            Id = id;
            Name = name;
            ObjectId = objectId;
            BoundSection = boundSection;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string ObjectId { get; set; }
        public string BoundSection { get; set; }
    }
}
