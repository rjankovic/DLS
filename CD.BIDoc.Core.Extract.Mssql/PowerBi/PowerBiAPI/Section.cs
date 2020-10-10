using CD.DLS.DAL.Objects.Extract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Extract.PowerBi.PowerBiAPI
{
    class Section
    {
        

        public Section(string id, VisualContainer[] visualContainers, string name, string filters, double width, double height, string config, string displayName, string objectId, int displayOption)
        {
            this.Id = id;
            this.VisualContainers = visualContainers;
            this.Name = name;
            this.Filters = filters;
            this.Width = width;
            this.Height = height;
            this.Config = config;
            this.DisplayName = displayName;
            this.ObjectId = objectId;
            this.DisplayOption = displayOption;
        }
        

        public int DisplayOption { get; set; }
        public string ObjectId { get; set; }
        public string DisplayName { get; set; }
        public string Config { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public string Filters { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        internal VisualContainer[] VisualContainers { get; set; }

        public Filter[] GetFilters()
        {
            if (Filters == null)
            {
                return null;
            }
            return JsonConvert.DeserializeObject<Filter[]>(Filters);
        }
    }
}
