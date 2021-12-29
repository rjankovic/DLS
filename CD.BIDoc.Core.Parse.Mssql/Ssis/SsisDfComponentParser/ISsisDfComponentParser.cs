using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.DAL.Objects.Extract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.BIDoc.Core.Parse.Mssql.Ssis;

namespace CD.DLS.Parse.Mssql.Ssis.SsisDfComponentParser
{
    public interface ISsisDfComponentParser
    {
        /// <summary>
        /// Higher value = higher priority; > 0; 0 reserved for fallback parser
        /// </summary>
        int Priority { get; }
        bool CanParse(SsisDfComponent component);
        DfComponentElement ParseComponent(SsisDfComponentContext context);
    }
}
