using CD.DLS.Common.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.Extract
{
    public class Manifest
    {
        public List<ManifestItem> Items { get; set; }
        public ProjectConfig ProjectConfig { get; set; }
        public DateTime ExtractStart { get; set; }
        public string ExecutedBy { get; set; }
        public Guid ExtractId { get; set; }

        public string Serialize()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public static Manifest Deserialize(string serialized)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.DeserializeObject<Manifest>(serialized, settings);
        }

        public void Merge(Manifest otherManifest)
        {
            Items.AddRange(otherManifest.Items);
        }
    }

    public class ManifestItem
    {
        public string RelativePath { get; set; }
        public string Name { get; set; }
        public int ComponentId { get; set; }
        public string ExtractType { get; set; }
    }
}
