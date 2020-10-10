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

namespace CD.DLS.Parse.Mssql.Ssis.SsisDfComponentParser
{
    class AggregateParser : SsisDfComponentParserBase, ISsisDfComponentParser
    {
        public int Priority { get { return 10; } }

        public bool CanParse(SsisDfComponent component)
        {
            return component.ContractBase == "Aggregate";
        }

        public DfComponentElement ParseComponent(SsisDfComponentContext context)
        {
            var componentElement = new DfComponentElement(context.ComponentRefPath, context.Component.Name, context.ComponentDefinitionXml.OuterXml, context.DfElement);
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
            Dictionary<int, DfColumnElement> inputColumnsByLineageId = new Dictionary<int, DfColumnElement>();

            foreach (var input in context.Component.Inputs)
            {
                XmlElement inputDefinitionXml = null;
                DfInputElement inputNode = new DfInputElement(context.UrnBuilder.GetDfInputUrn(componentElement, input.Name),
                        input.Name, context.DefinitionSearcher.GetDfComponentInputDefinition(context.ComponentDefinitionXml,
                        input.IdString, out inputDefinitionXml), componentElement);
                componentElement.AddChild(inputNode);
                inputNode.InputType = DfInputTypeEnum.Input;

                ComponentInput conversionInputMapping = new ComponentInput() { ModelElement = inputNode };

                context.ComponentIO.Inputs[input.IdString] = conversionInputMapping;

                foreach (var inputCol in input.Columns)
                {
                    var name = inputCol.Name;
                    var componentIdString = inputCol.IdentificationString;
                    var externalId = inputCol.ExternalColumnID;

                    int inputeColId = inputCol.LineageID;

                    DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfInputColumnUrn(inputNode, inputCol.Name, inputCol.ID), inputCol.Name,
                        context.DefinitionSearcher.GetDfInputColumnDefinition(inputDefinitionXml, inputCol.IdentificationString), inputNode);

                    colNode.Precision = inputCol.Precision;
                    colNode.Scale = inputCol.Scale;
                    colNode.Length = inputCol.Length;
                    colNode.DtsDataType = inputCol.DataType.ToString();
                    inputNode.AddChild(colNode);

                    conversionInputMapping[inputCol.Name] = colNode;
                    inputColumnsByLineageId[inputCol.LineageID] = colNode;
                }
            }

            XmlElement outputDefinitionXml = null;
            DfOutputElement outputNode = new DfOutputElement(context.UrnBuilder.GetDfOutputUrn(componentElement, aggregateOutput.Name), aggregateOutput.Name,
                context.DefinitionSearcher.GetDfComponentOutputDefinition(context.ComponentDefinitionXml, aggregateOutput.IdString, out outputDefinitionXml), componentElement);
            componentElement.AddChild(outputNode);
            outputNode.OutputType = aggregateOutput.IsErrorOutput ? DfOutputTypeEnum.ErrorOutput : DfOutputTypeEnum.Output;

            ComponentOutput aggregateMapping = new ComponentOutput() { ModelElement = outputNode };
            context.ComponentIO.Outputs[aggregateOutput.IdString] = aggregateMapping;

            foreach (var outputCol in aggregateOutput.Columns)
            {
                int inputColId = int.Parse(outputCol.GetPropertyValue("AggregationColumnId"));
                
                DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfOutputColumnUrn(outputNode, outputCol.Name, outputCol.ID), outputCol.Name,
                    context.DefinitionSearcher.GetDfOutputColumnDefinition(outputDefinitionXml, outputCol.IdentificationString), outputNode);

                if (inputColumnsByLineageId.ContainsKey(inputColId))
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