using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Db;
using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.DAL.Objects.Extract;
using CD.BIDoc.Core.Parse.Mssql.Ssis;

namespace CD.DLS.Parse.Mssql.Ssis.SsisDfComponentParser
{
    /// <summary>
    /// The default parser for unimplemented components, lowest priority
    /// </summary>
    class FallbackDfComponentParser : SsisDfComponentParserBase, ISsisDfComponentParser
    {
        public int Priority { get { return 0; } }

        public bool CanParse(SsisDfComponent component)
        {
            return true;
        }

        public DfComponentElement ParseComponent(SsisDfComponentContext context)
        {
            var componentElement = new DfComponentElement(context.ComponentRefPath, context.Component.Name, context.Component.XmlDefinition, context.DfElement);
            context.DfElement.AddChild(componentElement);
            
            return componentElement;
        }
    }
}
