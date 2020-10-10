using CD.DLS.DAL.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models
{
    public class FulltextSearchResults
    {
        public Guid TileId { get; set; }
        public List<FulltextSearchResult> Results { get; set; }
    }
}