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
    class GeneralSourceDfComponentParser : SsisDfComponentParserBase, ISsisDfComponentParser
    {
        public int Priority { get { return 5; } }

        public bool CanParse(SsisDfComponent component)
        {
            return component.ClassId.Contains("Source");
        }

        public DfComponentElement ParseComponent(SsisDfComponentContext context)
        {
            var componentElement = new DfSourceElement(context.ComponentRefPath, context.Component.Name, context.Component.XmlDefinition, context.DfElement);
            context.DfElement.AddChild(componentElement);

            if (!string.IsNullOrEmpty(context.Component.GetPropertyValue("OpenRowset")) || !string.IsNullOrEmpty(context.Component.GetPropertyValue("OpenRowsetVariable")))
            {
                string openRowset = context.Component.GetPropertyValue("OpenRowset");
                componentElement.OpenRowset = openRowset;
            }

            string conMagId = null;
            if (context.Component.Connections.Count > 0)
            {
                conMagId = context.Component.Connections[0].ConnectionManagerID;
            }

            var sourceOutput = context.Component.Outputs[0];
            if (sourceOutput.IsErrorOutput)
            {
                sourceOutput = context.Component.Outputs[1];
            }

            ConnectionManagerElement conMagNode;
            if (context.Connections.TryGetConnectionManager(conMagId, out conMagNode))
            {
                componentElement.SourceConnection = conMagNode;
                if (conMagNode.SourceType == "EXCEL" || conMagNode.SourceType == "FLATFILE" || conMagNode.SourceType.Contains("XML") || conMagNode.SourceType.ToLower().Contains("odata"))
                {
                    componentElement.IsExternalSource = true;
                }
            }
            
            return componentElement;
        }
    }
}
