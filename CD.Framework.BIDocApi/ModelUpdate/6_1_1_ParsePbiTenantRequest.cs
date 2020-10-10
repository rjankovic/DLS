using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.ModelUpdate
{
    
        /// <summary>
        /// WF pos: 5-1-1
        /// </summary>
        public class ParsePbiTenantRequest : DLSApiRequest<DLSApiMessage>
        {
            public Guid ExtractId { get; set; }
            public int ItemIndex { get; set; }
            public string TenantRefPath { get; set; }
            public List<ParsePbiTenantItem> TenantItems { get; set; }
            public int PbiComponentId { get; set; }
        }

        public class ParsePbiTenantItem
        {
            public int ExtractItemId { get; set; }
        }
    
}
