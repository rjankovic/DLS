using CD.DLS.API.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models.TechView
{
    public class TechViewReportVisual : ElementTechViewVisual
    {
        public ReportGridStructure Structure { get; set; }
    }
}