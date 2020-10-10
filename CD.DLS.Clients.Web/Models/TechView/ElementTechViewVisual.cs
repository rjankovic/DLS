using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models.TechView
{
    public enum VisualTypeEnum { None, Code, Ssis, Report }

    public class ElementTechViewVisual
    {
        public string VisualType { get; set; }
    }
}