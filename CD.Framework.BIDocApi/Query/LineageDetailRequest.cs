using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.Query
{
    public enum LineageDetailLevelEnum
    {
        High = 1, Medium = 2, Low = 3
    }

    public class LineageDetailRequest : DLSApiRequest<LineageDetailResponse>
    {
        public string SourceRefPath { get; set; }
        public string TargetRefPath { get; set; }
        public LineageDetailLevelEnum DetailLevel { get; set; }
    }
    
    public class LineageDetailResponse : DLSApiMessage
    {
        public List<NodeDescription> Nodes { get; set; }
        public List<LinkDeclaration> Links { get; set; }
        public string SourceRefPath { get; set; }
        public string TargetRefPath { get; set; }
    }
}
