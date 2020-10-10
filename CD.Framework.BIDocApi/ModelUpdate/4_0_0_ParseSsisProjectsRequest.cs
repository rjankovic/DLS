using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.ModelUpdate
{
    /// <summary>
    /// WF pos: 4
    /// </summary>
    public class ParseSsisProjectsRequest : DLSApiRequest<DLSApiProgressResponse>
    {
        public Guid ExtractId { get; set; }
    }
}
