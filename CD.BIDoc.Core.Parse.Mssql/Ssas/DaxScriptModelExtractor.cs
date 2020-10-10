using System;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.Model.Mssql.Ssas;
using CD.DLS.Model.Mssql;
using System.Data;
using Irony.Parsing;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Mssql.Tabular;

namespace CD.DLS.Parse.Mssql.Ssas
{
    /// <summary>
    /// Extracts a model of MDX scripts - mainly cube calculations and reporting datasets.
    /// </summary>
    public class DaxScriptModelExtractor
    {
        private Parser _parser;
        private UrnBuilder _urnBuilder;
        private DaxFunctionFactory _functionFactory = new DaxFunctionFactory();

        public DaxScriptModelExtractor()
        {
            _parser = new Parser(new DaxGrammar());
            _urnBuilder = new UrnBuilder();
        }


        private SsasModelElement TryResolveIdentifier(SsasDatabaseIndex environment, string identifier, TabularReferenceType referenceType)
        {
            if (environment.SsasType == Common.Structures.SsasTypeEnum.Tabular)
            {
                var tabularEnvironment = environment as SsasTabularDatabaseIndex;
                tabularEnvironment.QueryMode = SsasQueryMode.DAX;
                return tabularEnvironment.TryResolveIdentifier(identifier, referenceType);
            }
            else
            {
                var multidimensionalEnvironment = environment as SsasMultidimensionalDatabaseIndex;
                multidimensionalEnvironment.QueryMode = SsasQueryMode.DAX;
                var res = multidimensionalEnvironment.TryResolveIdentifier(identifier);
                multidimensionalEnvironment.QueryMode = SsasQueryMode.MDX;
                return res;
            }
        }


        private DaxFragmentElement ParseDaxExpression(DaxParseTreeNavigator navigator, ParseTreeNode innerRootNode, 
            SsasDatabaseIndex environment, DaxFragmentElement parentElement, 
            out Dictionary<string, SsasModelElement> resultColumns)
        {
            resultColumns = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);
            var dummyColumns = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);

            var specificNode = navigator.GetBottomCoveringNode(innerRootNode);
            var fragmentRefPath = _urnBuilder.GetDaxFragmentUrn(parentElement);
            DaxFragmentElement expressionElement;         
            switch (specificNode.Term.Name)
            {
                case DaxGrammar.NONTERM_TABLE_ID:
                case DaxGrammar.NONTERM_TABLE_ID_QUOTED:
                    var tableResolution = TryResolveIdentifier(environment, specificNode.GetText(navigator.Script), TabularReferenceType.Table);
                    var tableReferenceUrn = _urnBuilder.GetDaxTableReferenceUrn(parentElement);
                    var tableReferenceElement = new DaxTableReferenceElement(tableReferenceUrn, specificNode.Token.Text, specificNode.Token.Text, parentElement);
                    parentElement.AddChild(tableReferenceElement);
                    expressionElement = tableReferenceElement;
                    expressionElement.OffsetFrom = specificNode.Span.EndPosition - specificNode.Span.Length;
                    expressionElement.Length = specificNode.Span.Length;

                    tableReferenceElement.Reference = tableResolution;

                    break;
                case DaxGrammar.NONTERM_FULL_ID:
                case DaxGrammar.NONTERM_COLUMN_ID:
                    var columnResolution = TryResolveIdentifier(environment, specificNode.GetText(navigator.Script), TabularReferenceType.Column);
                    var columnReferenceUrn = _urnBuilder.GetDaxColumnReferenceUrn(parentElement);
                    var columnReferenceElement = new DaxColumnReferenceElement(columnReferenceUrn, specificNode.GetText(navigator.Script), specificNode.GetText(navigator.Script), parentElement);
                    parentElement.AddChild(columnReferenceElement);
                    columnReferenceElement.Reference = columnResolution;
                    expressionElement = columnReferenceElement;
                    expressionElement.OffsetFrom = specificNode.Span.EndPosition - specificNode.Span.Length;
                    expressionElement.Length = specificNode.Span.Length;
                    
                    if (specificNode.ChildNodes.Count > 1)
                    {
                        columnReferenceElement.TableName = specificNode.ChildNodes[0].Token.Text;
                        columnReferenceElement.ColumnName = specificNode.ChildNodes[1].Token.Text;
                    }
                    else
                    {
                        columnReferenceElement.ColumnName = specificNode.Token.Text;
                    }
                    
                    break;
                case DaxGrammar.NONTERM_ID_UNQUOTED:
                    var referenceResolution = TryResolveIdentifier(environment, specificNode.GetText(navigator.Script), TabularReferenceType.General);
                    var referenceUrn = _urnBuilder.GetDaxColumnReferenceUrn(parentElement);
                    if (referenceResolution != null)
                    {
                        if (referenceResolution is DaxTableOperationElement || referenceResolution is SsasTabularTableElement)
                        {
                            var tableReferenceUrn2 = _urnBuilder.GetDaxTableReferenceUrn(parentElement);
                            var tableReferenceElement2 = new DaxTableReferenceElement(tableReferenceUrn2, specificNode.Token.Text, specificNode.Token.Text, parentElement);
                            parentElement.AddChild(tableReferenceElement2);
                            expressionElement = tableReferenceElement2;
                            expressionElement.OffsetFrom = specificNode.Span.EndPosition - specificNode.Span.Length;
                            expressionElement.Length = specificNode.Span.Length;

                            tableReferenceElement2.Reference = referenceResolution;
                        }
                        else
                        {
                            var columnReferenceUrn2 = _urnBuilder.GetDaxColumnReferenceUrn(parentElement);
                            var columnReferenceElement2 = new DaxColumnReferenceElement(columnReferenceUrn2, specificNode.Token.Text, specificNode.Token.Text, parentElement);
                            parentElement.AddChild(columnReferenceElement2);
                            expressionElement = columnReferenceElement2;
                            expressionElement.OffsetFrom = specificNode.Span.EndPosition - specificNode.Span.Length;
                            expressionElement.Length = specificNode.Span.Length;

                            columnReferenceElement2.Reference = referenceResolution;
                            if (specificNode.ChildNodes.Count > 1)
                            {
                                columnReferenceElement2.TableName = specificNode.ChildNodes[0].Token.Text;
                                columnReferenceElement2.ColumnName = specificNode.ChildNodes[1].Token.Text;
                            }
                            else
                            {
                                columnReferenceElement2.ColumnName = specificNode.Token.Text;
                            }
                        }
                    }
                    else
                    {
                        var referenceFragmentElement = new DaxFragmentElement(fragmentRefPath, specificNode.GetText(navigator.Script), specificNode.GetText(navigator.Script), parentElement);
                        parentElement.AddChild(referenceFragmentElement);
                        expressionElement = referenceFragmentElement;
                        expressionElement.OffsetFrom = specificNode.Span.EndPosition - specificNode.Span.Length;
                        expressionElement.Length = specificNode.Span.Length;

                    }
                    break;
                case DaxGrammar.NONTERM_NUMBER:
                    var numberFragmentElement = new DaxFragmentElement(fragmentRefPath, specificNode.GetText(navigator.Script), specificNode.GetText(navigator.Script), parentElement);
                    parentElement.AddChild(numberFragmentElement);
                    expressionElement = numberFragmentElement;
                    expressionElement.OffsetFrom = specificNode.Span.EndPosition - specificNode.Span.Length;
                    expressionElement.Length = specificNode.Span.Length;

                    break;
                case DaxGrammar.NONTERM_STRING:
                    var literalFragmentElement = new DaxLiteralElement(fragmentRefPath, specificNode.GetText(navigator.Script), specificNode.GetText(navigator.Script), parentElement);
                    parentElement.AddChild(literalFragmentElement);
                    expressionElement = literalFragmentElement;
                    expressionElement.OffsetFrom = specificNode.Span.EndPosition - specificNode.Span.Length;
                    expressionElement.Length = specificNode.Span.Length;

                    break;
                case DaxGrammar.NONTERM_FUNCTION_CALL:
                    var functionName = navigator.GetFirstIdContent(specificNode);
                    var functionElement = _functionFactory.CreateFunctionElement(functionName, parentElement);
                    parentElement.AddChild(functionElement);
                    expressionElement = functionElement;
                    expressionElement.OffsetFrom = specificNode.Span.EndPosition - specificNode.Span.Length;
                    expressionElement.Length = specificNode.Span.Length;

                    var arguments = navigator.ListOperationArguments(specificNode);
                    int argumentCount = 0;
                    foreach (var argument in arguments)
                    {
                        argumentCount++;
                        var argumentExpression = navigator.FindExpressionNode(argument);
                        var argumentUrn = _urnBuilder.GetDaxOperationArgumentUrn(functionElement);
                        DaxOperationArgumentElement argumentElement = new DaxOperationArgumentElement(argumentUrn, string.Format("Argument {0}", argumentCount), null, functionElement);
                        functionElement.AddChild(argumentElement);
                        ParseDaxExpression(navigator, argumentExpression, environment, argumentElement, out dummyColumns);
                    }
                    functionElement.CreateDataFlowLinksAndOutputColumns();
                    
                    break;
                case DaxGrammar.NONTERM_IN_OPERATOR:
                    DaxBinaryScalarOperationElement inOperationElement = new DaxBinaryScalarOperationElement(fragmentRefPath, "IN", specificNode.GetText(navigator.Script), parentElement);
                    parentElement.AddChild(inOperationElement);
                    var firstId = navigator.GetFirstId(specificNode);
                    var firstIdExpression = ParseDaxExpression(navigator, firstId, environment, inOperationElement, out dummyColumns);
                    inOperationElement.AddDataFlowLink(firstIdExpression);
                    expressionElement = inOperationElement;
                    expressionElement.OffsetFrom = specificNode.Span.EndPosition - specificNode.Span.Length;
                    expressionElement.Length = specificNode.Span.Length;

                    var inSetItems = navigator.ListOperationArguments(specificNode);
                    int itemCount = 0;
                    foreach (var item in inSetItems)
                    {
                        itemCount++;
                        var argumentUrn = _urnBuilder.GetDaxOperationArgumentUrn(inOperationElement);
                        DaxOperationArgumentElement argumentElement = new DaxOperationArgumentElement(argumentUrn, string.Format("Item {0}", itemCount), null, inOperationElement);
                        inOperationElement.AddChild(argumentElement);
                        ParseDaxExpression(navigator, item, environment, argumentElement, out dummyColumns);
                        inOperationElement.AddDataFlowLink(argumentElement);
                    }
                    
                    break;
                case DaxGrammar.NONTERM_BOOL_OR:
                case DaxGrammar.NONTERM_BOOL_AND:
                case DaxGrammar.NONTERM_EQ:
                case DaxGrammar.NONTERM_REL:
                case DaxGrammar.NONTERM_CONCAT:
                case DaxGrammar.NONTERM_ADD:
                case DaxGrammar.NONTERM_MUL:
                    DaxBinaryScalarOperationElement binaryScalarOperationElement = new DaxBinaryScalarOperationElement(fragmentRefPath, specificNode.Term.Name, specificNode.GetText(navigator.Script), parentElement);
                    parentElement.AddChild(binaryScalarOperationElement);
                    expressionElement = binaryScalarOperationElement;
                    expressionElement.OffsetFrom = specificNode.Span.EndPosition - specificNode.Span.Length;
                    expressionElement.Length = specificNode.Span.Length;

                    var firstChild = specificNode.ChildNodes.First();
                    var lastChild = specificNode.ChildNodes.Last();
                    var leftSideExpression = ParseDaxExpression(navigator, firstChild, environment, binaryScalarOperationElement, out dummyColumns);
                    var rightSideExpression = ParseDaxExpression(navigator, lastChild, environment, binaryScalarOperationElement, out dummyColumns);
                    binaryScalarOperationElement.AddDataFlowLink(leftSideExpression);
                    binaryScalarOperationElement.AddDataFlowLink(rightSideExpression);

                    break;
                case DaxGrammar.NONTERM_UNARY_ARITHMETIC_EXPRESSION:
                case DaxGrammar.NONTERM_BOOL_UNARY:
                    DaxUnaryScalarOperationElement unaryScalarOperationElement = new DaxUnaryScalarOperationElement(fragmentRefPath, specificNode.Term.Name, specificNode.GetText(navigator.Script), parentElement);
                    parentElement.AddChild(unaryScalarOperationElement);
                    expressionElement = unaryScalarOperationElement;
                    expressionElement.OffsetFrom = specificNode.Span.EndPosition - specificNode.Span.Length;
                    expressionElement.Length = specificNode.Span.Length;

                    var innerNode = specificNode.ChildNodes.Last();
                    var innerExpression = ParseDaxExpression(navigator, innerNode, environment, unaryScalarOperationElement, out dummyColumns);
                    unaryScalarOperationElement.AddDataFlowLink(innerExpression);
                    
                    break;
                case DaxGrammar.NONTERM_TUPLE:
                    DaxScalarOperationElement tupleOperationElement = new DaxScalarOperationElement(fragmentRefPath, "Tuple", specificNode.GetText(navigator.Script), parentElement);
                    parentElement.AddChild(tupleOperationElement);
                    expressionElement = tupleOperationElement;
                    expressionElement.OffsetFrom = specificNode.Span.EndPosition - specificNode.Span.Length;
                    expressionElement.Length = specificNode.Span.Length;

                    var tupleItems = navigator.ListOperationArguments(specificNode);
                    int tupleItemCount = 0;
                    foreach (var item in tupleItems)
                    {
                        tupleItemCount++;
                        var argumentUrn = _urnBuilder.GetDaxOperationArgumentUrn(tupleOperationElement);
                        DaxOperationArgumentElement argumentElement = new DaxOperationArgumentElement(argumentUrn, string.Format("Item {0}", tupleItemCount), null, tupleOperationElement);
                        tupleOperationElement.AddChild(argumentElement);
                        ParseDaxExpression(navigator, item, environment, argumentElement, out dummyColumns);
                        tupleOperationElement.AddDataFlowLink(argumentElement);
                    }
                    
                    break;
                default:
                    throw new Exception("Unexpected DAX expression type: " + specificNode.Term.Name);
            }

            // offset & length
            

            // output columns 
            if (expressionElement is DaxTableOperationElement || 
                (expressionElement is DaxExpressionEvaluationFunctionElement &&
                (expressionElement as DaxExpressionEvaluationFunctionElement).HasTableOutput))
            {
                var tableOperation = expressionElement as DaxOperationElement; // DaxTableOperationElement;
                foreach (var column in tableOperation.OutputColumns)
                {
                    var columnName = column.Caption;
                    if(!columnName.StartsWith("["))
                    {
                        columnName = "[" + columnName + "]";
                    }
                    resultColumns.Add(columnName, column);
                }
            }
            else if (expressionElement is DaxTableReferenceElement)
            {
                var baseTable = expressionElement.Reference as SsasTabularTableElement;
                var tableOperation = expressionElement.Reference as DaxTableOperationElement;

                if (baseTable != null)
                {
                    foreach (var column in baseTable.Columns)
                    {
                        var columnName = column.Caption;
                        if (!columnName.StartsWith("["))
                        {
                            columnName = "[" + columnName + "]";
                        }
                        resultColumns.Add(columnName, column);
                    }
                }
                else if (tableOperation != null)
                {
                    foreach (var column in tableOperation.OutputColumns)
                    {
                        var columnName = column.Caption;
                        if (!columnName.StartsWith("["))
                        {
                            columnName = "[" + columnName + "]";
                        }
                        resultColumns.Add(columnName, column);
                    }
                }
            }
            else if (expressionElement is DaxColumnReferenceElement)
            {
                var columnName = ((DaxColumnReferenceElement)expressionElement).ColumnName;
                if (!columnName.StartsWith("["))
                {
                    columnName = "[" + columnName + "]";
                }
                resultColumns.Add(columnName, expressionElement);
            }
            // scalar
            else
            {
                resultColumns.Add("[" + DaxArgumentColumn.DEFAULT_NAME + "]", expressionElement);
            }

            return expressionElement;
        }

        internal void ExtractDaxScript(string calculatedColumnExpression, SsasTabularDatabaseIndex columnCalculationsIndex, SsasTabularTableColumnElement columnElement, out object resultColumns)
        {
            throw new NotImplementedException();
        }

        private void ParseDaxQuery(DaxParseTreeNavigator navigator, ParseTreeNode innerRootNode, 
            SsasDatabaseIndex environment, DaxScriptElement scriptElement, 
            out Dictionary<string, SsasModelElement> resultColumns,
            int fragmentOffset = 0)
        {
            resultColumns = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);

            var multidimensionalEnvironment = environment as SsasMultidimensionalDatabaseIndex;
            var tabularEnvirionment = environment as SsasTabularDatabaseIndex;

            // local definitions

            var localDefinitions = navigator.GetLocalDefinitions();
            foreach (var localDefinition in localDefinitions)
            {
                var definitionType = navigator.GetLocalDefinitionType(localDefinition);
                var id = navigator.GetFirstIdContent(localDefinition);
                var expression = navigator.FindExpressionNode(localDefinition);
                DaxFragmentElement coveringElement = null;
                switch (definitionType)
                {
                    case LocalDefinitionType.Measure:
                        var measureUrn = _urnBuilder.GetDaxLocalMeasureUrn(scriptElement);
                        coveringElement = new DaxLocalMeasureElement(measureUrn, id, navigator.GetDefinition(localDefinition), scriptElement);
                        if (environment.SsasType == Common.Structures.SsasTypeEnum.Tabular)
                        {
                            tabularEnvirionment.AddLocalMeasure(id, coveringElement);
                        }
                        else
                        {
                            // local measure in DAX query for multidimensional DB - low priority
                            //multidimensionalEnvironment.AddLocalMeasure()
                        }
                        break;
                    case LocalDefinitionType.Variable:
                        var variableUrn = _urnBuilder.GetDaxLocalVariableUrn(scriptElement);
                        coveringElement = new DaxLocalVariableElement(variableUrn, id, navigator.GetDefinition(localDefinition), scriptElement);
                        if (environment.SsasType == Common.Structures.SsasTypeEnum.Tabular)
                        {
                            tabularEnvirionment.AddLocalVariable(id, coveringElement);
                        }
                        else
                        {
                            // local variable in DAX query for multidimensional DB - low priority
                        }
                        break;
                    default:
                        throw new Exception();
                }

                scriptElement.AddChild(coveringElement);

                Dictionary<string, SsasModelElement> expressionResultColumns = null;
                ParseDaxExpression(navigator, expression, environment, coveringElement, out expressionResultColumns);

                foreach (var expressionResColumn in expressionResultColumns)
                {
                    var linkCount = coveringElement.Children.Count();
                    var linkRefPath = coveringElement.RefPath.NamedChild("DataFlowLink", string.Format("No_{0}", linkCount + 1));
                    DaxDataFlowLinkElement daxDataFlowLinkElement = new DaxDataFlowLinkElement(linkRefPath, string.Format("DataFlowLink {0}", linkCount + 1), null, coveringElement);
                    coveringElement.AddChild(daxDataFlowLinkElement);
                    daxDataFlowLinkElement.Parent = coveringElement;
                    daxDataFlowLinkElement.Source = expressionResColumn.Value;
                    daxDataFlowLinkElement.Target = coveringElement;
                }
            }

            // first evaluate
            var topEvaluate = navigator.GetTopEvaluate();
            var topEvaluateExpression = navigator.FindExpressionNode(topEvaluate);

            ParseDaxExpression(navigator, topEvaluateExpression, environment, scriptElement, out resultColumns);

            environment.ClearLocalIndexes();
        }


        public DaxScriptElement ExtractDaxScript(string script, SsasDatabaseIndex environment, MssqlModelElement parent, out Dictionary<string, SsasModelElement> resultColumns)
        {
            resultColumns = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);
            var originalResultColumns = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);

            var scriptUrn = _urnBuilder.GetDaxScriptUrn(parent);
            var scriptElement = new DaxScriptElement(scriptUrn, scriptUrn.RefId, script, parent);
            
            var parsed = _parser.Parse(script);
            if (parsed.Root == null)
            {
                // TODO log errors
                foreach (var msg in parsed.ParserMessages)
                {
                    ConfigManager.Log.Warning(string.Format("Error parsing DAX: {0} | at {1}:{2} | {3} | {4}",
                        script, msg.Location.Line, msg.Location.Column, msg.Message, parent.RefPath.Path));
                }
                return null;
            }

            scriptElement.OffsetFrom = 0;
            scriptElement.Length = parsed.Root.Span.Length;
            
                var navigator = new DaxParseTreeNavigator(parsed);
            ParseTreeNode innerRootNode;
            var scriptType = navigator.FindScriptType(out innerRootNode);

            var originalQueryMode = environment.QueryMode;
            environment.QueryMode = SsasQueryMode.DAX;

            switch (scriptType)
            {
                case DaxScriptType.Expression:
                    ParseDaxExpression(navigator, innerRootNode, environment, scriptElement, out originalResultColumns);
                    break;
                case DaxScriptType.Query:
                    ParseDaxQuery(navigator, innerRootNode, environment, scriptElement, out originalResultColumns);
                    break;
            }

            foreach(var origColumn in originalResultColumns.Keys)
            {
                var columnName = origColumn;
                if (origColumn.StartsWith("["))
                {
                    columnName = origColumn.Substring(1, origColumn.Length - 2);
                }
                resultColumns.Add(columnName, originalResultColumns[origColumn]);
            }

            environment.ClearLocalIndexes();
            environment.QueryMode = originalQueryMode;

            return scriptElement;
        }

    }

}