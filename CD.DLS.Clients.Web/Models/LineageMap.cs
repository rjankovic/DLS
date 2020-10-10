using CD.DLS.DAL.Objects.Inspect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models
{
    public class LineageMap
    {
        public DataFlowBetweenGroupsItem[] DataFlow { get; set; }
        public string SourceDescriptivePath { get; set; }
        public string TargetDescriptivePath { get; set; }
    }
}