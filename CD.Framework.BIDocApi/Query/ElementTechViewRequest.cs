using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.Query
{
    public class ElementTechViewRequest : DLSApiRequest<ElementTechViewResponse>
    {
        public int ElementId { get; set; }
    }

    public class ElementTechViewResponse : DLSApiMessage
    {
        public NodeDescription NodeDescription { get; set; }
        public VisualNodeDescription VisualAncestor { get; set; }
        public int ElementId { get; set; }
        public int RequestedElementId { get; set; }
    }

    //public class DesignBlock
    //{
    //    public DesignPoint Position { get; set; }
    //    public DesignPoint Size { get; set; }
    //    public List<DesignBlock> ChildBlocks { get; set; }

    //    public List<DesignArrow> Arrows { get; set; }
    //    public string RefPath { get; set; }
    //    public int ElementId { get; set; }
    //    public string ElementType { get; set; }
    //    public string Name { get; set; }

    //}
}
