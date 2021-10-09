using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.PowerQuery;
using CD.DLS.Parse.Mssql;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.Parse.Mssql.PowerQuery;
using CD.DLS.Parse.Mssql.Ssas;
using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Core.Parse.Mssql.PowerQuery
{
    class PowerQueryExtractor
    {
        private Parser _parser;
        private Dictionary<string, MFragmentElement> _localVariables;
        private MParseTreeNavigator _navigator;

        public PowerQueryExtractor(ProjectConfig projectConfig, GraphManager graphManager)
        {
            _parser = new Parser(new MGrammar());
        }




        public PowerQueryElement ExtractPowerQuery(string script, AvailableDatabaseModelIndex sqlDbIndex, MssqlModelElement parent, out Dictionary<string, OperationOutputColumnElement> resultColumns)
        {
            _localVariables = new Dictionary<string, MFragmentElement>();
            resultColumns = new Dictionary<string, OperationOutputColumnElement>(StringComparer.OrdinalIgnoreCase);
            //var originalResultColumns = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);

            var ordinal = parent.Children.OfType<PowerQueryElement>().Count() + 1;
            var refPath = parent.RefPath.NamedChild("PowerQuery", "No_" + ordinal);
            var queryElement = new PowerQueryElement(refPath, refPath.RefId, script, parent);
            parent.AddChild(queryElement);

            var parsed = _parser.Parse(script);
            if (parsed.Root == null)
            {
                foreach (var msg in parsed.ParserMessages)
                {
                    ConfigManager.Log.Warning(string.Format("Error parsing Power Query: {0} | at {1}:{2} | {3} | {4}",
                        script, msg.Location.Line, msg.Location.Column, msg.Message, parent.RefPath.Path));
                }
                return null;
            }

            queryElement.OffsetFrom = 0;
            queryElement.Length = parsed.Root.Span.Length;

            _navigator = new MParseTreeNavigator(parsed);
            var rootNode = parsed.Root;

            var steps = _navigator.GetQuerySteps();

            int stepCounter = 0;
            foreach (var step in steps)
            {
                stepCounter++;

                var stepElement = new FormulaStepElement(parent.RefPath.NamedChild("Step", "Step_" + stepCounter.ToString()), $"Step {stepCounter}", step.AssignmentNode.GetText(_navigator.Script), parent);
                parent.AddChild(stepElement);
                stepElement.VariableName = step.Name;
                SetUpFramgmentNodeSpan(stepElement, step.AssignmentNode);

                if (step.ExpressionNode.Term.Name == MGrammar.NONTERM_FUNCTION_SPEC)
                {
                    // TODO: maybe later
                    continue;
                }

                var expressionElement = ExtractMExpression(step.ExpressionNode, sqlDbIndex, stepElement);
                _localVariables.Add(step.Name, expressionElement);
            }

            var outputExpression = _navigator.GetQueryOutputExpression();
            var outputParsed = ExtractMExpression(outputExpression, sqlDbIndex, queryElement);



            //ParseTreeNode innerRootNode;
            //var scriptType = navigator.FindScriptType(out innerRootNode);

            //var originalQueryMode = environment.QueryMode;
            //environment.QueryMode = SsasQueryMode.DAX;

            //switch (scriptType)
            //{
            //    case DaxScriptType.Expression:
            //        ParseDaxExpression(navigator, innerRootNode, environment, scriptElement, out originalResultColumns);
            //        break;
            //    case DaxScriptType.Query:
            //        ParseDaxQuery(navigator, innerRootNode, environment, scriptElement, out originalResultColumns);
            //        break;
            //}

            if (outputParsed is OperationElement)
            {
                foreach (var column in ((OperationElement)outputParsed).OutputColumns)
                {
                    resultColumns.Add(column.Caption, column);
                }
            }
            else
            { 
                // TODO: scalar result? can that even be used?
            }

            return queryElement;
        }

        private void SetUpFramgmentNodeSpan(MFragmentElement element, ParseTreeNode node)
        {
            element.OffsetFrom = node.Span.EndPosition - node.Span.Length;
            element.Length = node.Span.Length;
        }

        public MFragmentElement ExtractMExpression(ParseTreeNode parseTreeNode, AvailableDatabaseModelIndex sqlDbIndex, MFragmentElement parent)
        {
            var expressionNode = _navigator.FindExpressionNode(parseTreeNode);
            var specificNode = _navigator.GetBottomCoveringNode(expressionNode);
            var definition = specificNode.GetText(_navigator.Script);
            MFragmentElement fragmentElement = null;
            var fragmentUrn = parent.RefPath.NamedChild(specificNode.GetTokens().First(), $"No_{parent.Children.Count() + 1}");


            switch (specificNode.Term.Name)
            {
                case MGrammar.NONTERM_FUNCTION_CALL:
                    var arguments = _navigator.ListOperationArguments(specificNode);
                    var functionName = _navigator.GetFirstIdContent(specificNode);
                    var functionUrn = parent.RefPath.NamedChild(functionName, $"No_{parent.Children.Count() + 1}");
                    OperationElement operationElement = null;

                    switch (functionName)
                    {
                        case "Sql.Database":
                            operationElement = new SqlDatabaseOperationElement(functionUrn, functionName, definition, parent);
                            break;
                    }

                    int argumentCouter = 0;
                    foreach (var argument in arguments)
                    {
                        argumentCouter++;
                        var argumentName = $"Argument {argumentCouter}";
                        var argumentUrn = operationElement.RefPath.NamedChild("Argument", argumentName);
                        var argumentElement = new OperationArgumentElement(argumentUrn, argumentName, argument.GetText(_navigator.Script), operationElement);
                        operationElement.AddChild(argumentElement);
                        ExtractMExpression(argument, sqlDbIndex, argumentElement);
                    }

                    operationElement.CreateDataFlowLinksAndOutputColumns();
                    fragmentElement = operationElement;
                    break;
                case MGrammar.TERM_NUMBER:
                    var numberFragmentElement = new MFragmentElement(fragmentUrn, "Numer", definition, parent);
                    fragmentElement = numberFragmentElement;
                    break;
                case MGrammar.TERM_STRING:
                    var literalElement = new LiteralElement(fragmentUrn, "String", definition, parent);
                    fragmentElement = literalElement;
                    break;
                case MGrammar.NONTERM_LIST_ACCESS_EXPRESSION:
                    ListAccessElement listAccessElement = new ListAccessElement(fragmentUrn, "List Access", definition, parent);
                    var firstId = _navigator.GetFirstId(specificNode).GetText(_navigator.Script);
                    listAccessElement.ListName = firstId;
                    fragmentElement = listAccessElement;
                    
                    var indices = _navigator.GetIndices(specificNode);
                    MFragmentElement indexParent = listAccessElement;
                    foreach (var index in indices)
                    {
                        var indexDef = index.GetText(_navigator.Script);
                        var indexRefPath = indexParent.RefPath.Child("AccessIndex");
                        var indexElement = new ListIndexElement(indexRefPath, "Index", indexDef, indexParent);
                        indexParent.AddChild(indexElement);
                        ExtractMExpression(index, sqlDbIndex, indexParent);
                        indexParent = indexElement;
                    }
                    break;
                case MGrammar.NONTERM_RECORD_EXPRESSION:
                    var recordItems = _navigator.GetRecordItems(specificNode);
                    var recordElement = new RecordElement(fragmentUrn, "Record", definition, parent);
                    fragmentElement = recordElement;

                    int itemCount = 0;
                    foreach (var item in recordItems)
                    {
                        itemCount++;
                        var itemRefPath = recordElement.RefPath.NamedChild("Item", $"Item {itemCount}");
                        var recordItemElement = new RecordItemElement(itemRefPath, $"Item {itemCount}", item.AssignmentNode.GetText(_navigator.Script), recordElement);
                        recordElement.AddChild(recordItemElement);
                        recordItemElement.ItemName = item.Name;
                        ExtractMExpression(item.ExpressionNode, sqlDbIndex, recordItemElement);
                    }
                    break;


            }
            SetUpFramgmentNodeSpan(fragmentElement, specificNode);
            parent.AddChild(fragmentElement);

            return fragmentElement;
        }

    }
}
