using CD.DLS.API.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models
{
    public class SourceTargetFlowDetailSpec
    {
        public LineageDetailLevelEnum LineageDetailLevel { get; set; }
        public string SourceElementRefPath { get; set; }
        public string TargetElementRefPath { get; set; }
        public int SourceElementId { get; set; }
        public int TargetElementId { get; set; }
    }
}