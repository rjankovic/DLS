using CD.DLS.Common.Interfaces;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Interfaces
{
    /// <summary>
    /// Common interface for model extract settings.
    /// </summary>
    public class ModelSettings
    {
        public ILogger Log { get; set; }
        public ProjectConfig Config { get; set; }
    }

    /// <remarks>
    /// This interface allows extending the functionality of the extractor to other services, such as 
    /// SSAS, whithout requiring the exisiting clients to provide settings for them.
    /// </remarks>
    //public interface IExtractSettingsProvider
    //{
    //    ILogger Log { get; }
    //    ProjectConfig Config { get;  }
    //    //T GetSettings<T>() where T : class, IModelExtractSettings;
    //}
}
