using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models
{
    public class MainMenu
    {
        public List<MenuItem> Items { get; set; }
        public string Message { get; set; }
    }

    public class MenuItem
    {
        public string Name { get; set; }
        public string Action { get; set; }
        public bool HasAction { get { return Action != null; } }

        public List<MenuItem> Items { get; set; }
        public bool HasItems { get { return Items != null && Items.Count > 0; } }
    }
}