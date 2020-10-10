using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.ModelUpdate
{
    /// <summary>
    /// WF pos: 2-2
    /// </summary>
    public class ParseSqlDatabaseDeepRequest : DLSApiRequest<DLSApiMessage>
    {
        public Guid ExtractId { get; set; }
        public int DbComponentId { get; set; }
        public string DbRefPath { get; set; }
        public string ServerName { get; set; }
        public string DbName { get; set; }
    }
}
