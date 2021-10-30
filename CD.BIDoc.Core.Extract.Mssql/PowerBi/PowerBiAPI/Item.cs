using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Extract.PowerBi.PowerBiAPI
{
    class Item
    {
        //private int id;
        //private int resourcePackageId;
        //private int type;
        //private string path;
        //private string name;

        public Item(int id, int resourcePackageId, int type, string path, string name)
        {
            this.Id = id;
            this.ResourcePackageId = resourcePackageId;
            this.Type = type;
            this.Path = path;
            this.Name = name;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public int Type { get; set; }
        public int ResourcePackageId { get; set; }
        public int Id { get; set; }
    }
}
