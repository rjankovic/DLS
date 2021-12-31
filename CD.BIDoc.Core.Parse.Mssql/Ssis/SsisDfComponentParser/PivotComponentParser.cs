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
    class PivotComponentParser : SsisDfComponentParserBase, ISsisDfComponentParser
    {
        public int Priority { get { return 10; } }

        public bool CanParse(SsisDfComponent component)
        {
            return component.Contract.Contains("Pivot");
        }

        public DfComponentElement ParseComponent(SsisDfComponentContext context)
        {
            var componentElement = new DfComponentElement(context.ComponentRefPath, context.Component.Name, context.ComponentDefinitionXml.OuterXml, context.DfElement);
            context.DfElement.AddChild(componentElement);

            SsisDfOutput pivotOutput = null;
            foreach (var output in context.Component.Outputs)
            {
                if (!output.IsErrorOutput)
                {
                    pivotOutput = output;
                    break;
                }
            }
            if (pivotOutput == null)
            {
                throw new Exception("Missing pivot output");
            }

            /*create model component*/
            Dictionary<string, DfColumnElement> inputColumnsByLineageId = new Dictionary<string, DfColumnElement>();
            foreach (var input in context.Component.Inputs)
            {
                XmlElement inputDefinitionXml = null;
                DfInputElement inputNode = new DfInputElement(context.UrnBuilder.GetDfInputUrn(componentElement, input.Name),
                        input.Name, context.DefinitionSearcher.GetDfComponentInputDefinition(context.ComponentDefinitionXml,
                        input.RefId, out inputDefinitionXml), componentElement);
                componentElement.AddChild(inputNode);
                inputNode.InputType = DfInputTypeEnum.Input;

                ComponentInput conversionInputMapping = new ComponentInput() { ModelElement = inputNode };

                context.ComponentIO.Inputs[input.RefId] = conversionInputMapping;
                
                foreach (var inputCol in input.Columns)
                {
                    var name = inputCol.Name;
                    var componentIdString = inputCol.IdentificationString;
                    var externalId = inputCol.ExternalColumnID;

                    var inputeColId = inputCol.LineageID;

                    DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfInputColumnUrn(inputNode, inputCol.Name /*, inputCol.ID*/), inputCol.Name,
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
            DfOutputElement outputNode = new DfOutputElement(context.UrnBuilder.GetDfOutputUrn(componentElement, pivotOutput.Name), pivotOutput.Name,
                context.DefinitionSearcher.GetDfComponentOutputDefinition(context.ComponentDefinitionXml, pivotOutput.RefId, out outputDefinitionXml), componentElement);
            componentElement.AddChild(outputNode);
            outputNode.OutputType = pivotOutput.IsErrorOutput ? DfOutputTypeEnum.ErrorOutput : DfOutputTypeEnum.Output;

            ComponentOutput pivotOutputMapping = new ComponentOutput() { ModelElement = outputNode };
            context.ComponentIO.Outputs[pivotOutput.RefId] = pivotOutputMapping;

            foreach (var outputCol in pivotOutput.Columns)
            {
                var inputColId = outputCol.GetPropertyValue("SourceColumn"); //int.Parse(outputCol.GetPropertyValue("SourceColumn"));
                var inputColElement = inputColumnsByLineageId[inputColId];

                DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfOutputColumnUrn(outputNode, outputCol.Name /*, outputCol.ID*/), outputCol.Name,
                    context.DefinitionSearcher.GetDfOutputColumnDefinition(outputDefinitionXml, outputCol.IdentificationString), outputNode);
                colNode.SourceDfColumn = inputColElement;

                outputNode.AddChild(colNode);
                colNode.Precision = outputCol.Precision;
                colNode.Scale = outputCol.Scale;
                colNode.Length = outputCol.Length;
                colNode.DtsDataType = outputCol.DataType.ToString(); 
                pivotOutputMapping[outputCol.Name] = colNode;
            }

            return componentElement;
        }
    }
}