using System;
using System.Collections.Generic;
using System.Xml;
using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.DAL.Objects.Extract;
using CD.BIDoc.Core.Parse.Mssql.Ssis;

namespace CD.DLS.Parse.Mssql.Ssis.SsisDfComponentParser
{
    class AggregateParser : SsisDfComponentParserBase, ISsisDfComponentParser
    {
        public int Priority { get { return 10; } }

        public bool CanParse(SsisDfComponent component)
        {
            return component.ClassId.Contains("Aggregate");
        }

        public DfComponentElement ParseComponent(SsisDfComponentContext context)
        {
            var componentElement = new DfComponentElement(context.ComponentRefPath, context.Component.Name, context.Component.XmlDefinition, context.DfElement);
            context.DfElement.AddChild(componentElement);

            SsisDfOutput aggregateOutput = null;
            foreach (var output in context.Component.Outputs)
            {
                if (!output.IsErrorOutput)
                {
                    aggregateOutput = output;
                    break;
                }
            }
            if (aggregateOutput == null)
            {
                throw new Exception("Missing aggregate output");
            }

            /*create model component*/
            Dictionary<string, DfColumnElement> inputColumnsByLineageId = new Dictionary<string, DfColumnElement>();

            foreach (var input in context.Component.Inputs)
            {
                XmlElement inputDefinitionXml = null;
                DfInputElement inputNode = new DfInputElement(context.UrnBuilder.GetDfInputUrn(componentElement, input.Name),
                        input.Name, input.XmlDefinition 
                        //context.DefinitionSearcher.GetDfComponentInputDefinition(context.ComponentDefinitionXml,
                        //input.RefId, out inputDefinitionXml)
                        , componentElement);
                componentElement.AddChild(inputNode);
                inputNode.InputType = DfInputTypeEnum.Input;

                ComponentInput conversionInputMapping = new ComponentInput() { ModelElement = inputNode };

                context.ComponentIO.Inputs[input.RefId] = conversionInputMapping;

                foreach (var inputCol in input.Columns)
                {
                    var name = inputCol.Name;
                    var componentIdString = inputCol.RefId;//.IdentificationString;
                    var externalId = inputCol.ExternalColumnID;

                    string inputeColId = inputCol.LineageID;

                    DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfInputColumnUrn(inputNode, inputCol.Name), inputCol.Name,
                        //context.DefinitionSearcher.GetDfInputColumnDefinition(inputDefinitionXml, inputCol.RefId /*.IdentificationString*/)
                        inputCol.XmlDefinition
                        , inputNode);

                    colNode.Precision = inputCol.Precision;
                    colNode.Scale = inputCol.Scale;
                    colNode.Length = inputCol.Length;
                    colNode.DtsDataType = inputCol.DataType.ToString();
                    inputNode.AddChild(colNode);

                    conversionInputMapping[inputCol.Name] = colNode;
                    inputColumnsByLineageId[inputCol.LineageID] = colNode;
                }
            }

            //XmlElement outputDefinitionXml = null;
            DfOutputElement outputNode = new DfOutputElement(context.UrnBuilder.GetDfOutputUrn(componentElement, aggregateOutput.Name), aggregateOutput.Name,
                //context.DefinitionSearcher.GetDfComponentOutputDefinition(context.ComponentDefinitionXml, aggregateOutput.RefId, out outputDefinitionXml)
                aggregateOutput.XmlDefinition
                , componentElement);
            componentElement.AddChild(outputNode);
            outputNode.OutputType = aggregateOutput.IsErrorOutput ? DfOutputTypeEnum.ErrorOutput : DfOutputTypeEnum.Output;

            ComponentOutput aggregateMapping = new ComponentOutput() { ModelElement = outputNode };
            context.ComponentIO.Outputs[aggregateOutput.RefId] = aggregateMapping;

            foreach (var outputCol in aggregateOutput.Columns)
            {
                string inputColId = outputCol.GetPropertyValue("AggregationColumnId");
                
                DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfOutputColumnUrn(outputNode, outputCol.Name /*, outputCol.ID*/), outputCol.Name,
                    //context.DefinitionSearcher.GetDfOutputColumnDefinition(outputDefinitionXml, outputCol.RefId /* .IdentificationString*/)
                    outputCol.XmlDefinition
                    , outputNode);

                if (inputColId != null && inputColumnsByLineageId.ContainsKey(inputColId))
                {
                    var inputColElement = inputColumnsByLineageId[inputColId];
                    colNode.SourceDfColumn = inputColElement;
                }
                else
                {

                }

                outputNode.AddChild(colNode);
                colNode.Precision = outputCol.Precision;
                colNode.Scale = outputCol.Scale;
                colNode.Length = outputCol.Length;
                colNode.DtsDataType = outputCol.DataType.ToString();
                aggregateMapping[outputCol.Name] = colNode;
            }

            return componentElement;
        }
    }
}