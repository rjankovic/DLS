using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Common.Structures
{
    public enum DocumentTypeEnum { SqlCode, SsisGraph, MdxCode, BusinessDictionary, SsisExpressionCode }
    public class GraphDocument
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string NodeRefPath { get; set; }
        public DocumentTypeEnum DocumentType { get; set; }
        public int GraphNodeId { get; set; }
    }

}