using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API
{
    public interface IRequestProcessor<T, R> where T : DLSApiRequest<R> where R : DLSApiMessage
    {
        R Process(T request, ProjectConfig projectConfig);
    }
}
