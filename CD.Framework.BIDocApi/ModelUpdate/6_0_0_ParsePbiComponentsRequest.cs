﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.ModelUpdate
{
    /// <summary>
    /// WF pos: 6
    /// </summary>
    public class ParsePbiComponentsRequest : DLSApiRequest<DLSApiProgressResponse>
    {
        public Guid ExtractId { get; set; }
    }
}
