using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.ModelUpdate
{
    /// <summary>
    /// WF pos: 7-1
    /// </summary>
    public class ParsePivotTableTemplatesRequest : DLSApiRequest<DLSApiMessage>
    {
        public int ItemIndex { get; set; }
        public List<PivotTableParserReference> Items { get; set; }
    }

    public class PivotTableParserReference
    {
        public string PivotTableRefPath { get; set; }
    }
}
