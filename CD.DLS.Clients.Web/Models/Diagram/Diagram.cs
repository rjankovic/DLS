using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models.Diagram
{
    public class DiagramNode
    {
        public double width { get; set; }
        public double height { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int id { get; set; }

        public DiagramNode(int id, string name, string description)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            height = 70;
            width = 100 + Math.Max(name.Length, description.Length) * 5;
        }
    }

    public class DiagramLink
    {
        public int id { get; set; }
        public int source { get; set; }
        public int target { get; set; }
    }

    public class Diagram
    {
        public int DetailLevel { get; set; }
        public DiagramNode[] Nodes { get; set; }
        public DiagramLink[] Links { get; set; }
    }
}