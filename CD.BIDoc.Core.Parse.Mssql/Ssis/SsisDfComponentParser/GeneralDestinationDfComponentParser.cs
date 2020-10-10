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
using CD.DLS.Parse.Mssql.Db;

namespace CD.DLS.Parse.Mssql.Ssis.SsisDfComponentParser
{
    class GeneralDestinationDfComponentParser : SsisDfComponentParserBase, ISsisDfComponentParser
    {
        public int Priority { get { return 10; } }

        public bool CanParse(SsisDfComponent component)
        {
            return component.ContractBase == "OLE DB Destination" || component.ContractBase == "ODBC Destination"; ;
        }

        public DfComponentElement ParseComponent(SsisDfComponentContext context)
        {
            var componentElement = new DfDestinationElement(context.ComponentRefPath, context.Component.Name, context.ComponentDefinitionXml.OuterXml, context.DfElement);
            context.DfElement.AddChild(componentElement);

            string conMagId = null;
            DbConnectionManagerElement conMagNode = null;

            Dictionary<string, MssqlModelElement> targetColumnsFromTargetNames = new Dictionary<string, MssqlModelElement>(StringComparer.CurrentCultureIgnoreCase);

            string dbName = null;
            if (context.Component.Connections.Count > 0)
            {
                conMagId = context.Component.Connections[0].ConnectionManagerID;
            }

            string tgtTableName = string.Empty;
            string destinationType = string.Empty;

            if (context.Component.ContractBase.Equals("OLE DB Destination"))
            {
                destinationType = "OpenRowset";
            } else
            {
                destinationType = "TableName";
            }

            if (context.Connections.TryGetDbConnectionManager(conMagId, out conMagNode))
            {
                if (conMagNode.ProviderType == ConnectionStringParts.ProviderTypeEnum.SQLNCLI)
                {
                    //dbName = conMagNode.ConnectionDbName;
                    //conMagNode.ConnectionServerName

                    dbName = conMagNode.ConnectionDbName;
                    var serverName = conMagNode.ConnectionServerName;

                    //var originalServerName = context.DbIndex.ContextServerName;
                    //context.DbIndex.SetContextServer(serverName, componentElement.LocalhostServer());
                    var dbIndex = context.DbIndex.GetDatabaseIndex(serverName, componentElement.LocalhostServer());

                    var dbIdentifier = new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier() { Value = dbName };
                    tgtTableName = GetPropertyValueOrVariable(context.Component, destinationType, context.Referrables);
                    var potentialTempTableName = tgtTableName.TrimStart('[').TrimEnd(']');
                    
                    if (context.TempTablesAvailable.ContainsKey(potentialTempTableName))
                    {
                        var tempTargetTable = context.TempTablesAvailable[potentialTempTableName];
                        foreach (var column in tempTargetTable.Columns)
                        {
                            targetColumnsFromTargetNames.Add(((Microsoft.SqlServer.TransactSql.ScriptDom.Identifier)(column.Identifier)).Value, (MssqlModelElement)(column.ModelElement));
                        }
                    }
                    else
                    {
                        var tgtTableNode = dbIndex.FindNodeByTableObjectName(tgtTableName, context.SqlScriptExtractor.Parser, dbIdentifier);
                        if (tgtTableNode != null)
                        {

                            foreach (var column in ((MssqlColumnScriptElement)tgtTableNode).Columns)
                            {
                                targetColumnsFromTargetNames.Add(column.Caption, column);
                            }
                        }
                    }
                }
            }

            // only error outputs appear in destination outputs
            var destinationInput = context.Component.Inputs[0];


            Dictionary<int, MssqlModelElement> externalIdsToNodes = new Dictionary<int, MssqlModelElement>();
            foreach (var externalCol in destinationInput.ExternalColumns)
            {
                if (targetColumnsFromTargetNames.ContainsKey(externalCol.Name))
                {
                    externalIdsToNodes.Add(externalCol.ID, targetColumnsFromTargetNames[externalCol.Name]);
                }
            }

            XmlElement inputDefinitionXml = null;
            DfInputElement inputNode = new DfInputElement(context.UrnBuilder.GetDfInputUrn(componentElement, destinationInput.Name), destinationInput.Name,
                context.DefinitionSearcher.GetDfComponentInputDefinition(context.ComponentDefinitionXml, destinationInput.IdString, out inputDefinitionXml), componentElement);
            componentElement.AddChild(inputNode);
            inputNode.InputType = DfInputTypeEnum.Input;

            ComponentInput destinationInputMapping = new ComponentInput() { ModelElement = inputNode };
            context.ComponentIO.Inputs[destinationInput.IdString] = destinationInputMapping;

            foreach (var inputColumn in destinationInput.Columns)
            {
                DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfInputColumnUrn(inputNode, inputColumn.Name, inputColumn.ID),
                    inputColumn.Name, context.DefinitionSearcher.GetDfInputColumnDefinition(inputDefinitionXml, inputColumn.IdentificationString), inputNode);

                colNode.Precision = inputColumn.Precision;
                colNode.Scale = inputColumn.Scale;
                colNode.Length = inputColumn.Length;
                colNode.DtsDataType = inputColumn.DataType.ToString();

                inputNode.AddChild(colNode);
                destinationInputMapping[inputColumn.Name] = colNode;

                // if all external columns could be resolved from source
                if (/*externalIdsToNodes.Count >= destinationInput.InputColumnCollection.Count &&*/ externalIdsToNodes.ContainsKey(inputColumn.ExternalColumnID))
                {
                    var externalColNode = externalIdsToNodes[inputColumn.ExternalColumnID];
                    colNode.ExternalDestinationColumn = externalColNode;
                }
            }

            return componentElement;
        }
    }
}
