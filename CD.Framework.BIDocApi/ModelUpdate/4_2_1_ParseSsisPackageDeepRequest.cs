using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.ModelUpdate
{
    /// <summary>
    /// WF pos: 4-2-1
    /// </summary>
    public class ParseSsisPackagesDeepRequest : DLSApiRequest<DLSApiMessage>
    {
        public Guid ExtractId { get; set; }
        /*
        public int PackageExractItemId { get; set; }
        public int XmlExtractItemId { get; set; }
        public string PackageRefPath { get; set; }
        */
        public int ItemIndex { get; set; }
        public string ProjectRefPath { get; set; }
        public int SsisComponentId { get; set; }
        public string ServerRefPath { get; set; }
        public string FolderRefPath { get; set; }
        public List<ParseSsisPackageItem> Items { get; set; }

    }

    public class ParseSsisPackageItem
    {
        public int PackageExractItemId { get; set; }
        public int XmlExtractItemId { get; set; }
        public string PackageRefPath { get; set; }
    }

    //    PackageExractItemId = jn.Item2,
    //    XmlExtractItemId = jn.Item3,
    //    PackageRefPath = jn.Item4,
}
