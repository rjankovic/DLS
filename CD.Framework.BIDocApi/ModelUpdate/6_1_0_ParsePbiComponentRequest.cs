using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.ModelUpdate
{
    /// <summary>
    /// WF pos: 6-1
    /// </summary>
    public class ParsePbiComponentRequest : DLSApiRequest<DLSApiProgressResponse>
    {
        public Guid ExtractId { get; set; }
        public int PbiComponentId { get; set; }
        public string TenantRefPath { get; set; }
    }
}
