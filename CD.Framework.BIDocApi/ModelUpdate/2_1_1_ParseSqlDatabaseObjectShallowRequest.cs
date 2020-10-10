using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.ModelUpdate
{
    /// <summary>
    /// WF pos: 2-1-1
    /// </summary>
    public class ParseSqlDatabaseObjectShallowRequest : DLSApiRequest<DLSApiMessage>
    {
        public Guid ExtractId { get; set; }
        public int ExtractItemId { get; set; }
        public string DatabaseRefPath { get; set; }
    }
}
