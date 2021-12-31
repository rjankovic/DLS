using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Parse.Mssql.Ssis;
using System.Text.RegularExpressions;
using CD.DLS.DAL.Configuration;
using CD.BIDoc.Core.Parse.Mssql.Ssis;

namespace CD.DLS.Parse.Mssql.Ssis.SsisDfComponentParser
{
    class DerivedColumnComponentParser : SsisDfComponentParserBase, ISsisDfComponentParser
    {
        public int Priority { get { return 10; } }

        public bool CanParse(SsisDfComponent component)
        {
            return component.Contract.Contains("Derived Column");
        }

        public DfComponentElement ParseComponent(SsisDfComponentContext context)
        {
            var componentElement = new DfDerivedColumnElement(context.ComponentRefPath, context.Component.Name, context.ComponentDefinitionXml.OuterXml, context.DfElement);
            context.DfElement.AddChild(componentElement);

            Dictionary<string, DfColumnElement> inputColumnsByName = new Dictionary<string, DfColumnElement>();
            Dictionary<string, DfColumnElement> inputColumnsByLineageId = new Dictionary<string, DfColumnElement>();

            //create local SsisIndex which will later be used for expression extractor
            SsisIndex localIndex = new SsisIndex();

            foreach (var input in context.Component.Inputs)
            {

                XmlElement inputDefinitionXml = null;
                DfInputElement inputNode = new DfInputElement(context.UrnBuilder.GetDfInputUrn(componentElement, input.Name), input.Name,
                    context.DefinitionSearcher.GetDfComponentInputDefinition(context.ComponentDefinitionXml, input.RefId, out inputDefinitionXml), componentElement);
                componentElement.AddChild(inputNode);

                ComponentInput derivedColumnInputMapping = new ComponentInput() { ModelElement = inputNode };
                inputNode.InputType = DfInputTypeEnum.Input;

                context.ComponentIO.Inputs[input.RefId] = derivedColumnInputMapping;


                foreach (var inputCol in input.Columns)
                {
                    var name = inputCol.Name;
                    var componentIdString = inputCol.IdentificationString;
                    var externalId = inputCol.ExternalColumnID;
                    var lineageID = inputCol.LineageID;
                    //DfColumnElement outputColElement = null;

                    DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfInputColumnUrn(inputNode, inputCol.Name /*, inputCol.ID*/),
                        inputCol.Name, context.DefinitionSearcher.GetDfInputColumnDefinition(inputDefinitionXml, inputCol.IdentificationString), inputNode);

                    colNode.Precision = inputCol.Precision;
                    colNode.Scale = inputCol.Scale;
                    colNode.Length = inputCol.Length;
                    colNode.DtsDataType = inputCol.DataType.ToString();

                    inputNode.AddChild(colNode);

                    derivedColumnInputMapping[inputCol.Name] = colNode;
                    inputColumnsByName[inputCol.Name] = colNode;
                    inputColumnsByLineageId[lineageID] = colNode;

                    //add input column nodes into local SsisIndex
                    localIndex.AddColumn(lineageID, colNode);
                }
            }



            SsisDfOutput derivedColumnOutput = null;
            foreach (var output in context.Component.Outputs)
            {
                if (!output.IsErrorOutput)
                {
                    derivedColumnOutput = output;
                    break;
                }
            }
            if (derivedColumnOutput == null)
            {
                throw new Exception("Missing derived column output");
            }


            XmlElement outputDefinitionXml = null;
            DfOutputElement outputNode = new DfOutputElement(context.UrnBuilder.GetDfOutputUrn(componentElement, derivedColumnOutput.Name), derivedColumnOutput.Name,
                context.DefinitionSearcher.GetDfComponentOutputDefinition(context.ComponentDefinitionXml, derivedColumnOutput.RefId, out outputDefinitionXml), componentElement);
            componentElement.AddChild(outputNode);
            outputNode.OutputType = derivedColumnOutput.IsErrorOutput ? DfOutputTypeEnum.ErrorOutput : DfOutputTypeEnum.Output;

            ComponentOutput derivedColumnOutputMapping = new ComponentOutput() { ModelElement = outputNode };
            context.ComponentIO.Outputs[derivedColumnOutput.RefId] = derivedColumnOutputMapping;

            Dictionary<int, DfColumnElement> outputColsById = new Dictionary<int, DfColumnElement>();
            Dictionary<String, SsisModelElement> expressionModelsByOutputColsLineageId = new Dictionary<String, SsisModelElement>();

            ExpressionModelExtractor extractor = new ExpressionModelExtractor(context.UrnBuilder);

            foreach (var outputCol in derivedColumnOutput.Columns)
            {
                DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfOutputColumnUrn(outputNode, outputCol.Name /*, outputCol.ID*/), outputCol.Name,
                    context.DefinitionSearcher.GetDfOutputColumnDefinition(outputDefinitionXml, outputCol.IdentificationString), outputNode);

                colNode.Precision = outputCol.Precision;
                colNode.Scale = outputCol.Scale;
                colNode.Length = outputCol.Length;
                colNode.DtsDataType = outputCol.DataType.ToString();

               // var lineageID = outputCol.GetPropertyValue("OutputColumnLineageID");

                String expression = outputCol.GetPropertyValue("Expression");

                outputNode.AddChild(colNode);
                derivedColumnOutputMapping[outputCol.Name] = colNode;

                SsisModelElement expressionModel = extractor.ExtractExpressionModel(expression, localIndex, colNode);

                int index = 1;
                //check every fragment in expressionModel against input columns, if fragment lineageId is the same as inputColumn lineageId, create an Aggreagation link element
                foreach (SsisExpressionFragmentElement fragment in expressionModel.Children)
                {
                    String lineageIdstr = fragment.Definition;
                    if (lineageIdstr.StartsWith("#"))
                    {
                        String lineageIdCut = lineageIdstr.Substring(1, lineageIdstr.Length - 1);
                        var lineageId = lineageIdCut; // /*str*/);

                        if (inputColumnsByLineageId.Keys.Contains(lineageId))
                        {
                            //ConfigManager.Log.Info(string.Format("{0} in {1} refers to {2}", lineageId, expression, inputColumnsByLineageId[lineageId].RefPath.Path));
                            DfColumnAggregationLinkElement linkElement = new DfColumnAggregationLinkElement(context.UrnBuilder.DfColumnAggregationLinkElement(colNode, outputCol.Name, inputColumnsByLineageId[lineageId].Caption, lineageIdstr, index++), 
                                outputCol.Name + "_" + inputColumnsByLineageId[lineageId].Caption,
                                    null, componentElement)
                            {
                                SourceDfColumn = inputColumnsByLineageId[lineageId],
                                TargetDfColumn = colNode
                            };
                            componentElement.AddChild(linkElement);
                        }
                    }
                }
            }
            return componentElement;
        }
    }
}

