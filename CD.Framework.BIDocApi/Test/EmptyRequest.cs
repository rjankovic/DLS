using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.Test
{
    public class EmptyResponse : DLSApiMessage
    {
        public string Message { get; set; }
    }

    public class EmptyRequest : DLSApiRequest<EmptyResponse>
    {
    }
}
