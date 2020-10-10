using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.ModelUpdate
{
    /// <summary>
    /// WF pos: 3-2
    /// </summary>
    public class ParseSsasDatabaseCubesRequest : DLSApiRequest<DLSApiProgressResponse>
    {
        public Guid ExtractId { get; set; }
        public int SsasDbComponentId { get; set; }
    }
}
