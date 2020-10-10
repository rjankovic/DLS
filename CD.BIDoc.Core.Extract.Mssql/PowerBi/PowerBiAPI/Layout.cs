using CD.DLS.DAL.Objects.Extract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Extract.PowerBi.PowerBiAPI
{
    class Layout
    {
        public Layout(string id, Section[] sections, Pod[] pods, string config, string reportId,string filters, ResourcePackage[] resourcePackages, int layoutOptimalization)
        {
            this.Id = id;
            this.Sections = sections;
            this.Pods = pods;
            this.Config = config;
            this.ReportId = reportId;
            Filters = filters;
            this.ResourcePackages = resourcePackages;
            this.LayoutOptimalization = layoutOptimalization;
        }

        public int LayoutOptimalization { get; set; }
        public string ReportId { get; set; }
        public string Config { get; set; }
        public Pod[] Pods { get; set; }
        public string Id { get; set; }
        internal ResourcePackage[] ResourcePackages { get; set; }
        internal Section[] Sections { get; set; }
        internal string Filters { get; set; }

        public Filter[] GetFilters()
        {
            if (String.IsNullOrEmpty(Filters))
            {
                return Array.Empty<Filter>();
            }
            return JsonConvert.DeserializeObject<Filter[]>(Filters);
        }
    }
}
