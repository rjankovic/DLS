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
    class DataConversionDfComponentParser : SsisDfComponentParserBase, ISsisDfComponentParser
    {
        public int Priority { get { return 10; } }

        public bool CanParse(SsisDfComponent component)
        {
            return component.Contract.Contains("Data Conversion");
        }

        public DfComponentElement ParseComponent(SsisDfComponentContext context)
        {
            var componentElement = new DfDataConversionElement(context.ComponentRefPath, context.Component.Name, context.Component.XmlDefinition, context.DfElement);
            context.DfElement.AddChild(componentElement);
            
            var conversionInput = context.Component.Inputs[0];
            XmlElement inputDefinitionXml = null;
            DfInputElement inputNode = new DfInputElement(context.UrnBuilder.GetDfInputUrn(componentElement, conversionInput.Name), conversionInput.Name,
                //context.DefinitionSearcher.GetDfComponentInputDefinition(context.ComponentDefinitionXml, conversionInput.RefId, out inputDefinitionXml)
                conversionInput.XmlDefinition
                , componentElement);
            componentElement.AddChild(inputNode);

            inputNode.InputType = DfInputTypeEnum.Input;

            ComponentInput conversionInputMapping = new ComponentInput() { ModelElement = inputNode };

            context.ComponentIO.Inputs[conversionInput.RefId] = conversionInputMapping;
            Dictionary<string, DfColumnElement> inputColumnsByLineageId = new Dictionary<string, DfColumnElement>();
            foreach (var inputCol in conversionInput.Columns)
            {
                var name = inputCol.Name;
                var componentIdString = inputCol.RefId;
                var externalId = inputCol.ExternalColumnID;
                
                DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfInputColumnUrn(inputNode, inputCol.Name /*, inputCol.ID*/),
                    inputCol.Name, 
                    //context.DefinitionSearcher.GetDfInputColumnDefinition(inputDefinitionXml, inputCol.RefId)
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

            SsisDfOutput conversionOutput = null;
            foreach (var output in context.Component.Outputs)
            {
                if (output.Name == "Data Conversion Output" && !output.IsErrorOutput)
                {
                    conversionOutput = output;
                }
            }
            if (conversionOutput == null)
            {
                throw new Exception("Missing data conversion output");
            }

            XmlElement outputDefinitionXml = null;
            DfOutputElement outputNode = new DfOutputElement(context.UrnBuilder.GetDfOutputUrn(componentElement, conversionOutput.Name), conversionOutput.Name,
                //context.DefinitionSearcher.GetDfComponentOutputDefinition(context.ComponentDefinitionXml, conversionOutput.RefId, out outputDefinitionXml)
                conversionOutput.XmlDefinition
                , componentElement);
            componentElement.AddChild(outputNode);
            outputNode.OutputType = conversionOutput.IsErrorOutput ? DfOutputTypeEnum.ErrorOutput : DfOutputTypeEnum.Output;

            ComponentOutput conversionOutputMapping = new ComponentOutput() { ModelElement = outputNode };
            context.ComponentIO.Outputs[conversionOutput.RefId] = conversionOutputMapping;

            foreach (var outputCol in conversionOutput.Columns)
            {
                DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfOutputColumnUrn(outputNode, outputCol.Name/*, outputCol.ID*/),
                    outputCol.Name, 
                    //context.DefinitionSearcher.GetDfOutputColumnDefinition(outputDefinitionXml, outputCol.RefId)
                    outputCol.XmlDefinition
                    , outputNode);

                colNode.Precision = outputCol.Precision;
                colNode.Scale = outputCol.Scale;
                colNode.Length = outputCol.Length;
                colNode.DtsDataType = outputCol.DataType.ToString();

                outputNode.AddChild(colNode);
                conversionOutputMapping[outputCol.Name] = colNode;
                
                var sourceColId = outputCol.GetPropertyValue("SourceInputColumnLineageID");

                colNode.SourceDfColumn = inputColumnsByLineageId[sourceColId];

            }
            
            return componentElement;
        }
    }
}
