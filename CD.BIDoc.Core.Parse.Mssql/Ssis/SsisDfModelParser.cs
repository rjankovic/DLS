using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.Parse.Mssql.Ssis.SsisDfComponentParser;
using CD.DLS.DAL.Objects.Extract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CD.DLS.DAL.Configuration;
using System.Reflection;
using CD.DLS.Parse.Mssql.Db;

namespace CD.DLS.Parse.Mssql.Ssis
{
    /// <summary>
    /// Extracts model for SSIS data flows.
    /// </summary>
    class SsisDfModelParser
    {
        private readonly UrnBuilder _urnBuilder = new UrnBuilder();
        private readonly SsisXmlProvider _definitionSearcher;
        //private readonly Db.IReferrableIndex _dbIndex;
        private readonly AvailableDatabaseModelIndex _availableDatabaseModelIndex;
        private readonly Db.ISqlScriptModelParser _sqlScriptExtractor;
        private List<ISsisDfComponentParser> _componentParsers;

        public SsisDfModelParser(SsisXmlProvider definitionSearcher, AvailableDatabaseModelIndex availableDatabaseModelIndex, Db.ISqlScriptModelParser sqlScriptExtractor)
        {
            this._definitionSearcher = definitionSearcher;
            //this._dbIndex = dbIndex;
            _availableDatabaseModelIndex = availableDatabaseModelIndex;
            this._sqlScriptExtractor = sqlScriptExtractor;
            
            var componentParserType = typeof(ISsisDfComponentParser);
            var parserClasses = 
                //AppDomain.CurrentDomain.GetAssemblies()
                //.SelectMany(s => s.GetTypes())
                Assembly.GetExecutingAssembly().GetTypes()
                .Where(p => componentParserType.IsAssignableFrom(p) && p.IsClass)
                .ToList();

            _componentParsers = new List<ISsisDfComponentParser>();
            foreach (var parserClass in parserClasses)
            {
                ISsisDfComponentParser instance = (ISsisDfComponentParser)Activator.CreateInstance(parserClass);
                _componentParsers.Add(instance);
            }
        }

        /// <summary>
        /// Extracts data flow model for an executable element
        /// </summary>
        public void ParseDataFlow(List<SsisDfComponent> components,
            List<SsisDfPath> paths,
            ExecutableElement parent,
            XmlElement outerTaskDefinitionElement,
            string parentLayoutRefPath,
            SsisPackage package,
            SsisIndex referrables,
            ConnectionIndex connections,
            Dictionary<string, Db.TableSourceColumnList> tempTablesAvailable)
        {
            XmlElement flowXml;
            var dfDefinition = _definitionSearcher.GetDfInnerDefinition(outerTaskDefinitionElement, out flowXml);
            var dfUrn = _urnBuilder.GetDfInnerUrn(parent);
            DfInnerElement dfElement = new DfInnerElement(dfUrn, parent.Caption /*+ " Main pipe"*/, dfDefinition, parent);
            dfElement.Position = new DesignPoint() { X = 0, Y = 0 };

            parent.AddChild(dfElement);

            Dictionary<string, DfColumnElement> colIdStringsToNodes = new Dictionary<string, DfColumnElement>();
            //Dictionary<DfComponentElement, IDTSComponentMetaData100> componentNodesToComponents = new Dictionary<DfComponentElement, IDTSComponentMetaData100>();

            IOPerComponent cummulativeIO = new IOPerComponent();

            foreach (SsisDfComponent component in components)
            {
                var fullName = component.ObjectType; // .GetType().FullName;

                var allIO = new ComponentAllIO();
                cummulativeIO[component.IdString] = allIO;

                var componentInputMapping = allIO.Inputs;
                var componentOutputMapping = allIO.Outputs;

                var contract = component.Contract;
                var contractBase = contract.Split(';')[0];
                var objType = component.ObjectType;
                var objTypeString = component.ObjectType.ToString();
                var idString = component.IdString;
                var classId = component.ClassId;
                var properties = component.Properties;
                //PropertyCollection props = new PropertyCollection(properties);
                var componentId = component.ID;
                var layout = _definitionSearcher.GetContainerDesign(package.Container.ID, parentLayoutRefPath + "\\" + idString);
                var isSource = contractBase.EndsWith("Source");

                var componentUrn = isSource ? _urnBuilder.GetDfSourceComponentUrn(dfElement, component.IdString) : _urnBuilder.GetDfComponentUrn(dfElement, component.IdString);
                XmlElement componentDefinitionXml;
                var componentDefinition = _definitionSearcher.GetDfComponentDefinition(flowXml, parentLayoutRefPath, component.IdString, out componentDefinitionXml);

                DfComponentElement componentElement = null;

                SsisDfComponentContext context = new SsisDfComponentContext(
                    referrables
                    , connections
                    , tempTablesAvailable
                    , component
                    , componentDefinitionXml
                    , allIO
                    , dfElement
                    , _urnBuilder
                    , _definitionSearcher
                    , _availableDatabaseModelIndex
                    , _sqlScriptExtractor
                    , componentUrn
                    );

                foreach (var parser in _componentParsers.OrderByDescending(x => x.Priority))
                {
                    if (parser.CanParse(component))
                    {
                        componentElement = parser.ParseComponent(context);
                        break;
                    }
                }
                if (componentElement == null)
                {
                    throw new Exception("Could not find a parser for component " + component.Contract);
                }

                var cName = component.Name;

                foreach (var input in component.Inputs)
                {
                    DfInputElement inputNode = null;
                    XmlElement inputDefinitionXml = null;
                    var inputDefinition = _definitionSearcher.GetDfComponentInputDefinition(componentDefinitionXml, input.IdString, out inputDefinitionXml);


                    if (!componentInputMapping.ContainsKey(input.IdString))
                    {
                        inputNode = new DfInputElement(_urnBuilder.GetDfInputUrn(componentElement, input.Name), input.Name, inputDefinition, componentElement);
                        componentElement.AddChild(inputNode);
                        inputNode.InputType = DfInputTypeEnum.Input;
                        ComponentInput colCollection = new ComponentInput() { ModelElement = inputNode };
                        componentInputMapping[input.IdString] = colCollection;
                    }

                    var inputMapping = componentInputMapping[input.IdString];
                    inputNode = inputMapping.ModelElement;

                    foreach (var inputCol in input.Columns)
                    {
                        var inputColUrn = _urnBuilder.GetDfInputColumnUrn(inputNode, inputCol.Name, inputCol.ID);
                        var inputColDef = _definitionSearcher.GetDfInputColumnDefinition(inputDefinitionXml, inputCol.IdentificationString);
                        var inputColNode = new DfColumnElement(inputColUrn, inputCol.Name, inputColDef, inputNode);
                        
                        if (inputMapping.Dictionary.ContainsKey(inputCol.Name))
                        {
                            var col = inputMapping.Dictionary[inputCol.Name];
                            col.DtsDataType = inputCol.DataType.ToString();
                            SetDataTypeLength(col);
                            continue;
                        }
                        inputColNode.DtsDataType = inputCol.DataType.ToString();
                        inputColNode.Precision = inputCol.Precision;
                        inputColNode.Scale = inputCol.Scale;                       
                        inputColNode.Length = inputCol.Length;
                        SetDataTypeLength(inputColNode);

                        inputNode.AddChild(inputColNode);
                        inputMapping[inputCol.Name] = inputColNode;

                    }
                }

                foreach (var output in component.Outputs)
                {
                    DfOutputElement outputNode = null;
                    XmlElement outputDefinitionXml = null;
                    var outputDefinition = _definitionSearcher.GetDfComponentOutputDefinition(componentDefinitionXml, output.IdString, out outputDefinitionXml);

                    if (!componentOutputMapping.ContainsKey(output.IdString))
                    {
                        outputNode = new DfOutputElement(_urnBuilder.GetDfOutputUrn(componentElement, output.Name), output.Name, outputDefinition, componentElement);
                        outputNode.OutputType = output.IsErrorOutput ? DfOutputTypeEnum.ErrorOutput : DfOutputTypeEnum.Output;

                        componentElement.AddChild(outputNode);
                        var colCollection = new ComponentOutput() { ModelElement = outputNode };
                        componentOutputMapping[output.IdString] = colCollection;

                    }

                    var outputMapping = componentOutputMapping[output.IdString];
                    outputNode = componentOutputMapping[output.IdString].ModelElement;

                    foreach (var outputCol in output.Columns)
                    {
                        var outputColUrn = _urnBuilder.GetDfOutputColumnUrn(outputNode, outputCol.Name, outputCol.ID);
                        var outputColDef = _definitionSearcher.GetDfOutputColumnDefinition(outputDefinitionXml, outputCol.IdentificationString);
                        var outputColNode = new DfColumnElement(outputColUrn, outputCol.Name, outputColDef, outputNode);
                        
                        if (outputMapping.Dictionary.ContainsKey(outputCol.Name))
                        {
                            var col = outputMapping.Dictionary[outputCol.Name];
                            col.DtsDataType = outputCol.DataType.ToString();
                            SetDataTypeLength(col);
                            continue;
                        }

                        outputColNode.DtsDataType = outputCol.DataType.ToString();
                        outputColNode.Precision = outputCol.Precision;
                        outputColNode.Scale = outputCol.Scale; 
                        outputColNode.Length = outputCol.Length;
                        SetDataTypeLength(outputColNode);

                        outputNode.AddChild(outputColNode);
                        outputMapping[outputCol.Name] = outputColNode;
                    }
                }

                componentElement.Position = layout.TopLeft;
                componentElement.Size = layout.Size;

                cummulativeIO[component.IdString].ModelElement = componentElement;
            } // end foreach component

            // add DF path in topological order

            if (dfElement.ChildBlocks.Any())
            {
                var width = dfElement.ChildBlocks.Max(x => x.Position.X + x.Size.X);
                var height = dfElement.ChildBlocks.Max(x => x.Position.Y + x.Size.Y);
                dfElement.Size = new DesignPoint() { X = width, Y = height };
            }
            else
            {
                dfElement.Size = new DesignPoint() { X = 0, Y = 0 };
            }
            var topolPrecedences = new Dictionary<string, List<string>>();
            var topolSuccessors = new Dictionary<string, List<string>>();
            var outboundPaths = new Dictionary<string, List<SsisDfPath>>();
            List<string> departures = new List<string>();
            //var topolOrder = new List<string>();
            foreach (var component in components)
            {
                topolPrecedences.Add(component.IdString, new List<string>());
                outboundPaths[component.IdString] = new List<SsisDfPath>();
                topolSuccessors[component.IdString] = new List<string>();
            }
            foreach (var path in paths)
            {
                topolPrecedences[path.TargetComponentIdString].Add(path.SourceComponentIdString);
                topolSuccessors[path.SourceComponentIdString].Add(path.TargetComponentIdString);
                outboundPaths[path.SourceComponentIdString].Add(path);
            }

            while (topolPrecedences.Any())
            {
                departures.Clear();
                if (topolPrecedences.Keys.All(x => topolPrecedences[x].Count > 0))
                {
                    throw new Exception("!Looop");
                }
                foreach (var independentComponentId in topolPrecedences.Keys.Where(x => topolPrecedences[x].Count == 0))
                {
                    // link outputs to the next component
                    var srcOutputs = cummulativeIO[independentComponentId].Outputs;
                    foreach (var path in outboundPaths[independentComponentId])
                    {
                        var srcOutput = srcOutputs[path.SourceIdString];
                        var tgtComponent = cummulativeIO[path.TargetComponentIdString];
                        var tgtInput = tgtComponent.Inputs[path.TargetIdString];
                        foreach (var inputColumnName in tgtInput.Dictionary.Keys)
                        {
                            if (!srcOutput.Dictionary.ContainsKey(inputColumnName))
                            {
                                ConfigManager.Log.Warning(string.Format("Unresolved input column in {0}: {1}, looking throuh lineageId", tgtComponent.ModelElement.RefPath, inputColumnName));
                                try
                                {
                                    XmlDocument xDoc = new XmlDocument();
                                    xDoc.LoadXml(tgtInput.Dictionary[inputColumnName].Definition);
                                    var lineageId = ((XmlElement)xDoc.FirstChild).GetAttributeNode("lineageId").InnerXml;
                                    var lineageColumnName = lineageId.Substring(lineageId.LastIndexOf(".Columns[") + ".Columns[".Length).TrimEnd(']');
                                    if (!srcOutput.Dictionary.ContainsKey(lineageColumnName))
                                    {
                                        ConfigManager.Log.Error(string.Format("Could not resolve through lineageId: {0}, {1}", lineageId, lineageColumnName));
                                    }
                                    else
                                    {
                                        var srcOutputColumn1 = srcOutput[lineageColumnName];
                                        var tgtColumn1 = tgtInput[inputColumnName];
                                        //tgtColumn.
                                        if (tgtColumn1.SourceDfColumn == null)
                                        {
                                            tgtColumn1.SourceDfColumn = srcOutputColumn1;
                                        }
                                    }
                                     
                                }
                                catch (Exception ex)
                                {
                                    ConfigManager.Log.Error(string.Format("Failed to resolved through lineage id: {0} \n {1}", ex.Message, ex.StackTrace));
                                }
                                continue;
                            }
                            var srcOutputColumn = srcOutput[inputColumnName];
                            var tgtColumn = tgtInput[inputColumnName];
                            //tgtColumn.
                            if (tgtColumn.SourceDfColumn == null)
                            {
                                tgtColumn.SourceDfColumn = srcOutputColumn;
                            }
                        }

                        // create path node
                        string fullLayoutPath;
                        var x_startIdString = path.SourceIdString;
                        var x_endIdString = path.TargetIdString;

                        var pathDef = _definitionSearcher.GetDfPathDefinition(flowXml, x_startIdString, x_endIdString, out fullLayoutPath);
                        var pathUrn = _urnBuilder.GetDfPathUrn(dfElement, path.IdString);
                        DesignArrow pathArrow;
                        pathArrow = _definitionSearcher.GetDfPathArrow(package.Container.ID, fullLayoutPath);

                        DfPathElement pathNode = new DfPathElement(pathUrn, path.Name, pathDef, dfElement);
                        dfElement.AddChild(pathNode);
                        pathNode.Arrow = pathArrow;
                        pathNode.From = srcOutput.ModelElement;
                        pathNode.To = tgtInput.ModelElement;

                        // cummulate outputs
                        foreach (var tgtOutput in tgtComponent.Outputs.Values)
                        {
                            foreach (var sourceColumnName in srcOutput.Dictionary.Keys)
                            {
                                // this column is not redefined in the component
                                if (!tgtOutput.Dictionary.Values.Any(x => x.Caption == srcOutput[sourceColumnName].Caption))
                                {
                                    tgtOutput[sourceColumnName] = srcOutput[sourceColumnName];
                                }
                            }
                        }
                    }
                    departures.Add(independentComponentId);
                }

                foreach (var departure in departures)
                {
                    // remove source from topology
                    foreach (var successor in topolSuccessors[departure])
                    {
                        topolPrecedences[successor].Remove(departure);
                    }
                    topolPrecedences.Remove(departure);
                }
            }

        }
        private void SetDataTypeLength(DfColumnElement dfColumnElement)
        {
            if (dfColumnElement.DtsDataType == "DT_I1" || dfColumnElement.DtsDataType == "DT_UI1" || dfColumnElement.DtsDataType == "DT_BOOL")
            {
                dfColumnElement.Length = 1;
            }
            else if (dfColumnElement.DtsDataType == "DT_I2" || dfColumnElement.DtsDataType == "DT_UI2")
            {
                dfColumnElement.Length = 2;
            }
            else if (dfColumnElement.DtsDataType == "DT_I4" || dfColumnElement.DtsDataType == "DT_UI4" || dfColumnElement.DtsDataType == "DT_R4")
            {
                dfColumnElement.Length = 4;
            }
            else if (dfColumnElement.DtsDataType == "DT_I8" || dfColumnElement.DtsDataType == "DT_UI8" || dfColumnElement.DtsDataType == "DT_CY" || dfColumnElement.DtsDataType == "DT_R8")
            {
                dfColumnElement.Length = 8;
            }
        }

        public string GetPropertyValueOrVariable(SsisDfComponent component, string key, SsisIndex referrables)
        {
            string value = component.GetPropertyValue(key);
            
            //string testValue;
            string accessMode = "";
            accessMode = component.GetPropertyValue("AccessMode");
            //if (!accessModeSpecified)
            //{
            //    accessMode = "";
            //}
            if (string.IsNullOrEmpty(accessMode))
            {
                accessMode = "";
            }
            
            if ((string.IsNullOrEmpty(component.GetPropertyValue(key)) //!properties.TryGetNonEmptyString(key, out value) 
                || !string.IsNullOrEmpty (component.GetPropertyValue(key + "Variable")) && (accessMode == "2" || accessMode == "4")))
            {
                var variableName = component.GetPropertyValue(key + "Variable"); // properties.GetString(key + "Variable");
                value = referrables.GetValueByName(variableName);
            }

            return value;
        }

    }
}
