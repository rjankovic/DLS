using CD.DLS.DAL.Objects.Extract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Extract.PowerBi.PowerBiAPI
{
    class VisualContainer
    {


        public VisualContainer(string id, double x, double y, double z, double width, double height, string config, string filters, string query, string queryHash, string dataTransforms)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Width = width;
            this.Height = height;
            this.Config = config;
            this.Filters = filters;
            this.Query = query;
            this.QueryHash = queryHash;
            this.DataTransforms = dataTransforms;
        }

        public string Query { get; set; }
        public string QueryHash { get; set; }
        public string DataTransforms { get; set; }
        public string Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Config { get; set; }
        public string Filters { get; set; }
        //public List<VisualContainerExtensionMeasure> ExtensionMeasures { get; set; } = new List<VisualContainerExtensionMeasure>();


        public VisualContainerConfig GetConfig()
        {
            return JsonConvert.DeserializeObject<VisualContainerConfig>(Config);
        }

        public Filter[] GetFilters()
        {           
            if (Filters == null)
            {
                return Array.Empty<Filter>() ;
            }
            return JsonConvert.DeserializeObject<Filter[]>(Filters);
        }
    }
}
