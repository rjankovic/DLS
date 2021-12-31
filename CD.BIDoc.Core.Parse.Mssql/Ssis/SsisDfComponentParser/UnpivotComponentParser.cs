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
    class UnpivotComponentParser : SsisDfComponentParserBase, ISsisDfComponentParser
    {
        public int Priority { get { return 10; } }

        public bool CanParse(SsisDfComponent component)
        {
            return component.Contract.Contains("Unpivot");
        }

        public DfComponentElement ParseComponent(SsisDfComponentContext context)
        {
            var componentElement = new DfUnpivotElement(context.ComponentRefPath, context.Component.Name, context.ComponentDefinitionXml.OuterXml, context.DfElement);
            context.DfElement.AddChild(componentElement);
            
            SsisDfOutput unpivotOutput = null;
            foreach (var output in context.Component.Outputs)
            {
                if (!output.IsErrorOutput)
                {
                    unpivotOutput = output;
                    break;
                }
            }
            if (unpivotOutput == null)
            {
                throw new Exception("Missing unpivot output");
            }

            /*create model component*/
            XmlElement outputDefinitionXml = null;
            DfOutputElement outputNode = new DfOutputElement(context.UrnBuilder.GetDfOutputUrn(componentElement, unpivotOutput.Name), unpivotOutput.Name,
                context.DefinitionSearcher.GetDfComponentOutputDefinition(context.ComponentDefinitionXml, unpivotOutput.RefId, out outputDefinitionXml), componentElement);
            componentElement.AddChild(outputNode);
            outputNode.OutputType = unpivotOutput.IsErrorOutput ? DfOutputTypeEnum.ErrorOutput : DfOutputTypeEnum.Output;

            ComponentOutput unpivotOutputMapping = new ComponentOutput() { ModelElement = outputNode };
            context.ComponentIO.Outputs[unpivotOutput.RefId] = unpivotOutputMapping;

            /*different for each component, creating cild of elements, use property*/
            Dictionary<string, DfColumnElement> outputColsByLineageId = new Dictionary<string, DfColumnElement>();
            Dictionary<string, DfColumnElement> outputColsByName = new Dictionary<string, DfColumnElement>();


            DfColumnElement pivotKeyColumn = null;

            foreach (var outputCol in unpivotOutput.Columns)
            {
                DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfOutputColumnUrn(outputNode, outputCol.Name), outputCol.Name,
                    context.DefinitionSearcher.GetDfOutputColumnDefinition(outputDefinitionXml, outputCol.IdentificationString), outputNode);

                colNode.Precision = outputCol.Precision;
                colNode.Scale = outputCol.Scale;
                colNode.Length = outputCol.Length;
                colNode.DtsDataType = outputCol.DataType.ToString();

                outputNode.AddChild(colNode);
                unpivotOutputMapping[outputCol.Name] = colNode;


                var outputColId = outputCol.LineageID;               
                outputColsByLineageId.Add(outputColId, colNode);

                var outputColName = outputCol.Name;
                outputColsByName.Add(outputColName, colNode);

                var pivotKey = outputCol.GetPropertyValue("PivotKey");
                if (pivotKey != null)
                {
                    var pivotKeyBoolean = bool.Parse(pivotKey);
                    if (pivotKeyBoolean)
                    {
                        pivotKeyColumn = colNode;
                    }
                }
            }
            foreach (var input in context.Component.Inputs)
            {
                XmlElement inputDefinitionXml = null;
                DfInputElement inputNode = new DfInputElement(context.UrnBuilder.GetDfInputUrn(componentElement, input.Name),
                    input.Name, context.DefinitionSearcher.GetDfComponentInputDefinition(context.ComponentDefinitionXml,
                    input.RefId, out inputDefinitionXml), componentElement);
                componentElement.AddChild(inputNode);

                inputNode.InputType = DfInputTypeEnum.Input;

                ComponentInput unpivotInputMapping = new ComponentInput() { ModelElement = inputNode };
                context.ComponentIO.Inputs[input.RefId] = unpivotInputMapping;

                Dictionary<int, DfColumnElement> inputColumnsByLineageId = new Dictionary<int, DfColumnElement>();


                foreach (var inputCol in input.Columns)
                {
                    var name = inputCol.Name;
                    var componentIdString = inputCol.IdentificationString;
                    var externalId = inputCol.ExternalColumnID;
                    string outputColId = inputCol.GetPropertyValue("DestinationColumn");

                    //if (outputColId == -1)
                    //{
                    //    continue;
                    //}

                    //if (!outputColsByLineageId.ContainsKey(outputColId))
                    //{
                    //    continue;
                    //}

                    var outputColElement = outputColsByLineageId[outputColId];

                    var pivotKeyValue = inputCol.GetPropertyValue("PivotKeyValue");
                    
                    if (pivotKeyValue != null)
                    {
                        DfUnpivotSourceReferenceElement colNode = new DfUnpivotSourceReferenceElement(context.UrnBuilder.GetDfInputColumnUrn(inputNode, inputCol.Name /*, inputCol.ID*/), inputCol.Name,
                        context.DefinitionSearcher.GetDfInputColumnDefinition(inputDefinitionXml, inputCol.IdentificationString), inputNode);
                        outputColElement.SourceDfColumn = colNode;

                        inputNode.AddChild(colNode);
                        colNode.Precision = inputCol.Precision;
                        colNode.Scale = inputCol.Scale;
                        colNode.Length = inputCol.Length;
                        colNode.DtsDataType = inputCol.DataType.ToString();

                        colNode.TargetPivotKeyColumn = pivotKeyColumn;
                        colNode.TargetValueColumn = outputColElement;
                        unpivotInputMapping[inputCol.Name] = colNode;
                    }
                    else
                    {
                        DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfInputColumnUrn(inputNode, inputCol.Name), inputCol.Name,
                        context.DefinitionSearcher.GetDfInputColumnDefinition(inputDefinitionXml, inputCol.IdentificationString), inputNode);
                        outputColElement.SourceDfColumn = colNode;

                        inputNode.AddChild(colNode);
                        colNode.Precision = inputCol.Precision;
                        colNode.Scale = inputCol.Scale;
                        colNode.Length = inputCol.Length;
                        colNode.DtsDataType = inputCol.DataType.ToString();
                        unpivotInputMapping[inputCol.Name] = colNode;
                    }

                }

            }
            return componentElement;
        }
    }
}