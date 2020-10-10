using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects
{
    public class AbandonedRequest
    {
        public Guid RequestId { get; set; }
        public Guid MessageId { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
