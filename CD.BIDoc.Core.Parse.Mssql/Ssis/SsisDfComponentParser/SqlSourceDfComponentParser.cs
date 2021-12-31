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
    class SqlSourceDfComponentParser : SsisDfComponentParserBase, ISsisDfComponentParser
    {
        public int Priority { get { return 10; } }

        public bool CanParse(SsisDfComponent component)
        {
            return component.Contract.Contains("OLE DB Source") || component.Contract.Contains("ODBC Source");
        }

        public DfComponentElement ParseComponent(SsisDfComponentContext context)
        {
            var componentElement = new DfSourceElement(context.ComponentRefPath, context.Component.Name, context.ComponentDefinitionXml.OuterXml, context.DfElement);
            var sourceElement = componentElement as DfSourceElement;
            sourceElement.IsExternalSource = false;
            context.DfElement.AddChild(componentElement);
            
            DbConnectionManagerElement conMagNode = null;

            string dbName = null;
            string serverName = null;
            string conMagId = null;

            if (context.Component.Connections.Count > 0)
            {
                conMagId = context.Component.Connections[0].ConnectionManagerID;
            }

            Dictionary<string, MssqlModelElement> outputColumnsFromExtMetadata = new Dictionary<string, MssqlModelElement>();
            var sourceOutput = context.Component.Outputs[0];
            if (sourceOutput.IsErrorOutput)
            {
                sourceOutput = context.Component.Outputs[1];
            }


            if (context.Connections.TryGetDbConnectionManager(conMagId, out conMagNode))
            {
                ((DfSourceElement)componentElement).SourceConnection = conMagNode;

                if (conMagNode.ProviderType == ConnectionStringParts.ProviderTypeEnum.SQLNCLI)
                {
                    dbName = conMagNode.ConnectionDbName;
                    serverName = conMagNode.ConnectionServerName;

                    //var originalServerName = context.DbIndex.ContextServerName;
                    //context.DbIndex.SetContextServer(serverName, componentElement.LocalhostServer());
                    var dbIndex = context.DbIndex.GetDatabaseIndex(serverName, componentElement.LocalhostServer());

                    var dbIdentifier = new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier() { Value = dbName };


                    Dictionary<string, MssqlModelElement> outputColumnsFromNames = null;

                    var accessMode = context.DefinitionSearcher.GetAccessMode(context.ComponentDefinitionXml);
                                                                                               
                    if (accessMode == SsisXmlProvider.AccessMode.SqlCommand || accessMode == SsisXmlProvider.AccessMode.SqlCommandVariable)
                    {

                        string sqlCommand;
                        if (accessMode == SsisXmlProvider.AccessMode.SqlCommandVariable)
                        {
                            var variableName = context.Component.GetPropertyValue("SqlCommandVariable");
                            sqlCommand = context.Referrables.GetValueByName(variableName);
                        }
                        else
                        {
                            sqlCommand = context.Component.GetPropertyValue("SqlCommand");
                        }

                        if (sqlCommand != null)
                        {
                            (componentElement as DfSourceElement).Command = sqlCommand;
                        }

                        Dictionary<string, ReferrableValueElement> inputParamMapping = new Dictionary<string, ReferrableValueElement>();
                        Dictionary<string, ReferrableValueElement> outputParamMapping = new Dictionary<string, ReferrableValueElement>();

                        var sourceParametrized = sqlCommand;

                        var variablesIndex = new Db.AddPresetVariablesReferrableIndex(dbIndex);

                        if (context.Component.Properties.Any(x => x.Name == "ParameterMapping"))
                        {
                            ParameterMappings paramMappings = new ParameterMappings(context.Component.GetPropertyValue("ParameterMapping"));
                            
                            var paramNo = 0;
                            foreach (var paramMapping in paramMappings.Mappings)
                            {
                                var sqlParName = "@" + SQLUtils.RemoveSqlParamSpecialCharacters(paramMapping.ParameterName) + "_" + paramNo.ToString() + (paramMapping.Directíon == ParameterMappings.ParameterDirectionEnum.Input ? "" : "_Output");
                                paramNo++;
                                sourceParametrized = SQLUtils.ParametrizeSql(sourceParametrized, sqlParName);

                                ReferrableValueElement variableNode;
                                if (context.Referrables.TryGetNodeById(paramMapping.VariableId, out variableNode))
                                {
                                    if (paramMapping.Directíon == ParameterMappings.ParameterDirectionEnum.Input)
                                    {
                                        inputParamMapping.Add(sqlParName, variableNode);

                                        variablesIndex.AddPresetVariable(sqlParName, variableNode);
                                    }
                                    else
                                    {
                                        // do output params have any place in DF Sources?
                                        outputParamMapping.Add(sqlParName, variableNode);
                                    }
                                }
                            }
                        }

                        sourceElement.IsExternalSource = false; // !context.Settings.Config.DatabaseComponents.Any(x => x.DbName == dbIdentifier.Value);
                        foreach (var tempTable in context.TempTablesAvailable)
                        {
                            variablesIndex.AddTable(tempTable.Value);
                        }

                        try
                        {

                            var sqlNode = context.SqlScriptExtractor.ExtractScriptModel(sourceParametrized, componentElement, variablesIndex, dbIdentifier, out outputColumnsFromNames);
                            if (componentElement.Caption == "Get Applicants Helios")
                            {

                            }
                            componentElement.AddChild(sqlNode);
                        }
                        catch
                        {
                            // TODO: not like this
                        }

                        variablesIndex.ClearPresetTables();

                    }
                    else if (accessMode == SsisXmlProvider.AccessMode.OpenRowset || accessMode == SsisXmlProvider.AccessMode.OpenRowsetVariable)
                    {
                        string openRowset;

                        if (accessMode == SsisXmlProvider.AccessMode.OpenRowsetVariable)
                        {
                            var variableName = context.Component.GetPropertyValue("OpenRowsetVariable");
                            openRowset = context.Referrables.GetValueByName(variableName);
                        }
                        else
                        {
                            openRowset = context.Component.GetPropertyValue("OpenRowset");
                        }
                        
                        componentElement.OpenRowset = openRowset;


                        var srcTableNode = dbIndex.FindNodeByTableObjectName(openRowset, context.SqlScriptExtractor.Parser, dbIdentifier);
                        if (srcTableNode != null)
                        {
                            outputColumnsFromNames = new Dictionary<string, MssqlModelElement>();
                            //#if vdfalse
                            foreach (var column in ((MssqlColumnScriptElement)srcTableNode).Columns)
                            {
                                outputColumnsFromNames.Add(column.Caption, column);
                            }
                            //#endif
                        }
                        sourceElement.IsExternalSource = srcTableNode == null;
                    }
                    else
                    {
                        throw new Exception();
                        // TODO: can there be something else (Variable source is not OLE DB Source and does not use SQNCLI)
                    }


                    // if all external columns could be resolved from source
                    if (outputColumnsFromNames != null /*&& sourceOutput.ExternalMetadataColumnCollection.Count == outputColumnsFromNames.Count*/)
                    {
                        foreach (var outputColumn in sourceOutput.ExternalColumns /*sourceOutput.ExternalMetadataColumnCollection*/)
                        {
                            var colName = outputColumn.Name;
                            if (outputColumnsFromNames.ContainsKey(colName))
                            {
                                outputColumnsFromExtMetadata[outputColumn.LineageID] = outputColumnsFromNames[colName];
                            }
                        }
                    }

                    //context.DbIndex.SetContextServer(originalServerName);
                }
                else
                {

                    MssqlModelElement foreignSource = null;


                    if (!string.IsNullOrEmpty(context.Component.GetPropertyValue("SqlCommand")) || !string.IsNullOrEmpty(context.Component.GetPropertyValue("SqlCommandVariable")))
                    {

                        string sqlCommand = context.Component.GetPropertyValue("SqlCommand");
                        if (string.IsNullOrEmpty(sqlCommand))
                        {
                            var variableName = context.Component.GetPropertyValue("SqlCommandVariable");
                            sqlCommand = context.Referrables.GetValueByName(variableName);
                        }

                        if (sqlCommand != null)
                        {
                            (componentElement as DfSourceElement).Command = sqlCommand;

                            sourceElement.IsExternalSource = true;

                            var sqlNodePath = componentElement.RefPath.Child("ForeignSqlCommand");


                            Dictionary<string, MssqlModelElement> outputColumnsFromNames = null;

                            var sqlNode = context.SqlScriptExtractor.ExtractForeignScriptModel(sqlCommand, componentElement, out outputColumnsFromNames);
                            componentElement.AddChild(sqlNode);
                            foreignSource = sqlNode;

                        }

                    }
                    else if (!string.IsNullOrEmpty(context.Component.GetPropertyValue("OpenRowset")) || !string.IsNullOrEmpty(context.Component.GetPropertyValue("OpenRowsetVariable")))
                    {
                        string openRowset = GetPropertyValueOrVariable(context.Component, "OpenRowset", context.Referrables);


                        sourceElement.IsExternalSource = true;

                        var openRowsetPath = componentElement.RefPath.Child("Openrowset");
                        ForeignDbTableElement foreignTable = new ForeignDbTableElement(openRowsetPath, openRowset);
                        foreignTable.Definition = openRowset;
                        foreignTable.Parent = componentElement;
                        ((DfSourceElement)componentElement).OpenRowset = openRowset;
                        componentElement.AddChild(foreignTable);
                        foreignSource = foreignTable;

                    }
                    else if (!string.IsNullOrEmpty(context.Component.GetPropertyValue("TableName")))
                    {
                        string table = GetPropertyValueOrVariable(context.Component, "TableName", context.Referrables);
                        var tableSelfName = table.Split(new string[] { "\".\"" }, StringSplitOptions.None).Last();
                        tableSelfName = tableSelfName.Trim('"');
                        
                        var openRowsetPath = componentElement.RefPath.Child("Table");
                        ForeignDbTableElement foreignTable = new ForeignDbTableElement(openRowsetPath, tableSelfName);
                        sourceElement.IsExternalSource = true;
                        foreignTable.Definition = table;
                        foreignTable.Parent = componentElement;
                        componentElement.AddChild(foreignTable);
                        foreignSource = foreignTable;
                        componentElement.OpenRowset = table;

                    }
                    else
                    {
                        throw new Exception();
                        // TODO: can there be something else (Variable source is not OLE DB Source and does not use SQNCLI)
                    }


                    // if all external columns could be resolved from source
                    if (foreignSource != null)
                    {
                        foreach (var outputColumn in sourceOutput.ExternalColumns)
                        {
                            var colName = outputColumn.Name;
                            outputColumnsFromExtMetadata[outputColumn.LineageID] = foreignSource;
                        }
                    }
                }
            }
            else
            {
                sourceElement.IsExternalSource = true;
            }

            XmlElement outputDefinitionXml = null;
            DfOutputElement outputNode = new DfOutputElement(context.UrnBuilder.GetDfOutputUrn(componentElement, sourceOutput.Name),
                sourceOutput.Name, context.DefinitionSearcher.GetDfComponentOutputDefinition(context.ComponentDefinitionXml, sourceOutput.RefId, out outputDefinitionXml), componentElement);
            outputNode.OutputType = sourceOutput.IsErrorOutput ? DfOutputTypeEnum.ErrorOutput : DfOutputTypeEnum.Output;
            componentElement.AddChild(outputNode);


            //var sourceOutputColMapping = componentMapping.AddOutput(sourceOutput.IdentificationString, outputNode);

            ComponentOutput sourceOutputColMapping = new ComponentOutput() { ModelElement = outputNode };
            context.ComponentIO.Outputs[sourceOutput.RefId] = sourceOutputColMapping;



            foreach (var outputColumn in sourceOutput.Columns)
            {
                DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfOutputColumnUrn(outputNode, outputColumn.Name/*, outputColumn.ID*/), outputColumn.Name,
                    context.DefinitionSearcher.GetDfOutputColumnDefinition(outputDefinitionXml, outputColumn.IdentificationString), outputNode);

                colNode.Precision = outputColumn.Precision;
                colNode.Scale = outputColumn.Scale;
                colNode.Length = outputColumn.Length;
                colNode.DtsDataType = outputColumn.DataType.ToString();
                outputNode.AddChild(colNode);

                sourceOutputColMapping[outputColumn.Name] = colNode;
                if (outputColumnsFromExtMetadata.ContainsKey(outputColumn.ExternalColumnID))
                {
                    var externalColNode = outputColumnsFromExtMetadata[outputColumn.ExternalColumnID];
                    colNode.ExternalSourceColumn = externalColNode;
                    
                }
            }

            return componentElement;
        }
    }
}
