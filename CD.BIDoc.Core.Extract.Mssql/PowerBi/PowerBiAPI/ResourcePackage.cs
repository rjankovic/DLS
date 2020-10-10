using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Extract.PowerBi.PowerBiAPI
{
    class ResourcePackage
    {

        public ResourcePackage(int id, int type, string name, Item[] items, bool disabled)
        {
            this.Id = id;
            this.Type = type;
            this.Name = name;
            this.Items = items;
            this.Disabled = disabled;
        }

        public bool Disabled { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int Id { get; set; }
        internal Item[] Items { get; set; }
    }
}
