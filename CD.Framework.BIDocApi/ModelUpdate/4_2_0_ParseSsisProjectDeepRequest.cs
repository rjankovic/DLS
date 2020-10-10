using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.ModelUpdate
{
    /// <summary>
    /// WF pos: 4-2
    /// </summary>
    public class ParseSsisProjectDeepRequest : DLSApiRequest<DLSApiProgressResponse>
    {
        public Guid ExtractId { get; set; }
        public int SsisComponentId { get; set; }
        public string ProjectRefPath { get; set; }
        public string ServerRefPath { get; set; }
        public string FolderRefPath { get; set; }
    }
}
