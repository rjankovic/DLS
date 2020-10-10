using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Common.Interfaces
{
    public interface ICore : IDisposable
    {
        Task<RequestMessage> ProcessMessage(RequestMessage input);
        Guid Id { get; }
        string Name { get; }
        CoreTypeEnum CoreType { get; }
        ILogger Log { get; }
        void Init(Guid id, string name, ILogger log, ProjectConfig projctConfig);
        void UpdateConfig(ProjectConfig projectConfig);
        Boolean IsBusy { get; }
    }

    public enum CoreTypeEnum { BIDoc, ManagementApi }
}
