using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.ModelUpdate
{
    /// <summary>
    /// WF pos: 1
    /// </summary>
    public class UpdateModelRequest : DLSApiRequest<DLSApiMessage>
    {
        public Guid ExtractId { get; set; }
        
    }
}
