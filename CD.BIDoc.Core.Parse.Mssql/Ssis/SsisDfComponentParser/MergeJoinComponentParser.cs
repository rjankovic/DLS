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
    class MergeJoinComponentParser : SsisDfComponentParserBase, ISsisDfComponentParser
    {
        public int Priority { get { return 10; } }

        public bool CanParse(SsisDfComponent component)
        {
            return component.Contract.Contains("Merge Join");
        }

        public DfComponentElement ParseComponent(SsisDfComponentContext context)
            {
            var componentElement = new DfMergeJoinElement(context.ComponentRefPath, context.Component.Name, context.Component.XmlDefinition, context.DfElement);
            context.DfElement.AddChild(componentElement);

            Dictionary<string, DfColumnElement> inputColumnsById = new Dictionary<string, DfColumnElement>();

            foreach (var input in context.Component.Inputs) {

                XmlElement inputDefinitionXml = null;
                DfInputElement inputNode = new DfInputElement(context.UrnBuilder.GetDfInputUrn(componentElement, input.Name), input.Name,
                    //context.DefinitionSearcher.GetDfComponentInputDefinition(context.ComponentDefinitionXml, input.RefId, out inputDefinitionXml)
                    input.XmlDefinition
                    , componentElement);
                componentElement.AddChild(inputNode);

                ComponentInput mergeJoinInputMapping = new ComponentInput() { ModelElement = inputNode };
                inputNode.InputType = DfInputTypeEnum.Input;

                context.ComponentIO.Inputs[input.RefId] = mergeJoinInputMapping;


                foreach (var inputCol in input.Columns)
                {
                    var name = inputCol.Name;
                    var componentIdString = inputCol.RefId;
                    var externalId = inputCol.ExternalColumnID;

                    DfColumnElement colNode = new DfColumnElement(context.UrnBuilder.GetDfInputColumnUrn(inputNode, inputCol.Name /*, inputCol.ID*/),
                        inputCol.Name
                        //, context.DefinitionSearcher.GetDfInputColumnDefinition(inputDefinitionXml, inputCol.RefId)
                        , inputCol.XmlDefinition
                        , inputNode);

                    colNode.Precision = inputCol.Precision;
                    colNode.Scale = inputCol.Scale;
                    colNode.Length = inputCol.Length;
                    colNode.DtsDataType = inputCol.DataType.ToString();

                    inputNode.AddChild(colNode);

                    mergeJoinInputMapping[inputCol.Name] = colNode;
                    inputColumnsById[inputCol.LineageID] = colNode;

                }
            }
            SsisDfOutput mergeJoinOutput = null;
            foreach (var output in context.Component.Outputs)
            {
                if (!output.IsErrorOutput)
                {
                    mergeJoinOutput = output;
                    break;
                }
            }
            if (mergeJoinOutput == null)
            {
                throw new Exception("Missing merge join output");
            }

            /*create model component*/
            XmlElement outputDefinitionXml = null;
            DfOutputElement outputNode = new DfOutputElement(context.UrnBuilder.GetDfOutputUrn(componentElement, mergeJoinOutput.Name), mergeJoinOutput.Name,
                //context.DefinitionSearcher.GetDfComponentOutputDefinition(context.ComponentDefinitionXml, mergeJoinOutput.RefId, out outputDefinitionXml)
                mergeJoinOutput.XmlDefinition
                , componentElement);
            componentElement.AddChild(outputNode);
            outputNode.OutputType = mergeJoinOutput.IsErrorOutput ? DfOutputTypeEnum.ErrorOutput : DfOutputTypeEnum.Output;

            ComponentOutput mergeJoinOutputMapping = new ComponentOutput() { ModelElement = outputNode };
            context.ComponentIO.Outputs[mergeJoinOutput.RefId] = mergeJoinOutputMapping;

            Dictionary<int, DfColumnElement> outputColsById = new Dictionary<int, DfColumnElement>();

            foreach (var outputCol in mergeJoinOutput.Columns)
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
                mergeJoinOutputMapping[outputCol.Name] = colNode;

                var sourceColId = outputCol.GetPropertyValue("InputColumnID");

                colNode.SourceDfColumn = inputColumnsById[sourceColId];
                /*
                var outputColId = outputCol.ID;
                outputColsById.Add(outputColId, colNode);
                */
            }

            return componentElement;
        }
    }
}
