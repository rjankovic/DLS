using CD.DLS.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Common.Structures
{
    public enum SignalTypeEnum { RequestMessage }

    public class SignalHeader
    {
        public SignalTypeEnum SignalType { get; set; }
        public Guid RequestId { get; set; }
        public Guid MessageId { get; set; }
        
        public SignalHeader()
        {
            RequestId = Guid.NewGuid();
            MessageId = Guid.NewGuid();
            SignalType = SignalTypeEnum.RequestMessage;
        }        
    }
}
