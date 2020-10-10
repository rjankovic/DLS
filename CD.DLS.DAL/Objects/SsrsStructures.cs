using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.SsrsStructures
{
    public class SsrsReportListItem
    {
        public int ModelElementId { get; set; }
        public string RefPath { get; set; }
        public string SsrsPath { get; set; }
        public int SsrsComponentId { get; set; }
        public string Name { get; set; }
    }
}
