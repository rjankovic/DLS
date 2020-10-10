using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Business.Excel
{
    public abstract class ExcelModelElement : MssqlModelElement
    {
        public ExcelModelElement(RefPath refPath, string caption)
            : base(refPath, caption)
        {
        }

        public ExcelModelElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null)
            : this(refPath, caption)
        {
            this.Parent = parent;
            this.Definition = definition;
        }
    }
}
