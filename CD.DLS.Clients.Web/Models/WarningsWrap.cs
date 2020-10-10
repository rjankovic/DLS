using CD.DLS.DAL.Objects.Inspect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models
{
    public class WarningsWrap
    {
        public List<WarningMessagesItem> Warnings { get; set; }
        public string TileId { get; set; }
    }
}