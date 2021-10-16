using CD.BIDoc.Core.Parse.Mssql.PowerQuery;
using CD.DLS.Common.Structures;
using CD.DLS.Common.Tools;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Db;
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
    public class PowerQueryExtractor
    {
        private Parser _parser;
        private Dictionary<string, MFragmentElement> _localVariables;
        private MParseTreeNavigator _navigator;
        private AvailableDatabaseModelIndex _sqlDbIndex;

        public PowerQueryExtractor(ProjectConfig projectConfig, GraphManager graphManager)
        {
            _parser = new Parser(new MGrammar());
        }


        public PowerQueryElement ExtractPowerQuery(string script, AvailableDatabaseModelIndex sqlDbIndex, MssqlModelElement parent, out Dictionary<string, OperationOutputColumnElement> resultColumns)
        {
            _sqlDbIndex = sqlDbIndex;
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

                var stepElement = new FormulaStepElement(queryElement.RefPath.NamedChild("Step", "Step_" + stepCounter.ToString()), $"Step {stepCounter}", step.AssignmentNode.GetText(_navigator.Script), queryElement);
                queryElement.AddChild(stepElement);
                stepElement.VariableName = step.Name;
                SetUpFramgmentNodeSpan(stepElement, step.AssignmentNode);

                if (step.ExpressionNode.Term.Name == MGrammar.NONTERM_FUNCTION_SPEC)
                {
                    // TODO: maybe later
                    continue;
                }

                var expressionElement = ExtractMExpression(step.ExpressionNode, stepElement);
                _localVariables.Add(step.Name, expressionElement);
            }

            var outputExpression = _navigator.GetQueryOutputExpression();
            var outputParsed = ExtractMExpression(outputExpression, queryElement);



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

        public MFragmentElement ExtractMExpression(ParseTreeNode parseTreeNode, MFragmentElement parent)
        {
            //var expressionNode = _navigator.FindExpressionNode(parseTreeNode);
            var specificNode = _navigator.GetBottomCoveringNode(parseTreeNode);
            var definition = specificNode.GetText(_navigator.Script);
            MFragmentElement fragmentElement = null;
            var fragmentUrn = parent.RefPath.NamedChild("Fragment", $"No_{parent.Children.Count() + 1}");


            switch (specificNode.Term.Name)
            {
                case MGrammar.NONTERM_FUNCTION_CALL:
                    var arguments = _navigator.ListOperationArguments(specificNode);
                    var functionName = _navigator.GetFirstIdContent(specificNode);
                    var functionUrn = parent.RefPath.NamedChild("FunctionCall", $"{functionName}_{parent.Children.Count() + 1}");
                    OperationElement operationElement = null;

                    switch (functionName)
                    {
                        case "Table.AlternateRows":
                        case "Table.Combine":
                        case "Table.FindText":
                        case "Table.First":
                        case "Table.FirstN":
                        case "Table.FirstValue":
                        case "Table.FromPartitions":
                        case "Table.InsertRows":
                        case "Table.Last":
                        case "Table.LastN":
                        case "Table.MatchesAllRows":
                        case "Table.MatchesAnyRows":
                        case "Table.Partition":
                        case "Table.PartitionValues":
                        case "Table.Range":
                        case "Table.RemoveFirstN":
                        case "Table.RemoveLastN":
                        case "Table.RemoveRows":
                        case "Table.RemoveRowsWithErrors":
                        case "Table.Repeat":
                        case "Table.ReplaceRows":
                        case "Table.ReverseRows":
                        case "Table.SelectRows":
                        case "Table.SelectRowsWithErrors":
                        case "Table.SingleRow":
                        case "Table.Skip":
                        case "Table.SplitAt":

                        case "Table.Distinct":
                        case "Table.RemoveMatchingRows":
                        case "Table.ReplaceMatchingRows":
                        case "Table.TransformColumns":
                        case "Table.TransformColumnTypes":
                        case "Table.ReplaceValue":
                            operationElement = new TableRowOperationElement(functionUrn, functionName, definition, parent);
                            break;
                        case "Sql.Database":
                            operationElement = new SqlDatabaseOperationElement(functionUrn, functionName, definition, parent);
                            break;
                        case "Table.SplitColumn":
                            operationElement = new TableSplitColumnOperationElement(functionUrn, functionName, definition, parent);
                            break;
                        case "Table.DuplicateColumn":
                            operationElement = new TableDuplicateColumnOperationElement(functionUrn, functionName, definition, parent);
                            break;
                        case "Table.RemoveColumns":
                            operationElement = new TableRemoveColumnsOperationElement(functionUrn, functionName, definition, parent);
                            break;
                        case "Table.SelectColumns":
                            operationElement = new TableSelectColumnsOperationElement(functionUrn, functionName, definition, parent);
                            break;
                        case "Table.RenameColumns":
                            operationElement = new TableRenameColumnsOperationElement(functionUrn, functionName, definition, parent);
                            break;
                        default:
                            operationElement = new GeneralOperationElement(functionUrn, functionName, definition, parent);
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
                        ExtractMExpression(argument, argumentElement);
                    }
                    operationElement.FunctionName = functionName;

                    var actualType = operationElement;
                    CreateDataFlowLinksAndOutputColumns((dynamic)operationElement);
                    //operationElement.CreateDataFlowLinksAndOutputColumns();
                    fragmentElement = operationElement;
                    break;
                case MGrammar.TERM_STRING:
                    var literalElement = new LiteralElement(fragmentUrn, definition, definition, parent);
                    fragmentElement = literalElement;
                    break;
                case MGrammar.NONTERM_LIST_ACCESS_EXPRESSION:
                    ListAccessElement listAccessElement = new ListAccessElement(fragmentUrn, "List Access", definition, parent);
                    var firstIdNode = _navigator.GetFirstId(specificNode);
                    var firstId = firstIdNode.GetText(_navigator.Script);
                    listAccessElement.ListName = firstId;
                    fragmentElement = listAccessElement;
                    
                    var idParsed = ExtractMExpression(firstIdNode, listAccessElement);
                    if (idParsed is VariableReferenceElement)
                    {
                        listAccessElement.ListFromVariable = (VariableReferenceElement)idParsed;
                    }

                    var indices = _navigator.GetIndices(specificNode);

                    ParseListAccessIndices(indices, listAccessElement);
                    //MFragmentElement indexParent = listAccessElement;
                    //foreach (var index in indices)
                    //{
                    //    var indexDef = index.GetText(_navigator.Script);
                    //    var indexRefPath = indexParent.RefPath.Child("AccessIndex");
                    //    var indexElement = new ListIndexElement(indexRefPath, "Index", indexDef, indexParent);
                    //    indexParent.AddChild(indexElement);
                    //    var indexContent = ExtractMExpression(index, indexParent);
                    //    indexElement.Content = indexContent;
                    //    indexParent = indexElement;
                    //}
                    CreateDataFlowLinksAndOutputColumns((dynamic)listAccessElement);
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
                        ExtractMExpression(item.ExpressionNode, recordItemElement);
                    }
                    break;
                case MGrammar.NONTERM_LIST_EXPRESSION:
                    ListElement listElement = new ListElement(fragmentUrn, "List", definition, parent);

                    var items = _navigator.FindListItems(specificNode);
                    foreach (var item in items)
                    {
                        ExtractMExpression(item, listElement);
                    }
                    var indices2 = _navigator.GetIndices(specificNode);
                    ParseListAccessIndices(indices2, listElement);
                    fragmentElement = listElement;
                    break;
                case MGrammar.TERM_ID:
                    var id = definition;
                    if (_localVariables.ContainsKey(id))
                    {
                        var variable = _localVariables[id];
                        VariableReferenceElement variableReferenceElement = new VariableReferenceElement(fragmentUrn, definition, definition, parent);
                        variableReferenceElement.Reference = variable;
                        if (variable is OperationElement)
                        {
                            var inputOperation = variable as OperationElement;
                            PassThroughTableColumns(variableReferenceElement, inputOperation);
                        }
                        fragmentElement = variableReferenceElement;
                    }
                    else
                    {
                        IdentifierElement idElement = new IdentifierElement(fragmentUrn, "ID", definition, parent);
                        fragmentElement = idElement;
                    }
                    break;
                case MGrammar.NONTERM_RECORD_ITEM_ID:
                    var itemId = _navigator.GetFirstId(specificNode).GetText(_navigator.Script);
                    RecordItemIdentifierElement recordItemIdentifierElement = new RecordItemIdentifierElement(fragmentUrn, definition, definition, parent);
                    recordItemIdentifierElement.ItemId = itemId;
                    fragmentElement = recordItemIdentifierElement;
                    break;
                    // don't care about the index for now
                case MGrammar.NONTERM_PRIMARY_EXPRESSION_WITH_RECORD_INDEX:
                    var primaryExpressionNode = _navigator.FindTermByName(specificNode, MGrammar.NONTERM_PRIMARY_EXPRESSION);
                    return ExtractMExpression(primaryExpressionNode, parent);
                    break;
                //case MGrammar.TERM_NUMBER:
                //case MGrammar.NONTERM_PARAMETERS:
                default:
                    var numberFragmentElement = new MFragmentElement(fragmentUrn, definition, definition, parent);
                    fragmentElement = numberFragmentElement;
                    break;

            }
            SetUpFramgmentNodeSpan(fragmentElement, specificNode);
            parent.AddChild(fragmentElement);

            return fragmentElement;
        }

        private void ParseListAccessIndices(List<ParseTreeNode> indices, MFragmentElement indexParent)
        {
            foreach (var index in indices)
            {
                var indexDef = index.GetText(_navigator.Script);
                var indexRefPath = indexParent.RefPath.Child("AccessIndex");
                var indexElement = new ListIndexElement(indexRefPath, "Index", indexDef, indexParent);
                indexParent.AddChild(indexElement);
                var content = _navigator.FindExpressionNode(index);
                var indexContent = ExtractMExpression(content, indexParent);
                indexElement.Content = indexContent;
                indexParent = indexElement;
            }
        }

        // for unknown functions
        private void CreateDataFlowLinksAndOutputColumns(OperationElement operation)
        { 
        
        }

        private void CreateDataFlowLinksAndOutputColumns(SqlDatabaseOperationElement sqlDatabaseOperation)
        {
            var args = CollectArgumentList(sqlDatabaseOperation);
            var serverName = TrimLiteral(args[0].FragmentElement.Definition);
            var serverNameNormalized = ConnectionStringTools.NormalizeServerName(serverName);
            var dbName = TrimLiteral(args[1].FragmentElement.Definition);
            var dbElement = _sqlDbIndex.GetDatabaseElement(serverNameNormalized, dbName);
            if (dbElement == null)
            {
                return;
            }
            sqlDatabaseOperation.DatabaseReference = dbElement;

            if (args.Count < 3)
            {
                return;
            }

            var options = args[2].FragmentElement as RecordElement;
            if (options == null)
            {
                return;
            }

            var queryItem = options.Items.FirstOrDefault(x => x.ItemName == "Query");
            if (queryItem == null)
            {
                return;
            }

            string query = queryItem.ItemValue.Definition;
            query = TrimLiteral(query);

            Dictionary<string, MssqlModelElement> outputColumns;
            ParseSqlQuery(serverNameNormalized, dbName, query, sqlDatabaseOperation, out outputColumns);
            foreach (var kv in outputColumns)
            {
                var outputColumn = AddOutputColumn(sqlDatabaseOperation, kv.Key);
                outputColumn.Reference = kv.Value;
            }
        }

        private void CreateDataFlowLinksAndOutputColumns(TableRowOperationElement operation)
        {
            var args = CollectArgumentList(operation);
            var arg = args[0];
            if (arg.FragmentElement is OperationElement)
            {
                var inputOperation = arg.FragmentElement as OperationElement;
                if (inputOperation is VariableReferenceElement)
                {
                    var varRef = inputOperation as VariableReferenceElement;
                    
                    // save 1 extra step through the variable ref. as argument
                    if (varRef.Reference is OperationElement && varRef.Reference != null)
                    {
                        inputOperation = varRef.Reference as OperationElement;
                    }
                }
                PassThroughTableColumns(operation, inputOperation);
            }
            else
            {
                PassThroughTableColumns(operation, arg);
            }
        }

        // TODO
        private void CreateDataFlowLinksAndOutputColumns(TableSplitColumnOperationElement operation)
        {
            //#"Split Column by Delimiter" = Table.SplitColumn(Table.TransformColumnTypes(dbo_GeneralService_T, {{"Source_System_Reference_Datetime", type text}}, "en-US"), "Source_System_Reference_Datetime", Splitter.SplitTextByDelimiter(",", QuoteStyle.Csv), {"Source_System_Reference_Datetime.1", "Source_System_Reference_Datetime.2", "Source_System_Reference_Datetime.3"}),
            var args = CollectArgumentList(operation);

            var inputTable = args[0].FragmentElement as OperationElement;
            var splitColumn = TrimLiteral(args[1].FragmentElement.Definition);
            var splitColumnElement = inputTable.OutputColumns.FirstOrDefault(x => x.Caption == splitColumn);
            var resColumns = args[args.Count - 1].FragmentElement as ListElement;

            if (inputTable == null || resColumns == null)
            {
                return;
            }

            var outColumns = resColumns.Items.Select(x => x as LiteralElement).Where(x => x != null).Select(x => x.Definition).ToList();
            PassThroughTableColumns(operation, inputTable, new List<string>() { splitColumn });

            foreach (var oc in outColumns)
            {
                var splitOutColumn = AddOutputColumn(operation, oc);
                splitOutColumn.Reference = splitColumnElement;
            }
        }

        private void CreateDataFlowLinksAndOutputColumns(TableDuplicateColumnOperationElement operation)
        {
            //#"Duplicated Column" = Table.DuplicateColumn(#"Trimmed Text", "Main_Service_ID", "Main_Service_ID - Copy"),
            var args = CollectArgumentList(operation);
            var inputTable = args[0].FragmentElement as OperationElement;
            PassThroughTableColumns(operation, inputTable);

            var inputColumn = TrimLiteral(args[1].FragmentElement.Definition);
            var inputColumnElement = inputTable.OutputColumns.FirstOrDefault(x => x.Caption == inputColumn);
            var duplicatedColumn = TrimLiteral(args[2].FragmentElement.Definition);
            
            if (inputColumnElement == null)
            {
                return;
            }
            
            var duplicatedOutput = AddOutputColumn(operation, duplicatedColumn);
            duplicatedOutput.Reference = inputColumnElement;
        }

        private void CreateDataFlowLinksAndOutputColumns(TableRemoveColumnsOperationElement operation)
        {
            //#"Removed Columns" = Table.RemoveColumns(#"Duplicated Column",{"Source_System_Extract_Datetime"}),
            var args = CollectArgumentList(operation);

            var inputTable = args[0].FragmentElement as OperationElement;
            var removedColumns = args[1].FragmentElement as ListElement;
            
            if (inputTable == null)
            {
                return;
            }

            List<string> removedColumnsList;
            if (removedColumns != null)
            {
                removedColumnsList = removedColumns.Items.Select(x => x as LiteralElement).Where(x => x != null).Select(x => TrimLiteral(x.Definition)).ToList();
            }
            else
            {
                var singleRemovedColumn = args[1].FragmentElement as LiteralElement;
                if (singleRemovedColumn == null)
                {
                    return;
                }
                removedColumnsList = new List<string>() { TrimLiteral(singleRemovedColumn.Definition) };
            }
            PassThroughTableColumns(operation, inputTable, removedColumnsList);
        }

        private void CreateDataFlowLinksAndOutputColumns(TableSelectColumnsOperationElement operation)
        {
            //#"Removed Other Columns" = Table.SelectColumns(#"Removed Errors",{"Source_System_Reference_Datetime.2", "Source_System_Code", "SOR_GeneralService_ID", "Main_Service_ID", "Service_Description", "Service_Type", "Variable_Type", "Quantity_Unit", "Extra_Service_Configured", "Effective_Datetime", "End_Datetime", "Current_Row", "Insert_Batch_ID", "Update_Batch_ID", "Main_Service_ID - Copy"}),
            var args = CollectArgumentList(operation);
            var inputTable = args[0].FragmentElement as OperationElement;
            
            var selectColumns = args[1].FragmentElement as ListElement;
            var singleSelectColumn = args[1].FragmentElement as LiteralElement;
            List<string> selectedColumnsList = null;

            if (selectColumns != null)
            {
                selectedColumnsList = selectColumns.Items.Select(x => x as LiteralElement).Where(x => x != null).Select(x => TrimLiteral(x.Definition)).ToList();
            }
            else if (singleSelectColumn != null)
            {
                selectedColumnsList = new List<string>() { TrimLiteral(singleSelectColumn.Definition) };
            }
            else return;

            var removedColumns = args[0].Columns.Select(x => x.Name).Except(selectedColumnsList).ToList();
            PassThroughTableColumns(operation, inputTable, removedColumns);
        }

        private void CreateDataFlowLinksAndOutputColumns(TableRenameColumnsOperationElement operation)
        {
            //#"Renamed Columns" = Table.RenameColumns(#"Filtered Rows",{{"Service_Type", "Service_Type_Rename"}})
            var args = CollectArgumentList(operation);
            var inputTable = args[0].FragmentElement as OperationElement;
            var renamesArg = args[1].FragmentElement as ListElement;

            if (renamesArg == null || inputTable == null)
            {
                return;
            }

            List<Tuple<string, string>> renames = new List<Tuple<string, string>>();
            IEnumerable<ListElement> renameArgTuples = null;
            if (renamesArg.Items.Any(x => x is ListElement))
            {
                renameArgTuples = renamesArg.Items.Select(x => x as ListElement);
            }
            else
            {
                renameArgTuples = new List<ListElement>() { renamesArg }.AsEnumerable();
            }

            foreach (var tpl in renameArgTuples)
            {
                renames.Add(new Tuple<string, string>(TrimLiteral(tpl.Items.First().Definition), TrimLiteral(tpl.Items.Last().Definition)));
            }

            var renamesDict = renames.ToDictionary(x => x.Item1, x => x.Item2);
            foreach (var inputColumn in inputTable.OutputColumns)
            {
                if (renamesDict.ContainsKey(inputColumn.Caption))
                {
                    var ooc = AddOutputColumn(operation, renamesDict[inputColumn.Caption]);
                    ooc.Reference = inputColumn;
                }
                else
                {
                    PassThroughOutputColumn(operation, inputColumn);
                }
            }
        }

        private void CreateDataFlowLinksAndOutputColumns(ListAccessElement listAccess)
        {
            if (listAccess.ListFromVariable == null)
            {
                return;
            }

            var referencedDb = listAccess.ListFromVariable.Reference as SqlDatabaseOperationElement;
            if (referencedDb == null)
            {
                return;
            }

            var tableRefRecord = listAccess.Index.Content as RecordElement;
            if (tableRefRecord == null)
            {
                return;
            }

            var schemaItem = tableRefRecord.Items.FirstOrDefault(x => x.ItemName == "Schema");
            var itemItem = tableRefRecord.Items.FirstOrDefault(x => x.ItemName == "Item");
            if (schemaItem == null || itemItem == null)
            {
                return;
            }

            var schema = TrimLiteral(schemaItem.ItemValue.Definition);
            var item = TrimLiteral(itemItem.ItemValue.Definition);

            var dbElement = referencedDb.DatabaseReference;
            var server = dbElement.Parent as ServerElement;
            var tableRefQuoted = $"[{schema}].[{item}]";

            Dictionary<string, MssqlModelElement> outputColumns;
            var resolvedTable = FindSqlTable(server.Caption, dbElement.DbName, tableRefQuoted, out outputColumns);

            foreach (var kv in outputColumns)
            {
                var outputColumn = AddOutputColumn(listAccess, kv.Key);
                outputColumn.Reference = kv.Value;
            }

        }

        private string TrimLiteral(string literal)
        {
            return literal.Trim('"');
        }

        private MFragmentElement TryResolveVariableReference(string reference)
        {
            var refTrim = TrimLiteral(reference);
            if (_localVariables.ContainsKey(refTrim))
            {
                return _localVariables[refTrim];
            }
            return null;
        }

        protected OperationOutputColumnElement AddOutputColumn(OperationElement operationElement, string name)
        {
            name = TrimLiteral(name);
            var refPath = operationElement.RefPath.NamedChild("OutputColumn", name);
            var columnElement = new OperationOutputColumnElement(refPath, name, null, operationElement);
            operationElement.AddChild(columnElement);
            return columnElement;
        }

        public ArgumentList CollectArgumentList(OperationElement operationElement)
        {
            ArgumentList list = new ArgumentList();
            list.Arguments = new List<Argument>();

            foreach (var argument in operationElement.Arguments)
            {
                var argumentWrap = new Argument()
                {
                    Columns = new List<ArgumentColumn>(),
                    FragmentElement = argument.Content
                };

                if (argument.Content is OperationElement /*&& ((OperationElement)argument.Content).OutputColumns.Any()*/)
                {
                    argumentWrap.ArgumentType = ArgumentType.Table;

                    var operation = (OperationElement)(argument.Content);

                    foreach (var column in operation.OutputColumns)
                    {
                        argumentWrap.Columns.Add(new ArgumentColumn()
                        {
                            Name = column.Caption,
                            RefereneElement = column
                        });
                    }
                }
                //else if (argument.Content is VariableReferenceElement)
                //{
                //    argumentWrap.ArgumentType = ArgumentType.Table;

                //    var variable = argument.Content.Reference as FormulaStepElement;

                //    foreach (var column in variable.Operation.OutputColumns)
                //    {
                //        argumentWrap.Columns.Add(new ArgumentColumn()
                //        {
                //            Name = column.Caption,
                //            RefereneElement = column
                //        });
                //    }
                //}
                else if (argument.Content is ListElement)
                {
                    var lst = argument.Content as ListElement;
                    argumentWrap.ArgumentType = ArgumentType.List;
                }
                // scalar
                else
                {
                    argumentWrap.ArgumentType = ArgumentType.ColumnOrScalar;

                    argumentWrap.Columns.Add(new ArgumentColumn()
                    {
                        RefereneElement = argument.Content
                    });
                }

                list.Arguments.Add(argumentWrap);
            }

            return list;
        }

        public List<OperationOutputColumnElement> PassThroughTableColumns(OperationElement targetOperation, Argument input, List<string> excludedColumns = null)
        {
            if (excludedColumns == null)
            {
                excludedColumns = new List<string>();
            }

            if (input.ArgumentType != ArgumentType.Table)
            {
                throw new InvalidOperationException("Only table argument columns can be passed through");
            }

            List<OperationOutputColumnElement> res = new List<OperationOutputColumnElement>();
            foreach (var column in input.Columns)
            {
                if (excludedColumns.Contains(column.Name))
                {
                    continue;
                }

                if (column.RefereneElement is OperationOutputColumnElement)
                {
                    res.Add(PassThroughOutputColumn(targetOperation, (OperationOutputColumnElement)(column.RefereneElement)));
                }
            }
            return res;
        }

        public List<OperationOutputColumnElement> PassThroughTableColumns(OperationElement targetOperation, OperationElement input, List<string> excludedColumns = null)
        {
            if (excludedColumns == null)
            {
                excludedColumns = new List<string>();
            }
            List<OperationOutputColumnElement> res = new List<OperationOutputColumnElement>();
            foreach (var column in input.OutputColumns)
            {
                if (excludedColumns.Contains(column.Caption))
                {
                    continue;
                }

                res.Add(PassThroughOutputColumn(targetOperation, column));
            }
            return res;
        }

        private OperationOutputColumnElement PassThroughOutputColumn(OperationElement targetOperation, OperationOutputColumnElement inputColumn)
        {
            // lower DF link (input column -> output column)
            var columnName = inputColumn.Caption;
            var outColumn = AddOutputColumn(targetOperation, columnName);
            outColumn.Reference = inputColumn;
            //AddDataFlowLink(targetOperation, inputColumn, outColumn);
            return outColumn;
        }

        public DataFlowLinkElement AddDataFlowLink(MssqlModelElement source, OperationOutputColumnElement targetColumn)
        {
            var targetOperation = (OperationElement)targetColumn.Parent;
            var linkCount = targetOperation.DataFlowLinks.Count();
            var refPath = targetOperation.RefPath.NamedChild("DataFlowLink", string.Format("No_{0}", linkCount + 1));
            DataFlowLinkElement daxDataFlowLinkElement = new DataFlowLinkElement(refPath, string.Format("DataFlowLink {0}", linkCount + 1), null, targetOperation);
            targetOperation.AddChild(daxDataFlowLinkElement);
            daxDataFlowLinkElement.Parent = targetOperation;
            daxDataFlowLinkElement.Source = source;
            daxDataFlowLinkElement.Target = targetColumn;

            return daxDataFlowLinkElement;
        }

        public DataFlowLinkElement AddDataFlowLink(OperationElement targetOperation, MssqlModelElement source, OperationOutputColumnElement targetColumn)
        {
            var linkCount = targetOperation.DataFlowLinks.Count();
            var refPath = targetOperation.RefPath.NamedChild("DataFlowLink", string.Format("No_{0}", linkCount + 1));
            DataFlowLinkElement daxDataFlowLinkElement = new DataFlowLinkElement(refPath, string.Format("DataFlowLink {0}", linkCount + 1), null, targetOperation);
            targetOperation.AddChild(daxDataFlowLinkElement);
            daxDataFlowLinkElement.Parent = targetOperation;
            daxDataFlowLinkElement.Source = source;
            daxDataFlowLinkElement.Target = targetColumn;

            //// add link to self so that functions that use the table as a whole
            //var refPathUpper = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}_Upper", linkCount + 1));
            //DaxDataFlowLinkElement daxDataFlowLinkElementUpper = new DaxDataFlowLinkElement(refPathUpper, string.Format("DataFlowLink {0} [U]", linkCount + 1), null, this);
            //this.AddChild(daxDataFlowLinkElementUpper);
            //daxDataFlowLinkElementUpper.Parent = this;
            //daxDataFlowLinkElementUpper.Source = source;
            //daxDataFlowLinkElementUpper.Target = this;

            return daxDataFlowLinkElement;
        }

        private Model.Mssql.Db.SqlScriptElement ParseSqlQuery(string server, string database, string query, MModelElement parent, out Dictionary<string, MssqlModelElement> outputColumns)
        {
            var sqlParser = new ScriptModelParser();
            sqlParser.ContextServerName = server;
            var dbEnvironment = _sqlDbIndex.GetDatabaseIndex(server);
            var sqlNode = sqlParser.ExtractScriptModel(query, parent, dbEnvironment, new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier() { Value = database }, out outputColumns);
            parent.AddChild(sqlNode);
            return sqlNode;
        }

        private MssqlModelElement FindSqlTable(string server, string database, string tableName, out Dictionary<string, MssqlModelElement> outputColumns)
        {
            var dbIdentifier = new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier() { Value = database };
            var dbIndex = _sqlDbIndex.GetDatabaseIndex(server);
            var table  = dbIndex.FindNodeByTableObjectName(tableName, new ScriptModelParser().Parser, dbIdentifier) as MssqlColumnScriptElement;

            outputColumns = new Dictionary<string, MssqlModelElement>();
            if (table == null)
            {
                return null;
            }

            foreach (var column in table.Columns)
            {
                outputColumns.Add(column.Caption, column);
            }
            return table;
        }

    }
}
