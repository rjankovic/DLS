using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API
{
    public abstract class DLSApiRequest<R> : DLSApiMessage where R : DLSApiMessage //, ISecuredObject
    {

    }
}
