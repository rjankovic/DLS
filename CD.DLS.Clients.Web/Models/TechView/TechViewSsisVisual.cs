using CD.DLS.API.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models.TechView
{
    public class TechViewSsisVisual : ElementTechViewVisual
    {
        public SsisFlowCanvas ControlFlow { get; set; }
        public SsisFlowCanvas DataFlow { get; set; }
    }

    public class VisualPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class SsisVisualBlock
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public string Name { get; set; }
        public string RefPath { get; set; }
        public int ModelElementId { get; set; }
        public VisualPoint Position { get; set; }
        public bool Highlighted { get; set; }
    }

    public class SsisVisualArrow
    {
        public VisualPoint Position { get; set; }
        public List<VisualPoint> Path { get; set; }
    }

    public class SsisFlowCanvas
    {
        public List<SsisVisualBlock> Blocks { get; set; }
        public List<SsisVisualArrow> Arrows { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}