using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.ModelUpdate
{
    /// <summary>
    /// WF pos: 4-1-1
    /// </summary>
    public class ParseSsisPackageShallowRequest : DLSApiRequest<DLSApiMessage>
    {
        public Guid ExtractId { get; set; }
        public int PackageExractItemId { get; set; }
        public string ProjectRefPath { get; set; }
    }
}
