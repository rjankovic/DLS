using CD.DLS.DAL.Objects.Inspect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models
{
    public class SourceTargetFlowTypes
    {
        public List<ElementTypeDescription> SourceTypes { get; set; }
        public List<ElementTypeDescription> TargetTypes { get; set; }
        public string SourceDescriptivePath { get; set; }
        public string TargetDescriptivePath { get; set; }
    }
}