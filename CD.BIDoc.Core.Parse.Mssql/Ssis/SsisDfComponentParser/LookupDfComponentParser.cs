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
    class LookupDfComponentParser : SsisDfComponentParserBase, ISsisDfComponentParser
    {
        public int Priority { get { return 10; } }

        public bool CanParse(SsisDfComponent component)
        {
            return component.ContractBase == "Lookup";
        }

        public DfComponentElement ParseComponent(SsisDfComponentContext context)
        {
            var componentElement = new DfLookupElement(context.ComponentRefPath, context.Component.Name, context.ComponentDefinitionXml.OuterXml, context.DfElement);
            context.DfElement.AddChild(componentElement);

            string conMagId = null;
            DbConnectionManagerElement conMagNode = null;

            Dictionary<string, DbModelElement> targetColumnsFromTargetNames = new Dictionary<string, DbModelElement>();

            string dbName = null;
            if (context.Component.Connections.Count > 0)
            {
                conMagId = context.Component.Connections[0].ConnectionManagerID;
            }
            if (context.Connections.TryGetDbConnectionManager(conMagId, out conMagNode))
            {
                if (conMagNode.ProviderType == ConnectionStringParts.ProviderTypeEnum.SQLNCLI)
                {
                    dbName = conMagNode.ConnectionDbName;
                    var serverName = conMagNode.ConnectionServerName;

                    //var originalServerName = context.DbIndex.ContextServerName;
                    //context.DbIndex.SetContextServer(serverName, componentElement.LocalhostServer());
                    var dbIndex = context.DbIndex.GetDatabaseIndex(serverName, componentElement.LocalhostServer());


                    var dbNameIdentifier = new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier() { Value = dbName };

                    var sqlCommand = context.Component.GetPropertyValue("SqlCommand");

                    context.SqlScriptExtractor.ContextServerName = dbIndex.ContextServerName;

                    bool continueParametrization = true;
                    var paramCounter = 0;
                    while (continueParametrization)
                    {
                        if (sqlCommand.IndexOf("?") == -1)
                        {
                            break;
                        }

                        var sqlNew = SQLUtils.ParametrizeSql(sqlCommand, "p" + paramCounter.ToString());
                        if (sqlNew == sqlCommand)
                        {
                            continueParametrization = false;
                        }
                        sqlCommand = sqlNew;
                        paramCounter++;
                    }

                    Dictionary<string, MssqlModelElement> externalSourceColumnsFromNames = new Dictionary<string, MssqlModelElement>();

                    var sqlNode = context.SqlScriptExtractor.ExtractScriptModel(sqlCommand, componentElement, dbIndex, dbNameIdentifier, out externalSourceColumnsFromNames);
                    componentElement.AddChild(sqlNode);

                    var lookupInput = context.Component.Inputs[0];
                    XmlElement inputDefinitionXml = null;
                    DfInputElement inputNode = new DfInputElement(context.UrnBuilder.GetDfInputUrn(componentElement, lookupInput.Name),
                        lookupInput.Name, context.DefinitionSearcher.GetDfComponentInputDefinition(context.ComponentDefinitionXml, lookupInput.IdString, out inputDefinitionXml), componentElement);
                    componentElement.AddChild(inputNode);

                    inputNode.InputType = DfInputTypeEnum.Input;

                    ComponentInput lookupInputMapping = new ComponentInput() { ModelElement = inputNode };
                    context.ComponentIO.Inputs[lookupInput.IdString] = lookupInputMapping;
                    List<DfColumnElement> lookupJoinColumns = new List<DfColumnElement>();

                    SsisDfOutput matchOutput = null;
                    foreach (var output in context.Component.Outputs)
                    {
                        if (output.Name == "Lookup Match Output" && !output.IsErrorOutput)
                        {
                            matchOutput = output;
                        }
                    }
                    if (matchOutput == null)
                    {
                        throw new Exception("Missing lookup match output");
                    }

                    XmlElement outputDefinitionXml = null;
                    DfOutputElement outputNode = new DfOutputElement(context.UrnBuilder.GetDfOutputUrn(componentElement, matchOutput.Name), matchOutput.Name, context.DefinitionSearcher
                        .GetDfComponentOutputDefinition(context.ComponentDefinitionXml, matchOutput.IdString, out outputDefinitionXml), componentElement);
                    componentElement.AddChild(outputNode);
                    outputNode.OutputType = matchOutput.IsErrorOutput ? DfOutputTypeEnum.ErrorOutput : DfOutputTypeEnum.Output;

                    ComponentOutput lookupMatchOutputMapping = new ComponentOutput() { ModelElement = outputNode };
                    context.ComponentIO.Outputs[matchOutput.IdString] = lookupMatchOutputMapping;

                    foreach (var inputCol in lookupInput.Columns)
                    {
                        var name = inputCol.Name;
                        var lookupIdString = inputCol.IdentificationString;
                        var lokkupExternalId = inputCol.ExternalColumnID;
                        
                        DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfInputColumnUrn(inputNode, inputCol.Name /*, inputCol.ID*/), inputCol.Name,
                            context.DefinitionSearcher.GetDfInputColumnDefinition(inputDefinitionXml, inputCol.IdentificationString), inputNode);

                        colNode.Precision = inputCol.Precision;
                        colNode.Scale = inputCol.Scale;
                        colNode.Length = inputCol.Length;
                        colNode.DtsDataType = inputCol.DataType.ToString();

                        inputNode.AddChild(colNode);

                        lookupInputMapping[inputCol.Name] = colNode;

                        if (!string.IsNullOrEmpty(inputCol.GetPropertyValue("JoinToReferenceColumn")))
                        {
                            string joinToCol = inputCol.GetPropertyValue("JoinToReferenceColumn"); // inputProps.GetString("JoinToReferenceColumn");
                            var lookupColUrn = context.UrnBuilder.GetDfLookupColumnUrn(colNode, inputCol.Name, joinToCol);
                            DfLookupColumnElement lookupColNode = new DfLookupColumnElement(lookupColUrn, "Lookup " + inputCol.Name, string.Empty, colNode);
                            colNode.AddChild(lookupColNode);

                            //// may refer a DB not coverd by the rel. DB graph
                            //if (externalSourceColumnsFromNames.ContainsKey(joinToCol))
                            //{
                            //    //lookupColNode.Links.Add(new BasicNodeLink() { NodeFrom = lookupColNode, NodeTo = externalSourceColumnsFromNames[joinToCol], ValidRevisionFrom = -1, ValidRevisionTo = -1 });
                            //    throw new NotImplementedException();
                            //    //   lookupColNode.ExternalColumn = externalSourceColumnsFromNames[joinToCol];
                            //}

                            lookupJoinColumns.Add(colNode);
                        }

                        // no output matches this input column - send it directly out
                        if (!matchOutput.Columns.Any(x => x.Name == inputCol.Name))
                        {
                            DfColumnElement outputColNode = new DfColumnElement(context.UrnBuilder.GetDfOutputColumnUrn(outputNode, inputCol.Name /*, inputCol.ID*/), inputCol.Name,
                            /*context.DefinitionSearcher.GetDfOutputColumnDefinition(outputDefinitionXml, inputCol.IdentificationString),*/ null, outputNode);

                            outputColNode.Precision = inputCol.Precision;
                            outputColNode.Scale = inputCol.Scale;
                            outputColNode.Length = inputCol.Length;
                            outputColNode.DtsDataType = inputCol.DataType.ToString();

                            outputNode.AddChild(outputColNode);
                            lookupMatchOutputMapping[inputCol.Name] = outputColNode;

                            outputColNode.SourceDfColumn = colNode;
                        }

                        //if (!string.IsNullOrEmpty(inputCol.GetPropertyValue("CopyFromReferenceColumn")))
                        //{
                        //    string copyFromCol = inputCol.GetPropertyValue("CopyFromReferenceColumn"); //inputProps.GetString("CopyFromReferenceColumn");

                        //    // may refer a DB not coverd by the rel. DB graph
                        //    if (externalSourceColumnsFromNames.ContainsKey(copyFromCol))
                        //    {
                        //        //colNode.Links.Add(new BasicNodeLink() { NodeFrom = colNode, NodeTo = externalSourceColumnsFromNames[copyFromCol], ValidRevisionFrom = -1, ValidRevisionTo = -1 });
                        //        colNode.ExternalSourceColumn = externalSourceColumnsFromNames[copyFromCol];
                        //    }
                        //}
                    }
                    
                    foreach (var outputCol in matchOutput.Columns)
                    {
                        DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfOutputColumnUrn(outputNode, outputCol.Name /*, outputCol.ID*/), outputCol.Name, 
                            context.DefinitionSearcher.GetDfOutputColumnDefinition(outputDefinitionXml, outputCol.IdentificationString), outputNode);

                        colNode.Precision = outputCol.Precision;
                        colNode.Scale = outputCol.Scale;
                        colNode.Length = outputCol.Length;
                        colNode.DtsDataType = outputCol.DataType.ToString();

                        outputNode.AddChild(colNode);
                        lookupMatchOutputMapping[outputCol.Name] = colNode;

                        foreach (var joinColumn in lookupJoinColumns)
                        {
                            var refPath = context.UrnBuilder.GetDfLookupOutputJoinToInputReferenceColumnUrn(componentElement, joinColumn, colNode);
                            var joinColOutputRefElement = new DfLookupOutputJoinReferenceElement(refPath, "Lookup impact from " + joinColumn.Caption + " to " + colNode.Caption, string.Empty, componentElement);
                            joinColOutputRefElement.InputJoinColumn = joinColumn;
                            joinColOutputRefElement.OutputColumn = colNode;
                            componentElement.AddChild(joinColOutputRefElement);
                        }

                        string copyFromCol = outputCol.GetPropertyValue("CopyFromReferenceColumn");

                        // may refer a DB not coverd by the rel. DB graph
                        if (externalSourceColumnsFromNames.ContainsKey(copyFromCol))
                        {
                            colNode.ExternalSourceColumn = externalSourceColumnsFromNames[copyFromCol];

                        }
                    }


                }
            }

            return componentElement;
        }
    }
}
