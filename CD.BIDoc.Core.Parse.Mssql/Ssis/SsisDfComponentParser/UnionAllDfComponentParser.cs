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
    class UnionAllDfComponentParser : SsisDfComponentParserBase, ISsisDfComponentParser
    {
        public int Priority { get { return 10; } }

        public bool CanParse(SsisDfComponent component)
        {
            return component.Contract.Contains("Union All");
        }

        public DfComponentElement ParseComponent(SsisDfComponentContext context)
        {
            var componentElement = new DfUnionAllElement(context.ComponentRefPath, context.Component.Name, context.Component.XmlDefinition, context.DfElement);
            context.DfElement.AddChild(componentElement);
            
            SsisDfOutput unionOutput = null;
            foreach (var output in context.Component.Outputs)
            {
                if (!output.IsErrorOutput)
                {
                    unionOutput = output;
                    break;
                }
            }
            if (unionOutput == null)
            {
                throw new Exception("Missing union all output");
            }

            XmlElement outputDefinitionXml = null;
            DfOutputElement outputNode = new DfOutputElement(context.UrnBuilder.GetDfOutputUrn(componentElement, unionOutput.Name), unionOutput.Name,
                //context.DefinitionSearcher.GetDfComponentOutputDefinition(context.ComponentDefinitionXml, unionOutput.RefId, out outputDefinitionXml)
                unionOutput.XmlDefinition
                , componentElement);
            componentElement.AddChild(outputNode);
            outputNode.OutputType = unionOutput.IsErrorOutput ? DfOutputTypeEnum.ErrorOutput : DfOutputTypeEnum.Output;

            ComponentOutput unionOutputMapping = new ComponentOutput() { ModelElement = outputNode };
            context.ComponentIO.Outputs[unionOutput.RefId] = unionOutputMapping;


            Dictionary<string, DfColumnElement> outputColsById = new Dictionary<string, DfColumnElement>();
            foreach (var outputCol in unionOutput.Columns)
            {
                DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfOutputColumnUrn(outputNode, outputCol.Name /*, outputCol.ID*/), outputCol.Name,
                    //context.DefinitionSearcher.GetDfOutputColumnDefinition(outputDefinitionXml, outputCol.RefId)
                    outputCol.XmlDefinition
                    , outputNode);

                colNode.Precision = outputCol.Precision;
                colNode.Scale = outputCol.Scale;
                colNode.Length = outputCol.Length;
                colNode.DtsDataType = outputCol.DataType.ToString();

                outputNode.AddChild(colNode);
                unionOutputMapping[outputCol.Name] = colNode;

                var outputColId = outputCol.LineageID; //.ID;
                outputColsById.Add(outputColId, colNode);
                
            }
            
            foreach (var input in context.Component.Inputs)
            {
                XmlElement inputDefinitionXml = null;
                DfInputElement inputNode = new DfInputElement(context.UrnBuilder.GetDfInputUrn(componentElement, input.Name),
                    input.Name
                    //, context.DefinitionSearcher.GetDfComponentInputDefinition(context.ComponentDefinitionXml,
                    //input.RefId, out inputDefinitionXml)
                    , input.XmlDefinition
                    , componentElement);
                componentElement.AddChild(inputNode);

                inputNode.InputType = DfInputTypeEnum.Input;

                ComponentInput conversionInputMapping = new ComponentInput() { ModelElement = inputNode };

                context.ComponentIO.Inputs[input.RefId] = conversionInputMapping;
                Dictionary<string, DfColumnElement> inputColumnsByLineageId = new Dictionary<string, DfColumnElement>();
                foreach (var inputCol in input.Columns)
                {
                    var name = inputCol.Name;
                    var componentIdString = inputCol.RefId;
                    var externalId = inputCol.ExternalColumnID;
                    
                    var outputeColId = inputCol.GetPropertyValue("OutputColumnLineageID");

                    var outputColElement = outputColsById[outputeColId];
                    
                    DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfInputColumnUrn(inputNode, inputCol.Name /*, inputCol.ID*/), inputCol.Name,
                        //context.DefinitionSearcher.GetDfInputColumnDefinition(inputDefinitionXml, inputCol.RefId)
                        inputCol.XmlDefinition
                        , outputColElement);

                    colNode.Precision = inputCol.Precision;
                    colNode.Scale = inputCol.Scale;
                    colNode.Length = inputCol.Length;
                    colNode.DtsDataType = inputCol.DataType.ToString();
                    outputColElement.AddChild(colNode);
                    

                    conversionInputMapping[inputCol.Name] = colNode;
                    inputColumnsByLineageId[inputCol.LineageID] = colNode;

                }

            }

            return componentElement;
        }
    }
}
