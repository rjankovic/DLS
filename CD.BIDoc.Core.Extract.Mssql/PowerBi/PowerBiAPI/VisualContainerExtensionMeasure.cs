using CD.DLS.DAL.Objects.Extract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Extract.PowerBi.PowerBiAPI
{
    class VisualContainerExtensionMeasure
    {
        public string TableName { get; set; }
        public string MeasureName { get; set; }
        public string Expression { get; set; }
    }
}
