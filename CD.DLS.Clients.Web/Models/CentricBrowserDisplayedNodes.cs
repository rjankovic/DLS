using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models
{
    public class CentricBrowserDisplayedNodes
    {
        public int CentralNodeId { get; set; }
        public int[] DisplayedNodeIds { get; set; }
    }

    public class CentricBrowserDisplaySwitch : CentricBrowserDisplayedNodes
    {
        public int SourceDetailLevel { get; set; }
        public int TargetDetailLevel { get; set; }
    }
}