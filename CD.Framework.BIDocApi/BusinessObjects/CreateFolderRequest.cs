using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.BusinessObjects
{
    public class CreateFolderRequest : DLSApiRequest<CreateFolderRequestResponse>
    {
        public string FolderName { get; set; }
        public string ParentFolderRefPath { get; set; }
    }

    public class CreateFolderRequestResponse : DLSApiMessage
    {
        public string FolderRefPath { get; set; }
    }
}
