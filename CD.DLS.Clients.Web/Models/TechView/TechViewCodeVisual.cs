using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models.TechView
{
    public class TechViewCodeVisual : ElementTechViewVisual
    {
        public string Text { get; set; }
        public int HighlightFrom { get; set; }
        public int HighlightLength { get; set; }
    }
}