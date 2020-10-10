using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Business.Organization
{
    public abstract class BusinessOrganizationElement : MssqlModelElement
    {
        public BusinessOrganizationElement(RefPath refPath, string caption)
            : base(refPath, caption)
        {
        }

        public BusinessOrganizationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : this(refPath, caption)
        {
            this.Parent = parent;
            this.Definition = definition;
        }
    }
    

    public class BusinessFolderElement : BusinessOrganizationElement
    {
        public BusinessFolderElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public IEnumerable<BusinessFolderElement> Folders { get { return ChildrenOfType<BusinessFolderElement>(); } }
    }

    public class BusinessRootElement : BusinessFolderElement
    {
        public BusinessRootElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {

        }
    }
}
