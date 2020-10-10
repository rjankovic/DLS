using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Extract.Mssql.Sharepoint
{
    public enum SharepointLibraryItemTypeEnum { Folder, File };
    public class SharepointLibraryItem
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string RelativePath { get; set; }
        public string Content { get; set; }
        public SharepointLibraryItemTypeEnum Type { get; set; }
    }
}
