//using CD.Framework.Common.Structures;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CD.Framework.EF.Entities
//{
//    public class BIDocGraphInfoNode
//    {
//        public int Id { get; set; }
//        public string RefPath { get; set; }
//        public string Name { get; set; }
//        public string NodeType { get; set; }
//        public string DocumentRelativePath { get; set; }
//        public string Description { get; set; }

//        public int? ParentId { get; set; }

//        public DependencyGraphKind GraphKind { get; set; }
        
//        public Guid ProjectConfigId { get; set; }
//    }

//    public class BIDocDataFlowGraphInfoNode : BIDocGraphInfoNode
//    {
//        public int TopologicalOrder { get; set; }
//    }

//    public class BIDocGraphInfoLink
//    {
//        public int Id { get; set; }
//        public LinkTypeEnum LinkType { get; set; }
        
//        public int NodeFromId { get; set; }
//        public int NodeToId { get; set; }

//        public Guid ProjectConfigId { get; set; }
//    }
//}
